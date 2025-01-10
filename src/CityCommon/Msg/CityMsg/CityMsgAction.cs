using System;
using System.Collections.Generic;

namespace Plugins.CityCommon.Msg
{
    [Serializable]
    public class CityMsgAction
    {
        public int agent_guid;
        public string content;
        public List<AttrChange> attr_chagne;
        public int content_type;
        public int display_duration;
        public string song;


        public CityMsgAction(int agent_guid, string content, List<AttrChange> attr_chagne, int content_type, int display_duration, string song)
        {
            this.content = content;
            this.agent_guid = agent_guid;
            this.attr_chagne = attr_chagne;
            this.content_type = content_type;
            this.display_duration = display_duration;
            this.song = song;
        }

        public static string GetMsg(int agent_guid, string content, List<AttrChange> attr_chagne, int content_type, int display_duration, string song)
        {
            return (MyStringBuilder.Create(CityMsgId.CITY_MSG_ACTION) + "@" + Newtonsoft.Json.JsonConvert.SerializeObject(new CityMsgAction(agent_guid, content, attr_chagne, content_type, display_duration, song))).ToStr();
        }
    }
}