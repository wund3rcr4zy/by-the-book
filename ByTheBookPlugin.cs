using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using ByTheBook.Dialog;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using SOD.Common.Helpers;
using ByTheBook.SyncDisks;

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

            // Register custom sync disks via SOD.Common
            PrivateEyeDisk.Register();

            PerformHarmonyPatches();
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
