using BepInEx.Unity.IL2CPP.Utils.Collections;
using ByTheBook.Upgrades;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ByTheBook.Dialog
{

    public class GuardGuestPassDialogPreset : DialogPreset
    {
        public const string NAME = "guard-guest-pass";

        private static GuardGuestPassDialogPreset _instance;
        public static GuardGuestPassDialogPreset Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = ScriptableObject.CreateInstance<GuardGuestPassDialogPreset>();
                    Init(_instance);
                }

                return _instance;
            }
        }

        // Seems like constructor work, but I don't think I can trust ScriptableObject.CreateInstance with a constructor.
        private static void Init(GuardGuestPassDialogPreset instance)
        {
            instance.presetName = NAME;
            instance.name = instance.presetName;

            instance.affectChanceIfRestrained = 0;
            instance.baseChance = 1;
            instance.cost = 0;
            instance.displayAsIllegal = false;
            instance.isJobDetails = false;
            instance.specialCase = (SpecialCase)((int)ByTheBookDialogSpecialCases.GuardGuestPass);
            instance.telephoneCallOption = false;
            instance.hospitalDecisionOption = false;
            instance.ignoreActiveJobRequirement = true;
            instance.inputBox = InputSetting.none;
            instance.msgID = "b0bde6bb-0803-4e23-b441-8ceb16c87133";
            instance.tiedToKey = Evidence.DataKey.voice;

            Il2CppSystem.Collections.Generic.List<AIActionPreset.AISpeechPreset> potentialResponses = new Il2CppSystem.Collections.Generic.List<AIActionPreset.AISpeechPreset>();
            potentialResponses.Add(GuardGuestPassAISuccessResponsePreset.Instance);
            potentialResponses.Add(GuardGuestPassAIFailureResponsePreset.Instance);

            instance.responses = potentialResponses;

            // TODO: Dialog is not being removed after trying it. A bug because the dialog option can be spammed until success.
            instance.removeAfterSaying = true;

            // TODO: is this actually working? Can't really test until the dialog is removed as it should be after trying.
            instance.dailyReplenish = true;
        }
    }
}
