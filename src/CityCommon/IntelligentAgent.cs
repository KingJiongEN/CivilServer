using System;
using System.Collections.Generic;
using Plugins.CityCommon.Data;
using Plugins.CityCommon.Server;

[Serializable]
public class IntelligentAgent
{
    public int id;
    public string body_name;
    public string agent_name;
    public MyVector2 postion;
    public float speed;
    public Dictionary<int, int> attr = new Dictionary<int, int>();
    public List<string> artwork_list;

    public IntelligentAgent()
    {
    }

    public IntelligentAgent(AgentModel model)
    {
        this.id = model.agent_id;
        this.body_name = model.avatar;
        this.agent_name = model.agent_name;
        this.speed = GameServerConfig.AGENT_BASE_SPEED + MyRandom.NextFloat(0.1f);
        attr[AttrInfo.ENERGY] = model.energy;
        attr[AttrInfo.GOLD] = model.gold;
        attr[AttrInfo.PRIMITIVE] = model.primitive;
        attr[AttrInfo.CHARACTER] = model.character;
        attr[AttrInfo.CREATIVITY] = model.creativity;
        attr[AttrInfo.CHARM] = model.charm;
        attr[AttrInfo.ART_STYLE] = model.art_style;
        attr[AttrInfo.REBELLIOUSNESS] = model.rebelliousness;
        attr[AttrInfo.MOOD] = model.mood;
        attr[AttrInfo.HEALTH] = model.health;
    }

    public IntelligentAgent(int id, string body_name, float speed)
    {
        this.id = id;
        this.body_name = body_name;
        this.speed = speed;
    }

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

    public bool RemoveArtwork(string artwork_id)
    {
        if (artwork_list != null && artwork_list.Count > 0)
        {
            for (int i = artwork_list.Count - 1; i >= 0; i--)
            {
                if (artwork_list[i].Equals(artwork_id))
                {
                    artwork_list.RemoveAt(i);
                    return true;
                }
            }
        }
        return false;
    }
    public bool HasArtwork(string artwork_id)
    {
        if (artwork_list != null && artwork_list.Count > 0)
        {
            for (int i = artwork_list.Count - 1; i >= 0; i--)
            {
                if (artwork_list[i].Equals(artwork_id))
                {
                    return true;
                }
            }
        }
        return false;
    }
}