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
        public static void OnNPCGathered(IJob job, Vector3Int location, List<ItemTypes.ItemTypeDrops> results)
        {
            //try
            //{
                if (job.NPCType.Equals(Server.NPCs.NPCType.GetByKeyNameOrDefault("pipliz.wheatfarmer")))
                {
                    Vector3Int position = location.Add(0, -1, 0);
                    if (World.TryGetTypeAt(position, out ushort index) 
                        && ItemTypes.TryGetType(index, out ItemTypes.ItemType itemtype) 
                        && itemtype.CustomDataNode != null 
                        && itemtype.CustomDataNode.TryGetAs<float>("fertilizervalue", out float result))
                    {
                         results.Add(new ItemTypes.ItemTypeDrops(BuiltinBlocks.Wheat, 1, result));
                    }
                    return;
                }
                if (job.NPCType.Equals(Server.NPCs.NPCType.GetByKeyNameOrDefault("pipliz.alkanetfarmer")))
                {
                    Vector3Int position = location.Add(0, -1, 0);
                    if (World.TryGetTypeAt(position, out ushort index)
                        && ItemTypes.TryGetType(index, out ItemTypes.ItemType itemtype)
                        && itemtype.CustomDataNode != null
                        && itemtype.CustomDataNode.TryGetAs<float>("fertilizervalue", out float result))
                {
                        results.Add(new ItemTypes.ItemTypeDrops(BuiltinBlocks.Alkanet, 1, result));
                    }
                    return;
                }
                if (job.NPCType.Equals(Server.NPCs.NPCType.GetByKeyNameOrDefault("pipliz.hollyhockfarmer")))
                {
                    Vector3Int position = location.Add(0, -1, 0);
                    if (World.TryGetTypeAt(position, out ushort index)
                        && ItemTypes.TryGetType(index, out ItemTypes.ItemType itemtype)
                        && itemtype.CustomDataNode != null
                        && itemtype.CustomDataNode.TryGetAs<float>("fertilizervalue", out float result))
                {
                        results.Add(new ItemTypes.ItemTypeDrops(BuiltinBlocks.Hollyhock, 1, result));
                    }
                    return;
                }
                if (job.NPCType.Equals(Server.NPCs.NPCType.GetByKeyNameOrDefault("pipliz.flaxfarmer")))
                {
                    Vector3Int position = location.Add(0, -1, 0);
                    if (World.TryGetTypeAt(position, out ushort index)
                        && ItemTypes.TryGetType(index, out ItemTypes.ItemType itemtype)
                        && itemtype.CustomDataNode != null
                        && itemtype.CustomDataNode.TryGetAs<float>("fertilizervalue", out float result))
                {
                        results.Add(new ItemTypes.ItemTypeDrops(BuiltinBlocks.Flax, 1, result));
                    }
                    return;
                }
                if (job.NPCType.Equals(Server.NPCs.NPCType.GetByKeyNameOrDefault("pipliz.wolfsbanefarm")))
                {
                    Vector3Int position = location.Add(0, -1, 0);
                    if (World.TryGetTypeAt(position, out ushort index)
                        && ItemTypes.TryGetType(index, out ItemTypes.ItemType itemtype)
                        && itemtype.CustomDataNode != null
                        && itemtype.CustomDataNode.TryGetAs<float>("fertilizervalue", out float result))
                    {
                        results.Add(new ItemTypes.ItemTypeDrops(BuiltinBlocks.Wolfsbane, 1, result));
                    }
                    return;
                }
            //}
            //catch (System.Exception e)
            //{
            //    Logger.Log("{0}.OnNPCGathered had an error : {1}", Fertilizer.MOD_NAMESPACE, e.StackTrace);
            //}
        }
    }
}
