using ByTheBook.AIActions;
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
        [HarmonyPatch(typeof(AssetLoader), nameof(AssetLoader.GetAllActions))]
        public class AssetLoaderGetAllActionsHook()
        {
            private static bool presetsLoaded = false;

            [HarmonyPostfix]
            public static void Postfix(List<AIActionPreset> __result)
            {
                ByTheBookPlugin.Logger.LogDebug($"AssetLoaderGetAllActionsHook Post.");

                if (!presetsLoaded)
                {
                    LoadActions(__result);
                    presetsLoaded = true;
                }
            }

            private static void LoadActions(List<AIActionPreset> data)
            {
                data.Add(SeekOutDetectiveAction.Instance);
            }
        }

        [HarmonyPatch(typeof(AssetLoader), nameof(AssetLoader.GetAllPresets))]
        public class AssetLoaderGetAllPresetsHook()
        {
            private static bool presetsLoaded = false;

            [HarmonyPostfix]
            public static void Postfix(List<ScriptableObject> __result)
            {
                ByTheBookPlugin.Logger.LogDebug($"AssetLoaderGetAllPresetsHook Post.");

                if (!presetsLoaded)
                {
                    LoadSyncDisks(__result);
                    LoadDialogs(__result);
                    
                    // TODO: Still figuring out how to modify the AI and change goals.
                    //LoadGoals(__result);
                    
                    presetsLoaded = true;
                }
            }

            // SOD.Common handles registering custom SyncDisks; no manual injection needed.
            private static void LoadSyncDisks(List<ScriptableObject> data) { }

            private static void LoadDialogs(List<ScriptableObject> data)
            {
                data.Add(GuardGuestPassDialogPreset.Instance);
            }

            private static void LoadGoals(List<ScriptableObject> data)
            {
                data.Add(SeekOutDetectiveGoal.Instance);
            }
        }


        [HarmonyPatch(typeof(Toolbox), nameof(Toolbox.LoadAll))]
        public class ToolboxLoadAllHook
        {
            private static bool presetsLoaded = false;

            [HarmonyPostfix]
            [HarmonyPriority(Priority.Last)]
            public static void Postfix()
            {
                ByTheBookPlugin.Logger.LogDebug($"Toolbox Post LoadAll.");

                // Ensure legacy DDS keys are present once DDS is initialized
                if (!presetsLoaded) { PrivateEyeDisk.AddLegacyDdsStrings(); presetsLoaded = true; }

                // Always dedupe on each load to counter repeated injections by other patches
                DeduplicatePoliceAutomatSyncDisks();
            }

            // Remove duplicate sync disk entries for the PoliceAutomat menu
            private static void DeduplicatePoliceAutomatSyncDisks()
            {
                var policeVendingMenus = Resources.FindObjectsOfTypeAll<MenuPreset>()
                    .Where(preset => preset.GetPresetName() == "PoliceAutomat")
                    .ToList();

                foreach (var menu in policeVendingMenus)
                {
                    try
                    {
                        var seen = new System.Collections.Generic.HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
                        var unique = new Il2CppSystem.Collections.Generic.List<SyncDiskPreset>();

                        foreach (var disk in menu.syncDisks)
                        {
                            var key = disk?.name ?? disk?.presetName;
                            if (key == null) continue;
                            if (seen.Add(key))
                                unique.Add(disk);
                        }

                        if (unique.Count != menu.syncDisks.Count)
                        {
                            menu.syncDisks.Clear();
                            foreach (var disk in unique)
                                menu.syncDisks.Add(disk);
                            ByTheBookPlugin.Logger.LogInfo($"Deduplicated PoliceAutomat sync disks: {unique.Count} unique entries.");
                        }
                    }
                    catch { /* Best-effort cleanup */ }
                }
            }
        }
    }
}
