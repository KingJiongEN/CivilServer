using System;
using System.Collections.Generic;
using Plugins.CityCommon.Data;
using Plugins.CityCommon.Msg;
using Plugins.CityCommon.Server;

public interface ISimAgentCom
{
    void HideWordsText();
    void ShowWordsText(string msg, int content_type, bool show_at_left);
    void ShowAttrChange(int attr_id, string attr_icon, float anim_duration, int attr_value, int value_change);
    void ShowCreateNft(string artwork_id);
    void HideCreateNft();
    void StartEmojiOnWalk(string emoji_on_the_way, float emoji_interval, float emoji_show_duration);
    void EndEmojiOnWalk();
    void StartSong(string song);
    void EndSong();
}

public class SimAgent : IMovable
{
    public static float MIN_DIST_STEP = 1 / 32f;
    
    public IntelligentAgent il_agent;
    public int target_place_guid;
    public Point target_pos;
    public SimMoveAgent move_agent = new SimMoveAgent();
    public CityMap city_map;
    
    private Point _pos_point;
    private int _pos_point_cal_frame_id = -1;
    private ISimAgentCom com;
    private string midi;
    public Point CurPoint
    {
        get
        {
            if (_pos_point_cal_frame_id != Clock.frameCount)
            {
                _pos_point_cal_frame_id = Clock.frameCount;
                _pos_point = Point.PositionToPoint(Position);
            }
            return _pos_point;
        }
    }

    public int Guid
    {
        get => il_agent.id;
    }
    public MyVector2 Position { get=>il_agent.postion; set=>il_agent.postion=value; }
    public bool IsDead { get; }
    public void OnPause()
    {
        
    }

    public void OnResume()
    {
        
    }

    public void OnMove(MyVector2 delta)
    {
        
    }

    public void OnMoveStart(MyVector2 delta)
    {
        
    }

    public void OnIdle()
    {
        
    }

    public void BindCom(ISimAgentCom com)
    {
        this.com = com;
    }

    public void SetInfo(CityMap city_map, IntelligentAgent il_agent, SimAgentBrainHelper agent_brain_helper)
    {
        this.city_map = city_map;
        move_agent.Init(this, il_agent.speed);
        this.il_agent = il_agent;
        ChangeBrain(new SimAgentBrain(this, agent_brain_helper));
    }

    private Func<SimAgent, Point, int, bool> cb_OnMoveStopped;
    private Func<SimAgent, bool> cb_OnSimAgentIdleEnd;
    public void SetOnMoveStoppedCb(Func<SimAgent, Point, int, bool> cb_OnMoveStopped)
    {
        this.cb_OnMoveStopped = cb_OnMoveStopped;
    }
    public void SetOnIdleEndCb(Func<SimAgent, bool> cb_OnSimAgentIdleEnd)
    {
        this.cb_OnSimAgentIdleEnd = cb_OnSimAgentIdleEnd;
    }
    
    public SimAgentBrain brain;
    private void ChangeBrain(SimAgentBrain brain)
    {
        this.brain = brain;
    }
    
    public string LOG_PREFIX { get { return string.Format("[{0}][{1}]", Clock.frameCount, il_agent.id); } }
    public string _cur_state_name;
    public ISimAgentState cur_npc_state;

    public virtual void ChangeState(ISimAgentState new_state)
    {
        // _cur_state_name = new_state.GetType().ToString();
        // Log.d(LOG_PREFIX + _cur_state_name);

        if (cur_npc_state != null)
        {
            cur_npc_state.Exit();
            cur_npc_state = null;
        }
        cur_npc_state = new_state;
        cur_npc_state.Enter(this);
    }

    public void OnTick()
    {
        move_agent.Tick();
        cur_npc_state?.Tick();
    }
    
    public void ChangeStateToIdle()
    {
        ChangeState(city_map.npc_state_pool.Get<SimAgentStateIdle>());
    }

    public void ChangeStateToWork()
    {
        ChangeState(city_map.npc_state_pool.Get<SimAgentStateWork>());
    }

    public void ChangeStateToExecuteDecision()
    {
        ChangeState(city_map.npc_state_pool.Get<SimAgentStateExecuteDecision>());
    }
    internal void ChangeStateToRequestDecision()
    {
        brain.SetDecision(null);
        ChangeState(city_map.npc_state_pool.Get<SimAgentStateRequestDecision>());
    }

    public void ShowStateText()
    {
        if (target_place_guid != 0)
        {
            MapObj t = city_map.city_state.GetMapObjByGuid(target_place_guid);
            if (t != null)
            {
                com?.ShowWordsText(t.name, CityMsgSpeak.CHAT, false);
            }
            else
            {
                Log.e("错误：不存在的位置" + target_place_guid);
            }
        }
    }
    
    public void ShowWordsText(string words_content, int content_type, bool show_at_left)
    {
        com?.ShowWordsText(words_content, content_type, show_at_left);
    }

