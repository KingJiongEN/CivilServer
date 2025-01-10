using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Plugins.CityCommon.Data;
using Plugins.CityCommon.Msg;
using Plugins.CityCommon.Server;

public class LLMServer : SockServer
{
    public bool is_stop_by_cmd = false;
    public virtual void StartPythonServer(string python_script_path = null)
    {
    }

    public override void OnTick()
    {
        base.OnTick();
    }
    
    protected override string GetUserName()
    {
        return GameServerConfig.USER_NAME_LLM;
    }
    protected override List<string> GetChannelList()
    {
        return new List<string>() { GameServerConfig.CHANNEL_CITY_SERVER };
    }

    private bool is_start = false;
    protected virtual void DoOnNewDay(int days)
    {
        if (is_stop_by_cmd)
        {
            return;
        }
        
        if (!is_init || !IsConnected())
        {
            return;
        }
        
        //if (!is_start)
        //{
        //    is_start = true;
        //    //启动时，全部都动起来
        //    foreach (var t in city_map.sim_agent_list)
        //    {
        //        if (t.IsIdle() && !IsCooldown(t.Guid, days))
        //        {
        //            MapObj obj = GetRandomMapObjExceptBed();
        //            if (obj != null)
        //            {
        //                GetRandomEmojiOnTheWay(out string emoji_on_the_way, out float emoji_interval, out float emoji_show_duration,
        //                    out string song);
        //                SendMsgToServer( LLmMsgWalkToDo.GetMsg(t.Guid, obj.guid, MyRandom.Between(0.25f, 2f), emoji_on_the_way, emoji_interval, emoji_show_duration, song), "*");
        //                last_send_msg_time_dic[t.Guid] = days;
        //            }
        //        }
        //    }
        //}
        //else
        //{
        //    foreach (var t in city_map.sim_agent_list)
        //    {
        //        if (t.IsIdle() && !IsCooldown(t.Guid, days))
        //        {
        //            MapObj obj = GetRandomMapObjExceptBed();
        //            if (obj != null)
        //            {
        //                if (obj.name.Equals("Workshop"))
        //                {
        //                    if (CreateNft(out string url, out string artwork_id))
        //                    {
        //                        SendMsgToServer(
        //                            LLmMsgNft.GetMsg(t.Guid, artwork_id, url),
        //                            "*");
        //                        last_send_msg_time_dic[t.Guid] = days;
        //                        return;
        //                    }
        //                }

        //                //交易撮合,身上有艺术品的人发起
        //                if (t.HasAnyArtwork())
        //                {
        //                    string artwork_id = t.GetRandomArtwork();
        //                    List<SimAgent> agent_around = city_map.GetSimAgentInSameArea(t);
        //                    int price = 0;
        //                    SimAgent agent_to_buy = null;
        //                    foreach (var _agent_to_buy in agent_around)
        //                    {
        //                        int gold = _agent_to_buy.GetGoldCount();
        //                        if (gold > 0)
        //                        {
        //                            int _price = MyRandom.Between(1, gold);
        //                            if (_price > price)
        //                            {
        //                                price = _price;
        //                                agent_to_buy = _agent_to_buy;
        //                            }
        //                        }
        //                    }

        //                    if (agent_to_buy != null)
        //                    {
        //                        SendMsgToServer(LLmMsgArtworkTrade.GetMsg(t.Guid, agent_to_buy.Guid, artwork_id, price), "*");
        //                        last_send_msg_time_dic[t.Guid] = days;
        //                        last_send_msg_time_dic[agent_to_buy.Guid] = days;
        //                        return;
        //                    }
        //                }

                       
        //                //智能体没钱了
        //                if(t.GetGoldCount () <= 0)
        //                {
        //                    MapObj government_office = GetGovernmentOffice();
        //                    if (government_office != null)
        //                    {
        //                        if (government_office.interactive_pos_list.Contains(t.CurPoint))
        //                        {
        //                            if (t.HasAnyArtwork())
        //                            {
        //                                //回收艺术品
        //                                SendMsgToServer(LLmMsgArtworkRecycle.GetMsg(t.Guid, MyRandom.Choose(t.il_agent.artwork_list), 1000), "*");
        //                                return;
        //                            }
        //                            else
        //                            {
        //                                //领救济
        //                                SendMsgToServer(LLmMsgReceiveSubsistenceAllowances.GetMsg(t.Guid, 100, 100), "*");
        //                                return;
        //                            }
        //                        }
        //                        else
        //                        {

        //                            if (t.HasAnyArtwork())
        //                            {
        //                                //走过去：回收艺术品
        //                                SendMsgToServer(LLmMsgWalkToDo.GetMsg(t.Guid, government_office.guid, MyRandom.Between(0.25f, 2f), "🤑", 1f, 2f, null), "*");
        //                                return;
        //                            }
        //                            else
        //                            {
        //                                //走过去：领救济
        //                                SendMsgToServer(LLmMsgWalkToDo.GetMsg(t.Guid, government_office.guid, MyRandom.Between(0.25f, 2f), "💰", 1f, 2f, null), "*");
        //                                return;
        //                            }
        //                        }
        //                    }
        //                    else
        //                    {
        //                        Log.e("错误，未找到政府办公室");
        //                    }
        //                }

        //                //随机移动
        //                GetRandomEmojiOnTheWay(out string emoji_on_the_way, out float emoji_interval, out float emoji_show_duration,
        //                    out string song);

        //                if (MyRandom.NextBool())
        //                {
        //                    SendMsgToServer(LLmMsgWalkToDo.GetMsg(t.Guid, obj.guid, MyRandom.Between(0.25f, 2f), emoji_on_the_way, emoji_interval, emoji_show_duration, song), "*");
        //                }
        //                else
        //                {
        //                    //走到某个人的跟前
        //                    Point pos = MyRandom.Choose(city_map.sim_agent_list).CurPoint;
        //                    SendMsgToServer(LLmMsgWalkTo.GetMsg(pos, t.Guid, MyRandom.Between(0.25f, 2f), emoji_on_the_way, emoji_interval, emoji_show_duration, song), "*");
        //                }
                        
        //                last_send_msg_time_dic[t.Guid] = days;
        //                return;
                        
        //            }
        //        }
        //    }
        //}
    }
    
