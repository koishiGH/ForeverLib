using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Diagnostics;

namespace ForeverLib
{
    public class ModManager : MonoBehaviour
    {
        public static ModManager Instance { get; private set; }
        
        private Dictionary<string, IMod> loadedMods = new Dictionary<string, IMod>();
        private Dictionary<string, Sprite> spriteOverrides = new Dictionary<string, Sprite>();
        private static string TEMP_ASSETS_FOLDER = "TempAssets";
        private static string RESTART_FLAG_FILE = "mod_restart.flag";
        private bool hasRestoredAssets = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
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

            foreach (string modFile in Directory.GetFiles(assetsFolder))
            {
                string fileName = Path.GetFileName(modFile);
                string originalFile = Path.Combine(Application.dataPath, fileName);
                string backupFile = Path.Combine(tempAssetsPath, fileName);

                if (File.Exists(originalFile))
                {
                    if (!File.Exists(backupFile))
                    {
                        File.Copy(originalFile, backupFile);
                    }

                    File.Copy(modFile, originalFile, true);
                }
            }

            File.WriteAllText(restartFlagPath, DateTime.Now.ToString());
            RestartGame();
        }

        private void RestoreOriginalAssets()
        {
            string gameDataPath = Path.GetDirectoryName(Application.dataPath);
            string tempAssetsPath = Path.Combine(gameDataPath, TEMP_ASSETS_FOLDER);

            if (!Directory.Exists(tempAssetsPath)) return;

            foreach (string backupFile in Directory.GetFiles(tempAssetsPath))
            {
                string fileName = Path.GetFileName(backupFile);
                string originalPath = Path.Combine(Application.dataPath, fileName);
                File.Copy(backupFile, originalPath, true);
            }

            Directory.Delete(tempAssetsPath, true);
            hasRestoredAssets = true;
        }

        private void RestartGame()
        {
            string gameExePath = Path.Combine(
                Path.GetDirectoryName(Application.dataPath),
                Path.GetFileNameWithoutExtension(Application.dataPath) + ".exe"
            );

            Process.Start(gameExePath);
            Application.Quit();
        }

        private void OnApplicationQuit()
        {
            if (!hasRestoredAssets)
            {
                RestoreOriginalAssets();
            }
        }
    }
} 