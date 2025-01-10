using System;
using System.Collections.Generic;

[Serializable]
public class MapObj
{
    public int guid;
    public MyRectInt rect;
    public Point pos;
    public bool is_block;
    public bool is_interactive;
    public string name;
    public List<Point> block_pos_list = new List<Point>();
    
    public List<MyVector2> polygon_pos_list = new List<MyVector2>();
    public List<Point> interactive_pos_list = new List<Point>();
}