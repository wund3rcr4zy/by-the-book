using ByTheBook.Dialog;
using ByTheBook.SyncDisks;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using UnityEngine;

namespace ByTheBook.Upgrades
{
    public class ByTheBookUpgradeManager
    {

        private static ByTheBookUpgradeManager _instance;
        public static ByTheBookUpgradeManager Instance 
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ByTheBookUpgradeManager();
                }

                return _instance;
            }
        }

        public Dictionary<string, SyncDiskPreset> byTheBookSyncDisks = new Dictionary<string, SyncDiskPreset>();

        // preset name, upgrade options
        private Dictionary<string, ImmutableList<ByTheBookSyncEffects>> byTheBookSyncUpgrades = new Dictionary<string, ImmutableList<ByTheBookSyncEffects>>();

        private HashSet<ByTheBookSyncEffects> enabledUpgrades = new HashSet<ByTheBookSyncEffects>();

        public bool TryGetSyncUpgrades(string upgradeName, out ImmutableList<ByTheBookSyncEffects> upgradeEffects)
        {
            return byTheBookSyncUpgrades.TryGetValue(upgradeName, out upgradeEffects);
        }

        public void AddSyncUpgradeEffects(string upgradeName, SyncDiskPreset syncDiskPreset, ImmutableList<ByTheBookSyncEffects> upgradeEffects)
        {
            byTheBookSyncDisks.Add(upgradeName, syncDiskPreset);
            byTheBookSyncUpgrades.Add(upgradeName, upgradeEffects);
        }

        public void EnableEffect(ByTheBookSyncEffects effect)
        {
            enabledUpgrades.Add(effect);

            // TODO: there is certainly a better way of organizing where this code happens.
            // Need it to be somewhere where it happens once
            switch (effect) 
            {
                case ByTheBookSyncEffects.GuardGuestPass:
                    PrivateEyeUpgradeStatusChanged(enabled: true);
                    break;
                case ByTheBookSyncEffects.RelaySeenUnusual:
                    RelaySeenUnusualUpgradeStatusChanged(enabled: true);
                    break;
            }
        }

        private void PrivateEyeUpgradeStatusChanged(bool enabled)
        {
            ByTheBookPlugin.Instance.Log.LogInfo($"PrivateEyeUpgradeStatusChanged to: {enabled}");
            var enforcers = Resources.FindObjectsOfTypeAll<Citizen>()
                .Where(citizen => citizen.isEnforcer)
                .ToList();

            ByTheBookPlugin.Instance.Log.LogInfo($"Found {enforcers.Count} enforcers to enabled dialog.");
            foreach (var enforcer in enforcers)
            {
                if (enabled)
                {
                    enforcer.evidenceEntry?.AddDialogOption(Evidence.DataKey.voice, GuardGuestPassDialogPreset.Instance, newSideJob: null, roomRef: null, allowPresetDuplicates: false);
                }
                else
                {
                    enforcer.evidenceEntry?.RemoveDialogOption(Evidence.DataKey.voice, GuardGuestPassDialogPreset.Instance, newSideJob: null, roomRef: null);
                }
            }    
        }

        private void RelaySeenUnusualUpgradeStatusChanged(bool enabled)
        {
            ByTheBookPlugin.Instance.Log.LogInfo($"RelaySeenUnusualUpgradeStatusChanged to: {enabled}");
            var citizens = Resources.FindObjectsOfTypeAll<Citizen>()
                .ToList();

            ByTheBookPlugin.Instance.Log.LogInfo($"Found {citizens.Count} citizens to add dialog option 'ForceRelaySeenUnusual'.");
            foreach (var citizen in citizens)
            {
                if (enabled)
                {
                    citizen.evidenceEntry?.AddDialogOption((Evidence.DataKey)ByTheBookDataKey.Default, SeekDetectiveDialogPreset.Instance, newSideJob: null, roomRef: null, allowPresetDuplicates: false);
                }
                else
                {
                    citizen.evidenceEntry?.RemoveDialogOption((Evidence.DataKey)ByTheBookDataKey.Default, SeekDetectiveDialogPreset.Instance, newSideJob: null, roomRef: null);
                }
            }
        }

        public void DisableEffect(ByTheBookSyncEffects effect)
        {
            enabledUpgrades.Remove(effect);

            // TODO: there is certainly a better way of organizing where this code happens.
            // Need it to be somewhere where it happens once as the EvidenceWitness is setup and it should be available after load.
            switch (effect)
            {
                case ByTheBookSyncEffects.GuardGuestPass:
                    PrivateEyeUpgradeStatusChanged(enabled: false);
                    break;
                case ByTheBookSyncEffects.RelaySeenUnusual:
                    RelaySeenUnusualUpgradeStatusChanged(enabled: false);
                    break;
            }
        }

        public void DisableAllEffects()
        {
            foreach (var effect in enabledUpgrades) 
            {
                DisableEffect(effect);
            }
        }

        public bool IsEffectEnabled(ByTheBookSyncEffects effect)
        {
            return Game.Instance.giveAllUpgrades || enabledUpgrades.Contains(effect);
        }
    }
}
