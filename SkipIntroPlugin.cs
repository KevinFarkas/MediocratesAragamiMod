using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine.SceneManagement;
using BepInEx.Unity.IL2CPP;
using UnityEngine.Events;

namespace MediocratesAragamiMod
{
    [BepInPlugin("com.mediocrates.aragami2.skipintro", "Skip Intro", "1.0.0")]
    public class SkipIntroPlugin : BasePlugin
    {
        internal static ManualLogSource Log;

        public override void Load()
        {
            Log = base.Log;
            Log.LogInfo("[SkipIntroPlugin] Loaded...");

            // These two lines set up Harmony, which is a library I use to change the game’s code at runtime.
            // Tells Harmony to look through this project for any patch classes (marked with [HarmonyPatch])
            // and apply them. That’s how my patch for skipping the intro actually gets injected into the game.
            var harmony = new Harmony("com.mediocrates.aragami2.skipintro");
            harmony.PatchAll();

            //Attach my OnSceneLoaded method.
            SceneManager.add_sceneLoaded((UnityAction<Scene, LoadSceneMode>)OnSceneLoaded);
        }

        /// <summary>
        /// Runs every time Unity finishes loading a scene.
        /// Then if the scene is TitleScreen, it terminates any RootGameObjects with 'CanvasSplash' in the name.
        /// This makes sure they never display.
        /// </summary>
        /// <param name="scene">The current scene</param>
        /// <param name="mode">The current scene mode</param>
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "TitleScreen")
            {
                Log.LogInfo("[SkipIntroPlugin] TitleScreen scene loaded: disabling splash screens...");
                foreach (var go in scene.GetRootGameObjects())
                {
                    if (go.name.Contains("CanvasSplash"))
                    {
                        Log.LogInfo($"[SkipIntroPlugin] Disabling splash root: {go.name}");
                        go.SetActive(false); 
                    }
                }
            }
        }
    }
}
