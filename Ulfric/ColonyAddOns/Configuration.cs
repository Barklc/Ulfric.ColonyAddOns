using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Pipliz.JSON;

namespace Ulfric.ColonyAddOns
{

    [ModLoader.ModManager]
    public static class Configuration
    {
        public const string MOD_NAMESPACE = GameLoader.NAMESPACE + ".Configuration";
        private static string _saveFileName = $"{GameLoader.SavedGameFolder}/{ServerManager.WorldName}/{GameLoader.NAMESPACE}.json";
        private static JSONNode _rootSettings = new JSONNode();

        public static bool AllowDehydration = true;
         public static float HydrationValuePerColonists = 5.0f;

        public static bool AllowHandPickingBerryBushes = true;
        public static int NumberOfBerriesPerPick = 2;
        public static float ChanceOfBerriesPerPick = .50f;

        public static int HeraldWarningDistance = 20;

        public static bool EnableStatisticCollecting = true;

        public static bool EnableTrades = true;

        public static bool EnableMiningWithPickAxe = true;

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterSelectedWorld, GameLoader.NAMESPACE + ".Configuration.AfterSelectedWorld")]
//        ModLoader.ModCallbackDependsOn(GameLoader.NAMESPACE + ".AfterSelectedWorld")]
        public static void AfterSelectedWorld()
        {
            Reload();
            AllowDehydration = GetorDefault("AllowDehydration", AllowDehydration);
            HydrationValuePerColonists = GetorDefault("HydrationValuePerColonists", HydrationValuePerColonists);

            AllowHandPickingBerryBushes = GetorDefault("AllowHandPickingBerryBushes", AllowHandPickingBerryBushes);
            NumberOfBerriesPerPick = GetorDefault("NumberOfBerriesPerPick", NumberOfBerriesPerPick);
            ChanceOfBerriesPerPick = GetorDefault("ChanceOfBerriesPerPick", ChanceOfBerriesPerPick);

            HeraldWarningDistance = GetorDefault("HeraldWarningDistance", HeraldWarningDistance);

            EnableStatisticCollecting = GetorDefault("EnableStatisticCollecting", EnableStatisticCollecting);

            EnableTrades = GetorDefault("EnableTrades", EnableTrades);

            EnableMiningWithPickAxe = GetorDefault("EnableMiningWithPickAxe", EnableMiningWithPickAxe);

            Save();
        }

        public static void Reload()
        {
            if (File.Exists(_saveFileName) && JSON.Deserialize(_saveFileName, out var config))
            {
                _rootSettings = config;

                AllowDehydration = GetorDefault("AllowDehydration", AllowDehydration);
                HydrationValuePerColonists = GetorDefault("HydrationValuePerColonists", HydrationValuePerColonists);

                AllowHandPickingBerryBushes = GetorDefault("AllowHandPickingBerryBushes", AllowHandPickingBerryBushes);
                NumberOfBerriesPerPick = GetorDefault("NumberOfBerriesPerPick", NumberOfBerriesPerPick);
                ChanceOfBerriesPerPick = GetorDefault("ChanceOfBerriesPerPick", ChanceOfBerriesPerPick);

                HeraldWarningDistance = GetorDefault("HeraldWarningDistance", HeraldWarningDistance);

                EnableStatisticCollecting = GetorDefault("EnableStatisticCollecting", EnableStatisticCollecting);

                EnableTrades = GetorDefault("EnableTrades", EnableTrades);

                EnableMiningWithPickAxe = GetorDefault("EnableMiningWithPickAxe", EnableMiningWithPickAxe);
            }
        }

        public static void Save()
        {
            
            _rootSettings.SetAs("AllowDehydration", AllowDehydration);
            _rootSettings.SetAs("HydrationValuePerColonists", HydrationValuePerColonists);

            _rootSettings.SetAs("AllowHandPickingBerryBushes", AllowHandPickingBerryBushes);
            _rootSettings.SetAs("NumberOfBerriesPerPick", NumberOfBerriesPerPick);
            _rootSettings.SetAs("ChanceOfBerriesPerPick", ChanceOfBerriesPerPick);

            _rootSettings.SetAs("HeraldWarningDistance", HeraldWarningDistance);

            _rootSettings.SetAs("EnableStatisticCollecting", EnableStatisticCollecting);

            _rootSettings.SetAs("EnableTrades", EnableTrades);

            _rootSettings.SetAs("EnableMiningWithPickAxe", EnableMiningWithPickAxe);

            JSON.Serialize(_saveFileName, _rootSettings);
        }

        public static T GetorDefault<T>(string key, T defaultVal)
        {
            if (!_rootSettings.HasChild(key))
                SetValue(key, defaultVal);

            return _rootSettings.GetAs<T>(key);
        }

        public static void SetValue<T>(string key, T val)
        {
            _rootSettings.SetAs(key, val);
            Save();
        }
    }
}