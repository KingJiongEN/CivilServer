using System;

namespace Plugins.CityCommon.Msg
{
    /// <summary>
    /// 移动
    /// </summary>
    [Serializable]
    public class DecisionMove
    {
        public int target_place_guid;
        public int agent_guid;
        public float speed_rate;
        public string emoji_on_the_way;
        public float emoji_interval;
        public float emoji_show_duration;
        public string song;
    }
}