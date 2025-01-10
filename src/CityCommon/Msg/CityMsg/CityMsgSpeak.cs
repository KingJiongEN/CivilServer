using System;

namespace Plugins.CityCommon.Msg
{
    [Serializable]
    public class CityMsgSpeak
    {
      
        public int agent_guid;
        public string content;
        public string song;
        public int content_type;
        public int display_duration;

        /// <summary>
        /// 聊天
        /// </summary>
        public const int CHAT = 0;
        /// <summary>
        /// 内心读白
        /// </summary>
        public const int INNER_MONOLOGUE = 1;
        /// <summary>
        /// 状态变化
        /// </summary>
        public const int STATE_CHANGE = 2;

        public CityMsgSpeak(int agent_guid, string content, string song, int content_type, int display_duration)
        {
            this.agent_guid = agent_guid;
            this.content = content;
            this.song = song;
            this.content_type = content_type;
            this.display_duration = display_duration;
        }

        public static string GetMsg(int agent_guid, string content, string song, int content_type, int display_duration)
        {
            return (MyStringBuilder.Create(CityMsgId.CITY_MSG_SPEAK) + "@" + Newtonsoft.Json.JsonConvert.SerializeObject(new CityMsgSpeak(agent_guid, content, song, content_type, display_duration))).ToStr();
        }
    }
}