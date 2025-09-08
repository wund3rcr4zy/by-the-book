// Patches/EnforcerTalkAccessPatches.cs
// v0.2.1 — Ensure “Talk To” appears for on-duty enforcers (any active crime scene)
//           without touching actions while the dialog UI is open.
//
// What this does
//  • Skips changes when a dialog is open (avoid race with End Conversation).
//  • Finds the guard’s existing Talk InteractionAction from aiActionReference (IL2CPP dict).
//  • Wraps it into Interactable.InteractableCurrentAction and promotes to PRIMARY.
//  • Error-only logging (release-friendly).

#nullable enable
using System;
using HarmonyLib;
using UnityEngine;
using BepInEx.Logging;
// IL2CPP collections live here; iterate them directly (don’t cast to System.Collections.*)
using Il2CppSystem.Collections.Generic; // Enumerator<>, Dictionary<,> etc.  (see example mods) [ref]

internal static class BTB_TalkLog
{
    private static ManualLogSource? _log;
    public static ManualLogSource Log => _log ??= BepInEx.Logging.Logger.CreateLogSource("ByTheBook");
    public static void Err(string msg) => Log.LogError(msg);
}

internal static class BTB_EnforcerTalk
{
    public static bool IsOnDutyCrimeSceneGuard(Citizen? guard)
    {
        try
        {
            if (guard == null || !guard.isEnforcer) return false;

            var gc = GameplayController._instance != null
                ? GameplayController._instance
                : GameplayController.Instance;
            if (gc == null) return false;

            var calls = gc.enforcerCalls;
            if (calls == null) return false;

            foreach (var kv in calls)
            {
                var call = kv.Value;
                if (call != null && call.isCrimeScene && call.guard == guard.humanID)
                    return true;
            }
        }
        catch (Exception ex) { BTB_TalkLog.Err($"[BTB] On-duty check failed: {ex}"); }
        return false;
    }

    // Best-effort: don’t mutate actions while dialog UI is open/closing.
    public static bool IsDialogOpen()
    {
        try
        {
            var dc = (DialogController._instance != null) ? DialogController._instance : DialogController.Instance;
            if (dc == null) return false;

            // Try common flags; ignore if not present on this build.
            try { if ((bool)(dc.GetType().GetProperty("isOpen")?.GetValue(dc) ?? false)) return true; } catch { }
            try { if ((bool)(dc.GetType().GetProperty("dialogOpen")?.GetValue(dc) ?? false)) return true; } catch { }
        }
        catch { }
        return false;
    }

    static bool IsTalkLabel(string? s)
        => !string.IsNullOrEmpty(s) && s.IndexOf("talk", StringComparison.OrdinalIgnoreCase) >= 0;

    /// Find the *InteractionAction* for Talk in aiActionReference (IL2CPP Dictionary<AIActionPreset, InteractionAction>).
    public static bool TryGetTalkActionFromAIRef(Interactable i, out InteractablePreset.InteractionAction? action)
    {
        action = null;
        try
        {
            var dict = i.aiActionReference; // Il2CppSystem.Collections.Generic.Dictionary<AIActionPreset, InteractionAction>
            if (dict == null) return false;

            // Iterate the IL2CPP ValueCollection directly — NO casts to System.* IEnumerable.
            var values = dict.Values; // ValueCollection<InteractionAction>
            var e = values.GetEnumerator(); // Il2CppSystem.Collections.Generic.Enumerator<InteractionAction>
            while (e.MoveNext())
            {
                var a = e.Current;
                if (a != null && IsTalkLabel(a.ToString()))
                {
                    action = a;
                    return true;
                }
            }
        }
        catch (Exception ex)
        {
            BTB_TalkLog.Err($"[BTB] TryGetTalkActionFromAIRef failed: {ex}");
        }
        return false;
    }

    public static Interactable.InteractableCurrentAction MakeCurrent(InteractablePreset.InteractionAction action)
        => new Interactable.InteractableCurrentAction
        {
            currentAction = action,
            display = true,
            enabled = true
        };
}

[HarmonyPatch(typeof(Interactable), nameof(Interactable.UpdateCurrentActions))]
static class BTB_EnsureEnforcerTalk_Postfix
{
    static void Postfix(Interactable __instance)
    {
        try
        {
            if (__instance == null) return;

            // Don’t touch action maps while dialog is open (prevents End Conversation crashes).
            if (BTB_EnforcerTalk.IsDialogOpen()) return;

            var citizen = __instance.belongsTo as Citizen;
            if (citizen == null) return;
            if (!BTB_EnforcerTalk.IsOnDutyCrimeSceneGuard(citizen)) return;

            // Locale-agnostic: obtain the Talk InteractionAction from AI ref and surface it.
            if (BTB_EnforcerTalk.TryGetTalkActionFromAIRef(__instance, out var talk) && talk != null)
            {
                __instance.currentActions[InteractablePreset.InteractionKey.primary] =
                    BTB_EnforcerTalk.MakeCurrent(talk);
            }
        }
        catch (Exception ex)
        {
            BTB_TalkLog.Err($"[BTB] EnsureEnforcerTalk failed: {ex}");
        }
    }
}
