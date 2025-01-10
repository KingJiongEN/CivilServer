
//寻路
using System;
using System.Collections.Generic;

//TODO 优化：在刷新导航网格时，复用上一次的内存，不重新申请内存
public class NavServiceOfCity
{
    //整个地图
    private NavGrid grid_whole_map;


    //一次刷新所有个网格
    public void CreateNavGrid(CityMap area_map, bool is_async = true)
    {
        UpdateNavGrid(area_map);
    }

    //刷新导航路径
    public void UpdateNavGrid(CityMap area_map, bool is_async = true)
    {
        UpdateNavGrid(area_map);
    }
    
    
    public List<Point> FindPath(Point start, Point end, int nav_grid_type)
    {
        return grid_whole_map.FindPath(start, end);
    }

    public bool IsWalkableOnWholeMap(int x,int y)
    {
        if (grid_whole_map != null)
        {
            return grid_whole_map.IsWalkable(x, y);
        }
        return false;
    }
    private void PrepareWholeMapData(CityMap area_map, out MapData<byte> map_data)
    {
        map_data = new MapData<byte>(area_map.city_state.map_w, area_map.city_state.map_h, (byte[,])area_map.city_state.terrain_data.data.Clone());
    } 
    private void UpdateNavGrid(CityMap area_map)
    {
        PrepareWholeMapData(area_map, out MapData<byte> map_data_of_whole_map);
        grid_whole_map = new NavGrid(NavGrid.NAVGRID_TYPE_WHOLE_MAP, "gird_whole_map", map_data_of_whole_map);
    }

    private void UpdateNavGridCallback(NavGrid nav_grid_of_road, NavGrid nav_grid_of_downtown, NavGrid nav_grid_wholemap, int op_guid, Action<int> cb)
    {
        this.grid_whole_map = nav_grid_wholemap;
        cb?.Invoke(op_guid);
    }
}

