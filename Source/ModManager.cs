using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Reflection;
using System.Diagnostics;

namespace ForeverLib
{
    [Serializable]
    public class ModState
    {
        public Dictionary<string, bool> ActiveMods = new Dictionary<string, bool>();
        public Dictionary<string, List<string>> ModifiedAssets = new Dictionary<string, List<string>>();
    }

    public class ModManager : MonoBehaviour
    {
        public static ModManager Instance { get; private set; }
        
        private Dictionary<string, IMod> loadedMods = new Dictionary<string, IMod>();
        private Dictionary<string, Sprite> spriteOverrides = new Dictionary<string, Sprite>();
        private static string TEMP_ASSETS_FOLDER = "TempAssets";
        private static string RESTART_FLAG_FILE = "mod_restart.flag";
        private bool hasRestoredAssets = false;
        private bool isRestarting = false;
        private static string MOD_STATE_FILE = "mod_state.json";
        private ModState currentState = new ModState();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                LoadModState();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void LoadMod(string modPath)
        {
            try
            {
                UnityEngine.Debug.Log($"ModManager: Loading mod from {modPath}");
                
                Assembly modAssembly = Assembly.LoadFrom(modPath);
                UnityEngine.Debug.Log($"ModManager: Assembly loaded successfully");

                foreach (Type type in modAssembly.GetTypes())
                {
                    if (typeof(IMod).IsAssignableFrom(type) && !type.IsInterface)
                    {
                        UnityEngine.Debug.Log($"ModManager: Found mod type {type.FullName}");
                        IMod mod = (IMod)Activator.CreateInstance(type);
                        loadedMods[mod.ModID] = mod;
                        mod.Initialize();
                        UnityEngine.Debug.Log($"ModManager: Successfully initialized mod {mod.ModName} ({mod.ModID})");
                    }
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"Failed to load mod at {modPath}: {e.Message}\nStack trace: {e.StackTrace}");
            }
        }

        public void RegisterSpriteOverride(string originalSpritePath, Sprite newSprite)
        {
            spriteOverrides[originalSpritePath] = newSprite;
        }

        public Sprite GetOverriddenSprite(string spritePath)
        {
            return spriteOverrides.TryGetValue(spritePath, out Sprite sprite) ? sprite : null;
        }

        public void RegisterAssetOverride(string originalAssetPath, string modifiedAssetPath)
        {
            AssetBundleManager.RegisterAssetOverride(originalAssetPath, modifiedAssetPath);
        }

        public void LoadModAssets(string modFolderPath)
        {
            string modId = Path.GetFileName(modFolderPath);
            
            if (currentState.ActiveMods.ContainsKey(modId) && currentState.ActiveMods[modId])
            {
                UnityEngine.Debug.Log($"ModManager: Mod {modId} is already active");
                return;
            }

            string assetsFolder = Path.Combine(modFolderPath, "assets");
            string gameDataPath = Path.GetDirectoryName(Application.dataPath);
            string tempAssetsPath = Path.Combine(gameDataPath, TEMP_ASSETS_FOLDER);
            string restartFlagPath = Path.Combine(gameDataPath, RESTART_FLAG_FILE);

            if (File.Exists(restartFlagPath))
            {
                File.Delete(restartFlagPath);
                return;
            }

            if (!Directory.Exists(assetsFolder))
            {
                UnityEngine.Debug.LogWarning($"ModManager: Assets folder not found at {assetsFolder}");
                return;
            }

            Directory.CreateDirectory(tempAssetsPath);

            currentState.ActiveMods[modId] = true;
            currentState.ModifiedAssets[modId] = Directory.GetFiles(assetsFolder)
                .Select(Path.GetFileName)
                .ToList();
            
            SaveModState();
            
            foreach (string assetFile in Directory.GetFiles(assetsFolder))
            {
                string fileName = Path.GetFileName(assetFile);
                string originalFile = Path.Combine(Application.dataPath + "_Data", fileName);
                string backupFile = Path.Combine(
                    Path.GetDirectoryName(Application.dataPath),
                    TEMP_ASSETS_FOLDER,
                    fileName
                );

                if (File.Exists(originalFile))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(backupFile));
                    if (!File.Exists(backupFile))
                    {
                        File.Copy(originalFile, backupFile);
                    }
                    File.Copy(assetFile, originalFile, true);
                }
            }

