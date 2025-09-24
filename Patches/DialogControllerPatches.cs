using ByTheBook.Dialog;
using ByTheBook.Upgrades;
using ByTheBook.Utils;
using HarmonyLib;
using System;
using static DialogController;

namespace ByTheBook.Patches
{
    internal class DialogControllerPatches
    {
        [HarmonyPatch(typeof(DialogController), nameof(DialogController.ExecuteDialog))]
        public class DialogExecuteHook
        {
            private static readonly Random random = new Random(Guid.NewGuid().GetHashCode());
            private static readonly int GUARD_PASS_MAX_CHANCE_SOCIAL_LEVEL = Math.Clamp(
                ByTheBookPlugin.Instance.Config.Bind(
                    "SyncDisk",
                    "guard-pass-max-chance-social-credit-level",
                    3,
                    "Scales guest-pass success chance against the game's social credit thresholds. Higher tier (1-8) = stricter (lower chance)."
                ).Value,
                1, 8);

            [HarmonyPrefix]
            public static void Prefix(EvidenceWitness.DialogOption dialog, Interactable saysTo, NewNode where, Actor saidBy, ForceSuccess forceSuccess, ref bool __runOriginal)
            {
                // TODO: figure out a more flexible way of doing this. I can't seem to be able to use the original function to execute the dialog.
                // I believe this is because the <DialogPreset, MethodInfo> reflection map relies on the Object invoking the MethodInfo to be a DialogController.
                // With no way of adding functions to the DialogController, I get an "Object is not of the correct type" error during the runtime invoke.
                if (dialog?.preset == GuardGuestPassDialogPreset.Instance)
                {
                    Citizen citizen = saysTo?.controller?.GetComponentInParent<Citizen>();
                    ByTheBookPlugin.Logger.LogDebug($"Executing Dialog: {dialog?.preset?.name} executed by: {saidBy?.name}");


                    if (citizen != null)
                    {
                        bool success = false;
                        switch (forceSuccess)
                        {
                            case ForceSuccess.success:
                                success = true;
                                break;
                            case ForceSuccess.fail:
                                success = false;
                                break;
                            default:
                                double guardPassSocialCreditRequired = Convert.ToDouble(GameplayController.Instance.GetSocialCreditThresholdForLevel(GUARD_PASS_MAX_CHANCE_SOCIAL_LEVEL));
                                double socialCreditNumerator = Math.Clamp(Convert.ToDouble(GameplayController.Instance.socialCredit), 1.0, guardPassSocialCreditRequired + 1.0);
                                // Compute a threshold to beat: higher social credit lowers the threshold, making success more likely
                                double requiredThreshold = 0.99 - Math.Clamp((socialCreditNumerator / guardPassSocialCreditRequired), 0, 0.75);
                                double randomDouble = random.NextDouble();
                                success = (randomDouble >= requiredThreshold);
                                // FIX: Force success only when the upgrade's ALWAYS-PASS effect is enabled.
                                // Base disk enables GuardGuestPass (dialog available). The upgrade enables CrimeSceneGuestPass (guaranteed pass).
                                if (ByTheBookUpgradeManager.Instance.IsCrimeSceneGuestPassEnabled())
                                {
                                    success = true;
                                }
                                ByTheBook.ByTheBookPlugin.Logger.LogInfo($"GuardIssueGuestPass rolled: {randomDouble} - required: {requiredThreshold}");
                                break;
                        }

                        ByTheBook.ByTheBookPlugin.Logger.LogInfo($"GuardIssueGuestPass - success?: {success}");
                        ByTheBookDialogManager.Instance.IssueGuardGuestPass(citizen, saysTo, where, saidBy, success, dialog.roomRef, dialog.jobRef);
                    }              
                    __runOriginal = false;
                }
            }
        }

        [HarmonyPatch(typeof(DialogController), nameof(DialogController.TestSpecialCaseAvailability))]
        public class DialogControllerSpecialCaseHook
        {
            [HarmonyPostfix]
            public static void Postfix(ref bool __result, DialogPreset preset, Citizen saysTo, SideJob jobRef)
            {
                if (__result) { return; }

                switch ((int)preset.specialCase)
                {
                    case ((int)ByTheBookDialogSpecialCases.GuardGuestPass):
                        __result = IsEnforcerGuardingLatestMurderScene(saysTo);
                        ByTheBookPlugin.Instance.Log.LogInfo($"GuardGuestPass special case: {__result}");
                        break;
                }
            }

            private static bool IsEnforcerGuardingLatestMurderScene(Citizen saysTo)
            {
                if (!ByTheBookUpgradeManager.Instance.GuardGuestPassEnabled)
                {
                    return false;
                }

                MurderController.Murder? latestMurder = MurderController.Instance.GetLatestMurder();
                if (latestMurder == null || !saysTo.isEnforcer)
                {
                    return false;
                }

                GameplayController.EnforcerCall enforcerCallForMurder;
                if (GameplayController.Instance.enforcerCalls.TryGetValue(latestMurder.location, out enforcerCallForMurder))
                {
                    return (enforcerCallForMurder.guard == saysTo.humanID);
                }

                return false;
            }
        }
    }
}
