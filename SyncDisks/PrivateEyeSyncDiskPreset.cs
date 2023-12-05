using ByTheBook.Upgrades;
using System.Collections.Immutable;
using System.Linq;
using UnityEngine;
using static SyncDiskPreset;

namespace ByTheBook.SyncDisks
{
    public enum ByTheBookSyncEffects
    {
        GuardGuestPass = 50,
        CrimeSceneGuestPass = 51
    }

    public class PrivateEyeSyncDiskPreset
    {
        public const string NAME = "private-eye";

        private static SyncDiskPreset _instance;
        public static SyncDiskPreset Instance
        {
            get
            {
                if (_instance == null)
                {
                    ByTheBookPlugin.Logger.LogInfo($"BTB: Creating PrivateEye SyncDisk Preset.");
                    _instance = ScriptableObject.CreateInstance<SyncDiskPreset>();
                    Init(_instance);

                    // TODO: Work more directly with the game's upgrade system rather than creating this key to upgrade list map.
                    // ^ This is currently difficult to do with IL2Cpp.
                    ByTheBookUpgradeManager.Instance.AddSyncUpgradeEffects($"{NAME}_{UpgradesController.SyncDiskState.notInstalled}_0", _instance, ImmutableList.Create<ByTheBookSyncEffects>());
                    ByTheBookUpgradeManager.Instance.AddSyncUpgradeEffects($"{NAME}_{UpgradesController.SyncDiskState.option1}_0", _instance, ImmutableList.Create(ByTheBookSyncEffects.GuardGuestPass));
                    ByTheBookUpgradeManager.Instance.AddSyncUpgradeEffects($"{NAME}_{UpgradesController.SyncDiskState.option1}_1", _instance, ImmutableList.Create(ByTheBookSyncEffects.GuardGuestPass, ByTheBookSyncEffects.CrimeSceneGuestPass));
                }

                return _instance;
            }
        }

        // Seems like constructor work, but I don't think I can trust ScriptableObject.CreateInstance with a constructor.
        private static void Init(SyncDiskPreset instance)
        {
            instance.presetName = NAME;
            instance.name = instance.presetName;

            instance.syncDiskNumber = (int)ByTheBookSyncEffects.GuardGuestPass;

            instance.price = ByTheBookPlugin.Instance.Config.Bind("SyncDiskCosts", NAME, 500).Value;
            instance.rarity = Rarity.medium;

            instance.mainEffect1 = (Effect)ByTheBookSyncEffects.GuardGuestPass;
            instance.mainEffect1Name = $"{NAME}_effect1_name";
            instance.mainEffect1Description = $"{NAME}_effect1_description";

            Il2CppSystem.Collections.Generic.List<UpgradeEffect> upgradeEffects = new Il2CppSystem.Collections.Generic.List<UpgradeEffect>();
            upgradeEffects.Add((UpgradeEffect)((int)ByTheBookSyncEffects.CrimeSceneGuestPass));
            instance.option1UpgradeEffects = upgradeEffects;

            Il2CppSystem.Collections.Generic.List<string> upgradeNames = new Il2CppSystem.Collections.Generic.List<string>();
            upgradeNames.Add($"{NAME}_upgrade1_description");
            instance.option1UpgradeNameReferences = upgradeNames;

            Il2CppSystem.Collections.Generic.List<float> upgradeValues = new Il2CppSystem.Collections.Generic.List<float>();
            upgradeValues.Add(1.0f);
            instance.option1UpgradeValues = upgradeValues;

            instance.mainEffect2 = Effect.none;
            instance.mainEffect3 = Effect.none;

            instance.interactable = Resources.FindObjectsOfTypeAll<InteractablePreset>()
                .Where(preset => preset.presetName == "SyncDisk")
                .LastOrDefault();

            instance.canBeSideJobReward = true;

            instance.manufacturer = Manufacturer.Kaizen;
        }
    }
}
