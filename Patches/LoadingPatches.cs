using ByTheBook.Dialog;
using ByTheBook.SyncDisks;
using HarmonyLib;
using Il2CppInterop.Runtime;
using Il2CppSystem.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ByTheBook.Patches
{
    internal class LoadingPatches
    {

        [HarmonyPatch(typeof(AssetLoader), nameof(AssetLoader.GetAllPresets))]
        public class AssetLoaderGetAllPresetsHook()
        {
            private static bool presetsLoaded = false;

            [HarmonyPostfix]
            public static void Postfix(List<ScriptableObject> __result)
            {
                ByTheBookPlugin.Logger.LogDebug($"AssetLoaderGetAllPresetsHook Post LoadAll.");

                if (!presetsLoaded)
                {
                    LoadSyncDisks(__result);
                    LoadDialogs(__result);
                    presetsLoaded = true;
                }
            }

            private static void LoadSyncDisks(List<ScriptableObject> data)
            {
                data.Add(PrivateEyeSyncDiskPreset.Instance);
            }

            private static void LoadDialogs(List<ScriptableObject> data)
            {
                data.Add(GuardGuestPassDialogPreset.Instance);
            }
        }


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
                    AddPrivateEyeSyncDiskToWeaponsLocker();
                    presetsLoaded = true;
                }
            }

            private static void AddPrivateEyeSyncDiskToWeaponsLocker() 
            {
                var policeVendingMenu = Resources.FindObjectsOfTypeAll<MenuPreset>()
                    .Where(preset => preset.GetPresetName() == "PoliceAutomat");

                foreach (var pv in policeVendingMenu)
                {
                    ByTheBookPlugin.Logger.LogInfo($"Attempted to add PrivateEye SyncDisk to WeaponsLocker.");
                    pv.syncDisks.Add(PrivateEyeSyncDiskPreset.Instance);
                }
            }
        }
    }
}
