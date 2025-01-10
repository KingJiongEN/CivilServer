public abstract class ISimAgentState : IReusableObj
{
    protected SimAgent npc;
    protected float _duration = -1;
    protected float _start_time;
    //标识位，保证OnTimeout只被调用一次
    protected bool _is_timeout = false;
    public CityMap city_map;
    public void Enter(SimAgent npc)
    {
        _start_time = Clock.time;
        city_map = npc.city_map;
        this.npc = npc;
        OnEnter();
    }
    public  ISimAgentState Init() {return this; }
    public void Tick()
    {
        OnTick();
    }
    public void Exit()
    {
        BeforeOnExit();
        OnExit();
        city_map.npc_state_pool.Put(this);
    }

    public override void Reuse()
    {
        npc = null;
        _duration = -1;
        _start_time = 0;
        is_on_time_half = false;
        _is_timeout = false;
        city_map = null;
    }

    protected virtual void OnEnter(){ }
    protected virtual void OnTimeout(){}
    
    /// <summary>
    /// 该方法只做基础状态维护和清理，其中不能包含业务逻辑
    /// </summary>
    protected virtual void OnExit()
    {

    }
    protected virtual void OnTick()
    {
        if (_duration > 0 && Clock.time - _start_time > _duration * 0.5f && !is_on_time_half)
        {
            is_on_time_half = true;
            OnTimeHalf();
        }

        if (_duration > 0 && Clock.time - _start_time > _duration && !_is_timeout)
        {
            _is_timeout = true;
            OnTimeout();
        }
    }

    protected virtual void DebugCheckTask()
    {

    }
    protected virtual void BeforeOnTimeout() {

    }
    protected virtual void BeforeOnExit() {

        DebugCheckTask();


    }
    protected bool is_on_time_half = false;
    protected virtual void OnTimeHalf()
    {

    }
    public virtual void OnHit()
    {

    }

    public abstract string GetDesc();
}