using System;

namespace Plugins.CityCommon.Msg
{
    [Serializable]
    public class CityMsgRechargeResult
    {
        public bool is_succ;
        public string msg;

        public CityMsgRechargeResult(bool is_succ, string msg)
        {
            this.is_succ = is_succ;
            this.msg = msg;
        }
        
        public static string GetMsg(bool is_succ, string msg)
        {
            return (MyStringBuilder.Create(CityMsgId.CITY_MSG_RECHARGE_RESULT) + "@" + Newtonsoft.Json.JsonConvert.SerializeObject(new CityMsgRechargeResult(is_succ, msg))).ToStr();
        }
    }
}