    public void StartSong(string song)
    {
        com?.StartSong(song);
    }

    public ISimAgentState WhatToDoNext()
    {
        return brain.WhatToDoNext();
    }

    public void HideWordsText()
    {
        com?.HideWordsText();
    }

    public bool IsIdle()
    {
        return cur_npc_state is SimAgentStateIdle;
    }

    public void WalkToDo(int target_place_guid)
    {
        this.target_place_guid = target_place_guid;
        MapObj place = city_map.city_state.GetMapObjByGuid(target_place_guid);
        if (place != null)
        {
            ChangeState(city_map.npc_state_pool.Get<SimAgentStateWalk>()
                .Init(target_place_guid, MyRandom.Choose(place.interactive_pos_list)));
        }
        else
        {
            Log.e("错误：不存在的位置 target_place_guid=" + target_place_guid);
        }
    }

    public void WalkToDo(Point pos)
    {
        ChangeState(city_map.npc_state_pool.Get<SimAgentStateWalk>().Init(0, pos));
    }
    
    public void WalkToDo(int target_place_guid, List<Point> path, float speed_rate, string emoji_on_the_way, float emoji_interval, float emoji_show_duration, string song)
    {
        this.target_place_guid = target_place_guid;
        if (path != null && path.Count > 0)
        {
            Position = path[0].ToMyVector2();
            ChangeState(city_map.npc_state_pool.Get<SimAgentStateWalk>().Init(target_place_guid, path, true, speed_rate, emoji_on_the_way, emoji_interval, emoji_show_duration, song));
        }
        else
        {
            Log.e("错误： path为空");
        }
    }

    public void WalkToDo(List<Point> path, float speed_rate, string emoji_on_the_way, float emoji_interval, float emoji_show_duration, string song)
    {
        if (path != null && path.Count > 0)
        {
            Position = path[0].ToMyVector2();
            ChangeState(city_map.npc_state_pool.Get<SimAgentStateWalk>().Init(target_place_guid, path, true, speed_rate, emoji_on_the_way, emoji_interval, emoji_show_duration, song));
        }
        else
        {
            Log.e("错误： path为空");
        }
    }

    public void DoAction(string words_content, List<AttrChange> attr_change, int content_type, int display_duration, string song)
    {
        DoSpeak(words_content, song, attr_change, content_type, display_duration);
    }

    internal void StartDialog(DecisionDialog dialog)
    {
        ChangeState(city_map.npc_state_pool.Get<SimAgentStateDialog>().Init(dialog, 0));
    }
    public void DoSpeak(string words_content, string song, List<AttrChange> attr_change, int content_type, int display_duration)
    {
        List<string> words_list = new List<string>();
        float speak_duration = 0;
        if (!string.IsNullOrEmpty(words_content))
        {
            string[] arr = words_content.Split("\n");
            foreach (var line in arr)
            {
                string[] arr2 = line.Split(".");
                if (arr2.Length > 0)
                {
                    words_list.AddRange(arr2);
                }
            }

            speak_duration = GameServerConfig.SPEAK_DURATION_SECONDS_MAX / words_list.Count;
            if (speak_duration > GameServerConfig.SPEAK_INTERVAL_MAX)
            {
                speak_duration = GameServerConfig.SPEAK_INTERVAL_MAX;
            }

            if (speak_duration < display_duration)
            {
                speak_duration = display_duration;
            }
        }

        ChangeState(city_map.npc_state_pool.Get<SimAgentStateSpeak>().Init(words_list, 0, speak_duration, content_type, attr_change, song));
    }

    public void Snapshoot()
    {
        
    }

    public bool OnMoveStopped()
    {
        if (cb_OnMoveStopped != null)
        {
            return cb_OnMoveStopped.Invoke(this, CurPoint, target_place_guid);
        }
        return false;
    }

    public void AddNft(string artwork_id)
    {
        il_agent.AddArtwork(artwork_id);
    }

    public int GetAttr(int attr_id)
    {
        if (il_agent.attr.TryGetValue(attr_id, out int r))
        {
            return r;
        }
        return 0;
    }

    public void SetAttr(List<AttrChange> attr_chagne)
    {
        if (attr_chagne != null)
        {
            foreach (var t in attr_chagne)
            {
                il_agent.attr[t.attr] = t.value;
            }
        }
    }
    
    public void SetAttr(int attr_id, int attr_value)
    {
        il_agent.attr[attr_id] = attr_value;
    }
    
    public void AddAttr(int attr_id, int delta)
    {
        if (il_agent.attr.TryGetValue(attr_id, out int n))
        {
            il_agent.attr[attr_id] = n + delta;
        }
        else
        {
            il_agent.attr[attr_id] = delta;
        }
    }

