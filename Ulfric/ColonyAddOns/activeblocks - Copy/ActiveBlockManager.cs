using System;
using System.Collections.Generic;
using Pipliz;
using Pipliz.JSON;
using BlockTypes.Builtin;

namespace Ulfric.ColonyAddOns.ActiveBlocks
{

    [ModLoader.ModManager]
    public static class ActiveBlockManager
    {
        public class BlockState
        {
            public Vector3Int Position { get; private set; } = Vector3Int.invalidPos;
            public ushort BlockType { get; private set; }
            public Players.Player Owner { get; private set; }
            public IBlockSetting BlockSetting { get; private set; }
            public double NextTimeForWork { get; set; } = Time.SecondsSinceStartDouble + Pipliz.Random.NextDouble(0, 5);

            public BlockState(Vector3Int pos, Players.Player owner, string blocktype)
            {
                Position = pos;
                Owner = owner;
                BlockType = ItemTypes.IndexLookup.GetIndex(blocktype);

                BlockSetting = ActiveBlockManager.GetCallbacks(blocktype);
            }

            public BlockState(JSONNode node, Players.Player owner)
            {
                Position = (Vector3Int)node[nameof(Position)];
                Owner = owner;
                BlockType = node.GetAs<ushort>(nameof(BlockType));
            }

            public virtual JSONNode GetJSON()
            {
                JSONNode node = new JSONNode();
                node.SetAs(nameof(Position),(JSONNode)Position);
                node.SetAs(nameof(BlockType), BlockType);

                return node;
            }
        }

        public interface IBlockSetting
        {
            Action<Players.Player, BlockState> DoWork { get; set; }
        }

        public class BlockSetting : IBlockSetting
        {
            public ushort ItemIndex { get; set; }
            public Action<Players.Player, BlockState> DoWork { get; set; }

            public BlockSetting(ushort itemindex, Action<Players.Player, BlockState> dowork)
            {
                ItemIndex = itemindex;
                DoWork = dowork;
            }

        }

        private const int BLOCK_REFRESH = 1;
        public static Vector3Int Position { get; private set; } = Vector3Int.invalidPos;
        public static Dictionary<string, IBlockSetting> BlockCallbacks = new Dictionary<string, IBlockSetting>(StringComparer.OrdinalIgnoreCase);
        public static Dictionary<Players.Player, Dictionary<Vector3Int, BlockState>> ActiveBlocks { get; private set; } = new Dictionary<Players.Player, Dictionary<Vector3Int, BlockState>>();
        private static double nextUpdate = 0;
        public static event EventHandler ActiveBlockRemoved;

        public static bool RegisterBlockType(string machineType, IBlockSetting callback)
        {
            BlockCallbacks[machineType] = callback;
            return true;
        }

        public static void RegisterBlockState(Players.Player player, BlockState state)
        {
            lock (ActiveBlocks)
            {
                if (!ActiveBlocks.ContainsKey(player))
                    ActiveBlocks.Add(player, new Dictionary<Vector3Int, BlockState>());

                ActiveBlocks[player][state.Position] = state;
            }
        }

