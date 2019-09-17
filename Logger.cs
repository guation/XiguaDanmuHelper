using System;

namespace XiguaDanmakuHelper
{
    public class Logger
    {
        public readonly string time = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss");
        public void SaveLog(string str, string level)
        {
            try
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter("log/" + time + ".log", true))
                {
                    file.WriteLine("[" + level + "]" + str);// 直接追加文件末尾，换行 
                    file.Flush();
                    file.Close();
                }
            }
            catch (Exception)
            {

            }
        }
        public static void DebugLog(string str)
        {
            try
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter("log/debug.log", true))
                {
                    file.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss")}]" + str);// 直接追加文件末尾，换行 
                    file.Flush();
                    file.Close();
                }
            }
            catch (Exception)
            {

            }
        }

        public static string GetLevel(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Info:
                    return "info";
                case LogLevel.Warn:
                    return "warn";
                case LogLevel.Error:
                    return "error";
                case LogLevel.Debug:
                    return "debug";
                default:
                    return "none";
            }
        }
    }
    public enum LogLevel
    {
        None,
        Info,
        Warn,
        Error,
        Debug
    }
}
