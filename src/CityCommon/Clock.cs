using System;
using System.Collections.Generic;

public class Clock
{
    //真实1小时，等于1年。一年360天
    //时钟：
    //1年 12个月 真实1小时
    //每个月 真实5分钟
    //每天 真实10秒钟

    //正常流逝，1年，真实时间1小时
    //5倍速，真实时间12分钟
    //10倍速，真实时间6分钟
    // 游戏时间1天，真实时间10秒
    //10倍速，真实1秒
    //60倍速，真实时间10帧（60帧）
    public static float time_scale = 360 * 24 * 6;
    
    //游戏中的一天，对应的真实时间
    public static float real_time_of_day = (24 * 60 * 60) / time_scale; 
    public static float days_of_real_time_second = 1 / real_time_of_day;
    
    private DTime dtime = new DTime();
    public static DTime Now = new DTime();
    public static int Days = 0;
    public bool is_clock_started = false;
    
    public static int frameCount;
    public static float timeDelta;
    public static float unscaledDeltaTime;
    public static float time;
    public static float unscaledTime;
    public static float timeScale;

    public Action OnNewDay;
    public Action OnNewMonth;
    public Action OnNewYear;
    

    public static float GameDeltaTimeOfDay
    {
        get { return timeDelta * days_of_real_time_second; }
    }

    internal string GetDateStr()
    {
        return Now.GetDateStr()+ Now.Day;
    }

    public static float GameTimeToRealTime(float days)
    {
        return days * real_time_of_day;
    }

    private List<Pair<int,int>> dark_day = new List<Pair<int, int>>();

    public void Init()
    {
        List<int> dark_month = new List<int>() { 5, 11 };
        foreach(var t in dark_month)
        {
            //结束时间，是开始时间加上 DTime.DAY_IN_MONTH-1，减一是为了避免出现11月最后一天，是360。因为最大时间是359，然后是0开始
            dark_day.Add(new Pair<int,int>(DTime.DAY_IN_MONTH * t + 1, DTime.DAY_IN_MONTH * t + (DTime.DAY_IN_MONTH-1)));
        }
    }
    
    public bool IsAtDarkStartDays(int prepare_days, int delay_days)
    {
        int day_in_year = Now.DayInYear;
        foreach (var t in dark_day)
        {
            int delta = day_in_year - t.a;
            if(delta >= -prepare_days && delta <= delay_days)
            {
                return true;
            }
        }
        return false;
    }

    public bool IsDaysNeedMakeFire(int prepare_days)
    {
        int day_in_year = Now.DayInYear;
        foreach (var t in dark_day)
        {
            if (day_in_year <= t.b && t.a - prepare_days < day_in_year)
            {
                return true;
            }
        }
        return false;
    }

    public void Tick()
    {
        if (!is_clock_started) { return; }
        // 1年 12个月 真实1小时
        // 每个月 真实5分钟
        // 每天 真实10秒钟

        //5倍加速，1天，真实2秒钟
        //10倍加速，1天，真实1秒钟
        int days = dtime.Days;
        int months = dtime.Months;
        int years = dtime.Years;
        dtime.time += Clock.timeDelta;
        //赋值给
        Now.time = dtime.time;
        Days = Now.Days;

        //Log.d("游戏时间："+ GetDateStr() + "[" + days + "] 帧：" + Game.frameCount);
        if (days != dtime.Days)
        {
            DoOnNewDay();
        }
        if (months != dtime.Months)
        {
            DoOnNewMonth();
        }
        if (years != dtime.Years)
        {
            DoOnNewYear();
        }
    }

   public bool IsDarkTime
    {
        get {
            int day_in_year = Now.DayInYear;
            foreach(var t in dark_day)
            {
                if (t.a <= day_in_year && day_in_year <= t.b) { 
                    return true; 
                }
            }
            return false;
        }
    }

    private void DoOnNewDay(){
        try{ OnNewDay?.Invoke();}catch(Exception e){Log.e(e);}
    }

    private void DoOnNewMonth(){
        try{ OnNewMonth?.Invoke();}catch(Exception e){Log.e(e);}
    }

    private void DoOnNewYear()
    {
        try{ OnNewYear?.Invoke();}catch(Exception e){Log.e(e);}
    }

    public void Recover(float time)
    {
        SetTime(time);
        is_clock_started = true;
    }

    public void SetTime(float time)
    {
        dtime.time = time;
        Now.time = time;
        Days = Now.Days;
    }

    public static void Update(int frameCount, float deltaTime, float unscaledDeltaTime, float time, float unscaledTime, float timeScale)
    {
        Clock.frameCount = frameCount;
        Clock.timeDelta = deltaTime;
        Clock.unscaledDeltaTime = unscaledDeltaTime;
        Clock.time = time;
        Clock.unscaledTime = unscaledTime;
        Clock.timeScale = timeScale;
    }
}
