using Pipliz.Mods.APIProvider.Jobs;
using Server.NPCs;
using UnityEngine;

namespace Ulfric.ColonyAddOns.Jobs
{
    [ModLoader.ModManager]
    public static class FletcherRegister
    {
        public static string JOB_NAME = GameLoader.NAMESPACE + ".Fletcher";
        public static string JOB_ITEM_KEY = GameLoader.NAMESPACE + ".Blocks.FletcheryWorkbench";
        public static string JOB_RECIPE = JOB_ITEM_KEY + ".recipe";
        public const string MOD_NAMESPACE = GameLoader.NAMESPACE + ".FletcheryRegister";

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, FletcherRegister.MOD_NAMESPACE + ".FletcheryRegister.RegisterJobs")]
        [ModLoader.ModCallbackProvidesFor("pipliz.apiprovider.jobs.resolvetypes")]
        public static void RegisterJobs()
        {
            Logger.Log("Registering Job..." + JOB_NAME + " and Item Key " +JOB_ITEM_KEY);
            BlockJobManagerTracker.Register<FletcheryJob>(JOB_ITEM_KEY);
        }

    }

    public class FletcheryJob : CraftingJobBase, IBlockJobBase, INPCTypeDefiner
    {
        private static NPCTypeStandardSettings _type = new NPCTypeStandardSettings
        {
            keyName = FletcherRegister.JOB_NAME,
            printName = "Fletcher",
            maskColor1 = new Color32(109,0,148,1),
            type = NPCTypeID.GetNextID()
        };

        public static float StaticCraftingCooldown = 5f;

        public override string NPCTypeKey
        {
            get
            {
                return FletcherRegister.JOB_NAME;
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
                return FletcheryJob.StaticCraftingCooldown;
            }
            set
            {
                FletcheryJob.StaticCraftingCooldown = value;
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
