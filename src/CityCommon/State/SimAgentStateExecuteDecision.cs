using Plugins.CityCommon.Msg;
using System;

public class SimAgentStateExecuteDecision : ISimAgentState
{

    public override string GetDesc()
    {
        return "Thinking";
    }

    protected override void OnTick()
    {
        base.OnTick();
        //执行决策
        if (npc.brain.decision != null)
        {
            if (npc.brain.decision.error_code == (int)LLmResponseErrorCode.SUCC)
            {
                SimAgent cur_npc = npc;
                if (city_map.ExecuteDecision(npc, npc.brain.decision))
                {
                    cur_npc.brain.SetDecision(null);
                }
                else
                {
                    npc.ChangeStateToIdle();
                }
            }
            else if (npc.brain.plan_to_execute_on_move_end != null)
            {
                if (IsAtPlanPos(npc.brain.plan_to_execute_on_move_end, npc.CurPoint))
                {
                    if (city_map.ExecutePlan(npc, npc.brain.plan_to_execute_on_move_end))
                    {
                        npc.brain.plan_to_execute_on_move_end = null;
                    }
                    else
                    {
                        npc.ChangeStateToIdle();
                    }
                }
                else
                {
                    npc.ChangeStateToIdle();
                }
            }
        }
    }

    internal bool IsAtPlanPos(AgentPlanItem plan, Point p)
    {
        MapObj map_obj = city_map.GetMapObjByGuid(plan.target_place_guid);
        if(map_obj != null && map_obj.interactive_pos_list.Contains(p)) 
        {
            return true;
        }
        return false;
    }

}
