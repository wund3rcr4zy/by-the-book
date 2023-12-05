﻿using ByTheBook.Dialog;
using HarmonyLib;
using System.Linq;
using UnityEngine;

namespace ByTheBook.Patches
{
    internal class InteractablePatches
    {
        // This seems to make the "Talk To" action available for the Enforcer, but none of the dialog options trigger any type of response.
        // It also did not trigger the GuardIssueGuestPass callback when selected.
        [HarmonyPatch(typeof(Interactable), nameof(Interactable.UpdateCurrentActions))]
        public class InteractableHook
        {
            private static AIActionPreset __talkTo;
            private static AIActionPreset TalkTo
            {
                get
                {
                    if (__talkTo == null)
                    {
                        __talkTo = Resources.FindObjectsOfTypeAll<AIActionPreset>().Where(preset => preset.presetName == "TalkTo").LastOrDefault();
                    }
                    return __talkTo;
                }
            }

            [HarmonyPostfix]
            public static void Postfix(Interactable __instance)
            {
                Human human = __instance?.belongsTo;
                if (human == null || !human.isEnforcer || __instance.speechController == null)
                {
                    return;
                }

                human.evidenceEntry?.AddDialogOption(Evidence.DataKey.voice, GuardGuestPassDialogPreset.Instance, newSideJob: null, roomRef: null, allowPresetDuplicates: false);
                if (TalkTo != null && (__instance?.currentActions?.TryGetValue(InteractablePreset.InteractionKey.primary, out var currentAction) ?? false))
                {
                    // This is required because normally, we are not allowed to talk to active duty Enforcers at the scene of a crime.
                    // Force the "Talk To" interaction to be present.
                    // TODO: relying on the name of interactions is brittle and prone to breaking. This also includes searching for the "TalkTo" AI action above.
                    if (currentAction.currentAction.interactionName == "Take Print")
                    {
                        ByTheBookPlugin.Instance.Log.LogDebug("Set TalkTo action on guard.");
                        currentAction.currentAction.action = TalkTo;
                        currentAction.currentAction.interactionName = "Talk To";
                        currentAction.currentAction.specialCase = InteractablePreset.InteractionAction.SpecialCase.none;
                    }
                }
            }
        }
    }
}