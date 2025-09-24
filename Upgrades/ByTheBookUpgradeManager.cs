using ByTheBook.Dialog;
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

        // Direct boolean flags toggled by SOD.Common events
        public bool GuardGuestPassEnabled { get; private set; }
        public bool CrimeSceneGuestPassEnabled { get; private set; }
        public bool CrimePursuitSocialCreditEnabled { get; private set; }

        public void SetGuardGuestPass(bool enabled)
        {
            if (GuardGuestPassEnabled == enabled) return;
            GuardGuestPassEnabled = enabled;
            PrivateEyeUpgradeStatusChanged(enabled);
        }

        public void SetCrimeSceneGuestPass(bool enabled)
        {
            CrimeSceneGuestPassEnabled = enabled;
        }

        public void SetCrimePursuitSocialCredit(bool enabled)
        {
            CrimePursuitSocialCreditEnabled = enabled;
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

        public bool IsCrimeSceneGuestPassEnabled() => Game.Instance.giveAllUpgrades || CrimeSceneGuestPassEnabled;
        public bool IsCrimePursuitSocialCreditEnabled() => Game.Instance.giveAllUpgrades || CrimePursuitSocialCreditEnabled;
    }
}
