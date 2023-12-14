using ByTheBook.SyncDisks;
using ByTheBook.Upgrades;
using HarmonyLib;
using System;

namespace ByTheBook.Patches
{
    internal class PlayerPatches
    {
        [HarmonyPatch(typeof(Actor), nameof(Actor.AddPersuedBy))]
        public class ActorPersuedByHook
        {
            /// <summary>
            ///  0.75 %
            /// </summary>
            private const int CRIME_PENALTY_DIVISOR = 150;

            private const int MAXIMUM_SOCIAL_CREDIT_PENALTY = 100;

            [HarmonyPrefix]
            public static void Prefix(Actor __instance)
            {
                if (!__instance.isPlayer || !ByTheBookUpgradeManager.Instance.IsEffectEnabled(ByTheBookSyncEffects.CrimePersuitSocialCredit))
                {
                    return;
                }

                if (Player.Instance.persuedBy.Count == 0 && Player.Instance.persuedProgress <= 0)
                {
                    // Going to ingore the possibility of hitting MAX_INT for now. Won't happen in normal gameplay.
                    int totalPenalty = 0;
                    foreach (var statusCountList in StatusController.Instance.activeStatusCounts.Values)
                    {
                        foreach (var statusCount in statusCountList)
                        {
                            totalPenalty += statusCount.GetPenaltyAmount();
                        }
                    }

                    ByTheBookPlugin.Instance.Log.LogInfo($"Calculated Total Penalty: {totalPenalty}");
                    int deductionAmount = Math.Min(
                        totalPenalty / CRIME_PENALTY_DIVISOR, 
                        MAXIMUM_SOCIAL_CREDIT_PENALTY
                        );

                    if (deductionAmount > 0)
                    {
                        ByTheBookPlugin.Instance.Log.LogInfo($"Player persued while bounty active. Deducting {deductionAmount} social credit. (totalFines/150 max 100)");
                        GameplayController.Instance.AddSocialCredit(-deductionAmount, true, "Someone caught you doing illegal activities!");
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.OnGameLocationChange))]
        public class PlayerLocationChangeHook
        {
            [HarmonyPrefix]
            public static void Prefix()
            {
                if (!ByTheBookUpgradeManager.Instance.IsEffectEnabled(ByTheBookSyncEffects.CrimeSceneGuestPass))
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
