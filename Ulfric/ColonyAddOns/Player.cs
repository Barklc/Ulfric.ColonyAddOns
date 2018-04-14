using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlockTypes.Builtin;
using Pipliz.JSON;
using Pipliz.Mods.BaseGame.BlockNPCs;
using Pipliz.Mods.APIProvider.Jobs;

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

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerClicked, GameLoader.NAMESPACE + ".PickAxeDig")]
        public static void PickAxeDig(Players.Player player, Pipliz.Box<Shared.PlayerClickedData> boxedData)
        {
            if (boxedData.item1.IsConsumed)
                return;

            var click = boxedData.item1;
            Shared.VoxelRayCastHit rayCastHit = click.rayCastHit;
            var state = PlayerState.GetPlayerState(player);

            ushort tool = click.typeSelected;
            if (tool == ItemTypes.IndexLookup.GetIndex("bronzepickaxe") &&
                click.rayCastHit.rayHitType == Shared.RayHitType.Block &&
                click.clickType == Shared.PlayerClickedData.ClickType.Left)
            {
                long millisecondsSinceStart = Pipliz.Time.MillisecondsSinceStart;

                if (Players.LastPunches.TryGetValue(player, out long num) && millisecondsSinceStart - num < (ItemTypes.GetType(boxedData.item1.typeHit).DestructionTime/2))
                {
                    return;
                }

                Players.LastPunches[player] = millisecondsSinceStart;
                boxedData.item1.consumedType = Shared.PlayerClickedData.ConsumedType.UsedByMod;

                ushort blockhit = boxedData.item1.typeHit;
                ItemTypes.ItemType itemMined = ItemTypes.GetType(blockhit);

                if (itemMined != null && CanMineBlock(itemMined.ItemIndex))
                {
                    List<ItemTypes.ItemTypeDrops> itemList = ItemTypes.GetType(itemMined.ItemIndex).OnRemoveItems;

                    bool itemadd = false;
                    for (int i = 0; i < itemList.Count; i++)
                        if (Pipliz.Random.NextDouble() <= itemList[i].chance)
                            if (Inventory.GetInventory(player).TryAdd(itemList[i].item.Type))
                                itemadd = true;

                    if (itemadd)
                        ServerManager.SendAudio(player.Position, GameLoader.NAMESPACE + ".MiningAudio");
                    else
                        Chat.Send(player, "Item could not be harvested!");
                }
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterSelectedWorld, Player.MOD_NAMESPACE+ ".RegisterAudio"),
        ModLoader.ModCallbackProvidesFor("pipliz.server.loadaudiofiles"), ModLoader.ModCallbackDependsOn("pipliz.server.registeraudiofiles")]
        public static void RegisterAudio()
        {
            Logger.Log("Registering Audio...{0}", GameLoader.NAMESPACE + ".MiningAudio");
            GameLoader.AddSoundFile(GameLoader.NAMESPACE + ".MiningAudio", new List<string>()
            {
                GameLoader.AudioFolder + "/mining.ogg"
            });
        }

        public static bool CanMineBlock(ushort itemMined)
        {
            return ItemTypes.TryGetType(itemMined, out var item) &&
                        item.CustomDataNode.TryGetAs("minerIsMineable", out bool minable) &&
                        minable;
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

