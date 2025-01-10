using System;
using System.Collections.Generic;

namespace Plugins.CityCommon.Msg
{
    /// <summary>
    /// 与建筑交互行为
    /// 行为发生时，AI程序直接写数据库，以处仅为通知之用
    /// </summary>
    [Serializable]
    public class DecisionPlaceInteraction
    {
        //交互设施
        public int target_place_guid;
        //交互后，属性变化
        public List<AttrChange>? attr_chagne;
        //交互后，读白内容
        public DecisionMonologue? monologue;
        //是否睡着了（进入挖矿状态）
        public bool is_asleep;
    }
}