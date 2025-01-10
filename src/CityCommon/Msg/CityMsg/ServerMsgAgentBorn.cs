using System;

namespace Plugins.CityCommon.Msg
{
    [Serializable]
    public class ServerMsgAgentBorn
    {
        public int agent_guid;

        public ServerMsgAgentBorn(int agent_guid)
        {
            this.agent_guid = agent_guid;
        }

        public static string GetMsg(int agent_guid)
        {
            return (MyStringBuilder.Create(ServerMsgId.SERVER_MSG_AGENT_BORN) + "@" + Newtonsoft.Json.JsonConvert.SerializeObject(new ServerMsgAgentBorn(agent_guid))).ToStr();
        }
    }
}