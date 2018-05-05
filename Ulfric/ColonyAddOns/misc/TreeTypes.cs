using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pipliz;
using Pipliz.JSON;
using System.IO;

namespace Ulfric.ColonyAddOns
{
    [ModLoader.ModManager]
    public static class TreeTypes
    {
        private static Dictionary<string, TreeConfig> treeStorage;
        private static Dictionary<string, ForestConfig> forestStorage;
        public static BoundsInt MaximumTreeSize;

        private static List<Structures.StructureBlock> columnTreesCache = new List<Structures.StructureBlock>();

        public struct TreeConfig
        {
            public Structures.StructureBlock[] blocks;

            public BoundsInt blockLimits;

            public TreeConfig(string jsonPath)
            {
                blocks = Structures.ReadStructure(jsonPath);
                Vector3Int vector3Int = Vector3Int.maximum;
                Vector3Int vector3Int2 = Vector3Int.minimum;
                for (int i = 0; i < blocks.Length; i++)
                {
                    vector3Int = Vector3Int.Min(vector3Int, blocks[i].position);
                    vector3Int2 = Vector3Int.Max(vector3Int2, blocks[i].position);
                }
                blockLimits = new BoundsInt(vector3Int, vector3Int2);
            }
        }

        private struct ForestConfig
        {
            private struct TreeConfigChance
            {
                public TreeConfig tree;

                public double chance;

                public TreeConfigChance(JSONNode node, ref double summedChance, double modifier)
                {
                    tree = treeStorage[node["tree"].GetAs<string>()];
                    double num = node["chance"].GetAs<double>() * modifier;
                    chance = num + summedChance;
                    summedChance += num;
                }
            }

            private TreeConfigChance[] forestTrees;

            public ForestConfig(string jsonPath)
            {
                if (!JSON.Deserialize(jsonPath, out JSONNode jSONNode, true, true))
                {
                    forestTrees = null;
                }
                else
                {
                    JSONNode jSONNode2 = jSONNode["trees"];
                    forestTrees = new TreeConfigChance[jSONNode2.ChildCount];
                    double num = 0.0;
                    double total = 0.0;

                    for (int i = 0; i < forestTrees.Length; i++)
                    {
                        total += jSONNode2[i]["chance"].GetAs<double>();
                    }

                    double modifier = 1 / total;
                    for (int i = 0; i < forestTrees.Length; i++)
                    {
                        forestTrees[i] = new TreeConfigChance(jSONNode2[i], ref num, modifier);
                        Logger.Log("Tree: {0}    Chance: {1}   Num: {2}", jSONNode2[1]["tree"].GetAs<string>(), jSONNode2[i]["chance"].GetAs<double>(), num);
                    }
                }
            }

            public bool IsTree(double roll, out TreeConfig rolledTree)
            {
                for (int i = 0; i < forestTrees.Length; i++)
                {
                    TreeConfigChance treeConfigChance = forestTrees[i];
                    if (roll < treeConfigChance.chance)
                    {
                        rolledTree = treeConfigChance.tree;
                        return true;
                    }
                }
                rolledTree = default(TreeConfig);
                return false;
            }
        }

        /// <summary>
        /// The afterItemType callback entrypoint. Used for registering jobs and recipes.
        /// </summary>
        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.MOD_NAMESPACE + "TreeTypes.AfterItemTypesDefined"),
            ModLoader.ModCallbackProvidesFor("pipliz.apiprovider.jobs.resolvetypes")]
        public static void AfterItemTypesDefined()
        {
            string[] files = Directory.GetFiles("gamedata/structures/trees/individualTrees/", "*.json");
            treeStorage = new Dictionary<string, TreeConfig>(files.Length);
            MaximumTreeSize = default(BoundsInt);
            for (int i = 0; i < files.Length; i++)
            {
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(files[i]);
                TreeConfig value = new TreeConfig(files[i]);
                treeStorage.Add(fileNameWithoutExtension, value);
                MaximumTreeSize = BoundsInt.Union(value.blockLimits, MaximumTreeSize);
            }
            string[] files2 = Directory.GetFiles("gamedata/structures/trees/", "*.json");
            forestStorage = new Dictionary<string, ForestConfig>(files2.Length);
            for (int j = 0; j < files2.Length; j++)
            {
                string fileNameWithoutExtension2 = Path.GetFileNameWithoutExtension(files2[j]);
                Log.Write("[Initializing] Loading forest from file: {0}", fileNameWithoutExtension2);
                forestStorage.Add(fileNameWithoutExtension2, new ForestConfig(files2[j]));
            }
        }

        public static void AddTree(TreeConfig t, Vector3Int position, Structures.StructureBlock[] blocksToAdd, BoundsInt columnBox)
        {
            for (int i = 0; i < t.blocks.Length; i++)
            {
                Structures.StructureBlock block = t.blocks[i];
                Vector3Int current = position.Add(block.position.x, block.position.y, block.position.z);
                ServerManager.TryChangeBlock(current, block.type);
            }
        }

        public static bool GrowTreeOfType(ETreeType treetype, Vector3Int treeposition)
        {
                double roll = Pipliz.Random.NextDouble(0.0,1.0);
                ForestConfig forestConfig;
                switch (treetype)
                {
                    case ETreeType.Savanna:
                        forestConfig = forestStorage["savanna"];
                        break;
                    case ETreeType.Taiga:
                        forestConfig = forestStorage["taiga"];
                        break;
                    case ETreeType.Jungle:
                        forestConfig = forestStorage["jungle"];
                        break;
                    default:
                        forestConfig = forestStorage["temperate"];
                        break;
                }
                if (forestConfig.IsTree(roll, out TreeConfig t))
                {
                    AddTree(t, treeposition, t.blocks, t.blockLimits);
                    return true;
                }
    
            return false;
        }

        public enum ETreeType
        {
            Invalid,
            None,
            Temperate,
            Taiga,
            Savanna,
            Jungle
        }
    }
}
