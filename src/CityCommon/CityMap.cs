using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Plugins.CityCommon.Data;
using Plugins.CityCommon.Msg;
using Plugins.CityCommon.Server;

public class AgentModel
{
    public int agent_id;
    public string avatar;
    public string agent_name;
    public int primitive;
    public int character;
    public int creativity;
    public int charm;
    public int art_style;
    public int rebelliousness;
    public int energy;
    public int gold;
    public int health;
    public string user_id;
    public bool is_unpack;
    public bool is_pledged;
    public bool is_sleeping;
    public bool is_enter_city;
    public int mood;
}

public interface IRedis
{
    public void SubmitAgentState(SimAgent agent);
    public void SubmitAllAgentPos(List<SimAgent> agents);
}

public interface IMySql
{
    public List<AgentModel> GetAgentsToEnterCity();
    public void AgentEnterCity(SimAgent agent);

    public List<AgentModel> GetAgentsInCity(int count_limit);
    MyListDic<int, string> GetAllArtworks();
}

public class CityMap
{
    public SimAgentStatePool npc_state_pool = new SimAgentStatePool();
    public CityState city_state;
    public NavManagerOfCity nav_manager;
    public List<SimAgent> sim_agent_list = new List<SimAgent>();
    public Dictionary<int, SimAgent> sim_agent_dic = new Dic<int, SimAgent>();
    private Queue<ServerMsg> queue = new Queue<ServerMsg>();
    public Action<CityState> OnInitCallback;
    public Action<SimAgent> OnAgentBornCallback;
    public SimAgentBrainHelper agent_brain_helper;

    public CityMap()
    {
        city_state = new CityState();
        nav_manager = new NavManagerOfCity(this);
    }

    private IRedis redis;
    public IMySql mysql;
    internal void SetRedis(IRedis redis_controler)
    {
        this.redis = redis_controler;
    }
    internal void SetMySql(IMySql mysql_controler)
    {
        this.mysql = mysql_controler;
    }
    public void InitCity(CityState city_state)
    {
        this.city_state = city_state;

        //PrintAllMapObjInteractive();
    }

    private void PrintAllMapObjInteractive()
    {
        foreach (var t in city_state.map_obj_list_dic_of_interactive)
        {
            string name = t.Key;
            foreach (var obj in t.Value)
            {
                MyStringBuilder sb = MyStringBuilder.Create();
                sb.Append(obj.guid);
                sb.Append(",");
                sb.Append(name);
                sb.Append(",");
                sb.Append(city_state.GetCurAreaName(obj.pos));
                Log.d(sb.ToStr());
            }
        }
    }

    public SimAgent CreateSimAgent(AgentModel model)
    {
        //随机摆在一个位置
        MyVector2 pos = MyRandom.Choose(city_state.GetRandomMapObj().interactive_pos_list).ToMyVector2();
        
        IntelligentAgent il_agent = new IntelligentAgent(model.agent_id, model.avatar, GameServerConfig.AGENT_BASE_SPEED + MyRandom.NextFloat(0.1f));
        il_agent.postion = pos;
        
        //初始化属性
        il_agent.attr[AttrInfo.ENERGY] = 100;
        il_agent.attr[AttrInfo.GOLD] = 0;
        il_agent.attr[AttrInfo.PRIMITIVE] = model.primitive;
        il_agent.attr[AttrInfo.CHARACTER] = model.character;
        il_agent.attr[AttrInfo.CREATIVITY] = model.creativity;
        il_agent.attr[AttrInfo.CHARM] = model.charm;
        il_agent.attr[AttrInfo.ART_STYLE] = model.art_style;
        il_agent.attr[AttrInfo.REBELLIOUSNESS] = model.rebelliousness;
        il_agent.attr[AttrInfo.MOOD] = GameServerConfig.DEFAULT_MOOD;
        il_agent.attr[AttrInfo.HEALTH] = 100;
        
        return CreateSimAgent(il_agent);
    }

