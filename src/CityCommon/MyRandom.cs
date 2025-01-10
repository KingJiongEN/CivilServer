using System.Collections.Generic;

public class MyRandom
{
    static System.Random random = new System.Random();
    static public int NextInt(int n)
    {
        return random.Next(n);
    }

    static public bool NextBool()
    {
        return NextInt(2) == 1;
    }
    
    static public bool NextBool(float rate)
    {
        return NextFloat(1) < rate;
    }

    static public float UpDown(float range)
    {
        return (float)(random.NextDouble()) * range - range / 2;
    }
    static public float NextFloat(float range)
    {
        return (float)random.NextDouble() * range;
    }

    static public int Between(int index_start, int index_end)
    {
        return random.Next(index_end - index_start) + index_start;
    }

    static public float Between(float start, float end)
    {
        if (start > end)
        {
            float t = start;
            start = end;
            end = t;
        }

        float range = end - start;
        return NextFloat(range) + start;
    }

    public static T Choose<T>(List<T> list)
    {
        if (list == null|| list.Count==0) return default(T);
        return list[NextInt(list.Count)];
    }

    public static T Choose<T>(HashSet<T> set)
    {
        int index = NextInt(set.Count);
        foreach(var t in set)
        {
            index--;
            if (index == 0) { return t; }
        }
        return default(T);
    }

    public static List<T> Choose<T>(List<T> list, int n)
    {
        if (list == null || list.Count == 0) return default(List<T>);
        List<T> tlist = new List<T>(list);
        List<T> r = new List<T>();
        MyRandom.DisruptList(tlist);
        int count = n;
        if (count > tlist.Count) { count = tlist.Count; }
        for(int i = 0; i < count; i++)
        {
            r.Add(tlist[i]);
        }
        return r;
    }

    //private static int  id = DateTime.Now.Second;
    //public static int NewRuntimeGuid()
    //{
    //    id++;
    //    return id;
    //}

    public static T Choose<T>(T[] arr)
    {
        if (arr == null || arr.Length == 0) return default(T);
        return arr[NextInt(arr.Length)];
    }

    public static string Choose(List<string> list)
    {
        return Choose<string>(list);
    }

    public static int Choose(List<int> list)
    {
        return Choose<int>(list);
    }

    public static int Choose(int[] list)
    {
        return list[NextInt(list.Length)];
    }

    public static MyVector2 Choose(List<MyVector2> list)
    {
        return Choose<MyVector2>(list);
    }

    public static string Choose(string[] list)
    {
        if (list == null) return null;
        return list[NextInt(list.Length)];
    }

    public static void DisruptList<T>(List<T> list)
    {
        if (list == null || list.Count == 0) return;
        for (int i = 0; i < list.Count; i++)
        {
            int tp = MyRandom.NextInt(list.Count);
            T t = list[tp];
            list[tp] = list[i];
            list[i] = t;
        }
    }

    public static void DisruptList<T>(T[] list)
    {
        if (list == null || list.Length == 0) return;
        for (int i = 0; i < list.Length; i++)
        {
            int tp = MyRandom.NextInt(list.Length);
            T t = list[tp];
            list[tp] = list[i];
            list[i] = t;
        }
    }

    public static int Choose(List<int> list, List<int> probability)
    {
        int denominator = 0;
        foreach (int n in probability)
        {
            denominator += n;
        }

        int r = MyRandom.NextInt(denominator);
        int count = 0;
        int i = 0;
        for (i = 0; i < probability.Count; i++)
        {
            if (count <= r && count + probability[i] > r)
            {
                break;
            }
            count += probability[i];
        }
        return list[i];
    }
}
