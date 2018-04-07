using Pipliz.JSON;
using System;
using System.Collections.Generic;
using System.IO;


namespace Ulfric.Decorations
{
    /// <summary>
    /// Execution entry points for our mod.
    /// </summary>
    [ModLoader.ModManager]
    public static class Localization
    {
        private const string MOD_NAMESPACE = GameLoader.NAMESPACE + ".Localization";

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterWorldLoad, MOD_NAMESPACE + ".Localize")]
        [ModLoader.ModCallbackDependsOn("pipliz.server.localization.waitforloading")]
        [ModLoader.ModCallbackProvidesFor("pipliz.server.localization.convert")]
        public static void Localize()
        {
            Logger.Log("Localization directory: {0}", GameLoader.LocalizationFolder);
            try
            {
                string[] array = new string[]
                {
                        "translation.json"
                };
                for (int i = 0; i < array.Length; i++)
                {
                    string text = array[i];
                    string[] files = Directory.GetFiles(GameLoader.LocalizationFolder, text, SearchOption.AllDirectories);
                    string[] array2 = files;
                    for (int j = 0; j < array2.Length; j++)
                    {
                        string text2 = array2[j];
                        try
                        {
                            JSONNode jsonFromMod;
                            if (JSON.Deserialize(text2, out jsonFromMod, false))
                            {
                                string name = Directory.GetParent(text2).Name;

                                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(text))
                                {
                                    Logger.Log("Found mod localization file for '{0}' localization", name);
                                    localize(name, text, jsonFromMod);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Log("Exception reading localization from {0}; {1}", text2, ex.Message);
                        }
                    }
                }
            }
            catch (DirectoryNotFoundException)
            {
                Logger.Log("Localization directory not found at {0}", GameLoader.LocalizationFolder);
            }
        }

        public static void localize(string locName, string locFilename, JSONNode jsonFromMod)
        {
            try
            {
                if (Server.Localization.Localization.LoadedTranslation == null)
                {
                    Logger.Log("Unable to localize. Server.Localization.Localization.LoadedTranslation is null.");
                }
                else
                {
                    if (Server.Localization.Localization.LoadedTranslation.TryGetValue(locName, out JSONNode jsn))
                    {
                        if (jsn != null)
                        {
                            foreach (KeyValuePair<string, JSONNode> modNode in jsonFromMod.LoopObject())
                            {
                                Logger.Log("Adding localization for '{0}' from '{1}'.", modNode.Key, Path.Combine(locName, locFilename));
                                AddRecursive(jsn, modNode);
                            }
                        }
                        else
                            Logger.Log("Unable to localize. Localization '{0}' not found and is null.", locName);
                    }
                    else
                        Logger.Log("Localization '{0}' not supported", locName);
                }

                Logger.Log("Patched mod localization file '{0}/{1}'", locName, locFilename);

            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Exception while localizing {0}", Path.Combine(locName, locFilename));
            }
        }

        private static void AddRecursive(JSONNode gameJson, KeyValuePair<string, JSONNode> modNode)
        {
            int childCount = 0;

            try
            {
                childCount = modNode.Value.ChildCount;
            }
            catch { }

            if (childCount != 0)
            {
                if (gameJson.HasChild(modNode.Key))
                {
                    foreach (var child in modNode.Value.LoopObject())
                        AddRecursive(gameJson[modNode.Key], child);
                }
                else
                {
                    gameJson[modNode.Key] = modNode.Value;
                }
            }
            else
            {
                gameJson[modNode.Key] = modNode.Value;
            }
        }
    }
}
