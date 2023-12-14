using ByTheBook.SyncDisks;
using ByTheBook.Upgrades;
using HarmonyLib;
using System;
using System.Collections.Immutable;


namespace ByTheBook.Patches
{
    internal class SaveStatePatches
    {

        [HarmonyPatch(typeof(SaveStateController), nameof(SaveStateController.LoadSaveState))]
        public class LoadSaveHook
        {
            [HarmonyPrefix]
            public static void Prefix(StateSaveData load)
            {
                ByTheBookUpgradeManager.Instance.DisableAllEffects();
                
                foreach (var upgrade in load.upgrades)
                {
                    string upgradeKey = $"{upgrade.upgrade}_{upgrade.state}_{upgrade.level}";
                    bool containsKey = ByTheBookUpgradeManager.Instance.byTheBookSyncDisks.ContainsKey(upgradeKey);
                    ByTheBookPlugin.Logger.LogDebug($"Have related upgrade: {upgradeKey}?: {containsKey}");

                    if (!containsKey)
                    {
                        upgradeKey = $"{upgrade.upgrade}_{UpgradesController.SyncDiskState.option1}_{upgrade.level}";
                        containsKey = ByTheBookUpgradeManager.Instance.byTheBookSyncDisks.ContainsKey(upgradeKey);
                        ByTheBookPlugin.Logger.LogDebug($"Have related upgrade fallback: {upgradeKey}?: {containsKey}");
                    }

                    ImmutableList<ByTheBookSyncEffects> upgradeEffects = ImmutableList.Create<ByTheBookSyncEffects>();
                    if (ByTheBookUpgradeManager.Instance.TryGetSyncUpgrades(upgradeKey, out upgradeEffects)) 
                    {
                        if (upgrade.preset == null)
                        {
                            ByTheBookPlugin.Logger.LogWarning($"SaveStateControllerHook: Hack Forcing {upgrade.upgrade} preset. Really need to figure out why this happens.");
                            upgrade.preset = ByTheBookUpgradeManager.Instance.byTheBookSyncDisks.GetValueSafe(upgradeKey);
                        }

                        if (UpgradesController.SyncDiskState.notInstalled == upgrade.state)
                        {
                            continue;
                        }

                        foreach (ByTheBookSyncEffects effect in upgradeEffects)
                        {
                            ByTheBookUpgradeManager.Instance.EnableEffect(effect);
                        }
                    }
                }

                if (Game.Instance.giveAllUpgrades)
                {
                    foreach (var upgrade in Enum.GetValues<ByTheBookSyncEffects>())
                    {
                        ByTheBookUpgradeManager.Instance.EnableEffect(upgrade);
                    }
                }
            }
        }
    }
}
