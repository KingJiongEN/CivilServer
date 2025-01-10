using System;
using System.Collections;
using System.Collections.Generic;

public class MyListHashtable<K, T> : IEnumerable<T>
{
    public MyHashtable<K, List<T>> data_dic  =new MyHashtable<K, List<T>>();
    //public void Add(K k, T t)
    //{
    //    List<T> list = data_dic[k];
    //    if (list == null)
    //    {
    //        this[k] = new List<T>() { t};
    //    }
    //    else
    //    {
    //        list.Add(t);
    //    }
        
    //}

    //public bool Remove(K k, T t)
    //{
    //    List<T> list = data_dic[k];
    //    if (list != null)
    //    {
    //        return list.Remove(t);
    //    }
    //    return false;
    //}

    //public bool Contains(K k, T t)
    //{
    //    List<T> list = data_dic[k];
    //    return list != null && list.Contains(t);
    //}

    public List<T> this[K key]
    {
        get
        {
            return data_dic[key];
        }
        set
        {
            data_dic[key] = value;
        }
    }

    public T FetchOne(K k)
    {
        T r = default(T);
        //访方法可能涉及多线程访问
        lock (this)
        {
            List<T> list = data_dic[k];
            if (list != null && list.Count > 0)
            {
                int p = list.Count - 1;
                r = list[p];
                list.RemoveAt(p);
            }
        }
        return r;
    }

    public List<T> GetOrCreate(K k)
    {
        List<T> list = data_dic[k];
        if (list != null)
        {
            return list;
        }
        else
        {
            list = new List<T>();
            data_dic[k] = list;
        }
        return list;
    }


    #region 迭代器
    public IEnumerator<T> GetEnumerator()
    {
        return new MyListDicEnumerate(data_dic);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return (IEnumerator)GetEnumerator();
    }


    private class MyListDicEnumerate : IEnumerator<T>
    {
        private MyHashtable<K, List<T>> my_list_dic;
        private int position = -1;
        private List<T> cur_list = null;
        private IEnumerator<List<T>> dic_enumerator;

        public MyListDicEnumerate(MyHashtable<K, List<T>> my_list_dic)
        {
            this.my_list_dic = my_list_dic;
            dic_enumerator = my_list_dic.Values.GetEnumerator();
        }

        public T Current => cur_list[position];

        object IEnumerator.Current => Current;

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            position++;
            if (cur_list == null || position >= cur_list.Count)
            {
                while (dic_enumerator.MoveNext())
                {
                    cur_list = dic_enumerator.Current;
                    if (cur_list.Count > 0)
                    {
                        position = 0;
                        return true;
                    }
                }
                return false;
            }
            return true;
        }

        public void Reset()
        {
            position = -1;
            cur_list = null;
        }
    }

    #endregion

    internal bool ContainsKey(K key)
    {
        return data_dic.ContainsKey(key);
    }

    public IEnumerable<K> Keys
    {
        get { return data_dic.Keys; }
    }

    public IEnumerable<List<T>> Values
    {
        get { return data_dic.Values; }
    }

    public int KeyCount { get { return data_dic.Count; } }
}
