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
        private const string MOD_NAMESPACE = NAMESPACE + ".LineUpChatCommand";

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, LineUpChatCommand.MOD_NAMESPACE + ".registercommand")]
        public static void AfterItemTypesDefined()
        {
            ChatCommands.CommandManager.RegisterCommand(new LineUpChatCommand());
        }

        public bool IsCommand(string chat)
        {
            return chat.Equals("/lineup") || chat.Equals("/lineup list");
        }

        public bool TryDoCommand(Players.Player causedBy, string chattext)
        {
            SortedDictionary<string, int> lineup = new SortedDictionary<string, int>();

            var m = Regex.Match(chattext, @"/lineup( list)?");
            if (!m.Success)
            {
                Chat.Send(causedBy, "Command didn't match, use /lineup [list]");
                return true;
            }

            Colony colony = Colony.Get(causedBy);

            if (chattext.Equals("/lineup"))
            {
                SetPriorityJobs(colony);
            }
            else
            {
                for(int i=0;i<5;i++)
                {
                    string slot = PlayerState.GetPlayerState(causedBy).LineUp[i];
                    Chat.Send(causedBy, "Lineup slot [{0}] {1}", ChatColor.white, i.ToString(), slot);
                }
            }


            return true;
        }

        private static void SetPriorityJobs(Colony colony)
        {
            SortedDictionary<string, IJob> current = new SortedDictionary<string, IJob>();

            JobTracker.JobFinder jobfinder = JobTracker.GetOrCreateJobFinder(colony.Owner) as JobTracker.JobFinder;

            foreach (NPCBase follower in colony.Followers)
            {
                if (follower.Job != null)
                {
                    follower.Job.NPC = null;
                    follower.ClearJob();
                }
            }

            IJob[] openjobs = jobfinder.openJobs.ToArray();
            int count = 0;
            Logger.Log("{0} ", openjobs.Length.ToString());
            foreach(string o in PlayerState.GetPlayerState(colony.Owner).LineUp)
            {
                count = 0;
                while (count < openjobs.Length)
                {
                    IJob j = openjobs[count];
                    if (o == j.NPCType.ToString() && j.NeedsNPC)
                    {
                        foreach (NPCBase follower in colony.Followers)
                        {
                            if (follower.Job == null)
                            {
                                Logger.Log("Job set to : {0}", j.NPCType.ToString());
                                follower.TakeJob(j);
                                break;
                            }
                        }
                    }
                    count++;
                }
            }
        }
    }
}
