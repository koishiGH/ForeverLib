using ForeverLib.Core;
using ForeverLib.Utils;
using HarmonyLib;
using UnityEngine;
using TMPro;

namespace TextChanger_FLM
{
    public class TextChanger : ModBase
    {
        private static ForeverLib.Utils.Logger _staticLogger;

        public override void OnLoad()
        {
            _staticLogger = Logger;
            _staticLogger.Log("Text Changer mod loaded!");
            var harmony = new Harmony("com.maybekoi.textchanger");
            harmony.PatchAll(typeof(TextChanger).Assembly);
        }

        public override void OnUnload()
        {
            Logger.Log("Text Changer mod unloaded!");
        }

        [HarmonyPatch(typeof(TextSetter), "Update")]
        class TextSetterUpdatePatch
        {
            static void Postfix(TextSetter __instance)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    __instance.text.text = "Hi, I'm the modded text lol\nPress LeftShift to see a different one :D";
                    _staticLogger.Log("Text changed via Update patch!");
                }
                if (Input.GetKeyDown(KeyCode.LeftShift))
                {
                    __instance.text.text = "Listen to out of cbd by kawai sprite!!\nPress space to see the original modded text";
                    _staticLogger.Log("Text changed via Update patch!");
                }
            }
        }
    }
}
