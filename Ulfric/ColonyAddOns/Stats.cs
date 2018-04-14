using System.Collections.Generic;
using Pipliz;
using NPC;
using BlockTypes.Builtin;
using System.Text.RegularExpressions;
using System.IO;
using Pipliz.JSON;
using System;

namespace Ulfric.ColonyAddOns
{
    [ModLoader.ModManager]
    public class Statistics
    {
        // Location of MOD .DLL file at runtime.
        private const string MOD_NAMESPACE = GameLoader.NAMESPACE + ".Statistics";
        private const string BLOCKS_NAMESPACE = GameLoader.NAMESPACE + ".Blocks";
        public static Dictionary<string, SortedDictionary<string, int>> Stats = new Dictionary<string, SortedDictionary<string, int>>();

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnNPCCraftedRecipe, Statistics.MOD_NAMESPACE + ".OnNPCCraftedRecipe")]
        public static void OnNPCCraftedRecipe(IJob job, Recipe recipe, List<InventoryItem> results)
        {
            try
            {
                if (Configuration.EnableStatisticCollecting)
                {

                    Players.Player p = Players.GetPlayer(job.Owner.ID);
                    SortedDictionary<string, int> PlayerStats;

                    if (!Stats.ContainsKey(p.Name))
                    {
                        PlayerStats = new SortedDictionary<string, int>();
                        Stats.Add(p.Name, PlayerStats);
                    }
                    else
                    {
                        PlayerStats = Stats[p.Name];
                    }

                    foreach (InventoryItem it in results)
                    {
                        string name = ItemTypes.IndexLookup.GetName(it.Type);
                        if (name.Contains("."))
                            name = name.Substring(name.LastIndexOf('.') + 1);

                        if (!PlayerStats.ContainsKey(name))
                            PlayerStats.Add(name, 1);
                        else
                        {
                            if (PlayerStats.TryGetValue(name, out int count))
                            {
                                count++;
                                PlayerStats[name] = count;
                            }
                        }
                    }
                    Stats[p.Name] = PlayerStats;
                }
            }
            catch (System.Exception e)
            {
                Logger.Log("{0}.OnNPCCraftedRecipe had an error : {1}", Statistics.MOD_NAMESPACE, e.Message);

            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnQuit, Statistics.MOD_NAMESPACE + ".OnQuit")]
        public static void OnQuit()
        {
            if (Configuration.EnableStatisticCollecting)
                SaveStatistics();
        }
        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnAutoSaveWorld, Statistics.MOD_NAMESPACE + ".OnAutoSaveWorld")]
        public static void OnAutoSaveWorld()
        {
            if (Configuration.EnableStatisticCollecting)
                SaveStatistics();
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnQuitEarly, Statistics.MOD_NAMESPACE + ".OnQuitEarly")]
        public static void OnQuitEarly()
        {
            if (Configuration.EnableStatisticCollecting)
                SaveStatistics();
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerDisconnected, Statistics.MOD_NAMESPACE + "OnPlayerDisconnected")]
        public static void OnPlayerDisconnected(Players.Player p)
        {
            if (Configuration.EnableStatisticCollecting)
                SaveStatistics();
        }

        public static void SaveStatistics()
        {
            try
            {
                JSONNode n = null;

                string folder = $"{GameLoader.SavedGameFolder}/{ServerManager.WorldName}/players/Statistics/";

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                string file = $"{folder}Statistics.json";

                if (File.Exists(file))
                    JSON.Deserialize(file, out n);

                if (n == null)
                    n = new JSONNode();

                foreach (KeyValuePair<string, SortedDictionary<string, int>> playerstats in Statistics.Stats)
                {
                    Logger.Log("Saving Playerstats For = {0}", playerstats.Key);

                    if (n.HasChild(playerstats.Key))
                        n.RemoveChild(playerstats.Key);

                    JSONNode Root = new JSONNode(NodeType.Object);

                    foreach (KeyValuePair<string, int> item in playerstats.Value)
                    {
                        Root.SetAs(item.Key, item.Value);
                    }

                    if (Root != null)
                    {
                        n.SetAs(playerstats.Key, Root);
                    }
                }
                JSON.Serialize(file, n);
            }
            catch (System.Exception e)
            {
                Logger.Log("{0}.SaveStatistics had an error : {1}", Statistics.MOD_NAMESPACE, e.Message);

            }

        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterWorldLoad, Statistics.MOD_NAMESPACE + ".AfterWorldLoad")]
        public static void AfterWorldLoad()
        {
            try
            {
                if (Configuration.EnableStatisticCollecting)
                {
                    string folder = $"{GameLoader.SavedGameFolder}/{ServerManager.WorldName}/players/Statistics/";
                    string file = $"{folder}Statistics.json";

                    if (File.Exists(file))
                    {
                        if (JSON.Deserialize(file, out var n))
                        {
                            if (n.NodeType == NodeType.Object)
                            {

                                foreach (KeyValuePair<string, JSONNode> playerEntry in n.LoopObject())
                                {
                                    Logger.Log("Loading Player Stats {0} = {1}", playerEntry.Key, playerEntry.Value.ChildCount);
                                    foreach (var child in playerEntry.Value.LoopObject())
                                    {
                                        if (Statistics.Stats.ContainsKey(playerEntry.Key))
                                        {
                                            Statistics.Stats[playerEntry.Key].Add(child.Key, child.Value.GetAs<int>());
                                        }
                                        else
                                        {
                                            SortedDictionary<string, int> item = new SortedDictionary<string, int>();
                                            item.Add(child.Key, child.Value.GetAs<int>());
                                            Statistics.Stats.Add(playerEntry.Key, item);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Logger.Log("{0}.AfterWorldLoad had an error : {1}", Statistics.MOD_NAMESPACE, e.Message);

            }
        }
    }
}
