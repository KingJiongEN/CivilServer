using System;

namespace Plugins.CityCommon.Msg
{
    [Serializable]
    public class CityMsgAgentBorn
    {
        public IntelligentAgent il_agent;

        public CityMsgAgentBorn(IntelligentAgent il_agent)
        {
            this.il_agent = il_agent;
        }

        public static string GetMsg(IntelligentAgent il_agent)
        {
            return (MyStringBuilder.Create(CityMsgId.CITY_MSG_AGENT_BORN) + "@" + Newtonsoft.Json.JsonConvert.SerializeObject(new CityMsgAgentBorn(il_agent))).ToStr();
        }
    }
}