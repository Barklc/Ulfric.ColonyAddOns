using System;
using System.IO;
using Pipliz;
using UnityEngine;

namespace Ulfric.ColonyAddOns
{
    [ModLoader.ModManager]
    public static class CommandsModEntries
    {
        public static string MODPATH;
        private const string MOD_NAMESPACE = GameLoader.NAMESPACE + ".CommandsModEntries";

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnAssemblyLoaded, CommandsModEntries.MOD_NAMESPACE + ".assemblyload")]
        public static void OnAssemblyLoaded(string path)
        {
            MODPATH = Path.GetDirectoryName(path);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterStartup, CommandsModEntries.MOD_NAMESPACE + ".registercallbacks")]
        public static void AfterStartup()
        {
            Pipliz.Log.Write("Loaded Commands Mod 1.0.0 Ulfric Thanks to Scarabol");
        }
    }

    public static class PlayerHelper
    {
        public static bool TryGetPlayer(string identifier, out Players.Player targetPlayer, out string error)
        {
            return TryGetPlayer(identifier, out targetPlayer, out error, false);
        }

        public static bool TryGetPlayer(string identifier, out Players.Player targetPlayer, out string error, bool includeOffline)
        {
            targetPlayer = null;
            if (identifier.StartsWith("'"))
            {
                if (identifier.EndsWith("'"))
                {
                    identifier = identifier.Substring(1, identifier.Length - 2);
                }
                else
                {
                    error = "missing ' after playername";
                    return false;
                }
            }
            if (identifier.Length < 1)
            {
                error = "no playername given";
                return false;
            }
            ulong steamid;
            if (ulong.TryParse(identifier, out steamid))
            {
                Steamworks.CSteamID csteamid = new Steamworks.CSteamID(steamid);
                if (csteamid.IsValid())
                {
                    NetworkID networkId = new NetworkID(csteamid);
                    error = "";
                    if (Players.TryGetPlayer(networkId, out targetPlayer))
                    {
                        return true;
                    }
                    else
                    {
                        targetPlayer = null;
                    }
                }
            }
            float closestDist = float.MaxValue;
            Players.Player closestMatch = null;
            foreach (Players.Player player in Players.PlayerDatabase.ValuesAsList)
            {
                if (!player.IsConnected && !includeOffline)
                {
                    continue;
                }
                if (player.Name != null)
                {
                    if (string.Equals(player.Name, identifier, StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (targetPlayer == null)
                        {
                            targetPlayer = player;
                        }
                        else
                        {
                            targetPlayer = null;
                            error = "duplicate player name, pls use SteamID";
                            return false;
                        }
                    }
                    else
                    {
                        float dis = Vector3.Distance(player.Position, targetPlayer.Position);
                        if (dis < closestDist)
                        {
                            closestDist = dis;
                            closestMatch = player;
                        }
                        else if (dis == closestDist)
                        {
                            closestMatch = null;
                        }
                    }
                }
            }
            if (targetPlayer != null)
            {
                error = "";
                return true;
            }
            else if (closestMatch != null && (closestDist < closestMatch.Name.Length * 0.2))
            {
                error = "";
                targetPlayer = closestMatch;
                Pipliz.Log.Write(string.Format("Name '{0}' did not match, picked closest match '{1}' instead", identifier, targetPlayer.Name));
                return true;
            }
            error = "player not found";
            return false;
        }
    }
}