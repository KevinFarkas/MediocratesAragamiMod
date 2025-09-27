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
using Il2CppInterop.Runtime.Injection;
using TriangleNet;
using Il2CppSystem.Collections;
using System.Linq;

namespace CratesAragamiMod
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BasePlugin
    {
        internal static new ManualLogSource Log;
        private UnityAction<Scene, LoadSceneMode> _onLoaded;

        public override void Load()
        {
            Log = base.Log;
            Log.LogInfo("CratesAragamiMod Started");

            // Register our tiny updater so AddComponent works in IL2CPP
            ClassInjector.RegisterTypeInIl2Cpp<TitleScreenCleaner>();

            _onLoaded = DS.ConvertDelegate<UnityAction<Scene, LoadSceneMode>>(
          new Action<Scene, LoadSceneMode>(OnSceneLoaded));

            SceneManager.add_sceneLoaded(_onLoaded);
            Log.LogInfo("SkipIntro armed: will jump to CharacterSelect once GlobalScene is loaded.");

        }
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "GlobalScene") return;

            // When GlobalScene shows up (after platform init), spawn cleaner.
            var go = new GameObject("TitleScreenCleaner_GO");
            UnityEngine.Object.DontDestroyOnLoad(go);
            go.AddComponent(Il2CppType.Of<TitleScreenCleaner>());
            Log.LogInfo("Queued TitleScreenCleaner.");
        }
    }

    public class TitleScreenCleaner : MonoBehaviour
    {
        // IL2CPP constructors
        public TitleScreenCleaner(IntPtr handle) : base(handle) { }
        public TitleScreenCleaner() : base(ClassInjector.DerivedConstructorPointer<TitleScreenCleaner>())
        { ClassInjector.DerivedConstructorBody(this); }

        private int frames;

        // Names to remove from TitleScreen
        private static readonly string[] KillNames =
        {
        "CanvasSplash", "CanvasSplash_02", "LinceLogo", "Trademark", "Trademark_LW",
        "Trademark_Partners", "Background", "Logo", "Image_", "Image", "Visuals"
    };

        // Names to keep so the prompt/menu remains
        private static readonly string[] KeepNames =
        {
        "UITitleScreen", "PromptContainer", "UIMenuFooter", "TextLabelVersion"
    };

        private bool ShouldKill(string name)
            => KillNames.Any(k => name.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0)
               && !KeepNames.Any(k => name.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0);

        private bool ShouldKeep(string name)
            => KeepNames.Any(k => name.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0);

        private void Update()
        {
            // Give TitleScreen a frame or two to finish instantiating under GlobalScene
            if (++frames < 2) return;

            var ts = SceneManager.GetSceneByName("TitleScreen");
            if (!ts.IsValid() || !ts.isLoaded)
            {
                // Try again next frame until it’s visible
                if (frames < 120) return;
                Destroy(gameObject);
                return;
            }

            int killed = 0, kept = 0;

            foreach (var root in ts.GetRootGameObjects())
            {
                ProcessTree(root.transform, ref killed, ref kept);
            }

            DebugLog($"Cleaned TitleScreen. Hid={killed}, kept-specified={kept}. Press Enter prompt should now be visible.");
            Destroy(gameObject);
        }

        private void ProcessTree(Transform t, ref int killed, ref int kept)
        {
            var go = t.gameObject;
            var n = go.name;

            if (go.activeSelf)
            {
                if (ShouldKill(n))
                {
                    go.SetActive(false);
                    killed++;
                    DebugLog($"Hid: {GetPath(t)}");
                }
                else if (ShouldKeep(n))
                {
                    // Ensure the prompt/menu bits are enabled
                    if (!go.activeSelf) go.SetActive(true);
                    kept++;
                }
            }

            for (int i = 0; i < t.childCount; i++)
                ProcessTree(t.GetChild(i), ref killed, ref kept);
        }

        private static string GetPath(Transform t)
        {
            string path = t.name;
            while (t.parent != null) { t = t.parent; path = t.name + "/" + path; }
            return path;
        }

        private static void DebugLog(string msg)
            => BepInEx.Logging.Logger.CreateLogSource("Aragami SkipTitleFX").LogInfo(msg);
    }

}
