using BepInEx;
using BepInEx.Unity.IL2CPP;
using UnityEngine;
using UnityEngine.SceneManagement;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine.Video;
using Il2CppInterop.Runtime;

namespace CratesAragamiMod
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BasePlugin
    {
        internal static new ManualLogSource Log;

        public override void Load()
        {
            Log = base.Log;
            Log.LogInfo("CratesAragamiMod Started");

            var harmony = new Harmony("com.yourname.aragami2.scenelogger");
            harmony.Patch(
                original: AccessTools.Method(typeof(SceneManager), "Internal_SceneLoaded"),
                prefix: new HarmonyMethod(typeof(Plugin), nameof(Prefix))
            );
        }
        // This prefix runs whenever Unity finishes loading a scene
        public static void Prefix(Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            Log.LogInfo($"[Harmony] Scene loaded: {scene.name} | Mode: {mode}");
        }

    }
}
