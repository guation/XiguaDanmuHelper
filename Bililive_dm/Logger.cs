using System;

namespace Bililive_dm
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
    }
}
