using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Bililive_dm
{
    class Config
    {
        public string Read(string path="Config.json")
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
        public bool Write(string str,string path="Config.json")
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
    }
    class ConfigData
    {
        public string RoomID { get; set; } = "162474";
        public string UserName { get; set; } = "sy挂神";

        public bool ShowBrand { get; set; } = false;
        public bool ShowChar { get; set; } = true;
        public bool ShowPresent { get; set; } = true;
        public bool ShowLike { get; set; } = true;
        public bool DanMu { get; set; } = true;
        public bool JoinRoom { get; set; } = false;
        public bool ShowFollow { get; set; } = true;

        public bool DeBug { get; set; } = false;
    }
}
