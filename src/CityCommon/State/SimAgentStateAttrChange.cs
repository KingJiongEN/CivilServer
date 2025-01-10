using System.Collections.Generic;
using Plugins.CityCommon.Data;
using Plugins.CityCommon.Msg;

public class SimAgentStateAttrChange: ISimAgentState
{
    private List<AttrChange> attr_change;
    private int attr_id;
    private int value_change;
    private int attr_value;

    public override void Reuse()
    {
        base.Reuse();
        attr_change = null;
    }

    public SimAgentStateAttrChange Init(List<AttrChange> attr_change, int attr_id, int attr_value, int value_change)
    {
        this.attr_id = attr_id;
        this.value_change = value_change;
        this.attr_change = attr_change;
        this.attr_value = attr_value;
        return this;
    }
    
    protected override void OnEnter()
    {
        base.OnEnter();
        AttrInfo t = ConfigManager.Ins.attr_info_dic[attr_id];
        _duration = t.anim_duration;
        npc.ShowAttrChange(attr_id, t.attr_icon, t.anim_duration, attr_value, value_change);
    }

    protected override void OnTimeout()
    {
        base.OnTimeout();

        if (attr_change != null && attr_change.Count > 0)
        {
            AttrChange t = attr_change[0];
            attr_change.RemoveAt(0);
            npc.ChangeState(city_map.npc_state_pool.Get<SimAgentStateAttrChange>().Init(attr_change, t.attr, t.value, t.value_change));
        }
        else
        {
            npc.ChangeStateToIdle();
        }
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
        if (attr_id != 0)
        {
            AttrInfo t = ConfigManager.Ins.attr_info_dic[attr_id];
            if (value_change > 0)
            {
                return t.attr_name + " +" + value_change;
            }

            else
            {
                return t.attr_name + " " + value_change;
            }

        }
        return null;
    }
}