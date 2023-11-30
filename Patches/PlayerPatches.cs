using ByTheBook.SyncDisks;
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
                if (!ByTheBookPlugin.Instance.IsUpgradeEnabled(SyncDisks.ByTheBookSyncEffects.PrivateEye))
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
