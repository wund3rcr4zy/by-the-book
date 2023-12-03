using ByTheBook.SyncDisks;
using ByTheBook.Upgrades;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByTheBook.Patches
{
    internal class PlayerPatches
    {
        [HarmonyPatch(typeof(Player), nameof(Player.OnGameLocationChange))]
        public class PlayerLocationChangeHook
        {
            [HarmonyPrefix]
            public static void Prefix()
            {
                // TODO: now that the dialog option for a pass is in, this should only be unabled if upgrade status 1.
                if (!ByTheBookUpgradeManager.Instance.IsUpgradeEnabled(SyncDisks.ByTheBookSyncEffects.PrivateEye))
                {
                    return;
                }

                if (Player.Instance.currentGameLocation.isCrimeScene && !Player.Instance.currentGameLocation.isOutside)
                {
                    GameplayController.Instance.AddGuestPass(Player.Instance.currentGameLocation.thisAsAddress, 2);
                }
            }
        }
    }
}
