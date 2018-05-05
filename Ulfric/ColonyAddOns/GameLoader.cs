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
        public static string StructuresFolder = "";

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

            StructuresFolder = path.Substring(0, path.IndexOf("gamedata") + "gamedata".Length) + "/structures/";
            Logger.Log("StrucutresFolder Path {0}", StructuresFolder);

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


    }
}
