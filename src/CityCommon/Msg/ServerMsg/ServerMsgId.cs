namespace Plugins.CityCommon.Msg
{
    public class ServerMsgId
    {
        public const int LLM_USER_BUY_ARTWORK_RESULT = 1004;
        public const int LLM_USER_SELL_ARTWORK_RESULT = 1005;
        //在建筑内，请求决策
        public const int LLM_RESPONSE_AGENT_DECISION = 1012;
        //兜底的行程计划
        public const int LLM_RESPONSE_AGENT_PLAN = 1013;


        public const int SERVER_MSG_INIT = 2000;
        public const int SERVER_MSG_USER_BUY_ARTWORK = 2004;
        public const int SERVER_MSG_USER_SELL_ARTWORK = 2005;
        public const int SERVER_MSG_ACK = 2007;
        public const int SERVER_MSG_AGENT_BORN = 2008;

        //建筑内的决策返回
        public const int LLM_REQUEST_AGENT_DECISION = 2012;
        //行程计划返回
        public const int LLM_REQUEST_AGENT_PLAN = 2013;
    }
}