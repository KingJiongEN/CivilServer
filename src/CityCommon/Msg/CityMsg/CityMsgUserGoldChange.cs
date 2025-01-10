using System;

namespace Plugins.CityCommon.Msg
{
    [Serializable]
    public class CityMsgUserGoldChange
    {
        public string user_name;
        public int gold;
        
        public CityMsgUserGoldChange(string user_name, int gold)
        {
            this.user_name = user_name;
            this.gold = gold;
        }
        
        public static string GetMsg(string user_name, int gold)
        {
            return (MyStringBuilder.Create(CityMsgId.CITY_MSG_USER_GOLD_CHANGE) + "@" + Newtonsoft.Json.JsonConvert.SerializeObject(new CityMsgUserGoldChange(user_name, gold))).ToStr();
        }
    }
}