/*
 *  UpgradesLinkerPatches.cs
 *  v0.2.1 — “By-the-Book” sync-disk linkage hardening (IL2CPP / Harmony)
 *
 *  WHAT THIS DOES
 *  ─────────────────────────────────────────────────────────────────────────────
 *  Fixes a race where the Sync Disk UI row is built before a custom disk’s
 *  SyncDiskPreset is linked to its Upgrades row, causing NREs in
 *  SyncDiskElementController.Setup(...). We ensure the preset is resolvable from
 *  UpgradesController.upgradesQuickRef and “late-link” it if necessary before
 *  the UI reads from it. We also keep the quickRef populated across screen
 *  opens so your disk always renders correctly.
 *
 *  GAME API TOUCHPOINTS
 *  ─────────────────────────────────────────────────────────────────────────────
 *  • UpgradesController
 *      - SetupQuickRef(): ensure our preset exists in upgradesQuickRef and
 *        opportunistically relink any rows that loaded without a preset.
 *      - UpdateUpgrades(): pre-pass to relink rows created after SetupQuickRef
 *        (robust against load order).
 *      - Fields used: upgrades (List<Upgrades>), spawnedDisks (List<SyncDiskElementController>),
 *        upgradesQuickRef (Dictionary<string, SyncDiskPreset>), isOpen, _instance.
 *
 *  • SyncDiskElementController
 *      - Setup(Upgrades newUpgrade): late-link the preset in case it arrived
 *        null, then allow the original Setup to run. While diagnosing, we
 *        skip the original if the preset is still missing to prevent an NRE.
 *
 *  • UpgradeEffectController
 *      - Utility: ForceRefresh() calls OnSyncDiskChange(true) for a clean,
 *        official state refresh after programmatic installs/uninstalls.
 *
 *  HOW TO USE / TWEAK
 *  ─────────────────────────────────────────────────────────────────────────────
 *  • Upgrade key: by default this file targets the “private-eye” disk via
 *    BTB_UpgradesLinker.UpgradeKey. If you rename the disk, update that
 *    constant to your new key (must match Upgrades.upgrade).
 *
 *  • Logging: Info/Warn/Debug are compiled out in Release via [Conditional("DEBUG")].
 *    Errors (exceptions) are always logged.
 *
 *  • Safety guard: the Setup() prefix currently skips the original method if
 *    the preset is still null *after* late-linking. Once your logs show it’s
 *    always resolved, you can remove that early-return to reduce noise.
 *
 *  • One-liner late-link: BTB_LinkHelpers.LateLinkPreset(u) uses TryGetValue
 *    (Unity-friendly) and null-coalescing assignment (??=) to attach the
 *    preset only when missing.
 *
 *  COMPATIBILITY / ASSUMPTIONS
 *  ─────────────────────────────────────────────────────────────────────────────
 *  • BepInEx 6 (IL2CPP) + Harmony.
 *  • Uses Il2CppInterop and Unity 2021 APIs; no modern BCL extensions required.
 *  • Does not change vanilla behavior for other disks; only fills in missing
 *    preset references and protects against early UI reads.
 *
 *  VERSIONING / NOTES
 *  ─────────────────────────────────────────────────────────────────────────────
 *  • 0.2.1  One-liner helper, pre-/post-hooks, and diagnostic breadcrumbs; prevents
 *           NRE in SyncDiskElementController.Setup when presets are late. Also includes
 *           zero-risk tweaks: filter hidden/internal objects in the Resources scan,
 *           extra null guards, and a low Harmony priority hint on the quick-ref patch.
 */

#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics; // ConditionalAttribute
using System.Runtime.CompilerServices;
using HarmonyLib;
using UnityEngine;
using BepInEx.Logging; // ManualLogSource

// Local logger proxy without depending on your plugin class.
// In Release builds, Info/Warn/Debug calls are compiled out entirely.
internal static class BTB_Log
{
    private static ManualLogSource? _log;
    public static ManualLogSource Log =>
        _log ??= BepInEx.Logging.Logger.CreateLogSource("ByTheBook"); // fully-qualified to avoid CS0104