    public const int MAX_NFT_INDEX = 107;
    public int nft_index_to_create;
    private bool CreateNft(out string url, out string artwork_id)
    {
        url = null;
        artwork_id = null;
        int index = nft_index_to_create;
        if (index < MAX_NFT_INDEX)
        {
            url = "nft/img (" + index + ").png";
            artwork_id = "nft" + index;
            nft_index_to_create++;
            return true;
        }
        return false;
    }

    public void SendMsgToServer(string msg, string target = "*")
    {
        websocket_manager[GameServerConfig.CHANNEL_CITY_SERVER].SendMsg(msg, target);
    }
    
    

    private List<string> place_list = new List<string>();
    public MapObj GetRandomMapObjExceptBed()
    {
        if (place_list.Count == 0)
        {
            place_list.AddRange(city_map.city_state.map_obj_list_dic_of_interactive.Keys);
            place_list.Remove("small bed");
            place_list.Remove("big bed");
            place_list.Remove("Entrance");
        }
        return MyRandom.Choose(city_map.city_state.map_obj_list_dic_of_interactive[MyRandom.Choose(place_list)]);
    }

    public MapObj GetGovernmentOffice()
    {
        if(city_map.city_state.map_obj_list_dic_of_interactive.TryGetValue("Office Desk", out List<MapObj> list))
        {
            return MyRandom.Choose(list);
        }
        return null;
    }

