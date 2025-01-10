using System;

namespace Plugins.CityCommon.Msg
{
    [Serializable]
    public class UserMsgWithdraw
    {
        public int withdraw_count;
        public string to_user_name;
        public int from_agent_id;

        public UserMsgWithdraw(int withdraw_count, string to_user_name, int from_agent_id)
        {
            this.withdraw_count = withdraw_count;
            this.to_user_name = to_user_name;
            this.from_agent_id = from_agent_id;
        }
        
        public static string GetMsg(int withdraw_count, string to_user_name, int from_agent_id)
        {
            return (MyStringBuilder.Create(CityMsgId.USER_MSG_WITHDRAW) + "@" + Newtonsoft.Json.JsonConvert.SerializeObject(new UserMsgWithdraw(withdraw_count, to_user_name, from_agent_id))).ToStr();
        }
    }
}