﻿using HarmonyLib;
using JsonDiffPatch;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ByTheBook.Patches
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// This code is adapted from piepieonline's DDSLoader mod. Credit goes to them and the open licensing that allows for use and modification of the code.
    /// <see cref="https://github.com/piepieonline/ShadowsOfDoubtMods/tree/master/DDSLoader"/>
    /// </remarks>
    internal class DDSLoaderPatches
    {
        [HarmonyPatch(typeof(Toolbox), nameof(Toolbox.Start))]
        public class Toolbox_Start
        {
            private static List<DirectoryInfo> directoryInfos;
            private static List<DirectoryInfo> DdsDirectoryInfo
            {
                get
                {
                    if (directoryInfos == null)
                    {
                        directoryInfos = Directory.GetDirectories(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), ".."), "BTBDDS", SearchOption.AllDirectories)
                            .Select(dirPath => new DirectoryInfo(dirPath))
                            .ToList();
                    }
                    return directoryInfos;
                }
            }

            public static void Postfix()
            {
                foreach (var dir in DdsDirectoryInfo)
                {
                    var treesPath = Path.Combine(dir.FullName, "DDS", "Trees");
                    var messagesPath = Path.Combine(dir.FullName, "DDS", "Messages");
                    var blocksPath = Path.Combine(dir.FullName, "DDS", "Blocks");

                    if (Directory.Exists(blocksPath))
                    {
                        foreach (var blockPath in Directory.GetFiles(blocksPath, "*.block"))
                        {
                            try
                            {
                                var block = JsonUtility.FromJson<DDSSaveClasses.DDSBlockSave>(File.ReadAllText(blockPath));
                                Toolbox.Instance.allDDSBlocks.Add(block.id, block);
                            }
                            catch (Exception exception)
                            {
                                ByTheBookPlugin.Instance.Log.LogError($"Failed to load: {blockPath}");
                                ByTheBookPlugin.Instance.Log.LogError(exception);
                            }
                        }

                        foreach (var blockPath in Directory.GetFiles(blocksPath, "*.block_patch"))
                        {
                            try
                            {
                                var patchedBlock = JsonUtility.FromJson<DDSSaveClasses.DDSBlockSave>(CreatePatchedJson(blockPath));
                                Toolbox.Instance.allDDSBlocks[patchedBlock.id] = patchedBlock;
                            }
                            catch (Exception exception)
                            {
                                ByTheBookPlugin.Instance.Log.LogError($"Failed to load: {blockPath}");
                                ByTheBookPlugin.Instance.Log.LogError(exception);
                            }
                        }
                    }

                    if (Directory.Exists(messagesPath))
                    {
                        foreach (var messagePath in Directory.GetFiles(messagesPath, "*.msg"))
                        {
                            try
                            {
                                var message = JsonUtility.FromJson<DDSSaveClasses.DDSMessageSave>(File.ReadAllText(messagePath));
                                Toolbox.Instance.allDDSMessages.Add(message.id, message);
                            }
                            catch (Exception exception)
                            {
                                ByTheBookPlugin.Instance.Log.LogError($"Failed to load: {messagePath}");
                                ByTheBookPlugin.Instance.Log.LogError(exception);
                            }
                        }

                        foreach (var messagePath in Directory.GetFiles(messagesPath, "*.msg_patch"))
                        {
                            try
                            {
                                var patchedMessage = JsonUtility.FromJson<DDSSaveClasses.DDSMessageSave>(CreatePatchedJson(messagePath));
                                Toolbox.Instance.allDDSMessages[patchedMessage.id] = patchedMessage;
                            }
                            catch (Exception exception)
                            {
                                ByTheBookPlugin.Instance.Log.LogError($"Failed to load: {messagePath}");
                                ByTheBookPlugin.Instance.Log.LogError(exception);
                            }
                        }
                    }

                    if (Directory.Exists(treesPath))
                    {
                        foreach (var treePath in Directory.GetFiles(treesPath, "*.tree"))
                        {
                            try
                            {
                                var tree = JsonUtility.FromJson<DDSSaveClasses.DDSTreeSave>(File.ReadAllText(treePath));
                                tree.messageRef = new Il2CppSystem.Collections.Generic.Dictionary<string, DDSSaveClasses.DDSMessageSettings>();

                                foreach (var msg in tree.messages)
                                {
                                    tree.messageRef.Add(msg.instanceID, msg);
                                }

                                Toolbox.Instance.allDDSTrees.Add(tree.id, tree);
                            }
                            catch (Exception exception)
                            {
                                ByTheBookPlugin.Instance.Log.LogError($"Failed to load: {treePath}");
                                ByTheBookPlugin.Instance.Log.LogError(exception);
                            }
                        }

                        foreach (var treePath in Directory.GetFiles(treesPath, "*.tree_patch"))
                        {
                            try
                            {
                                var patchedTree = JsonUtility.FromJson<DDSSaveClasses.DDSTreeSave>(CreatePatchedJson(treePath));
                                patchedTree.messageRef = new Il2CppSystem.Collections.Generic.Dictionary<string, DDSSaveClasses.DDSMessageSettings>();

                                foreach (var msg in patchedTree.messages)
                                {
                                    patchedTree.messageRef.Add(msg.instanceID, msg);
                                }

                                Toolbox.Instance.allDDSTrees[patchedTree.id] = patchedTree;
                            }
                            catch (Exception exception)
                            {
                                ByTheBookPlugin.Instance.Log.LogError($"Failed to load: {treePath}");
                                ByTheBookPlugin.Instance.Log.LogError(exception);
                            }
                        }
                    }

                    ByTheBookPlugin.Instance.Log.LogInfo($"Loaded DDS Content and Patches For: {dir.Parent.Name}");

                    var selectedLanguagePath = Path.Combine(dir.FullName, "Strings", Game.Instance.language);
                    var englishLanguagePath = Path.Combine(dir.FullName, "Strings", "English");

                    var stringsPath = Directory.Exists(selectedLanguagePath) ? selectedLanguagePath : Directory.Exists(englishLanguagePath) ? englishLanguagePath : "";
                    if (stringsPath != "")
                    {
                        foreach (var stringFile in Directory.GetFiles(stringsPath, "*.csv", SearchOption.AllDirectories))
                        {
                            var fileName = Path.GetFileNameWithoutExtension(stringFile);
                            foreach (var line in File.ReadAllLines(stringFile))
                            {
                                Strings.ParseLine(line.Trim(), out var key, out var notes, out var display, out var alt, out var freq, out var suffix, out var misc);
                                Strings.LoadIntoDictionary(fileName, Strings.stringTable[fileName].Count + 1, key, display.Replace("\\r\\n", "\r\n"), alt, freq, suffix);
                            }
                        }

                        ByTheBookPlugin.Instance.Log.LogInfo($"Loaded String Content For: {dir.Parent.Name}");
                    }
                }
            }

            static string CreatePatchedJson(string patchPath)
            {
                var patchFileInfo = new FileInfo(patchPath);
                var patchDirInfo = new DirectoryInfo(patchFileInfo.DirectoryName);

                JToken existingDDSContent = Newtonsoft.Json.Linq.JToken.Parse(File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "DDS", patchDirInfo.Name, patchFileInfo.Name.Split("_")[0])));
                PatchDocument patchDDSContent = PatchDocument.Parse(File.ReadAllText(patchPath));

                JsonPatcher patcher = new JsonPatcher();
                patcher.Patch(ref existingDDSContent, patchDDSContent);

                return existingDDSContent.ToString();
            }
        }
    }
}
