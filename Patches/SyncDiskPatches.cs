using ByTheBook.SyncDisks;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

                // TODO: correctly handle options and allow for more than mainEffect1
                if (ByTheBookPlugin.Instance.byTheBookSyncDisks.ContainsKey(application.preset.presetName))
                {
                    ByTheBookPlugin.Instance.EnableUpgrade((ByTheBookSyncEffects)application.preset.mainEffect1);
                }
            }
        }

        [HarmonyPatch(typeof(UpgradesController), nameof(UpgradesController.UninstallSyncDisk))]
        public class UninstallSyncDiskHook
        {
            [HarmonyPostfix]
            public static void Postfix(UpgradesController.Upgrades removal)
            {
                if (removal == null || removal.preset == null)
                {
                    return;
                }

                // TODO: correctly handle options and allow for less hardcoding.
                if (PrivateEyeSyncDiskPreset.NAME == removal.preset.presetName)
                {
                    ByTheBookPlugin.Instance.DisableUpgrade((ByTheBookSyncEffects)removal.preset.mainEffect1);
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
                // This issue is the root of other hackiness required in the code and why this mod
                // can only support 1 sync disk as of now.
                if (newUpgrade != null && newUpgrade.preset == null)
                {
                    ByTheBookPlugin.Logger.LogWarning($"SyncDiskElementControllerHook: Hack Forcing PrivateEye preset. Really need to figure out why this happens.");
                    newUpgrade.preset = PrivateEyeSyncDiskPreset.Instance;
                }
            }
        }
    }
}
