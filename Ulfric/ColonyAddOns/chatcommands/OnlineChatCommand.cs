using System;
using Pipliz;
using Pipliz.Chatting;

namespace Ulfric.ColonyAddOns
{
    [ModLoader.ModManager]
    public class OnlineChatCommand : ChatCommands.IChatCommand
    {
        public string MODPATH;
        public const string NAMESPACE = "Ulfric.ColonyAddOns";
        private const string MOD_NAMESPACE = NAMESPACE + ".OnlineChatCommand";

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, OnlineChatCommand.MOD_NAMESPACE + ".registercommand")]
        public static void AfterItemTypesDefined()
        {
            ChatCommands.CommandManager.RegisterCommand(new OnlineChatCommand());
        }

        public bool IsCommand(string chat)
        {
            return chat.Equals("/online");
        }

        public bool TryDoCommand(Players.Player causedBy, string chattext)
        {
            try
            {
                String msg = "";
                for (int c = 0; c < Players.CountConnected; c++)
                {
                    Players.Player player = Players.GetConnectedByIndex(c);
                    msg += player.Name;
                    if (c < Players.CountConnected - 1)
                    {
                        msg += ", ";
                    }
                }
                msg += string.Format("\nTotal {0} players online", Players.CountConnected);
                Chat.Send(causedBy, msg);
            }
            catch (Exception exception)
            {
                Logger.Log(string.Format("Exception while parsing command; {0}", exception.Message));
            }
            return true;
        }
    }
}