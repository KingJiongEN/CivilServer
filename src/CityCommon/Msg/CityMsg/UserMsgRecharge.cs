using System;

namespace Plugins.CityCommon.Msg
{
    [Serializable]
    public class UserMsgRecharge
    {
        public int recharge_count;
        public string from_user_name;
        public int to_agent_id;

        public UserMsgRecharge(int recharge_count, string from_user_name, int to_agent_id)
        {
            this.recharge_count = recharge_count;
            this.from_user_name = from_user_name;
            this.to_agent_id = to_agent_id;
        }
        
        public static string GetMsg(int recharge_count, string from_user_name, int to_agent_id)
        {
            return (MyStringBuilder.Create(CityMsgId.USER_MSG_RECHARGE) + "@" + Newtonsoft.Json.JsonConvert.SerializeObject(new UserMsgRecharge(recharge_count, from_user_name, to_agent_id))).ToStr();
        }
    }
}