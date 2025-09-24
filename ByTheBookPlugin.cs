using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using ByTheBook.Dialog;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using SOD.Common; // for Lib
using SOD.Common.Helpers;
using ByTheBook.SyncDisks;
using System.Linq;
using System.Reflection;

namespace ByTheBook
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class ByTheBookPlugin : BasePlugin
    {
        // Normally, I'd advise against using the Singleton pattern so heavily in favor of more decoupled event driven architecture,
        // but doing so is following the paradigm of the game. Many classes have an 'Instance' singleton in the code as it was designed for 
        // single player only.
        public static ByTheBookPlugin Instance { get; private set; }

        public static ManualLogSource Logger;

        public override void Load()
        {
            if (!Config.Bind("General", "Enabled", true).Value)
            {
                Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is disabled.");
                return;
            }

            Instance = this;
            Logger = Log;

            Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
            
            RegisterTypes();

            // Migrate old config keys once (0.3.0 migration) and clean stale entries
            int? legacyPrice;
            int? legacyGuardTier;
            TryMigrateLegacyConfig(out legacyPrice, out legacyGuardTier);

            // Pre-bind config so options appear immediately in the config file
            // Guard pass success scaling target (1-8)
            Config.Bind(
                "SyncDisk",
                "guard-pass-max-chance-social-credit-level",
                legacyGuardTier ?? 3,
                "Scales guest-pass success chance against the game's social credit thresholds. Higher tier (1-8) = stricter (lower chance)."
            );

            // Disk price (migrated from SyncDiskCosts.private-eye)
            Config.Bind(
                "SyncDisk",
                "private-eye-cost",
                legacyPrice ?? 500,
                "Purchase price for the Private Eye License sync disk."
            );

            // Pursuit social credit penalty knobs
            Config.Bind(
                "EnabledSideEffects",
                "social-credit-penalty-divisor",
                150,
                "Divisor used to scale pursuit social credit penalty: deduction = total fines / divisor."
            );
            Config.Bind(
                "EnabledSideEffects",
                "social-credit-penalty-cap",
                100,
                "Maximum social credit deducted once at the start of a pursuit episode."
            );

            // Register custom sync disks via SOD.Common
            PrivateEyeDisk.Register();

            PerformHarmonyPatches();
        }

        // One-time migration from legacy keys to 0.3.0 keys with file cleanup
        private void TryMigrateLegacyConfig(out int? legacyPrice, out int? legacyGuardTier)
        {
            legacyPrice = null;
            legacyGuardTier = null;

            try
            {
                var cfgPath = Config.ConfigFilePath;
                if (string.IsNullOrWhiteSpace(cfgPath) || !System.IO.File.Exists(cfgPath)) return;

                // Side-file marker under Savestore to ensure one-time migration
                var markerPath = Lib.SaveGame.GetSavestoreDirectoryPath(Assembly.GetExecutingAssembly(), "btb_migrated_0_3_0");
                if (System.IO.File.Exists(markerPath)) return;

                var lines = System.IO.File.ReadAllLines(cfgPath).ToList();

                // Simple parser to capture legacy values and remove legacy entries
                string currentSection = null;
                var toRemove = new System.Collections.Generic.HashSet<int>();

                for (int i = 0; i < lines.Count; i++)
                {
                    var raw = lines[i];
                    var line = raw.Trim();
                    if (line.StartsWith("[") && line.EndsWith("]"))
                    {
                        currentSection = line.Substring(1, line.Length - 2);
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#") || currentSection == null)
                        continue;

                    // Legacy price
                    if (string.Equals(currentSection, "SyncDiskCosts", System.StringComparison.OrdinalIgnoreCase))
                    {
                        if (line.StartsWith("private-eye", System.StringComparison.OrdinalIgnoreCase))
                        {
                            var idx = raw.IndexOf('=');
                            if (idx > -1)
                            {
                                var valStr = raw.Substring(idx + 1).Trim();
                                if (int.TryParse(valStr, out var val)) legacyPrice = val;
                            }
                        }
                    }

                    // Legacy guard tier
                    if (string.Equals(currentSection, "SyncDisks", System.StringComparison.OrdinalIgnoreCase))
                    {
                        if (line.StartsWith("guard-pass-max-chance-social-credit-level", System.StringComparison.OrdinalIgnoreCase))
                        {
                            var idx = raw.IndexOf('=');
                            if (idx > -1)
                            {
                                var valStr = raw.Substring(idx + 1).Trim();
                                if (int.TryParse(valStr, out var val)) legacyGuardTier = val;
                            }
                        }
                    }
                }

                // Remove entire [SyncDiskCosts] section and the single legacy key in [SyncDisks]
                // Re-scan to mark lines to remove
                currentSection = null;
                bool removeSyncDiskCosts = false;
                for (int i = 0; i < lines.Count; i++)
                {
                    var raw = lines[i];
                    var line = raw.Trim();
                    if (line.StartsWith("[") && line.EndsWith("]"))
                    {
                        currentSection = line.Substring(1, line.Length - 2);
                        removeSyncDiskCosts = string.Equals(currentSection, "SyncDiskCosts", System.StringComparison.OrdinalIgnoreCase);
                        if (removeSyncDiskCosts) toRemove.Add(i); // remove header
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(line) || currentSection == null) continue;

                    if (removeSyncDiskCosts)
                    {
                        // remove all lines until next section header
                        toRemove.Add(i);
                        continue;
                    }

                    if (string.Equals(currentSection, "SyncDisks", System.StringComparison.OrdinalIgnoreCase))
                    {
                        if (line.StartsWith("guard-pass-max-chance-social-credit-level", System.StringComparison.OrdinalIgnoreCase))
                            toRemove.Add(i);
                    }
                }

                if (toRemove.Count > 0)
                {
                    var kept = new System.Collections.Generic.List<string>(lines.Count - toRemove.Count);
                    for (int i = 0; i < lines.Count; i++)
                        if (!toRemove.Contains(i)) kept.Add(lines[i]);

                    System.IO.File.WriteAllLines(cfgPath, kept);
                }

                // Write side-file marker to avoid repeating
                System.IO.File.WriteAllText(markerPath, "by-the-book migration 0.3.0 completed");
            }
            catch (System.Exception ex)
            {
                Log.LogWarning($"Config migration skipped due to error: {ex.Message}");
            }
        }

        private void PerformHarmonyPatches()
        {
            var harmony = new Harmony($"{MyPluginInfo.PLUGIN_GUID}");
            harmony.PatchAll();
            Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is patched!");
        }

        private void RegisterTypes()
        {
            ClassInjector.RegisterTypeInIl2Cpp<ByTheBookDialogManager>();
            Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} has added custom types!");
        }
    }
}
