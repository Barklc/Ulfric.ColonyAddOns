using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlockTypes.Builtin;

namespace Ulfric.ColonyAddOns
{
    [ModLoader.ModManager]
    public static class Player
    {
        public const string NAMESPACE = "Ulfric.ColonyAddOns";
        private const string MOD_NAMESPACE = NAMESPACE + ".Player";

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerClicked, MOD_NAMESPACE + ".OnPlayerClicked")]
        public static void OnPlayerClicked(Players.Player player, Pipliz.Box<Shared.PlayerClickedData> boxedData)
        {
            if (Configuration.AllowHandPickingBerryBushes)
            {
                if (boxedData.item1.clickType == Shared.PlayerClickedData.ClickType.Right &&
                    boxedData.item1.rayCastHit.rayHitType == Shared.RayHitType.Block &&
                    World.TryGetTypeAt(boxedData.item1.rayCastHit.voxelHit, out var blockHit) &&
                    blockHit == BlockTypes.Builtin.BuiltinBlocks.BerryBush)
                {
                    Random random = new Random();

                    float chance = (float)random.NextDouble();
                    if (chance <= Configuration.ChanceOfBerriesPerPick)
                    {
                        var inv = Inventory.GetInventory(player);
                        inv.TryAdd(BuiltinBlocks.Berry, Configuration.NumberOfBerriesPerPick);
                    }
                    //else
                    //{
                    //    Chat.Send(player, "No berries picked.",ChatColor.red);
                    //}
                }
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterWorldLoad, MOD_NAMESPACE + ".AfterWorldLoad")]
        public static void AfterWorldLoad()
        {
            Logger.Log("Add 10 Water buckets to initial stockpile");
            Stockpile.AddToInitialPile(new InventoryItem(Blocks.MOD_NAMESPACE + ".WaterBucket", 10));
        }
    }
}
