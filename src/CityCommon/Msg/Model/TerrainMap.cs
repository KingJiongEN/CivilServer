using System;

[Serializable]
public class WallMap : MapData<byte>
{
    public WallMap(int w, int h, byte fill) : base(w, h, fill) { }
    public WallMap(int w, int h) : base(w, h) {}
    public void Reset()
    {
        for (int i = 0; i < w; i++)
        {
            for (int j = 0; j < h; j++)
            {
                this[i, j] = 0;
            }
        }
    }
}

[Serializable]
public class ResMap : MapData<int>
{
    public ResMap(int w, int h, int fill) : base(w, h, fill) { }
    public ResMap(int w, int h) : base(w, h) { }

    internal byte[] DumpDataToByte()
    {
        byte[] r = new byte[w * h * 4];
        for (int i = 0; i < w; i++)
        {
            for (int j = 0; j < h; j++)
            {
                int n = data[i, j];
                r[(i + j * w) * 4] = (byte)((n >> 24) & 0xFF);
                r[(i + j * w) * 4 + 1] = (byte)((n >> 16) & 0xFF);
                r[(i + j * w) * 4 + 2] = (byte)((n >> 8) & 0xFF);
                r[(i + j * w) * 4 + 3] = (byte)((n >> 0) & 0xFF);
            }
        }
        return r;
    }
}

[Serializable]
public class TerrainMap : MapData<byte>
{
    public const int LAND_MIN_ID = 1;
    public const byte WATER = 22;
    public TerrainMap(int w, int h, byte fill) : base(w, h, fill) { }
    public TerrainMap(int w, int h) : base(w, h) { }

    #region 业务逻辑功能
    public bool IsCellLand(int x, int y) {
        if (!InRect(x, y))
        {
            return false;
        }
        return this[x, y] == LAND_MIN_ID;
    }

    #endregion
}

public class AreaIndexMap : MapData<int>
{
    public AreaIndexMap(int w, int h, int fill) : base(w, h, fill) { }
    public AreaIndexMap(int w, int h) : base(w, h) { }
}