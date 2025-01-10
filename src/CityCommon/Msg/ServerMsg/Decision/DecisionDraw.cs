using System;
using System.Collections.Generic;

namespace Plugins.CityCommon.Msg
{
    /// <summary>
    /// 
    /// 艺术品图片存储访问规则
    /// AI将图片，存放到 服务器/data/sim-server/web-server/public/nft目录中，图片名称使用UUID(带连字符36字符长度）
    /// 例如：
    /// artwork_id 是3315542d-b204-4b4e-a814-6fdd804931b4
    /// 图片名称为 3315542d-b204-4b4e-a814-6fdd804931b4.png
    /// 在发给游戏中的消息ServerMsgId.LLM_MSG_NFT中，
    /// 承上例：
    /// artwork_id字段为3315542d-b204-4b4e-a814-6fdd804931b4
    /// url字段为：nft/3315542d-b204-4b4e-a814-6fdd804931b4.png
    /// 最终通过web访问的URL为 域名/nft/3315542d-b204-4b4e-a814-6fdd804931b4.png
    /// 当前没有域名，通过IP访问URL为：http://localhost2:3001/nft/3315542d-b204-4b4e-a814-6fdd804931b4.png
    /// 
    /// 创作时，AI程序直接写入数据库，此处仅为通知之用
    /// </summary>
    [Serializable]
    public class DecisionDraw
    {
        public int agent_guid;
        public string url;
        public string artwork_id;
        public List<string> monologue;

        public DecisionDraw(int agent_guid, string artwork_id, string url, List<string> monologue)
        {
            this.agent_guid = agent_guid;
            this.artwork_id = artwork_id;
            this.url = url;
            this.monologue = monologue;
        }
    }
}