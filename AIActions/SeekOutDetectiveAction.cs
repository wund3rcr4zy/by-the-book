using ByTheBook.Dialog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DialogPreset;
using UnityEngine;
using Il2CppSystem.Collections.Generic;
using BepInEx.Unity.IL2CPP.Utils.Collections;

namespace ByTheBook.AIActions
{
    public class SeekOutDetectiveGoal
    {
        public const string NAME = "by-the-book-seek-detective-goal";

        private static AIGoalPreset _instance;
        public static AIGoalPreset Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = ScriptableObject.CreateInstance<AIGoalPreset>();
                    Init(_instance);
                }

                return _instance;
            }
        }

        private static void Init(AIGoalPreset instance)
        {
            instance.presetName = NAME;
            instance.name = instance.presetName;

            instance.startingGoal = true;
            instance.forcePriorityUpdateOnCreation = true;
            instance.appliesTo = AIGoalPreset.StartingGoal.all;
            instance.category = AIGoalPreset.GoalCategory.vital;
            instance.completable = true;
            
            instance.basePriority = 11;
            //AddSeekOutDetectiveGoalModifiers(instance.goalModifiers);
            AddSeekOutDetectiveGoalSetup(instance.actionsSetup);

            instance.validBetweenHours = new Vector2(0f, 23.5f);
            instance.dontUpdateGoalPriorityWhileActive = true;
            instance.rainFactor = AIGoalPreset.RainFactor.none;
            instance.affectPriorityOverTime = true;
            instance.locationOption = AIGoalPreset.LocationOption.useCurrent;
            instance.actionSource = AIGoalPreset.GoalActionSource.thisConfiguration;

            instance.roomOption = AIGoalPreset.RoomOption.none;
        }

        private static void AddSeekOutDetectiveGoalSetup(Il2CppSystem.Collections.Generic.List<AIGoalPreset.GoalActionSetup> goalSetupList)
        {
            var setupSeekAction = new AIGoalPreset.GoalActionSetup()
            {
                condition = AIGoalPreset.ActionCondition.always
            };

            setupSeekAction.actions.Add(SeekOutDetectiveAction.Instance);
            goalSetupList.Add(setupSeekAction);
        }

        private static void AddSeekOutDetectiveGoalModifiers(Il2CppSystem.Collections.Generic.List<AIGoalPreset.GoalModifierRule> modifierRules)
        {
            Il2CppSystem.Collections.Generic.List<CharacterTrait> requiredTraits = new Il2CppSystem.Collections.Generic.List<CharacterTrait>();
            int count = 0;
            int index = 0;
            while (count < 3 && index < Toolbox.Instance.allCharacterTraits.Count)
            {
                var trait = Toolbox.Instance.allCharacterTraits[index];
                if (trait.presetName == "Char-Trusting" || trait.presetName == "Char-Friendly" || trait.presetName == "Char-Honest")
                {
                    requiredTraits.Add(trait);
                    count++;
                }

                index++;
            }

            var rule = new AIGoalPreset.GoalModifierRule()
            {
                rule = CharacterTrait.RuleType.ifAnyOfThese,
                traitList = requiredTraits,
                mustPassForApplication = true,
                priorityMultiplier = 0.0f
            };

            modifierRules.Add(rule);
        }
    }

    public class SeekOutDetectiveAction
    {
        public const string NAME = "by-the-book-seek-detective";

        private static AIActionPreset _instance;
        public static AIActionPreset Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = ScriptableObject.CreateInstance<AIActionPreset>();
                    Init(_instance);
                }

                return _instance;
            }
        }

        private static void Init(AIActionPreset instance)
        {
            instance.presetName = NAME;
            instance.name = instance.presetName;

            AIActionPreset.AutomaticAction automaticAction = new AIActionPreset.AutomaticAction()
            {
                forcedAction = ForceSeenUnusualAction.Instance,
                proximityCheck = true
            };

            instance.forcedActionsSearchLevel = AIActionPreset.ForcedActionsSearchLevel.thisObjectOnly;
            instance.forcedActionsOnArrival.Add(automaticAction);
            instance.requiresForcedUpdate = true;
            //instance.dontUpdateGoalPriorityWhileActive = true;
            instance.inputPriority = 11;

            instance.specificOutfitOnArrive = true;
            instance.makeClothedOnArrive = false;
            instance.allowedOutfitOnArrive = ClothesPreset.OutfitCategory.undressed;
            instance.completableAction = true;
            instance.avoidRepeatingInteractables = true;
            instance.repeatOnComplete = false;


            instance.defaultKey = InteractablePreset.InteractionKey.none;

            instance.actionLocation = AIActionPreset.ActionLocation.player;
            instance.facePlayerWhileTalkingTo = true;
            instance.facing = AIActionPreset.ActionFacingDirection.player;
            instance.runIfSeesPlayer = true;
            instance.completeOnSeeIllegal = true;     
            instance.searchSetting = AIActionPreset.FindSetting.nonTrespassing;
            instance.onUsePointBusy = AIActionPreset.ActionBusy.skipAction;

            instance.holsterCurrentItemOnAction = true;
            instance.disableConversationTriggers = true;
            instance.exitConversationOnActivate = true;
        }
    }

    public class ForceSeenUnusualAction
    {
        public const string NAME = "force-seen-unusual-action";

        private static AIActionPreset _instance;
        public static AIActionPreset Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = ScriptableObject.CreateInstance<AIActionPreset>();
                    Init(_instance);
                }

                return _instance;
            }
        }

        private static void Init(AIActionPreset instance)
        {
            instance.presetName = NAME;
            instance.name = instance.presetName;

            instance.requiresForcedUpdate = true;
            instance.completableAction = true;
            instance.avoidRepeatingInteractables = true;
            instance.repeatOnComplete = false;

            instance.defaultKey = InteractablePreset.InteractionKey.none;
            instance.actionLocation = AIActionPreset.ActionLocation.player;
            instance.facePlayerWhileTalkingTo = true;
            instance.facing = AIActionPreset.ActionFacingDirection.player;
            instance.runIfSeesPlayer = true;
            instance.completeOnSeeIllegal = true;
            instance.searchSetting = AIActionPreset.FindSetting.nonTrespassing;
            instance.onUsePointBusy = AIActionPreset.ActionBusy.skipAction;
            instance.holsterCurrentItemOnAction = true;
            instance.disableConversationTriggers = true;
            instance.exitConversationOnActivate = true;
        }
    }
}
