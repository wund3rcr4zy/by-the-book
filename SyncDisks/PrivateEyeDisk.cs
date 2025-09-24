using BepInEx.Configuration;
using SOD.Common;
using SOD.Common.Helpers;
using SOD.Common.Helpers.SyncDiskObjects;
using ByTheBook.Upgrades;

namespace ByTheBook.SyncDisks
{
    internal static class PrivateEyeDisk
    {
        private const string DiskDisplayName = "Private Eye License";
        private const string Effect1Name = "Detective Consultant";
        private const string Effect1Description = "The Enforcer on duty at a crime scene may be asked for a guest pass after they have canvased the scene. Higher social credit increases odds of success.";
        private const string Upgrade1Description = "You will always receive a guest pass (if required) when entering a crime scene.";
        private const string SideEffectDescription = "You're on the side of the law now. If caught (persued) doing illegal activities your social credit score will be reduced based on the size of your current fine.";

        private static int _effectId_GuardGuestPass;
        private static int _sideEffectId_SocialCreditPenalty;
        private static SyncDiskBuilder.OptionIds _optionIds;
        private static SyncDisk _syncDisk;

        private static ConfigEntry<int> _priceConfig;
        private static ConfigEntry<bool> _enableSideEffectConfig;

        public static void Register()
        {
            // Config
            _priceConfig = ByTheBookPlugin.Instance.Config.Bind(
                "SyncDisk",
                "private-eye-cost",
                500,
                "Purchase price for the Private Eye License sync disk."
            );
            _enableSideEffectConfig = ByTheBookPlugin.Instance.Config.Bind(
                "EnabledSideEffects",
                "private-eye",
                true,
                "Enable side effect: social credit penalty when pursued while fines are active."
            );

            // Build the disk via SOD.Common
            var builder = Lib.SyncDisks
                .Builder(DiskDisplayName, MyPluginInfo.PLUGIN_GUID, reRaiseEventsOnSaveLoad: true)
                .SetPrice(_priceConfig.Value)
                .SetManufacturer(SyncDiskPreset.Manufacturer.Kaizen)
                .SetRarity(SyncDiskPreset.Rarity.medium)
                .AddSaleLocation(SyncDiskBuilder.SyncDiskSaleLocation.PoliceAutomat)
                .AddEffect(Effect1Name, Effect1Description, out _effectId_GuardGuestPass)
                .AddUpgradeOption(new SyncDiskBuilder.Options(Upgrade1Description), out _optionIds);

            if (_enableSideEffectConfig.Value)
            {
                builder.AddSideEffect("Crime pursuit social credit penalty", SideEffectDescription, out _sideEffectId_SocialCreditPenalty);
            }

            _syncDisk = builder.CreateAndRegister();

            // Wire SOD.Common SyncDisk events
            Lib.SyncDisks.OnAfterSyncDiskInstalled += OnAfterSyncDiskInstalled;
            Lib.SyncDisks.OnAfterSyncDiskUpgraded += OnAfterSyncDiskUpgraded;
            Lib.SyncDisks.OnAfterSyncDiskUninstalled += OnAfterSyncDiskUninstalled;
        }

        private static bool _legacyDdsAdded = false;
        internal static void AddLegacyDdsStrings()
        {
            if (_legacyDdsAdded) return;
            // Preserve legacy DDS keys alongside builder-provided texts; must run after DDS is initialized
            Lib.DdsStrings.AddOrUpdateEntries("evidence.syncdisks",
                ("private-eye", DiskDisplayName),
                ("private-eye_effect1_name", Effect1Name),
                ("private-eye_effect1_description", Effect1Description),
                ("private-eye_upgrade1_description", Upgrade1Description),
                ("private-eye_side-effect_description", SideEffectDescription)
            );
            _legacyDdsAdded = true;
        }

        private static bool IsOurDisk(SyncDisk disk) => disk != null && disk.Name == DiskDisplayName;

        private static void OnAfterSyncDiskInstalled(object sender, SyncDiskArgs e)
        {
            if (!IsOurDisk(e.SyncDisk)) return;

            // Enable main effect: Guard guest pass dialog access
            if (e.Effect.HasValue && e.Effect.Value.Id == _effectId_GuardGuestPass)
            {
                ByTheBookUpgradeManager.Instance.SetGuardGuestPass(true);
            }

            // If we created the side effect, enable it for the duration of installation
            if (_enableSideEffectConfig.Value)
            {
                ByTheBookUpgradeManager.Instance.SetCrimePursuitSocialCredit(true);
            }
        }

        private static void OnAfterSyncDiskUpgraded(object sender, SyncDiskArgs e)
        {
            if (!IsOurDisk(e.SyncDisk)) return;

            if (e.UpgradeOption.HasValue)
            {
                // Only one upgrade option set; any selection enables the crime scene guest pass behavior
                var id = e.UpgradeOption.Value.Id;
                if (id == _optionIds.Option1Id || id == _optionIds.Option2Id || id == _optionIds.Option3Id)
                {
                    ByTheBookUpgradeManager.Instance.SetCrimeSceneGuestPass(true);
                }
            }
        }

        private static void OnAfterSyncDiskUninstalled(object sender, SyncDiskArgs e)
        {
            if (!IsOurDisk(e.SyncDisk)) return;

            // Disable all related effects
            ByTheBookUpgradeManager.Instance.SetGuardGuestPass(false);
            ByTheBookUpgradeManager.Instance.SetCrimeSceneGuestPass(false);
            if (_enableSideEffectConfig.Value)
            {
                ByTheBookUpgradeManager.Instance.SetCrimePursuitSocialCredit(false);
            }
        }
    }
}
