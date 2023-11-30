using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using ByTheBook.SyncDisks;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ByTheBook
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class ByTheBookPlugin : BasePlugin
    {
        public static ByTheBookPlugin Instance { get; private set; }
        public static ManualLogSource Logger;

        // preset name, preset
        public Dictionary<string, SyncDiskPreset> byTheBookSyncDisks = new Dictionary<string, SyncDiskPreset>();

        private HashSet<ByTheBookSyncEffects> enabledUpgrades = new HashSet<ByTheBookSyncEffects>();

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

            PerformHarmonyPatches();

            RegisterTypes();
        }

        private void PerformHarmonyPatches()
        {
            var harmony = new Harmony($"{MyPluginInfo.PLUGIN_GUID}");
            harmony.PatchAll();
            Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is patched!");
        }

        private void RegisterTypes()
        {
            ClassInjector.RegisterTypeInIl2Cpp<PrivateEyeSyncDiskPreset>();
            Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} has added custom types!");
        }

        public void EnableUpgrade(ByTheBookSyncEffects effect)
        {
            enabledUpgrades.Add(effect);
        }

        public void DisableUpgrade(ByTheBookSyncEffects effect)
        {
            enabledUpgrades.Remove(effect);
        }

        public bool IsUpgradeEnabled(ByTheBookSyncEffects effect) 
        {
            return enabledUpgrades.Contains(effect);
        }
    }
}
