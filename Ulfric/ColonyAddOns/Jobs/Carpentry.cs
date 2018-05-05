using Pipliz.Mods.APIProvider.Jobs;
using Server.NPCs;
using UnityEngine;

namespace Ulfric.ColonyAddOns.Jobs
{
    [ModLoader.ModManager]
    public static class CarpentryRegister
    {
        public static string JOB_NAME = GameLoader.NAMESPACE + ".Carpenter";
        public static string JOB_ITEM_KEY = GameLoader.NAMESPACE + ".Blocks.CarpentryWorkbench";
        public static string JOB_RECIPE = JOB_ITEM_KEY + ".recipe";
        public const string MOD_NAMESPACE = GameLoader.NAMESPACE + ".CarpentryRegister";

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, CarpentryRegister.MOD_NAMESPACE + ".CarpentryRegister.RegisterJobs")]
        [ModLoader.ModCallbackProvidesFor("pipliz.apiprovider.jobs.resolvetypes")]
        public static void RegisterJobs()
        {
            Logger.Log("Registering Job..." + JOB_NAME + " and Item Key " +JOB_ITEM_KEY);
            BlockJobManagerTracker.Register<CarpentryJob>(JOB_ITEM_KEY);
        }

    }

    public class CarpentryJob : CraftingJobBase, IBlockJobBase, INPCTypeDefiner
    {
        private static NPCTypeStandardSettings _type = new NPCTypeStandardSettings
        {
            keyName = CarpentryRegister.JOB_NAME,
            printName = "Carpenter",
            maskColor1 = new Color32(58,0,97,1),
            type = NPCTypeID.GetNextID()
        };

        public static float StaticCraftingCooldown = 5f;

        public override string NPCTypeKey
        {
            get
            {
                return CarpentryRegister.JOB_NAME;
            }
        }

        public override int MaxRecipeCraftsPerHaul
        {
            get
            {
                return 1;
            }
        }

        public override float CraftingCooldown
        {
            get
            {
                return CarpentryJob.StaticCraftingCooldown;
            }
            set
            {
                CarpentryJob.StaticCraftingCooldown = value;
            }
        }

        NPCTypeStandardSettings INPCTypeDefiner.GetNPCTypeDefinition()
        {
            return _type;
        }

        protected override void OnRecipeCrafted()
        {
            base.OnRecipeCrafted();
            ServerManager.SendAudio(this.position.Vector, "crafting");
        }
    }
}
