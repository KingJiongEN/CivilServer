using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Plugins.CityCommon.Data;
using Plugins.CityCommon.Msg;
using Plugins.CityCommon.Server;

public interface IGameLogicServerBinder
{
    public bool IsLlmOnline();
    public bool OnSimAgentIdleEnd(SimAgent agent);
    public bool OnSimAgentMoveStopped(SimAgent agent, Point pos, int cur_place_guid);
    public bool ExecutePlan(SimAgent agent, AgentPlanItem plan);
    public bool ExecuteDecision(SimAgent agent, LLmResponseAgentDecision decision);
}

public class GameLogicServer : SockServer, IGameLogicServerBinder
{
    public override void OnTick()
    {
        base.OnTick();
    }
   
    protected override void DoOnNewDay()
    {
        base.DoOnNewDay();

        //尝试创建新的AGENT
        try { TryAgentEtnerCity(); } catch (Exception e) { Log.e(e); }
    }

    private void TryAgentEtnerCity()
    {
        if (city_map.mysql != null)
        {
            List<AgentModel> list = city_map.mysql.GetAgentsToEnterCity();
            if (list != null)
            {
                foreach (AgentModel t in list)
                {
                    if (!city_map.sim_agent_dic.ContainsKey(t.agent_id))
                    {
                        SimAgent agent = city_map.CreateSimAgent(t);

                        //通知AI，新建AGENT
                        SendMsgToServer(ServerMsgAgentBorn.GetMsg(agent.Guid));

                        //通知前端游戏
                        SendMsgToCity(CityMsgAgentBorn.GetMsg(agent.il_agent), "*");

                        //更新数据据
                        city_map.mysql.AgentEnterCity(agent);
                    }
                    else
                    {
                        Log.e("错误：重复进入城市" + t.agent_id);
                    }
                }
            }
        }
    }

    protected override string GetUserName()
    {
        return GameServerConfig.USER_NAME_GAME_SERVER;
    }
    
    protected override List<string> GetChannelList()
    {
        return new List<string>() { GameServerConfig.CHANNEL_CITY_SERVER, GameServerConfig.CHANNEL_CITY };
    }
    
    protected override void DoOnNewMonth()
    {
        base.DoOnNewMonth();
        //存档
        city_map.Snapshoot();
        ArchiveManager.Save(city_map.city_state);
    }
    
    public override void BindCtiyMap(CityMap city_map)
    {
        base.BindCtiyMap(city_map);
        city_map.SetGameLogicServerBinder(this);
    }


    public bool IsLlmOnline()
    {
        return is_llm_online;
    }

    public bool OnSimAgentIdleEnd(SimAgent agent)
    {
        SimAgentBrain brain = agent.brain;
        if (brain.agent_brain_helper != null)
        {
            if (brain.plan != null && brain.plan.Count > 0)
            {
                AgentPlanItem plan_item = brain.plan[brain.cur_index_in_plan % brain.plan.Count];
                brain.cur_index_in_plan++;

                MapObj obj = GetMapObj(plan_item.target_place_guid);
                Point start_pos = agent.CurPoint;
                Point target_pos = MyRandom.Choose(obj.interactive_pos_list);
                List<Point> path = city_map.nav_manager.FindPath(start_pos, target_pos);
                if (path != null && path.Count > 0)
                {
                    agent.brain.plan_to_execute_on_move_end = plan_item;

                    float speed_rate = 1;
                    string emoji_on_the_way = null;
                    float emoji_interval = 0;
                    float emoji_show_duration = 0;
                    string song = null;

                    if (plan_item.mood > 0 && Math.Abs(agent.GetAttr(AttrInfo.MOOD) - plan_item.mood) / plan_item.mood < GameServerConfig.mood_threshold)
                    {
                        speed_rate = plan_item.speed_rate;
                        emoji_on_the_way = plan_item.emoji_on_the_way;
                        emoji_interval = plan_item.emoji_interval;
                        emoji_show_duration = plan_item.emoji_show_duration;
                        song = plan_item.song;
                    }

                    //通知前端行动
                    SendMsgToCity(
                        CityMsgWalkToDo.GetMsg(agent.Guid, plan_item.target_place_guid, path, speed_rate, emoji_on_the_way, emoji_interval, emoji_show_duration, song), "*");

                    //游戏世界内部同步行动
                    agent.WalkToDo(plan_item.target_place_guid, path, speed_rate, emoji_on_the_way, emoji_interval, emoji_show_duration, song);
                    return true;
                }
                else
                {
                    Log.e("错误：LLM_MSG_WALK_TO_DO 寻路失败 agent_guid = " + agent.Guid + " start_pos=" + start_pos + " target_pos=" + target_pos);
                }
            }
        }
        return false;
    }
    public bool OnSimAgentMoveStopped(SimAgent agent, Point pos, int cur_place_guid)
    {
        if (IsConnected())
        {
            //移动到达某个位置后，请求AI决策，下一步的行动
            agent.ChangeStateToRequestDecision();
            return true;

        }
        return false;
    }

