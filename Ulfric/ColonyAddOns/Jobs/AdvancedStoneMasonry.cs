using Pipliz.Mods.APIProvider.Jobs;
using Server.NPCs;
using UnityEngine;

namespace Ulfric.ColonyAddOns.Jobs
{
    [ModLoader.ModManager]
    public static class AdvancedStoneMasonryRegister
    {
        public static string JOB_NAME = GameLoader.NAMESPACE + ".AdvancedStoneMason";
        public static string JOB_ITEM_KEY = GameLoader.NAMESPACE + ".Blocks.AdvancedStoneMasonryWorkbench";
        public static string JOB_RECIPE = JOB_ITEM_KEY + ".recipe";
        public const string MOD_NAMESPACE = GameLoader.NAMESPACE + ".AdvancedStoneMasonryRegister";

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, AdvancedStoneMasonryRegister.MOD_NAMESPACE + ".AdvancedStoneMasonryRegister.RegisterJobs")]
        [ModLoader.ModCallbackProvidesFor("pipliz.apiprovider.jobs.resolvetypes")]
        public static void RegisterJobs()
        {
            Logger.Log("Registering Job..." + JOB_NAME + " and Item Key " + JOB_ITEM_KEY);
            BlockJobManagerTracker.Register<AdvancedStoneMasonryJob>(JOB_ITEM_KEY);
        }

    }

    public class AdvancedStoneMasonryJob : CraftingJobBase, IBlockJobBase, INPCTypeDefiner
    {
        private static NPCTypeStandardSettings _type = new NPCTypeStandardSettings
        {
            keyName = AdvancedStoneMasonryRegister.JOB_NAME,
            printName = "AdvancedStoneMason",
            maskColor1 = new Color32(59, 55, 0, 255),
            type = NPCTypeID.GetNextID()
        };

        public static float StaticCraftingCooldown = 5f;

        public override string NPCTypeKey
        {
            get
            {
                return AdvancedStoneMasonryRegister.JOB_NAME;
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
                return AdvancedStoneMasonryJob.StaticCraftingCooldown;
            }
            set
            {
                AdvancedStoneMasonryJob.StaticCraftingCooldown = value;
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

        protected override string GetRecipeLocation()
        {
            return GameLoader.ConfigFolder + "/" + "AdvancedStoneMasonry.json";
        }
    }
}

