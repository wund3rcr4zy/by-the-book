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


            [HarmonyPrefix]
            public static void Prefix(EvidenceWitness.DialogOption dialog, Interactable saysTo, NewNode where, Actor saidBy, ForceSuccess forceSuccess, ref bool __runOriginal)
            {
                // TODO: figure out a more flexible way of doing this. I can't seem to be able to use the original function to execute the dialog.
                // I believe this is because the <DialogPreset, MethodInfo> reflection map relies on the Object invoking the MethodInfo to be a DialogController.
                // With no way of adding functions to the DialogController, I get an "Object is not of the correct type" error during the runtime invoke.
                if (dialog?.preset != null && ByTheBookDialogActions.DialogActionDictionary.TryGetValue(dialog.preset, out var handleDialogOptionAction))
                {
                    handleDialogOptionAction.Invoke(dialog, saysTo, where, saidBy, forceSuccess);
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
                if (!ByTheBookUpgradeManager.Instance.IsEffectEnabled(SyncDisks.ByTheBookSyncEffects.GuardGuestPass))
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
