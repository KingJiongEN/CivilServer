using System;

namespace Plugins.CityCommon.Msg
{
    [Serializable]
    public class ServerMsgUserSellArtwork
    {
        public int price;
        public string artwork_id;
        public int to_agent_id;
        public string from_user_name;

        public ServerMsgUserSellArtwork(int price, string artwork_id, int to_agent_id, string from_user_name)
        {
            this.price = price;
            this.artwork_id = artwork_id;
            this.to_agent_id = to_agent_id;
            this.from_user_name = from_user_name;
        }
        
        public static string GetMsg(int price, string artwork_id, int to_agent_id, string from_user_name)
        {
            return MyStringBuilder.Create(ServerMsgId.SERVER_MSG_USER_SELL_ARTWORK + "@" + Newtonsoft.Json.JsonConvert.SerializeObject(new ServerMsgUserSellArtwork(price, artwork_id, to_agent_id, from_user_name))).ToStr();
        }
    }
}