using System;

namespace Plugins.CityCommon.Msg
{
    [Serializable]
    public class CityMsgUserSellArtworkResult
    {
        public bool is_succ;
        public string msg;

        public CityMsgUserSellArtworkResult(bool is_succ, string msg)
        {
            this.is_succ = is_succ;
            this.msg = msg;
        }
        
        public static string GetMsg(bool is_succ, string msg)
        {
            return (MyStringBuilder.Create(CityMsgId.CITY_USER_SELL_ARTWORK_RESULT) + "@" + Newtonsoft.Json.JsonConvert.SerializeObject(new CityMsgUserSellArtworkResult(is_succ, msg))).ToStr();
        }
    }
}