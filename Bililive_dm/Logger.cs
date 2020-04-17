using GT_XiguaAPI;
using System;
using System.IO;

namespace Bililive_dm
{
    public class Logger
    {
        public readonly string Time = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss");
        public void SaveLog(string str, string level)
        {
            try
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter("log/" + Time + ".log", true))
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

        public string SaveToggle()
        {
            var NowTime = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss");
            var str = $"连接时间{Time}结束时间{NowTime}\n本次直播共 {User.UserList.Count} 位观众在你直播间留下足迹。\n";
            var retstr = str + $"详情请查看toggle/{Time}.txt";
            long[] key = new long[User.UserList.Count];
            User.UserList.Keys.CopyTo(key, 0);
            for(var i = 0; i < User.UserList.Count; i++)
            {
                str += $"{key[i]} : {User.UserList[key[i]]}\n";
            }
            try
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter("toggle/" + Time + ".txt", true))
                {
                    file.WriteLine(str);
                    file.Flush();
                    file.Close();
                }
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.ToString());
            }
            return retstr;
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
        
        public static void DisplayText(string str, bool isGift = false)
        {
            string[] idata = new string[6];
            idata[5] = str;
            try
            {
                StreamReader sr = new StreamReader("log/Chat.txt", System.Text.Encoding.UTF8);
                string line;
                for(var i = 0; i < 5; i++)
                {
                    if ((line = sr.ReadLine()) != null)
                    {
                        idata[i] = line.ToString();
                    }
                }
                sr.Close();
            }
            catch (Exception e)
            {
                idata[0] = e.ToString();
            }

            try
            {
                //FileMode.Append为不覆盖文件效果.create为覆盖
                FileStream fs = new FileStream("log/Chat.txt", FileMode.Create);
                byte[] data = System.Text.Encoding.UTF8.GetBytes(idata[1] + "\n" + idata[2] + "\n" + idata[3] + "\n" + idata[4] + "\n" + idata[5]);
                fs.Write(data, 0, data.Length);
                fs.Flush();
                fs.Close();

            }
            catch (Exception)
            {

            }
            if (isGift)
            {
                idata = new string[6];
                idata[5] = str;
                try
                {
                    StreamReader sr = new StreamReader("log/Gift.txt", System.Text.Encoding.UTF8);
                    string line;
                    for (var i = 0; i < 5; i++)
                    {
                        if ((line = sr.ReadLine()) != null)
                        {
                            idata[i] = line.ToString();
                        }
                    }
                    sr.Close();
                }
                catch (Exception e)
                {
                    idata[0] = e.ToString();
                }

                try
                {
                    //FileMode.Append为不覆盖文件效果.create为覆盖
                    FileStream fs = new FileStream("log/Gift.txt", FileMode.Create);
                    byte[] data = System.Text.Encoding.UTF8.GetBytes(idata[1] + "\n" + idata[2] + "\n" + idata[3] + "\n" + idata[4] + "\n" + idata[5]);
                    fs.Write(data, 0, data.Length);
                    fs.Flush();
                    fs.Close();

                }
                catch (Exception)
                {

                }
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
