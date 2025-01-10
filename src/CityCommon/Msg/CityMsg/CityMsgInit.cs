using System;

namespace Plugins.CityCommon.Msg
{
    [Serializable]
    public class CityMsgInit
    {
        public CityState city_state;

        public CityMsgInit(CityState city_state)
        {
            this.city_state = city_state;
        }

        public static string GetMsg(CityState city_state)
        {
            return (MyStringBuilder.Create(CityMsgId.CITY_MSG_INIT) + "@" + Newtonsoft.Json.JsonConvert.SerializeObject(new CityMsgInit(city_state))).ToStr();
        }
    }
    
    [Serializable]
    public class ServerMsgInit
    {
        public CityState city_state;

        public ServerMsgInit(CityState city_state)
        {
            this.city_state = city_state;
        }

        public static string GetMsg(CityState city_state)
        {
            return (MyStringBuilder.Create(ServerMsgId.SERVER_MSG_INIT) + "@" + Newtonsoft.Json.JsonConvert.SerializeObject(new ServerMsgInit(city_state))).ToStr();
        }
    }

}