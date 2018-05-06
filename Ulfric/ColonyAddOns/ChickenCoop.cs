using System;
using System.Collections.Generic;
using BlockTypes.Builtin;
using System.Text;

namespace Ulfric.ColonyAddOns.ActiveBlocks
{
    [ModLoader.ModManager]
    public static class ChickenCoop
    {
        public static ItemTypesServer.ItemTypeRaw Item { get; private set; }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterAddingBaseTypes, GameLoader.NAMESPACE + ".ActiveBlocks.ChickenCoop.RegisterActiveBlocks")]
        public static void RegisterMachines(Dictionary<string, ItemTypesServer.ItemTypeRaw> items)
        {
            ActiveBlockManager.RegisterBlockType(nameof(ChickenCoop), new ActiveBlockManager.BlockSetting(Item.ItemIndex, DoWork));
        }

        public static void DoWork(Players.Player player, ActiveBlockManager.BlockState BlockState)
        {

        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnTryChangeBlock, GameLoader.NAMESPACE + ".Items.Machines.Miner.OnTryChangeBlockUser")]
        public static void OnTryChangeBlockUser(ModLoader.OnTryChangeBlockData d)
        {
            if (d.CallbackState == ModLoader.OnTryChangeBlockData.ECallbackState.Cancelled)
                return;

            if (d.TypeNew == Item.ItemIndex && d.TypeOld == BuiltinBlocks.Air)
            {
                if (World.TryGetTypeAt(d.Position.Add(0, -1, 0), out ushort itemBelow))
                {
                    ActiveBlockManager.RegisterBlockState(d.RequestedByPlayer, new ActiveBlockManager.BlockState(d.Position, d.RequestedByPlayer, nameof(ChickenCoop)));
                    return;
                }

                Chat.Send(d.RequestedByPlayer, "The mining machine must be placed on stone or ore.", ChatColor.orange);
                d.CallbackState = ModLoader.OnTryChangeBlockData.ECallbackState.Cancelled;
                return;
            }
        }
    }
}
