using Plugins.CityCommon.Data;
using System;
using System.Collections.Generic;
using System.IO;

namespace GameServer.src
{
    public class ConfigLoader
    {
        private static string dir_path = "config";
        public static Dictionary<int, AttrInfo> attr_info_dic = new Dictionary<int, AttrInfo>();
        public static List<SongInfo> song_info_list = new List<SongInfo>();
        public static Dictionary<int, AreaInfo> area_info_dic = new Dic<int, AreaInfo>();
        public static void Load()
        {
            if (File.Exists("D:\\looming_deubg_mark.txt"))
            {
                //本地调试
                dir_path = "E:\\game\\sim-webgl\\config";
            }
            LoadDataToDic(LoadTxt("attr.json"), attr_info_dic);
            ConfigManager.Ins.SetAttrInfo(attr_info_dic);

            LoadDataToList(LoadTxt("song.json"), song_info_list);
            ConfigManager.Ins.SetSongInfo(song_info_list);

            LoadDataToDic(LoadTxt("area.json"), area_info_dic);
            ConfigManager.Ins.SetAreaInfo(area_info_dic);
        }

        private static string LoadTxt(string file_name)
        {
            string file = dir_path + "/" + file_name;
            return File.ReadAllText(file);
        }

        private static void LoadDataToList<T>(string data, List<T> list)
        {
            if (data != null)
            {
                list.AddRange(Newtonsoft.Json.JsonConvert.DeserializeObject<List<T>>(data));
            }
        }

        private static void LoadDataToDic<T>(string data, Dictionary<int, T> dic) where T : IKey
        {
            if (data != null)
            {
                List<T> objs = Newtonsoft.Json.JsonConvert.DeserializeObject<List<T>>(data);
                foreach (T t in objs)
                {
                    dic[t.Key] = t;
                }
            }
        }
    }


}
