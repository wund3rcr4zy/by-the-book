using ByTheBook.SyncDisks;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                ByTheBookPlugin.Instance.DisableUpgrade(ByTheBookSyncEffects.PrivateEye);
                foreach (var upgrade in load.upgrades)
                {
                    if (upgrade?.upgrade == PrivateEyeSyncDiskPreset.NAME && upgrade?.preset == null) 
                    {
                        ByTheBookPlugin.Logger.LogWarning($"SaveStateControllerHook: Hack Forcing PrivateEye preset. Really need to figure out why this happens.");
                        upgrade.preset = PrivateEyeSyncDiskPreset.Instance;
                        ByTheBookPlugin.Instance.EnableUpgrade(ByTheBookSyncEffects.PrivateEye);
                        break;
                    }
                }
            }
        }
    }
}
