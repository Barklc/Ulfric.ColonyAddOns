using System;
using System.Collections.Generic;
using BlockTypes.Builtin;
using Pipliz.JSON;
using Server.Science;

namespace Ulfric.ColonyAddOns.Research
{
    class AdvancedMerchant
    {
        private const string MOD_NAMESPACE = GameLoader.NAMESPACE + ".Research";
        private const string MOD_NAMESPACE_BLOCKS = GameLoader.NAMESPACE + ".Blocks";
        private static string VANILLA_PREFIX = "vanilla.";

        public const string AdvancedMerchantTraining = "AdvancedMerchantTraining";

        public static void AddAdvancedMerchant(Dictionary<ushort, int> researchDic)
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
            var research = new Research(researchDic, level, AdvancedMerchantTraining, 1f, requirements, 12, false);
            research.ResearchComplete += AdvancedMerchant_ResearchComplete; 
            ScienceManager.RegisterResearchable(research);
        }

        public static void Research_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            throw new NotImplementedException();
        }

        public static void AdvancedMerchant_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            if (JSON.Deserialize(GameLoader.ConfigFolder + "/shopping.json", out JSONNode jsonRecipes, false))
            {
                if (jsonRecipes.NodeType == NodeType.Array)
                {
                    foreach (JSONNode craftingEntry in jsonRecipes.LoopArray())
                    {
                        if (craftingEntry.TryGetAs("name", out string name))
                        {
                            if (name.StartsWith(VANILLA_PREFIX))
                                name = name.Substring(VANILLA_PREFIX.Length);
                            else
                                name = MOD_NAMESPACE_BLOCKS + "." + name;

                        }
                        RecipeStorage.GetPlayerStorage(e.Manager.Player).SetRecipeAvailability(name, true);
                    }
                }
            }
        }

    }
}
