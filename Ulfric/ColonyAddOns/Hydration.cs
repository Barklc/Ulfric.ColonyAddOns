using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ulfric.ColonyAddOns
{
    [ModLoader.ModManager]
    public static class Hydration
    {
        private static Dictionary<string, bool> waterConsumed = new Dictionary<string, bool>();
        private static bool waterchecked = false;

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnUpdate, GameLoader.MOD_NAMESPACE + ".OnUpdate")]
        public static void OnUpdate()
        {
            if (Configuration.AllowDehydration)
            {
                if (TimeCycle.TimeOfDay >= TimeCycle.SunSet && TimeCycle.TimeOfDay < TimeCycle.SunSet + 1)
                {
                    if (!waterchecked)
                    {
                        waterchecked = true;

                        List<Players.Player> players = new List<Players.Player>();
                        Players.PlayerDatabase.ForeachValue(x => players.Add(x));
                        players.RemoveAll(x => string.IsNullOrEmpty(x.Name) && x.ID.type != NetworkID.IDType.LocalHost);

                        foreach (Players.Player p in players)
                        {
                            Logger.Log("players {0}", p.Name);
                            ushort type = ItemTypes.IndexLookup.GetIndex(Blocks.MOD_NAMESPACE + ".WaterBucket");

                            if (!waterConsumed.ContainsKey(p.Name))
                            {
                                waterConsumed.Add(p.Name, false);
                            }

                            if (!waterConsumed[p.Name])
                            {
                                var stockpile = Stockpile.GetStockPile(p);

                                Colony colony = Colony.Get(p);

                                //Determine to total value if hydration available
                                float totalHydrationValue = stockpile.AmountContained(type) * Configuration.WaterHydrationValue;
                                Logger.Log("Water amount {0} ID {1} IsValid {2}", stockpile.AmountContained(type), p.ID.ToString(), p.IsValid.ToString());
                                Logger.Log("Total Hydration Value = {0}", totalHydrationValue);

                                //Determine the total hydration value need to hydrate all the colonists
                                float followerHydrationNeeds = colony.FollowerCount * Configuration.HydrationValuePerColonists;
                                Logger.Log("Follower Hydration Needs = {0}", followerHydrationNeeds);

                                //If the total water used in less or equal to the total hydration value in the stock pile, remove the appropriate amount of water
                                if (followerHydrationNeeds <= totalHydrationValue)
                                {
                                    int waterToRemove = Convert.ToInt32(followerHydrationNeeds / Configuration.WaterHydrationValue);
                                    stockpile.TryRemove(type, waterToRemove);
                                    Logger.Log("Water to remove = {0}", waterToRemove);
                                }
                                else
                                {
                                    float neededHydration = followerHydrationNeeds - totalHydrationValue;
                                    Logger.Log("Need Hydrations = {0}", neededHydration.ToString());

                                    int waterToRemove = stockpile.AmountContained(type);
                                    stockpile.TryRemove(type, waterToRemove);
                                    Logger.Log("Water to remove = {0}", waterToRemove);

                                    int colonistsDehydrated = Convert.ToInt32(neededHydration / Configuration.HydrationValuePerColonists);
                                    Logger.Log("Colonists Dehydrated = {0}", colonistsDehydrated.ToString());

                                    for(int count =0; count<= colonistsDehydrated; count++)
                                    {
                                        NPC.NPCBase follower = colony.Followers.Last<NPC.NPCBase>();
                                        if (follower == null)
                                            break;
                                        follower.OnDeath();
                                        
                                    }
                                    Chat.Send(p, "You do not have enough water for the whole colony!", ChatColor.red);
                                }
                                waterConsumed[p.Name] = true;
                            }
                            else
                            {
                                waterConsumed[p.Name] = false;
                            }
                        }
                    }
                }
                else
                {
                    waterchecked = false;
                }
                
            }
        }
    }
}