        public static IBlockSetting GetCallbacks(string machineType)
        {
            if (BlockCallbacks.ContainsKey(machineType))
                return BlockCallbacks[machineType];
            else
            {
                return null;
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnUpdate, GameLoader.NAMESPACE + ".ActiveBlocks.ActiveBlockManager.OnUpdate")]
        public static void OnUpdate()
        {
            if (GameLoader.WorldLoaded && nextUpdate < Pipliz.Time.SecondsSinceStartDouble)
            {
                lock (ActiveBlocks)
                    foreach (var block in ActiveBlocks)
                        if ((!block.Key.IsConnected && Configuration.OfflineColonies) || block.Key.IsConnected)
                            foreach (var state in block.Value)
                                try
                                {
                                    state.Value.BlockSetting.DoWork(block.Key, state.Value);

                                    if (state.Value.Load <= 0)
                                        Server.Indicator.SendIconIndicatorNear(state.Value.Position.Add(0, 1, 0).Vector, new Shared.IndicatorState(BLOCK_REFRESH, GameLoader.Reload_Icon, true, false));

                                    if (state.Value.Durability <= 0)
                                        Server.Indicator.SendIconIndicatorNear(state.Value.Position.Add(0, 1, 0).Vector, new Shared.IndicatorState(BLOCK_REFRESH, GameLoader.Repairing_Icon, true, false));

                                    if (state.Value.Fuel <= 0)
                                        Server.Indicator.SendIconIndicatorNear(state.Value.Position.Add(0, 1, 0).Vector, new Shared.IndicatorState(BLOCK_REFRESH, GameLoader.Refuel_Icon, true, false));

                                }
                                catch (Exception ex)
                                {
                                    Logger.LogError(ex);
                                }

                nextUpdate = Pipliz.Time.SecondsSinceStartDouble + BLOCK_REFRESH;
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnLoadingPlayer, GameLoader.NAMESPACE + ".ActiveBlocks.ActiveBlockManager.OnLoadingPlayer")]
        public static void OnLoadingPlayer(JSONNode n, Players.Player p)
        {
            if (n.TryGetChild(GameLoader.NAMESPACE + ".ActiveBlocks", out var machinesNode))
            {
                lock (ActiveBlocks)
                {
                    foreach (var node in machinesNode.LoopArray())
                        RegisterBlockState(p, new BlockState(node, p));

                    if (ActiveBlocks.ContainsKey(p))
                        Logger.Log(ChatColor.lime, $"{ActiveBlocks[p].Count} machines loaded from save for {p.ID.steamID.m_SteamID}!");
                    else
                        Logger.Log(ChatColor.lime, $"No machines found in save for {p.ID.steamID.m_SteamID}.");
                }
            }
            else
                Logger.Log(ChatColor.lime, $"No machines found in save for {p.ID.steamID.m_SteamID}.");
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnSavingPlayer, GameLoader.NAMESPACE + ".ActiveBlocks.ActiveBlockManager.OnSavingPlayer")]
        public static void OnSavingPlayer(JSONNode n, Players.Player p)
        {
            lock (ActiveBlocks)
                if (ActiveBlocks.ContainsKey(p))
                {
                    if (n.HasChild(GameLoader.NAMESPACE + ".ActiveBlocks"))
                        n.RemoveChild(GameLoader.NAMESPACE + ".ActiveBlocks");

                    var machineNode = new JSONNode(NodeType.Array);

                    foreach (var node in ActiveBlocks[p])
                        machineNode.AddToArray(node.Value.GetJSON());

                    n[GameLoader.NAMESPACE + ".ActiveBlocks"] = machineNode;
                }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnTryChangeBlock, GameLoader.NAMESPACE + ".ActiveBlocks.ActiveBlockManager.OnTryChangeBlockUser")]
        public static void OnTryChangeBlockUser(ModLoader.OnTryChangeBlockData d)
        {
            if (d.CallbackState == ModLoader.OnTryChangeBlockData.ECallbackState.Cancelled)
                return;

            if (d.TypeNew == BuiltinBlocks.Air && d.RequestedByPlayer != null)
                RemoveMachine(d.RequestedByPlayer, d.Position);
        }

        public static void RemoveMachine(Players.Player p, Vector3Int pos, bool throwEvent = true)
        {
            lock (ActiveBlocks)
            {
                if (!ActiveBlocks.ContainsKey(p))
                    ActiveBlocks.Add(p, new Dictionary<Vector3Int, BlockState>());


                if (ActiveBlocks[p].ContainsKey(pos))
                {
                    var mach = ActiveBlocks[p][pos];

                    ActiveBlocks[p].Remove(pos);

                    if (throwEvent && ActiveBlockRemoved != null)
                        ActiveBlockRemoved(mach, new EventArgs());
                }
            }
        }

    }
}

