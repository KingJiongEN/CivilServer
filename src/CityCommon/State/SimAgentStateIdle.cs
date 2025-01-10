using Plugins.CityCommon.Msg;

public class SimAgentStateIdle : ISimAgentState
{
    protected override void OnEnter()
    {
        base.OnEnter();
        _duration = MyRandom.Between(10, 20);
    }

    protected override void OnTimeout()
    {
        int agent_guid = npc.Guid;
        //尝试决策，执行行程计划
        if (npc.TryChangeStateOnIdleEnd())
        {
            Log.d("智能体按行程计划行事：agent_guid=" + agent_guid);
        }
        else
        {
            //再等等
            npc.ChangeStateToIdle();
        }
        base.OnTimeout();
    }

    public override string GetDesc()
    {
        return "Resting";
    }
}