using System;
using Il2CppInterop.Runtime.Injection;

namespace ByTheBook.Dialog
{
    public class GuardGuestPassAISuccessResponsePreset : AIActionPreset.AISpeechPreset
    {
        public const string NAME = "guard-guest-pass-response";
        public const string DDS_STRING_ID = "a3caf795-6987-4e72-9d19-c6d2fc94a7fe";

        private static GuardGuestPassAISuccessResponsePreset _instance;

        public GuardGuestPassAISuccessResponsePreset(IntPtr ptr) : base(ptr) 
        {
            this.endsDialog = true;
            this.onlyIfEnfocerOnDuty = true;
            this.ddsMessageID = "38da3c6d-3065-434b-992a-0cc190d39dd7";
        }

        public static GuardGuestPassAISuccessResponsePreset Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GuardGuestPassAISuccessResponsePreset(ClassInjector.DerivedConstructorPointer<GuardGuestPassAISuccessResponsePreset>());
                }

                return _instance;
            }
        }
    }

    public class GuardGuestPassAIFailureResponsePreset : AIActionPreset.AISpeechPreset
    {
        public const string NAME = "guard-guest-pass-response-fail";
        public const string DDS_STRING_ID = "16e1892c-2ded-4f8b-a38d-a6f2322358db";

        private static GuardGuestPassAIFailureResponsePreset _instance;

        public GuardGuestPassAIFailureResponsePreset(IntPtr ptr) : base(ptr)
        {
            this.endsDialog = true;
            this.onlyIfEnfocerOnDuty = true;
            this.ddsMessageID = "38da3c6d-3065-434b-992a-0cc190d39dd7";
        }

        public static GuardGuestPassAIFailureResponsePreset Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GuardGuestPassAIFailureResponsePreset(ClassInjector.DerivedConstructorPointer<GuardGuestPassAIFailureResponsePreset>());
                }

                return _instance;
            }
        }
    }
}
