using System;

public class MapData<T>
{
    public T[,] data;
    public int w;
    public int h;
    public MapData(MapData<T> map) : this(map.w, map.h)
    {
        Array.Copy(map.data, this.data, map.w*map.h);
    }

    public MapData(int w,int h, T fill)
    {
        data = new T[w,h];
        this.w = w;
        this.h = h;
        for(int i = 0; i < w; i++)
        {
            for (int j = 0; j < h; j++)
            {
                data[i, j] = fill;
            }
        }
    }

    public MapData(int w, int h)
    {
        data = new T[w, h];
        this.w = w;
        this.h = h;
        for (int i = 0; i < w; i++)
        {
            for (int j = 0; j < h; j++)
            {
                data[i, j] = default(T);
            }
        }
    }

    public MapData(int w, int h, T[,] data)
    {
        this.data = data;
        this.w = w;
        this.h = h;
    }

    public T this[int x,int y]
    {
        get
        {
            return data[x, y];
        }
        set
        {
            data[x, y] = value;
        }
    }

    public int Length { get { return w * h; } }

    public bool InRect(int x, int y)
    {
        return x >= 0 && y >= 0 && x < w && y < h;
    }

    internal void Set(int x, int y, T terrain)
    {
        if (InRect(x, y))
        {
            this[x, y] = terrain;
        }
    }

    internal T[] DumpData()
    {
        T[] r = new T[w * h];
        for(int i = 0; i < w; i++)
        {
            for(int j = 0; j < h; j++)
            {
                r[i + j * w] = data[i, j];
            }
        }
        return r;
    }
}
