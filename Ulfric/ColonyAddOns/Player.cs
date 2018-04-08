using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlockTypes.Builtin;
using Pipliz.JSON;

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

    [ModLoader.ModManager]
    public class PlayerState
    {

        static Dictionary<Players.Player, PlayerState> _playerStates = new Dictionary<Players.Player, PlayerState>();

        public bool BossesEnabled { get; set; } = true;

        public Players.Player Player { get; private set; }

        public bool EnableHeraldAnnouncingSunrise { get; set; } = true;

        public bool EnableHeraldAnnouncingSunset { get; set; } = true;

        public bool EnableHeraldWarning { get; set; } = true;


        public PlayerState(Players.Player p)
        {
            Player = p;
        }

        public static PlayerState GetPlayerState(Players.Player p)
        {
            if (p != null)
            {
                if (!_playerStates.ContainsKey(p))
                    _playerStates.Add(p, new PlayerState(p));

                return _playerStates[p];
            }

            return null;
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnLoadingPlayer, GameLoader.NAMESPACE + ".Entities.PlayerState.OnLoadingPlayer")]
        public static void OnLoadingPlayer(JSONNode n, Players.Player p)
        {
            if (!_playerStates.ContainsKey(p))
                _playerStates.Add(p, new PlayerState(p));

            if (n.TryGetChild(GameLoader.NAMESPACE + ".PlayerState", out var stateNode))
            {
 
                if (stateNode.TryGetAs("EnableHeraldAnnouncingSunrise", out bool bEnableHeraldAnnouncingSunrise))
                    _playerStates[p].EnableHeraldAnnouncingSunrise = bEnableHeraldAnnouncingSunrise;

                if (stateNode.TryGetAs("EnableHeraldAnnouncingSunset", out bool bEnableHeraldAnnouncingSunset))
                    _playerStates[p].EnableHeraldAnnouncingSunrise = bEnableHeraldAnnouncingSunset;

                if (stateNode.TryGetAs("EnableHeraldWarning", out bool bEnableHeraldWarning))
                    _playerStates[p].EnableHeraldWarning = bEnableHeraldWarning;

            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnSavingPlayer, GameLoader.NAMESPACE + ".Entities.PlayerState.OnSavingPlayer")]
        public static void OnSavingPlayer(JSONNode n, Players.Player p)
        {
            if (_playerStates.ContainsKey(p))
            {
                var node = new JSONNode();

                node.SetAs("EnableHeraldAnnouncingSunrise", _playerStates[p].EnableHeraldAnnouncingSunrise);
                node.SetAs("EnableHeraldAnnouncingSunset", _playerStates[p].EnableHeraldAnnouncingSunset);
                node.SetAs("EnableHeraldWarning", _playerStates[p].EnableHeraldWarning);

                n.SetAs(GameLoader.NAMESPACE + ".PlayerState", node);
            }
        }
    }
}