    public SimAgent CreateSimAgentForDebug(int agent_id)
    {
        //随机摆在一个位置
        MyVector2 pos = MyRandom.Choose(city_state.GetRandomMapObj().interactive_pos_list).ToMyVector2();
        
        IntelligentAgent il_agent = new IntelligentAgent(agent_id, "avatar ("+(agent_id%50+1)+")", GameServerConfig.AGENT_BASE_SPEED + MyRandom.NextFloat(0.1f));
        il_agent.postion = pos;
        
        //初始化属性
        il_agent.attr[AttrInfo.ENERGY] = 100;
        il_agent.attr[AttrInfo.GOLD] = 0;
        il_agent.attr[AttrInfo.PRIMITIVE] = MyRandom.NextInt(100);
        il_agent.attr[AttrInfo.CHARACTER] = MyRandom.NextInt(16);
        il_agent.attr[AttrInfo.CREATIVITY] = MyRandom.NextInt(100);
        il_agent.attr[AttrInfo.CHARM] = MyRandom.NextInt(100);
        il_agent.attr[AttrInfo.ART_STYLE] = MyRandom.NextInt(12);
        il_agent.attr[AttrInfo.REBELLIOUSNESS] = MyRandom.NextInt(100);
        il_agent.attr[AttrInfo.MOOD] = GameServerConfig.DEFAULT_MOOD;
        il_agent.attr[AttrInfo.HEALTH] = 100;
        
        return CreateSimAgent(il_agent);   
    }

    public SimAgent CreateSimAgent(IntelligentAgent il_agent)
    {
        SimAgent t = new SimAgent();
        city_state.intelligent_agent_dic[il_agent.id] = il_agent;
        t.SetInfo(this, il_agent, agent_brain_helper);
        sim_agent_list.Add(t);
        sim_agent_dic[t.Guid] = t;
        t.Position = il_agent.postion;
        t.ChangeStateToIdle();
        return t;
    }

    public void ReInit(CityState _city_state)
    {
        OnInitCallback?.Invoke(_city_state);
    }

    public bool IsLlmOnline()
    {
        if (game_logic_server_binder != null)
        {
            return game_logic_server_binder.IsLlmOnline();
        }
        return false;
    }

    public bool ExecutePlan(SimAgent agent, AgentPlanItem plan)
    {
        if (game_logic_server_binder != null)
        {
            return game_logic_server_binder.ExecutePlan(agent, plan);
        }
        return false;
    }
    public bool ExecuteDecision(SimAgent agent, LLmResponseAgentDecision decision)
    {
        if (game_logic_server_binder != null)
        {
            return game_logic_server_binder.ExecuteDecision(agent, decision);
        }
        return false;
    }


    public void RecreateSimAgent()
    {
        foreach (var kv in city_state.intelligent_agent_dic)
        {
            SimAgent t = new SimAgent();
            IntelligentAgent il_agent = kv.Value;
            t.SetInfo(this, il_agent, agent_brain_helper);
            t.SetOnMoveStoppedCb(OnSimAgentMoveStopped);
            t.SetOnIdleEndCb(OnSimAgentIdleEnd);
            //随机摆在一个位置
            t.Position = new MyVector2(il_agent.postion.x, il_agent.postion.y);
            t.ChangeStateToIdle();

            //此逻辑，需要给llm发送init数据之后，再执行
            //if (il_agent.path != null && il_agent.path.Count > 0)
            //{
            //    t.WalkToDo(il_agent.target_place_guid, il_agent.path, 1, null, 0, 0, null);
            //}
            
            sim_agent_list.Add(t);
            sim_agent_dic[t.Guid] = t;

            SubmitAgentState(t);
        }
    }


    private IGameLogicServerBinder game_logic_server_binder;
    public void SetGameLogicServerBinder(IGameLogicServerBinder binder)
    {
        this.game_logic_server_binder = binder;
    }
    
    private bool OnSimAgentMoveStopped(SimAgent sim_agent, Point pos, int cur_place_guid)
    {
        if (game_logic_server_binder != null)
        {
            return game_logic_server_binder.OnSimAgentMoveStopped(sim_agent, pos, cur_place_guid);
        }
        return false;
    }

    private bool OnSimAgentIdleEnd(SimAgent agent)
    {
        if (game_logic_server_binder != null)
        {
            return game_logic_server_binder.OnSimAgentIdleEnd(agent);
        }
        return false;
    }

    public void InitTerrain(CityState city_state)
    {
        foreach (var kv in city_state.map_obj_list_dic_of_not_interactive)
        {
            foreach (var obj in kv.Value)
            {
                if (obj.is_block)
                {
                    foreach (var p in obj.block_pos_list)
                    {
                        city_state.terrain_data[p.x, p.y] = TerrainMap.WATER;
                    }
                }
            }
        }
        foreach (var kv in city_state.map_obj_list_dic_of_interactive)
        {
            foreach (var obj in kv.Value)
            {
                if (obj.is_block)
                {
                    foreach (var p in obj.block_pos_list)
                    {
                        city_state.terrain_data[p.x, p.y] = TerrainMap.WATER;
                    }
                }
            }
        }
    }
    
