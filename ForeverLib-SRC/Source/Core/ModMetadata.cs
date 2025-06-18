using System;
using Newtonsoft.Json;

namespace ForeverLib.Core
{
    public class ModMetadata
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Author { get; set; }
        public Version Version { get; set; }
        public string Description { get; set; }
        public string[] Dependencies { get; set; }
        public string EntryPoint { get; set; }
        public string MinGameVersion { get; set; }
        public int LoadPriority { get; set; }
        
        [JsonIgnore]
        public string ModPath { get; internal set; }

        public ModMetadata()
        {
            Id = string.Empty;
            Name = string.Empty;
            Author = string.Empty;
            Version = new Version(1, 0, 0);
            Description = string.Empty;
            Dependencies = Array.Empty<string>();
            EntryPoint = string.Empty;
            MinGameVersion = "1.0.0";
            LoadPriority = 0;
            ModPath = string.Empty;
        }

        public bool Validate(out string error)
        {
            if (string.IsNullOrEmpty(Id))
            {
                error = "Mod ID is required";
                return false;
            }
            if (string.IsNullOrEmpty(Name))
            {
                error = "Mod Name is required";
                return false;
            }
            if (string.IsNullOrEmpty(Author))
            {
                error = "Mod Author is required";
                return false;
            }
            if (Version == null)
            {
                error = "Mod Version is required";
                return false;
            }
            if (string.IsNullOrEmpty(EntryPoint))
            {
                error = "Mod EntryPoint is required";
                return false;
            }

            error = string.Empty;
            return true;
        }
    }
}