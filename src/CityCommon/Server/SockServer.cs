
using System.Collections.Generic;
using System.Threading.Tasks;
using Plugins.CityCommon.Server;

public abstract class SockServer
{
    protected CityMap city_map;
    protected Dictionary<string, WebsocketManager> websocket_manager = new Dic<string, WebsocketManager>();
    private Queue<ServerMsg> queue = new Queue<ServerMsg>();
    protected bool is_init = false;
    private List<string> channels = new List<string>();

    public async Task Init()
    {
        this.is_init = true;
        channels = GetChannelList();
        foreach (var channel in channels)
        {
            WebsocketManager sock = new WebsocketManager(GetUserName(), channel, GameServerConfig.URL_GATE);
            sock.SetCallback(() =>
            {
                sock.Entry();
            }, (_channel, data) =>
            {
                if (data!=null && data.ContainsKey("from") && data.ContainsKey("msg"))
                {
                    OnMessage(_channel, data["from"].ToString(), data["msg"].ToString());
                }
                else
                {
                    Log.e("错误的数据格式 data=" + data);
                }
            }, OnUserEnter, (error_code) =>
            {
                Log.e("登录错误 error_code=" + error_code);
            });
            websocket_manager[channel] = sock;
            await sock.Login();
        }
    }
    
    public virtual void BindCtiyMap(CityMap city_map)
    {
        this.city_map = city_map;
    }
    
    protected abstract string GetUserName();
    protected abstract List<string> GetChannelList();

    public bool IsConnected()
    {
        foreach (var key in channels)
        {
            if (websocket_manager.TryGetValue(key, out var t))
            {
                if (!t.IsConnected())
                {
                    return false;
                }
            }
        }
        return websocket_manager.Count > 0;
    }

    public void Tick()
    {
        if (is_init)
        {
            foreach(var key in channels)
            {
                if(websocket_manager.TryGetValue(key, out var t))
                {
                    t.Tick();
                }
            }
            if (IsConnected())
            {
                OnTick();
            }
        }
    }

    public void OnNewDay()
    {
        if (is_init && IsConnected())
        {
            DoOnNewDay();
        }
    }

    public void OnNewMonth()
    {
        if (is_init)
        {
            DoOnNewMonth();
        }
    }
    public void OnNewYear()
    {
        if (is_init)
        {
            DoOnNewYear();
        }
    }
    protected virtual void DoOnNewYear(){}
    protected virtual void DoOnNewMonth(){}
    protected virtual void DoOnNewDay(){}

    public virtual void OnTick()
    {
        while (is_init && queue.TryDequeue(out ServerMsg msg))
        {
            int index = msg.msg.IndexOf("@");
            if (index != -1 && int.TryParse(msg.msg.Substring(0, index), out int msg_id))
            {
                DealMessage(msg.channel, msg.from, msg_id, msg.msg.Substring(index + 1));
            }
            else
            {
                Log.e("错误：未知的消息类型" + msg.msg);
            }

        }
    }

    public void OnMessage(string channel, string from, string msg)
    {
        if (is_init)
        {
            queue.Enqueue(new ServerMsg(from, msg, channel));
        }
    }

    public virtual void OnUserEnter(string channel, string user)
    {
        
    }

    public abstract void DealMessage(string channel, string from, int msg_id, string msg);

    public void OnApplicationQuit()
    {
        foreach (var kv in websocket_manager)
        {
            kv.Value.OnApplicationQuit();
        }
    }

    protected void LogError(string channel, string from, int msg_id, string msg)
    {
        Log.e("错误：未处理的消息类型 channel=" + channel + " from=" + from + " msg_id=" + msg_id + " msg=" + msg);
    }
}