using Il2CppInterop.Runtime.Injection;
using System;
using System.Linq;
using UnityEngine;

namespace ByTheBook.Dialog
{
    public enum ByTheBookDataKey
    {
        Default = 100
    }

    public enum ByTheBookDialogSpecialCases
    {
        GuardGuestPass = 100,
        SeekDetective = 101
    }

    public class ByTheBookDialogManager : Il2CppSystem.Object
    {
        public const string DDS_BLOCKS_DICTIONARY = "dds.blocks";
        public const string SEEK_DETECTIVE_BLOCK_ID = "44cab549-03ba-459b-845c-3fe93b797ec6";

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
                    __talkTo = Resources.FindObjectsOfTypeAll<AIActionPreset>().Where(preset => preset.presetName == "TalkTo").FirstOrDefault();
                }
                return __talkTo;
            }
        }

        private DialogPreset __seenUnusual;
        public DialogPreset SeenOrHeardUnusualDialog
        {
            get
            {
                if (__seenUnusual == null)
                {
                    __seenUnusual = Resources.FindObjectsOfTypeAll<DialogPreset>().Where(preset => preset.presetName == "SeenOrHeardUnusual").FirstOrDefault();
                }
                return __seenUnusual;
            }
        }

        public ByTheBookDialogManager(IntPtr ptr) : base(ptr)
        {
        }
    }
}
