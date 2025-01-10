using System;
using System.Text;
using SimpleJson;
using System.Net;
using System.Net.Sockets;

namespace Pomelo.DotNetClient
{
    public class HandShakeService
    {
        private ProtocolHelper protocol_helper;
        private Action<JsonObject> callback;

        public const string Version = "0.0.1";
        public const string Type = "js-websocket";


        public HandShakeService(ProtocolHelper protocol_helper)
        {
            this.protocol_helper = protocol_helper;
        }

        public void request(Action<JsonObject> callback)
        {
            byte[] body = Encoding.UTF8.GetBytes(buildHandshackMsg().ToString());

            protocol_helper.send(PackageType.PKG_HANDSHAKE, body);

            this.callback = callback;
        }

        internal void invokeCallback(JsonObject data)
        {
            //Invoke the handshake callback
            if (callback != null) callback.Invoke(data);
        }

        public void ack()
        {
            protocol_helper.send(PackageType.PKG_HANDSHAKE_ACK, new byte[0]);
        }

        private JsonObject buildHandshackMsg()
        {
            JsonObject msg = new JsonObject();

            //Build sys option
            JsonObject sys = new JsonObject();
            sys["version"] = Version;
            sys["type"] = Type;
            sys["rsa"] = new JsonObject();
            sys["protoVersion"] = "";
            //sys["protoVersion"] = "2zWPnSlRsRxjqgcU215Uxg==";

            //Build handshake message
            msg["sys"] = sys;
            //{"sys":{"type":"js-websocket","version":"0.0.1","rsa":{},"protoVersion":"2zWPnSlRsRxjqgcU215Uxg=="}}
            return msg;
        }
    }
}