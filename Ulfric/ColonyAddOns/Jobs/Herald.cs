using Pipliz.Mods.APIProvider.Jobs;
using Server.NPCs;
using UnityEngine;
using System.Collections.Generic;
using NPC;
using Pipliz;
using Pipliz.JSON;
using Server.Monsters;
using System;
using Pipliz.Collections;

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
            Logger.Log("Registering Job..." + JOB_NAME + " and Item Key " + JOB_ITEM_KEY);
            BlockJobManagerTracker.Register<HeraldJob>(JOB_ITEM_KEY);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterSelectedWorld, GameLoader.NAMESPACE + ".Jobs.Hearld.RegisterAudio"),
        ModLoader.ModCallbackProvidesFor("pipliz.server.loadaudiofiles"), ModLoader.ModCallbackDependsOn("pipliz.server.registeraudiofiles")]
        public static void RegisterAudio()
        {
            try
            {
                Logger.Log("Registering Audio...{0}", GameLoader.NAMESPACE + ".NightAudio");
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
            catch (System.Exception e)
            {
                Logger.Log("(0).RegisterAudio had an error : {1}", HeraldRegister.MOD_NAMESPACE, e.Message);
            }
        }
    }

    public class HeraldJob : BlockJobBase, IBlockJobBase, INPCTypeDefiner
    {
        Vector3Int originalPosition;
        Players.Player player;
        double lastNightTrumpeted = 0;
        double lastDayTrumpeted = 0;
        double lastRally = 0;

        List<int> MilitiaList = new List<int>();

        private static NPCTypeStandardSettings _type = new NPCTypeStandardSettings
        {
            keyName = HeraldRegister.JOB_NAME,
            printName = "Herald",
            maskColor1 = new Color32(160, 13, 199, 1),
            type = NPCTypeID.GetNextID(),
            inventoryCapacity = 1f
        };

        public static float StaticCraftingCooldown = 10f;
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
            this.player = player;
            lastRally = node.GetAsOrDefault<double>(nameof(lastRally),0);
            InitializeJob(player, (Vector3Int)node["position"], node.GetAs<int>("npcID"));
            lastDayTrumpeted = node.GetAsOrDefault<double>(nameof(lastDayTrumpeted), TimeCycle.TotalTime);
            lastNightTrumpeted = node.GetAsOrDefault<double>(nameof(lastNightTrumpeted),TimeCycle.TotalTime);

            return this;
        }

        public ITrackableBlock InitializeOnAdd(Vector3Int position, ushort type, Players.Player player)
        {
            originalPosition = position;
            this.player = player;
            lastRally = TimeCycle.TotalTime - Configuration.MilitiaRallyCooldown;
            InitializeJob(player, position, 0);
            lastDayTrumpeted = TimeCycle.TotalTime;
            lastNightTrumpeted = TimeCycle.TotalTime;

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

            if (PlayerState.GetPlayerState(player).EnableHeraldWarning )
            {
                IMonster monster = MonsterTracker.Find(originalPosition.Add(0, 1, 0), Configuration.HeraldWarningDistance, 500.0f);

                if (monster == null)
                    monster = MonsterTracker.Find(originalPosition.Add(1, 0, 0), Configuration.HeraldWarningDistance, 500.0f);

                if (monster == null)
                    monster = MonsterTracker.Find(originalPosition.Add(-1, 0, 0), Configuration.HeraldWarningDistance, 500.0f);

                if (monster == null)
                    monster = MonsterTracker.Find(originalPosition.Add(0, -1, 0), Configuration.HeraldWarningDistance, 500.0f);

                if (monster == null)
                    monster = MonsterTracker.Find(originalPosition.Add(0, 0, 1), Configuration.HeraldWarningDistance, 500.0f);

                if (monster == null)
                    monster = MonsterTracker.Find(originalPosition.Add(0, 0, -1), Configuration.HeraldWarningDistance, 500.0f);

                if (monster != null)
                {
                    if (General.Physics.Physics.CanSee(originalPosition.Add(0, 1, 0).Vector, monster.PositionToAimFor) && TimeCycle.TotalTime >= lastRally + Configuration.MilitiaRallyCooldown)
                    {

                        ServerManager.SendAudio(owner.Position, GameLoader.NAMESPACE + ".Rally");
                        status = GameLoader.Trumpeting_Icon;

                        if (Configuration.AllowMilitiaToBeCalled && MilitiaList.Count == 0 && PlayerState.GetPlayerState(player).EnableMilitia)
                        {
                            lastRally = TimeCycle.TotalTime;
                            ActivateMilitia(player, originalPosition);
                        }

                    }
                }
            }
            if (TimeCycle.IsDay)
            {
                if (PlayerState.GetPlayerState(player).EnableHeraldAnnouncingSunrise)
                {
                    
                    if (TimeCycle.TimeOfDay > TimeCycle.SunRise && TimeCycle.TimeOfDay < TimeCycle.SunRise + 1 && TimeCycle.TotalTime > lastDayTrumpeted + 1)
                    {
                        Logger.Log("SunRise trumpeted at {0} the Last Day Trumpeted at {1}", TimeCycle.TotalTime, lastDayTrumpeted);
                        lastDayTrumpeted = TimeCycle.TotalTime + TimeCycle.DayLength;
                        ServerManager.SendAudio(owner.Position, GameLoader.NAMESPACE + ".DayAudio");
                        cooldown = 20f;
                        status = GameLoader.Trumpeting_Icon;

                    }
                }
            }
            if (!TimeCycle.IsDay)
            {
                if (PlayerState.GetPlayerState(player).EnableHeraldAnnouncingSunset)
                {
                    if (TimeCycle.TimeOfDay > TimeCycle.SunSet && TimeCycle.TimeOfDay < TimeCycle.SunSet + 1 && TimeCycle.TotalTime > lastNightTrumpeted + 1)
                    {
                        Logger.Log("SunSet trumpeted at {0} the Last Night Trumpeted at {1}", TimeCycle.TotalTime, lastNightTrumpeted);
                        lastNightTrumpeted = TimeCycle.TotalTime + TimeCycle.DayLength;
                        ServerManager.SendAudio(owner.Position, GameLoader.NAMESPACE + ".NightAudio");
                        cooldown = 20f;
                        status = GameLoader.Trumpeting_Icon;

                    }
                }
            }

            state.SetIndicator(new Shared.IndicatorState(cooldown, status));

        }

        public override JSONNode GetJSON()
        {
            JSONNode newJSON = base.GetJSON();
            newJSON.SetAs(nameof(originalPosition), (JSONNode)originalPosition);
            newJSON.SetAs(nameof(lastRally), lastRally);
            newJSON.SetAs(nameof(lastDayTrumpeted), lastDayTrumpeted);
            newJSON.SetAs(nameof(lastNightTrumpeted), lastNightTrumpeted);

            return newJSON;
        }

        private void ActivateMilitia(Players.Player player, Vector3Int heraldPosition)
        {
            //Determine open spaces around herald to determine the number of militia that will be called.
            List<Vector3Int> openspot = new List<Vector3Int>();

            if (IsSpacesEmpty(heraldPosition, 1, 1)) openspot.Add(heraldPosition.Add(1, 0, 1));
            if (IsSpacesEmpty(heraldPosition, 1, -1)) openspot.Add(heraldPosition.Add(1, 0, -1));
            if (IsSpacesEmpty(heraldPosition, -1, 1)) openspot.Add(heraldPosition.Add(-1, 0, 1));
            if (IsSpacesEmpty(heraldPosition, -1, -1)) openspot.Add(heraldPosition.Add(-1, 0, -1));

            Logger.Log("Open Spot count: {0}", openspot.Count);

            //Get all Laborers available and assign them to Milita first
            List<NPCBase> laborers = Colony.Get(player).Followers.FindAll(x => x.NPCType.IsLaborer);
            Logger.Log("Laborers Count : {0}", laborers.Count);
            if (openspot.Count<4)
            {
                Chat.Send(player,"One or more corners of Herald Stand is blocked.", ChatColor.red);
            }
            if (openspot.Count > 0)
            {
                foreach (NPCBase follower in Colony.Get(player).Followers)
                {
                    if (openspot.Count > 0)
                    {
                        if (AssignLaborerFirst(follower,openspot[0]))
                        {
                            openspot.RemoveAt(0);
                            Logger.Log("Follower {0} set to {1}", follower.ID, follower.NPCType.ToString());
                        }
                    }
                }
                
                if (openspot.Count > 0)
                {
                    foreach (NPCBase follower in Colony.Get(player).Followers)
                    {
                        if (openspot.Count > 0)
                        {
                            if (AssignNonExcludedFollower(follower, openspot[0]))
                            {
                                openspot.RemoveAt(0);
                                Logger.Log("Follower {0} set to {1}", follower.ID, follower.NPCType.ToString());
                            }
                        }
                    }
                }
            }
            if (openspot.Count > 0)
            {
                Chat.Send(player, "Not enough colonists to fill all Milita positions!", ChatColor.red);
            }
        }

        public bool AssignLaborerFirst(NPCBase follower,Vector3Int position)
        {
            if (follower.NPCType.IsLaborer)
            {
                var militiaJob = new MilitiaJob();
                militiaJob.InitializeJob(player, position, follower.ID);
                militiaJob.OnAssignedNPC(follower);
                follower.TakeJob(militiaJob);
                ColonyManager.AddJobs(militiaJob);
                return true;
            }
            return false;
        }

        public bool AssignNonExcludedFollower(NPCBase follower, Vector3Int position)
        {
            var job = follower.Job;

            if (!IsJobExcluded(job))
            {
                try
                {
                    if (follower.Job != null)
                    {
                        follower.Job.NPC = null;
                        follower.ClearJob();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log("ActivateMilitia Error has occurred: {0}", ex.Message);
                }

                var militiaJob = new MilitiaJob();
                militiaJob.InitializeJob(player, position, follower.ID);
                militiaJob.OnAssignedNPC(follower);
                follower.TakeJob(militiaJob);
                ColonyManager.AddJobs(militiaJob);
                return true;
            }

            return false;
        }

        private bool IsSpacesEmpty(Vector3Int position, int x, int z)
        {
            World.TryIsSolid(position.Add(x, 0, z), out bool result1);
            World.TryIsSolid(position.Add(x, 1, z), out bool result2);

            if (result1 || result2)
                return false;
            else
                return true;
        }

        private bool IsJobExcluded(IJob jobname)
        {
            bool returnvalue = false;

            if (jobname != null)
            {
                Logger.Log("Is {0} excluded for Militia duty? {1}", jobname.NPCType.ToString() == null ? "": jobname.NPCType.ToString(), Configuration.ExcludeJobTypes.Contains(jobname.NPCType.ToString() == null ? "": jobname.NPCType.ToString()));
                returnvalue = Configuration.ExcludeJobTypes.Contains(jobname.NPCType.ToString());
            }
            return returnvalue;
        }
    }
}