    public void Tick()
    {
        for(int i=sim_agent_list.Count-1; i>=0; i--)
        {
            try { sim_agent_list[i].OnTick(); }  catch (Exception e) { Log.e(e); }
        }
        try { OnTick();}  catch (Exception e) { Log.e(e); }
        
    }
    
    public void OnNewDay()
    {
        agent_brain_helper?.OnNewDay();
        redis?.SubmitAllAgentPos(sim_agent_list);
    }

    public void OnNewMonth()
    {

    }

    public void OnNewYear()
    {

    }

    public SimAgent GetSimAgent(int agent_guid)
    {
        if (sim_agent_dic.TryGetValue(agent_guid, out SimAgent t))
        {
            return t;
        }
        else
        {
            Log.e("错误：不存在的agent_guid = " + agent_guid);
        }
        return null;
    }

    public MapObj GetPlaceRandomOfType(string target_place)
    {
        if (city_state.map_obj_list_dic_of_interactive.TryGetValue(target_place, out List<MapObj> list))
        {
            return MyRandom.Choose(list);
        }
        else
        {
            Log.e("错误：不存在的位置 target_place=" + target_place);
        }
        return null;
    }
    
    //处理消息
    public void OnTick()
    {
        if (queue.TryDequeue(out ServerMsg msg))
        {
            int index = msg.msg.IndexOf("@");
            if (index!=-1 && int.TryParse(msg.msg.Substring(0, index), out int msg_id))
            {
                Log.d(msg);
                DealMessage(msg.channel, msg.from, msg_id, msg.msg.Substring(index+1));
            }
            else
            {
                Log.e("错误：未知的消息类型" + msg.msg);
            }
        } 
    }

    public void OnMessage(string channel, string from, string msg)
    {
        queue.Enqueue(new ServerMsg(from, msg, channel));   
    }

    public void DealMessage(string channel, string from, int msg_id, string msg)
    {
        switch (msg_id)
        {
            case CityMsgId.CITY_MSG_INIT:
            {
                CityMsgInit m = JsonConvert.DeserializeObject<CityMsgInit>(msg);
                ReInit(m.city_state);
                break;
            }
            case CityMsgId.CITY_MSG_WALK_TO_DO:
            {
                CityMsgWalkToDo m = JsonConvert.DeserializeObject<CityMsgWalkToDo>(msg);
                GetSimAgent(m.agent_guid)?.WalkToDo(m.target_place_guid, m.path, m.speed_rate, m.emoji_on_the_way, m.emoji_interval, m.emoji_show_duration, m.song);
                break;
            }
            case CityMsgId.CITY_MSG_SPEAK:
            {
                CityMsgSpeak m = JsonConvert.DeserializeObject<CityMsgSpeak>(msg);
                GetSimAgent(m.agent_guid)?.DoSpeak(m.content, m.song, null, m.content_type, m.display_duration);
                break;
            }
            case CityMsgId.CITY_MSG_ACTION:
            {
                CityMsgAction m = JsonConvert.DeserializeObject<CityMsgAction>(msg);
                GetSimAgent(m.agent_guid)?.DoAction(m.content, m.attr_chagne, m.content_type, m.display_duration, m.song);
                break;
            }
            case CityMsgId.CITY_MSG_DIALOG:
                {
                    CityMsgDialog m = JsonConvert.DeserializeObject<CityMsgDialog>(msg);
                    GetSimAgent(m.agent_guid)?.StartDialog(m.dialog);
                    break;
                }
            case CityMsgId.CITY_MSG_AGENT_ATTR_CHANGE:
            {
                CityMsgAgentAttrChange m = JsonConvert.DeserializeObject<CityMsgAgentAttrChange>(msg);
                GetSimAgent(m.agent_guid)?.SetAttr(m.attr_id, m.attr_value);
                break;
            }
            case CityMsgId.CITY_MSG_USER_GOLD_CHANGE:
            {
                CityMsgUserGoldChange m = JsonConvert.DeserializeObject<CityMsgUserGoldChange>(msg);
                //SetUserGold(m.user_name, m.gold);
                break;
            }
            case CityMsgId.CITY_MSG_NFT:
            {
                CityMsgNft m = JsonConvert.DeserializeObject<CityMsgNft>(msg);
                GetSimAgent(m.agent_guid)?.OnCreateNft(m.artwork_id);
                break;
            }
            case CityMsgId.CITY_MSG_AGENT_LOSE_ARTWORK:
            {
                CityMsgAgentLoseArtwork m = JsonConvert.DeserializeObject<CityMsgAgentLoseArtwork>(msg);
                GetSimAgent(m.agent_guid)?.RemoveArtwork(m.artwork_id); 
                break;
            }
            case CityMsgId.CITY_MSG_AGENT_GET_ARTWORK:
            {
                CityMsgAgentGetArtwork m = JsonConvert.DeserializeObject<CityMsgAgentGetArtwork>(msg);
                GetSimAgent(m.agent_guid)?.AddArtwork(m.artwork_id); 
                break;
            }
            case CityMsgId.CITY_MSG_USER_LOSS_ARTWORK:
            {
                CityMsgUserLossArtwork m = JsonConvert.DeserializeObject<CityMsgUserLossArtwork>(msg);
                //RemoveUserArtwork(m.user_name, m.artwork_id);
                break;
            }
            case CityMsgId.CITY_MSG_USER_GET_ARTWORK:
            {
                CityMsgUserGetArtwork m = JsonConvert.DeserializeObject<CityMsgUserGetArtwork>(msg);
                //AddUserArtwork(m.user_name, m.artwork_id);
                break;
            }
            case CityMsgId.CITY_MSG_AGENT_BORN:
            {
                CityMsgAgentBorn m = JsonConvert.DeserializeObject<CityMsgAgentBorn>(msg);
                SimAgent agent = CreateSimAgent(m.il_agent);
                Log.d("新智能体进入：" + msg);
                OnAgentBornCallback?.Invoke(agent);
                break;
            }
            default:
                Log.e("错误：未处理的消息类型 channel=" + channel + " from=" + from + " msg_id=" + msg_id + " msg=" + msg);
                break;
        }
        
    }
    
