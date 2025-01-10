using System;
using System.Collections.Generic;

public class MyObjectPool<K> where K : IReusableObj
{
    protected MyListHashtable<Type, K> cache = new MyListHashtable<Type, K>();

    public MyObjectPool() { }

    public virtual T Get<T>() where T : K, new()
    {
        K t = cache.FetchOne(typeof(T));
        if (t == null)
        {
            t = new T();
        }
        else
        {
            t.Reuse();
        }
        return t as T; 
    }

    public virtual void Put(K obj)
    {
        Type type = obj.GetType();
        List<K> list = cache.GetOrCreate(type);
        if (!list.Contains(obj))
        {
            list.Add(obj);
            //缓存里，立即释放相关引用
            obj.Reuse();
        }
        else
        {
            Log.e("错误，即将放到缓存的对象，已经存在于缓存列表中");
        }
    }
}
