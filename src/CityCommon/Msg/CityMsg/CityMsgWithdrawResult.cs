using System;

namespace Plugins.CityCommon.Msg
{
    [Serializable]
    public class CityMsgWithdrawResult
    {
        public bool is_succ;
        public string msg;

        public CityMsgWithdrawResult(bool is_succ, string msg)
        {
            this.is_succ = is_succ;
            this.msg = msg;
        }
        
        public static string GetMsg(bool is_succ, string msg)
        {
            return (MyStringBuilder.Create(CityMsgId.CITY_MSG_WITHDRAW_RESULT) + "@" + Newtonsoft.Json.JsonConvert.SerializeObject(new CityMsgWithdrawResult(is_succ, msg))).ToStr();
        }
    }
}