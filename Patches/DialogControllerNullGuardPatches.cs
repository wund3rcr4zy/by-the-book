// Patches/DialogControllerNullGuardPatches.cs
// v0.2.2 — Defensive crash guard for DialogController.TestSpecialCaseAvailability
//
// OVERVIEW
// ──────────────────────────────────────────────────────────────────────────────
// During dialog teardown (or other edge transitions), the game may invoke
// DialogController.TestSpecialCaseAvailability(...) with a null/destroyed
// DialogPreset or Citizen. Other patches sometimes dereference fields like
// `preset.name`, which turns that into a NullReferenceException and crashes.
//
// WHAT THIS PATCH DOES
// • Prefix (Priority.First) — If `preset` or `saysTo` is null/destroyed, we
//   short-circuit with `__result = false` (option “not available”) and skip the
//   original. This prevents any dereference in downstream patches.
// • Finalizer — Wraps the original + all patches and converts any thrown
//   exception into `__result = false` instead of crashing the game.
// • Logging — Error-only (release-friendly).
//
// IMPLEMENTATION NOTES
// • Unity “truthiness”: destroyed UnityEngine.Object instances compare equal to
//   null, so we check both `obj == null` and `!obj`.
// • Conservative behavior: this never enables an option; it only prevents
//   crashes by reporting the option as unavailable on bad inputs.
//
// COMPATIBILITY
// • Harmony prefix priority is set to run first; finalizer safely swallows
//   exceptions from any patch/original below.
//
// VERSION HISTORY
// • 0.2.2
//   - Added Prefix with [HarmonyPriority(Priority.First)] to short-circuit on null/destroyed args.
//   - Added Finalizer to swallow exceptions and return __result=false instead of crashing.
//   - Kept logging to error-only for release builds.
//   - Clarified header and inline comments for PR review.

#nullable enable
using System;
using HarmonyLib;
using UnityEngine;
using BepInEx.Logging;

internal static class BTB_DialogGuardLog
{
    private static ManualLogSource? _log;
    public static ManualLogSource Log =>
        _log ??= BepInEx.Logging.Logger.CreateLogSource("ByTheBook");
    public static void Err(string msg) => Log.LogError(msg);
}

[HarmonyPatch(typeof(DialogController), nameof(DialogController.TestSpecialCaseAvailability))]
static class BTB_DialogController_TestSpecialCaseAvailability_Guard
{
    // Run first so we can safely short-circuit on invalid args.
    [HarmonyPrefix, HarmonyPriority(Priority.First)]
    static bool Prefix(ref bool __result, DialogPreset preset, Citizen saysTo, SideJob jobRef)
    {
        try
        {
            // Unity "truthy" check: destroyed objects compare equal to null.
            if (preset == null || !preset)
            {
                __result = false;   // option not available → no crash
                return false;       // skip original
            }

            if (saysTo == null || !saysTo)
            {
                __result = false;
                return false;
            }
        }
        catch (Exception ex)
        {
            // Extremely defensive: if our guard fails, still avoid the call chain.
            BTB_DialogGuardLog.Err($"[BTB] Dialog guard prefix failed: {ex}");
            __result = false;
            return false;
        }

        return true; // continue to other patches/original
    }

    // Final safety net: swallow any exception and downgrade to "not available".
    [HarmonyFinalizer]
    static Exception? Finalizer(Exception __exception, ref bool __result)
    {
        if (__exception != null)
        {
            try
            {
                BTB_DialogGuardLog.Err($"[BTB] Suppressed dialog availability exception: {__exception}");
            }
            catch { /* avoid recursive logging issues */ }

            __result = false;
            return null; // swallow the exception
        }

        return null;
    }
}
