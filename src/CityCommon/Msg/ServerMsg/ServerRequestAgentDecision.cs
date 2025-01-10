using System;
using System.Collections.Generic;

namespace Plugins.CityCommon.Msg
{
    [Serializable]
    public class ServerRequestAgentDecision
    {
        public int agent_guid;
        public string area_name;
        public int cur_place_guid;
        public List<int> angents_around;

        public ServerRequestAgentDecision(int agent_guid, string area_name, int cur_place_guid, List<int> angents_around)
        {
            this.agent_guid = agent_guid;
            this.area_name = area_name;
            this.cur_place_guid = cur_place_guid;
            this.angents_around = angents_around;
        }

        public static string GetMsg(int agent_guid, string area_name, int cur_place_guid, List<int> angents_around)
        {
            return (MyStringBuilder.Create(ServerMsgId.LLM_REQUEST_AGENT_DECISION) + "@" + Newtonsoft.Json.JsonConvert.SerializeObject(new ServerRequestAgentDecision(agent_guid, area_name, cur_place_guid, angents_around))).ToStr();
        }
    }
}