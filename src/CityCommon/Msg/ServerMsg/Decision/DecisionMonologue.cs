using System;

namespace Plugins.CityCommon.Msg
{
    /// <summary>
    /// 自言自语
    /// </summary>
    [Serializable]
    public class DecisionMonologue
    {
        public string? content;
        public string? song;
        public int content_type;
        public int display_duration;

        /// <summary>
        /// 聊天
        /// </summary>
        public const int CHAT = 0;
        /// <summary>
        /// 内心读白
        /// </summary>
        public const int INNER_MONOLOGUE = 1;
        /// <summary>
        /// 状态变化
        /// </summary>
        public const int STATE_CHANGE = 2;
    }
}