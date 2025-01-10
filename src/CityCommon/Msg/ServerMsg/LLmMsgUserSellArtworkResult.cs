using System;

namespace Plugins.CityCommon.Msg
{
    [Serializable]
    public class LLmMsgUserSellArtworkResult : LLmMsgBase
    {
        public int price;
        public string artwork_id;
        public int to_agent_id;
        public string from_user_name;
        public bool is_succ;
        public string error_msg;
    
        public LLmMsgUserSellArtworkResult(bool is_succ, string error_msg, int price, string artwork_id, int to_agent_id, string from_user_name)
        {
            this.price = price;
            this.artwork_id = artwork_id;
            this.to_agent_id = to_agent_id;
            this.from_user_name = from_user_name;
            this.is_succ = is_succ;
            this.error_msg = error_msg;
        }

        public static string GetMsg(bool is_succ, string error_msg, int price, string artwork_id, int to_agent_id, string from_user_name)
        {
            return MyStringBuilder.Create(ServerMsgId.LLM_USER_SELL_ARTWORK_RESULT + "@" +
                                          Newtonsoft.Json.JsonConvert.SerializeObject(
                                              new LLmMsgUserSellArtworkResult(is_succ, error_msg, price, artwork_id, to_agent_id, from_user_name)))
                .ToStr();
        }
    }
}