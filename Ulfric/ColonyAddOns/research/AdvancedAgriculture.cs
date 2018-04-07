using System;
using System.Collections.Generic;
using BlockTypes.Builtin;
using Pipliz.Mods.APIProvider.Science;
using Server.Science;

namespace Ulfric.ColonyAddOns.Research
{
    class AdvancedAgriculture
    {
        private const string MOD_NAMESPACE = GameLoader.NAMESPACE + ".Research";
        private const string MOD_NAMESPACE_BLOCKS = GameLoader.NAMESPACE + ".Blocks";

        public const string AdvancedAgricultureTraining = "AdvancedAgricultureTraining";

        public static void AddAdvancedAgriculture(Dictionary<ushort, int> researchDic)
        {

            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.CopperTools, 2);
            researchDic.Add(BuiltinBlocks.ScienceBagBasic, 1);

            var requirements = new List<string>()
            {
                "pipliz.baseresearch.herbfarming",
                "pipliz.baseresearch.sciencebagadvanced"
            };

            Register(researchDic, 1, requirements);

            //for (int i = 2; i <= 5; i++)
            //{
            //    Register(researchDic, i);
            //}
        }

        public static void Register(Dictionary<ushort, int> researchDic, int level, List<string> requirements = null)
        {
            var research = new Research(researchDic, level, AdvancedAgricultureTraining, 1f, requirements, 12, false);
            research.ResearchComplete += AdvancedAgriculture_ResearchComplete; 
            ScienceManager.RegisterResearchable(research);
        }

        public static void Research_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            throw new NotImplementedException();
        }

        public static void AdvancedAgriculture_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {

            RecipeStorage.GetPlayerStorage(e.Manager.Player).SetRecipeAvailability(MOD_NAMESPACE_BLOCKS + ".AdvancedAgricultureWorkbench", true);
        }

    }
}
