using System;

namespace Plugins.CityCommon.Msg
{
    /// <summary>
    /// 交易发生时，AI程序直接写数据库，以处消息存为通知数据变动之用
    /// </summary>
    [Serializable]
    public class DecisionTrade
    {
        public int from_agent_guid;
        public int to_agent_guid;
        public string artwork_id;
        public int price;

        public DecisionTrade(int from_agent_guid, int to_agent_guid, string artwork_id, int price)
        {
            this.from_agent_guid = from_agent_guid;
            this.to_agent_guid = to_agent_guid;
            this.artwork_id = artwork_id;
            this.price = price;
        }
    }
}