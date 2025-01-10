using System;

namespace Plugins.CityCommon.Msg
{
    [Serializable]
    public class CityMsgUserBuyArtworkResult
    {
        public bool is_succ;
        public string msg;

        public CityMsgUserBuyArtworkResult(bool is_succ, string msg)
        {
            this.is_succ = is_succ;
            this.msg = msg;
        }
        
        public static string GetMsg(bool is_succ, string msg)
        {
            return (MyStringBuilder.Create(CityMsgId.CITY_USER_BUY_ARTWORK_RESULT) + "@" + Newtonsoft.Json.JsonConvert.SerializeObject(new CityMsgUserBuyArtworkResult(is_succ, msg))).ToStr();
        }
    }
}