using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using Newtonsoft.Json.Linq;

namespace XiguaDanmakuHelper
{
    public class Config
    {
        public static string Read(string path="Config.json")
        {
            string data = "";
            try
            {
                StreamReader sr = new StreamReader(path, UTF8Encoding.Default);
                String line;
                while ((line = sr.ReadLine()) != null)
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
        public static bool Write(string str , string path="Config.json")
        {
            try
            {
                //FileMode.Append为不覆盖文件效果.create为覆盖
                FileStream fs = new FileStream(path, FileMode.Create);
                byte[] data = System.Text.UTF8Encoding.Default.GetBytes(str);
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
        public static bool Write(object obj , string path = "Config.json")
        {
            return Write(getJsonByObject(obj),path);
        }
        public static string getJsonByObject(object obj)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(obj.GetType());//实例化DataContractJsonSerializer对象，需要待序列化的对象类型
            MemoryStream stream = new MemoryStream();//实例化一个内存流，用于存放序列化后的数据
            serializer.WriteObject(stream, obj);//使用WriteObject序列化对象
            byte[] dataBytes = new byte[stream.Length];//写入内存流中
            stream.Position = 0;
            stream.Read(dataBytes, 0, (int)stream.Length);
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
        public bool ShowChar { get; set; } = true;
        public bool ShowPresent { get; set; } = true;
        public bool ShowLike { get; set; } = true;
        public bool DanMu { get; set; } = true;
        public bool JoinRoom { get; set; } = false;
        public bool ShowFollow { get; set; } = true;

        public int spd = 5, pit = 5, vol = 1, per = 4;

        public bool canUpdate = true;

        public bool DeBug { get; set; } = false;
    }
}
