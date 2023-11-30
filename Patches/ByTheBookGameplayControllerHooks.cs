using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using Il2CppSystem;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using static GameplayController;

namespace ByTheBook.Patches
{
    internal class ByTheBookGameplayControllerHooks
    {
        [HarmonyPatch(typeof(GameplayController), nameof(GameplayController.NewMurderCaseNotify))]
        public class NewMurderCaseNotifyHook
        {

            [HarmonyPostfix]
            public static void Postfix(NewGameLocation newLocation)
            {
                if (!Game.Instance.giveAllUpgrades && !ByTheBookPlugin.Instance.IsUpgradeEnabled(SyncDisks.ByTheBookSyncEffects.PrivateEye))
                {
                    ByTheBookPlugin.Logger.LogDebug("Private Eye SyncDisk is not enabled. No GuestPass will be given for Murder notification.");
                    return;
                }
                else if (newLocation.isOutside)
                {
                    ByTheBookPlugin.Logger.LogDebug("Murder Location is Outside. No GuestPass needed.");
                    return;
                }

                GameplayController.Instance.AddGuestPass(newLocation.thisAsAddress, 2);
            }
        }
    }
}
