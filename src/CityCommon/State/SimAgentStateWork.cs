public class SimAgentStateWork : ISimAgentState
{
    protected override void OnEnter()
    {
        base.OnEnter();
        _duration = MyRandom.Between(3, 10);
        npc.ShowStateText();
    }

    protected override void OnTimeout()
    {
        base.OnTimeout();
        npc.ChangeStateToIdle();
    }

    protected override void OnExit()
    {
        npc.target_pos = Point.zero;
        npc.target_place_guid = 0;
        npc.HideWordsText();
        base.OnExit();
    }

    public override string GetDesc()
    {
        return "Working";
    }
}