using System;

namespace Plugins.CityCommon.Msg
{
    [Serializable]
    public class CityMsgAgentLoseArtwork
    {
        public string artwork_id;
        public int agent_guid;
        
        public CityMsgAgentLoseArtwork(int agent_guid, string artwork_id)
        {
            this.agent_guid = agent_guid;
            this.artwork_id = artwork_id;
        }
        
        public static string GetMsg(int agent_guid, string artwork_id)
        {
            return (MyStringBuilder.Create(CityMsgId.CITY_MSG_AGENT_LOSE_ARTWORK) + "@" + Newtonsoft.Json.JsonConvert.SerializeObject(new CityMsgAgentLoseArtwork(agent_guid, artwork_id))).ToStr();
        }
    }
}