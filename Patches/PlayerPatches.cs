using ByTheBook.Upgrades;
using BepInEx.Configuration;
using HarmonyLib;
using System;

namespace ByTheBook.Patches
{
    internal class PlayerPatches
    {
        [HarmonyPatch(typeof(Actor), nameof(Actor.AddPersuedBy))]
        public class ActorPersuedByHook
        {
            // Configurable knobs for the pursuit social credit penalty
            private static readonly int CRIME_PENALTY_DIVISOR = Math.Clamp(
                ByTheBookPlugin.Instance.Config.Bind(
                    "EnabledSideEffects",
                    "social-credit-penalty-divisor",
                    150,
                    "Divisor used to scale pursuit social credit penalty: deduction = total fines / divisor."
                ).Value,
                1, int.MaxValue);

            private static readonly int MAXIMUM_SOCIAL_CREDIT_PENALTY = Math.Clamp(
                ByTheBookPlugin.Instance.Config.Bind(
                    "EnabledSideEffects",
                    "social-credit-penalty-cap",
                    100,
                    "Maximum social credit deducted once at the start of a pursuit episode."
                ).Value,
                0, int.MaxValue);

            [HarmonyPrefix]
            public static void Prefix(Actor __instance)
            {
                if (!__instance.isPlayer || !ByTheBookUpgradeManager.Instance.IsCrimePursuitSocialCreditEnabled())
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
                if (!ByTheBookUpgradeManager.Instance.IsCrimeSceneGuestPassEnabled())
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
