
//寻路
using System.Collections.Generic;

public class NavGrid{
    public const int NAVGRID_TYPE_ROAD = 0;
    public const int NAVGRID_TYPE_DOWNTOWN = 1;
    public const int NAVGRID_TYPE_WHOLE_MAP = 2;
    public const int NAVGRID_TYPE_RIVER = 3;


    public JPSGrid grid;
    public int offsetx;
    public int offsety;
    public int w;
    public int h;
    public MapData<byte> data;
    public string name;
    public int type;
    public MyRectInt rect;
    public NavGrid(int type,string name, MapData<byte> data, int offsetx=0, int offsety=0)
    {
        this.type = type;
        this.offsetx = offsetx;
        this.offsety = offsety;
        this.w = data.w;
        this.h = data.h;
        this.data = data;
        this.name = name;
        grid = new JPSGrid(data);    
    }

    public NavGrid(int type, string name, MapData<byte> data, MyRectInt rect)
    {
        this.type = type;
        this.rect = rect;
        this.offsetx = rect.xMin;
        this.offsety = rect.yMin;
        this.w = rect.width;
        this.h = rect.height;
        this.data = data;
        this.name = name;
        grid = new JPSGrid(data, rect);
    }

    public bool IsWalkable(int x,int y)
    {
        if (x >= offsetx && y >= offsety && x - offsetx < w && y - offsety < h)
        {
            return grid.IsWalkable(x - offsetx, y - offsety);
        }
        return false;
    }

    public List<Point> FindPath(Point start, Point end)
    {
        int x0 = offsetx;
        int y0 = offsety;
        int x1 = x0 + w;
        int y1 = y0 + h;
        if (start.x < x0 || start.y < y0 || start.x >= x1 || start.y >= y1) { return null; }
        if (end.x < x0 || end.y < y0 || end.x >= x1 || end.y >= y1) { return null; }

        //return new List<Point>() { start, end };
        if (offsetx != 0 || offsety != 0)
        {
            start.x -= offsetx;
            start.y -= offsety;
            end.x -= offsetx;
            end.y -= offsety;
        }

        if (data[start.x, start.y] != TerrainMap.LAND_MIN_ID) { return null; }
        if (data[end.x, end.y] != TerrainMap.LAND_MIN_ID) { return null; }

        //TickLog.Start();
        List<Point> r = grid.getPath(start, end);
        //TickLog.End("Find Path: " + r.Count);
        if (r != null)
        {
            if (offsetx != 0 || offsety != 0)
            {
                for (int i = 0; i < r.Count; i++)
                {
                    Point p = r[i];
                    p.x += offsetx;
                    p.y += offsety;
                    r[i] = p;
                }
            }
        }
        return r;
    }

    public override string ToString()
    {
        return "[NavGrid" + w + "x" + h+"]"+name;
    }

}

