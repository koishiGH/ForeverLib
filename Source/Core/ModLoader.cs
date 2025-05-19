using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Reflection;
using ForeverLib.Utils;
using Newtonsoft.Json;

namespace ForeverLib.Core
{
    public class ModLoader
    {
        private readonly string _modsDirectory;
        private readonly Dictionary<string, ModBase> _loadedMods;
        private readonly AssemblyManager _assemblyManager;
        private readonly Logger _logger;

        public ModLoader(string modsPath)
        {
            _modsDirectory = modsPath;
            _loadedMods = new Dictionary<string, ModBase>();
            _assemblyManager = new AssemblyManager();
            _logger = new Logger("ModLoader");
        }

        public async Task LoadModsAsync()
        {
            await Task.Run(() =>
            {
                if (!Directory.Exists(_modsDirectory))
                {
                    Directory.CreateDirectory(_modsDirectory);
                    _logger.Log("Created mods directory");
                    return;
                }

                var modDirectories = Directory.GetDirectories(_modsDirectory);
                var modMetadata = new List<ModMetadata>();

                foreach (var modDir in modDirectories)
                {
                    try
                    {
                        var metadata = LoadModMetadata(modDir);
                        if (metadata != null)
                        {
                            metadata.ModPath = modDir;
                            modMetadata.Add(metadata);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Failed to load mod metadata from {modDir}: {ex.Message}");
                    }
                }

                var sortedMods = SortModsByLoadOrder(modMetadata);

                foreach (var metadata in sortedMods)
                {
                    try
                    {
                        LoadMod(metadata);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Failed to load mod {metadata.Name}: {ex.Message}");
                    }
                }
            });
        }

        private ModMetadata? LoadModMetadata(string modDirectory)
        {
            var metadataPath = Path.Combine(modDirectory, "mod.json");
            if (!File.Exists(metadataPath))
            {
                _logger.Error($"No mod.json found in {modDirectory}");
                return null;
            }

            try
            {
                var json = File.ReadAllText(metadataPath);
                var metadata = JsonConvert.DeserializeObject<ModMetadata>(json);
                
                if (metadata == null)
                {
                    _logger.Error($"Failed to deserialize mod.json in {modDirectory}");
                    return null;
                }

                if (!metadata.Validate(out string error))
                {
                    _logger.Error($"Invalid mod.json in {modDirectory}: {error}");
                    return null;
                }

                return metadata;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error loading mod.json from {modDirectory}: {ex.Message}");
                return null;
            }
        }

        private List<ModMetadata> SortModsByLoadOrder(List<ModMetadata> mods)
        {
            var sorted = new List<ModMetadata>();
            var visited = new HashSet<string>();

            void VisitMod(ModMetadata mod)
            {
                if (visited.Contains(mod.Id)) return;

                foreach (var depId in mod.Dependencies)
                {
                    var dep = mods.FirstOrDefault(m => m.Id == depId);
                    if (dep == null)
                    {
                        throw new Exception($"Missing dependency {depId} for mod {mod.Id}");
                    }
                    VisitMod(dep);
                }

                visited.Add(mod.Id);
                sorted.Add(mod);
            }

            var prioritySorted = mods.OrderBy(m => m.LoadPriority).ToList();

            foreach (var mod in prioritySorted)
            {
                if (!visited.Contains(mod.Id))
                {
                    VisitMod(mod);
                }
            }

            return sorted;
        }

        private void LoadMod(ModMetadata metadata)
        {
            var assemblyPath = Directory.GetFiles(metadata.ModPath, "*.dll").FirstOrDefault();
            if (assemblyPath == null)
            {
                throw new Exception($"No .dll found for mod {metadata.Name}");
            }

            var assembly = _assemblyManager.LoadModAssembly(assemblyPath);
            var entryType = assembly.GetType(metadata.EntryPoint);
            
            if (entryType == null)
            {
                throw new Exception($"Entry point {metadata.EntryPoint} not found in assembly");
            }

            if (!typeof(ModBase).IsAssignableFrom(entryType))
            {
                throw new Exception($"Entry point {metadata.EntryPoint} must inherit from ModBase");
            }

            var mod = (ModBase)Activator.CreateInstance(entryType)!;
            mod.Metadata = metadata;
            mod.Logger = new Logger(metadata.Name);

            _loadedMods[metadata.Id] = mod;
            mod.OnLoad();
            _logger.Log($"Loaded mod: {metadata.Name} v{metadata.Version}");
        }

        public void UnloadMod(string modId)
        {
            if (_loadedMods.TryGetValue(modId, out var mod))
            {
                try
                {
                    mod.OnUnload();
                    _loadedMods.Remove(modId);
                    _logger.Log($"Unloaded mod: {mod.Metadata.Name}");
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error unloading mod {modId}: {ex.Message}");
                }
            }
        }

        public void UnloadAllMods()
        {
            foreach (var modId in _loadedMods.Keys.ToList())
            {
                UnloadMod(modId);
            }
        }

        public void OnGameStart()
        {
            foreach (var mod in _loadedMods.Values)
            {
                try
                {
                    mod.OnGameStart();
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error in OnGameStart for mod {mod.Metadata.Name}: {ex.Message}");
                }
            }
        }

        public void OnGameExit()
        {
            foreach (var mod in _loadedMods.Values)
            {
                try
                {
                    mod.OnGameExit();
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error in OnGameExit for mod {mod.Metadata.Name}: {ex.Message}");
                }
            }
        }

        public void OnSceneLoaded(string sceneName)
        {
            foreach (var mod in _loadedMods.Values)
            {
                try
                {
                    mod.OnSceneLoaded(sceneName);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error in OnSceneLoaded for mod {mod.Metadata.Name}: {ex.Message}");
                }
            }
        }

        public void OnSceneUnloaded(string sceneName)
        {
            foreach (var mod in _loadedMods.Values)
            {
                try
                {
                    mod.OnSceneUnloaded(sceneName);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error in OnSceneUnloaded for mod {mod.Metadata.Name}: {ex.Message}");
                }
            }
        }
    }
}