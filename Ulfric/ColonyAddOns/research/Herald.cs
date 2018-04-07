using System;
using System.Collections.Generic;
using BlockTypes.Builtin;
using Pipliz.Mods.APIProvider.Science;
using Server.Science;

namespace Ulfric.ColonyAddOns.Research
{
    class Herald
    {
        private const string MOD_NAMESPACE = GameLoader.NAMESPACE + ".Research";
        private const string MOD_NAMESPACE_BLOCKS = GameLoader.NAMESPACE + ".Blocks";

        public const string HeraldTraining = "HeraldTraining";

        public static void AddHerald(Dictionary<ushort, int> researchDic)
        {

            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.BronzeIngot, 5);
            researchDic.Add(BuiltinBlocks.ScienceBagBasic, 1);

            var requirements = new List<string>()
            {
                "pipliz.baseresearch.sciencebagbasic"
            };

            Register(researchDic, 1, requirements);

        }

        public static void Register(Dictionary<ushort, int> researchDic, int level, List<string> requirements = null)
        {
            var research = new Research(researchDic, level, HeraldTraining, 1f, requirements, 12, false);
            research.ResearchComplete += Herald_ResearchComplete; 
            ScienceManager.RegisterResearchable(research);
        }

        public static void Herald_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {

            RecipeStorage.GetPlayerStorage(e.Manager.Player).SetRecipeAvailability(MOD_NAMESPACE_BLOCKS + ".HeraldStand", true);
        }

    }
}
