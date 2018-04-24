using Pipliz.JSON;
using System.Collections.Generic;
using System;
using Pipliz.Threading;
using Pipliz;
using BlockTypes.Builtin;
using System.IO;

namespace Ulfric.ColonyAddOns

{
    /// <summary>
    /// Execution entry points for our mod.
    /// </summary>
    [ModLoader.ModManager]
    public static class Blocks
    {
        // Location of MOD .DLL file at runtime.
        public const string MOD_NAMESPACE = GameLoader.NAMESPACE + ".Blocks";
        private static string VANILLA_PREFIX = "vanilla.";
        private static List<string> crateTypeKeys = new List<string>();

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, Blocks.MOD_NAMESPACE + ".registercallbacks")]
        static void RegisterCallbacks()
        {
            Logger.Log("Register Crates.....");
            foreach (string crate in crateTypeKeys)
            {
                Logger.Log("Register Crate.....{0}", crate);
                ItemTypesServer.RegisterOnAdd(crate, StockpileBlockTracker.Add);
                ItemTypesServer.RegisterOnRemove(crate, StockpileBlockTracker.Remove);
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, Blocks.MOD_NAMESPACE + ".terraingenerator.setdefault"),
            ModLoader.ModCallbackDependsOn("pipliz.server.terraingenerator.setdefault")]
        static void TerrainGenerator()
        {
            Logger.Log("Adding ores......"); 

            Server.TerrainGeneration.ITerrainGenerator iTerrain = Server.TerrainGeneration.TerrainGenerator.UsedGenerator;
            Server.TerrainGeneration.TerrainGeneratorDefault dTerrain = ((Server.TerrainGeneration.TerrainGeneratorDefault)iTerrain);
            dTerrain.AddDepthLayer(35, ItemTypes.IndexLookup.GetIndex(MOD_NAMESPACE + ".infiniteMarble"), (byte)10);
            dTerrain.AddDepthLayer(15, ItemTypes.IndexLookup.GetIndex(MOD_NAMESPACE + ".infiniteWaterSource"), (byte)10);

            Logger.Log("Maximum depth layer...{0}",dTerrain.layersMaxDepth);

            foreach (Server.TerrainGeneration.TerrainGeneratorDefault.DepthLayer dl in ((Server.TerrainGeneration.TerrainGeneratorDefault)iTerrain).layers)
            {
                Logger.Log("Depth..{0}   Block..{1}   Chance..{2}%", dl.depth, ItemTypes.IndexLookup.GetName(dl.block), dl.chance);
            }
        }

        /// <summary>
        /// The afterItemType callback entrypoint. Used for registering jobs and recipes.
        /// </summary>
        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, Blocks.MOD_NAMESPACE + ".AfterItemTypesDefined"),
            ModLoader.ModCallbackProvidesFor("pipliz.apiprovider.jobs.resolvetypes")]
        public static void AfterItemTypesDefined()
        {
            //HelperFunctions hf = new HelperFunctions();
            //hf.DumpItemsToJSON(GameLoader.MODPATH + "/test.json", new List<ushort> { 267, 268, 269, 270, 266 });

            Logger.Log("Loading recipes...");

            foreach (string[] jobAndFilename in new string[][] {
                            new string[] { "pipliz.crafter", "crafting.json"},
                            new string[] { "pipliz.tailor", "tailoring.json" },
                            new string[] { "pipliz.grinder", "grinding.json" },
                            new string[] { "pipliz.minter", "minting.json" },
                            new string[] { "pipliz.merchant", "shopping.json" },
                            new string[] { "pipliz.technologist", "technologist.json" },
                            new string[] { "pipliz.smelter", "smelting.json" },
                            new string[] { "pipliz.baker", "baking.json" },
                            new string[] { "pipliz.stonemason", "stonemasonry.json" },
                            new string[] { "pipliz.metalsmithjob", "metalsmithing.json" },
                            new string[] { "pipliz.kilnjob", "kiln.json" },
                            new string[] { "pipliz.gunsmithjob", "gunsmith.json" },
                            new string[] { "pipliz.fineryforgejob", "fineryforge.json" },
                            new string[] { "pipliz.dyer", "dyeing.json" },
                            new string[] { "pipliz.splittingstump", "splittingstump.json" },
                            new string[] { Jobs.CarpentryRegister.JOB_NAME, "CarpentryWorkbench.json" },
                            new string[] { Jobs.FletcherRegister.JOB_NAME, "FletcheryWorkbench.json" },
                            new string[] { Jobs.AdvancedStoneMasonryRegister.JOB_NAME, "AdvancedStoneMasonryWorkbench.json" },
                            new string[] { Jobs.AdvancedAgricultureRegister.JOB_NAME, "AdvancedAgricultureWorkbench.json" },
                            new string[] {"pipliz.player","player.json" }
                    })
            {
                Recipe craftingRecipe = null;
                try
                {
                    AddRecipe(GameLoader.ConfigFolder, jobAndFilename);

                    //Add recipes in any subdiectories in Config directory
                    foreach(string path in Directory.GetDirectories(GameLoader.ConfigFolder))
                    {
                        AddRecipe(GameLoader.ConfigFolder+ "/" + path, jobAndFilename);
                    }
                }
                catch (Exception exception)
                {
                    if (craftingRecipe != null)
                        Logger.Log("Exception while loading recipes from {0}: {1}. {2} (3)", jobAndFilename[0], jobAndFilename[1], craftingRecipe.ToString(), exception.Message);
                    else
                        Logger.Log("Exception while loading recipes from {0}: {1}. {3}", jobAndFilename[0], jobAndFilename[1], exception.Message);

                }
            }

        }
         
        private static void AddRecipe(string path, string[] jobAndFilename)
        {
            Recipe craftingRecipe = null;
            try
            {
                if (JSON.Deserialize(path + "/" + jobAndFilename[1], out JSONNode jsonRecipes, false))
                {
                    if (jsonRecipes.NodeType == NodeType.Array)
                    {
                        foreach (JSONNode craftingEntry in jsonRecipes.LoopArray())
                        {
                            if (craftingEntry.TryGetAs("name", out string name))
                            {
                                if (name.StartsWith(VANILLA_PREFIX))
                                    name = name.Substring(VANILLA_PREFIX.Length);
                                else
                                    name = MOD_NAMESPACE + "." + name;

                                craftingEntry.SetAs("name", name);

                                foreach (string recipePart in new string[] { "results", "requires" })
                                {
                                    JSONNode jsonRecipeParts = craftingEntry.GetAs<JSONNode>(recipePart);

                                    foreach (JSONNode jsonRecipePart in jsonRecipeParts.LoopArray())
                                    {
                                        string type = jsonRecipePart.GetAs<string>("type");
                                        string realtype;

                                        if (type.StartsWith(VANILLA_PREFIX))
                                            realtype = type.Substring(VANILLA_PREFIX.Length);
                                        else
                                            realtype = MOD_NAMESPACE + "." + type;

                                        //Check to make sure that the items specified in the results and requires are valid items
                                        if (!ItemTypes.IndexLookup.TryGetIndex(realtype, out ushort index))
                                            Logger.Log("ERROR Recipe Name {0}  {1} Type does not exist", name, realtype);

                                        jsonRecipePart.SetAs("type", realtype);
                                    }
                                }

                            }

                            craftingRecipe = new Recipe(craftingEntry);
                            craftingEntry.TryGetAs("isOptional", out bool result);
                            if (jobAndFilename[0] == "pipliz.player")
                            {
                                if (result)
                                {
                                    RecipePlayer.AddOptionalRecipe(craftingRecipe);
                                }
                                else
                                {
                                    RecipePlayer.AddDefaultRecipe(craftingRecipe);
                                }

                            }
                            else
                            {
                                if (result)
                                {
                                    RecipeStorage.AddOptionalLimitTypeRecipe(jobAndFilename[0], craftingRecipe);
                                }
                                else
                                {
                                    RecipeStorage.AddDefaultLimitTypeRecipe(jobAndFilename[0], craftingRecipe);
                                }
                            }
                            Logger.Log("Loading Recipe for " + jobAndFilename[0] + "..." + craftingRecipe.Name);
                        }
                    }
                    else
                    {
                        Logger.Log("Expected json array in {0}, but got {1} instead", jobAndFilename[1], jsonRecipes.NodeType);
                    }
                }
            }
            catch (Exception exception)
            {
                if (craftingRecipe != null)
                    Logger.Log("Exception while loading recipes from {0}: {1}. {2} (3)", jobAndFilename[0], jobAndFilename[1], craftingRecipe.ToString(), exception.Message);
                else
                    Logger.Log("Exception while loading recipes from {0}: {1}. {3}", jobAndFilename[0], jobAndFilename[1], exception.Message);

            }

        }
        /// <summary>
        /// afterAddingBaseTypes callback. Used for adding blocks.
        /// </summary>
        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterAddingBaseTypes, Blocks.MOD_NAMESPACE + ".AfterAddingBaseTypes")]
        [ModLoader.ModCallbackDependsOn("pipliz.blocknpcs.addlittypes")]
        public static void afterAddingBaseTypes(Dictionary<string, ItemTypesServer.ItemTypeRaw> items)
        {
            Logger.Log("Loading Types.....");

            AddTypes(GameLoader.ConfigFolder, items);
            //Add recipes in any subdiectories in Config directory
            Logger.Log("{0}", Directory.GetDirectories(GameLoader.ConfigFolder + "/").Length);
            foreach (string path in Directory.GetDirectories(GameLoader.ConfigFolder))
            {
                Logger.Log("{0}", path);
                AddTypes(path, items);
            }

        }

        private static void AddTypes(string path, Dictionary<string, ItemTypesServer.ItemTypeRaw> items)
        {
            if (JSON.Deserialize(path + "/types.json", out JSONNode jsonTypes, false))
            {
                if (jsonTypes.NodeType == NodeType.Object)
                {
                    foreach (KeyValuePair<string, JSONNode> typeEntry in jsonTypes.LoopObject())
                    {
                        try
                        {
                            if (typeEntry.Value.TryGetAs("icon", out string icon))
                            {
                                string realicon;

                                if (icon.StartsWith(VANILLA_PREFIX))
                                    realicon = "gamedata" + "/" + "textures" + "/" + "icons" + "/" + icon.Substring(VANILLA_PREFIX.Length);
                                else
                                    realicon = GameLoader.IconFolder + "/" + icon;

                                typeEntry.Value.SetAs("icon", realicon);

                            }

                            foreach (string rotatable in new string[] { "rotatablex+", "rotatablex-", "rotatablez+", "rotatablez-" })
                            {
                                if (typeEntry.Value.TryGetAs(rotatable, out string key))
                                {
                                    string rotatablekey;

                                    if (key.StartsWith(VANILLA_PREFIX))
                                        rotatablekey = key.Substring(VANILLA_PREFIX.Length);
                                    else
                                        rotatablekey = Blocks.MOD_NAMESPACE + "." + key;

                                    typeEntry.Value.SetAs(rotatable, rotatablekey);
                                }
                            }

                            if (typeEntry.Value.TryGetAs("parentType", out string parentType))
                            {
                                string realParentType;

                                if (parentType.StartsWith(VANILLA_PREFIX))
                                    realParentType = parentType.Substring(VANILLA_PREFIX.Length);
                                else
                                    realParentType = Blocks.MOD_NAMESPACE + "." + parentType;

                                typeEntry.Value.SetAs("parentType", realParentType);
                            }

                            foreach (string side in new string[] { "sideall", "sidex+", "sidex-", "sidey+", "sidey-", "sidez+", "sidez-" })
                            {
                                if (typeEntry.Value.TryGetAs(side, out string key))
                                {
                                    if (!key.Equals("SELF"))
                                    {
                                        string sidekey;

                                        if (key.StartsWith(VANILLA_PREFIX))
                                            sidekey = key.Substring(VANILLA_PREFIX.Length);
                                        else
                                            sidekey = Blocks.MOD_NAMESPACE + "." + key;

                                        typeEntry.Value.SetAs(side, sidekey);
                                    }
                                }
                            }

                            if (typeEntry.Value.TryGetAs("onRemoveType", out string onRemoveType))
                            {
                                string realOnRemoveType;

                                if (onRemoveType.StartsWith(VANILLA_PREFIX))
                                    realOnRemoveType = onRemoveType.Substring(VANILLA_PREFIX.Length);
                                else
                                    realOnRemoveType = Blocks.MOD_NAMESPACE + "." + onRemoveType;

                                typeEntry.Value.SetAs("onRemoveType", realOnRemoveType);
                            }


                            if (typeEntry.Value.TryGetAs("onPlaceAudio", out string onPlaceAudio))
                            {
                                string realOnPlaceAudio;

                                if (onPlaceAudio.StartsWith(VANILLA_PREFIX))
                                    realOnPlaceAudio = onPlaceAudio.Substring(VANILLA_PREFIX.Length);
                                else
                                    realOnPlaceAudio = Blocks.MOD_NAMESPACE + "." + onPlaceAudio;

                                typeEntry.Value.SetAs("onPlaceAudio", realOnPlaceAudio);
                            }

                            if (typeEntry.Value.TryGetAs("onRemoveAudio", out string onRemoveAudio))
                            {
                                string realOnRemoveAudio;

                                if (onRemoveAudio.StartsWith(VANILLA_PREFIX))
                                    realOnRemoveAudio = onRemoveAudio.Substring(VANILLA_PREFIX.Length);
                                else
                                    realOnRemoveAudio = Blocks.MOD_NAMESPACE + "." + onRemoveAudio;

                                typeEntry.Value.SetAs("onRemoveAudio", realOnRemoveAudio);
                            }

                            if (typeEntry.Value.TryGetAs("mesh", out string meshes))
                            {
                                string realMeshes;

                                if (meshes.StartsWith(VANILLA_PREFIX))
                                    realMeshes = "gamedata" + "/" + "meshes" + "/" + meshes.Substring(VANILLA_PREFIX.Length);
                                else
                                    realMeshes = GameLoader.MeshesFolder + "/" + meshes;

                                typeEntry.Value.SetAs("mesh", realMeshes);
                            }

                            string realkey = Blocks.MOD_NAMESPACE + "." + typeEntry.Key;

                            if (typeEntry.Value.TryGetAs("isCrate", out bool isCrate) && isCrate)
                                crateTypeKeys.Add(realkey);

                            items.Add(realkey, new ItemTypesServer.ItemTypeRaw(realkey, typeEntry.Value));
                            Logger.Log("Loading Type..." + realkey);


                        }
                        catch (Exception exception)
                        {
                            Logger.Log("Exception while loading block type {0}; {1}", typeEntry.Key, exception.Message);
                        }
                    }
                }
                else
                {
                    Logger.Log("Expected json object in {0}, but got {1} instead", "types.json", jsonTypes.NodeType);
                }
            }
        }

        /// <summary>
        /// AfterSelectedWorld callback entry point. Used for adding textures.
        /// </summary>
        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterSelectedWorld, Blocks.MOD_NAMESPACE + ".afterSelectedWorld"),
            ModLoader.ModCallbackProvidesFor("pipliz.server.registertexturemappingtextures")]
        public static void afterSelectedWorld()
        {
            Logger.Log("Loading texture mappings...");

            AddTextureMapping(GameLoader.ConfigFolder);
            //Add recipes in any subdiectories in Config directory
            foreach (string path in Directory.GetDirectories(GameLoader.ConfigFolder))
            {
                AddTextureMapping(GameLoader.ConfigFolder + "/" + path);
            }
        }

        private static void AddTextureMapping(string path)
        {
            if (JSON.Deserialize(path + "/texturemapping.json", out JSONNode jsonTextureMapping, false))
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
                                realkey = MOD_NAMESPACE + "." + textureEntry.Key;
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

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnTryChangeBlock, GameLoader.NAMESPACE + ".AutoLoad.trychangeblock")]
        public static void OnTryChangeBlockUser(ModLoader.OnTryChangeBlockData userData)
        {
            if (userData.CallbackState == ModLoader.OnTryChangeBlockData.ECallbackState.Cancelled)
                return;

            if (userData.TypeNew == BuiltinBlocks.Dirt)
            {
                if (ItemTypes.TryGetType(userData.TypeOld, out ItemTypes.ItemType itemtype)
                    && itemtype.CustomDataNode.TryGetAs<float>("fertilizervalue", out float result))
                {
                    userData.TypeNew = userData.TypeOld;
                }
            }

            if (userData.CallbackOrigin == ModLoader.OnTryChangeBlockData.ECallbackOrigin.ClientPlayerManual)
            {
                VoxelSide side = userData.PlayerClickedData.VoxelSideHit;
                ushort newType = userData.TypeNew;
                string suffix = "bottom";

                switch (side)
                {
                    case VoxelSide.yPlus:
                        suffix = "bottom";
                        break;

                    case VoxelSide.yMin:
                        suffix = "top";
                        break;
                }

                if (newType != userData.TypeOld && ItemTypes.IndexLookup.TryGetName(newType, out string typename))
                {
                    string otherTypename = typename + suffix;

                    if (ItemTypes.IndexLookup.TryGetIndex(otherTypename, out ushort otherIndex))
                    {
                        Vector3Int position = userData.Position;
                        ThreadManager.InvokeOnMainThread(delegate () {
                            ServerManager.TryChangeBlock(position, otherIndex);
                        }, 0.1f);
                    }
                }
            }

        }
    }
}


