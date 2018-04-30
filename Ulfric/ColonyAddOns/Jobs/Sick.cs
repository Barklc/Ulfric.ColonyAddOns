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
    public class SickJob : Job, INPCTypeDefiner
    {
        protected ushort blockType;
        protected double SickTime = 0;
        public Job OldJob = null;

        static NPCTypeStandardSettings SickNPCSettings = new NPCTypeStandardSettings()
        {
            type = NPCTypeID.GetNextID(),
            keyName = GameLoader.NAMESPACE + ".Sick",
            printName = "Sick",
            maskColor0 = UnityEngine.Color.white,
            maskColor1 = UnityEngine.Color.yellow
        };
        public static NPCType SickNPCType;

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".SickJob.Init"),
            ModLoader.ModCallbackProvidesFor("pipliz.apiprovider.jobs.resolvetypes"),
            ModLoader.ModCallbackDependsOn("pipliz.blocknpcs.registerjobs")]
        public static void Init()
        {
            Logger.Log("Init Sick Job");
            NPCType.AddSettings(SickNPCSettings);
            SickNPCType = NPCType.GetByKeyNameOrDefault(SickNPCSettings.keyName);
            
        }


        public override bool ToSleep => true;

        public override NPCType NPCType { get { return SickNPCType; } }

        public override Vector3Int GetJobLocation()
        {
             return position;
        }

        public override void OnNPCAtJob(ref NPCBase.NPCState state)
        {
            if (TimeCycle.TotalTime > SickTime + Configuration.LengthOfDiseaseInDays)
            {

                OnRemovedNPC();

            }
        }

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

        public override ITrackableBlock InitializeFromJSON(Players.Player player, JSONNode node)
        {
            SickTime = node.GetAs<double>("SickTime");
            InitializeJob(player, (Vector3Int)node[nameof(position)], node.GetAs<int>("npcID"));
            return this;
        }
        public override JSONNode GetJSON()
        {
            JSONNode node = base.GetJSON();
            node.SetAs("type", SickNPCSettings.KeyName);
            node.SetAs<double>("SickTime", SickTime);
            node.SetAs(nameof(position), (JSONNode)position);

            return node;
        }

        public NPCTypeStandardSettings GetNPCTypeDefinition()
        {
            return SickNPCSettings;
        }

        public override bool IsValid => base.IsValid;

        public override void OnAssignedNPC(NPCBase npc)
        {
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