    private Dictionary<int, int> last_send_msg_time_dic = new Dic<int, int>();
    private bool IsCooldown(int t,int days)
    {
        if (last_send_msg_time_dic.TryGetValue(t, out int time))
        {
            if (days - time <= 5)
            {
                return true;
            }
        }
        return false;
    }

    public override void DealMessage(string channel, string from, int msg_id, string msg)
    {
        if (GetUserName().Equals(from))
        {
            //自己发出去的消息，不必处理
            return;
        }
        
        if (GameServerConfig.CHANNEL_CITY_SERVER.Equals(channel))
        {
            if (GameServerConfig.USER_NAME_GAME_SERVER.Equals(from))
            {
                switch (msg_id)
                {
                    case ServerMsgId.SERVER_MSG_INIT:
                    {
                        //服务器状态同步
                        break;
                    }
                    case ServerMsgId.SERVER_MSG_ACK:
                        break;
                    case ServerMsgId.LLM_REQUEST_AGENT_PLAN:
                        //行程计划
                        {
                            ServerRequestAgentPlan m = JsonConvert.DeserializeObject<ServerRequestAgentPlan>(msg);
                            SendMsgToServer(GetRandomPlan(m.agent_guid).ToMsg(), "*");
                        }
                        break;
                    case ServerMsgId.LLM_REQUEST_AGENT_DECISION:
                        //具体行为决策
                        {
                            ServerRequestAgentDecision m = JsonConvert.DeserializeObject<ServerRequestAgentDecision>(msg);
                            SendMsgToServer(GetRandomDecsion(m).ToMsg(), "*");
                        }
                        break;
                    case ServerMsgId.SERVER_MSG_AGENT_BORN:
                    {
                        Log.d("LLMServer 智能体出生：" + msg);
                        break;
                    }
                    case ServerMsgId.SERVER_MSG_USER_BUY_ARTWORK:
                    {
                        //交易
                        ServerMsgUserBuyArtwork m = JsonConvert.DeserializeObject<ServerMsgUserBuyArtwork>(msg);
                        SimAgent agent = GetSimAgent(m.from_agent_id);
                        bool is_succ = false;
                        string error_msg = null;
                       
                        if (agent != null)
                        {
                            error_msg = "The trading function is not implemented in LLMServer";
                        }
                        else
                        {
                            error_msg = "The agent does not exist";
                        }
                        SendMsgToServer(LLmMsgUserBuyArtworkResult.GetMsg(is_succ, error_msg, m.price, m.artwork_id, m.from_agent_id, m.to_user_name), "*");
                        break;
                    }
                    case ServerMsgId.SERVER_MSG_USER_SELL_ARTWORK:
                    {
                        //交易
                        ServerMsgUserSellArtwork m = JsonConvert.DeserializeObject<ServerMsgUserSellArtwork>(msg);
                        SimAgent agent = GetSimAgent(m.to_agent_id);
                        bool is_succ = false;
                        string error_msg = null;
                       
                        if (agent != null)
                        {
                            error_msg = "The trading function is not implemented in LLMServer";
                        }
                        else
                        {
                            error_msg = "The agent does not exist";
                        }
                        SendMsgToServer(LLmMsgUserSellArtworkResult.GetMsg(is_succ, error_msg, m.price, m.artwork_id, m.to_agent_id, m.from_user_name), "*");
                        break;
                    }
                    default:
                        LogError(channel, from, msg_id, msg);
                        break;
                }
            }
            else
            {
                LogError(channel, from, msg_id, msg);
            }
        }
        else
        {
            LogError(channel, from, msg_id, msg);
        }
    }
    
