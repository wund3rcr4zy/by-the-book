// Patches/EnforcerTalkAccessPatches.cs
// v0.2.2 — Ensure “Talk To” appears for on-duty enforcers (active crime scenes) without
//           touching action maps while the dialog UI is open (IL2CPP-safe).
//
// OVERVIEW
// ──────────────────────────────────────────────────────────────────────────────
// Some enforcers guarding an active crime scene lack the “Talk To” prompt even
// though the base game exposes it via their AI action map. This patch ensures
// the prompt reliably appears for *on-duty* enforcers while avoiding races with
// the dialog UI lifecycle (which can crash if action maps are mutated mid-dialog).
//
// WHAT THIS PATCH DOES
// ─ Ensures we only act when the target is an *on-duty* enforcer guarding the
//   current crime scene.
// ─ Skips any mutations when the dialog UI is open/tearing down (prevents “End
//   Conversation” crashes).
// ─ Finds the canonical “Talk” InteractionAction from the NPC’s
//   aiActionReference (an IL2CPP Dictionary) and promotes it to PRIMARY by
//   wrapping it in InteractableCurrentAction (no string/locale dependency).
// ─ Keeps logging error-only in release builds.
//
// IL2CPP NOTES
// ─ Unity IL2CPP collections live under Il2CppSystem.Collections.Generic.
//   Iterate their ValueCollection/Enumerator directly (do not cast to
//   System.Collections.*). This file does exactly that for aiActionReference.
//
// SCOPE & SAFETY
// ─ Postfix on Interactable.UpdateCurrentActions: conservative, runs after the
//   game builds the map. No changes if guard isn’t on duty, or if dialog is open.
// ─ No string matching beyond a single “talk” substring check on the action’s
//   ToString() (the source object is the game’s InteractionAction).
//
// VERSION HISTORY
// ─ 0.2.2  • On-duty guard detection via GameplayController.enforcerCalls.
//          • Dialog-open guard to avoid mutating actions during dialog teardown.
//          • IL2CPP-safe iteration of aiActionReference.ValueCollection (no casts to System.*).
//          • Promotion of the canonical Talk action to PRIMARY using
//            Interactable.InteractableCurrentAction (visible + enabled).
//          • Error-only logging in release builds.
//          • Header/docs refresh for PR review clarity.

#nullable enable
using System;
using HarmonyLib;
using UnityEngine;
using BepInEx.Logging;
// IL2CPP collections live here; iterate them directly (don’t cast to System.Collections.*)
using Il2CppSystem.Collections.Generic; // Enumerator<>, Dictionary<,> etc.

/// <summary>Release-friendly logger: errors only.</summary>
internal static class BTB_TalkLog
{
    private static ManualLogSource? _log;
    public static ManualLogSource Log => _log ??= BepInEx.Logging.Logger.CreateLogSource("ByTheBook");
    public static void Err(string msg) => Log.LogError(msg);
}

/// <summary>Helpers for identifying on-duty guards and safely surfacing “Talk”.</summary>
internal static class BTB_EnforcerTalk
{
    /// <summary>True if this citizen is the assigned guard for any active crime scene.</summary>
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

    /// <summary>Best-effort guard: don’t mutate while dialog UI is open/tearing down.</summary>
    public static bool IsDialogOpen()
    {
        try
        {
            var dc = (DialogController._instance != null) ? DialogController._instance : DialogController.Instance;
            if (dc == null) return false;

            // Different builds expose different flags; try both reflectively.
            try { if ((bool)(dc.GetType().GetProperty("isOpen")?.GetValue(dc) ?? false)) return true; } catch { }
            try { if ((bool)(dc.GetType().GetProperty("dialogOpen")?.GetValue(dc) ?? false)) return true; } catch { }
        }
        catch { }
        return false;
    }

    /// <summary>Loose “talk” identifier; we check the game action’s own label.</summary>
    static bool IsTalkLabel(string? s)
        => !string.IsNullOrEmpty(s) && s.IndexOf("talk", StringComparison.OrdinalIgnoreCase) >= 0;

    /// <summary>
    /// Fetch the canonical Talk InteractionAction from aiActionReference
    /// (IL2CPP Dictionary&lt;AIActionPreset, InteractionAction&gt;).
    /// </summary>
    public static bool TryGetTalkActionFromAIRef(Interactable i, out InteractablePreset.InteractionAction? action)
    {
        action = null;
        try
        {
            var dict = i.aiActionReference; // Il2CppSystem.Collections.Generic.Dictionary<AIActionPreset, InteractionAction>
            if (dict == null) return false;

            // Iterate IL2CPP ValueCollection directly — no casts to System.Collections.*.
            var values = dict.Values; // ValueCollection<InteractionAction>
            var e = values.GetEnumerator(); // Enumerator<InteractionAction>
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

    /// <summary>Wrap a game InteractionAction into a visible, enabled current action.</summary>
    public static Interactable.InteractableCurrentAction MakeCurrent(InteractablePreset.InteractionAction action)
        => new Interactable.InteractableCurrentAction
        {
            currentAction = action,
            display = true,
            enabled = true
        };
}

/// <summary>
/// Postfix: after the game builds currentActions, promote the guard’s “Talk” action
/// to PRIMARY — but only if it’s safe (no dialog open) and only for on-duty guards.
/// </summary>
[HarmonyPatch(typeof(Interactable), nameof(Interactable.UpdateCurrentActions))]
static class BTB_EnsureEnforcerTalk_Postfix
{
    static void Postfix(Interactable __instance)
    {
        try
        {
            if (__instance == null) return;

            // Prevent race with dialog teardown (“End Conversation”) by skipping while open.
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
