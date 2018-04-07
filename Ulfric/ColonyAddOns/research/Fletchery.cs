using System;
using System.Collections.Generic;
using BlockTypes.Builtin;
using Pipliz.Mods.APIProvider.Science;
using Server.Science;

namespace Ulfric.ColonyAddOns.Research
{
    class Fletchery
    {
        private const string MOD_NAMESPACE = GameLoader.NAMESPACE + ".Research";
        private const string MOD_NAMESPACE_BLOCKS = GameLoader.NAMESPACE + ".Blocks";

        public const string FletcheryTraining = "FletcheryTraining";

        public static void AddFletchery(Dictionary<ushort, int> researchDic)
        {

            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.CopperTools, 2);
            researchDic.Add(BuiltinBlocks.ScienceBagBasic, 1);

            var requirements = new List<string>()
            {
                "pipliz.baseresearch.crossbow"
            };

            Register(researchDic, 1, requirements);

        }

        public static void Register(Dictionary<ushort, int> researchDic, int level, List<string> requirements = null)
        {
            var research = new Research(researchDic, level, FletcheryTraining, 1f, requirements, 12, true);
            research.ResearchComplete += Fletchery_ResearchComplete; 
            ScienceManager.RegisterResearchable(research);
        }

        public static void Fletchery_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {

            RecipeStorage.GetPlayerStorage(e.Manager.Player).SetRecipeAvailability(MOD_NAMESPACE_BLOCKS + ".FletcheryWorkbench", true);
        }

    }
}
