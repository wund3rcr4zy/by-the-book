using System;
using System.Collections.Generic;
using static DialogController;

namespace ByTheBook.Dialog
{
    public class ByTheBookDialogActions
    {
        private static readonly Random random = new Random(Guid.NewGuid().GetHashCode());
        private static readonly int GUARD_PASS_MAX_CHANCE_SOCIAL_LEVEL = Math.Clamp(ByTheBookPlugin.Instance.Config.Bind("SyncDisks", "guard-pass-max-chance-social-credit-level", 3).Value, 1, 8);

        public static readonly IReadOnlyDictionary<DialogPreset, Action<EvidenceWitness.DialogOption, Interactable, NewNode, Actor, DialogController.ForceSuccess>>
            DialogActionDictionary = new Dictionary<DialogPreset, Action<EvidenceWitness.DialogOption, Interactable, NewNode, Actor, DialogController.ForceSuccess>>()
            {
                { GuardGuestPassDialogPreset.Instance, OnGuardGuestPassDialog },
                { SeekDetectiveDialogPreset.Instance, OnSeekDetectiveSeenUnusual }
            };

        #region GuardGuestPass
        public static void OnGuardGuestPassDialog(EvidenceWitness.DialogOption dialog, Interactable saysTo, NewNode where, Actor saidBy, ForceSuccess forceSuccess)
        {
            Citizen citizen = saysTo?.controller?.GetComponentInParent<Citizen>();
            ByTheBookPlugin.Logger.LogDebug($"Executing Dialog: {dialog?.preset?.name} executed by: {saidBy?.name}");


            if (citizen != null)
            {
                bool success = false;
                switch (forceSuccess)
                {
                    case ForceSuccess.success:
                        success = true;
                        break;
                    case ForceSuccess.fail:
                        success = false;
                        break;
                    default:
                        double guardPassSocialCreditRequired = Convert.ToDouble(GameplayController.Instance.GetSocialCreditThresholdForLevel(GUARD_PASS_MAX_CHANCE_SOCIAL_LEVEL));
                        double socialCreditNumerator = Math.Clamp(Convert.ToDouble(GameplayController.Instance.socialCredit), 1.0, guardPassSocialCreditRequired + 1.0);
                        double successChance = 0.99 - Math.Clamp((socialCreditNumerator / guardPassSocialCreditRequired), 0, 0.75);
                        double randomDouble = random.NextDouble();
                        success = (randomDouble >= successChance);
                        ByTheBookPlugin.Logger.LogInfo($"GuardIssueGuestPass rolled: {randomDouble} - required: {successChance}");
                        break;
                }

                ByTheBookPlugin.Logger.LogInfo($"GuardIssueGuestPass - success?: {success}");
                IssueGuardGuestPass(citizen, saysTo, where, saidBy, success, dialog.roomRef, dialog.jobRef);
            }
        }

        public static void IssueGuardGuestPass(Citizen saysTo, Interactable saysToInteractable, NewNode where, Actor saidBy, bool success, NewRoom roomRef, SideJob jobRef)
        {
            ByTheBookPlugin.Instance.Log.LogDebug($"IssueGuardGuestPass function called. Citizen?: {saysTo?.GetFirstName()} with SpeechController exists?: {saysTo?.speechController != null}");

            if (success)
            {
                saysTo?.speechController?.Speak(ByTheBookDialogManager.DDS_BLOCKS_DICTIONARY, GuardGuestPassAISuccessResponsePreset.DDS_STRING_ID,
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
                saysTo?.speechController?.Speak(ByTheBookDialogManager.DDS_BLOCKS_DICTIONARY, GuardGuestPassAIFailureResponsePreset.DDS_STRING_ID,
                    dialogPreset: GuardGuestPassDialogPreset.Instance, dialog: GuardGuestPassAIFailureResponsePreset.Instance);
            }

            // TODO: I don't think this is properly making use of the game's SpeechHistory functionality.. but this works for now.
            saysTo?.evidenceEntry?.RemoveDialogOption(Evidence.DataKey.voice, GuardGuestPassDialogPreset.Instance, newSideJob: null, roomRef: null);
        }
        #endregion

        #region SeekDetective
        public static void OnSeekDetectiveSeenUnusual(EvidenceWitness.DialogOption dialog, Interactable saysTo, NewNode where, Actor saidBy, ForceSuccess forceSuccess)
        {
            Citizen citizen = saysTo?.controller?.GetComponentInParent<Citizen>();
            ByTheBookPlugin.Logger.LogDebug($"Executing Dialog: {dialog?.preset?.name} executed by: {saidBy?.name}");


            if (citizen != null)
            {
                DialogController.Instance.SeenOrHeardUnusual(citizen, saysTo, where, saidBy, success: true, null, null);
            }
        }
        #endregion
    }
}
