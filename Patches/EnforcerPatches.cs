using ByTheBook.Dialog;
using ByTheBook.Upgrades;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByTheBook.Patches
{
    internal class EnforcerPatches
    {
        [HarmonyPatch(typeof(ActionController), nameof(ActionController.CallEnforcers))]
        [HarmonyPatch(typeof(GameplayController), nameof(GameplayController.CallEnforcers))]
        public class CallEnforcersHook
        {
            // Required because the dialog option is removed from an enforcer if tried at a previous crime scene.
            // This will add the dialog option back to the guard when a new call comes in.
            [HarmonyPostfix]
            public static void Postfix()
            {
                if (!ByTheBookUpgradeManager.Instance.IsUpgradeEnabled(SyncDisks.ByTheBookSyncEffects.GuardGuestPass))
                {
                    return;
                }

                foreach (var enforcerCall in GameplayController.Instance.enforcerCalls.Values)
                {
                    foreach (var enforcer in GameplayController.Instance.enforcers)
                    {
                        if (enforcer.humanID == enforcerCall.guard)
                        {
                            enforcer.evidenceEntry?.AddDialogOption(Evidence.DataKey.voice, GuardGuestPassDialogPreset.Instance, newSideJob: null, roomRef: null, allowPresetDuplicates: false);
                            break;
                        }
                    }
                }
            }
        }
    }
}
