using System;

[Serializable]
public struct MyRectInt
{
    public int x;
    public int y;
    public int w;
    public int h;

    public MyRectInt(int x, int y, int w, int h) : this()
    {
        this.x = x;
        this.y = y;
        this.w = w;
        this.h = h;
    }

    public int xMin => x;
    public int yMin => y;
    public int xMax => x + w;
    public int yMax => y + h;

    public int width => w;
    public int height => h;

    public bool Contains(int _x, int _y)
    {
        return _x >= x && _x < x + w && _y >= y && _y < y + h;
    }
    public bool Contains(float _x, float _y)
    {
        return _x >= x && _x < x + w && _y >= y && _y < y + h;
    }
    public bool Contains(Point p)
    {
        return Contains(p.x, p.y);
    }
    
    public bool Contains(MyVector2 p)
    {
        return Contains(p.x, p.y);
    }

    //public override bool Equals(object obj)
    //{
    //    if(obj is MyRectInt)
    //    {
    //        MyRectInt r = (MyRectInt)obj;
    //        return r.x == x && r.y == y && r.w == w && r.h == h;
    //    }
    //    return false;
    //}
    public MyVector2 GetCenter()
    {
        return new MyVector2((xMin + xMax) / 2, (yMin + yMax) / 2);
    }
}

