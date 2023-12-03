using ByTheBook.SyncDisks;
using ByTheBook.Upgrades;
using HarmonyLib;
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
                ByTheBookUpgradeManager.Instance.DisableAllUpgrades();

                foreach (var upgrade in load.upgrades)
                {
                    ImmutableList<ByTheBookSyncEffects> upgradeEffects;
                    if (ByTheBookUpgradeManager.Instance.TryGetSyncUpgrades(upgrade.upgrade, out upgradeEffects)) 
                    {
                        ByTheBookPlugin.Logger.LogWarning($"SaveStateControllerHook: Hack Forcing {upgrade.upgrade} preset. Really need to figure out why this happens.");     
                        upgrade.preset = ByTheBookUpgradeManager.Instance.byTheBookSyncDisks[upgrade.upgrade];

                        if (UpgradesController.SyncDiskState.notInstalled == upgrade.state)
                        {
                            continue;
                        }

                        foreach (ByTheBookSyncEffects effect in upgradeEffects)
                        {
                            ByTheBookUpgradeManager.Instance.EnableUpgrade(effect);
                        }
                    }
                }
            }
        }
    }
}
