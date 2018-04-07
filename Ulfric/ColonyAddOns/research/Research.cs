using BlockTypes.Builtin;
using Pipliz.Mods.APIProvider.Science;
using Server.Science;
using System;
using System.Collections.Generic;

namespace Ulfric.ColonyAddOns.Research
{
    public class ResearchCompleteEventArgs : EventArgs
    {
        public Research Research { get; private set; }

        public ScienceManagerPlayer Manager { get; private set; }

        public ResearchCompleteEventArgs(Research research, ScienceManagerPlayer player)
        {
            Research = research;
            Manager = player;
        }
    }

    [ModLoader.ModManager]
    public class Research : BaseResearchable
    {
        private const string MOD_NAMESPACE = GameLoader.NAMESPACE + ".Research";
        private const string MOD_NAMESPACE_BLOCKS = GameLoader.NAMESPACE + ".Blocks";

        public const string CarpentryTraining = "CarpentryTraining";
        public const string AdvancedStoneMasonryTraining = "AdvancedStoneMasonryTraining";
        public const string FletcheryTraining = "FletcheryTraining";
        public const string AdvancedAgricultureTraining = "AdvancedAgricultureTraining";
        public const string AdvancedMerchantTraining = "AdvancedMerchantTraining";
        public const string HeraldTraining = "HeraldTraining";

        public string TmpValueKey { get; private set; } = string.Empty;
        public int Level { get; private set; } = 1;
        public float Value { get; private set; } = 0;
        public float BaseValue { get; private set; } = 0;
        public string LevelKey { get; private set; } = string.Empty;

        public event EventHandler<ResearchCompleteEventArgs> ResearchComplete;
        static Dictionary<string, float> _baseSpeed = new Dictionary<string, float>();

        public Research(Dictionary<ushort, int> requiredItems, int level, string name, float baseValue, List<string> dependancies = null, int baseIterationCount = 10, bool addLevelToName = true)
        {
            try
            {
                BaseValue = baseValue;
                Value = baseValue * level;
                Level = level;
                TmpValueKey = GetResearchKey(name);
                LevelKey = GetLevelKey(name);

                key = TmpValueKey + level;
                icon = GameLoader.IconFolder + "\\" + name + level + ".png";

                if (!addLevelToName)
                    icon = GameLoader.IconFolder + "\\" + name + ".png";

                iterationCount = baseIterationCount + (2 * level);

                foreach (var kvp in requiredItems)
                {
                    var val = kvp.Value;

                    if (level > 1)
                        for (int i = 1; i <= level; i++)
                            if (i % 2 == 0)
                                val += kvp.Value * 2;
                            else
                                val += kvp.Value;

                    AddIterationRequirement(kvp.Key, val);
                }

                if (level != 1)
                    AddDependency(TmpValueKey + (level - 1));

                if (dependancies != null)
                    foreach (var dep in dependancies)
                        AddDependency(dep);
            }
            catch(Exception e)
            {
                Logger.Log("Research error...{0}", e.Message);
            }
        }

        public override void OnResearchComplete(ScienceManagerPlayer manager, EResearchCompletionReason reason)
        {
            try
            {
                manager.Player.GetTempValues(true).Set(TmpValueKey, Value);
                manager.Player.GetTempValues(true).Set(LevelKey, Level);

                if (ResearchComplete != null)
                    ResearchComplete(this, new ResearchCompleteEventArgs(this, manager));
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error OnResearchComplete for {TmpValueKey} Level: {Level}.");
            }
        }

        public static string GetLevelKey(string researchName)
        {
            return GetResearchKey(researchName) + "_Level";
        }

        public static string GetResearchKey(string researchName)
        {
            return GameLoader.NAMESPACE + "." + researchName;
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnAddResearchables, GameLoader.NAMESPACE + ".Research.OnAddResearchables")]
        public static void Register()
        {
            var researchDic = new Dictionary<ushort, int>();
            Logger.Log("Registering Research..." + CarpentryTraining);
            Carpentry.AddCarpentry(researchDic);

            Logger.Log("Registering Research..." + AdvancedStoneMasonryTraining);
            AdvancedStoneMasonry.AddAdvancedStoneMasonry(researchDic);

            Logger.Log("Registering Research..." + FletcheryTraining);
            Fletchery.AddFletchery(researchDic);

            Logger.Log("Registering Research..." + AdvancedAgricultureTraining);
            AdvancedAgriculture.AddAdvancedAgriculture(researchDic);

            Logger.Log("Registering Research..." + AdvancedMerchantTraining);
            AdvancedMerchant.AddAdvancedMerchant(researchDic);

            Logger.Log("Registering Research..." + HeraldTraining);
            Herald.AddHerald(researchDic);

            Logger.Log("Research Registering Complete!");
        }
    }
}
