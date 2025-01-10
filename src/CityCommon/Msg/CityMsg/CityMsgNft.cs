using System;

namespace Plugins.CityCommon.Msg
{
    [Serializable]
    public class CityMsgNft
    {
        public int agent_guid;
        public string url;
        public string artwork_id;
        
        public CityMsgNft(int agent_guid, string artwork_id, string url)
        {
            this.agent_guid = agent_guid;
            this.artwork_id = artwork_id;
            this.url = url;
        }

        public static string GetMsg(int agent_guid, string artwork_id, string url)
        {
            return (MyStringBuilder.Create(CityMsgId.CITY_MSG_NFT) + "@" + Newtonsoft.Json.JsonConvert.SerializeObject(new CityMsgNft(agent_guid, artwork_id, url))).ToStr();
        }
    }
}