    private LLmResponseAgentPlan GetRandomPlan(int agent_guid)
    {
        List<AgentPlanItem> plan = new List<AgentPlanItem> ();       
        for(int i=0;i<20;i++)
        {
            MapObj obj = GetRandomMapObjExceptBed();
            AgentPlanItem t = new AgentPlanItem();

            t.target_place_guid = obj.guid;
            t.mood = MyRandom.Between(60, 80);
            GetRandomEmojiOnTheWay(out string emoji_on_the_way, out float emoji_interval, out float emoji_show_duration,out string song);
            t.speed_rate = MyRandom.Between(1f, 2f);
            t.emoji_interval = emoji_interval;
            t.emoji_on_the_way = emoji_on_the_way; 
            t.emoji_show_duration = emoji_show_duration;
            t.song = song;

            plan.Add(t);
        }

        return new LLmResponseAgentPlan(agent_guid, plan);
    }

    private LLmResponseAgentDecision GetRandomDecsion(ServerRequestAgentDecision m)
    {
        LLmResponseAgentDecision r = new LLmResponseAgentDecision(m.agent_guid);
        SimAgent agent = GetSimAgent(m.agent_guid);

        float random_value = MyRandom.NextFloat(1f);
        if (random_value < 0.1f)
        {
            if (MyRandom.NextBool())
            {
                r.error_code = (int)LLmResponseErrorCode.BUSY;
                r.error_msg = "AI忙";
            }
            else
            {
                r.error_code = (int)LLmResponseErrorCode.TIMEOUT;
                r.error_msg = "AI超时";
            }
            return r;
        }

        MapObj map_obj = city_map.GetMapObjByPoint(agent.CurPoint);
        if (random_value < 0.2f && map_obj != null)
        {
            DecisionPlaceInteraction place_interaction = new DecisionPlaceInteraction();
            place_interaction.target_place_guid = map_obj.guid;
            place_interaction.is_asleep = false;
            place_interaction.monologue = GetRandomMonologue();
            if (MyRandom.NextBool())
            {
                place_interaction.attr_chagne = GetRandomValueChange(agent);
            }
            r.place_interaction = place_interaction;
            return r;
        }

        if (random_value < 0.4f && m.angents_around != null && m.angents_around.Count > 0)
        {

            r.dialog = GetRandomDialog(agent.Guid, MyRandom.Choose(m.angents_around));
            return r;
        }

        if (random_value < 0.5f)
        {
            r.draw = GetRandomDraw();
            if (r.draw != null)
            {
                return r;
            }
        }

        if (random_value < 0.6f)
        {
            r.trade = GetRandomTrade();
            if (r.trade != null)
            {
                return r;
            }
        }

        if (random_value < 0.8f)
        {
            r.move = GetRandomMove(agent.Guid);
            if (r.move != null)
            {
                return r;
            }
        }

        if (random_value < 0.9f)
        {
            MapObj government_office = GetGovernmentOffice();
            if (government_office != null)
            {
                if (government_office.interactive_pos_list.Contains(agent.CurPoint))
                {
                    if (agent.HasAnyArtwork())
                    {
                        string artwork_id = agent.GetRandomArtwork();
                        r.artwork_recycle = new DecisionArtworkRecycle(agent.Guid, artwork_id, 1000);
                        return r;
                    }
                }
             }
        }

        r.monologue = GetRandomMonologue();
        return r;
    }

    private DecisionTrade? GetRandomTrade()
    {
        return null;
    }

    private DecisionDraw? GetRandomDraw()
    {
        return null;
    }

    private DecisionMove? GetRandomMove(int agent_guid)
    {
        DecisionMove r = new DecisionMove();
        GetRandomEmojiOnTheWay(out string emoji_on_the_way, out float emoji_interval, out float emoji_show_duration, out string song);
        r.emoji_interval = emoji_interval;
        r.emoji_on_the_way = emoji_on_the_way;
        r.emoji_show_duration = emoji_show_duration;
        r.song = song;
        r.speed_rate = MyRandom.Between(1f, 2f);
        r.agent_guid = agent_guid;
        r.target_place_guid = GetRandomMapObjExceptBed().guid;
        return r;
    }

