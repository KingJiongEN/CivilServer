using Pipelines.Sockets.Unofficial.Arenas;
using Plugins.CityCommon.Server;
using StackExchange.Redis;
using System;
using System.Collections.Generic;

public class RedisControler : IRedis
{
    private IDatabase db;
    public void Init()
    {
        var configurationOptions = new ConfigurationOptions
        {
            EndPoints = {GameServerConfig.REDIS_IP, GameServerConfig.REDIS_PORT },
            KeepAlive = 180,
            Password = "Civil_123",
            DefaultVersion = new Version("2.8.5"),
            AllowAdmin = true
        };

        ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(configurationOptions);
        // 获取数据库实例
        db = redis.GetDatabase();
    }

    public void SubmitAgentState(SimAgent agent)
    {
        try
        {
            db.StringSet(agent.GetStateKeyForWeb(), Newtonsoft.Json.JsonConvert.SerializeObject(agent.GetStateForWeb()));
        }
        catch (Exception e)
        {
            Log.e(e);
        }
    }

    public void SubmitAllAgentPos(List<SimAgent> agents)
    {
        try
        {
            MyStringBuilder sb = MyStringBuilder.Create();
            bool is_first = true;
            foreach (var agent in agents)
            {
                if (!is_first) { sb.Append(","); } else { is_first = false; }
                sb.Append(agent.Guid);
                sb.Append(",");
                MyVector2 p = agent.Position;
                sb.Append((int)(p.x * GameServerConfig.CELL_SIZE_IN_PIXEL));
                sb.Append(",");
                sb.Append((int)(p.y * GameServerConfig.CELL_SIZE_IN_PIXEL));
            }
            db.StringSet("agent_pos_all", sb.ToStr());
        }catch(Exception e) { Log.e(e); }
    }
}
