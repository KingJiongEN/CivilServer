
//寻路
using System;
using System.Collections.Generic;

public class NavManagerOfCity
{
    private CityMap area_map;
    public NavManagerOfCity(CityMap area_map) { this.area_map = area_map; }

    private NavServiceOfCity service;

    //对外提供的刷新接口
    public void UpdateNavGrid()
    {
        if (service == null)
        {
            service = new NavServiceOfCity();
            //创建导航网格，是同步方法
            service.CreateNavGrid(area_map, false);
        }
        else {
            //刷新网格，是异步操作
            service.UpdateNavGrid(area_map, false);
        }
    }

    //对外提供的寻路接口
    public List<Point> FindPath(Point start, Point end, int nav_grid_type=0)
    {
        if (service == null) { return new List<Point>(); }
        start = SimAgentStateWalk.TryAdjustPointForNav(area_map, start);
        end = SimAgentStateWalk.TryAdjustPointForNav(area_map, end);
        return service.FindPath(start, end, nav_grid_type);
    }

    public bool IsWalkable(int x, int y)
    {
        if (service != null)
        {
            return service.IsWalkableOnWholeMap(x, y);
        }
        return false;
    }
}

