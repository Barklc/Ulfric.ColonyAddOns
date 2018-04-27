using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pipliz.JSON;
using System.IO;

namespace Ulfric.ColonyAddOns
{
    [ModLoader.ModManager]
    public static class ColonyManager
    {
        public static List<Job> JobList = new List<Job>();

        public static bool AddJobs(Job job)
        {
            JobList.Add(job);
            return true;
        }

        public static bool RemoveJobs(Job job)
        {
            
            return JobList.Remove(job);
        }

        //Set class variables and constants
        public const string NAMESPACE = "Ulfric.ColonyAddOns";
        private const string MOD_NAMESPACE = NAMESPACE + ".ColonyManager";

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnQuit, ColonyManager.MOD_NAMESPACE + ".OnQuit")]
        public static void OnQuit()
        {
            SaveJobs();
        }
        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnAutoSaveWorld, ColonyManager.MOD_NAMESPACE + ".OnAutoSaveWorld")]
        public static void OnAutoSaveWorld()
        {
            SaveJobs();
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnQuitEarly, ColonyManager.MOD_NAMESPACE + ".OnQuitEarly")]
        public static void OnQuitEarly()
        {
            SaveJobs();
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerDisconnected, ColonyManager.MOD_NAMESPACE + "OnPlayerDisconnected")]
        public static void OnPlayerDisconnected(Players.Player p)
        {
            SaveJobs();
        }

        public static void SaveJobs()
        {
            string name = "";
            try
            {
                JSONNode root = null;

                string folder = $"{GameLoader.SavedGameFolder}/{ServerManager.WorldName}/";

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                string file = $"{folder}ColonyAddOnsJobs.json";

                if (File.Exists(file))
                    JSON.Deserialize(file, out root);

                if (root == null)
                    root = new JSONNode();
                
                foreach (Players.Player player in Players.PlayerDatabase.ValuesAsList)
                {
                    if (player.ID == null)
                        name = player.Name;
                    else
                        name = player.IDString;

                     if (root.TryGetAs(name, out JSONNode playerjoblist))
                     {

                        JSONNode subnode = new JSONNode(NodeType.Array);
                        foreach (Job j in JobList)
                        {
                            if (j.NPC != null && j.Owner == player)
                                subnode.AddToArray(j.GetJSON());
                        }
                        root.SetAs<JSONNode>(name, subnode);
                     }
                    else
                    {
                        root.SetAs<JSONNode>(name, new JSONNode());
                    }

                }
                JSON.Serialize(file, root);
            }
            catch (System.Exception e)
            {
                Logger.Log("{0}.SaveJobs had an error : {1}", ColonyManager.MOD_NAMESPACE, e.Message);

            }

        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterWorldLoad, ColonyManager.MOD_NAMESPACE + ".AfterWorldLoad")]
        public static void AfterWorldLoad()
        {
            try
            {
                string folder = $"{GameLoader.SavedGameFolder}/{ServerManager.WorldName}/";
                string file = $"{folder}ColonyAddOnsJobs.json";

                if (File.Exists(file))
                {
                    if (JSON.Deserialize(file, out var n))
                    {
                        foreach(KeyValuePair<string,JSONNode> p in n.LoopObject())
                        {
                            Logger.Log("Player {0}", p.Key);
                            foreach (JSONNode j in p.Value.LoopArray())
                            {
                                if (j.TryGetAs<string>("type", out string result))
                                {
                                    Logger.Log("Type {0} Player {1}", result, Players.GetPlayer(NetworkID.Parse(p.Key)).Name);
                                    //Add new jobs here to create them
                                    
                                    switch (result)
                                    {
                                        case "Ulfric.ColonyAddOns.Militia":
                                            MilitiaJob m = new MilitiaJob();
                                            m.InitializeFromJSON(Players.GetPlayer(NetworkID.Parse(p.Key)), j);
                                            JobList.Add(m);
                                            break;
                                    }

                                }
                            }
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Logger.Log("{0}.AfterWorldLoad had an error : {1}", ColonyManager.MOD_NAMESPACE, e.Message);

            }
        }
    }
}
