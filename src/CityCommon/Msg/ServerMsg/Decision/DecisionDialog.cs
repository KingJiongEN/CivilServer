using System;
using System.Collections.Generic;

namespace Plugins.CityCommon.Msg
{
    /// <summary>
    /// 对话
    /// </summary>
    [Serializable]
    public class DecisionDialog
    {
        public int agent_guid;
        public int agent_guid_object;
        public List<Words>? words_list;
    }
}