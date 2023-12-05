using System;
using Il2CppInterop.Runtime.Injection;

namespace ByTheBook.Dialog
{
    public class GuardGuestPassAISuccessResponsePreset
    {
        public const string NAME = "guard-guest-pass-response";
        public const string DDS_STRING_ID = "a3caf795-6987-4e72-9d19-c6d2fc94a7fe";

        private static AIActionPreset.AISpeechPreset _instance;

        public static AIActionPreset.AISpeechPreset Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new AIActionPreset.AISpeechPreset(ClassInjector.DerivedConstructorPointer<AIActionPreset.AISpeechPreset>());
                    _instance.endsDialog = true;
                    _instance.onlyIfEnfocerOnDuty = true;
                    _instance.ddsMessageID = "38da3c6d-3065-434b-992a-0cc190d39dd7";
                }

                return _instance;
            }
        }
    }

    public class GuardGuestPassAIFailureResponsePreset
    {
        public const string NAME = "guard-guest-pass-response-fail";
        public const string DDS_STRING_ID = "16e1892c-2ded-4f8b-a38d-a6f2322358db";

        private static AIActionPreset.AISpeechPreset _instance;

        public static AIActionPreset.AISpeechPreset Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new AIActionPreset.AISpeechPreset(ClassInjector.DerivedConstructorPointer<AIActionPreset.AISpeechPreset>());
                    _instance.endsDialog = true;
                    _instance.onlyIfEnfocerOnDuty = true;
                    _instance.ddsMessageID = "38da3c6d-3065-434b-992a-0cc190d39dd7";
                }

                return _instance;
            }
        }
    }
}
