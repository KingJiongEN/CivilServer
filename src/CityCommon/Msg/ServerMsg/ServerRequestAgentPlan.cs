using System;

namespace Plugins.CityCommon.Msg
{
    [Serializable]
    public class ServerRequestAgentPlan
    {
        public int agent_guid;

        public ServerRequestAgentPlan(int agent_guid)
        {
            this.agent_guid = agent_guid;
        }

        public static string GetMsg(int agent_guid)
        {
            return (MyStringBuilder.Create(ServerMsgId.LLM_REQUEST_AGENT_PLAN) + "@" + Newtonsoft.Json.JsonConvert.SerializeObject(new ServerMsgAgentBorn(agent_guid))).ToStr();
        }

    }
}