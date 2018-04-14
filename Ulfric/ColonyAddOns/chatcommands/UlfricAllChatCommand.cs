using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Pipliz;
using Pipliz.Chatting;
using Pipliz.JSON;

/*
 * Copy of Crone's top command
 */
namespace Ulfric.ColonyAddOns
{
    [ModLoader.ModManager]
    public class UlfricAllChatCommand : ChatCommands.IChatCommand
    {
        public string MODPATH;
        public const string NAMESPACE = "Ulfric.ColonyAddOns";
        private const string MOD_NAMESPACE = NAMESPACE + ".UlfricAllChatCommand";

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, UlfricAllChatCommand.MOD_NAMESPACE + ".registercommand")]
        public static void AfterItemTypesDefined()
        {
            ChatCommands.CommandManager.RegisterCommand(new UlfricAllChatCommand());
        }

        public bool IsCommand(string chat)
        {
            return chat.Equals("/ulfric") || chat.StartsWith("/ulfric ");
        }

        public bool TryDoCommand(Players.Player causedBy, string chattext)
        {
            try
            {
                if (!Permissions.PermissionsManager.CheckAndWarnPermission(causedBy, UlfricAllChatCommand.MOD_NAMESPACE))
                {
                    Chat.Send(causedBy,string.Format("{0} does not have permission for '/ulfric' command.", causedBy.Name));
                    return true;
                }
                var m = Regex.Match(chattext, @"/ulfric (?<typename>.+)");
                if (!m.Success)
                {
                    Chat.Send(causedBy, "Command didn't match, use /ulfric <item>");
                    return true;
                }
                string typename = m.Groups["typename"].Value;
                if (typename.Equals("help"))
                {
                    Chat.Send(causedBy, "ColonyAddOns Chat Commands");
                    Chat.Send(causedBy,"/ulfric <item or all> - Place one of the block specified or one of every block added by the mod in your stockpile. 'Cheats need'");
                    Chat.Send(causedBy, "/herald <action> - Toggle on or off sunrise, sunset and rally for your heralds.");
                    Chat.Send(causedBy, "/hydration - Displays the total hydration value you have and how much you need per day for your colony.");
                    Chat.Send(causedBy, "/roster <job name> - Displays all jobs in colony that is manned and by how many colonists.");
                    Chat.Send(causedBy, "/stats [item] [page] - Displays the number of the item specified that has been crafted.  /stat reset - Clears list.");
                    Chat.Send(causedBy, "/trade [Player] [item] [number] - Send the player the number of item specified.");

                }
                if (typename.Equals("all"))
                {
                    Stockpile targetStockpile;
                    if (Stockpile.TryGetStockpile(causedBy, out targetStockpile))
                    {
                        if (JSON.Deserialize(GameLoader.ConfigFolder + "/" + "types.json", out JSONNode jsonTypes, false))
                        {
                            if (jsonTypes.NodeType == NodeType.Object)
                            {
                                foreach (KeyValuePair<string, JSONNode> typeEntry in jsonTypes.LoopObject())
                                {
                                    try
                                    {
                                        string itemName = NAMESPACE + ".Blocks." + typeEntry.Key;
                                        bool placeable = true;
                                        if (!typeEntry.Value.TryGetAs("isPlaceable", out placeable))
                                        {
                                            placeable = true;
                                        }
                                        if (ItemTypes.IndexLookup.TryGetIndex(itemName, out ushort itemType) && placeable && !itemName.EndsWith("+") && !itemName.EndsWith("-"))
                                        {
                                            InventoryItem tradeItem = new InventoryItem(itemType, 1);
                                            targetStockpile.Add(tradeItem);
                                        }
                                    }
                                    catch (Exception exception)
                                    {
                                        Logger.Log(string.Format("Item not found; {0}", exception.Message));
                                    }
                                }
                                Chat.Send(causedBy, string.Format("Added [Ulfric.ColonyAddOns.Blocks] to stockpile"));

                            }
                        }
                    }
                }
                else 
                {
                    ushort itemType;
                    if (!ItemTypes.IndexLookup.TryGetIndex(NAMESPACE + ".Blocks." + typename, out itemType))
                    {
                        Chat.Send(causedBy, "Command didn't match, item type not found");
                        return true;
                    }
                    Stockpile targetStockpile;
                    if (Stockpile.TryGetStockpile(causedBy, out targetStockpile))
                    {
                        InventoryItem tradeItem = new InventoryItem(GameLoader.NAMESPACE + ".Blocks." +  typename, 1);
                        targetStockpile.Add(tradeItem);
                        Chat.Send(causedBy, string.Format("Added [{0}] to stockpile", typename));
                    }
                    else
                    {
                        Chat.Send(causedBy, string.Format("Could not find stockpile for {0}",causedBy.Name));
                    }

                }
            }
            catch (Exception exception)
            {
                Logger.Log(string.Format("Exception while parsing command; {0}", exception.Message));
            }
            return true;
        }
    }

    public static class Utility
    {
        public static double CalculatePlayerScore(Players.Player c1)
        {
            var colony = Colony.Get(c1);
            int colonists = colony.FollowerCount;
            double foodForDays;
            if (colonists != 0)
            {
                foodForDays = Stockpile.GetStockPile(c1).TotalFood / System.Math.Round((colony.FoodUsePerHour * colonists) * 24, 1);
            }
            else
            {
                foodForDays = 0;
            }
            return (DiminishingReturns(foodForDays, 1)) * (colonists);
        }

        public static double DiminishingReturns(double val, double scale)
        {
            if (val < 0)
            {
                return -DiminishingReturns(-val, scale);
            }
            double mult = val / scale;
            double trinum = (System.Math.Sqrt(8.0 * mult + 1.0) - 1.0) / 2.0;
            return trinum * scale;
        }
    }
}
