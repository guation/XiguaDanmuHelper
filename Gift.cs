using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace XiguaDanmakuHelper
{
    public struct Gift
    {
        public User user;
        private readonly long ID;
        public static long RoomID = 0;
        public long count;
        public static Dictionary<long, string> GiftList = new Dictionary<long, string>();
        public static Dictionary<long, int> GiftValue = new Dictionary<long, int>();

        public Gift(JObject j)
        {
            ID = 0;
            count = 0;
            user = new User(j);
            if (j["common"]?["room_id"] != null)
            {
                //RoomID = 6787894152118930189;
                RoomID = (long)j["common"]["room_id"];
                //UpdateGiftList();
            }
            if (j["extra"]?["present_end_info"] != null && j["extra"]["present_end_info"].Any())
            {
                ID = (long)j["extra"]["present_end_info"]["id"];
                count = (long)j["extra"]["present_end_info"]["count"];
            }
            else if (j["extra"]?["present_info"] != null && j["extra"]["present_info"].Any())
            {
                ID = (long)j["extra"]["present_info"]["id"];
                count = (long)j["extra"]["present_info"]["repeat_count"];
            }
            if (ID != 0 && !GiftList.ContainsKey(ID))
            {
                UpdateGiftList();
            }
        }

        private static void UpdateGiftList()
        {
            //GiftList = new Dictionary<long, string>();
            //GiftList.Add(10001, "西瓜");
            /*
            if (GiftList.ContainsKey(10001))
            {
                GiftList[10001] = "西瓜1";
                GiftValue[10001] = 0;
            }
            else
            {
                GiftList.Add(10001, "西瓜1");
                GiftValue.Add(10001, 0);
            }*/
            try
            {
                //var _text = Common.HttpGet($"https://i.snssdk.com/videolive/gift/get_gift_list?room_id={RoomID}&version_code=730&device_platform=android");
                var _text = Common.HttpGet($"https://webcast3-lq.ixigua.com/webcast/gift/list/?room_id={RoomID}&webcast_sdk_version=1450&aid=32&version_code=836&device_platform=android");
                //Logger.DebugLog(_text);
                var j = JObject.Parse(_text);
                /*
                if (j["gift_info"].Any())
                    foreach (var g in j["gift_info"])
                        if (GiftList.ContainsKey((long)g["id"]))
                        {
                            GiftList[(long)g["id"]] = (string)g["name"];
                            GiftValue[(long)g["id"]] = (int)g["diamond_count"];
                        }
                        else
                        {
                            GiftList.Add((long)g["id"], (string)g["name"]);
                            GiftValue.Add((long)g["id"], (int)g["diamond_count"]);
                        }
                        */
                if (j["data"]["pages"].Any())
                    foreach (var p in j["data"]["pages"])
                        foreach(var g in p["gifts"])
                            if (GiftList.ContainsKey((long)g["id"]))
                            {
                                GiftList[(long)g["id"]] = (string)g["name"];
                                GiftValue[(long)g["id"]] = (int)g["diamond_count"];
                            }
                            else
                            {
                                GiftList.Add((long)g["id"], (string)g["name"]);
                                GiftValue.Add((long)g["id"], (int)g["diamond_count"]);
                            }
            }
            catch
            {

            }
        }

        public override string ToString()
        {
            return $"感谢 {user} 送出的 {count} 个 {GetName()}";
        }

        public string GetName()
        {
            string GiftN;
            if (GiftList.ContainsKey(ID))
                GiftN = GiftList[ID];
            else
                GiftN = $"未知礼物{ID}";

            return GiftN;
        }

        public long GetValue()
        {
            long GiftV;
            if (GiftValue.ContainsKey(ID))
                GiftV = GiftValue[ID];
            else
                GiftV = -1;

            return GiftV;
        }

        public static async void UpdateGiftListAsync(long roomId)
        {
            GiftList = new Dictionary<long, string>();
            var _text = await Common.HttpGetAsync($"https://i.snssdk.com/videolive/gift/get_gift_list?room_id={roomId}");
            var j = JObject.Parse(_text);
            if (j["gift_info"] != null)
                foreach (var g in j["gift_info"])
                    if (GiftList.ContainsKey((long)g["id"]))
                    {
                        GiftList[(long)g["id"]] = (string)g["name"];
                    }
                    else
                    {
                        GiftList.Add((long)g["id"], (string)g["name"]);
                    }
        }
    }
}