    [Conditional("DEBUG")] public static void Info(string msg) => Log.LogInfo(msg);
    [Conditional("DEBUG")] public static void Warn(string msg) => Log.LogWarning(msg);
    [Conditional("DEBUG")] public static void Debug(string msg) => Log.LogDebug(msg);

    // Always-on for exception paths
    public static void Error(string msg) => Log.LogError(msg);
}

public static class BTB_UpgradesLinker
{
    public const string UpgradeKey = "private-eye";  // MUST match UpgradesController.Upgrades.upgrade

    // Set this from your factory if you build the preset in code.
    public static SyncDiskPreset? CachedPreset;

    [Conditional("DEBUG")] public static void LogInfo(string msg) => BTB_Log.Info(msg);
    [Conditional("DEBUG")] public static void LogWarn(string msg) => BTB_Log.Warn(msg);
    public static void LogErr(string msg)  => BTB_Log.Error(msg);
    [Conditional("DEBUG")] public static void LogDbg(string msg)  => BTB_Log.Debug(msg);

    public static SyncDiskPreset? GetOrFindPreset()
    {
        if (CachedPreset != null)
        {
            if (CachedPreset) return CachedPreset; // Unity "truthy" check
            CachedPreset = null; // stale destroyed ref
        }

        try
        {
            // One-time scan during boot/setup is fine (very slow; don't do every frame).
            // Filter out hidden/internal objects to avoid odd picks.
            var all = Resources.FindObjectsOfTypeAll<SyncDiskPreset>();
            foreach (var p in all)
            {
                if (!p || p.hideFlags != HideFlags.None) continue; // ignore internal/hidden
                if (string.Equals(p.name, UpgradeKey, StringComparison.Ordinal))
                {
                    CachedPreset = p;
#if DEBUG
                    LogInfo($"[BTB] Found SyncDiskPreset in resources: '{p.name}'.");
#endif
                    return CachedPreset;
                }
            }
        }
        catch (Exception ex)
        {
            LogErr($"[BTB] Error scanning for SyncDiskPreset: {ex}");
        }

#if DEBUG
        LogWarn($"[BTB] Could not find SyncDiskPreset by name '{UpgradeKey}'. If you create it in code, set BTB_UpgradesLinker.CachedPreset after creation.");
#endif
        return null;
    }

    public static int RelinkMissingPresets(UpgradesController uc)
    {
        if (uc == null || uc.upgrades == null) return 0;

        int fixedCount = 0;
        try
        {
            foreach (var u in uc.upgrades)
            {
                if (u == null) continue;
                if (u.preset != null) continue;
                if (string.IsNullOrEmpty(u.upgrade)) continue;

                if (uc.upgradesQuickRef.TryGetValue(u.upgrade, out var p) && p != null)
                {
                    u.preset = p;
                    fixedCount++;
                }
            }
        }
        catch (Exception ex)
        {
            LogErr($"[BTB] Exception while relinking presets: {ex}");
        }
        return fixedCount;
    }

    public static void Dump(UpgradesController uc, string when)
    {
#if DEBUG
        try
        {
            var keys = string.Join(", ", uc.upgradesQuickRef.Keys);
            LogDbg($"[BTB] quickRef keys {when}: {keys}");

            foreach (var u in uc.upgrades)
            {
                if (u == null) continue;
                var presetName = u.preset ? u.preset.name : "null";
                LogDbg($"[BTB] row {when}: upgrade='{u.upgrade}', preset='{presetName}', state={u.state}, level={u.level}");
            }
        }
        catch (Exception ex)
        {
            LogErr($"[BTB] Dump error: {ex}");
        }
#endif
    }
}

// --- ONE-LINER HELPER ---
internal static class BTB_LinkHelpers
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LateLinkPreset(UpgradesController.Upgrades u)
        => u.preset ??=
            (UpgradesController._instance != null
             && !string.IsNullOrEmpty(u.upgrade)
             && UpgradesController._instance.upgradesQuickRef.TryGetValue(u.upgrade, out var p))
            ? p
            : null;
}

