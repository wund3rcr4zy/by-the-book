// Patches/EnforcerTalkAccessPatches.cs
// v0.2.1 — Ensure “Talk To” appears for on-duty enforcers (any active crime scene)
//           without touching actions while the dialog UI is open.
//
// What this does
//  • Skips changes when a dialog is open (avoid race with End Conversation).
//  • Finds the guard’s existing Talk InteractionAction from aiActionReference.
//  • Wraps it into Interactable.InteractableCurrentAction and promotes to PRIMARY.
//  • Error-only logging (release-friendly).

#nullable enable
using System;
using System.Collections;
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

            // Different builds expose different flags; try both safely.
            try { if ((bool)AccessProp(dc, "isOpen")) return true; } catch { }
            try { if ((bool)AccessProp(dc, "dialogOpen")) return true; } catch { }
        }
        catch { }
        return false;
    }

    // Tiny helper to read a property safely without hard type deps
    static object? AccessProp(object obj, string name)
    {
        try { return obj.GetType().GetProperty(name)?.GetValue(obj); } catch { return null; }
    }

    static bool IsTalkLabel(string? s)
        => !string.IsNullOrEmpty(s) && s.IndexOf("talk", StringComparison.OrdinalIgnoreCase) >= 0;

    /// Find the *InteractionAction* for Talk in aiActionReference.
    public static bool TryGetTalkActionFromAIRef(Interactable i, out InteractablePreset.InteractionAction? action)
    {
        action = null;
        try
        {
            var dictObj = i.aiActionReference;       // usually Dictionary<InteractablePreset.InteractionAction, …>
            if (dictObj == null) return false;

            // Use non-generic IDictionary shape to avoid Il2Cpp KeyValuePair types.
            if (dictObj is IDictionary dict)
            {
                foreach (DictionaryEntry de in dict)
                {
                    var key = de.Key as InteractablePreset.InteractionAction;
                    if (key == null) continue;
                    if (IsTalkLabel(key.ToString()))
                    {
                        action = key;
                        return true;
                    }
                }
            }
            else
            {
                foreach (var entry in (IEnumerable)dictObj)
                {
                    dynamic e = entry;
                    object? keyObj = null;
                    try { keyObj = e.Key; } catch { }
                    var key = keyObj as InteractablePreset.InteractionAction;
                    if (key != null && IsTalkLabel(key.ToString()))
                    {
                        action = key;
                        return true;
                    }
                }
            }
        }
        catch (Exception ex) { BTB_TalkLog.Err($"[BTB] TryGetTalkActionFromAIRef failed: {ex}"); }
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

            // Light, locale-agnostic enable: obtain the Talk InteractionAction from AI ref and surface it.
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
