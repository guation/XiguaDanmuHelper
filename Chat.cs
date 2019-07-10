using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace XiguaDanmakuHelper
{
    public struct Chat
    {
        public string content;
        public User user;
        private List<string> filter;

        public Chat(JObject j)
        {
            filter = new List<string>(); ;
            filter.Add("");
            content = "";
            user = new User(j);
            if (j["extra"]?["content"] != null) content = (string)j["extra"]["content"];
        }

        public override string ToString()
        {
            return $"{user} : {content}";
        }
    }
}