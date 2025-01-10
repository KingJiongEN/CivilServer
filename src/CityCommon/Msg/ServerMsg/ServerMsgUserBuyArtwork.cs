using System;

namespace Plugins.CityCommon.Msg
{
    [Serializable]
    public class ServerMsgUserBuyArtwork
    {
        public int price;
        public string artwork_id;
        public int from_agent_id;
        public string to_user_name;

        public ServerMsgUserBuyArtwork(int price, string artwork_id, int from_agent_id, string to_user_name)
        {
            this.price = price;
            this.artwork_id = artwork_id;
            this.from_agent_id = from_agent_id;
            this.to_user_name = to_user_name;
        }
        
        public static string GetMsg(int price, string artwork_id, int from_agent_id, string to_user_name)
        {
            return MyStringBuilder.Create(ServerMsgId.SERVER_MSG_USER_BUY_ARTWORK + "@" + Newtonsoft.Json.JsonConvert.SerializeObject(new ServerMsgUserBuyArtwork(price, artwork_id, from_agent_id, to_user_name))).ToStr();
        }
    }
}