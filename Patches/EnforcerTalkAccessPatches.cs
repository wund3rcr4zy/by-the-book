// Patches/EnforcerTalkAccessPatches.cs
// v0.2.1 — Ensure “Talk To” appears for on-duty enforcers (any active crime scene).
//
// What this does
//  - After Interactable.UpdateCurrentActions builds a target’s action list,
//    if the target is an Enforcer *guarding any active crime scene*, we make sure
//    the “Talk To” action is available and visible:
//      • First, try re-enabling it via SetActionDisable("Talk To", false).
//      • If a talk action already exists on another slot, promote it to PRIMARY.
//      • If it exists in disabledActions, surface it to PRIMARY.
//  - Writes the correct wrapper type (Interactable.InteractableCurrentAction)
//    into currentActions; avoids System<> vs Il2CppSystem<> KeyValuePair mismatches.
//  - Error-only logging (release-friendly).

#nullable enable
using System;
using HarmonyLib;
using UnityEngine;
using BepInEx.Logging;

internal static class BTB_TalkLog
{
    private static ManualLogSource? _log;
    public static ManualLogSource Log => _log ??= BepInEx.Logging.Logger.CreateLogSource("ByTheBook");
    public static void Err(string msg) => Log.LogError(msg);
}

internal static class BTB_EnforcerTalk
{
    /// <summary>Is this citizen guarding ANY active taped-off crime scene right now?</summary>
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

            foreach (var kv in calls) // e.g., Dictionary<NewGameLocation, EnforcerCall>
            {
                var call = kv.Value;
                if (call == null) continue;

                // Typical fields on EnforcerCall:
                //   isCrimeScene : bool
                //   guard        : int (humanID)
                if (call.isCrimeScene && call.guard == guard.humanID)
                    return true;
            }
        }
        catch (Exception ex)
        {
            BTB_TalkLog.Err($"[BTB] On-duty check failed: {ex}");
        }
        return false;
    }

    static bool IsTalkLabel(string? s)
        => !string.IsNullOrEmpty(s) &&
           s.IndexOf("talk", StringComparison.OrdinalIgnoreCase) >= 0;

    /// <summary>
    /// Look for an already-present talk action in currentActions.
    /// Returns the InteractableCurrentAction (avoids KeyValuePair type issues).
    /// </summary>
    public static bool TryFindExistingTalk(
        Interactable i,
        out Interactable.InteractableCurrentAction? found)
    {
        found = null;

        var dict = i.currentActions; // Il2CppSystem.Collections.Generic.Dictionary<InteractionKey, InteractableCurrentAction>
        if (dict == null) return false;

        // Iterate values to avoid dealing with Il2Cpp KeyValuePair generics.
        foreach (var cur in dict.Values)
        {
            if (cur == null) continue;
            var label = cur.currentAction?.ToString();
            if (IsTalkLabel(label))
            {
                found = cur;
                return true;
            }
        }
        return false;
    }

    /// <summary>Build the wrapper type the UI expects.</summary>
    public static Interactable.InteractableCurrentAction MakeCurrent(InteractablePreset.InteractionAction action)
        => new Interactable.InteractableCurrentAction
        {
            currentAction = action,
            display = true,
            enabled = true
        };

    /// <summary>If talk exists in disabledActions, surface it to PRIMARY.</summary>
    public static bool TrySurfaceFromDisabled(Interactable i)
    {
        var list = i.disabledActions; // (Il2Cpp) List<InteractablePreset.InteractionAction>
        if (list == null) return false;

        foreach (var a in list)
        {
            if (a == null) continue;
            if (IsTalkLabel(a.ToString()))
            {
                i.currentActions[InteractablePreset.InteractionKey.primary] = MakeCurrent(a);
                return true;
            }
        }
        return false;
    }
}

[HarmonyPatch(typeof(Interactable), nameof(Interactable.UpdateCurrentActions))]
static class BTB_EnsureEnforcerTalk_Postfix
{
    static void Postfix(Interactable __instance)
    {
        try
        {
            if (__instance == null) return;

            var citizen = __instance.belongsTo as Citizen;
            if (citizen == null) return;
            if (!BTB_EnforcerTalk.IsOnDutyCrimeSceneGuard(citizen)) return;

            // Zero-risk re-enable by label (no-op if locale differs or method absent).
            try { __instance.SetActionDisable("Talk To", false); } catch { }

            // If a talk action is already present, make it visible/enabled and move it to PRIMARY.
            if (BTB_EnforcerTalk.TryFindExistingTalk(__instance, out var current) && current != null)
            {
                current.display = true;
                current.enabled = true;
                __instance.currentActions[InteractablePreset.InteractionKey.primary] = current;
                return;
            }

            // Otherwise, if it's in disabledActions, surface it.
            BTB_EnforcerTalk.TrySurfaceFromDisabled(__instance);
        }
        catch (Exception ex)
        {
            BTB_TalkLog.Err($"[BTB] EnsureEnforcerTalk failed: {ex}");
        }
    }
}
