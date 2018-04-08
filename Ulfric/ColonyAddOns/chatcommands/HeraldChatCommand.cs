using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Ulfric.ColonyAddOns
{
    [ModLoader.ModManager]
    public class HeraldChatCommand : ChatCommands.IChatCommand
    {
        private const string MOD_NAMESPACE = GameLoader.NAMESPACE + ".Herald";

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, HeraldChatCommand.MOD_NAMESPACE + ".registercommand")]
        public static void AfterItemTypesDefined()
        {
            ChatCommands.CommandManager.RegisterCommand(new StatisticsChatCommand());
        }

        public bool IsCommand(string chat)
        {
            return chat.Equals("/herald") || chat.StartsWith("/herald ");
        }

        public bool TryDoCommand(Players.Player causedBy, string chattext)
        {
            try
            {
                var m = Regex.Match(chattext, @"/herald (?<action>.+)");
                if (!m.Success)
                {
                    Chat.Send(causedBy, "Command didn't match, use /herald [action]");
                    return true;
                }
                string action = m.Groups["action"].Value;

                PlayerState playerState = PlayerState.GetPlayerState(causedBy);

                if (action.Equals(""))
                {
                    Chat.Send(causedBy, "Herald:");
                    Chat.Send(causedBy, "Sunrise: {0}", ChatColor.white, playerState.EnableHeraldAnnouncingSunrise.ToString());
                    Chat.Send(causedBy, "Sunset: {0}", ChatColor.white, playerState.EnableHeraldAnnouncingSunset.ToString());
                    Chat.Send(causedBy, "Warning: {0}", ChatColor.white, playerState.EnableHeraldWarning.ToString());
                }

                if (action.Equals("sunrise"))
                {
                    if (PlayerState.GetPlayerState(causedBy).EnableHeraldAnnouncingSunrise)
                    {
                        PlayerState.GetPlayerState(causedBy).EnableHeraldAnnouncingSunrise = false;
                        Chat.Send(causedBy, "Herald Sunrise Announcement is OFF");
                    }
                    else
                    {
                        PlayerState.GetPlayerState(causedBy).EnableHeraldAnnouncingSunrise = true;
                        Chat.Send(causedBy, "Herald Sunrise Announcement is ON");
                    }
                }

                if (action.Equals("sunset"))
                {
                    if (PlayerState.GetPlayerState(causedBy).EnableHeraldAnnouncingSunset)
                    {
                        PlayerState.GetPlayerState(causedBy).EnableHeraldAnnouncingSunset = false;
                        Chat.Send(causedBy, "Herald Sunset Announcement is OFF");
                    }
                    else
                    {
                        PlayerState.GetPlayerState(causedBy).EnableHeraldAnnouncingSunset = true;
                        Chat.Send(causedBy, "Herald Sunset Announcement is ON");
                    }
                }

                if (action.Equals("rally"))
                {
                    if (PlayerState.GetPlayerState(causedBy).EnableHeraldWarning)
                    {
                        PlayerState.GetPlayerState(causedBy).EnableHeraldWarning = false;
                        Chat.Send(causedBy, "Herald Rally Announcement is OFF");
                    }
                    else
                    {
                        PlayerState.GetPlayerState(causedBy).EnableHeraldWarning = true;
                        Chat.Send(causedBy, "Herald Rally Announcement is ON");
                    }
                }
            }
            catch (System.Exception exception)
            {
                Logger.Log(string.Format("Exception while parsing command; {0}", exception.Message));
            }
            return true;
        }
    }
}