    public bool ExecutePlan(SimAgent agent, AgentPlanItem plan)
    {
        if (plan.action_id != 0 && MyRandom.NextBool(plan.action_rate))
        {
            switch (plan.action_id)
            {
                //TODO 需讨论，是否执行实质行为
                default:
                    Log.e("错误：未知的行为类型 plan.action_id=" + plan.action_id);
                    break;
            }
        }
        return false;
    }

    public bool ExecuteDecision(SimAgent agent,LLmResponseAgentDecision decision)
    {
        if (decision.place_interaction != null)
        {
            DecisionPlaceInteraction place_interaction = decision.place_interaction;
            DecisionMonologue monologue = place_interaction.monologue;
            string content = null;
            string song = null;
            int content_type = 0;
            int display_duration = 0;
            if (monologue != null)
            {
                content = monologue.content;
                song = monologue.song;
                content_type = monologue.content_type;
                display_duration = monologue.display_duration;
            }
            //服务端同步切换智能体的状态机，保证时长一致
            agent.DoAction(content, place_interaction.attr_chagne, content_type, display_duration, song);
            //通知前端行动
            SendMsgToCity(CityMsgAction.GetMsg(agent.Guid, content, place_interaction.attr_chagne, content_type, display_duration, song), "*");
            return true;
        }
        else if (decision.dialog != null)
        {
            //后端直接将对话的双方摆在一起
            //发起者，站右边
            agent.Position = agent.CurPoint.ToMyVector2() + new MyVector2(0.4f, 0);
            //对话者，站左边
            SimAgent target_agent = city_map.GetSimAgent(decision.dialog.agent_guid_object);
            target_agent.Position = agent.CurPoint.ToMyVector2() + new MyVector2(-0.4f, 0);

            agent.StartDialog(decision.dialog);
            //通知前端行动
            SendMsgToCity(CityMsgDialog.GetMsg(agent.Guid, decision.dialog), "*");
            return true;

        }
        else if (decision.monologue != null)
        {
            //服务端同步切换智能体的状态机，保证时长一致
            agent.DoSpeak(decision.monologue.content, decision.monologue.song, null, decision.monologue.content_type, decision.monologue.display_duration);
            //通知前端行动
            SendMsgToCity(CityMsgSpeak.GetMsg(agent.Guid, decision.monologue.content, decision.monologue.song, decision.monologue.content_type, decision.monologue.display_duration), "*");
            return true;
        }
        else if (decision.draw != null)
        {
            DecisionDraw draw = decision.draw;
            agent.AddNft(draw.artwork_id);
            agent.OnCreateNft(draw.artwork_id);
            SendMsgToCity(CityMsgNft.GetMsg(draw.agent_guid, draw.artwork_id, draw.url), "*");
            return true;
        }
        else if (decision.trade != null)
        {
            DecisionTrade trade = decision.trade;
            SimAgent from_agent = GetSimAgent(trade.from_agent_guid);
            SimAgent to_agent = GetSimAgent(trade.to_agent_guid);
            if (from_agent != null && to_agent != null)
            {
                if (from_agent.HasArtwork(trade.artwork_id) && to_agent.GetGoldCount() >= trade.price)
                {
                    from_agent.AddGold(trade.price);
                    to_agent.AddGold(-trade.price);
                    from_agent.RemoveArtwork(trade.artwork_id);
                    to_agent.AddArtwork(trade.artwork_id);

                    SendMsgToCity(CityMsgAgentAttrChange.GetMsg(from_agent.Guid, AttrInfo.GOLD, from_agent.GetGoldCount()), "*");
                    SendMsgToCity(CityMsgAgentAttrChange.GetMsg(to_agent.Guid, AttrInfo.GOLD, to_agent.GetGoldCount()), "*");
                    SendMsgToCity(CityMsgAgentLoseArtwork.GetMsg(from_agent.Guid, trade.artwork_id), "*");
                    SendMsgToCity(CityMsgAgentGetArtwork.GetMsg(to_agent.Guid, trade.artwork_id), "*");
                }
                else
                { 
                    Log.e("交易失败：交易双方钱物没准备好" + ToJson(trade));
                }
            }
            else
            {
                Log.e("交易失败：交易中至少有一方并不存在" + ToJson(trade));
            }
            agent.ChangeStateToIdle();
            return true;
        }
        else if (decision.move != null)
        {
            DecisionMove t = decision.move;

            MapObj obj = GetMapObj(t.target_place_guid);
            if (obj != null)
            {
                Point start_pos = agent.CurPoint;
                Point target_pos = MyRandom.Choose(obj.interactive_pos_list);
                List<Point> path = city_map.nav_manager.FindPath(start_pos, target_pos);
                SendMsgToCity(CityMsgWalkToDo.GetMsg(agent.Guid, t.target_place_guid, path, t.speed_rate, t.emoji_on_the_way, t.emoji_interval, t.emoji_show_duration, t.song), "*");
                //游戏世界内部同步行动
                agent.WalkToDo(t.target_place_guid, path, t.speed_rate, t.emoji_on_the_way, t.emoji_interval, t.emoji_show_duration, t.song);
                return true;
            }
        }
        else if(decision.artwork_recycle != null)
        {
            DecisionArtworkRecycle recylcle= decision.artwork_recycle;
            //将艺术品回收到国有
            SimAgent from_agent = GetSimAgent(recylcle.from_agent_guid);
            if (from_agent == null)
            {
                Log.e("错误：agent不存在 agent_guid=" + recylcle.from_agent_guid);
                return false;
            }
            from_agent.AddGold(recylcle.price);
            //city_map.city_state.GetTreasury().AddGold(-recylcle.price);
            from_agent.RemoveArtwork(recylcle.artwork_id);
            //city_map.city_state.GetTreasury().AddArtwork(recylcle.artwork_id);

            //记账
            SendMsgToCity(CityMsgAgentAttrChange.GetMsg(from_agent.Guid, AttrInfo.GOLD, from_agent.GetGoldCount()), "*");
            SendMsgToCity(CityMsgAgentLoseArtwork.GetMsg(from_agent.Guid, recylcle.artwork_id), "*");

        }
        return false;
    }

