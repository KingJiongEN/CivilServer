using System;

namespace Plugins.CityCommon.Msg
{
    [Serializable]
    public class LLmMsgUserBuyArtworkResult : LLmMsgBase
    {
        public int price;
        public string artwork_id;
        public int from_agent_id;
        public string to_user_name;
        public bool is_succ;
        public string error_msg;

        public LLmMsgUserBuyArtworkResult(bool is_succ, string error_msg, int price, string artwork_id, int from_agent_id, string to_user_name)
        {
            this.price = price;
            this.artwork_id = artwork_id;
            this.from_agent_id = from_agent_id;
            this.to_user_name = to_user_name;
            this.is_succ = is_succ;
            this.error_msg = error_msg;
        }
        
        public static string GetMsg(bool is_succ, string error_msg,int price, string artwork_id, int from_agent_id, string to_user_name)
        {
            return MyStringBuilder.Create(ServerMsgId.LLM_USER_BUY_ARTWORK_RESULT + "@" + Newtonsoft.Json.JsonConvert.SerializeObject(new LLmMsgUserBuyArtworkResult(is_succ, error_msg, price, artwork_id, from_agent_id, to_user_name))).ToStr();
        }
    }
}