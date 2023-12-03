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

        public void EnableUpgrade(ByTheBookSyncEffects effect)
        {
            enabledUpgrades.Add(effect);

            // TODO: there is certainly a better way of organizing where this code happens.
            // Need it to be somewhere where it happens once
            switch (effect) 
            {
                case ByTheBookSyncEffects.PrivateEye:
                    PrivateEyeUpgradeStatusChanged(enabled: true);
                    break;
            }
        }

        private void PrivateEyeUpgradeStatusChanged(bool enabled)
        {
            var enforcers = Resources.FindObjectsOfTypeAll<Citizen>()
                .Where(citizen => citizen.isEnforcer);

            foreach (var enforcer in enforcers)
            { 
            }    
        }

        public void DisableUpgrade(ByTheBookSyncEffects effect)
        {
            enabledUpgrades.Remove(effect);

            // TODO: there is certainly a better way of organizing where this code happens.
            // Need it to be somewhere where it happens once as the EvidenceWitness is setup and it should be available after load.
            switch (effect)
            {
                case ByTheBookSyncEffects.PrivateEye:
                    PrivateEyeUpgradeStatusChanged(enabled: false);
                    break;
            }
        }

        public void DisableAllUpgrades()
        {
            enabledUpgrades.Clear();
        }

        public bool IsUpgradeEnabled(ByTheBookSyncEffects effect)
        {
            return Game.Instance.giveAllUpgrades || enabledUpgrades.Contains(effect);
        }
    }
}
