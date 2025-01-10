using System;

namespace Plugins.CityCommon.Msg
{
    [Serializable]
    public class CityMsgUserLossArtwork
    {
        public string user_name;
        public string artwork_id;
        
        public CityMsgUserLossArtwork(string user_name, string artwork_id)
        {
            this.user_name = user_name;
            this.artwork_id = artwork_id;
        }
        
        public static string GetMsg(string user_name, string artwork_id)
        {
            return (MyStringBuilder.Create(CityMsgId.CITY_MSG_USER_LOSS_ARTWORK) + "@" + Newtonsoft.Json.JsonConvert.SerializeObject(new CityMsgUserLossArtwork(user_name, artwork_id))).ToStr();
        }
    }
}