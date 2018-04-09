using Pipliz.Mods.APIProvider.Jobs;
using Server.NPCs;
using UnityEngine;
using System.Collections.Generic;
using NPC;
using Pipliz;
using Pipliz.JSON;
using Server.Monsters;

namespace Ulfric.ColonyAddOns.Jobs
{
    [ModLoader.ModManager]
    public static class HeraldRegister
    {
        public static string JOB_NAME = GameLoader.NAMESPACE + ".Herald";
        public static string JOB_ITEM_KEY = GameLoader.NAMESPACE + ".Blocks.HeraldStand";
        public static string JOB_RECIPE = JOB_ITEM_KEY + ".recipe";
        public const string MOD_NAMESPACE = GameLoader.NAMESPACE + ".HeraldRegister";

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, HeraldRegister.MOD_NAMESPACE + ".HeraldRegister.RegisterJobs")]
        [ModLoader.ModCallbackProvidesFor("pipliz.apiprovider.jobs.resolvetypes")]
        public static void RegisterJobs()
        {
            Logger.Log("Registering Job..." + JOB_NAME + " and Item Key " +JOB_ITEM_KEY);
            BlockJobManagerTracker.Register<HeraldJob>(JOB_ITEM_KEY);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterSelectedWorld, GameLoader.NAMESPACE + ".Jobs.Hearld.RegisterAudio"),
        ModLoader.ModCallbackProvidesFor("pipliz.server.loadaudiofiles"), ModLoader.ModCallbackDependsOn("pipliz.server.registeraudiofiles")]
        public static void RegisterAudio()
        {
            Logger.Log("Registering Audio...{0}",GameLoader.NAMESPACE + ".NightAudio");
            GameLoader.AddSoundFile(GameLoader.NAMESPACE + ".NightAudio", new List<string>()
            {
                GameLoader.AudioFolder + "/Taps.ogg"
            });

            Logger.Log("Registering Audio...{0}", GameLoader.NAMESPACE + ".DayAudio");
            GameLoader.AddSoundFile(GameLoader.NAMESPACE + ".DayAudio", new List<string>()
            {
                GameLoader.AudioFolder + "/Reveille.ogg"
            });

            Logger.Log("Registering Audio...{0}", GameLoader.NAMESPACE + ".Rally");
            GameLoader.AddSoundFile(GameLoader.NAMESPACE + ".Rally", new List<string>()
            {
                GameLoader.AudioFolder + "/Rally.ogg"
            });

        }
    }

    public class HeraldJob : BlockJobBase, IBlockJobBase, INPCTypeDefiner
    {
        Vector3Int originalPosition;
        Players.Player _player;
        static bool DayAlarmed = false;
        static bool NightAlarmed = false;
        

        private static NPCTypeStandardSettings _type = new NPCTypeStandardSettings
        {
            keyName =HeraldRegister.JOB_NAME,
            printName = "Herald",
            maskColor1 = new Color32(59, 55, 0, 255),
            type = NPCTypeID.GetNextID(),
            inventoryCapacity = 1f
        };

        public static float StaticCraftingCooldown =10f;
        public override bool ToSleep => false;

        public override string NPCTypeKey
        {
            get
            {
                return HeraldRegister.JOB_NAME;
            }
        }

        public override ITrackableBlock InitializeFromJSON(Players.Player player, JSONNode node)
        {
            originalPosition = (Vector3Int)node[nameof(originalPosition)];
            _player = player;
            InitializeJob(player, (Vector3Int)node["position"], node.GetAs<int>("npcID"));
            return this;
        }

        public ITrackableBlock InitializeOnAdd(Vector3Int position, ushort type, Players.Player player)
        {
            originalPosition = position;
            InitializeJob(player, position, 0);
            return this;
        }

        public override NPCBase.NPCGoal CalculateGoal(ref NPCBase.NPCState state)
        {
            return NPCBase.NPCGoal.Job;
        }

        NPCTypeStandardSettings INPCTypeDefiner.GetNPCTypeDefinition()
        {
            return _type;
        }

        public override void OnNPCAtJob(ref NPCBase.NPCState state)
        {

            float cooldown = HeraldJob.StaticCraftingCooldown;
            ushort status = GameLoader.Waiting_Icon;

            if (!TimeCycle.ShouldSleep)
            {
                if (!DayAlarmed && PlayerState.GetPlayerState(_player).EnableHeraldAnnouncingSunrise)
                {
                    ServerManager.SendAudio(owner.Position, GameLoader.NAMESPACE + ".DayAudio");
                    status = GameLoader.Trumpeting_Icon;
                    DayAlarmed = true;
                    NightAlarmed = false;
                }
            }

            if (TimeCycle.ShouldSleep)
            {
                if (!NightAlarmed && PlayerState.GetPlayerState(_player).EnableHeraldAnnouncingSunset)
                {
                    ServerManager.SendAudio(owner.Position, GameLoader.NAMESPACE + ".NightAudio");
                    status = GameLoader.Trumpeting_Icon;
                    DayAlarmed = false;
                    NightAlarmed = true;
                }
            }

            if (PlayerState.GetPlayerState(_player).EnableHeraldWarning)
            {

                IMonster monster = MonsterTracker.Find(originalPosition.Add(0, 1, 0), Configuration.HeraldWarningDistance, 100000.0f);

                if (monster == null)
                    monster = MonsterTracker.Find(originalPosition.Add(1, 0, 0), Configuration.HeraldWarningDistance, 100000.0f);

                if (monster == null)
                    monster = MonsterTracker.Find(originalPosition.Add(-1, 0, 0), Configuration.HeraldWarningDistance, 100000.0f);

                if (monster == null)
                    monster = MonsterTracker.Find(originalPosition.Add(0, -1, 0), Configuration.HeraldWarningDistance, 100000.0f);

                if (monster == null)
                    monster = MonsterTracker.Find(originalPosition.Add(0, 0, 1), Configuration.HeraldWarningDistance, 100000.0f);

                if (monster == null)
                    monster = MonsterTracker.Find(originalPosition.Add(0, 0, -1), Configuration.HeraldWarningDistance, 100000.0f);

                if (monster != null && General.Physics.Physics.CanSee(originalPosition.Add(0,1,0).Vector, monster.Position))
                {
                    ServerManager.SendAudio(owner.Position, GameLoader.NAMESPACE + ".Rally");
                    status = GameLoader.Trumpeting_Icon;
                }
            }

            state.SetIndicator(new Shared.IndicatorState(cooldown, status));
          
        }

        public override JSONNode GetJSON()
        {
            return base.GetJSON().SetAs(nameof(originalPosition), (JSONNode)originalPosition);
        }


    }
}
