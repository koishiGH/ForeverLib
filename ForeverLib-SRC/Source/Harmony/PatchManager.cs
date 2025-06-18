using HarmonyLib;
using System.Reflection;

namespace ForeverLib.Harmony
{
    public class PatchManager
    {
        private readonly HarmonyLib.Harmony _harmony;

        public PatchManager(string id)
        {
            _harmony = new HarmonyLib.Harmony(id);
        }

        public void PatchAll(Assembly assembly)
        {
            _harmony.PatchAll(assembly);
        }
    }
} 