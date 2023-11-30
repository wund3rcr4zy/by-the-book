using ByTheBook.SyncDisks;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ByTheBook.Patches
{
    internal class ToolboxPatches
    {
        [HarmonyPatch(typeof(Toolbox), nameof(Toolbox.LoadAll))]
        public class ToolboxLoadAllHook
        {
            public static SyncDiskPreset diskPreset;

            [HarmonyPostfix]
            public static void Postfix()
            {
                ByTheBookPlugin.Logger.LogInfo($"BTB: Toolbox Post LoadAll.");

                if (diskPreset == null)
                {
                    var policeVendingMenu = Resources.FindObjectsOfTypeAll<MenuPreset>()
                        .Where(preset => preset.GetPresetName() == "PoliceAutomat");


                    diskPreset = PrivateEyeSyncDiskPreset.CreateWarrantSyncDiskPreset();
                    Toolbox.Instance.allSyncDisks.Add(diskPreset);

                    if (diskPreset != null)
                    {
                        foreach (var pv in policeVendingMenu)
                        {
                            ByTheBookPlugin.Logger.LogInfo($"BTB: Attempted to add WarrantSync Disk to machine.");
                            pv.syncDisks.Add(diskPreset);
                        }
                    }
                }
            }
        }
    }
}
