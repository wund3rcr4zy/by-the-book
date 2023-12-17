using ByTheBook.Dialog;
using ByTheBook.Upgrades;
using ByTheBook.Utils;
using HarmonyLib;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static DialogController;
using static EvidenceWitness;
using static MS.Internal.Xml.XPath.Operator;
using Object = UnityEngine.Object;

namespace ByTheBook.Patches
{
    public class DialogPatches
    {
        [HarmonyPatch(typeof(InteractionController), nameof(InteractionController.RefreshDialogOptions))]
        public class InteractionControllerSetDialogHook
        {
            [HarmonyPrefix]
            public static void Prefix(InteractionController __instance, ref bool __runOriginal)
            {
                ByTheBookPlugin.Instance.Log.LogInfo($"In InteractionController SetDialog prefix.");

                if (((int)__instance.dialogType) == ((int)ByTheBookDialogSpecialCases.SeekDetective))
                {
                    __runOriginal = false;

                    // Most of this copied from the actual code because there doesn't seem to be an easy way to add new ConversationTypes without  such a workaround.

                    EvidenceWitness evidenceWitness = __instance.talkingTo.isActor.evidenceEntry;
                    if (__instance.citizenNameText != null)
                    {
                        __instance.citizenNameText.text = evidenceWitness?.GetNameForDataKey(Evidence.DataKey.voice);
                    }

                    for (int i = 0; i < __instance.dialogOptions.Count; i++)
                    {
                        Object.Destroy(__instance.dialogOptions[i].gameObject);
                    }
                    __instance.dialogOptions.Clear();

                    List<EvidenceWitness.DialogOption> list = new List<EvidenceWitness.DialogOption>();
                    if ((__instance.talkingTo?.speechController?.speechQueue?.Count ?? 0) <= 0)
                    {
                        // LINQ operations don't player nicely with IL2Cpp. Can't use AddRange because this particular list is System.
                        var foundOps = evidenceWitness.GetDialogOptions((Evidence.DataKey)ByTheBookDataKey.Default);
                        if (foundOps != null)
                        {
                            foreach (var op in foundOps)
                            {
                                ByTheBookPlugin.Instance.Log.LogInfo($"Adding dialogOp: {op?.preset?.presetName}");
                                list.Add(op);
                            }
                        }

                        list.Sort((EvidenceWitness.DialogOption p1, EvidenceWitness.DialogOption p2) => p2.preset.ranking.CompareTo(p1.preset.ranking));
                    }

                    int num = 0;
                    for (int j = 0; j < list.Count; j++)
                    {
                        EvidenceWitness.DialogOption dialogOption = list[j];
                        DialogButtonController component = Object.Instantiate<GameObject>(PrefabControls.Instance.dialogOption, PrefabControls.Instance.dialogOptionContainer).GetComponent<DialogButtonController>();
                        component.Setup(dialogOption);
                        component.rect.anchoredPosition = new Vector2(0f, (float)num);
                        num -= 52;
                        __instance.dialogOptions.Add(component);
                        component.SetSelectable(val: true);
                    }
                    __instance.SetDialogSelection(Mathf.Clamp(__instance.dialogSelection, 0, __instance.dialogOptions.Count - 1));
                }
            }
        }

        [HarmonyPatch(typeof(DialogController), nameof(DialogController.ExecuteDialog))]
        public class DialogExecuteHook
        {
            [HarmonyPrefix]
            public static void Prefix(EvidenceWitness.DialogOption dialog, Interactable saysTo, NewNode where, Actor saidBy, ForceSuccess forceSuccess, ref bool __runOriginal)
            {
                // TODO: figure out a more flexible way of doing this. I can't seem to be able to use the original function to execute the dialog.
                // I believe this is because the <DialogPreset, MethodInfo> reflection map relies on the Object invoking the MethodInfo to be a DialogController.
                // With no way of adding functions to the DialogController, I get an "Object is not of the correct type" error during the runtime invoke.
                if (dialog?.preset != null && ByTheBookDialogActions.Instance.DialogActionDictionary.TryGetValue(dialog.preset.presetName, out var handleDialogOptionAction))
                {
                    ByTheBookPlugin.Instance.Log.LogInfo($"Found dialog handler for {dialog.preset.presetName}. Invoking...");
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
