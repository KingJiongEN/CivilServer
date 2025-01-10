using System;
using System.Collections.Generic;

namespace Plugins.CityCommon.Msg
{
    [Serializable]
    public class CityMsgWalkToDo
    {
        public int target_place_guid;
        public int agent_guid;
        public List<Point> path;
        public float speed_rate;
        public string emoji_on_the_way;
        public float emoji_interval;
        public float emoji_show_duration;
        public string song;
        
        public CityMsgWalkToDo(int target_place_guid, int agent_guid, List<Point> path, float speed_rate, string emoji_on_the_way, float emoji_interval, float emoji_show_duration, string song)
        {
            this.target_place_guid = target_place_guid;
            this.agent_guid = agent_guid;
            this.path = path;
            this.speed_rate = speed_rate;
            this.emoji_on_the_way = emoji_on_the_way;
            this.emoji_interval = emoji_interval;
            this.emoji_show_duration = emoji_show_duration;
            this.song = song;
        }
        

        public static string GetMsg(int agent_guid, int target_place_guid, List<Point> path, float speed_rate, string emoji_on_the_way, float emoji_interval, float emoji_show_duration, string song)
        {
            return (MyStringBuilder.Create(CityMsgId.CITY_MSG_WALK_TO_DO) + "@" + Newtonsoft.Json.JsonConvert.SerializeObject(new CityMsgWalkToDo(target_place_guid, agent_guid, path, speed_rate, emoji_on_the_way, emoji_interval, emoji_show_duration, song))).ToStr();
        }
    }
}