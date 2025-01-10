
using Pomelo.Protobuf;
using System;
using System.Collections.Generic;

public class SimAgentStateWalk : ISimAgentState
{
    protected int target_place_guid;
    protected Point target_pos;
    private bool is_start_walk = false;
    private bool is_move_follow_path = false;
    protected List<Point> path;

    private Point real_end_pos;
    private bool is_move_through = false;
    private bool is_end_offest = true;
    private bool auto_adjust_end_pos = true;

    private float speed_rate;
    private string emoji_on_the_way;
    private float emoji_interval;
    private float emoji_show_duration;
    private string song;

    private bool has_no_way_callback_next_frame = false;
    
    public override void Reuse()
    {
        base.Reuse();
        target_pos = Point.EmptyPoint;
        is_start_walk = false;
        is_move_follow_path = false;

        real_end_pos = Point.EmptyPoint;
        is_move_through = false;
        is_end_offest = true;
        auto_adjust_end_pos = true;
        has_no_way_callback_next_frame = false;
        speed_rate = 1;
        emoji_on_the_way = null;
        emoji_interval = 0;
        emoji_show_duration = 0;
        song = null;
        target_place_guid = 0;
        is_arrived_target_pos = false;
    }
    
    #region 按GRID网格寻路
    /// <summary>
    /// 寻路，并移动
    /// </summary>
    public virtual SimAgentStateWalk Init(int target_place_guid, Point target_pos,  bool auto_adjust_end_pos = false, bool is_end_offest = true)
    {
        this.target_pos = target_pos;
        this.target_place_guid = target_place_guid;
        _duration = -1;
        this.auto_adjust_end_pos = auto_adjust_end_pos;
        this.is_end_offest = is_end_offest;
        return this;
    }
    


    /// <summary>
    /// 使用现成的路径，直接移动
    /// </summary>
    /// <param name="target_pos"></param>
    /// <param name="path"></param>
    public virtual SimAgentStateWalk Init(int target_place_guid, List<Point> path, Point target_pos)
    {
        this.target_place_guid = target_place_guid;
        this.target_pos = target_pos;
        this.path = path;
        _duration = -1;
        return this;
    }

    public virtual SimAgentStateWalk Init(int target_place_guid, List<Point> path, bool is_end_offest, float speed_rate, string emoji_on_the_way, float emoji_interval, float emoji_show_duration, string song)
    {
        this.target_place_guid = target_place_guid;
        target_pos = path[path.Count - 1];
        _duration = -1;
        this.is_end_offest = is_end_offest;
        this.path = path;
        this.speed_rate = speed_rate;
        this.emoji_on_the_way = emoji_on_the_way;
        this.emoji_interval = emoji_interval;
        this.emoji_show_duration = emoji_show_duration;
        this.song = song;
        return this;
    }
    #endregion

    protected override void OnEnter()
    {
        //目标地点为空，直接返回寻路失败
        if (target_pos.IsEmptyPoint)
        {
            has_no_way_callback_next_frame = true;
            return;
        }

        if (path==null || path.Count==0)
        {
            StartPathFinding();
        }
        else
        {
            //不要立即执行StartWalk，先做标记，在下一帧的Tick中，判断该标识位，再执行
            is_move_follow_path = true;
        }
    }

    private void StartPathFinding()
    {
        Point pstart = TryAdjustPointForNav(city_map, npc.CurPoint);
        Point pend = TryAdjustPointForNav(city_map, target_pos);

        List<Point> path = city_map.nav_manager.FindPath(pstart, pend);
        OnFindPathResult(path);
    }

    public static Point TryAdjustPointForNav(CityMap city_map, Point p)
    {
        if (city_map.city_state.IsCellLand(p))
        {
            return p;
        }

        int max_range = Min(p.x, city_map.city_state.map_w - p.x, p.y, city_map.city_state.map_h - p.y);
        MyRectInt rect = new MyRectInt(0, 0, city_map.city_state.map_w, city_map.city_state.map_h);
        List<Point> pos_list = new List<Point>();
        for (int range = 1; range < max_range; range++)
        {
            pos_list.Clear();
            GetPointAround(p, range, pos_list, rect);
            foreach (var t in pos_list)
            {
                if (city_map.city_state.IsCellLand(t))
                {
                    return t;
                }
            }
        }
        return Point.EmptyPoint;
    }

    public static void GetPointAround(Point p, int dist, List<Point> output_point_list, MyRectInt rect)
    {
        if (dist == 0) { output_point_list.Add(p); }
        else
        {
            int x = 0;
            int y = 0;
            for (int i = -dist; i <= dist; i++)
            {
                x = p.x + i;
                y = p.y - dist;
                CheckAndAddPos(rect, output_point_list, x, y);
                x = p.x + i;
                y = p.y + dist;
                CheckAndAddPos(rect, output_point_list, x, y);
            }
            for (int i = -dist + 1; i <= dist - 1; i++)
            {
                x = p.x + dist;
                y = p.y + i;
                CheckAndAddPos(rect, output_point_list, p.x + dist, p.y + i);
                x = p.x - dist;
                y = p.y + i;
                CheckAndAddPos(rect, output_point_list, p.x - dist, p.y + i);
            }
        }
    }

