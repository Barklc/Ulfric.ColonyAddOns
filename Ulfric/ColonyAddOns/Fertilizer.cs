using System.Collections.Generic;
using Pipliz;
using NPC;
using BlockTypes.Builtin;

namespace Ulfric.ColonyAddOns
{
    [ModLoader.ModManager]
    public class Fertilizer
    {
        // Location of MOD .DLL file at runtime.
        private const string MOD_NAMESPACE = GameLoader.NAMESPACE + ".Fertilizer";
        private const string BLOCKS_NAMESPACE = GameLoader.NAMESPACE + ".Blocks";

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnNPCGathered, Fertilizer.MOD_NAMESPACE + ".OnNPCGathered")]
        void OnNPCGathered(IJob job, Vector3Int location, List<ItemTypes.ItemTypeDrops> results)
        {
            if (job.NPCType.Equals(Server.NPCs.NPCType.GetByKeyNameOrDefault("pipliz.wheatfarm")))
            {
                Vector3Int position = location.Add(0,-1,0);
                if (ServerManager.TryChangeBlock(position, ItemTypes.IndexLookup.GetIndex(BLOCKS_NAMESPACE + ".FineSoil")))
                {
                    results.Add(new ItemTypes.ItemTypeDrops(BuiltinBlocks.Wheat, 1, 0.5));
                    return;
                }
            }
            if (job.NPCType.Equals(Server.NPCs.NPCType.GetByKeyNameOrDefault("pipliz.alkanetfarmer")))
            {
                Vector3Int position = location.Add(0, -1, 0);
                if (ServerManager.TryChangeBlock(position, ItemTypes.IndexLookup.GetIndex(BLOCKS_NAMESPACE + ".FineSoil")))
                {
                    results.Add(new ItemTypes.ItemTypeDrops(BuiltinBlocks.Alkanet, 1, 0.5));
                    return;
                }
            }
            if (job.NPCType.Equals(Server.NPCs.NPCType.GetByKeyNameOrDefault("pipliz.hollyhockfarmer")))
            { 
                Vector3Int position = location.Add(0, -1, 0);
                if (ServerManager.TryChangeBlock(position, ItemTypes.IndexLookup.GetIndex(BLOCKS_NAMESPACE + ".FineSoil")))
                {
                    results.Add(new ItemTypes.ItemTypeDrops(BuiltinBlocks.Hollyhock, 1, 0.5));
                    return;
                }
            }
            if (job.NPCType.Equals(Server.NPCs.NPCType.GetByKeyNameOrDefault("pipliz.flaxfarmer")))
            {
                Vector3Int position = location.Add(0, -1, 0);
                if (ServerManager.TryChangeBlock(position, ItemTypes.IndexLookup.GetIndex(BLOCKS_NAMESPACE + ".FineSoil")))
                {
                    results.Add(new ItemTypes.ItemTypeDrops(BuiltinBlocks.Flax, 1, 0.5));
                    return;
                }
            }
            if (job.NPCType.Equals(Server.NPCs.NPCType.GetByKeyNameOrDefault("pipliz.wolfsbanefarm")))
            {
                Vector3Int position = location.Add(0, -1, 0);
                if (ServerManager.TryChangeBlock(position, ItemTypes.IndexLookup.GetIndex(BLOCKS_NAMESPACE + ".FineSoil")))
                {
                    results.Add(new ItemTypes.ItemTypeDrops(BuiltinBlocks.Wolfsbane, 1, 0.5));
                    return;
                }
            }
        }
    }
}
