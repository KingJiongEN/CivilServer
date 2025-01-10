using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
public class MyHashtable<K, T> : IEnumerable<KeyValuePair<K, T>>
{
    #region 迭代器

    public class MyKVEnumerate : IEnumerator<KeyValuePair<K, T>>
    {
        private IDictionaryEnumerator dic_enumerator;

        public MyKVEnumerate(IDictionaryEnumerator dic_enumerator)
        {
            this.dic_enumerator = dic_enumerator;
        }

        public object Current => Current;

        KeyValuePair<K, T> IEnumerator<KeyValuePair<K, T>>.Current
        {
            get
            {
                return new KeyValuePair<K, T>((K)dic_enumerator.Key, (T)dic_enumerator.Value);
            }
        }

        public void Dispose()
        {

        }

        public bool MoveNext()
        {
            return dic_enumerator.MoveNext();
        }

        public void Reset()
        {
            dic_enumerator.Reset();
        }
    }
    private class MyDicEnumerate<S> : IEnumerator<S>
    {
        private IEnumerator dic_enumerator;

        public MyDicEnumerate(IEnumerator dic_enumerator)
        {
            this.dic_enumerator = dic_enumerator;
        }

        public S Current => (S)dic_enumerator.Current;

        object IEnumerator.Current => Current;

        public void Dispose()
        {

        }

        public bool MoveNext()
        {
            return dic_enumerator.MoveNext();
        }

        public void Reset()
        {
            dic_enumerator.Reset();
        }
    }

    private class ValuesEnumerable : IEnumerable<T>
    {
        private Hashtable dic;
        public ValuesEnumerable(Hashtable dic)
        {
            this.dic = dic;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new MyDicEnumerate<T>(dic.Values.GetEnumerator());
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)GetEnumerator();
        }
    }

    private class KeysEnumerable : IEnumerable<K>
    {
        private Hashtable dic;
        public KeysEnumerable(Hashtable dic)
        {
            this.dic = dic;
        }

        public IEnumerator<K> GetEnumerator()
        {
            return new MyDicEnumerate<K>(dic.Keys.GetEnumerator());
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)GetEnumerator();
        }
    }


    #endregion

    private Hashtable dic = new Hashtable();
   
    private string table_name;
    public IEnumerable<T> Values { get { return new ValuesEnumerable(dic); } }
    public IEnumerable<K> Keys { get { return new KeysEnumerable(dic); } }

    public string TableName{get{return table_name;}}

    public int Count { get { return dic.Count; } }

    public MyHashtable(string table_name)
    {
        this.table_name = table_name;
    }

    public MyHashtable() { }

    public T this[K key] 
    {
        get
        {
#if UNITY_EDITOR
            if (key == null)
            {
                if (table_name !=null)
                {
                    Log.e("[" + table_name + "]表,读取数据出错 key = null");
                }
                return default(T);
            }

            if (!dic.ContainsKey(key))
            {
                if (table_name != null)
                {
                    Log.e("[" + table_name + "]表中，不在存编号为" + key + "的数据");
                }
                return default(T);
            }
#endif
            return (T)dic[key];
        }
        set
        {
            dic[key] = value;
        }
    }

    public bool ContainsKey(K key)
    {
        return dic.ContainsKey(key);
    }

    public bool ContainsValue(T value)
    {
        return dic.ContainsValue(value);
    }


    internal bool Remove(K f)
    {
        if (!dic.ContainsKey(f)) { return false; }
        dic.Remove(f);
        return true;
    }

    public void Clear()
    {
        dic.Clear();
    }

    public IEnumerator<KeyValuePair<K, T>> GetEnumerator()
    {
        return new MyKVEnumerate(dic.GetEnumerator());
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    //调用方需自己判断，确保this.Count不为0
    internal void GetRandomKV(out K _k, out T _v)
    {
        int rnd_index = MyRandom.NextInt(this.Count);
        foreach(var kv in this)
        {
            if(rnd_index == 0)
            {
                _k = kv.Key;
                _v = kv.Value;
                return;
            }
            rnd_index--;
        }
        _k = default(K);
        _v = default(T);
    }
}

