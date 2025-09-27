using BepInEx;
using BepInEx.Unity.IL2CPP;
using UnityEngine;
using UnityEngine.SceneManagement;
using BepInEx.Logging;
using System;
using HarmonyLib;
using UnityEngine.Video;
using Il2CppInterop.Runtime;
using UnityEngine.Events;
using static Il2CppSystem.DateTimeParse;
using DS = Il2CppInterop.Runtime.DelegateSupport;
using LinceWorks;

namespace CratesAragamiMod
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BasePlugin
    {
        internal static new ManualLogSource Log;
        private UnityAction<Scene, LoadSceneMode> _onLoaded; // keep a ref so it doesn't get GC'd
        private static bool _sawTitle, _sawGlobal, _queued;

        public override void Load()
        {
            Log = base.Log;
            Log.LogInfo("CratesAragamiMod Started");

            // Create and apply Harmony patches
            var harmony = new Harmony("your.id.aragami2.skipintro");
            harmony.PatchAll();

            _onLoaded = DS.ConvertDelegate<UnityAction<Scene, LoadSceneMode>>(
           new Action<Scene, LoadSceneMode>(OnSceneLoaded));

            SceneManager.add_sceneLoaded(_onLoaded);

        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Log.LogInfo($"Scene loaded: {scene.name} ({mode})");
        }

    }
}
