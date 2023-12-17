using ByTheBook.AIActions;
using ByTheBook.Dialog;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DialogController;

namespace ByTheBook.Patches
{
    internal class AIPatches
    {
        [HarmonyPatch(typeof(NewAIController), nameof(NewAIController.OnReturnFromTalkTo))]
        public class NewAIControllerTalkToReturnHook
        {
            [HarmonyPostfix]
            public static void Postfix(NewAIController __instance)
            {
                // TODO: Without removing the action, the AI repeatedly tried to talk to the player, locking them in place.
                // This was even though the goal / action was marked as completable and had no repeat options.
                // need to figure out why this happens.
                if (__instance?.currentGoal?.preset?.presetName == SeekOutDetectiveGoal.NAME)
                {
                    ByTheBookPlugin.Instance.Log.LogInfo($"Removing currentAction: {SeekOutDetectiveGoal.NAME}");
                    __instance?.currentAction?.Remove();
                    __instance?.currentGoal?.Remove();
                }
            }
        }

        [HarmonyPatch(typeof(ActionController), nameof(ActionController.ExecuteAction))]
        public class ActionControllerExecuteActionHook
        {
            [HarmonyPrefix]
            public static void Prefix(ActionController __instance, AIActionPreset action,  Interactable what, Actor who, ref bool __runOriginal)
            {
                if (action != null && ByTheBookAIActions.Instance.AIActionDictionary.TryGetValue(action.presetName, out var handleAction))
                {
                    ByTheBookPlugin.Instance.Log.LogInfo($"Found action handler for {action?.presetName}. Invoking...");
                    handleAction.Invoke(action, what, who);
                    __runOriginal = false;
                }
            }
        }

        [HarmonyPatch(typeof(Human.Death), nameof(Human.Death.SetReported))]
        public class DeathReportedHook
        {
            [HarmonyPostfix]
            public static void Postfix(Human.Death __instance)
            {
                ByTheBookPlugin.Instance.Log.LogInfo($"DeathReported Postfix triggered.");

                if (!CityData.Instance.GetHuman(__instance.victim, out Human humanForId) || humanForId.ai == null)
                {
                    return;
                }


                foreach (Acquaintance acquaintance in humanForId.acquaintances)
                {
                    NewAIGoal newAIGoal = null;
                    foreach (NewAIGoal goal in acquaintance.with.ai.goals)
                    {
                        if (goal.preset == SeekOutDetectiveGoal.Instance)
                        {
                            newAIGoal = goal;
                            break;
                        }
                    }

                    if (newAIGoal == null)
                    {
                        ByTheBookPlugin.Instance.Log.LogInfo("Create SeekOutDetective goal for " + acquaintance.with.GetCitizenName());
                        acquaintance.with.ai.CreateNewGoal(SeekOutDetectiveGoal.Instance, newTrigerTime: 0f, newDuration: 0f, newPassedInteractable: Player.Instance.interactable);
                    }
                    else
                    {
                        newAIGoal.activeTime = 0f;
                    }
                }
            }
        }
    }
}
