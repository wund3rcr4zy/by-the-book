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

                ImmutableList<ByTheBookSyncEffects> upgradeOptions;
                if (ByTheBookUpgradeManager.Instance.TryGetSyncUpgrades(application.upgrade, out upgradeOptions))
                {
                    if (option >= 0 && option <  upgradeOptions.Count) 
                    {
                        ByTheBookUpgradeManager.Instance.EnableUpgrade(upgradeOptions[option]);
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

                ImmutableList<ByTheBookSyncEffects> effectsToRemove;
                if (ByTheBookUpgradeManager.Instance.TryGetSyncUpgrades(removal.upgrade, out effectsToRemove))
                {
                    foreach (ByTheBookSyncEffects effectToDisable in effectsToRemove)
                    {
                        ByTheBookUpgradeManager.Instance.DisableUpgrade(effectToDisable);
                    }
                }
            }
        }


        [HarmonyPatch(typeof(SyncDiskElementController), nameof(SyncDiskElementController.Setup))]
        public class SyncDiskElementControllerHooks
        {
            [HarmonyPrefix]
            public static void Prefix(UpgradesController.Upgrades newUpgrade)
            {
                // TODO: figure out why the SyncDiskPreset is not present on the Upgrade.
                // This issue is the root of other hackiness required in the code.
                if (newUpgrade?.upgrade == PrivateEyeSyncDiskPreset.NAME && newUpgrade?.preset == null)
                {
                    ByTheBookPlugin.Logger.LogWarning($"SyncDiskElementControllerHook: Hack Forcing PrivateEye preset. Really need to figure out why this happens.");
                    newUpgrade.preset = PrivateEyeSyncDiskPreset.Instance;
                }
            }
        }
    }
}
