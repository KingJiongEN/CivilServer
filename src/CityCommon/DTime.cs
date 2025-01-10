using System;
using System.Collections.Generic;

[Serializable]
public class DTime
{
    public static readonly List<string> MONTH_NAME = new List<string>() { "早春", "春", "晚春", "初夏", "仲夏", "夏末", "早秋", "仲秋", "晚秋", "早冬", "仲冬", "晚冬" };


    public const int MONTH_IN_YEAR = 12;
    public const int DAY_IN_MONTH = 30;
    public const int DAY_IN_YEAR = DAY_IN_MONTH * MONTH_IN_YEAR;

    public const int SPRING = 0;
    public const int SUMMER = 1;
    public const int AUTUMN = 2;
    public const int WINTER = 3;

    public DTime() { }

    //游戏系统时间，timescale为1时，与真实世界时间相同
    public float time;

    #region 修改动作，会产生一个新的对象
    public DTime DayIncrease(float game_day)
    {
        return new DTime(time + game_day * Clock.real_time_of_day);
    }

    public DTime YearIncrease(float game_year)
    {
        return DayIncrease(game_year * DAY_IN_YEAR * Clock.real_time_of_day);
    }
    #endregion

    public string MonthName
    {
        get { return MONTH_NAME[MonthInYear]; }
    }

    public static DTime zero = new DTime(0);

    public string GetDateStr()
    {
        return (MyStringBuilder.Create((Year + 1)) + "年 " + MONTH_NAME[MonthInYear]).ToString();

    }

    public string GetDateStrForArchive()
    {
        return (MyStringBuilder.Create((Year + 1)) + "年 " + MONTH_NAME[MonthInYear] + (Day + 1)).ToString();
    }

    public int Year { get { return Days / DAY_IN_YEAR; } }
    public int MonthInYear { get { 
            int t= (Days % DAY_IN_YEAR) / DAY_IN_MONTH;
            if (t < 0) { t += MONTH_IN_YEAR; }
            return t;
        } }
    public int Day { get { return Days % DAY_IN_MONTH; } }
    public int DayInYear { get { return Days % DAY_IN_YEAR; } }

    public DTime(float time=0){this.time = time; }

    public int DeltaDay(DTime d)
    {
        return Days - d.Days;
    }

    public int DeltaYear(DTime d)
    {
        return Years - d.Years;
    }


    public int Years { get { return Days / DAY_IN_YEAR; } }
    public int Months { get { return Days / DAY_IN_MONTH; } }
    public int Days { get { return (int)(time * Clock.days_of_real_time_second); } }
    public int Season { get { return MonthInYear / 4; } }

    public bool IsSummerOrAutumn
    {
        get
        {
            int s = Season;
            return s == SUMMER || s == AUTUMN;
        }
    }

    internal DTime Copy()
    {
        return new DTime(time);
    }
}
