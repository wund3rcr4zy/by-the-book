using Il2CppInterop.Runtime.Injection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByTheBook.Dialog
{
    public enum ByTheBookDialogSpecialCases
    {
        GuardGuestPass = 100
    }

    public class ByTheBookDialogManager : Il2CppSystem.Object
    {
        public const string DDS_BLOCKS_DICTIONARY = "dds.blocks";

        private static ByTheBookDialogManager __instance;

        public static ByTheBookDialogManager Instance
        { 
            get 
            {
                if (__instance == null)
                {
                    __instance = new ByTheBookDialogManager(ClassInjector.DerivedConstructorPointer<ByTheBookDialogManager>());
                }

                return __instance; 
            } 
        }

        public ByTheBookDialogManager(IntPtr ptr) : base(ptr)
        {
        }


        public void IssueGuardGuestPass(Citizen saysTo, Interactable saysToInteractable, NewNode where, Actor saidBy, bool success, NewRoom roomRef, SideJob jobRef)
        {
            ByTheBookPlugin.Instance.Log.LogDebug($"IssueGuardGuestPass function called. Citizen?: {saysTo?.GetFirstName()} with SpeechController exists?: {saysTo?.speechController != null}");

            if (success)
            {
                saysTo?.speechController?.Speak(DDS_BLOCKS_DICTIONARY, GuardGuestPassAISuccessResponsePreset.DDS_STRING_ID, 
                    dialogPreset: GuardGuestPassDialogPreset.Instance, dialog: GuardGuestPassAISuccessResponsePreset.Instance);

                if (MurderController.Instance.activeMurders.Count > 0)
                {
                    MurderController.Murder activeMurder = MurderController.Instance.activeMurders[MurderController.Instance.activeMurders.Count - 1];
                    if (!activeMurder.location.isOutside && !activeMurder.location.isLobby)
                    {
                        GameplayController.Instance.AddGuestPass(activeMurder.location.thisAsAddress, 2);
                    }
                }
            }
            else
            {
                saysTo?.speechController?.Speak(DDS_BLOCKS_DICTIONARY, GuardGuestPassAIFailureResponsePreset.DDS_STRING_ID, 
                    dialogPreset: GuardGuestPassDialogPreset.Instance, dialog: GuardGuestPassAIFailureResponsePreset.Instance);
            }

            // TODO: I don't think this is properly making use of the game's SpeechHistory functionality.. but this works for now.
            saysTo?.evidenceEntry?.RemoveDialogOption(Evidence.DataKey.voice, GuardGuestPassDialogPreset.Instance, newSideJob: null, roomRef: null);
        }
    }
}
