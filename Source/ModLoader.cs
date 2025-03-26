using UnityEngine;
using System.IO;

namespace ForeverLib
{
    [AddComponentMenu("ForeverLib/Mod Loader")]
    public class ModLoader : MonoBehaviour
    {
        private void Awake()
        {
            Debug.Log("ModLoader: Awake called");
            if (ModManager.Instance == null)
            {
                Debug.Log("ModLoader: Creating ModManager");
                var managerObj = new GameObject("ModManager");
                managerObj.AddComponent<ModManager>();
                DontDestroyOnLoad(managerObj);
            }
        }

        private void Start()
        {
            Debug.Log("ModLoader: Start called");
            LoadAllMods();
        }

        private void LoadAllMods()
        {
            string modsPath = Path.Combine(Path.GetDirectoryName(Application.dataPath), "Mods");
            Debug.Log($"ModLoader: Looking for mods in {modsPath}");

            if (!Directory.Exists(modsPath))
            {
                Directory.CreateDirectory(modsPath);
                Debug.Log($"ModLoader: Created mods folder at: {modsPath}");
                return;
            }

            string[] modFiles = Directory.GetFiles(modsPath, "*.dll");
            
            string[] modFolders = Directory.GetDirectories(modsPath);
            foreach (string folder in modFolders)
            {
                string dllPath = Path.Combine(folder, Path.GetFileName(folder) + ".dll");
                if (File.Exists(dllPath))
                {
                    modFiles = modFiles.Length > 0 ? 
                        new string[] { modFiles[0], dllPath } : 
                        new string[] { dllPath };
                }
            }

            Debug.Log($"ModLoader: Found {modFiles.Length} potential mod files");

            foreach (string modFile in modFiles)
            {
                try
                {
                    Debug.Log($"ModLoader: Attempting to load mod: {Path.GetFileName(modFile)}");
                    ModManager.Instance.LoadMod(modFile);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"ModLoader: Failed to load mod {Path.GetFileName(modFile)}: {e.Message}\n{e.StackTrace}");
                }
            }
        }
    }
}