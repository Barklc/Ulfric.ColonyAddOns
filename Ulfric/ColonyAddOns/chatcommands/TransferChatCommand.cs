using System;
using System.Text.RegularExpressions;
using Pipliz;
using Pipliz.Chatting;
using BlockTypes.Builtin;

namespace Ulfric.ColonyAddOns
{
    [ModLoader.ModManager]
    public class TransferChatCommand : ChatCommands.IChatCommand
    {
        public string MODPATH;
        private const string MOD_NAMESPACE = GameLoader.NAMESPACE + ".TransferChatCommand";

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, TransferChatCommand.MOD_NAMESPACE + ".registercommand")]
        public static void AfterItemTypesDefined()
        {
            ChatCommands.CommandManager.RegisterCommand(new TransferChatCommand());
        }

        public bool IsCommand(string chat)
        {
            return chat.Equals("/transfer") || chat.StartsWith("/transfer ");
        }

        public bool TryDoCommand(Players.Player causedBy, string chattext)
        {
            try
            {
                if (Configuration.EnableTransfers)
                {
                    var m = Regex.Match(chattext, @"/transfer (?<playername>['].+?[']|[^ ]+)");
                    if (!m.Success)
                    {
                        Chat.Send(causedBy, "Command didn't match, use /transfer [playername]");
                        return true;
                    }
                    string targetPlayerName = m.Groups["playername"].Value;
                    if (targetPlayerName.StartsWith("'"))
                    {
                        if (targetPlayerName.EndsWith("'"))
                        {
                            targetPlayerName = targetPlayerName.Substring(1, targetPlayerName.Length - 2);
                        }
                        else
                        {
                            Chat.Send(causedBy, "Command didn't match, missing ' after playername");
                            return true;
                        }
                    }
                    if (targetPlayerName.Length < 1)
                    {
                        Chat.Send(causedBy, "Command didn't match, no playername given");
                        return true;
                    }
                    Players.Player targetPlayer = null;
                    string error;
                    if (!PlayerHelper.TryGetPlayer(targetPlayerName, out targetPlayer, out error))
                    {
                        Chat.Send(causedBy, string.Format("Could not find target player '{0}'; {1}", targetPlayerName, error));
                        return true;
                    }
                    Inventory sourceInventory;
                    Stockpile targetStockpile;
                    if (Inventory.TryGetInventory(causedBy, out sourceInventory) && Stockpile.TryGetStockpile(targetPlayer, out targetStockpile))
                    {
                        if (sourceInventory.Items[7].Type != BuiltinBlocks.Air)
                        {
                            string itemTypeName = ItemTypes.IndexLookup.GetName(sourceInventory.Items[7].Type);
                            ushort sendItem = sourceInventory.Items[7].Type;
                            int sendNumber = sourceInventory.Items[7].Amount;
                            if (sourceInventory.TryRemove(sendItem))
                            {
                                targetStockpile.Add(sendItem, sendNumber);
                                Chat.Send(causedBy, string.Format("Send {0} x {1} to '{2}'", sendNumber, itemTypeName, targetPlayer.Name));
                                Chat.Send(targetPlayer, string.Format("Received {0} x {1} from '{2}'", sendNumber, itemTypeName, causedBy.Name));
                            }
                            else
                            {
                                Chat.Send(causedBy, "You don't have enough items");
                            }
                        }
                        else
                        {
                            Chat.Send(causedBy, "Nothing in slot 8 to send.");
                        }
                    }
                    else
                    {
                        Chat.Send(causedBy, "Could not get stockpile for both players");
                    }
                }
                else
                {
                    Chat.Send(causedBy, "Transfer chat command is disabled.");
                }
            }
            catch (Exception exception)
            {
                Pipliz.Log.WriteError(string.Format("Exception while parsing command; {0}", exception.Message));
            }
            return true;
        }
    }
}