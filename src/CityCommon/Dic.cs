using System.Collections.Generic;
using System.Text;

public class Dic<K,T> : Dictionary<K, T>
{
    private string table_name;
    public Dic() { }
    public Dic(string table_name)
    {
        this.table_name = table_name;
    }


    public new T this[K key]
    {
        get
        {
            if(TryGetValue(key, out T v))
            {
                return v;
            }
            else
            {
                if (table_name != null)
                {
                    Log.e("[" + table_name + "]表中，不在存编号为" + key + "的数据");
                }
                return default(T);
            }
        }
        set
        {
            base[key] = value;
        }
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        foreach(var t in this)
        {
            if (sb.Length > 0)
            {
                sb.Append(", ");
            }
            sb.Append(t.Key);
            sb.Append(":");
            sb.Append(t.Value);
        }
        return sb.ToString();

    }
}

