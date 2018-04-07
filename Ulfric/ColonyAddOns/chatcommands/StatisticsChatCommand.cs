using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Ulfric.ColonyAddOns
{
    [ModLoader.ModManager]
    public class StatisticsChatCommand : ChatCommands.IChatCommand
    {
        private const string MOD_NAMESPACE = GameLoader.NAMESPACE + ".Statistics";

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, StatisticsChatCommand.MOD_NAMESPACE + ".registercommand")]
        public static void AfterItemTypesDefined()
        {
            ChatCommands.CommandManager.RegisterCommand(new StatisticsChatCommand());
        }

        public bool IsCommand(string chat)
        {
            return chat.Equals("/stats") || chat.StartsWith("/stats ");
        }

        public bool TryDoCommand(Players.Player causedBy, string chattext)
        {
            if (Configuration.EnableStatisticCollecting)
            {
                try
                {
                    var m = Regex.Match(chattext, @"/stats (?<typename>.+)( (?<page>\d+))?");
                    if (!m.Success)
                    {
                        Chat.Send(causedBy, "Command didn't match, use /stats [item] [page]");
                        return true;
                    }
                    string typename = m.Groups["typename"].Value;
                    string page = m.Groups["page"].Value;

                    Chat.Send(causedBy, "Statistics:");

                    if (Statistics.Stats.ContainsKey(causedBy.Name))
                    {
                        if (typename.Equals("reset"))
                        {
                            Statistics.Stats[causedBy.Name].Clear();
                            Chat.Send(causedBy, "Reset");
                        }
                        else if (typename.Equals("all"))
                        {
                            int pagenum = 0;

                            if (!page.Equals(string.Empty))
                                pagenum = Convert.ToInt32(page);

                            List<string> Display = new List<string>();
                            StringBuilder results = new StringBuilder();

                            SortedDictionary<string, int> PlayerStats = Statistics.Stats[causedBy.Name];
                            foreach (KeyValuePair<string, int> i in PlayerStats)
                            {
                                if (results.Length > 240)
                                {
                                    Display.Add(results.ToString());
                                    results = new StringBuilder();
                                }
                                if (results.Length == 0)
                                {
                                    results.AppendFormat("{0}({1})", i.Key, i.Value.ToString());
                                }
                                else
                                {
                                    results.AppendFormat(", {0}({1})", i.Key, i.Value.ToString());
                                }
                            }
                            if (Display.Count > 0)
                            {
                                if (pagenum <= Display.Count)
                                {
                                    Chat.Send(causedBy, "Stats : {0}", ChatColor.white, Display[pagenum]);
                                    if (Display.Count > 0)
                                        Chat.Send(causedBy, "Pages available {0}", ChatColor.white, Display.Count.ToString());
                                    Logger.Log("PlayerStats ({0}) ", results.ToString());
                                }

                            }
                        }
                        else
                        {
                            ushort index = ItemTypes.IndexLookup.GetIndex(typename);
                            if (index > 0)
                            {
                                if (Statistics.Stats[causedBy.Name].TryGetValue(typename, out int count))
                                {
                                    Chat.Send(causedBy, string.Format("Statistics {0} : {1}", typename, count));
                                }
                            }
                            else
                            {
                                Chat.Send(causedBy, string.Format("No statistics for {0} have been collected.", typename));
                            }
                        }
                    }
                    else
                        Chat.Send(causedBy, string.Format("No statistics for {0} have been collected.", causedBy.Name));
                }
                catch (System.Exception exception)
                {
                    Logger.Log(string.Format("Exception while parsing command; {0}", exception.Message));
                }
            }
            return true;
        }
    }
}