    private string ToJson(DecisionTrade trade)
    {
        return JsonConvert.SerializeObject(trade);
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
            if (GameServerConfig.USER_NAME_LLM.Equals(from))
            {
                switch (msg_id)
                {
                    case ServerMsgId.LLM_RESPONSE_AGENT_PLAN:
                        {
                            LLmResponseAgentPlan obj_msg = JsonConvert.DeserializeObject<LLmResponseAgentPlan>(msg);
                            if (obj_msg == null)
                            {
                                SendErrorMsgToServer(msg_id, "错误：消息序列化失败=" + msg);
                                return;
                            }
                            SimAgent agent = GetSimAgent(obj_msg.agent_guid);
                            if (agent != null)
                            {
                                agent.brain.SetPlan(obj_msg.plan);
                                SendAckToServer(obj_msg.msg_guid);
                            }
                            else
                            {
                                SendErrorMsgToServer(obj_msg.msg_guid, "错误：agent不存在 agent_guid=" + obj_msg.agent_guid);
                            }
                            break;
                        }
                    case ServerMsgId.LLM_RESPONSE_AGENT_DECISION:
                        {
                            LLmResponseAgentDecision obj_msg = JsonConvert.DeserializeObject<LLmResponseAgentDecision>(msg);
                            if (obj_msg == null)
                            {
                                SendErrorMsgToServer(msg_id, "错误：消息序列化失败=" + msg);
                                return;
                            }
                            SimAgent agent = GetSimAgent(obj_msg.agent_guid);
                            if (agent != null)
                            {
                                agent.brain.SetDecision(obj_msg);
                                SendAckToServer(obj_msg.msg_guid);
                            }
                            break;
                        }
                    case ServerMsgId.LLM_USER_BUY_ARTWORK_RESULT:
                        {
                            LLmMsgUserBuyArtworkResult obj_msg =
                                JsonConvert.DeserializeObject<LLmMsgUserBuyArtworkResult>(msg);
                            if (obj_msg == null)
                            {
                                SendErrorMsgToServer(msg_id, "错误：消息序列化失败=" + msg);
                                return;
                            }
                            bool is_succ = obj_msg.is_succ;
                            string error_msg = obj_msg.error_msg;
                            if (obj_msg.is_succ)
                            {
                                SimAgent agent = GetSimAgent(obj_msg.from_agent_id);
                                if (agent != null)
                                {
                                    if (agent.RemoveArtwork(obj_msg.artwork_id))
                                    {
                                        agent.AddAttr(AttrInfo.GOLD, obj_msg.price);

                                        SendMsgToCity(CityMsgAgentAttrChange.GetMsg(agent.Guid, AttrInfo.GOLD, agent.GetAttr(AttrInfo.GOLD)), "*");
                                        SendMsgToCity(CityMsgUserGetArtwork.GetMsg(obj_msg.to_user_name, obj_msg.artwork_id), obj_msg.to_user_name);
                                        SendMsgToCity(CityMsgAgentLoseArtwork.GetMsg(agent.Guid, obj_msg.artwork_id), "*");

                                        SendAckToServer(obj_msg.msg_guid);
                                        is_succ = true;
                                    }
                                    else
                                    {
                                        error_msg = "The agent does not own the artwork";
                                    }
                                }
                                else
                                {
                                    error_msg = "The agent does not exist";
                                    SendErrorMsgToServer(obj_msg.msg_guid, "错误：agent不存在 agent_guid=" + obj_msg.from_agent_id);
                                }
                            }
                            else
                            {
                                // error_msg = "Your purchase request was rejected by the agent";

                            }
                            SendMsgToCity(CityMsgUserBuyArtworkResult.GetMsg(is_succ, error_msg), obj_msg.to_user_name);
                            break;
                        }
                    case ServerMsgId.LLM_USER_SELL_ARTWORK_RESULT:
                        {
                            LLmMsgUserSellArtworkResult obj_msg =
                                JsonConvert.DeserializeObject<LLmMsgUserSellArtworkResult>(msg);
                            if (obj_msg == null)
                            {
                                SendErrorMsgToServer(msg_id, "错误：消息序列化失败=" + msg);
                                return;
                            }
                            bool is_succ = obj_msg.is_succ;
                            string error_msg = obj_msg.error_msg;
                            if (obj_msg.is_succ)
                            {
                                SimAgent agent = GetSimAgent(obj_msg.to_agent_id);
                                if (agent != null)
                                {
                                    agent.AddAttr(AttrInfo.GOLD, -obj_msg.price);
                                    agent.AddArtwork(obj_msg.artwork_id);

                                    SendMsgToCity(CityMsgAgentAttrChange.GetMsg(agent.Guid, AttrInfo.GOLD, agent.GetAttr(AttrInfo.GOLD)), "*");
                                    SendMsgToCity(CityMsgUserLossArtwork.GetMsg(obj_msg.from_user_name, obj_msg.artwork_id), obj_msg.from_user_name);
                                    SendMsgToCity(CityMsgAgentGetArtwork.GetMsg(agent.Guid, obj_msg.artwork_id), "*");

                                    SendAckToServer(obj_msg.msg_guid);
                                    is_succ = true;
                                }
                                else
                                {
                                    error_msg = "The agent does not exist";
                                    SendErrorMsgToServer(obj_msg.msg_guid, "错误：agent不存在 agent_guid=" + obj_msg.to_agent_id);
                                }
                            }
                            else
                            {
                                //error_msg = "Your purchase request was rejected by the agent";
                            }
                            SendMsgToCity(CityMsgUserSellArtworkResult.GetMsg(is_succ, error_msg), obj_msg.from_user_name);
                            break;
                        }
                    default:
                        {
                            SendErrorMsgToServer(msg_id,"未知的消息类型 msg_id=" + msg_id);
                            LogError(channel, from, msg_id, msg);
                            break;
                        }
                        
                }
            }
            else
            {
                LogError(channel, from, msg_id, msg);
            }
        }
        else if (GameServerConfig.CHANNEL_CITY.Equals(channel))
        {
            switch (msg_id)
            {
                case CityMsgId.USER_MSG_RECHARGE:
                { 
                    UserMsgRecharge m = JsonConvert.DeserializeObject<UserMsgRecharge>(msg);
                    SimAgent agent = GetSimAgent(m.to_agent_id);
                    bool is_succ = false;
                    string error_msg = null;
                    if (agent != null)
                    {
                        agent.AddAttr(AttrInfo.GOLD, m.recharge_count);
                        SendMsgToCity(CityMsgAgentAttrChange.GetMsg(agent.Guid, AttrInfo.GOLD, agent.GetAttr(AttrInfo.GOLD)), "*");
                        is_succ = true;
                    }
                    else
                    {
                        error_msg = "The agent does not exist";
                    }
                    SendMsgToCity(CityMsgRechargeResult.GetMsg(is_succ, error_msg), m.from_user_name);
                    break;
                }
                case CityMsgId.USER_MSG_WITHDRAW:
                {
                    UserMsgWithdraw m = JsonConvert.DeserializeObject<UserMsgWithdraw>(msg);
                    SimAgent agent = GetSimAgent(m.from_agent_id);
                    bool is_succ = false;
                    string error_msg = null;
                    if (agent != null)
                    {
                        int cur_gold = agent.GetAttr(AttrInfo.GOLD);
                        if (cur_gold < m.withdraw_count)
                        {
                            error_msg = "The agent's balance is insufficient";
                        }
                        else
                        {
                            agent.AddAttr(AttrInfo.GOLD, -m.withdraw_count);
                            SendMsgToCity(CityMsgAgentAttrChange.GetMsg(agent.Guid, AttrInfo.GOLD, agent.GetAttr(AttrInfo.GOLD)), "*");
                            is_succ = true;
                        }
                    }
                    else
                    {
                        error_msg = "The agent does not exist";
                    }
                    SendMsgToCity(CityMsgWithdrawResult.GetMsg(is_succ, error_msg), m.to_user_name);
                    break;
                }
                case CityMsgId.USER_BUY_ARTWORK:
                {
                    UserMsgBuyArtwork m = JsonConvert.DeserializeObject<UserMsgBuyArtwork>(msg);
                    SimAgent agent = GetSimAgent(m.from_agent_id);
                    bool is_succ = false;
                    string error_msg = null;
                   
                    if (agent != null)
                    {
                        SendMsgToServer(ServerMsgUserBuyArtwork.GetMsg(m.price, m.artwork_id, m.from_agent_id, m.to_user_name));
                        //打断流程，由llm进一步处理
                        return;
                    }
                    else
                    {
                        error_msg = "The agent does not exist";
                    }
                    SendMsgToCity(CityMsgUserBuyArtworkResult.GetMsg(is_succ, error_msg), m.to_user_name);
                    break;
                }
                case CityMsgId.USER_SELL_ARTWORK:
                {
                    UserMsgSellArtwork m = JsonConvert.DeserializeObject<UserMsgSellArtwork>(msg);
                    SimAgent agent = GetSimAgent(m.to_agent_id);
                    bool is_succ = false;
                    string error_msg = null;
                   
                    if (agent != null)
                    {
                        SendMsgToServer(ServerMsgUserSellArtwork.GetMsg(m.price, m.artwork_id, m.to_agent_id, m.from_user_name));
                        //打断流程，由llm进一步处理
                        return;
                    }
                    else
                    {
                        error_msg = "The agent does not exist";
                    }
                    SendMsgToCity(CityMsgUserSellArtworkResult.GetMsg(is_succ, error_msg), m.from_user_name);
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

    private void SendErrorMsgToServer(int msg_guid, string error_msg)
    {
        Log.e(error_msg);
        SendMsgToServer(ServerMsgAck.GetMsg(msg_guid, 1, error_msg));
    }
    private void SendAckToServer(int msg_guid)
    {
        SendMsgToServer(ServerMsgAck.GetMsg(msg_guid));
    }

    public void SendMsgToCity(string content, string target)
    {
        websocket_manager[GameServerConfig.CHANNEL_CITY].SendMsg(content, target);
    }
    
    public void SendMsgToServer(string content)
    {
        if (content == null || content.Length < 300)
        {
            Log.d("[MSG TO LLM]+++++" + content);
        }
        websocket_manager[GameServerConfig.CHANNEL_CITY_SERVER].SendMsg(content, GameServerConfig.USER_NAME_LLM);
    }

    public SimAgent GetSimAgent(int agent_guid)
    {
        return city_map.GetSimAgent(agent_guid);
    }

    public MapObj GetMapObj(int target_place_guid)
    {
        return city_map.city_state.GetMapObjByGuid(target_place_guid);
    }

    private bool is_llm_online = false;
    public override void OnUserEnter(string channel, string user)
    {
        Log.e("用户进入" + user);
        if (GameServerConfig.USER_NAME_GAME_SERVER.Equals(user))
        {
            //服务器上限后，发广播，让所有客户端重置
            city_map.Snapshoot();
            SendMsgToCity(CityMsgInit.GetMsg(city_map.city_state), "*");
            SendMsgToServer(ServerMsgInit.GetMsg(city_map.city_state));
        }
        else
        {
            if (GameServerConfig.CHANNEL_CITY.Equals(channel))
            {
                //某一个客户端上线，让该客户端初始状态
                city_map.Snapshoot();
                SendMsgToCity(CityMsgInit.GetMsg(city_map.city_state), user);
            }
            else if(GameServerConfig.CHANNEL_CITY_SERVER.Equals(channel))
            {
                //LLM上线后，同步状态
                city_map.Snapshoot();
                SendMsgToServer(ServerMsgInit.GetMsg(city_map.city_state));

                //标记LLM上线了
                if (GameServerConfig.USER_NAME_LLM.Equals(user))
                {
                    is_llm_online = true;
                }
            }
        }
    }

}