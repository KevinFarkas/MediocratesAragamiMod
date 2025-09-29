using HarmonyLib;
using System.Reflection;

namespace MediocratesAragamiMod
{
    /// <summary>
    /// This patch hijacks the routine responsible for showing the intro splash screens and their delay, 
    /// then makes it finish instantly so the player is taken straight to the “Press Enter” screen.
    /// A coroutine (IEnumerator) is compiled by Unity into a state machine class 
    /// (like TitleScreenMenu+_TitleScreenCoroutine_d__34) with a MoveNext() method that controls its steps.
    ///The actual method you see in the decompiled game(TitleScreenCoroutine) just creates 
    ///and returns one of these state machine instances.
    ///
    /// /// NOTE FOR FUTURE READERS:
    ///
    /// How I figured out that `_TitleScreenCoroutine_d__34` was the right coroutine to patch:
    ///
    /// Unity automatically rewrites coroutines into hidden "state machine" classes.
    /// These usually look like `_SomeNameCoroutine_d__XX`. I didn’t know which coroutine
    /// controlled the splash screens, so I logged all of them during startup.
    ///
    /// By watching the logs, I saw that `_TitleScreenCoroutine_d__34` in the
    /// `LinceWorks.TitleScreenMenu` class always ran right when the splash screens and logos appeared.
    /// When I patched it, the splash visuals and wait time disappeared, confirming it was
    /// responsible for that part of startup.
    ///
    /// In short: `_TitleScreenCoroutine_d__34` is just Unity’s auto-generated class for
    /// the coroutine method that shows the splash screens at the beginning. That’s why I hook it.
    /// </summary>
    [HarmonyPatch(typeof(LinceWorks.TitleScreenMenu._TitleScreenCoroutine_d__34), "MoveNext")]
    public static class TitleScreenCoroutinePatch
    {

        //Gives us the live enumerator, so we can control or replace its execution.
        static void Postfix(object __instance)
        {
            try
            {
                var type = __instance.GetType();

                //get___1__state → returns the current state integer.
                //set___1__state → changes the coroutine’s state.
                //get___2__current / set___2__current → read / write the current yielded object.
                var getState = type.GetMethod("get___1__state", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                var getCurrent = type.GetMethod("get___2__current", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                var setCurrent = type.GetMethod("set___2__current", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                if (getState == null || getCurrent == null || setCurrent == null)
                    return;

                int state = (int)getState.Invoke(__instance, null);
                object current = getCurrent.Invoke(__instance, null);

                //state 3 because when I logged every state change, it was after 3 when steam had initialized but the splash screen had not started.
                //I wanted to make sure steam still initialized, so after 3 we setCurrent to null so we can skip the splash screens.
                if (state >= 3 && current != null)
                {
                    SkipIntroPlugin.Log.LogInfo($"[TitleScreenCoroutinePatch] Forcing skip at state={state}, replacing yield {current.GetType().FullName} with null");
                    setCurrent.Invoke(__instance, new object[] { null });
                }
            }
            catch (System.Exception ex)
            {
                SkipIntroPlugin.Log.LogWarning($"[TitleScreenCoroutinePatch] Exception in coroutine skip patch: {ex}");
            }
        }
    }
}