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

                        List<Players.Player> players = Player.PlayerList();

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

                                //Determine to total value if hydration available
                                float totalHydrationValue = TotalHydrationInStockPile(stockpile);

                                //Determine the total hydration value need to hydrate all the colonists
                                float followerHydrationNeeded = TotalHydrationNeeded(p);

                                //Remove the followedrHydrationNeeded amount, if this is greater then what is available then return the unhydrated colonist count.
                                int colonistsDehydrated = UseHydration(followerHydrationNeeded, totalHydrationValue, stockpile);

                                //
                                for(int count = 0; count < colonistsDehydrated; count++)
                                {
                                    NPC.NPCBase follower = Colony.Get(p).Followers.Last<NPC.NPCBase>();
                                    if (follower == null)
                                        break;
                                    follower.OnDeath();
                                        
                                }
                                if (colonistsDehydrated !=0)
                                    Chat.Send(p, "You do not have enough water for {0} Colonist(s)!", ChatColor.red, colonistsDehydrated.ToString());
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

        public static float ItemHydrateValue(ushort itemToCheck)
        {
            ItemTypes.TryGetType(itemToCheck, out var item);
            item.CustomDataNode.TryGetAs("hydrationvalue", out float hydratevalue);
            return hydratevalue;
        }

        public static float TotalHydrationInStockPile(Stockpile s)
        {
             ushort type = ItemTypes.IndexLookup.GetIndex(Blocks.MOD_NAMESPACE + ".WaterBucket");

            //Determine to total value if hydration available
            float results = s.AmountContained(type) * ItemHydrateValue(type);
            Logger.Log("Water amount {0}", s.AmountContained(type));
            Logger.Log("Total Hydration Value = {0}", results);

            return results;
        }

        public static float TotalHydrationNeeded(Players.Player p)
        {
            Colony colony = Colony.Get(p);
            
            //Determine the total hydration value need to hydrate all the colonists
            float results = colony.FollowerCount * Configuration.HydrationValuePerColonists;
            Logger.Log("Follower Hydration Needs = {0}", results);

            return results;
        }

        public static int UseHydration(float HydrationAmountNeeded, float TotalHydrationValue, Stockpile stockpile)
        {
            int results = 0;
            ushort type = ItemTypes.IndexLookup.GetIndex(Blocks.MOD_NAMESPACE + ".WaterBucket");

            //Future add logic to support other Hydration sources.

            //Figure out how many WaterBuckets are needed
            int totalWaterNeeded = Convert.ToInt32(HydrationAmountNeeded / ItemHydrateValue(type));
            Logger.Log("Total WaterBuckets Needed {0}", totalWaterNeeded.ToString());

            //Get the number of WaterBuckets in stockpile
            int totalWaterInStockPile = stockpile.AmountContained(type);
            Logger.Log("Total Water In Stockpile {0}", totalWaterInStockPile);

            //if total needed exceeds total in stockpile then figure out how many colonists do not have water
            if (totalWaterNeeded > totalWaterInStockPile)
            {

                results = Convert.ToInt32(((float)(totalWaterNeeded - totalWaterInStockPile) * ItemHydrateValue(type)) /Configuration.HydrationValuePerColonists);
                Logger.Log("Colonists Dehydrated = {0}", results.ToString());
                totalWaterNeeded = totalWaterInStockPile;
            }

            stockpile.TryRemove(type, totalWaterNeeded);
            Logger.Log("Water to remove = {0}", totalWaterNeeded);

            return results;
        }
    }
}
