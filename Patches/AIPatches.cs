using ByTheBook.AIActions;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        [HarmonyPatch(typeof(ActionController), nameof(ActionController.TalkTo))]
        public class ActionControllerTalkToHook
        {
            [HarmonyPrefix]
            public static void Prefix(ActionController __instance, ref Interactable what, ref Actor who)
            {
                //ByTheBookPlugin.Instance.Log.LogInfo($"WhatCtrlActorNull?: {what.controller.isActor == null}");

                // TODO: Why is the interactable controller null here when I'm passing the player's interactable?
                // It makes no sense... This probably has adver
                if (what?.controller == null && (what?.belongsTo?.isPlayer ?? false) && !(who?.isPlayer ?? false))
                {
                    what = who.interactable;
                    who = Player.Instance;
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
