using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace ForeverLib
{
    public class AssetBundleManager
    {
        private static Dictionary<string, AssetBundle> loadedBundles = new Dictionary<string, AssetBundle>();
        private static Dictionary<string, string> assetOverrides = new Dictionary<string, string>();

        public static void RegisterAssetOverride(string originalAssetPath, string modifiedAssetPath)
        {
            string normalizedOriginal = Path.GetFileName(originalAssetPath).ToLower();
            
            if (loadedBundles.ContainsKey(normalizedOriginal))
            {
                loadedBundles[normalizedOriginal].Unload(true);
                loadedBundles.Remove(normalizedOriginal);
            }
            
            assetOverrides[normalizedOriginal] = modifiedAssetPath;
        }

        public static AssetBundle LoadAssetBundle(string assetPath)
        {
            string fileName = Path.GetFileName(assetPath).ToLower();
            
            if (assetOverrides.TryGetValue(fileName, out string overridePath))
            {
                assetPath = overridePath;
            }

            if (!loadedBundles.TryGetValue(fileName, out AssetBundle bundle))
            {
                bundle = AssetBundle.LoadFromFile(assetPath);
                if (bundle != null)
                {
                    loadedBundles[fileName] = bundle;
                }
            }

            return bundle;
        }

        public static void UnloadAll()
        {
            foreach (var bundle in loadedBundles.Values)
            {
                bundle.Unload(true);
            }
            loadedBundles.Clear();
        }
    }
} 