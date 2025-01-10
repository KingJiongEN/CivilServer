using System;

namespace Plugins.CityCommon.Msg
{
    [Serializable]
    public class CityMsgDialog
    {
        public int agent_guid;
        public DecisionDialog? dialog;

        public CityMsgDialog(int agent_guid, DecisionDialog dialog)
        {
            this.dialog = dialog;
            this.agent_guid = agent_guid;
        }

        public static string GetMsg(int agent_guid, DecisionDialog dialog)
        {
            return (MyStringBuilder.Create(CityMsgId.CITY_MSG_DIALOG) + "@" + Newtonsoft.Json.JsonConvert.SerializeObject(new CityMsgDialog(agent_guid, dialog))).ToStr();
        }
    }
}