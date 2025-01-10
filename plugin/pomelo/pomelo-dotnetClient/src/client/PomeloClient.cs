using SimpleJson;
using System;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NativeWebSocket;
using GameServer.src;

namespace Pomelo.DotNetClient
{

    /// <summary>
    /// network state enum
    /// </summary>
    public enum NetWorkState
    {
        [Description("initial state")]
        CLOSED,

        [Description("connecting server")]
        CONNECTING,

        [Description("server connected")]
        CONNECTED,

        [Description("disconnected with server")]
        DISCONNECTED,

        [Description("connect timeout")]
        TIMEOUT,

        [Description("netwrok error")]
        ERROR
    }

    public class PomeloClient : IDisposable
    {
        private NetWorkState netWorkState = NetWorkState.CLOSED; //current network state

        private EventManager eventManager;
        private WebSocket socket;
        private ProtocolHelper protocol;
        private bool disposed = false;
        private uint reqId = 1;

        private string url_ws;

        public PomeloClient(string url_ws)
        {
            this.url_ws = url_ws;
        }

        private Action on_connected_callback;

        public async Task init(Action on_connected_callback = null)
        {
            this.on_connected_callback = on_connected_callback;
            eventManager = new EventManager();
            NetWorkChanged(NetWorkState.CONNECTING);

            this.socket = new WebSocket(url_ws);
            protocol = new ProtocolHelper(this, socket);
            InitSocket(socket);


            await socket.Connect();
        }

        private void InitSocket(WebSocket socket)
        {
            this.socket.OnOpen += this.OnOpen;
            this.socket.OnMessage += this.OnMessage;
            this.socket.OnClose += this.OnClose;
            this.socket.OnError += this.OnError;
        }

        private void OnError(string errormsg)
        {
            NetWorkChanged(NetWorkState.ERROR);
            Debug.Log("WebSocket OnError" + errormsg + " url_ws=" + url_ws);
        }

        private void OnClose(WebSocketCloseCode closecode)
        {
            Debug.Log("WebSocket OnClose closecode=" + closecode + " url_ws=" + url_ws);
        }

        private void OnMessage(byte[] data)
        {
            //Debug.Log("WebSocket OnMessage data=" + data.Length);
            protocol?.processMessage(data);
        }

        private void OnOpen()
        {
            Debug.Log("WebSocket OnOpen" + " url_ws=" + url_ws);
            //握手
            connect((data) =>
            {
                Debug.Log("握手成功" + data);
                NetWorkChanged(NetWorkState.CONNECTED);
                on_connected_callback?.Invoke();
            });

        }

        /// <summary>
        /// 网络状态变化
        /// </summary>
        /// <param name="state"></param>
        private void NetWorkChanged(NetWorkState state)
        {
            netWorkState = state;
        }

        public bool connect(Action<JsonObject> handshakeCallback)
        {
            try
            {
                protocol.startHandshake(handshakeCallback);
                return true;
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
                return false;
            }
        }

        private JsonObject emptyMsg = new JsonObject();

        public void request(string route, Action<JsonObject> action)
        {
            this.request(route, emptyMsg, action);
        }

        public void request(string route, JsonObject msg, Action<JsonObject> action)
        {
            if (netWorkState == NetWorkState.CONNECTED)
            {
                this.eventManager.AddCallBack(reqId, action);
                protocol.send(route, reqId, msg);

                reqId++;
            }
            else
            {
                string msg_str = msg.ToString();
                if (msg_str.Length > 100)
                {
                    msg_str = msg_str.Substring(0, 100);
                }
                Debug.LogError("Websocket is not connected! route=" + route + " msg=" + msg_str);
            }
        }

        public void ClearAllListener()
        {
            eventManager.ClearAllListener();
        }
        
        public void On(string eventName, Action<JsonObject> action)
        {
            eventManager.AddOnEvent(eventName, action);
        }

        internal void processMessage(Message msg)
        {
            if (msg.type == MessageType.MSG_RESPONSE)
            {
                //msg.data["__route"] = msg.route;
                //msg.data["__type"] = "resp";
                eventManager.InvokeCallBack(msg.id, msg.data);
            }
            else if (msg.type == MessageType.MSG_PUSH)
            {
                //msg.data["__route"] = msg.route;
                //msg.data["__type"] = "push";
                eventManager.InvokeOnEvent(msg.route, msg.data);
            }
        }

        public bool IsConnected() => netWorkState == NetWorkState.CONNECTED;

        public void disconnect()
        {
            Debug.Log("call PomeloClient.disconnect");
            Dispose();
            NetWorkChanged(NetWorkState.DISCONNECTED);
        }

        public void Dispose()
        {
            DisposeClient();
            GC.SuppressFinalize(this);
        }
        
        public void Tick()
        {
            socket?.DispatchMessageQueue();
            protocol?.Tick();
        }

        // The bulk of the clean-up code
        protected virtual void DisposeClient()
        {
            if (this.disposed)
            {
                return;
            }

            try
            {
                // free managed resources
                if (this.protocol != null)
                {
                    this.protocol.close();
                    protocol = null;
                }

                if (this.eventManager != null)
                {
                    this.eventManager.Dispose();
                    eventManager = null;
                }

                if (socket != null)
                {
                    this.socket.Close();
                    this.socket = null;
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
                this.disposed = true;
            }
        }
    }
}