    private DecisionDialog GetRandomDialog(int agent_guid, int target_agent_guid)
    {
        DecisionDialog t = new DecisionDialog();
        t.agent_guid = agent_guid;
        t.agent_guid_object = target_agent_guid;
        List<Words> words_list = new List<Words>();
        int count = MyRandom.Between(3, 8);
        for(int i = 0; i < count; i++)
        {
            if (i % 2 == 0)
            {
                words_list.Add(GetRandomWords(agent_guid, i, target_agent_guid));
            }
            else
            {
                words_list.Add(GetRandomWords(target_agent_guid, i, agent_guid));
            }
            
        }
        t.words_list = words_list;
        return t;
    }

    private Words GetRandomWords(int agent_guid, int index, int listener_agent_guid)
    {
        Words t = new Words();
        t.agent_guid = agent_guid;
        t.content_type = MyRandom.NextInt(3);
        if (MyRandom.NextBool(0.1f))
        {
            t.song = MyRandom.Choose(ConfigManager.Ins.song_list).song_name;
        }
        int count = MyRandom.Between(3, 8);
        MyStringBuilder sb = MyStringBuilder.Create();
        for (int i = 0; i < count; i++)
        {
            sb.Append("Chat content content content[" + agent_guid + " -> "+ listener_agent_guid + "] [" + index + "] [" + i + "/" + count + "]" +  "\n");
        }
        sb.Append("Chat content content content[" + agent_guid + " -> " + listener_agent_guid + "] [" + index + "]" + " end");
        t.content = sb.ToStr();
        return t;
    }

    private DecisionMonologue GetRandomMonologue()
    {
        DecisionMonologue t = new DecisionMonologue();
        t.content_type = MyRandom.NextInt(3);
        t.display_duration = MyRandom.NextInt(10);
        if (MyRandom.NextBool(0.25f))
        {
            t.song = MyRandom.Choose(ConfigManager.Ins.song_list).song_name;
        }
        MyStringBuilder sb = MyStringBuilder.Create();
        int count = MyRandom.Between(3, 8);
        for(int i= 0; i< count; i++)
        {
            sb.Append("Monologue content content content" + i + "/" + count + "\n");
        }
        sb.Append("Monologue end");
        t.content = sb.ToStr();
        return t;
    }

    public SimAgent GetSimAgent(int agent_guid)
    {
        return city_map.GetSimAgent(agent_guid);
    }
    private List<AttrChange> GetRandomValueChange(SimAgent agent)
    {
        List<AttrChange> r = new List<AttrChange>();
        float n = MyRandom.NextFloat(1);
        if (n < 1 / 3f)
        {
            AddRandomAttrChagne(r, agent);
        }
        else if (n < 2 / 3f)
        {
            AddRandomAttrChagne(r, agent);
            AddRandomAttrChagne(r, agent);
        }
        return r;
    }

    private void AddRandomAttrChagne(List<AttrChange> r, SimAgent agent)
    {
        AttrInfo t = MyRandom.Choose(ConfigManager.Ins.attr_variable_list);
        int value = agent.GetAttr(t.attr_id);
        int value_change = MyRandom.NextBool() ? 1 : -1;
        r.Add(new AttrChange(t.attr_id, value + value_change, value_change));
    }

    public int GetRandomDuration()
    {
        return (int)(MyRandom.Between(10, 20) * GameServerConfig.TIME_SCALE);
    }

    public string GetRandomContent(string cur_area, int cur_place_guid)
    {
        MapObj map_obj = city_map.city_state.GetMapObjByGuid(cur_place_guid);
        string cur_place = null;
        if (map_obj != null)
        {
            cur_place = map_obj.name;
        }
        MyStringBuilder sb = MyStringBuilder.Create();
        float n = MyRandom.NextFloat(1);
        if (n<1/3f)
        {
            sb.Append(MyRandom.Choose(random_words));
        }
        else if (n < 2 / 3f)
        {
            sb.Append(GetRandomEmoji());
        }
        else
        { 
            sb.Append(MyRandom.Choose(random_words));
            sb.Append(GetRandomEmoji());
        }

        int total_line = MyRandom.Between(3, 15);
        for (int i = 0; i < total_line; i++)
        {
            sb.Append("\n");
            sb.Append(MyRandom.Choose(random_words));
        }

        sb.Append("[");
        sb.Append(cur_area);
        sb.Append(" ");
        sb.Append(cur_place);
        sb.Append("]");
        return sb.ToStr();
    }

