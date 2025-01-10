using System;

namespace Plugins.CityCommon.Msg
{
    [Serializable]
    public class CityMsgAgentAttrChange
    {
        public int agent_guid;
        public int attr_id;
        public int attr_value;

        public CityMsgAgentAttrChange(int agent_guid, int attr_id, int attr_value)
        {
            this.agent_guid = agent_guid;
            this.attr_id = attr_id;
            this.attr_value = attr_value;
        }
        
        public static string GetMsg(int agent_guid, int attr_id, int attr_value)
        {
            return (MyStringBuilder.Create(CityMsgId.CITY_MSG_AGENT_ATTR_CHANGE) + "@" + Newtonsoft.Json.JsonConvert.SerializeObject(new CityMsgAgentAttrChange(agent_guid, attr_id, attr_value))).ToStr();
        }
    }

}