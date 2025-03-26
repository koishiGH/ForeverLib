using System.IO;
using System.Reflection;
using ForeverLib;
using UnityEngine;

public class ExampleMod : IMod
{
    public string ModID => "com.maybekoi.examplemod";
    public string ModName => "Example Mod";
    public string ModVersion => "1.0.0";

    public void Initialize() { 
        string modDllPath = Assembly.GetExecutingAssembly().Location;
        string modFolderPath = Path.GetDirectoryName(modDllPath);
    }

    public void OnEnabled() { }
    public void OnDisabled() { }
}