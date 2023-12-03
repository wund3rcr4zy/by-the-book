using ByTheBook.Upgrades;
using Il2CppSystem.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ByTheBook.SyncDisks
{
    public enum ByTheBookSyncEffects
    {
        GuardGuestPass = 50,
        CrimeSceneGuestPass = 51
    }

    public class PrivateEyeSyncDiskPreset : SyncDiskPreset
    {
        public const string NAME = "private-eye";

        private static PrivateEyeSyncDiskPreset _instance;
        public static PrivateEyeSyncDiskPreset Instance
        {
            get
            {
                if (_instance == null)
                {
                    ByTheBookPlugin.Logger.LogInfo($"BTB: Creating PrivateEye SyncDisk Preset.");
                    _instance = ScriptableObject.CreateInstance<PrivateEyeSyncDiskPreset>();
                    Init(_instance);


                    ByTheBookUpgradeManager.Instance.AddSyncUpgradeEffects($"{NAME}_{UpgradesController.SyncDiskState.option1}_0", _instance, ImmutableList.Create(ByTheBookSyncEffects.GuardGuestPass));
                    ByTheBookUpgradeManager.Instance.AddSyncUpgradeEffects($"{NAME}_{UpgradesController.SyncDiskState.option1}_1", _instance, ImmutableList.Create(ByTheBookSyncEffects.GuardGuestPass, ByTheBookSyncEffects.CrimeSceneGuestPass));
                }

                return _instance;
            }
        }

        // Seems like constructor work, but I don't think I can trust ScriptableObject.CreateInstance with a constructor.
        private static void Init(PrivateEyeSyncDiskPreset instance)
        {
            instance.presetName = NAME;
            instance.name = instance.presetName;

            instance.syncDiskNumber = (int)ByTheBook.SyncDisks.ByTheBookSyncEffects.GuardGuestPass;

            instance.price = 500;
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
