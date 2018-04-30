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
    public class RosterChatCommand : ChatCommands.IChatCommand
    {
        public string MODPATH;
        public const string NAMESPACE = "Ulfric.ColonyAddOns";
        private const string MOD_NAMESPACE = NAMESPACE + ".RosterChatCommand";

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, RosterChatCommand.MOD_NAMESPACE + ".registercommand")]
        public static void AfterItemTypesDefined()
        {
            ChatCommands.CommandManager.RegisterCommand(new RosterChatCommand());
        }

        public bool IsCommand(string chat)
        {
            return chat.Equals("/roster") || chat.StartsWith("/roster ");
        }

        public bool TryDoCommand(Players.Player causedBy, string chattext)
        {
            SortedDictionary<string, int> roster = new SortedDictionary<string, int>();

            var m = Regex.Match(chattext, @"/roster (?<jobname>['].+?[']|[^ ]+)");
            if (!m.Success)
            {
                Chat.Send(causedBy, "Command didn't match, use /roster <jobname>");
                return true;
            }
            string typename = m.Groups["jobname"].Value;
            if (typename.StartsWith("'"))
            {
                if (typename.EndsWith("'"))
                {
                    typename = typename.Substring(1, typename.Length - 2);
                }
                else
                {
                    Chat.Send(causedBy, "Missing ' after typename");
                    return true;
                }
            }
            Colony colony = Colony.Get(causedBy);

            foreach (NPCBase follower in colony.Followers)
            {
                string npc = follower.Job.NPCType.ToString();
                if (roster.ContainsKey(npc))
                {
                    roster[npc]++;
                }
                else
                {
                    if (follower.Job.NeedsNPC)
                        roster.Add(npc, 0);
                    else
                        roster.Add(npc, 1);
                }
            }

            if (typename.Equals("all"))
            {
                Chat.Send(causedBy, "Roster:",ChatColor.white);
                string results = "";

                foreach(KeyValuePair<string,int> job in roster)
                {
                    if (results.Equals(""))
                    {
                        results = job.Key + "(" + job.Value.ToString() + ")";
                    }
                    else
                    {
                        results += ", " + job.Key + "(" + job.Value.ToString() + ")";
                    }

                }
                Chat.Send(causedBy, "Jobs : {0}", ChatColor.white, results);
            }
            else if (typename.Equals("jobs"))
            {
                string results = "";
                foreach(KeyValuePair<string,int> job in roster)
                {
                    if (results.Equals(""))
                        results = job.Key;
                    else
                        results += ", " + job.Key;
                }
                Chat.Send(causedBy, "Available Jobs : {0}", ChatColor.white, results);
            }
            else if (roster.ContainsKey(typename))
            {
                roster.TryGetValue(typename, out int value);
                Chat.Send(causedBy, "Job {0} has {1} worker(s).", ChatColor.white, typename, value.ToString());
            }
            else
            {
                Chat.Send(causedBy, string.Format("There are no workers on that type.... {0}", typename));
            }

            return true;
        }
    }
}
