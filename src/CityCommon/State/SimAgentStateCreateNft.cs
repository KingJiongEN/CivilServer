public class SimAgentStateCreateNft : ISimAgentState
{
    private string artwork_id;
    public SimAgentStateCreateNft Init(string artwork_id)
    {
        this.artwork_id = artwork_id;
        return this;
    }
    
    protected override void OnEnter()
    {
        base.OnEnter();
        _duration = MyRandom.Between(10, 15);
        npc.ShowCreateNft(artwork_id);
    }

    protected override void OnExit()
    {
        base.OnExit();
        npc.HideCreateNft();
    }

    protected override void OnTimeout()
    {
        base.OnTimeout();
        npc.ChangeStateToIdle();
    }
    
    public override string GetDesc()
    {
        return "Creating artwork";
    }
    
}
