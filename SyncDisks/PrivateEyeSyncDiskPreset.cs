using System;
using System.Collections.Generic;
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
        public static SyncDiskPreset Instance { get; private set; }
        public const string NAME = "private-eye";

        public static SyncDiskPreset CreateWarrantSyncDiskPreset()
        {
            if (Instance != null)
            {
                return Instance;
            }

            ByTheBookPlugin.Logger.LogInfo($"BTB: Creating Warrant SyncDisk Preset.");
            SyncDiskPreset privateEyeSyncDiskPreset = ScriptableObject.CreateInstance<SyncDiskPreset>();
           
            privateEyeSyncDiskPreset.presetName = NAME;
            privateEyeSyncDiskPreset.name = privateEyeSyncDiskPreset.presetName;

            privateEyeSyncDiskPreset.syncDiskNumber = (int)ByTheBook.SyncDisks.ByTheBookSyncEffects.PrivateEye;

            privateEyeSyncDiskPreset.price = 1;
            privateEyeSyncDiskPreset.rarity = Rarity.common;

            privateEyeSyncDiskPreset.mainEffect1 = (Effect)ByTheBookSyncEffects.PrivateEye;
            privateEyeSyncDiskPreset.mainEffect1Name = $"{NAME}_effect1_name";
            privateEyeSyncDiskPreset.mainEffect1Description = $"{NAME}_effect1_description";

            privateEyeSyncDiskPreset.mainEffect2 = Effect.none;
            privateEyeSyncDiskPreset.mainEffect3 = Effect.none;

            privateEyeSyncDiskPreset.interactable = Resources.FindObjectsOfTypeAll<InteractablePreset>()
                .Where(preset => preset.presetName == "SyncDisk")
                .LastOrDefault();
         
            privateEyeSyncDiskPreset.canBeSideJobReward = true;

            privateEyeSyncDiskPreset.manufacturer = Manufacturer.Kaizen;

            Instance = privateEyeSyncDiskPreset;
            ByTheBookPlugin.Instance.byTheBookSyncDisks.Add(NAME, Instance);
            return privateEyeSyncDiskPreset;
        }
    }
}
