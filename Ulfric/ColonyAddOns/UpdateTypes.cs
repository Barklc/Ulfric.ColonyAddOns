using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pipliz.JSON;
using BlockTypes.Builtin;
using System.IO;

namespace Ulfric.ColonyAddOns
{
    [ModLoader.ModManager]
    public static class UpdateTypesAndTexture
    {
        public const string MOD_NAMESPACE = GameLoader.NAMESPACE + ".UpdateTypesAndTexture";
        private static string VANILLA_PREFIX = "vanilla.";

        //[ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, MOD_NAMESPACE + ".AfterItemTypesDefined")]
        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnItemTypeRegistered, "OnItemTypeRegistered")]
        [ModLoader.ModCallbackProvidesFor("pipliz.server.itemtypesserver")]
        static void OnItemTypeRegistered(ItemTypes.ItemType type)
        {

            if (JSON.Deserialize(GameLoader.ConfigFolder + "/" + "UpdateTypes.json", out JSONNode jsonTypes, false))
            {
                if (jsonTypes.NodeType == NodeType.Object)
                {
                    if (jsonTypes.TryGetAs(type.Name,out JSONNode typeEntry))
                    {
                        Logger.Log("Updating Exisiting Types.....{0}",type.Name);

                        type = Icon(typeEntry, type);
                        type = Rotatables(typeEntry, type);
                        type = ParentType(typeEntry, type);
                        type = Sides(typeEntry, type);
                        type = OnRemoveType(typeEntry, type);
                        //add OnRemove support
                        //add RemoveAmount
                        ////Add NeedBase
                        ////Add IsFertile
                        ////Add IsPlaceable
                        ////Add IsSolid
                        ////Add maxStackSize
                        ////Add Mesh
                        ////Add DestructionTime
                        ////Add Color
                        ////Add OnRemoveAmount
                        ////Add CustomDataNode
                        //type = OnPlaceAudio(typeEntry, type);
                        //type = OnRemoveAudio(typeEntry, type);

                        Logger.Log("Type {0} was updated.", type.Name);
                    }
                }
            }
        }

        private static string AddNamespace(string field)
        {
            string real;

            if (field.StartsWith(VANILLA_PREFIX))
                real = field.Substring(VANILLA_PREFIX.Length);
            else
                real = Blocks.MOD_NAMESPACE + "." + field;

            return real;
        }

        private static ItemTypes.ItemType Icon(JSONNode typeEntry, ItemTypes.ItemType type)
        {
            if (typeEntry.TryGetAs("icon", out string iconType))
            {

                string real;

                if (iconType.StartsWith(VANILLA_PREFIX))
                    real = "gamedata/textures/icons/" + iconType.Substring(VANILLA_PREFIX.Length);
                else
                    real = GameLoader.IconFolder + "/" + iconType;

                Logger.Log("New icon.....{0}", real);
 
                type.Icon = real;

            }

            return type;
        }

        private static ItemTypes.ItemType Rotatables(JSONNode typeEntry, ItemTypes.ItemType type)
        {
            foreach (string rotatable in new string[] { "rotatablex+", "rotatablex-", "rotatablez+", "rotatablez-" })
            {
                if (typeEntry.TryGetAs(rotatable, out string key))
                {
                    string rotatablekey = AddNamespace(key);

                    switch (rotatable)
                    {
                        case "rotatablex +":
                            type.RotatedXPlus = rotatablekey;
                            break;
                        case "rotatablex -":
                            type.RotatedXMinus = rotatablekey;
                            break;
                        case "rotatablez +":
                            type.RotatedZPlus = rotatablekey;
                            break;
                        case "rotatablez -":
                            type.RotatedZMinus = rotatablekey;
                            break;
                    }
                }
            }
            return type;
        }

        private static ItemTypes.ItemType ParentType(JSONNode typeEntry, ItemTypes.ItemType type)
        {
            if (typeEntry.TryGetAs("parentType", out string parentType))
            {
                type.ParentType = AddNamespace(parentType);
            }

            return type;
        }

        private static ItemTypes.ItemType Sides(JSONNode typeEntry, ItemTypes.ItemType type)
        {
            foreach (string side in new string[] { "sideall", "sidex+", "sidex-", "sidey+", "sidey-", "sidez+", "sidez-" })
            {
                if (typeEntry.TryGetAs(side, out string key))
                {
                    string sidekey = AddNamespace(key);
                    if (!key.Equals("SELF"))
                    {
                        switch (side)
                        {
                            case "sideall":
                                type.SideAll = sidekey;
                                break;
                            case "sidex+":
                                type.SideXPlus = sidekey;
                                break;
                            case "sidex-":
                                type.SideXMinus = sidekey;
                                break;
                            case "sidey+":
                                type.SideYPlus = sidekey;
                                break;
                            case "sidey-":
                                type.SideYMinus = sidekey;
                                break;
                            case "sidez+":
                                type.SideZPlus = sidekey;
                                break;
                            case "sidez-":
                                type.SideZMinus = sidekey;
                                break;
                        }
                    }
                    Logger.Log("New {0}.....{1}", side, sidekey);
                }
            }
            return type;
        }

        private static ItemTypes.ItemType OnRemoveType(JSONNode typeEntry, ItemTypes.ItemType type)
        {
            if (typeEntry.TryGetAs("onRemoveType", out string onRemoveType))
            {
                string realOnRemoveType = AddNamespace(onRemoveType);

                ushort i = ItemTypes.IndexLookup.GetIndex(realOnRemoveType);
                if (i > 0)
                {
                    ItemTypes.ItemTypeDrops itemtypedrop = new ItemTypes.ItemTypeDrops(ItemTypes.IndexLookup.GetIndex(realOnRemoveType), 1, 100);
                    type.OnRemoveItems = new List<ItemTypes.ItemTypeDrops> { itemtypedrop };
                }
                else
                {
                    Logger.Log("{0}.OnRemoveType of {1} not a valid type!", type.Name, realOnRemoveType);
                }

            }

            return type;
        }

        private static ItemTypes.ItemType OnPlaceAudio(JSONNode typeEntry, ItemTypes.ItemType type)
        {
            if (typeEntry.TryGetAs("onPlaceAudio", out string onPlaceAudio))
            {
                type.OnPlaceAudio = AddNamespace(onPlaceAudio);
            }

            return type;
        }

        private static ItemTypes.ItemType OnRemoveAudio(JSONNode typeEntry, ItemTypes.ItemType type)
        {
            if (typeEntry.TryGetAs("onRemoveAudio", out string onRemoveAudio))
            {
                type.OnRemoveAudio = AddNamespace(onRemoveAudio);
            }

            return type;
        }

        /// <summary>
        /// AfterSelectedWorld callback entry point. Used for adding textures.
        /// </summary>
        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterSelectedWorld,MOD_NAMESPACE + ".afterSelectedWorld"),
            ModLoader.ModCallbackProvidesFor("pipliz.server.registertexturemappingtextures")]
        public static void afterSelectedWorld()
        {
            Logger.Log("Update texture mappings...");

            if (JSON.Deserialize(GameLoader.ConfigFolder + "/" + "UpdateTextures.json", out JSONNode jsonTextureMapping, false))
            {
                if (jsonTextureMapping.NodeType == NodeType.Object)
                {
                    foreach (KeyValuePair<string, JSONNode> textureEntry in jsonTextureMapping.LoopObject())
                    {
                        try
                        {
                            string albedoPath = null;
                            string normalPath = null;
                            string emissivePath = null;
                            string heightPath = null;

                            foreach (string textureType in new string[] { "albedo", "normal", "emissive", "height" })
                            {
                                string textureTypeValue = textureEntry.Value.GetAs<string>(textureType);
                                string realTextureTypeValue = textureTypeValue;

                                if (!textureTypeValue.Equals("neutral"))
                                {
                                    if (textureTypeValue.StartsWith(VANILLA_PREFIX))
                                    {
                                        realTextureTypeValue = realTextureTypeValue.Substring(VANILLA_PREFIX.Length);
                                    }
                                    else
                                    {
                                        realTextureTypeValue = GameLoader.TextureFolder + "/" + textureType + "/" + textureTypeValue + ".png";

                                        switch (textureType.ToLowerInvariant())
                                        {
                                            case "albedo":
                                                albedoPath = realTextureTypeValue;
                                                break;
                                            case "normal":
                                                normalPath = realTextureTypeValue;
                                                break;
                                            case "emissive":
                                                emissivePath = realTextureTypeValue;
                                                break;
                                            case "height":
                                                heightPath = realTextureTypeValue;
                                                break;
                                        }
                                    }
                                }
                                textureEntry.Value.SetAs(textureType, realTextureTypeValue);
                            }

                            var textureMapping = new ItemTypesServer.TextureMapping(textureEntry.Value);

                            if (albedoPath != null)
                                textureMapping.AlbedoPath = albedoPath;

                            if (normalPath != null)
                                textureMapping.NormalPath = normalPath;

                            if (emissivePath != null)
                                textureMapping.EmissivePath = emissivePath;

                            if (heightPath != null)
                                textureMapping.HeightPath = heightPath;

                            string realkey;
                            if (!textureEntry.Key.StartsWith(VANILLA_PREFIX))
                            {
                                realkey = Blocks.MOD_NAMESPACE + "." + textureEntry.Key;
                            }
                            else
                                realkey = textureEntry.Key.Substring(VANILLA_PREFIX.Length);

                            ItemTypesServer.SetTextureMapping(realkey, textureMapping);
                            Logger.Log("TextureMapping loaded for..." + realkey);
                        }
                        catch (Exception exception)
                        {
                            Logger.Log("Exception while loading from {0}; {1}", "texturemapping.json", exception.Message);
                        }
                    }
                }
                else
                {
                    Logger.Log("Expected json object in {0}, but got {1} instead", "texturemapping.json", jsonTextureMapping.NodeType);
                }
            }
        }
    }
}
