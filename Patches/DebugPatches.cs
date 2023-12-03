using ByTheBook.Dialog;
using HarmonyLib;
using Il2CppInterop.Runtime;
using System;
using System.Reflection;


namespace ByTheBook.Patches
{
    public class DebugPatches
    {
        public void Test(ref int test)
        { 
        }

        [HarmonyPatch(typeof(EvidenceWitness), nameof(EvidenceWitness.GetDialogOptions), typeof(Evidence.DataKey))]
        public class EvidenceWitnessGetOptionsHook
        {
            private static bool syncDisksLoaded = false;

            [HarmonyPrefix]
            public static void Prefix(EvidenceWitness __instance)
            {
                // TODO: need to find a better place to add this DialogOption. I doubt it should go here.
                __instance.AddDialogOption(Evidence.DataKey.voice, GuardGuestPassDialogPreset.Instance, newSideJob: null, roomRef: null, allowPresetDuplicates: false);

                // TODO: this doesn't seem to work. The runtime invoke seems to depend on the actual object being a DialogController
                //var methodInfo = Il2CppType.Of<ByTheBookDialogManager>().GetMethod(nameof(ByTheBookDialogManager.IssueGuardGuestPass));
                //ByTheBookPlugin.Logger.LogInfo($"Attempting to register GuardGuestPass dialog handling. MethodInfo: {methodInfo?.Name}");
                //DialogController.Instance?.dialogRef?.TryAdd(GuardGuestPassDialogPreset.Instance, methodInfo);
            }
        }
    }
}
