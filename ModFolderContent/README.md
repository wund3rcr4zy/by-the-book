# By-The-Book — Private Eye License (Shadows of Doubt)

## IMPORTANT SAVE WARNING
- **Version 0.3.0+ is NOT compatible** with saves created with older versions (< 0.3.0). Please start a new game after upgrading.

## Overview
- Adds the **Private Eye License** sync disk (PoliceAutomat / Weapons Locker).
- Ask the **on-duty enforcer** guarding an active crime scene for a **guest pass** (daily cooldown; chance scales with social credit).
- **Upgrade:** Always receive a guest pass when entering a crime scene.
- **Optional side effect:** When pursued while you have active fines, reduce social credit (scaled by the fines).

## Requirements
- **BepInEx Pack IL2CPP 6.x**
- **SOD.Common 2.1.0**

## Install
- Install via **Thunderstore / R2Modman**, or copy this folder and the plugin DLL into your **BepInEx** profile for *Shadows of Doubt*.

## Usage
- Buy the disk from the **Weapons Locker**.
- With the disk installed, talk to the **on-duty enforcer** at the taped-off crime scene to request a guest pass (**once per in-game day**).
- With the upgrade installed, **entering a crime scene (indoors) auto-grants a guest pass**.

## Config (BepInEx)
- `SyncDisk.private-eye-cost` *(int)*: price *(default: 500)*
- `EnabledSideEffects.private-eye` *(bool)*: enable pursuit social credit penalty *(default: true)*
- `SyncDisk.guard-pass-max-chance-social-credit-level` *(1–8)*: success‑chance scaling tier *(default: 3)* — compared to the game’s social credit thresholds; a higher tier is stricter (lower chance).
- `EnabledSideEffects.social-credit-penalty-divisor` *(int)*: divisor to scale penalty; deduction = total fines / divisor *(default: 150)*
- `EnabledSideEffects.social-credit-penalty-cap` *(int)*: maximum one‑time deduction at pursuit start *(default: 100)*

## Notes
- The mod **deduplicates multiple disk entries** in the PoliceAutomat after hot-loads.
- Dialog uses the mod’s **bundled DDS assets**; no user action required.

## Credits
- ColePowered Games, Ven0maus (*SOD.Common*), Piepieonline (*DDSLoader*)
