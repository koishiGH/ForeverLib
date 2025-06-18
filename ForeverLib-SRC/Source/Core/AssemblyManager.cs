using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using ForeverLib.Utils;

namespace ForeverLib.Core
{
    public class AssemblyManager
    {
        private readonly Dictionary<string, Assembly> _loadedAssemblies;
        private readonly Logger _logger;

        public AssemblyManager()
        {
            _loadedAssemblies = new Dictionary<string, Assembly>();
            _logger = new Logger("AssemblyManager");
        }

        public Assembly LoadModAssembly(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"Mod assembly not found at path: {path}");

            try
            {
                var assembly = Assembly.LoadFrom(path);
                _loadedAssemblies[path] = assembly;
                _logger.Log($"Loaded assembly: {assembly.GetName().Name}");
                return assembly;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to load assembly from {path}: {ex.Message}");
                throw;
            }
        }

        public void UnloadModAssembly(string path)
        {
            if (_loadedAssemblies.ContainsKey(path))
            {
                _loadedAssemblies.Remove(path);
                _logger.Log($"Assembly marked for unload: {path}");
            }
        }

        public void ApplyPatches(Assembly assembly) {}
    }
}