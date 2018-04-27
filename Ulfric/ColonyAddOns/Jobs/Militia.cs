using BlockTypes.Builtin;
using Pipliz;
using Server.Monsters;
using Server.NPCs;
using System.Collections.Generic;
using NPC;
using Pipliz.Collections;
using Pipliz.Mods.APIProvider.Jobs;
using Pipliz.JSON;
using UnityEngine;

namespace Ulfric.ColonyAddOns
{
    [ModLoader.ModManager]
    public class MilitiaJob : Job, INPCTypeDefiner
    {
        protected ushort blockType;
        protected double RecruitedTime = 0;

        static NPCTypeStandardSettings MilitiaNPCSettings = new NPCTypeStandardSettings()
        {
            type = NPCTypeID.GetNextID(),
            keyName = GameLoader.NAMESPACE + ".Militia",
            printName = "Militia",
            maskColor0 = UnityEngine.Color.red,
            maskColor1 = UnityEngine.Color.magenta
        };
        public static NPCType MilitiaNPCType;

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".MilitiaJob.Init"),
            ModLoader.ModCallbackProvidesFor("pipliz.apiprovider.jobs.resolvetypes"),
            ModLoader.ModCallbackDependsOn("pipliz.blocknpcs.registerjobs")]
        public static void Init()
        {
            Logger.Log("Init Militia Job");
            NPCType.AddSettings(MilitiaNPCSettings);
            MilitiaNPCType = NPCType.GetByKeyNameOrDefault(MilitiaNPCSettings.keyName);
            
        }

        static GuardBaseJob.GuardSettings guardsettings = GetGuardSettings();
        IMonster target;

        public override bool ToSleep => false;

        public override NPCType NPCType { get { return MilitiaNPCType; } }

        public override Vector3Int GetJobLocation()
        {
             return position;
        }

        public bool HasTarget { get { return target != null && target.IsValid; } }

        public virtual void OnShoot()
        {
            if (guardsettings.OnShootAudio != null)
            {
                ServerManager.SendAudio(position.Vector, guardsettings.OnShootAudio);
            }
            if (guardsettings.OnHitAudio != null)
            {
                ServerManager.SendAudio(target.PositionToAimFor, guardsettings.OnHitAudio);
            }
            target.OnHit(guardsettings.shootDamage, usedNPC, ModLoader.OnHitData.EHitSourceType.NPC);
        }

        public virtual void ShootAtTarget(ref NPCBase.NPCState state)
        {
            if (Stockpile.GetStockPile(owner).TryRemove(guardsettings.shootItem))
            {
                OnShoot();
                state.SetIndicator(new Shared.IndicatorState(guardsettings.cooldownShot, guardsettings.shootItem[0].Type));
            }
            else
            {
                state.SetIndicator(new Shared.IndicatorState(guardsettings.cooldownMissingItem, guardsettings.shootItem[0].Type, true, false));
            }
        }

        public override void OnNPCAtJob(ref NPCBase.NPCState state)
        {
            if (TimeCycle.TotalTime > RecruitedTime + Configuration.MititiaTermOfDuty)
            {

                OnRemovedNPC();

            }
            if (HasTarget)
            {
                Vector3 npcPos = position.Add(0, 1, 0).Vector;
                Vector3 targetPos = target.PositionToAimFor;
                if (General.Physics.Physics.CanSee(npcPos, targetPos))
                {
                    usedNPC.LookAt(targetPos);
                    ShootAtTarget(ref state); // <- sets cooldown
                    return;
                }
                else
                {
                    target = null;
                }
            }
            target = MonsterTracker.Find(position.Add(0, 1, 0), guardsettings.range, guardsettings.shootDamage);
            if (HasTarget)
            {
                usedNPC.LookAt(target.PositionToAimFor);
                ShootAtTarget(ref state); // <- sets cooldown
            }
            else
            {
                state.SetCooldown(guardsettings.cooldownSearchingTarget);
                Vector3 pos = usedNPC.Position.Vector;
                if (blockType == guardsettings.typeXP)
                {
                    pos += Vector3.right;
                }
                else if (blockType == guardsettings.typeXN)
                {
                    pos += Vector3.left;
                }
                else if (blockType == guardsettings.typeZP)
                {
                    pos += Vector3.forward;
                }
                else if (blockType == guardsettings.typeZN)
                {
                    pos += Vector3.back;
                }
                usedNPC.LookAt(pos);
            }
        }

        public override bool NeedsItems => guardsettings == null;

        public override Vector3Int KeyLocation => position;

        public override NPCBase.NPCGoal CalculateGoal(ref NPCBase.NPCState state)
        {
            return NPCBase.NPCGoal.Job;
        }

        new public void InitializeJob(Players.Player owner, Vector3Int position, int desiredNPCID)
        {
            this.position = position;
            this.owner = owner;
            if (desiredNPCID != 0 && NPCTracker.TryGetNPC(desiredNPCID, out usedNPC))
            {
                 usedNPC.TakeJob(this);
            }
            else
            {
                desiredNPCID = 0;
            }

        }

        public static GuardBaseJob.GuardSettings GetGuardSettings()
        {
            if (guardsettings == null)
            {
                GuardBaseJob.GuardSettings set = new GuardBaseJob.GuardSettings();
                set.cooldownMissingItem = 1.5f;
                set.cooldownSearchingTarget = 0.5f;
                set.cooldownShot = 3f;
                set.range = 12;
                set.recruitmentItem = new InventoryItem(BuiltinBlocks.Sling);
                set.shootItem = new List<InventoryItem>() { new InventoryItem(BuiltinBlocks.SlingBullet) };
                set.shootDamage = 25f;
                set.sleepSafetyPeriod = 1f;
                set.typeXN = ItemTypes.IndexLookup.GetIndex("Ulfric.ColonyAddOns.Blocks.MilitiaJobx-");
                set.typeXP = ItemTypes.IndexLookup.GetIndex("Ulfric.ColonyAddOns.Blocks.MilitiaJobx+");
                set.typeZN = ItemTypes.IndexLookup.GetIndex("Ulfric.ColonyAddOns.Blocks.MilitiaJobz-");
                set.typeZP = ItemTypes.IndexLookup.GetIndex("Ulfric.ColonyAddOns.Blocks.MilitiaJobz+");
                set.OnShootAudio = "sling";
                set.OnHitAudio = "fleshHit";
                guardsettings = set;
            }
            return guardsettings;
        }

        public GuardBaseJob.GuardSettings SetupSettings()
        {
            return GetGuardSettings();
        }

        public override ITrackableBlock InitializeFromJSON(Players.Player player, JSONNode node)
        {
            RecruitedTime = node.GetAs<double>("RecruitedTime");
            InitializeJob(player, (Vector3Int)node[nameof(position)], node.GetAs<int>("npcID"));
            return this;
        }
        public override JSONNode GetJSON()
        {
            JSONNode node = base.GetJSON();
            node.SetAs("type", MilitiaNPCSettings.KeyName);
            node.SetAs<double>("RecruitedTime", RecruitedTime);
            node.SetAs(nameof(position), (JSONNode)position);

            return node;
        }

        public NPCTypeStandardSettings GetNPCTypeDefinition()
        {
            return MilitiaNPCSettings;
        }

        public override bool IsValid => base.IsValid;

        public override void OnAssignedNPC(NPCBase npc)
        {
            RecruitedTime = TimeCycle.TotalTime;
            NPC = npc;
        }

        public override void OnRemovedNPC()
        {
            usedNPC.ClearJob();
            usedNPC = null;
            JobTracker.Remove(owner, position);
        }

        public override void OnRemove()
        {
            isValid = false;
            if (usedNPC != null)
            {
                ColonyManager.RemoveJobs(this);
                usedNPC.ClearJob();
                usedNPC = null;
            }
        }
    }

}
