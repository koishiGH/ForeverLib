using System.IO;
using Newtonsoft.Json;

namespace ForeverLib.Utils
{
    public static class ConfigManager
    {
        public static T LoadConfig<T>(string path) where T : new()
        {
            if (!File.Exists(path))
                return new T();

            var json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<T>(json) ?? new T();
        }

        public static void SaveConfig<T>(string path, T config)
        {
            var json = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(path, json);
        }
    }
} 