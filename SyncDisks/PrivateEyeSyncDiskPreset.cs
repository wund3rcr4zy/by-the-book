using ByTheBook.Upgrades;
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
        PrivateEye = 50
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
                    ByTheBookUpgradeManager.Instance.AddSyncUpgradeEffects(NAME, _instance, ImmutableList.Create(ByTheBookSyncEffects.PrivateEye));
                }

                return _instance;
            }
        }

        // Seems like constructor work, but I don't think I can trust ScriptableObject.CreateInstance with a constructor.
        private static void Init(PrivateEyeSyncDiskPreset instance)
        {
            instance.presetName = NAME;
            instance.name = instance.presetName;

            instance.syncDiskNumber = (int)ByTheBook.SyncDisks.ByTheBookSyncEffects.PrivateEye;

            instance.price = 500;
            instance.rarity = Rarity.medium;

            instance.mainEffect1 = (Effect)ByTheBookSyncEffects.PrivateEye;
            instance.mainEffect1Name = $"{NAME}_effect1_name";
            instance.mainEffect1Description = $"{NAME}_effect1_description";

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
