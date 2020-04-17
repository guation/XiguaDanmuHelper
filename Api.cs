using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace XiguaDanmakuHelper
{
    public class Api
    {
        public delegate void Log(string msg, string level = "info");

        public delegate void RoomCounting(string popularity);

        public delegate void WhenMessage(MessageModel m);

        public delegate void WhenLeave();

        //        public delegate void WhenLotteryFinished();
        private long _roomPopularity;
        protected string cursor = "0";
        public bool isLive = false;
        public bool isValidRoom = false;
        public long RoomID = 0;
        public string Title = "";
        public User user;
        public string liverName;
        public bool isRoomID = false;

        public Api()
        {
            liverName = "挂神";
        }

        public Api(string name)
        {
            liverName = name;
            isRoomID = long.TryParse(liverName, out _);
        }

        public static event WhenMessage OnMessage;
        public static event RoomCounting OnRoomCounting;
        public static event Log LogMessage;
        public static event WhenLeave OnLeave;
        //        public static event WhenLotteryFinished OnLotteryFinished;

        public async Task<bool> ConnectAsync()
        {
            if (isRoomID)
                UpdateRoomInfoWeb();
            else
                await UpdateRoomInfoAsync();
            if (!isValidRoom)
            {
                LogMessage?.Invoke("请确认输入的用户名/房间号是否正确");
                return false;
            }

            if (!isLive)
            {
                LogMessage?.Invoke("主播未开播");
                return false;
            }
            LogMessage?.Invoke("连接成功");
            return true;
        }

        public void _updateRoomInfo(JObject j)
        {
            if (j["extra"]?["member_count"] != null) _roomPopularity = (long)j["extra"]["member_count"];
            if (j["data"]?["popularity"] != null) _roomPopularity = (long)j["data"]["popularity"];

            OnRoomCounting?.Invoke(_roomPopularity.ToString());
        }

        public async Task<bool> UpdateRoomInfoAsync()
        {
            if (isLive)
            {
                var url = $"https://i.snssdk.com/videolive/room/enter?version_code=730&device_platform=android";
                var data = $"room_id={RoomID}&version_code=730&device_platform=android";
                string _text;
                try
                {
                    _text = await Common.HttpPostAsync(url, data);
                }
                catch (WebException)
                {
                    LogMessage?.Invoke("网络错误");
                    return false;
                }
                var j = JObject.Parse(_text);
                if (j["data"] is null)
                {
                    LogMessage?.Invoke("无法获取Room信息，请与我联系");
                    return false;
                }

                Title = (string)j["data"][0]["anchor"]["room_name"];
                user = new User(j);
                if (isLive && (int)j["room"]?["status"] != 2)
                {
                    OnLeave?.Invoke();
                }
                isLive = (int)j["room"]?["status"] == 2;
                return true;
            }
            else
            {
                //var url = $"https://security.snssdk.com/video/app/search/live/?version_code=730&device_platform=android&format=json&keyword={liverName}";
                var url = $"https://search.ixigua.com/video/app/search/live/relate_anchor/?device_id=70829103337&aid=32&version_code=836&device_platform=android&m_tab=video&format=json&offset=0&count=1&keyword={liverName}";
                string _text;
                try
                {
                    _text = await Common.HttpGetAsync(url);
                }
                catch (WebException)
                {
                    LogMessage?.Invoke("网络错误");
                    return false;
                }
                var j = JObject.Parse(_text);
                if (!(j["data"] is null))
                {
                    foreach (var _j in j["data"])
                    {
                        /*
                        if ((int)_j["block_type"] != 0)
                        {
                            continue;
                        }
                        */
                        if (_j["anchor"].Any())
                        {
                            isValidRoom = true;

                            try
                            {
                                isLive = (bool)_j["anchor"]["user_info"]["is_living"];
                                RoomID = (long)_j["anchor"]["room_id"];
                                liverName = new User((JObject)_j).ToString();
                                user = new User((JObject)_j);
                            }
                            catch (Exception e)
                            {
                                LogMessage?.Invoke(e.ToString());
                                isLive = false;
                                return false;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }

                    if (isLive)
                    {
                        return await UpdateRoomInfoAsync();
                    }
                }
                return false;
            }
        }

        public bool UpdateRoomInfo()
        {
            if (isLive)
            {
                var url = $"https://i.snssdk.com/videolive/room/enter?version_code=730&device_platform=android";
                var data = $"room_id={RoomID}&version_code=730&device_platform=android";
                string _text;
                try
                {
                    _text = Common.HttpPost(url, data);
                }
                catch (WebException)
                {
                    LogMessage?.Invoke("网络错误");
                    return false;
                }
                var j = JObject.Parse(_text);
                if (j["data"] is null)
                {
                    LogMessage?.Invoke("无法获取Room信息，请与我联系");
                    return false;
                }

                isValidRoom = (int)j["base_resp"]?["status_code"] == 0;
                Title = (string)j["data"][0]["anchor"]["room_name"];
                RoomID = (long)j["data"][0]["anchor"]["room_id"];
                user = new User(j);
                if (isLive && (int)j["room"]?["status"] != 2)
                {
                    OnLeave?.Invoke();
                }
                isLive = (int)j["room"]?["status"] == 2;
                return true;
            }
            else
            {
               // var url = $"https://security.snssdk.com/video/app/search/live/?version_code=730&device_platform=android&format=json&keyword={liverName}";
                var url = $"https://search.ixigua.com/video/app/search/live/relate_anchor/?device_id=70829103337&aid=32&version_code=836&device_platform=android&m_tab=video&format=json&offset=0&count=10&keyword={liverName}";
                string _text;
                try
                {
                    _text = Common.HttpGet(url);
                }
                catch (WebException)
                {
                    LogMessage?.Invoke("网络错误");
                    return false;
                }
                var j = JObject.Parse(_text);
                if (!(j["data"] is null))
                {
                    foreach (var _j in j["data"])
                    {
                        /*
                        if ((int)_j["block_type"] != 0)
                        {
                            continue;
                        }
                        */
                        if (_j["anchor"].Any())
                        {
                            try
                            {
                                isValidRoom = true;
                                isLive = (bool)_j["anchor"]["user_info"]["is_living"];
                                RoomID = (int)_j["anchor"]["room_id"];
                                liverName = (new User((JObject)_j)).ToString();
                            }
                            catch (Exception err)
                            {
                                LogMessage?.Invoke(err.ToString());
                                return false;
                            }

                        }
                        else
                        {
                            return false;
                        }
                    }

                    if (isLive)
                    {
                        return UpdateRoomInfo();
                    }
                }
                return false;
            }
        }

        public bool UpdateRoomInfoWeb()
        {
            isLive = true;
            var url = $"https://live.ixigua.com/{liverName}";
            string _text;
            try
            {
                _text = Common.HttpGet(url, true);
            }
            catch (WebException)
            {
                LogMessage?.Invoke("网络错误");
                isValidRoom = false;
                return false;
            }
            //LogMessage?.Invoke(_text);
            try
            {
                string[] data1 = _text.Split(new String[] { "id=\"SSR_HYDRATED_DATA\">" }, StringSplitOptions.RemoveEmptyEntries);
                string[] data2 = data1[1].Split(new String[] { "</script>" }, StringSplitOptions.RemoveEmptyEntries);
                var j = JObject.Parse(data2[0]);
                if (j["roomData"] != null)
                {
                    isValidRoom = true;
                    RoomID = (long)j["roomData"]["id"];
                }
            }
            catch (Exception err)
            {
                LogMessage?.Invoke("出现故障");
                LogMessage?.Invoke(err.ToString(), "debug");
                isValidRoom = false;
                isLive = false;
                return false;
            }
            return true;
        }

        public void GetDanmaku()
        {
            if (!isValidRoom)
            {
                if (isRoomID)
                    UpdateRoomInfoWeb();
                else
                    UpdateRoomInfo();
                return;
            }

            var url = $"https://i.snssdk.com/videolive/im/get_msg?cursor={cursor}&room_id={RoomID}&version_code=730&device_platform=android";

            string _text;
            try
            {
                _text = Common.HttpGet(url);
            }
            catch (WebException)
            {
                LogMessage?.Invoke("网络错误");
                return;
            }

            var j = JObject.Parse(_text);
            if (j["extra"]?["cursor"] is null)
            {
                LogMessage?.Invoke("cursor 数据结构改变，请与作者联系");
                Console.Read();
                return;
            }

            cursor = (string)j["extra"]["cursor"];
            if (j["data"] is null)
            {
                if (isRoomID)
                    UpdateRoomInfoWeb();
                else
                    UpdateRoomInfo();
                return;
            }

            foreach (var m in j["data"])
            {
                if (m?["common"]?["method"] is null) continue;
                switch ((string)m["common"]["method"])
                {
                    case "VideoLivePresentMessage":
                        //Logger.DebugLog(m.ToString());
                        OnMessage?.Invoke(new MessageModel(MessageEnum.Gifting, new Gift((JObject)m)));
                        break;
                    case "SunDailyRankMessage":
                        break;
                    case "WebcastXGLotteryMessage":
                        break;
                    case "VideoLivePresentEndTipMessage":
                        OnMessage?.Invoke(new MessageModel(MessageEnum.Gift, new Gift((JObject)m)));
                        break;
                    case "VideoLiveRoomAdMessage":
                        OnMessage?.Invoke(new MessageModel(MessageEnum.Ad, (JObject)m));
                        break;
                    case "VideoLiveChatMessage":
                    case "VideoLiveDanmakuMessage":
                        OnMessage?.Invoke(new MessageModel(new Chat((JObject)m)));
                        break;
                    case "VideoLiveMemberMessage":
                        _updateRoomInfo((JObject)m);
                        //OnEnter?.Invoke(new Gift((JObject)m));
                        //OnMessage?.Invoke(new MessageModel(new Chat((JObject)m)));
                        OnMessage?.Invoke(new MessageModel(MessageEnum.Enter, (JObject)m));
                        break;
                    case "VideoLiveSocialMessage":
                        //Logger.DebugLog(m.ToString());
                        OnMessage?.Invoke(new MessageModel(MessageEnum.Subscribe, new User((JObject)m)));
                        break;
                    case "VideoLiveJoinDiscipulusMessage":
                        OnMessage?.Invoke(new MessageModel(MessageEnum.Join, new User((JObject)m)));
                        break;
                    case "VideoLiveControlMessage":
                        //Logger.DebugLog(m.ToString());
                        /*
                        if (isRoomID)
                            UpdateRoomInfoWeb();
                        else
                            UpdateRoomInfo();
                            */
                        OnLeave?.Invoke();
                        OnMessage?.Invoke(new MessageModel(MessageEnum.Leave));
                        break;
                    case "VideoLiveDiggMessage":
                        OnMessage?.Invoke(new MessageModel(MessageEnum.Like, new User((JObject)m)));
                        break;
                    case "VideoLiveVerifyMessage":
                        OnMessage?.Invoke(new MessageModel(new Chat((JObject)m)));
                        break;
                    default:
                        //Logger.DebugLog(m.ToString());
                        OnMessage?.Invoke(new MessageModel(MessageEnum.Other, (JObject)m));
                        break;
                }
            }
        }
    }
}