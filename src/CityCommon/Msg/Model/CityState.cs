using System;
using System.Collections.Generic;


[Serializable]
public class CityState
{
    public int map_w = 472;
    public int map_h = 447;
    
    public TerrainMap terrain_data;
    
    public MyListDic<string, MapObj> map_obj_list_dic_of_not_interactive  = new MyListDic<string, MapObj>();
    public MyListDic<string, MapObj> map_obj_list_dic_of_interactive = new MyListDic<string, MapObj>();
    public Dictionary<string, MyRectInt> area_dic = new Dictionary<string, MyRectInt>();
    public Dictionary<int, IntelligentAgent> intelligent_agent_dic = new Dictionary<int, IntelligentAgent>();
    
    [NonSerialized] public List<MapObj> all_map_obj_list_of_bed = new List<MapObj>();
    [NonSerialized] public Dictionary<int, MapObj> map_obj_by_guid_dic = new Dic<int, MapObj>();
    [NonSerialized] public MyListDic<Point, MapObj>? interactive_pos_mapobj_index = null;
    
    public CityState()
    {
        terrain_data = new TerrainMap(map_w, map_h, TerrainMap.LAND_MIN_ID);
    }
    
    [NonSerialized]
    public List<MapObj> all_map_obj_list_of_interactive = new List<MapObj>();
    public MapObj GetRandomMapObj()
    {
        if (all_map_obj_list_of_interactive.Count == 0)
        {
            foreach (var kv in map_obj_list_dic_of_interactive)
            {
                all_map_obj_list_of_interactive.AddRange(kv.Value);
            }
        }
        return MyRandom.Choose(all_map_obj_list_of_interactive);
    }

    public MapObj GetMapObjByGuid(int guid)
    {
        if (map_obj_by_guid_dic == null)
        {
            map_obj_by_guid_dic = new Dic<int, MapObj>();
        }

        if (map_obj_by_guid_dic.Count == 0)
        {
            //延迟生成索引
            foreach (var kv in map_obj_list_dic_of_interactive)
            {
                foreach (var t in kv.Value)
                {
                    map_obj_by_guid_dic[t.guid] = t;
                }
            }
        }

        if (map_obj_by_guid_dic.TryGetValue(guid, out MapObj r))
        {
            return r;
        }
        return null;
    }

    public MapObj GetMapObjByPoint(Point p)
    {
        if(interactive_pos_mapobj_index == null)
        {
            interactive_pos_mapobj_index = new MyListDic<Point, MapObj>();
            //延迟生成索引
            foreach (var kv in map_obj_list_dic_of_interactive)
            {
                foreach (var t in kv.Value)
                {
                    foreach(var interactive_pos in t.interactive_pos_list)
                    {
                        interactive_pos_mapobj_index.Add(interactive_pos, t);
                    }
                }
            }
        }

        if(interactive_pos_mapobj_index.TryGetValue(p, out List<MapObj> list))
        {
            if(list != null)
            {
                if (list.Count > 0)
                {
                    return list[0];
                }
                else
                {
                    Log.w("注意：该位置，与两个设施都可以交互，需调整一下地图");
                }
            }
        }
        return null;
    }
    
    public MapObj GetRandomMapObjOfBed()
    {
        if (all_map_obj_list_of_bed.Count == 0)
        {
            if (map_obj_list_dic_of_interactive.TryGetValue("small bed", out List<MapObj> list))
            {
                all_map_obj_list_of_bed.AddRange(list);
            }
            if (map_obj_list_dic_of_interactive.TryGetValue("big bed", out  list))
            {
                all_map_obj_list_of_bed.AddRange(list);
            }
        }
        return MyRandom.Choose(all_map_obj_list_of_bed);
    }
    
    public bool  GetAreaRect(Point p, out MyRectInt r)
    {
        foreach (var kv in area_dic)
        {
            if (kv.Value.Contains(p.x, p.y))
            {
                r = kv.Value;
                return true;
            }
        }
        r = new MyRectInt();
        return false;
    }
    
    [NonSerialized] public List<string> area_list;
    [NonSerialized] public AreaIndexMap area_index_map;
    internal bool IsInDoor(Point p)
    {
        CheckAndInitIndoorMap();

        if (area_index_map.InRect(p.x, p.y))
        {
            return area_index_map[p.x, p.y] != -1;
        }
        return false;
    }

    public int GetAreaIndex(Point p)
    {
        CheckAndInitIndoorMap();

        if (area_index_map.InRect(p.x, p.y))
        {
            return area_index_map[p.x, p.y];
        }
        return -1;
    }

    private void CheckAndInitIndoorMap()
    {
        if (area_index_map == null)
        {
            area_list = new List<string>(area_dic.Keys);
            area_index_map = new AreaIndexMap(map_w, map_h, -1);
            for (int i = 0; i < area_list.Count; i++)
            {
                MyRectInt rect = area_dic[area_list[i]];
                for (int x = 0; x < rect.w; x++)
                {
                    for (int y = 0; y < rect.h; y++)
                    {
                        area_index_map[x + rect.x, y + rect.y] = i;
                    }
                }
            }
        }
    }

    internal string GetCurAreaName(Point p)
    {
        CheckAndInitIndoorMap();

        if (area_index_map.InRect(p.x, p.y))
        {
            int area_index = area_index_map[p.x, p.y];
            if (area_index >= 0)
            {
                return area_list[area_index];
            }
        }
        return null;
    }

    internal bool IsCellLand(Point t)
    {
        return terrain_data[t.x, t.y] == TerrainMap.LAND_MIN_ID;
    }
}