    private static void CheckAndAddPos(MyRectInt rect, List<Point> output_list, int x, int y)
    {
        if (rect.Contains(x, y))
        {
            output_list.Add(new Point(x, y));
        }
    }

    private static int Min(int n1, int n2, int n3, int n4)
    {
        int r = n1;
        if (r > n2) { r = n2; }
        if( r > n3) { r = n3; }
        if(r > n4) { r = n4; }
        return r;
    }

    private void OnFindPathResult(List<Point> list)
    {
        if (list == null || list.Count == 0)
        {
            Log.w(npc.LOG_PREFIX + " 寻路失败 " + (this.GetType().Name) + npc.CurPoint + "->" + target_pos);
            OnNoWay();
        }
        else
        {
            //出发点位置去掉，避免强行让NPC走到当前格的中心点
            if (list.Count > 1 && npc.CurPoint.Equals(list[0]))
            {
                list.RemoveAt(0);
            }

            //加入经过栅栏的点
            list.Add(target_pos);

            //通过指定的点
            if (is_move_through)
            {
                list.Add(real_end_pos);
            }

            //通过梯子，增加点，以使表现看起来像是爬梯子
            //AppendLadderPoint(list);
            StartWalk(list);
            is_start_walk = true;

        }
    }
    
    private void StartWalk(List<Point> path)
    {
        List<MyVector2> real_path = PointPathToVector3Path(path);
        if (is_end_offest)
        {
            real_path[real_path.Count-1] += new MyVector2(MyRandom.Between(-5, 6)*SimAgent.MIN_DIST_STEP, MyRandom.Between(-5, 6) * SimAgent.MIN_DIST_STEP);
        }
        npc.move_agent.Walk(real_path, OnArriverPoint);
        OnMoveStart(path);
        
        //播放音乐
        npc.StartActionOnWalk(speed_rate, emoji_on_the_way, emoji_interval, emoji_show_duration, song);
    }

    public static List<MyVector2> PointPathToVector3Path(List<Point> path)
    {
        List<MyVector2> list = new List<MyVector2>();
        foreach (var t in path)
        {
#if UNITY_EDITOR
            if (t.Equals(Point.zero))
            {
                Log.e("注意：移动目标位置是（0,0），很可能有BUG");
            }
#endif
            list.Add(t.ToMyVector2());
        }
        return list;
    }


    protected virtual void OnArriverPoint(int index_in_path)
    {
        
    }

    protected override void OnTick()
    {
        if (is_move_follow_path && !is_start_walk)
        {
            StartWalk(path);
            is_start_walk = true;
        }

        if (has_no_way_callback_next_frame)
        {
            has_no_way_callback_next_frame = false;
            Log.e("目标行走位置为空，寻路失败");
            OnNoWay();
            return;
        }

        if (is_start_walk && npc.move_agent.IsStopped())
        {
            OnStopped();
        }
        else
        {
            base.OnTick();

            //同步位置信息
            city_map.SubmitAgentState(npc);
        }
    }

    protected virtual void OnMoveStart(List<Point> path) { }

    //在OnExit中，会执行agent.StopMove()。此处不需处理移动问题
    private bool is_arrived_target_pos = false;
    protected virtual void OnStopped()
    {
        is_arrived_target_pos = true;
        if (npc.OnMoveStopped())
        {
            //被主动改变了行为状态
        }
        else
        {
            npc.ChangeStateToIdle();
        }
    }
    protected virtual void OnTaskCancel() {
        npc.ChangeStateToIdle();
    }

    protected virtual void OnNoWay() 
    {
        GiveBackOrCancelTaskOnNoWay();
        npc.ChangeStateToIdle();
    }

    protected void GiveBackOrCancelTaskOnNoWay()
    {
        
    }

    protected override void OnExit()
    {
        if (!is_arrived_target_pos)
        {
            Log.e("注意：智能体的移动行为被中断 agent_guid=" + npc.Guid);
        }

        npc.StopActionOnWalk(emoji_on_the_way, song);
        base.OnExit();
        npc.move_agent.StopMove();
    }
    
    public override string GetDesc()
    {
        if (target_place_guid != 0)
        {
            MapObj obj = city_map.city_state.GetMapObjByGuid(target_place_guid);
            if (obj != null)
            {
                return "On the way to the " + obj.name;
            }
        }
        return "On the way to somewhere";
    }
}

