using Il2CppInterop.Runtime.Injection;
using System;
using System.Linq;
using UnityEngine;

namespace ByTheBook.Dialog
{
    public enum ByTheBookDialogSpecialCases
    {
        GuardGuestPass = 100,
        SeekDetective = 101
    }

    public class ByTheBookDialogManager : Il2CppSystem.Object
    {
        public const string DDS_BLOCKS_DICTIONARY = "dds.blocks";

        private static ByTheBookDialogManager __instance;

        public static ByTheBookDialogManager Instance
        { 
            get 
            {
                if (__instance == null)
                {
                    __instance = new ByTheBookDialogManager(ClassInjector.DerivedConstructorPointer<ByTheBookDialogManager>());
                }

                return __instance; 
            } 
        }

        private AIActionPreset __talkTo;
        public  AIActionPreset TalkToAction
        {
            get
            {
                if (__talkTo == null)
                {
                    __talkTo = Resources.FindObjectsOfTypeAll<AIActionPreset>().Where(preset => preset.presetName == "TalkTo").LastOrDefault();
                }
                return __talkTo;
            }
        }

        public ByTheBookDialogManager(IntPtr ptr) : base(ptr)
        {
        }
    }
}