    public void ShowAttrChange(int attr_id, string attr_icon, float anim_duration, int attr_value, int value_change)
    {
        SetAttr(attr_id, attr_value);
        com?.ShowAttrChange(attr_id, attr_icon, anim_duration, attr_value, value_change);
    }

    public void OnCreateNft(string artwork_id)
    {
        ChangeState(city_map.npc_state_pool.Get<SimAgentStateCreateNft>().Init(artwork_id));
    }

    public void ShowCreateNft(string artwork_id)
    {
        il_agent.AddArtwork(artwork_id);
        com?.ShowCreateNft(artwork_id);
    }

    public void HideCreateNft()
    {
        com?.HideCreateNft();
    }

    public bool RemoveArtwork(string artwork_id)
    {
        return il_agent.RemoveArtwork(artwork_id);
    }
    public bool HasArtwork(string artwork_id)
    {
        return il_agent.HasArtwork(artwork_id);
    }
    public bool HasAnyArtwork()
    {
        return GetArtworkCount() > 0;
    }

    public string GetRandomArtwork()
    {
        if (il_agent.artwork_list != null)
        {
            return MyRandom.Choose(il_agent.artwork_list);
        }
        return null;
    }
    public bool AddArtwork(string artwork_id)
    {
        return il_agent.AddArtwork(artwork_id);
    }

    public int GetArtworkCount()
    {
        if (il_agent.artwork_list != null)
        {
            return il_agent.artwork_list.Count;
        }
        return 0;
    }

    public int GetGoldCount()
    {
        return GetAttr(AttrInfo.GOLD);
    }

    public void AddGold(int delta)
    {
        AddAttr(AttrInfo.GOLD, delta);
    }

    public void StartActionOnWalk(float speed_rate, string emoji_on_the_way, float emoji_interval, float emoji_show_duration, string song)
    {
        //变速
        if (speed_rate > 0)
        {
            move_agent.speed_percent = speed_rate;
        }

        //移动过程中，循环显示emoji
        if (!string.IsNullOrEmpty(emoji_on_the_way) && emoji_interval > 0 && emoji_show_duration > 0)
        {
            com?.StartEmojiOnWalk(emoji_on_the_way, emoji_interval, emoji_show_duration);
        }

        //唱歌
        com?.StartSong(song);

        //同步到web
        midi = song;
    }

    public void StopActionOnWalk(string emoji_on_the_way, string song)
    {
        move_agent.speed_percent = 1;
        if (!string.IsNullOrEmpty(emoji_on_the_way))
        {
            com?.EndEmojiOnWalk();
        }

        if (!string.IsNullOrEmpty(song))
        {
            com?.EndSong();
        }

        //同步到web
        midi = null;
    }

    private string state_key_for_web = null;
    internal string GetStateKeyForWeb()
    {
        if(state_key_for_web == null)
        {
            state_key_for_web = "agent/state/" + Guid;
        }
        return state_key_for_web;
    }

    private AgentStateForWeb state_for_web = null;
    internal AgentStateForWeb GetStateForWeb()
    {
        if(state_for_web == null)
        {
            state_for_web = new AgentStateForWeb();
        }
        MyVector2 p = Position;
        state_for_web.x = (int)(p.x * GameServerConfig.CELL_SIZE_IN_PIXEL);
        state_for_web.y = (int)(p.y * GameServerConfig.CELL_SIZE_IN_PIXEL);
        state_for_web.midi = midi;
        state_for_web.is_indoor = IsIndoor();
        state_for_web.energy = GetAttr(AttrInfo.ENERGY);
        state_for_web.gold = GetAttr(AttrInfo.GOLD);
        state_for_web.health = GetAttr(AttrInfo.HEALTH);
        state_for_web.mood = GetAttr(AttrInfo.MOOD);

        return state_for_web;
    }

    private bool IsIndoor()
    {
        return city_map.IsInDoor(CurPoint);
    }

    internal string GetCurAreaName()
    {
        city_map.GetCurAreaName(CurPoint, out string area_name);
        return area_name;
    }

    internal int GetCurPlaceGuid()
    {
        MapObj map_obj = city_map.GetMapObjByPoint(CurPoint);
        if (map_obj != null)
        {
            return map_obj.guid;
        }
        return 0;
    }

    internal List<int> GetAgentsAround()
    {
        return city_map.GetAgentsAround(CurPoint, Guid);
    }
    internal List<int> GetAgentsIdleAround()
    {
        return city_map.GetAgentsIdleAround(CurPoint, Guid);
    }

    internal bool TryChangeStateOnIdleEnd()
    {
        if (cb_OnSimAgentIdleEnd != null)
        {
            return cb_OnSimAgentIdleEnd.Invoke(this);
        }
        return false;
    }
}

public class AgentStateForWeb
{
    public string midi;
    public int x;
    public int y;
    public bool is_indoor;
    public int energy;
    public int gold;
    public int health;
    public int mood;
}