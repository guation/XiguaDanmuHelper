using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web.Script.Serialization;

namespace Bililive_dm
{
    public class Config
    {
        public static string Read(string path = "Config.json")
        {
            string data = "";
            try
            {
                StreamReader sr = new StreamReader(path, UTF8Encoding.Default);
                string line;
                if ((line = sr.ReadLine()) != null)//只读取一行，多行用while
                {
                    data += line.ToString();
                }
                sr.Close();
            }
            catch (Exception e)
            {
                data = e.ToString();
            }

            return data;
        }
        public static bool Write(string str, string path = "Config.json")
        {
            try
            {
                //FileMode.Append为不覆盖文件效果.create为覆盖
                FileStream fs = new FileStream(path, FileMode.Create);
                byte[] data = System.Text.UTF8Encoding.Default.GetBytes(str + "\n//修改配置文件可能导致程序异常退出，若程序多次异常退出可尝试删除此文件");
                fs.Write(data, 0, data.Length);
                fs.Flush();
                fs.Close();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public static bool Write(object obj, string path = "Config.json")
        {
            return Write(getJsonByObject(obj), path);
        }
        public static string getJsonByObject(object obj)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(obj.GetType());//实例化DataContractJsonSerializer对象，需要待序列化的对象类型
            MemoryStream stream = new MemoryStream();//实例化一个内存流，用于存放序列化后的数据
            serializer.WriteObject(stream, obj);//使用WriteObject序列化对象
            byte[] dataBytes = new byte[stream.Length];//写入内存流中
            stream.Position = 0;
            stream.Read(dataBytes, 0, (int)stream.Length);
            stream.Close();
            return Encoding.UTF8.GetString(dataBytes);//通过UTF8格式转换为字符串
        }
    }
    public class ConfigData
    {
        public string RoomID { get; set; } = "162474";
        public string UserName { get; set; } = "sy挂神";
        public string Room { get; set; } = "162474";

        public bool ShowBrand { get; set; } = false;
        public bool ShowGrade { get; set; } = false;
        public bool ShowChat { get; set; } = true;
        public bool ShowPresent { get; set; } = true;
        public bool ShowLike { get; set; } = true;
        public bool DanMu { get; set; } = true;
        public bool JoinRoom { get; set; } = true;
        public bool ShowFollow { get; set; } = true;
        public int spd = 5, pit = 5, vol = 1, per = 4;

        public bool CanUpdate = true;

        public bool DeBug { get; set; } = false;
        public string BlackList = "";
        public int maxCapacity = 10;
        public int maxSize = 45;

        public override string ToString()
        {
            return Config.getJsonByObject(this);
        }

        public static explicit operator ConfigData(string json)
        {
            return new JavaScriptSerializer().Deserialize<ConfigData>(json);
        }
    }

    public enum SongChatEnum
    {
        Song,
        Gift,
        CheckIn,
        Speak,
        Delte
    }

    public class SongChat
    {
        public SongChatEnum Type = SongChatEnum.Song;
        public string Search = "";
        public string UserName = "sy挂神";
        public long UserID = 110756724607;
        public int Diamond = 0;
        public const string Version = "19.8.18";
        public override string ToString()
        {
            return Config.getJsonByObject(this);
        }
    }
}