    public string GetRandomEmoji()
    {
        return char.ConvertFromUtf32(MyRandom.Choose(emoji));
    }
    
    private void GetRandomEmojiOnTheWay(out string emoji_on_the_way, out float emoji_interval, out float emoji_show_duration,
        out string song)
    {
        if (MyRandom.NextBool(0.25f))
        {
            emoji_on_the_way = GetRandomEmoji();
            emoji_interval = 1f;
            emoji_show_duration = 2f;
            song = MyRandom.Choose(ConfigManager.Ins.song_list).song_name;
        }
        else
        {
            emoji_on_the_way = null;
            emoji_interval = 0;
            emoji_show_duration = 0;
            song = null;
        }
    }

    public List<string> random_words = new List<string>()
    {
        "Hello",
        "How can I help you?",
        "Thank you",
        "Sorry",
        "It's okay",
        "Please wait a moment",
        "What is your name?",
        "My name is [your name]",
        "Nice to meet you",
        "Goodbye"
    };

    public List<int> emoji = new List<int>()
    {
        126980, 127183, 127374, 127377, 127378, 127379, 127380, 127381, 127382, 127383, 127384, 127385, 127386, 127489,
        127514, 127535, 127538, 127539, 127540, 127541, 127542, 127544, 127545, 127546, 127568, 127569, 127789, 127790,
        127791, 127792, 127793, 127794, 127795, 127796, 127797, 127799, 127800, 127801, 127802, 127803, 127804, 127805,
        127806, 127807, 127808, 127809, 127810, 127811, 127812, 127813, 127814, 127815, 127816, 127817, 127818, 127819,
        127820, 127821, 127822, 127823, 127824, 127825, 127826, 127827, 127828, 127829, 127830, 127831, 127832, 127833,
        127834, 127835, 127836, 127837, 127838, 127839, 127840, 127841, 127842, 127843, 127844, 127845, 127846, 127847,
        127848, 127849, 127850, 127851, 127852, 127853, 127854, 127855, 127856, 127857, 127858, 127859, 127860, 127861,
        127862, 127863, 127864, 127865, 127866, 127867, 127868, 127870, 127871, 127872, 127873, 127874, 127875, 127876,
        127877, 127878, 127879, 127880, 127881, 127882, 127883, 127885, 127886, 127887, 127888, 127889, 127890, 127891,
        127907, 127908, 127909, 127910, 127911, 127912, 127913, 127915, 127916, 127917, 127918, 127919, 127920, 127921,
        127922, 127923, 127924, 127925, 127926, 127927, 127928, 127929, 127930, 127931, 127932, 127933, 127934, 127935,
        127936, 127938, 127939, 127940, 127941, 127942, 127943, 127944, 127945, 127946, 127951, 127952, 127953, 127954,
        127955, 127975, 127982, 127992, 127993, 127994, 127995, 127996, 127997, 127998, 127999, 128000, 128001, 128002,
        128003, 128004, 128005, 128006, 128007, 128008, 128009, 128010, 128011, 128012, 128013, 128014, 128015, 128016,
        128017, 128018, 128019, 128020, 128021, 128022, 128023, 128024, 128025, 128026, 128027, 128028, 128029, 128030,
        128031, 128032, 128033, 128034, 128035, 128036, 128037, 128038, 128039, 128040, 128041, 128042, 128043, 128044,
        128045, 128046, 128047, 128048, 128049, 128050, 128051, 128052, 128053, 128054, 128055, 128056, 128057, 128058,
        128059, 128060, 128061, 128062, 128064, 128066, 128067, 128068, 128069, 128070, 128071, 128072, 128073, 128074,
        128075, 128076, 128077, 128078, 128079, 128080, 128081, 128082, 128083, 128084, 128085, 128086, 128087, 128088,
        128089, 128090, 128091, 128092, 128093, 128094, 128095, 128096, 128097, 128098, 128099, 128100, 128101, 128102,
        128103, 128104, 128105, 128106, 128107, 128108, 128109, 128110, 128111, 128112, 128113, 128114, 128115, 128116,
        128117, 128118, 128119, 128120, 128121, 128122, 128123, 128124, 128125, 128126, 128127, 128128, 128129, 128130,
        128131, 128132, 128133, 128134, 128135, 128137, 128138, 128139, 128140, 128141, 128142, 128143, 128144, 128145,
        128147, 128148, 128149, 128150, 128151, 128152, 128153, 128154, 128155, 128156, 128157, 128158, 128159, 128160,
        128161, 128162, 128163, 128164, 128165, 128166, 128168, 128169, 128170, 128171, 128172, 128173, 128174, 128175,
        128176, 128177, 128178, 128179, 128180, 128181, 128182, 128183, 128184, 128185, 128187, 128188, 128189, 128190,
        128191, 128192, 128193, 128194, 128195, 128196, 128197, 128198, 128199, 128200, 128201, 128202, 128203, 128204,
        128205, 128206, 128207, 128208, 128209, 128210, 128211, 128212, 128213, 128214, 128215, 128216, 128217, 128218,
        128219, 128220, 128221, 128222, 128223, 128224, 128225, 128226, 128227, 128228, 128229, 128230, 128231, 128232,
        128233, 128234, 128235, 128236, 128237, 128238, 128239, 128240, 128241, 128242, 128243, 128244, 128245, 128246,
        128247, 128248, 128249, 128250, 128251, 128252, 128255, 128256, 128257, 128258, 128259, 128260, 128261, 128262,
        128263, 128264, 128265, 128266, 128267, 128268, 128269, 128270, 128271, 128272, 128273, 128274, 128275, 128276,
        128277, 128278, 128279, 128280, 128281, 128282, 128283, 128284, 128285, 128286, 128287, 128288, 128289, 128290,
        128291, 128292, 128294, 128295, 128296, 128297, 128298, 128299, 128300, 128301, 128302, 128303, 128304, 128305,
        128306, 128307, 128308, 128309, 128310, 128311, 128312, 128313, 128314, 128315, 128316, 128317, 128334, 128378,
        128405, 128406, 128420, 128511, 128512, 128513, 128514, 128515, 128516, 128517, 128518, 128519, 128520, 128521,
        128522, 128523, 128524, 128525, 128526, 128527, 128528, 128529, 128530, 128531, 128532, 128533, 128534, 128535,
        128536, 128537, 128538, 128539, 128540, 128541, 128542, 128543, 128544, 128545, 128546, 128547, 128548, 128549,
        128550, 128551, 128552, 128553, 128554, 128555, 128556, 128557, 128558, 128559, 128560, 128561, 128562, 128563,
        128564, 128565, 128566, 128567, 128568, 128569, 128570, 128571, 128572, 128573, 128574, 128575, 128576, 128577,
        128578, 128579, 128580, 128581, 128582, 128583, 128584, 128585, 128586, 128587, 128588, 128589, 128590, 128591,
        128675, 128682, 128683, 128684, 128685, 128686, 128687, 128688, 128689, 128691, 128692, 128693, 128694, 128695,
        128696, 128697, 128698, 128699, 128700, 128701, 128702, 128703, 128704, 128705, 128706, 128707, 128708, 128709,
        128716, 128720, 128722, 128727, 128732, 128759, 128992, 128993, 128994, 128995, 128996, 128997, 128998, 128999,
        129000, 129001, 129002, 129003, 129008, 129292, 129293, 129294, 129295, 129296, 129297, 129298, 129299, 129300,
        129301, 129302, 129303, 129304, 129305, 129306, 129307, 129308, 129309, 129310, 129311, 129312, 129313, 129314,
        129315, 129316, 129317, 129318, 129319, 129320, 129321, 129322, 129323, 129324, 129325, 129326, 129327, 129328,
        129329, 129330, 129331, 129332, 129333, 129334, 129335, 129336, 129337, 129338, 129340, 129341, 129342, 129343,
        129344, 129345, 129346, 129347, 129348, 129349, 129351, 129352, 129353, 129354, 129355, 129356, 129357, 129358,
        129359, 129360, 129361, 129362, 129363, 129364, 129365, 129366, 129367, 129368, 129369, 129370, 129371, 129372,
        129373, 129374, 129375, 129376, 129377, 129378, 129379, 129380, 129381, 129382, 129383, 129384, 129385, 129386,
        129387, 129388, 129389, 129390, 129391, 129392, 129393, 129394, 129395, 129396, 129397, 129398, 129399, 129400,
        129401, 129402, 129403, 129404, 129405, 129406, 129407, 129408, 129409, 129410, 129411, 129412, 129413, 129414,
        129415, 129416, 129417, 129418, 129419, 129420, 129421, 129422, 129423, 129424, 129425, 129426, 129427, 129428,
        129429, 129430, 129431, 129432, 129433, 129434, 129435, 129436, 129437, 129438, 129439, 129440, 129441, 129442,
        129443, 129444, 129445, 129446, 129447, 129448, 129449, 129450, 129451, 129452, 129453, 129454, 129455, 129460,
        129461, 129462, 129463, 129464, 129465, 129466, 129467, 129470, 129471, 129472, 129473, 129474, 129475, 129476,
        129477, 129478, 129479, 129480, 129481, 129482, 129483, 129484, 129485, 129486, 129487, 129488, 129489, 129490,
        129491, 129492, 129493, 129494, 129495, 129496, 129497, 129498, 129499, 129500, 129501, 129502, 129503, 129504,
        129505, 129506, 129507, 129508, 129509, 129510, 129511, 129512, 129513, 129514, 129515, 129516, 129518, 129519,
        129520, 129522, 129524, 129525, 129526, 129527, 129528, 129529, 129530, 129531, 129532, 129533, 129534, 129535,
        129648, 129649, 129650, 129651, 129652, 129653, 129654, 129655, 129656, 129657, 129658, 129659, 129660, 129664,
        129665, 129667, 129668, 129669, 129670, 129671, 129672, 129681, 129682, 129683, 129684, 129685, 129686, 129687,
        129688, 129689, 129690, 129691, 129692, 129693, 129694, 129695, 129696, 129697, 129698, 129699, 129700, 129701,
        129702, 129703, 129705, 129706, 129707, 129708, 129709, 129710, 129711, 129712, 129713, 129714, 129715, 129716,
        129718, 129719, 129720, 129721, 129722, 129723, 129724, 129725, 129727, 129728, 129729, 129730, 129731, 129732,
        129733, 129742, 129743, 129744, 129745, 129746, 129747, 129748, 129749, 129750, 129751, 129752, 129753, 129754,
        129755, 129760, 129761, 129762, 129763, 129764, 129765, 129766, 129767, 129768, 129776, 129777, 129778, 129779,
        129780, 129781, 129782, 129783, 129784, 9193, 9194, 9195, 9196, 9725, 9726, 9749, 9800, 9801, 9802, 9803, 9804,
        9805, 9806, 9807, 9808, 9809, 9810, 9811, 9855, 9898, 9899, 9917, 9918, 9934, 9940, 9971, 9989, 9994, 9995,
        10024, 10060, 10062, 10067, 10068, 10069, 10071, 10133, 10134, 10135, 10160, 10175, 11035, 11036, 11093
    };
}