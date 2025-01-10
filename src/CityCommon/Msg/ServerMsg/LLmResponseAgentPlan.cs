using System;
using System.Collections.Generic;

namespace Plugins.CityCommon.Msg
{
    [Serializable]
    public class AgentPlanItem
    {
        //什么地方
        public int target_place_guid;

        //在建筑中，如果AI拒绝响应，则保底执行某个行为(此行为是实际行为：会花钱，或影响数值）
        public int action_id;

        //保底行为执行的概率，如果随机结果为“不执行”，则简单idle等一会儿，按计划去下一个建筑
        public int action_rate;

        //移动路上的表现（根据当前的状态，去设置即可。如果当前心情值与计划时的心情差值超过20%，则以下数据失效
        public int mood;
        public float speed_rate = 1;
        public string emoji_on_the_way = null;
        public float emoji_interval = 0;
        public float emoji_show_duration = 0;
        public string song = null;


    }


    [Serializable]
    public class LLmResponseAgentPlan : LLmMsgBase
    {
        public int agent_guid;
        public List<AgentPlanItem> plan;

        public LLmResponseAgentPlan(int agent_guid, List<AgentPlanItem> plan)
        {
            this.agent_guid = agent_guid;
            this.plan = plan;
        }

        public string ToMsg()
        {
            return (MyStringBuilder.Create(ServerMsgId.LLM_RESPONSE_AGENT_PLAN) + "@" + Newtonsoft.Json.JsonConvert.SerializeObject(this)).ToStr();
        }
    }
}