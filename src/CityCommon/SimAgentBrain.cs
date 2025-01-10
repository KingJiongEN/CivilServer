using Plugins.CityCommon.Msg;
using System;
using System.Collections.Generic;

public class SimAgentBrain
{
    private SimAgent sim_agent;
    public CityState city_state;

    public int agent_guid;
    public List<AgentPlanItem> plan;
    public int cur_index_in_plan;
    public LLmResponseAgentDecision decision;

    public AgentPlanItem plan_to_execute_on_move_end;

    public bool is_waiting_plan;
    public bool is_waiting_decision;
    public SimAgentBrainHelper agent_brain_helper;

    public SimAgentBrain(SimAgent sim_agent, SimAgentBrainHelper agent_brain_helper)
    {
        this.sim_agent = sim_agent;
        this.agent_brain_helper = agent_brain_helper;
    }
    
    public ISimAgentState WhatToDoNext()
    {
        MapObj obj = sim_agent.city_map.city_state.GetRandomMapObj();

        sim_agent.target_place_guid = obj.guid;
        sim_agent.target_pos = MyRandom.Choose(obj.interactive_pos_list);
        
        return sim_agent.city_map.npc_state_pool.Get<SimAgentStateWalk>().Init(obj.guid, sim_agent.target_pos);
    }

    internal void OnNewDay()
    {
        if (agent_brain_helper != null)
        {
            if (!is_waiting_plan)
            {
                if (plan == null || plan.Count == 0 || cur_index_in_plan >= plan.Count)
                {
                    //请求行程计划
                    RequestPlan();
                }
            }
        }
    }

    /// <summary>
    /// 流程：
    /// 1. 向LLM发起请求行程决策
    /// 2. 接收到决策，将决策写入到智能体的brain，临时存储
    /// 3. 逻辑兜底时，执行行程活动，并消耗掉计划
    /// 4. 行程计划消耗完，下一轮需请求新的行程
    /// </summary>
    private void RequestPlan()
    {
        if(agent_brain_helper!=null)
        {
            is_waiting_plan = agent_brain_helper.RequestPlan(sim_agent);
        }
    }

    public void SetPlan(List<AgentPlanItem> plan)
    {
        is_waiting_plan = false;
        this.plan = plan;
    }

    /// <summary>
    /// 流程：
    /// 1. 到达某个建筑后，发起眼下行为决策
    /// 2. 执行决策行为
    /// 3. 发起进一步行为决策，进一步执行行为
    /// 4. 如果决策失败或决策要离开建筑，则开始向下一个建筑移动
    /// </summary>
    public void RequestDecision()
    {
        if (agent_brain_helper != null)
        {
            is_waiting_decision = agent_brain_helper.RequestDecision(sim_agent);
        }
    }


    public void SetDecision(LLmResponseAgentDecision decision)
    {
        is_waiting_decision = false;
        this.decision = decision;

        if(decision!= null)
        {
            if(decision.place_interaction != null)
            {
                sim_agent.SetAttr(decision.place_interaction.attr_chagne);
            }
        }
    }

}