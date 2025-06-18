using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ForeverLib.Core;
using System.IO;
using System.Threading.Tasks;

public class ModLoaderInitializer : MonoBehaviour
{
    private static ModLoader _modLoader;

    private async void Start()
    {
        try
        {
            string modsPath = Path.Combine(Application.dataPath, "..", "Mods");
            _modLoader = new ModLoader(modsPath);
            
            await _modLoader.LoadModsAsync();
            _modLoader.OnGameStart();
            
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
            UnityEngine.SceneManagement.SceneManager.sceneUnloaded += OnSceneUnloaded;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to initialize mod loader: {ex}");
        }
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        _modLoader?.OnSceneLoaded(scene.name);
    }

    private void OnSceneUnloaded(UnityEngine.SceneManagement.Scene scene)
    {
        _modLoader?.OnSceneUnloaded(scene.name);
    }

    private void OnApplicationQuit()
    {
        _modLoader?.OnGameExit();
        _modLoader?.UnloadAllMods();
    }

    private void OnDestroy()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        UnityEngine.SceneManagement.SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    void Update()
    {
        
    }
}
