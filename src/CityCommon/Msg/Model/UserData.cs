using System;
using System.Collections.Generic;

[Serializable]
public class UserData
{
    public string user_name;
    public int gold;
    public int last_visit_day_of_year;
    public string last_area_name;
    public List<int> agent_meet_today;
    public List<string> artwork_list;

    public bool AddArtwork(string artwork_id)
    {
        if (artwork_list == null)
        {
            artwork_list = new List<string>();
        }
        if (artwork_list.Contains(artwork_id))
        {
            return false;
        }
        artwork_list.Add(artwork_id);
        return true;
    }

    public string GetTodayAreaName()
    {
        int now = DateTime.Now.DayOfYear;
        if (last_visit_day_of_year == now)
        {
            return last_area_name;
        }
        return null;
    }

    public bool IsVisitSomeAreaToday()
    {
        int now = DateTime.Now.DayOfYear;
        return last_visit_day_of_year == now && agent_meet_today != null && agent_meet_today.Count>0;
    }

    public bool RemoveArtwork(string artwork_id)
    {
        if (artwork_list == null)
        {
            artwork_list = new List<string>();
        }
        return artwork_list.Remove(artwork_id);
    }

    public bool HasArtwork(string artwork_id)
    {
        if (artwork_list != null)
        {
            return artwork_list.Contains(artwork_id);
        }
        return false;
    }

    public int GetWarworkCount()
    {
        if (artwork_list != null)
        {
            return artwork_list.Count;
        }
        return 0;
    }
}