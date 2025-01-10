using System;

public class TickLog
{
    private static long start;
    private static MyHashtable<string, long> dic = new MyHashtable<string, long>();
    public static void Start()
    {
        start = DateTime.Now.Ticks; //单位 100 百纳秒，即0.1微秒
    }

    public static void Start(string title)
    {
        dic[title] = DateTime.Now.Ticks; //单位 100 百纳秒，即0.1微秒
    }

    public static void End(string title)
    {
        //1秒 = 1000毫秒
        //1毫秒 = 1000微秒
        //1微秒 = 1000纳秒
        if (dic.ContainsKey(title))
        {
            Log.d(title + ": " + (DateTime.Now.Ticks - dic[title]) / 10000 + "ms");
            dic.Remove(title);
        }
        else
        {
            Log.d(title + ": " + (DateTime.Now.Ticks - start) / 10000 + "ms");
            start = DateTime.Now.Ticks;
        }
    }
}

