using System;

namespace Plugins.CityCommon.Msg
{

    public class LLmResponseErrorCode
    {
        public const int SUCC = 0;
        public const int BUSY = 1;
        public const int TIMEOUT = 2;
    }

    /// <summary>
    /// 注意，AI决策返回时，意味着，数据已经生效，游戏只是在做用户反馈和画面表现，并不会去读写数据库。
    /// 也即，逻辑上，AI决策完，就意味着行为已结束，游戏的表现是滞后的，逻辑上可忽略的
    /// 
    /// 优先级的实现：
    /// 1.游戏发起请求后，AI程序，先初步计算，要做哪类事项
    /// 2.按事项类型，生成一个计算任务，添加到对应类别的计算队列(先入先出）,并标注超时时间，如果队列过长，则直接LLmResponseErrorCode.BUSY
    /// 3.另有扫描器，监控每个队列，超时，则销毁该任务，返回错误LLmResponseErrorCode.TIMEOUT
    /// 4.任务调度器，可配置，每个计算周期(每完成N个计算任务，为一个周期），每类任务处理的数量（即权重），来实现计算优先级的调度
    /// 
    /// 事项优先级：
    /// 用户购买智能体的艺术品 10
    /// 用户卖艺术品给智能体 9
    /// 游戏请求智能体的行程计划 8
    /// 用户与智能体聊天 7
    /// 智能体与建筑功能的交互 6
    /// 智能体与智能体交易艺术品 5
    /// 智能体与创作艺术品 5
    /// 智能体与智能体聊天 4
    /// 智能体自言自语 1
    /// </summary>
    [Serializable]
    public class LLmResponseAgentDecision : LLmMsgBase
    {
        public int agent_guid;
        public int error_code;
        public string? error_msg;

        //以下行为，AI决策后，只填其一。其它留空
        public DecisionPlaceInteraction? place_interaction;
        public DecisionDialog? dialog;
        public DecisionMonologue? monologue;
        public DecisionDraw? draw;
        public DecisionTrade? trade;
        public DecisionMove? move;
        public DecisionArtworkRecycle? artwork_recycle;

        public LLmResponseAgentDecision(int agent_guid)
        {
            this.agent_guid = agent_guid;
        }

        public string ToMsg()
        {
            return (MyStringBuilder.Create(ServerMsgId.LLM_RESPONSE_AGENT_DECISION) + "@" + Newtonsoft.Json.JsonConvert.SerializeObject(this)).ToStr();
        }

    }
}