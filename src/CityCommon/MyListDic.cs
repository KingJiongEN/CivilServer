using System.Collections.Generic;

public class MyListDic<K, T> : Dic<K, List<T>>
{
    public void Add(K k, T t)
    {
        if (TryGetValue(k, out List<T> list))
        {
            list.Add(t);
        }
        else
        {
            this[k] = new List<T>() { t };
        }
    }

    public bool Remove(K k, T t)
    {
        if (TryGetValue(k, out List<T> list))
        {
            return list.Remove(t);
        }
        return false;
    }

    public bool Contains(K k, T t)
    {
        if (TryGetValue(k, out List<T> list))
        {
            return list.Contains(t);
        }
        return false;
    }

    public void Get(List<T> output_list, List<K> klist)
    {
        foreach(var k in klist)
        {
            if (TryGetValue(k, out List<T> list))
            {
                output_list.AddRange(list);
            }
        }
    }

    private List<T> output_list = new List<T>();

    public List<T> Get(List<K> key_arr)
    {
        output_list.Clear();
        foreach (K k in key_arr)
        {
            if (TryGetValue(k, out List<T> list))
            {
                output_list.AddRange(list);
            }
        }
        return output_list;
    }

    public int DateItemCount {
        get
        {
            int count = 0;
            foreach(var t in this)
            {
                count += t.Value.Count;
            }
            return count;
        }
    }

    public bool IsEmpty()
    {
        foreach(var t in this)
        {
            if (t.Value.Count > 0)
            {
                return false;
            }
        }
        return true;
    }

    public void ClearListData()
    {
        foreach (var t in this)
        {
            t.Value.Clear();
        }
    }
}
