using ByTheBook.Dialog;
using ByTheBook.SyncDisks;
using HarmonyLib;
using Il2CppInterop.Runtime;
using System.Linq;
using UnityEngine;

namespace ByTheBook.Patches
{
    internal class ToolboxPatches
    {
        [HarmonyPatch(typeof(Toolbox), nameof(Toolbox.LoadAll))]
        public class ToolboxLoadAllHook
        {
            private static bool presetsLoaded = false;

            [HarmonyPostfix]
            public static void Postfix()
            {
                ByTheBookPlugin.Logger.LogDebug($"Toolbox Post LoadAll.");

                if (!presetsLoaded)
                {
                    LoadSyncDisks();
                    LoadDialogs();
                    presetsLoaded = true;
                }
            }

            private static void LoadSyncDisks() 
            {
                var policeVendingMenu = Resources.FindObjectsOfTypeAll<MenuPreset>()
                    .Where(preset => preset.GetPresetName() == "PoliceAutomat");

                Toolbox.Instance.allSyncDisks.Add(PrivateEyeSyncDiskPreset.Instance);

                foreach (var pv in policeVendingMenu)
                {
                    ByTheBookPlugin.Logger.LogInfo($"Attempted to add PrivateEye SyncDisk to WeaponsLocker.");
                    pv.syncDisks.Add(PrivateEyeSyncDiskPreset.Instance);
                }
            }

            private static void LoadDialogs()
            {
                Toolbox.Instance.allDialog.Add(GuardGuestPassDialogPreset.Instance);
            }
        }
    }
}
