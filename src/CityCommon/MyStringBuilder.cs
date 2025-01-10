using System.Collections.Generic;
using System.Text;


public class MyStringBuilder 
{
    public static Stack<MyStringBuilder> pool = new Stack<MyStringBuilder>();
    public static System.Object lock_obj = new object();

    private MyStringBuilder()
    {
        sb = new StringBuilder();
    }

    private StringBuilder sb;
    public static MyStringBuilder Create(string str = null)
    {
        lock (lock_obj)
        {
            MyStringBuilder ins = null;
            if (pool.Count > 0)
            {
                ins = pool.Pop();
            }
            else
            {
                ins = new MyStringBuilder();
            }

            if (str != null)
            {
                ins.sb.Append(str);
            }

            return ins;
        }
    }

    private void RecycleSb()
    {
        sb.Clear();
        pool.Push(this);
    }

    public static MyStringBuilder Create(int n)
    {
        return Create(n.ToString());
    }
    public static MyStringBuilder operator +(MyStringBuilder my_string_builder, string s)
    {
        my_string_builder.sb.Append(s);
        return my_string_builder;
    }
    public static MyStringBuilder operator +(MyStringBuilder my_string_builder, int n)
    {
        my_string_builder.sb.Append(n);
        return my_string_builder;
    }
    public static MyStringBuilder operator +(MyStringBuilder my_string_builder, float n)
    {
        my_string_builder.sb.Append(n);
        return my_string_builder;
    }
    public static MyStringBuilder operator +(MyStringBuilder my_string_builder, long n)
    {
        my_string_builder.sb.Append(n);
        return my_string_builder;
    }
    public string ToStr()
    {
        string r = sb.ToString();
        RecycleSb();
        return r;
    }

    public override string ToString()
    {
        Log.e("不应该调用该方法,需调用 ToStr");
        return sb.ToString();
    }

    public MyStringBuilder Append(string s)
    {
        sb.Append(s);
        return this;
    }
    
    public MyStringBuilder Append(int s)
    {
        sb.Append(s);
        return this;
    }
}