    public void Snapshoot()
    {
        foreach (var t in sim_agent_list)
        {
            t.Snapshoot();
        }
    }
    
    
    
    
    public List<SimAgent> GetSimAgentInSameArea(SimAgent agent)
    {
        List<SimAgent> r = new List<SimAgent>();
        city_state.GetAreaRect(agent.CurPoint, out MyRectInt rect);
        foreach (var t in sim_agent_list)
        {
            if (t.Guid == agent.Guid)
            {
                continue;
            }
            
            if (rect.Contains(t.CurPoint))
            {
                r.Add(t);
            }
        }
        return r;
    }

    public List<int> GetAgentsAround(Point p, int exclude_agent_guid)
    {
        List<int> r = new List<int>();
        if(city_state.GetAreaRect(p, out MyRectInt rect))
        {
            foreach (var t in sim_agent_list)
            {
                if (t.Guid == exclude_agent_guid)
                {
                    continue;
                }

                if (rect.Contains(t.CurPoint))
                {
                    r.Add(t.Guid);
                }
            }
        }
        return r;
    }

    public List<int> GetAgentsIdleAround(Point p, int exclude_agent_guid)
    {
        List<int> r = new List<int>();
        if (city_state.GetAreaRect(p, out MyRectInt rect))
        {
            foreach (var t in sim_agent_list)
            {
                if (t.Guid == exclude_agent_guid)
                {
                    continue;
                }

                if(!(t.cur_npc_state is SimAgentStateIdle))
                {
                    continue;
                }

                if (rect.Contains(t.CurPoint))
                {
                    r.Add(t.Guid);
                }
            }
        }
        return r;
    }
    internal bool IsInDoor(Point p)
    {
        return city_state.IsInDoor(p);
    }

    public void GetCurAreaName(Point pos, out string cur_area)
    {
        cur_area = city_state.GetCurAreaName(pos);
    }

    public MapObj GetMapObjByPoint(Point p)
    {
        return city_state.GetMapObjByPoint(p);
    }
    public MapObj GetMapObjByGuid(int guid)
    {
        return city_state.GetMapObjByGuid(guid);
    }

    internal void SubmitAgentState(SimAgent npc)
    {
        redis?.SubmitAgentState(npc);
    }

    public void InitCallback(Action<CityState> reInit, Action<SimAgent> agentBorn)
    {
        this.OnInitCallback = reInit;
        this.OnAgentBornCallback = agentBorn;
    }
}