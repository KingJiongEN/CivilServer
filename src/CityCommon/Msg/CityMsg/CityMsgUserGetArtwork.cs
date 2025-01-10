using System;

namespace Plugins.CityCommon.Msg
{
    [Serializable]
    public class CityMsgUserGetArtwork
    {
        public string user_name;
        public string artwork_id;
        
        public CityMsgUserGetArtwork(string user_name, string artwork_id)
        {
            this.user_name = user_name;
            this.artwork_id = artwork_id;
        }
        
        public static string GetMsg(string user_name, string artwork_id)
        {
            return (MyStringBuilder.Create(CityMsgId.CITY_MSG_USER_GET_ARTWORK) + "@" + Newtonsoft.Json.JsonConvert.SerializeObject(new CityMsgUserGetArtwork(user_name, artwork_id))).ToStr();
        }
    }
}