using System;
using System.Threading.Tasks;
using Pomelo.DotNetClient;
using SimpleJson;
public class WebsocketManager
{
    private static JsonObject users = null;
    public string user_name = null;
    private string url_gate = null;
    private string channel = null;
    private Action cb_OnConnectToServer;
    private Action<string, JsonObject> cb_OnMessage;
    private Action<string, string> cb_OnUserEnter;
    private Action<int> cb_OnEnterError;
    
    public WebsocketManager(string user_name, string channel, string url_gate)
    {
        this.user_name = user_name;
        this.url_gate = url_gate;
        this.channel = channel;
    }

    public void SetCallback(Action cb_OnConnectToServer, Action<string, JsonObject> cb_OnMessage, Action<string, string> cb_OnUserEnter, Action<int> cb_OnEnterError)
    {
        this.cb_OnConnectToServer = cb_OnConnectToServer;
        this.cb_OnMessage = cb_OnMessage;
        this.cb_OnUserEnter = cb_OnUserEnter;
        this.cb_OnEnterError = cb_OnEnterError;
    }
    
    public async void Start()
    {
        await Login();
    }

    public void Tick()
    {
        pclient?.Tick();
    }

    public void OnApplicationQuit()
    {
        if (pclient != null)
        {
            pclient.disconnect();
            pclient = null;
        }
    }
    
    private PomeloClient pclient = null;
    public async Task Login()
    {
        pclient = new PomeloClient(url_gate);
        Log.d("Login user_name=" + user_name + " channel=" + channel);
        await pclient.init(() =>
        {
            var userMessage = new JsonObject();
            userMessage.Add("uid", user_name);
            pclient.request("gate.gateHandler.queryEntry", userMessage, (data) =>
            {
                object code = null;
                if (data.TryGetValue("code", out code))
                {
                    if (Convert.ToInt32(code) == 500)
                    {
                        Log.e("错误: gate.gateHandler.queryEntry code=" + code);
                    }
                    else
                    {
                        pclient.disconnect();
                        pclient = null;
                        object host, port;
                        if (data.TryGetValue("host", out host) && data.TryGetValue("port", out port))
                        {
                            pclient = new PomeloClient("ws://" + host + ":" + port.ToString());
                            pclient.init(OnConnectToServer);
                        }
                    }
                }
            });
        });
    }

    private void OnConnectToServer()
    {
        InitChat(pclient);
        cb_OnConnectToServer?.Invoke();
    }

    //Entry chat application.
    public void Entry()
    {
        var userMessage = new JsonObject();
        userMessage.Add("username", user_name);
        userMessage.Add("rid", channel);
        pclient.request("connector.entryHandler.enter", userMessage, (data) =>
        {
            if (data.TryGetValue("code", out object _code))
            {
                if (int.TryParse(data["code"].ToString(), out int code))
                {
                    if (code == 500)
                    {
                        pclient.disconnect();
                        cb_OnEnterError?.Invoke(code);
                        return;
                    }
                }
            }
            users = data;
            Log.d(channel + " connector.entryHandler.enter:" + data);
            cb_OnUserEnter?.Invoke(channel, user_name);
        });
    }
    
    //private StringBuilder sb = new StringBuilder();
    private void InitChat(PomeloClient pclient)
    {
        pclient.On("onAdd", (data) => {
            Log.d("onAdd=====>" + data);
            //sb.Append("\r\n<color=red>" + data["user"] + "</color>\r\n加入房间");
            cb_OnUserEnter?.Invoke(channel, data["user"].ToString());
        });
			 
        pclient.On("onLeave", (data) => {
            Log.d("onLeave=====>" + data);
            //sb.Append("\r\n<color=red>" + data["user"] + "</color>\r\n退出房间");
        });
		  
        pclient.On("onChat", (data)=> {
            //Log.d("onChat=====>" + data);
            //sb.Append("\r\n<color=red>" + data["from"] + "</color>:\r\n " + data["msg"]);
            cb_OnMessage?.Invoke(channel, data);
        });
    
    }

    public void SendMsg(string content, string target)
    {
        if (string.IsNullOrEmpty(target))
        {
            target = "*";
        }
        var message = new JsonObject();
        message.Add("rid", channel);
        message.Add("content", content);
        message.Add("from", user_name);
        message.Add("target", target);
        pclient.request("chat.chatHandler.send", message, null);
    }

    public bool IsConnected()
    {
        if (pclient != null)
        {
            return pclient.IsConnected();
        }
        return false;
    }
}