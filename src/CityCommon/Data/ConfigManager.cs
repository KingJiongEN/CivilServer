using System.Collections.Generic;

namespace Plugins.CityCommon.Data
{
    public class ConfigManager
    {
        private static ConfigManager _ins;
        public Dictionary<int, AttrInfo> attr_info_dic = new Dictionary<int, AttrInfo>();
        public List<AttrInfo> attr_variable_list = new List<AttrInfo>();
        public List<AttrInfo> attr_visible_list = new List<AttrInfo>();
        public List<SongInfo> song_list = new List<SongInfo>();
        public Dictionary<int, AreaInfo> area_info_dic = new Dic<int, AreaInfo>();
        private ConfigManager(){}
        public static ConfigManager Ins
        {
            get
            {
                if (_ins == null)
                {
                    _ins = new ConfigManager();
                }
                return _ins;
            }
        }

        public void SetAttrInfo(Dictionary<int, AttrInfo> attr_info_dic)
        {
            this.attr_info_dic = attr_info_dic;
            foreach (var kv in this.attr_info_dic)
            {
                if (kv.Value.IsVariable)
                {
                    attr_variable_list.Add(kv.Value);
                }

                if (kv.Value.IsVisible)
                {
                    attr_visible_list.Add(kv.Value);
                }
            }
        }

        public void SetSongInfo(List<SongInfo> song_info_list)
        {
            this.song_list = song_info_list;
        }

        public void SetAreaInfo(Dictionary<int, AreaInfo> area_info_dic)
        {
            this.area_info_dic = area_info_dic;
        }
    }
}