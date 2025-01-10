public class SimAgentStateRequestDecision : ISimAgentState
{
    public override string GetDesc()
    {
        return "Thinking";
    }

    protected override void OnEnter()
    {
        base.OnEnter();
        //无限等待
        _duration = -1;
        npc.brain.RequestDecision();
    }

    protected override void OnTick()
    {
        base.OnTick();
        if (npc.brain.is_waiting_decision)
        {
            //等待响应
        }
        else
        {
            if (npc.brain.decision != null)
            {
                npc.ChangeStateToExecuteDecision();
            }
            else
            {
                //因通讯等原因，并没有发起请求
                npc.brain.RequestDecision();
            }
        }

    }

    protected override void OnExit()
    {
        if (npc.brain.is_waiting_decision)
        {
            Log.e("错误：智能体在退出决策状态时，决策并未完成");
        }
        base.OnExit();
    }
}
