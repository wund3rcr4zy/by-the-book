// Patches/DialogControllerNullGuardPatches.cs
// v0.2.1 — Crash guard for DialogController.TestSpecialCaseAvailability
//
// Why: Other patches may dereference `preset.name`. During dialog teardown,
//      the game can legitimately call this with a null/destroyed `preset`,
//      causing an NRE and a crash.
//
// What this does:
//   • Prefix (Priority.First): if `preset` (or `saysTo`) is null/destroyed,
//     short-circuit with __result=false so the option is simply unavailable.
//   • Finalizer: catch any exception from other prefixes/original and turn it
//     into __result=false instead of a crash.
//
// Logging: error-only (release-friendly).

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
