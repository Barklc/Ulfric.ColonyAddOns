using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ulfric.ColonyAddOns
{
    [ModLoader.ModManager]
    public static class Hydration
    {
        private static bool waterConsumed = false;

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnUpdate, GameLoader.MOD_NAMESPACE + ".OnUpdate")]
        public static void OnUpdate()
        {
            if (Configuration.AllowDehydration)
            {
                Players.PlayerDatabase.ForeachValue(p =>
                {
                    var stockpile = Stockpile.GetStockPile(p);
                    var colony = Colony.Get(p);
                    ushort type = ItemTypes.IndexLookup.GetIndex(Blocks.MOD_NAMESPACE + ".WaterBucket");

                    if (TimeCycle.IsDay)
                    {
                        if (!waterConsumed)
                        {
                            var hasWater = stockpile.Contains(type);

                            if (hasWater)
                            {
                                //Determine to total value if hydration available
                                float totalHydrationValue = stockpile.AmountContained(type) * Configuration.WaterHydrationValue;
                                Logger.Log("Total Hydration Value = {0}", totalHydrationValue);

                                //Determine the total hydration value need to hydrate all the colonists
                                float followerHydrationNeeds = colony.Followers.Count * Configuration.HydrationValuePerColonists;
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
                                    Logger.Log("Need Hydrations = (0)", neededHydration.ToString());

                                    int waterToRemove = stockpile.AmountContained(type);
                                    stockpile.TryRemove(type, waterToRemove);
                                    Logger.Log("Water to remove = {0}", waterToRemove);

                                    int colonistsDehydrated = Convert.ToInt32(neededHydration / followerHydrationNeeds);
                                    Logger.Log("Colonists Dehydrated = {0}", colonistsDehydrated.ToString());

                                    int count = 0;
                                    foreach (var follower in colony.Followers)
                                    {
                                        follower.OnDeath();
                                        follower.Update();
                                        count++;
                                        if (count > colonistsDehydrated)
                                            break;
                                    }
                                    Chat.Send(p, "You do not have enough water for the whole colony!", ChatColor.red);
                                }
                            }
                            waterConsumed = true;
                        }
                    }
                    else
                    {
                        waterConsumed = false;
                    }
                });
            }
        }
    }
}
