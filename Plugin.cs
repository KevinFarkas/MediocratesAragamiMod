using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using System.Reflection;

namespace Aragami2.SkipIntro
{
    [BepInPlugin("com.yourname.aragami2.skipintro", "Skip Intro", "1.0.5")]
    public class Plugin : BasePlugin
    {
        internal static ManualLogSource Log;

        public override void Load()
        {
            Log = base.Log;
            Log.LogInfo("SkipIntro loaded, patching TitleScreenCoroutine...");

            var harmony = new Harmony("com.yourname.aragami2.skipintro");
            harmony.PatchAll();
        }
    }

    [HarmonyPatch(typeof(LinceWorks.TitleScreenMenu._TitleScreenCoroutine_d__34), "MoveNext")]
    public static class Patch_TitleScreenCoroutine
    {
        static void Postfix(object __instance)
        {
            try
            {
                var type = __instance.GetType();

                var getState = type.GetMethod("get___1__state", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                var getCurrent = type.GetMethod("get___2__current", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                var setCurrent = type.GetMethod("set___2__current", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                if (getState == null || getCurrent == null || setCurrent == null)
                    return;

                int state = (int)getState.Invoke(__instance, null);
                object current = getCurrent.Invoke(__instance, null);

                if (state >= 3 && current != null)
                {
                    Plugin.Log.LogInfo($"[SkipIntro] Forcing skip at state={state}, replacing yield {current.GetType().FullName} with null");
                    setCurrent.Invoke(__instance, new object[] { null });
                }
            }
            catch (System.Exception ex)
            {
                Plugin.Log.LogWarning($"[SkipIntro] Exception in coroutine skip patch: {ex}");
            }
        }
    }


}
