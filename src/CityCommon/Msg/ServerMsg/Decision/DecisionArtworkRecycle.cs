using System;

namespace Plugins.CityCommon.Msg
{
    /// <summary>
    /// 回收向系统卖出系统品
    /// </summary>
    [Serializable]
    public class DecisionArtworkRecycle
    {
        public int from_agent_guid;
        public string artwork_id;
        public int price;

        public DecisionArtworkRecycle(int from_agent_guid, string artwork_id, int price)
        {
            this.from_agent_guid = from_agent_guid;
            this.artwork_id = artwork_id;
            this.price = price;
        }
    }
}