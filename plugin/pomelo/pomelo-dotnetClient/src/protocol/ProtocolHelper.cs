using System;
using SimpleJson;
using System.Text;
using NativeWebSocket;

namespace Pomelo.DotNetClient
{
    public class ProtocolHelper
    {
        private MessageProtocol messageProtocol;
        private ProtocolState state;
        private HandShakeService handshake;
        private HeartBeatService heartBeatService = null;
        private PomeloClient pc;
        private WebSocket socket;


        public ProtocolHelper(PomeloClient pc, WebSocket socket)
        {
            this.pc = pc;
            this.socket = socket;
            this.handshake = new HandShakeService(this);
            this.state = ProtocolState.start;
        }

        internal void startHandshake(Action<JsonObject> callback)
        {
            this.handshake.request(callback);
            this.state = ProtocolState.handshaking;
        }

        //Send request, user request id 
        internal void send(string route, uint id, JsonObject msg)
        {
            if (this.state != ProtocolState.working) return;

            byte[] body = messageProtocol.encode(route, id, msg);

            send(PackageType.PKG_DATA, body);
        }

        internal void send(PackageType type)
        {
            if (this.state == ProtocolState.closed) return;
            socket.Send(PackageProtocol.encode(type));
        }


        //Send message use the transporter
        internal void send(PackageType type, byte[] body)
        {
            if (this.state == ProtocolState.closed) return;
            byte[] pkg = PackageProtocol.encode(type, body);
            //Debug.Log("Websocket send pkg=" + pkg.Length);
            socket.Send(pkg);
        }

        //Invoke by Transporter, process the message
        internal void processMessage(byte[] bytes)
        {
            Package pkg = PackageProtocol.decode(bytes);
            //Ignore all the message except handshading at handshake stage
            if (pkg.type == PackageType.PKG_HANDSHAKE && this.state == ProtocolState.handshaking)
            {

                //Ignore all the message except handshading
                JsonObject data = (JsonObject)SimpleJson.SimpleJson.DeserializeObject(Encoding.UTF8.GetString(pkg.body));

                this.state = ProtocolState.working;
                processHandshakeData(data);
                
            }
            else if (pkg.type == PackageType.PKG_HEARTBEAT && this.state == ProtocolState.working)
            {

            }
            else if (pkg.type == PackageType.PKG_DATA && this.state == ProtocolState.working)
            {

                pc.processMessage(messageProtocol.decode(pkg.body));
            }
            else if (pkg.type == PackageType.PKG_KICK)
            {
                pc.disconnect();
            }
        }

        private void processHandshakeData(JsonObject msg)
        {
            //Handshake error
            if (!msg.ContainsKey("code") || !msg.ContainsKey("sys") || Convert.ToInt32(msg["code"]) != 200)
            {
                throw new Exception("Handshake error! Please check your handshake config.");
            }

            //Set compress data
            JsonObject sys = (JsonObject)msg["sys"];

            JsonObject dict = new JsonObject();
            if (sys.ContainsKey("dict")) dict = (JsonObject)sys["dict"];

            JsonObject protos = new JsonObject();
            JsonObject serverProtos = new JsonObject();
            JsonObject clientProtos = new JsonObject();

            if (sys.ContainsKey("protos"))
            {
                protos = (JsonObject)sys["protos"];
                serverProtos = (JsonObject)protos["server"];
                clientProtos = (JsonObject)protos["client"];
            }

            messageProtocol = new MessageProtocol(dict, serverProtos, clientProtos);

            //Init heartbeat service
            int interval = 0;
            if (sys.ContainsKey("heartbeat")) interval = Convert.ToInt32(sys["heartbeat"]);
            heartBeatService = new HeartBeatService(interval, this);

            if (interval > 0)
            {
                heartBeatService.start();
            }

            //send ack and change protocol state
            handshake.ack();
            this.state = ProtocolState.working;

            //Invoke handshake callback
            JsonObject user = new JsonObject();
            if (msg.ContainsKey("user")) user = (JsonObject)msg["user"];
            handshake.invokeCallback(user);
        }

        public void Tick()
        {
            heartBeatService?.Tick();
        }

        internal void close()
        {
            this.state = ProtocolState.closed;
        }
    }
}