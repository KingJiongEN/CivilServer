using System;
using Newtonsoft.Json;

[Serializable]
public struct Point : IEquatable<Point>, IComparable
{
	public const float CELL_SIZE = 1F;
	public const float MIN_FLOAT_DIST = 0.001f;
	public int x, y;

    public Point( int x, int y )
	{
		this.x = x;
		this.y = y;
	}

	public Point(Point p)
    {
		this.x = p.x;
		this.y = p.y;
    }

	public static Point PositionToPoint(MyVector2 p)
	{
		p.y += MIN_FLOAT_DIST;//保证，不会因整数与小数转换时，FloorToInt计算不符合预期
		return new Point((int)Math.Floor(p.x / CELL_SIZE), (int)Math.Floor(p.y / CELL_SIZE));
	}
	
	public static Point Zero { get { return new Point(0, 0); } }
	//只做默认值，不能对其进行修改
	[NonSerialized]
	public static Point zero = new Point(0, 0);
	[NonSerialized]
	//取hashcode,为x<<16 | y，所以最大边界值为<<15
	private const int MAX_XY = 1 << 15;
	public static Point EmptyPoint { get { return new Point(MAX_XY, MAX_XY); } }
	
	[JsonIgnore]
	public bool IsEmptyPoint { get { return x >= MAX_XY || y >= MAX_XY; } }
	
    
	public MyVector2 ToMyVector2()
	{
		return new MyVector2((x + 0.5f) * CELL_SIZE, (y + 0.5f) * CELL_SIZE);
	}
	public static Point operator+ (Point a, Point b){

		return new Point(a.x + b.x, a.y + b.y);
	}

	public static Point operator -(Point a, Point b)
	{

		return new Point(a.x - b.x, a.y - b.y);
	}

	public override bool Equals(object other)
	{
		if (other!=null && other is Point)
        {
			Point p = (Point)other;
			return x == p.x && y == p.y;
		}
		return false;
	}

	public bool Equals(Point p)
	{
		return x == p.x && y == p.y;
	}

    public override int GetHashCode()
    {
        return x << 16 | y;
    }

    public static bool operator== (Point a, Point b)
	{
		return a.x == b.y && a.y == b.y;
	}

	public static bool operator !=(Point a, Point b)
	{
		return a.x != b.x || a.y != b.y;
	}

	public bool IsNextTo(Point p)
    {
		return Math.Abs(p.x - x) == 1 && Math.Abs(p.y - y) == 1;
    }

    // Get Difference between two points, assuming only cardianal or diagonal movement is possible
    public static int diff( Point a, Point b )
	{
		// because diagonal
		// 0,0 diff 1,1 = 1 
		// 0,0 diff 0,1 = 1 
		// 0,0 diff 1,2 = 2 
		// 0,0 diff 2,2 = 2 
		// return max of the diff row or diff column
		int diff_columns = Math.Abs( b.x - a.x );
		int diff_rows    = Math.Abs( b.y - a.y );

		return Math.Max( diff_rows, diff_columns );
	}

	public override string ToString()
	{
		return "(" + this.x + "," + this.y + ")";
	}

	public int Dist(Point p)
    {
        return Math.Abs(x - p.x) + Math.Abs(y - p.y);
    }

    public int CompareTo(object obj)
    {
        if (obj is Point p)
        {
			return ((x<<16)|y) - ((p.x<<16)|p.y);
        }
		return -1;
    }
}
