using System;
using System.Collections.Generic;

public class SimMoveAgent
{
    public List<MyVector2> path = new List<MyVector2>();
    public Action<int> onArriverPointCallback;
    protected bool have_path_to_move = false;
    public int cur_index_in_path = 0;
    public float speed = 1f;
    public float speed_percent = 1f;
    public IMovable walker;
    public int Guid { get { if (walker != null) { return walker.Guid; } else { return 0; } } }

    protected virtual void InitGo()
    {
        movable_obj_index = -1;
    }

    public float SpeedPercent
    {
        get { return speed_percent; }
        set { speed_percent = value; }
    }

    public void Init(IMovable walker, float speed)
    {
        InitGo();

        this.walker = walker;
        this.Speed = speed;
    }

    public float Speed
    {
        get { return speed; }
        set { speed = value; }
    }

    public virtual void StopMove()
    {
        if (have_path_to_move)
        {
            path.Clear();
            is_paused = false;
            is_paused_a_while = false;
            pause_duration = -1;

            onArriverPointCallback = null;
            cur_index_in_path = 0;
            have_path_to_move = false;
        }
    }

 
    public bool IsStopped()
    {
        return !have_path_to_move;
    }

    public virtual void Walk(MyVector2 target_pos)
    {
        if (have_path_to_move)
        {
            Log.e("错误：新移动计划开始前，未StopMove");
            StopMove();
        }

        this.path.Clear();
        this.path.Add(target_pos);
        this.onArriverPointCallback = null;

        DoWalk();
    }
    public virtual void Walk(List<MyVector2> path, Action<int> onArriverPointCallback=null)
    {
        if (have_path_to_move)
        {
            Log.e("错误：新移动计划开始前，未StopMove");
            StopMove();
        }

        this.path.Clear();
        this.path.AddRange(path);
        this.onArriverPointCallback = onArriverPointCallback;

        DoWalk();
    }

    protected virtual void DoWalk()
    {
        cur_index_in_path = 0;
        have_path_to_move = path!=null && path.Count>0;
    }
    
    public virtual void Tick()
    {
        MoveFollowPath();
    }

    //暂停
    protected bool is_paused = false;
    //暂时一定时间
    protected float pause_duration = 0;
    protected bool is_paused_a_while = false;
    public void Pause(float move_pause_duration=-1)
    {
        is_paused = true;
        if (move_pause_duration > 0)
        {
            this.pause_duration = move_pause_duration;
            this.is_paused_a_while = true;
        }
    }

    public void Resume()
    {
        is_paused = false;
        is_paused_a_while = false;
        pause_duration = -1;
    }

    public float GetCurSpeed()
    {
        return (speed * SpeedPercent);
    }

    public bool IsMoving()
    {
        if (is_paused_a_while)
        {
            //动画表现是不受timescale影响的。为了配合表现，逻辑暂停时间不受timescale影响。
            pause_duration -= Clock.unscaledTime;
            if (pause_duration <= 0)
            {
                Resume();
            }
            return false;
        }

        if (is_paused)
        {
            return false;
        }

        if (have_path_to_move)
        {
            if (cur_index_in_path < path.Count)
            {
                return true;
            }
        }
        return false;
    }

    ///调用此方法前，需先调用IsMoving做判断
    public MyVector2 GetCurTargetPos()
    {
        return path[cur_index_in_path];
    }

    protected virtual void MoveFollowPath()
    {
        float time = Clock.timeDelta;
        if (is_paused_a_while)
        {
            //动画表现是不受timescale影响的。为了配合表现，逻辑暂停时间不受timescale影响。
            pause_duration -= Clock.unscaledTime;
            if (pause_duration <= 0)
            {
                Resume();
            }
            return;
        }

        if (is_paused)
        {
            return;
        }

        if (have_path_to_move)
        {
            MyVector2 cur_pos = walker.Position;
            float cur_speed = GetCurSpeed();
            float dist = cur_speed * time;
            //Log.d(dist + " " + cur_index_in_path + "/" + path.corners.Length);
            MyVector2 next_pos = MyVector2.zero;
            float next_dist = 0;
            while (cur_index_in_path < path.Count)
            {
                next_pos = path[cur_index_in_path];
                next_dist = (next_pos - cur_pos).magnitude;
                if (next_dist > dist)
                {
                    MyVector2 delta = (next_pos - cur_pos).normalized * dist;
                    //if (gameObject.name.Equals("李兆"))
                    //{
                    //    Log.d("李兆 Game.timeDelta=" + time);
                    //    Log.d("[" + Game.frameCount + "]" + cur_pos + " " + next_pos + "("+ delta.x+","+ delta .y+ ")" + " dist="+ dist + " speed=" + speed + " speed_percent="+ speed_percent+ " time="+ time+" " + transform.gameObject.name);
                    //}

                    //Log.d("speed=" + cur_speed + " dist=" + dist);
                    MoveStepBy(delta);
                    break;
                }
                dist -= next_dist;
                cur_pos = next_pos;
                MoveStepTo(cur_pos);
                OnArriverPoint();
            }
        }
    }

    public void OnArriverPoint()
    {
        onArriverPointCallback?.Invoke(cur_index_in_path);
        cur_index_in_path++;
        if (cur_index_in_path >= path.Count)
        {
            StopMove();
        }
    }


    //移动，且改变朝向
    public virtual void MoveStepTo(MyVector2 pos)
    {
        //计算模块，计算移动后，再调用MoveStepTo的动作。可能在该动作之前，NPC状态已切换，移动被强制中断了
        //这个时候，需忽略计算结果，避免覆盖，其它逻辑设置的NPC动画状态及位置
        if (have_path_to_move)
        {
            //Log.w(walker.GoName + walker.Position + pos);
            walker.Position = pos;
        }
    }

    public void OnMove(MyVector2 delta)
    {

    }
    protected virtual void MoveStepBy(MyVector2 delta)
    {
        MyVector2 p = walker.Position + delta;
        walker.Position = p;
    }
    
    #region 异步计算接口


    public void SetPosition(MyVector2 p)
    {
        if (!walker.IsDead)
        {
            MoveStepTo(p);
        }
    }

    public MyVector2 GetPosition()
    {
        return walker.Position;
    }

    public float GetSpeed()
    {
        return Speed;
    }

    public void OnMoveFinish()
    {
        StopMove();
    }
    
    public int movable_obj_index { get; set; }
    public int move_action_version { get; set; }
    
    #endregion

    public List<Point> GetRemainPath()
    {
        List<Point> r = new List<Point>();
        for (int i = cur_index_in_path; i < path.Count; i++)
        {
            r.Add(Point.PositionToPoint(path[i]));
        }
        return r;
    }
}
