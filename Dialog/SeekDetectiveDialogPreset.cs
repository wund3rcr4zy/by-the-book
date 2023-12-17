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
            instance.msgID = "1122d9da-03be-48ec-8f3c-61d8e21fab0a";
            instance.tiedToKey = (Evidence.DataKey)ByTheBookDataKey.Default;

            Il2CppSystem.Collections.Generic.List<AIActionPreset.AISpeechPreset> potentialResponses = new Il2CppSystem.Collections.Generic.List<AIActionPreset.AISpeechPreset>();
            instance.responses = potentialResponses;

            instance.defaultOption = true;
            instance.removeAfterSaying = true;
            instance.dailyReplenish = true;
        }
    }
}
