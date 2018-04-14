using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Pipliz;
using Pipliz.Chatting;
using Pipliz.JSON;
using NPC;
using Pipliz.Mods.APIProvider.Jobs;
/*
* Copy of Crone's top command
*/
namespace Ulfric.ColonyAddOns
{
    [ModLoader.ModManager]
    public class LineUpChatCommand : ChatCommands.IChatCommand
    {
        public string MODPATH;
        public const string NAMESPACE = "Ulfric.ColonyAddOns";
        private const string MOD_NAMESPACE = NAMESPACE + ". LineUpChatCommand";

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, LineUpChatCommand.MOD_NAMESPACE + ".registercommand")]
        public static void AfterItemTypesDefined()
        {
            ChatCommands.CommandManager.RegisterCommand(new LineUpChatCommand());
        }

        public bool IsCommand(string chat)
        {
            return chat.Equals("/lineup") || chat.StartsWith("/lineup ");
        }

        public bool TryDoCommand(Players.Player causedBy, string chattext)
        {
            SortedDictionary<string, int> lineup = new SortedDictionary<string, int>();

            if (!Permissions.PermissionsManager.CheckAndWarnPermission(causedBy, LineUpChatCommand.MOD_NAMESPACE))
            {
                Chat.Send(causedBy, string.Format("{0} does not have permission for '/roster' command.", causedBy.Name));
                return true;
            }
            var m = Regex.Match(chattext, @"/lineup (?<jobname>.+)");
            if (!m.Success)
            {
                Chat.Send(causedBy, "Command didn't match, use /lineup <jobname> <number>");
                return true;
            }
            string typename = m.Groups["jobname"].Value;

            Colony colony = Colony.Get(causedBy);


            //        foreach (NPCBase follower in colony.Followers)
            //        {
            //            string npc = follower.Job.NPCType.ToString();
            //            var jf =
            //        }

            //        if (typename.Equals("all"))
            //        {
            //            Chat.Send(causedBy, "Roster:", ChatColor.white);
            //            string results = "";

            //            foreach (KeyValuePair<string, int> job in roster)
            //            {
            //                if (results.Equals(""))
            //                {
            //                    results = job.Key + "(" + job.Value.ToString() + ")";
            //                }
            //                else
            //                {
            //                    results += ", " + job.Key + "(" + job.Value.ToString() + ")";
            //                }

            //            }
            //            Chat.Send(causedBy, "Jobs : {0}", ChatColor.white, results);
            //        }
            //        else if (typename.Equals("jobs"))
            //        {
            //            string results = "";
            //            foreach (KeyValuePair<string, int> job in roster)
            //            {
            //                if (results.Equals(""))
            //                    results = job.Key;
            //                else
            //                    results += ", " + job.Key;
            //            }
            //            Chat.Send(causedBy, "Available Jobs : {0}", ChatColor.white, results);
            //        }
            //        else if (roster.ContainsKey(typename))
            //        {
            //            roster.TryGetValue(typename, out int value);
            //            Chat.Send(causedBy, "Job {0} has {1} worker(s).", ChatColor.white, typename, value.ToString());
            //        }
            //        else
            //        {
            //            Chat.Send(causedBy, string.Format("There are no workers on that type.... {0}", typename));
            //        }

            return true;
        }

        private static Dictionary<int, IJob> JobsPresent(Colony colony)
        {
            Dictionary<int, IJob> joblist = new Dictionary<int, IJob>();

            foreach (NPCBase follower in colony.Followers)
            {
                joblist.Add(follower.ID, follower.Job);
            }

            return joblist;
        }

        private static Dictionary<string, int> JobsCount(Colony colony)
        {
            Dictionary<string, int> current = new Dictionary<string, int>();

            foreach (NPCBase follower in colony.Followers)
            {
                string npc = follower.Job.NPCType.ToString();
                if (current.ContainsKey(npc))
                {
                    current[npc]++;
                }
                else
                {
                    if (follower.Job.NeedsNPC)
                        current.Add(npc, 0);
                    else
                        current.Add(npc, 1);
                }
            }

            JobTracker.JobFinder jobfinder = JobTracker.GetOrCreateJobFinder(colony.Owner) as JobTracker.JobFinder;
            List<IJob> openjobs = jobfinder.openJobs;

            foreach (IJob job in openjobs)
            {
                string npc = job.NPCType.ToString();
                if (current.ContainsKey(npc))
                {
                    current[npc]++;
                }
                else
                {
                    current.Add(npc, 1);
                }
            }

            return current;
        }

        //    private static void ReJob(Colony colony, Dictionary<string, int> jobCount, Dictionary<int, IJob> jobsPresent, List<string> order)
        //    {
        //        Dictionary<int, bool> assigned = new Dictionary<int, bool>();

        //        //Loop through the order list and see what followers already are assigned to jobs based on order
        //        foreach (string currentJob in order)
        //        {

        //            //go through the list and see what jobs have been filled from order
        //            foreach (NPCBase follower in colony.Followers)
        //            {
        //                if (follower.Job.NPCType.ToString() == currentJob)
        //                {
        //                    if (jobCount.ContainsKey(currentJob) && jobCount[currentJob] > 0)
        //                    {
        //                        jobCount.TryGetValue(currentJob, out int value);
        //                        if (value > 0)
        //                        {
        //                            assigned[follower.ID] = true;
        //                            jobCount[currentJob] = value - 1;
        //                        }

        //                    }
        //                }
        //            }

        //            foreach (NPCBase follower in colony.Followers)
        //            {
        //                if (!assigned[follower.ID] && current.ContainsKey(currentJob) && current[currentJob] > 0)
        //                {
        //                    follower.Job =
        //                }
        //            }
        //        }
        //    }
    }
}
