using System;

[Serializable]
public struct MyVector2
{
    public float x;
    public float y;
    private static MyVector2 _zero = new MyVector2(0,0);

    public MyVector2(float x, float y)
    {
        this.x = x;
        this.y = y;
    }

    public override string ToString()
    {
        return (MyStringBuilder.Create("(") + x + "," + y + ")").ToStr();
    }

    public static MyVector2 operator +(MyVector2 a, MyVector2 b)
    {
        return new MyVector2(a.x + b.x, a.y + b.y);
    }
    public static MyVector2 operator -(MyVector2 a, MyVector2 b)
    {
        return new MyVector2(a.x - b.x, a.y - b.y);
    }
    
    public static MyVector2 operator /(MyVector2 a, float b)
    {
        return new MyVector2(a.x / b, a.y / b);
    }
    
    public static MyVector2 operator *(MyVector2 a, float b)
    {
        return new MyVector2(a.x * b, a.y * b);
    }

    [Newtonsoft.Json.JsonIgnore]
    public MyVector2 normalized
    {
        get
        {
            MyVector2 normalized = new MyVector2(this.x, this.y);
            normalized.Normalize();
            return normalized;
        }
    }
    
    public void Normalize()
    {
        float magnitude = this.magnitude;
        if ((double) magnitude > 9.999999747378752E-06)
            this = this / magnitude;
        else
            this = MyVector2.zero;
    }

    [Newtonsoft.Json.JsonIgnore]
    public float magnitude => (float) Math.Sqrt((double) x * (double) x + (double) y * (double) y);
    public static MyVector2 zero => _zero;
}