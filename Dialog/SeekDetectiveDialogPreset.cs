using UnityEngine;
using static DialogPreset;

namespace ByTheBook.Dialog
{

    public class SeekDetectiveDialogPreset
    {
        public const string NAME = "seek-detective";

        private static DialogPreset _instance;
        public static DialogPreset Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = ScriptableObject.CreateInstance<DialogPreset>();
                    Init(_instance);
                }

                return _instance;
            }
        }

        // Seems like constructor work, but I don't think I can trust ScriptableObject.CreateInstance with a constructor.
        private static void Init(DialogPreset instance)
        {
            instance.presetName = NAME;
            instance.name = instance.presetName;

            instance.affectChanceIfRestrained = 0;
            instance.baseChance = 1;
            instance.cost = 0;
            instance.displayAsIllegal = false;
            instance.isJobDetails = false;
            instance.specialCase = (SpecialCase)((int)ByTheBookDialogSpecialCases.SeekDetective);
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

            instance.defaultOption = true;
            instance.removeAfterSaying = true;
            instance.dailyReplenish = true;
        }
    }
}
