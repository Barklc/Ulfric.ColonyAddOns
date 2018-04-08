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
        //Set class variables and constants
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

                    //To keep this from being OP we lower che chance of gettings berries with each pick
                    float chance = (float)random.NextDouble();
                    if (chance <= Configuration.ChanceOfBerriesPerPick)
                    {
                        var inv = Inventory.GetInventory(player);
                        inv.TryAdd(BuiltinBlocks.Berry, Configuration.NumberOfBerriesPerPick);
                    }

                }
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterWorldLoad, MOD_NAMESPACE + ".AfterWorldLoad")]
        public static void AfterWorldLoad()
        {
            //Give players water to start off to support Hydration if enabled
            if (Configuration.AllowDehydration)
            {
                Logger.Log("Add 10 Water buckets to initial stockpile");
                Stockpile.AddToInitialPile(new InventoryItem(Blocks.MOD_NAMESPACE + ".WaterBucket", 10));
            }
        }

        public static List<Players.Player> PlayerList()
        {
            List<Players.Player> players = new List<Players.Player>();
            Players.PlayerDatabase.ForeachValue(x =>
            {
                if (!string.IsNullOrEmpty(x.Name))
                    players.Add(x);
                else if (x.ID.type == NetworkID.IDType.LocalHost)
                    players.Add(x);
            });

            return players;
        }

    }

    [ModLoader.ModManager]
    public class PlayerState
    {

        static Dictionary<Players.Player, PlayerState> _playerStates = new Dictionary<Players.Player, PlayerState>();

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

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnLoadingPlayer, GameLoader.NAMESPACE + ".PlayerState.OnLoadingPlayer")]
        public static void OnLoadingPlayer(JSONNode n, Players.Player p)
        {
            if (!_playerStates.ContainsKey(p))
                _playerStates.Add(p, new PlayerState(p));

            if (n.TryGetChild(GameLoader.NAMESPACE + ".PlayerState", out var stateNode))
            {
 
                //Load the configuration choices for the Herald for the specified player
                if (stateNode.TryGetAs("EnableHeraldAnnouncingSunrise", out bool bEnableHeraldAnnouncingSunrise))
                    _playerStates[p].EnableHeraldAnnouncingSunrise = bEnableHeraldAnnouncingSunrise;

                if (stateNode.TryGetAs("EnableHeraldAnnouncingSunset", out bool bEnableHeraldAnnouncingSunset))
                    _playerStates[p].EnableHeraldAnnouncingSunrise = bEnableHeraldAnnouncingSunset;

                if (stateNode.TryGetAs("EnableHeraldWarning", out bool bEnableHeraldWarning))
                    _playerStates[p].EnableHeraldWarning = bEnableHeraldWarning;

            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnSavingPlayer, GameLoader.NAMESPACE + ".PlayerState.OnSavingPlayer")]
        public static void OnSavingPlayer(JSONNode n, Players.Player p)
        {
            if (_playerStates.ContainsKey(p))
            {
                var node = new JSONNode();

                //Save the configuration choices for the Herald for the specified player
                node.SetAs("EnableHeraldAnnouncingSunrise", _playerStates[p].EnableHeraldAnnouncingSunrise);
                node.SetAs("EnableHeraldAnnouncingSunset", _playerStates[p].EnableHeraldAnnouncingSunset);
                node.SetAs("EnableHeraldWarning", _playerStates[p].EnableHeraldWarning);

                n.SetAs(GameLoader.NAMESPACE + ".PlayerState", node);
            }
        }
    }
}

