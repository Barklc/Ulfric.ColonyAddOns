using System.IO;
using Pipliz.JSON;
using System;
using System.Collections.Generic;

namespace Ulfric.ColonyAddOns
{
    [ModLoader.ModManager]
    public static class GameLoader
    {
        public static string MODPATH;
        public const string NAMESPACE = "Ulfric.ColonyAddOns";
        public const string MOD_NAMESPACE = NAMESPACE + ".GameLoader";
        public static string ConfigFolder = "";
        public static string LocalizationFolder = "";
        public static string TextureFolder = "";
        public static string IconFolder = "";
        public static string AudioFolder = "";
        public static string SavedGameFolder = "";
        public static string MeshesFolder = "";

        public static bool Debug = false;
        public static string DebugFile = "";

        public static ushort Trumpeting_Icon { get; private set; }
        public static ushort Waiting_Icon { get; private set; }

        /// <summary>
        /// OnAssemblyLoaded callback entrypoint. Used for mod configuration / setup.
        /// </summary>
        /// <param name="path">The starting point of our mod file structure.</param>
        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnAssemblyLoaded, GameLoader.MOD_NAMESPACE + ".OnAssemblyLoaded")]
        public static void OnAssemblyLoaded(string path)
        {
            // Get a nicely formatted version of our mod directory.
            MODPATH = System.IO.Path.GetDirectoryName(path).Replace("\\", "/");
            if (File.Exists(MODPATH + "/Debug.txt"))
            {
                Debug = true;
                DebugFile = MODPATH + "/Debug " + DateTime.Now.ToString("MM-dd-yy hh-mm-ss") + ".txt";
            }
            Logger.Log("Mod Path {0}",MODPATH);

            ConfigFolder = GameLoader.MODPATH + "/configs";
            Logger.Log("ConfigFolder Path {0}", ConfigFolder);

            LocalizationFolder = GameLoader.MODPATH + "/localization";
            Logger.Log("LocalizationFolder Path {0}", LocalizationFolder);

            TextureFolder = GameLoader.MODPATH + "/textures";
            Logger.Log("TextureFolder Path {0}", TextureFolder);

            IconFolder = GameLoader.MODPATH + "/icons";
            Logger.Log("IconFolder Path {0}", IconFolder);

            AudioFolder = GameLoader.MODPATH + "/audio";
            Logger.Log("AudioFolder Path {0}", AudioFolder);

            MeshesFolder = GameLoader.MODPATH + "/meshes";
            Logger.Log("MeshesFolder Path {0}", MeshesFolder);

            SavedGameFolder = path.Substring(0, path.IndexOf("gamedata") + "gamedata".Length) + "/savegames/";
            Logger.Log("SavedGameFolder Path {0}", SavedGameFolder);

        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterAddingBaseTypes, NAMESPACE + ".addlittypes")]
        public static void AddLitTypes(Dictionary<string, ItemTypesServer.ItemTypeRaw> items)
        {
            var trumpetingNode = new JSONNode();
            trumpetingNode["icon"] = new JSONNode(IconFolder + "/Trumpeting.png");
            var trumpeting = new ItemTypesServer.ItemTypeRaw(NAMESPACE + ".Trumpeting", trumpetingNode);
            Trumpeting_Icon = trumpeting.ItemIndex;

            items.Add(NAMESPACE + ".Trumpeting", trumpeting);

            var waitingNode = new JSONNode();
            waitingNode["icon"] = new JSONNode(IconFolder + "/Waiting.png");
            var waiting = new ItemTypesServer.ItemTypeRaw(NAMESPACE + ".Waiting", waitingNode);
            Waiting_Icon = waiting.ItemIndex;

            items.Add(NAMESPACE + ".Waiting", waiting);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterWorldLoad, GameLoader.MOD_NAMESPACE + ".Localize")]
        [ModLoader.ModCallbackDependsOn("pipliz.server.localization.waitforloading")]
        [ModLoader.ModCallbackProvidesFor("pipliz.server.localization.convert")]
        public static void Localize()
        {

            Logger.Log("Localization directory: {0}", GameLoader.LocalizationFolder);
            try
            {
                string[] array = new string[]
                {
                    "translation.json"
                };
                for (int i = 0; i < array.Length; i++)
                {
                    string text = array[i];
                    string[] files = Directory.GetFiles(GameLoader.LocalizationFolder, text, SearchOption.AllDirectories);
                    string[] array2 = files;
                    for (int j = 0; j < array2.Length; j++)
                    {
                        string text2 = array2[j];
                        try
                        {
                            JSONNode jsonFromMod;
                            if (JSON.Deserialize(text2, out jsonFromMod, false))
                            {
                                string name = Directory.GetParent(text2).Name;

                                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(text))
                                {
                                    Logger.Log("Found mod localization file for '{0}' localization", name);
                                    localize(name, text, jsonFromMod);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Log("Exception reading localization from {0}; {1}", text2, ex.Message);
                        }
                    }
                }
            }
            catch (DirectoryNotFoundException)
            {
                Logger.Log("Localization directory not found at {0}", GameLoader.LocalizationFolder);
            }
        }

        public static void AddSoundFile(string key, List<string> fileNames)
        {
            var node = new JSONNode();
            node.SetAs("clipCollectionName", key);

            var fileListNode = new JSONNode(NodeType.Array);

            foreach (var fileName in fileNames)
            {
                var audoFileNode = new JSONNode()
                    .SetAs("path", fileName)
                    .SetAs("audioGroup", "Effects");

                fileListNode.AddToArray(audoFileNode);
            }

            node.SetAs("fileList", fileListNode);

            ItemTypesServer.AudioFilesJSON.AddToArray(node);
        }

        public static void localize(string locName, string locFilename, JSONNode jsonFromMod)
        {
            try
            {
                if (Server.Localization.Localization.LoadedTranslation == null)
                {
                    Logger.Log("Unable to localize. Server.Localization.Localization.LoadedTranslation is null.");
                }
                else
                {
                    if (Server.Localization.Localization.LoadedTranslation.TryGetValue(locName, out JSONNode jsn))
                    {
                        if (jsn != null)
                        {
                            foreach (KeyValuePair<string, JSONNode> modNode in jsonFromMod.LoopObject())
                            {
                                Logger.Log("Adding localization for '{0}' from '{1}'.", modNode.Key, Path.Combine(locName, locFilename));
                                AddRecursive(jsn, modNode);
                            }
                        }
                        else
                            Logger.Log("Unable to localize. Localization '{0}' not found and is null.", locName);
                    }
                    else
                        Logger.Log("Localization '{0}' not supported", locName);
                }

                Logger.Log("Patched mod localization file '{0}/{1}'", locName, locFilename);

            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Exception while localizing {0}", Path.Combine(locName, locFilename));
            }
        }

        private static void AddRecursive(JSONNode gameJson, KeyValuePair<string, JSONNode> modNode)
        {
            int childCount = 0;

            try
            {
                childCount = modNode.Value.ChildCount;
            }
            catch { }

            if (childCount != 0)
            {
                if (gameJson.HasChild(modNode.Key))
                {
                    foreach (var child in modNode.Value.LoopObject())
                        AddRecursive(gameJson[modNode.Key], child);
                }
                else
                {
                    gameJson[modNode.Key] = modNode.Value;
                }
            }
            else
            {
                gameJson[modNode.Key] = modNode.Value;
            }
        }

    }
}
