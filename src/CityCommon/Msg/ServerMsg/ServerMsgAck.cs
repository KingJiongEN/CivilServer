using System;

namespace Plugins.CityCommon.Msg
{
    [Serializable]
    public class ServerMsgAck
    {
        public int msg_guid;
        /// <summary>
        /// 0表示无错误
        /// </summary>
        public int error_code;
        public string error_msg;

        public ServerMsgAck(int msg_guid, int error_code, string error_msg)
        {
            this.msg_guid = msg_guid;
            this.error_code = error_code;
            this.error_msg = error_msg;
        }

        public static string GetMsg(int msg_guid, int error_code=0, string error_msg=null)
        {
            return (MyStringBuilder.Create(ServerMsgId.SERVER_MSG_ACK) + "@" + Newtonsoft.Json.JsonConvert.SerializeObject(new ServerMsgAck(msg_guid, error_code, error_msg))).ToStr();
        }
    }
}