// Ensure the preset exists in quickRef + relink after quickRef is built
[HarmonyPatch(typeof(UpgradesController), nameof(UpgradesController.SetupQuickRef))]
[HarmonyPriority(HarmonyLib.Priority.Low)] // ordering hint only (safe)
static class BTB_LinkPresetIntoQuickRef_Postfix
{
    static void Postfix(UpgradesController __instance)
    {
        if (__instance == null) return;

        if (!__instance.upgradesQuickRef.TryGetValue(BTB_UpgradesLinker.UpgradeKey, out var preset) || preset == null)
        {
            var p = BTB_UpgradesLinker.GetOrFindPreset();
            if (p != null)
            {
                __instance.upgradesQuickRef[BTB_UpgradesLinker.UpgradeKey] = p;
                BTB_UpgradesLinker.LogInfo($"[BTB] Registered '{BTB_UpgradesLinker.UpgradeKey}' in upgradesQuickRef.");
            }
            else
            {
                BTB_UpgradesLinker.LogWarn($"[BTB] Preset for '{BTB_UpgradesLinker.UpgradeKey}' unavailable during SetupQuickRef.");
            }
        }

        int relinked = BTB_UpgradesLinker.RelinkMissingPresets(__instance);
        if (relinked > 0)
            BTB_UpgradesLinker.LogInfo($"[BTB] Linked {relinked} upgrade row(s) to their SyncDiskPreset.");

        BTB_UpgradesLinker.Dump(__instance, "after-SetupQuickRef");
    }
}

// Relink just before the UI rebuilds, to catch rows created after SetupQuickRef runs.
[HarmonyPatch(typeof(UpgradesController), nameof(UpgradesController.UpdateUpgrades))]
static class BTB_UpgradesController_UpdateUpgrades_Prefix
{
    static void Prefix(UpgradesController __instance)
    {
        if (__instance == null) return;

        // light-touch pass
        int relinked = BTB_UpgradesLinker.RelinkMissingPresets(__instance);
        if (relinked > 0)
            BTB_UpgradesLinker.LogInfo($"[BTB] Pre-UpdateUpgrades relinked {relinked} upgrade row(s).");
    }
}

// Use the one-liner inside Setup; only skip if still null to avoid NREs.
[HarmonyPatch(typeof(SyncDiskElementController), nameof(SyncDiskElementController.Setup))]
static class BTB_SyncDiskElementController_Setup_Prefix
{
    static bool Prefix(UpgradesController.Upgrades newUpgrade)
    {
        // breadcrumb (debug-only)
        BTB_UpgradesLinker.LogInfo($"[BTB] application.upgrade = '{newUpgrade?.upgrade}'");

        if (newUpgrade != null)
        {
            var before = newUpgrade.preset;
            BTB_LinkHelpers.LateLinkPreset(newUpgrade);

            if (before == null && newUpgrade.preset != null)
                BTB_UpgradesLinker.LogInfo($"[BTB] Late-linked preset for '{newUpgrade.upgrade}' in Setup.");
        }

        // If we still couldn't resolve it, skip to prevent NRE while diagnosing.
        if (newUpgrade == null || newUpgrade.preset == null)
        {
            BTB_UpgradesLinker.LogWarn("[BTB] Skipping SyncDiskElementController.Setup: 'newUpgrade' or its 'preset' was null (preventing NRE).");
            return false;
        }

        return true; // run the original Setup
    }
}

// Optional: use after programmatic install/uninstall to refresh effects/state.
public static class BTB_UpgradeEffectRefresh
{
    public static void ForceRefresh()
    {
        try
        {
            UpgradeEffectController._instance?.OnSyncDiskChange(true);
            // No info logs in Release by design
        }
        catch (Exception ex)
        {
            BTB_UpgradesLinker.LogErr($"[BTB] Error forcing sync disk refresh: {ex}");
        }
    }
}
