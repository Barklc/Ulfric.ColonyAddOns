using System;
using System.IO;

namespace Ulfric.ColonyAddOns
{
    internal class Logger
    {
        internal static void Log(ChatColor color, string message, params object[] args)
        {
            WriteTextToTextFile(GetFormattedMessage(string.Format(message, args)));
            //ServerLog.LogAsyncMessage(new Pipliz.LogMessage(Chat.BuildMessage(GetFormattedMessage(string.Format(message, args)), color), UnityEngine.LogType.Log));
        }

        internal static void Log(string message, params object[] args)
        {
            WriteTextToTextFile(GetFormattedMessage(string.Format(message, args)));
            //ServerLog.LogAsyncMessage(new Pipliz.LogMessage(GetFormattedMessage(string.Format(message, args)), UnityEngine.LogType.Log));
        }

        internal static void Log(string message)
        {
            WriteTextToTextFile(GetFormattedMessage(message));
            //ServerLog.LogAsyncMessage(new Pipliz.LogMessage(GetFormattedMessage(message), UnityEngine.LogType.Log));
        }

        internal static void LogError(Exception e, string message)
        {
            WriteTextToTextFile(GetFormattedMessage(message));
            //ServerLog.LogAsyncExceptionMessage(new Pipliz.LogExceptionMessage(Chat.BuildMessage(GetFormattedMessage(message), ChatColor.red), e));

            if (e.InnerException != null)
                LogError(e.InnerException);
        }

        internal static void LogError(Exception e, string message, params object[] args)
        {
            WriteTextToTextFile(GetFormattedMessage(string.Format(message, args)));
            //ServerLog.LogAsyncExceptionMessage(new Pipliz.LogExceptionMessage(Chat.BuildMessage(GetFormattedMessage(string.Format(message, args)), ChatColor.red), e));

            if (e.InnerException != null)
                LogError(e.InnerException);
        }

        internal static void LogError(Exception e)
        {
            WriteTextToTextFile(e.Message);
            //ServerLog.LogAsyncExceptionMessage(new Pipliz.LogExceptionMessage(Chat.BuildMessage("Exception", ChatColor.red), e));

            if (e.InnerException != null)
                LogError(e.InnerException);
        }

        private static string GetFormattedMessage(string message)
        {
            return string.Format("[{0}]<Ulfric => ColonyAddOns> {1}", DateTime.Now, message);
        }

        private static void WriteTextToTextFile(string message)
        {
            if (GameLoader.Debug)
            {
                StreamWriter log = File.AppendText(GameLoader.DebugFile);
                log.WriteLine(message);
                log.Flush();
                log.Close();
                if (!File.Exists(GameLoader.MODPATH + "/Debug.txt"))
                {
                    GameLoader.Debug = false;
                }
            }
            else
            {
                if (File.Exists(GameLoader.MODPATH + "/Debug.txt"))
                {
                    GameLoader.Debug = true;
                    GameLoader.DebugFile = GameLoader.MODPATH + "/Debug " + DateTime.Now.ToString("MM-dd-yy hh-mm-ss") + ".txt";
                }
            }
        
        }
    }
}
