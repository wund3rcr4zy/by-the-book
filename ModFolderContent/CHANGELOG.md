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
