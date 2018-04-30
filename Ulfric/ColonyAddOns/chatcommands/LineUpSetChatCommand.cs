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
    public class LineUpSetChatCommand : ChatCommands.IChatCommand
    {
        public string MODPATH;
        public const string NAMESPACE = "Ulfric.ColonyAddOns";
        private const string MOD_NAMESPACE = NAMESPACE + ".LineUpSetChatCommand";

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, LineUpSetChatCommand.MOD_NAMESPACE + ".registercommand")]
        public static void AfterItemTypesDefined()
        {
            ChatCommands.CommandManager.RegisterCommand(new LineUpSetChatCommand());
        }

        public bool IsCommand(string chat)
        {
            return chat.Equals("/lineupset") || chat.StartsWith("/lineupset ");
        }

        public bool TryDoCommand(Players.Player causedBy, string chattext)
        {
            SortedDictionary<string, int> lineup = new SortedDictionary<string, int>();

            var m = Regex.Match(chattext, @"/lineupset (?<jobname>['].+?[']|[^ ]+) (?<page>\d+)");
            if (!m.Success)
            {
                Chat.Send(causedBy, "Command didn't match, use /lineupset <jobname> <number>");
                return true;
            }
            string jobname = m.Groups["jobname"].Value;
            if (jobname.StartsWith("'"))
            {
                if (jobname.EndsWith("'"))
                {
                    jobname = jobname.Substring(1, jobname.Length - 2);
                }
                else
                {
                    Chat.Send(causedBy, "Missing ' after jobname");
                    return true;
                }
            }
            string place = m.Groups["place"].Value;

            int index = Convert.ToInt32(place);
            if (index > -1 && index < 5)
            {
                //Add Jobname validation
                PlayerState.GetPlayerState(causedBy).LineUp[index] = jobname;
                Chat.Send(causedBy, "LineUp spot {0} set to {1}.", ChatColor.white, place, jobname);
            }
            else
            {
                Chat.Send(causedBy, "Place must be 0 to 4.");
            }

            return true;
        }
    }
}
