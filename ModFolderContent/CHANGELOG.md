# 0.3.0

! Breaking: Version 0.3.0 is NOT compatible with saves created on older versions (< 0.3.0). Please start a new game when upgrading to 0.3.0 or later.

* Core: Migrated the Private Eye sync disk to SOD.Common (v2.1.0) using the builder + event pipeline (install/upgrade/uninstall) and PoliceAutomat sale location.
* Compatibility: Preserved legacy DDS sync disk strings and inject them after Toolbox initialization to avoid early-initialization issues.
* Upgrades: Reworked effect toggles to a simple flag manager (GuardGuestPass / CrimeSceneGuestPass / CrimePursuitSocialCredit) driven by SOD.Common events.
* UI: Fixed duplicate disk entries in the Weapons Locker by deduplicating PoliceAutomat sync disks on every Toolbox.LoadAll (runs with a late Harmony priority).
* Dialog: Kept legacy DDS-driven Guard Guest Pass dialog/preset with success/failure responses and daily replenish; maintained on‑duty “Talk To” injection for enforcers; success roll and “always pass” upgrade behavior unchanged.
* Cleanup: Removed obsolete Sync Disk UI linker patches (UpgradesLinkerPatches.cs) that are no longer needed with SOD.Common registration.
* Packaging: Added Thunderstore dependency `Venomaus-SODCommon-2.1.0` to the manifest.
* Config: Renamed keys to `SyncDisk.private-eye-cost` and `SyncDisk.guard-pass-max-chance-social-credit-level` and added knobs `EnabledSideEffects.social-credit-penalty-divisor` and `EnabledSideEffects.social-credit-penalty-cap`. All options are pre-bound so they appear immediately in BepInEx config.
* Migration: One-time migration of legacy config keys (with automatic cleanup of old entries) guarded by a side-file marker in Savestore.

# 0.2.3

* Feature: On-duty interaction injection for Enforcers at active crime scenes. **“Talk To”** is always surfaced; **“Inspect”** is surfaced when present. Applies when the target is the call **guard** or in the **response** list and the call state is **responding/arrived**.
* Behavior: No new actions added or priorities changed — we only toggle `enabled`/`display` on existing slots and never touch `specialCase`, priority, or input bindings.
* UX: HUD text/icons (`UpdateInteractionText`, `UpdateInteractionIcons`) are refreshed when we force visibility so the buttons appear immediately.
* Matching: Action label matching prefers per-slot `overrideInteractionName`, falling back to preset `interactionName`; comparison is case/whitespace-insensitive (`@"\s+"` normalization).
* Diagnostics: Safe snapshot/dump helpers with stable `Interactable.id`; skip empty action maps to avoid spam.
* Compatibility: Postfix on `Interactable.UpdateCurrentActions()` to preserve vanilla & other mods’ action building; robust `InteractableController → Interactable` resolution; extra null guards.
* Performance: Single O(n) scan of `currentActions`; effectively negligible with logging muted.

# 0.2.1

* Stability: Sync Disk linkage hardening for **PrivateEye** (IL2CPP/Harmony); prevents rare NullReferenceExceptions in `SyncDiskElementController.Setup(...)` by late-linking the `SyncDiskPreset` before the row renders.
* Robustness: Keeps `upgradesQuickRef` populated; relinks missing presets during `SetupQuickRef` and just before `UpdateUpgrades` to handle load-order edges.
* Safety: If the preset still can’t be resolved after late-link, the original `Setup(...)` is skipped to avoid an NRE.
* Internals: Resource scan now ignores hidden/internal objects; added extra null guards; quick-ref postfix uses a low Harmony priority to better cooperate with other mods.
* Utility: `BTB_UpgradeEffectRefresh.ForceRefresh()` helper to trigger `OnSyncDiskChange(true)` after programmatic install/uninstall.

# 0.2.0

* Bugfix: the player should no longer become very tall and fall through the floor on the second upgrade of PrivateEye. (Fingers crossed. That one was weird)
* Bugfix: this mod's text should now render again after the latest game update.
* A new downside side-effect has been added to the Private-Eye sync disk. If you are persued while you have active fines, you will lose social credit.
  * The amount of social credit you will lose is a small percentage of your current fines capping at 100 social credit.
  * A player cannot go below 0 social credit. 
* A configuration option has been added to the BepInEx config to be disable the new side-effect
* Mod Dependency on piepieonline's DDSLoader removed. Ported over necessary loading code. Shout out and thank you piepieonline!

# 0.1.0

* Previous functionality moved to the "upgrade 1" slot.
* Main Effect changed: The Enforcer guard on duty at a crime scene can be asked for a guest pass. 
  * Higher social credit affects the odds of success up to a 75% success rate.
  * The "guard" is usually found standing against a wall after crime scene tape has been put up.
* A configuration option has been added to the BepInEx config to be able to change the price of the PrivateEye sync disk.
* A configuration option for the SocialCreditLevel for the max success rate of asking a guard for a pass has also been added.

# 0.0.3

* Fixed mod overwriting all other instances of sync disks.
* GuestPass will now generate upon walking into the crime scene. This allows it to work even if the sync disk was purchased after a murder was reported.

# 0.0.2

* Fixed folder structure of DDS content. SyncDisk text should now render.

# 0.0.1

* Initial Release
