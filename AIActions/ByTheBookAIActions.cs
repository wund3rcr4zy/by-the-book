using ByTheBook.Dialog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Rewired.Controller;

namespace ByTheBook.AIActions
{
    public class ByTheBookAIActions
    {
        private static ByTheBookAIActions __instance;
        public static ByTheBookAIActions Instance
        {
            get 
            {
                if (__instance == null)
                {
                    __instance = new ByTheBookAIActions();
                    __instance.Init();
                }
                return __instance; 
            }
        }



        public readonly Dictionary<string, Action<AIActionPreset, Interactable, Actor>> AIActionDictionary 
            = new Dictionary<string, Action<AIActionPreset, Interactable, Actor>>();

        private void Init()
        {
            AIActionDictionary.Add(ForceSeenUnusualAction.Instance.presetName, RelaySeenUnusual);
        }

        public static void RelaySeenUnusual(AIActionPreset action, Interactable what, Actor who)
        {
            ByTheBookPlugin.Instance.Log.LogInfo("In RelaySeenUnusual.");
            if (what?.controller == null && (what?.belongsTo?.isPlayer ?? false) && !(who?.isPlayer ?? false))
            {
                what = who.interactable;
                who = Player.Instance;
            }

            var human = what?.isActor;
            if (human != null && (who?.isPlayer ?? false)) 
            {
                EvidenceWitness.DialogOption dialogOption = null;

                if (human.evidenceEntry.dialogOptions.TryGetValue((Evidence.DataKey)ByTheBookDataKey.Default, out var dialogOptions))
                {
                    foreach (var option in dialogOptions)
                    {
                        if (option.preset.name == SeekDetectiveDialogPreset.NAME)
                        {
                            dialogOption = option;
                            break;
                        }
                    }
                }

                if (dialogOption != null) 
                {
                   
                    ByTheBookPlugin.Instance.Log.LogInfo($"Invoking dialog (forceSuccess) with preset: {dialogOption.preset?.presetName}");
                    human?.speechController?.Speak(ByTheBookDialogManager.DDS_BLOCKS_DICTIONARY, ByTheBookDialogManager.SEEK_DETECTIVE_BLOCK_ID, useParsing: true, speakingAbout: Player.Instance, speakingTo: who.interactable);
                    human?.ai.TalkTo((InteractionController.ConversationType)ByTheBookDialogSpecialCases.SeekDetective);
                }            
            }
        }
    }
}
