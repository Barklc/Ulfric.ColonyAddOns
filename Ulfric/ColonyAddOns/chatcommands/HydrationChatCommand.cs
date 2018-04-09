using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Ulfric.ColonyAddOns
{
    [ModLoader.ModManager]
    public class HydrationChatCommand : ChatCommands.IChatCommand
    {
        private const string MOD_NAMESPACE = GameLoader.NAMESPACE + ".Hydration";

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, HydrationChatCommand.MOD_NAMESPACE + ".registercommand")]
        public static void AfterItemTypesDefined()
        {
            ChatCommands.CommandManager.RegisterCommand(new HydrationChatCommand());
        }

        public bool IsCommand(string chat)
        {
            return chat.Equals("/hydration");
        }

        public bool TryDoCommand(Players.Player causedBy, string chattext)
        {
            try
            {
                var m = Regex.Match(chattext, @"/hydration");
                if (!m.Success)
                {
                    Chat.Send(causedBy, "Command didn't match, use /hydration");
                    return true;
                }

                float TotalHydrationInStockPile = Hydration.TotalHydrationInStockPile(Stockpile.GetStockPile(causedBy));
                float TotalHydrationNeeded = Hydration.TotalHydrationNeeded(causedBy);

                Chat.Send(causedBy, "Hydration:");

                ChatColor chatColor = ChatColor.white;
                if (TotalHydrationNeeded > TotalHydrationInStockPile)
                    chatColor = ChatColor.red;
                if (TotalHydrationNeeded * 2 > TotalHydrationInStockPile)
                    chatColor = ChatColor.orange;

                Chat.Send(causedBy, "Total Hydration: {0}", chatColor, TotalHydrationInStockPile.ToString());
                Chat.Send(causedBy, "Need Hydration per day {0}", ChatColor.white, TotalHydrationNeeded.ToString());

            }
            catch (System.Exception exception)
            {
                Logger.Log(string.Format("Exception while parsing command; {0}", exception.Message));
            }
            return true;
        }
    }
}
