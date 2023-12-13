using ByTheBook.SyncDisks;
using ByTheBook.Upgrades;
using HarmonyLib;
using System.Collections.Immutable;

namespace ByTheBook.Patches
{
    internal class SyncDiskPatches
    {
        [HarmonyPatch(typeof(UpgradesController), nameof(UpgradesController.InstallSyncDisk))]
        public class InstallSyncDiskHook
        {
            [HarmonyPostfix]
            public static void Postfix(UpgradesController.Upgrades application, int option)
            {
                if (application == null || application.preset == null)
                {
                    return;
                }

                string upgradeKey = $"{application.upgrade}_option{option}_{application.level}";
                ByTheBookPlugin.Instance.Log.LogInfo($"Attempting to install syncDisk group: {upgradeKey}.");
                if (ByTheBookUpgradeManager.Instance.TryGetSyncUpgrades(upgradeKey, out ImmutableList<ByTheBookSyncEffects> upgradeOptions))
                {
                    ByTheBookPlugin.Instance.Log.LogInfo($"Found upgrades: {upgradeOptions.Count}");
                    foreach (var effect in upgradeOptions) 
                    {
                        ByTheBookUpgradeManager.Instance.EnableUpgrade(effect);
                    }
                }
                   
            }
        }

        [HarmonyPatch(typeof(UpgradesController), nameof(UpgradesController.UpgradeSyncDisk))]
        public class UpgradeSyncDiskHook
        {
            [HarmonyPostfix]
            public static void Postfix(UpgradesController.Upgrades upgradeThis)
            {
                if (upgradeThis == null || upgradeThis.preset == null)
                {
                    return;
                }

                string upgradeKey = $"{upgradeThis.upgrade}_{upgradeThis.state}_{upgradeThis.level}";
                ByTheBookPlugin.Instance.Log.LogInfo($"Attempting to install upgrade group: {upgradeKey}.");
                if (ByTheBookUpgradeManager.Instance.TryGetSyncUpgrades(upgradeKey, out ImmutableList<ByTheBookSyncEffects> upgradeOptions))
                {
                    ByTheBookPlugin.Instance.Log.LogInfo($"Found upgrades: {upgradeOptions.Count}");
                    foreach (var effect in upgradeOptions)
                    {
                        ByTheBookUpgradeManager.Instance.EnableUpgrade(effect);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(UpgradesController), nameof(UpgradesController.UninstallSyncDisk))]
        public class UninstallSyncDiskHook
        {
            [HarmonyPostfix]
            public static void Postfix(UpgradesController.Upgrades removal)
            {
                if (removal == null || removal.upgrade == null)
                {
                    return;
                }

                ImmutableList<ByTheBookSyncEffects> effectsToRemove = ImmutableList.Create<ByTheBookSyncEffects>();
                string upgradeKey = $"{removal.upgrade}_{removal.state}_{removal.level}";
                if (ByTheBookUpgradeManager.Instance.TryGetSyncUpgrades(upgradeKey, out effectsToRemove))
                {
                    foreach (ByTheBookSyncEffects effectToDisable in effectsToRemove)
                    {
                        ByTheBookUpgradeManager.Instance.DisableUpgrade(effectToDisable);
                    }
                }
            }
        }
    }
}