            RestartGame();
        }

        private void OnEnable()
        {
            UnityEngine.Debug.Log("ModManager: Registering quit handlers");
            Application.wantsToQuit += OnWantToQuit;
            Application.quitting += OnApplicationQuitting;
        }

        private void OnDisable()
        {
            Application.wantsToQuit -= OnWantToQuit;
            Application.quitting -= OnApplicationQuitting;
        }

        private bool OnWantToQuit()
        {
            UnityEngine.Debug.Log($"ModManager: OnWantToQuit called (isRestarting: {isRestarting}, hasRestoredAssets: {hasRestoredAssets})");
            if (!hasRestoredAssets && !isRestarting)
            {
                RestoreOriginalAssets();
            }
            return true;
        }

        private void OnApplicationQuitting()
        {
            UnityEngine.Debug.Log($"ModManager: OnApplicationQuitting called (isRestarting: {isRestarting}, hasRestoredAssets: {hasRestoredAssets})");
            if (!hasRestoredAssets && !isRestarting)
            {
                RestoreOriginalAssets();
            }
        }

        private void RestartGame()
        {
            try
            {
                isRestarting = true;
                string gameExePath = Path.Combine(
                    Path.GetDirectoryName(Application.dataPath),
                    Path.GetFileNameWithoutExtension(Application.dataPath) + ".exe"
                );
                
                UnityEngine.Debug.Log($"ModManager: Restarting game from {gameExePath}");
                
                #if UNITY_ANDROID
                    // Android restart, dunno why I have this here when I'd have to find a whole ass way to replace assets.
                    using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                    using (AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                    {
                        AndroidJavaObject intent = currentActivity.Call<AndroidJavaObject>("getBaseContext").Call<AndroidJavaObject>("getPackageManager").Call<AndroidJavaObject>("getLaunchIntentForPackage", Application.identifier);
                        if (intent != null)
                        {
                            intent.Call<AndroidJavaObject>("addFlags", 0x20000000); // FLAG_ACTIVITY_SINGLE_TOP, https://developer.android.com/reference/android/content/Intent#FLAG_ACTIVITY_SINGLE_TOP
                            currentActivity.Call("startActivity", intent);
                        }
                    }
                #elif UNITY_STANDALONE_WIN
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.FileName = gameExePath;
                    startInfo.UseShellExecute = true;
                    startInfo.WorkingDirectory = Path.GetDirectoryName(gameExePath);
                    
                    Process.Start(startInfo);
                    System.Threading.Thread.Sleep(500);
                #endif

                hasRestoredAssets = true;
                Application.Quit(0);
            }
            catch (Exception e)
            {
                isRestarting = false;
                UnityEngine.Debug.LogError($"ModManager: Failed to restart game: {e.Message}");
            }
        }

        private void RestoreOriginalAssets()
        {
            if (!currentState.ActiveMods.Any(x => x.Value))
            {
                UnityEngine.Debug.Log("ModManager: No active mods to restore");
                return;
            }

            foreach (var modId in currentState.ActiveMods.Keys.ToList())
            {
                if (currentState.ActiveMods[modId] && currentState.ModifiedAssets.ContainsKey(modId))
                {
                    RestoreModAssets(modId);
                    currentState.ActiveMods[modId] = false;
                }
            }
            
            SaveModState();
            
            string tempAssetsPath = Path.Combine(Path.GetDirectoryName(Application.dataPath), TEMP_ASSETS_FOLDER);
            if (Directory.Exists(tempAssetsPath))
            {
                Directory.Delete(tempAssetsPath, true);
            }
            
            hasRestoredAssets = true;
        }

        private void RestoreModAssets(string modId)
        {
            string tempAssetsPath = Path.Combine(Path.GetDirectoryName(Application.dataPath), TEMP_ASSETS_FOLDER);
            string gameDataPath = Path.Combine(
                Path.GetDirectoryName(Application.dataPath),
                Path.GetFileNameWithoutExtension(Application.dataPath) + "_Data"
            );

            foreach (string assetName in currentState.ModifiedAssets[modId])
            {
                string backupPath = Path.Combine(tempAssetsPath, assetName);
                string gamePath = Path.Combine(gameDataPath, assetName);
                
                if (File.Exists(backupPath))
                {
                    File.Copy(backupPath, gamePath, true);
                    UnityEngine.Debug.Log($"ModManager: Restored {assetName}");
                }
            }
        }

        private void LoadModState()
        {
            string statePath = Path.Combine(Path.GetDirectoryName(Application.dataPath), MOD_STATE_FILE);
            if (File.Exists(statePath))
            {
                try
                {
                    string[] lines = File.ReadAllLines(statePath);
                    currentState = new ModState();

                    foreach (string line in lines)
                    {
                        string[] parts = line.Split('=');
                        if (parts.Length != 2) continue;

                        if (parts[0] == "activeMods")
                        {
                            foreach (string modEntry in parts[1].Split('|'))
                            {
                                string[] modParts = modEntry.Split(':');
                                if (modParts.Length == 2)
                                {
                                    currentState.ActiveMods[modParts[0]] = bool.Parse(modParts[1]);
                                }
                            }
                        }
                        else if (parts[0] == "modifiedAssets")
                        {
                            foreach (string assetEntry in parts[1].Split('|'))
                            {
                                string[] assetParts = assetEntry.Split(':');
                                if (assetParts.Length == 2)
                                {
                                    currentState.ModifiedAssets[assetParts[0]] = 
                                        assetParts[1].Split(',').ToList();
                                }
                            }
                        }
                    }
                    UnityEngine.Debug.Log($"ModManager: Loaded state with {currentState.ActiveMods.Count} active mods");
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError($"ModManager: Failed to load mod state: {e.Message}");
                    currentState = new ModState();
                }
            }
        }

        private void SaveModState()
        {
            string statePath = Path.Combine(Path.GetDirectoryName(Application.dataPath), MOD_STATE_FILE);
            try
            {
                var stateDict = new Dictionary<string, object>
                {
                    ["activeMods"] = currentState.ActiveMods.Select(kvp => $"{kvp.Key}:{kvp.Value}").ToArray(),
                    ["modifiedAssets"] = currentState.ModifiedAssets.Select(kvp => 
                        $"{kvp.Key}:" + string.Join(",", kvp.Value)
                    ).ToArray()
                };

                string[] lines = stateDict.Select(kvp => $"{kvp.Key}={string.Join("|", (string[])kvp.Value)}").ToArray();
                File.WriteAllLines(statePath, lines);
                UnityEngine.Debug.Log("ModManager: Saved mod state");
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"ModManager: Failed to save mod state: {e.Message}");
            }
        }
    }
} 