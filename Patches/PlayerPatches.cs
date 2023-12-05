using ByTheBook.SyncDisks;
using ByTheBook.Upgrades;
using HarmonyLib;

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
                if (!ByTheBookUpgradeManager.Instance.IsUpgradeEnabled(ByTheBookSyncEffects.CrimeSceneGuestPass))
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
