using System;
using System.IO;
using System.Text;

public class ArchiveManager
{
    private static string summary_file_name = "city_status.json";
    private static string terrain_base_file_name = "terrain_base.json";
    private static string _root_dir = null;
    public static void SetRootDir(string s)
    {
        _root_dir = s;
    }

    public static string dir_path
    {
        get
        {
            if (_root_dir == null)
            {
                _root_dir = "./";
                if (!Directory.Exists(_root_dir))
                {
                    Directory.CreateDirectory(_root_dir);
                }
            }
            return _root_dir;
        }
    }
    
    public static void Save(CityState t)
    {
        // string archive_summary_file = dir_path + "/" + summary_file_name;
        // Log.d("保存存档" + archive_summary_file);
        // File.Delete(archive_summary_file);
        // WriteAllText(archive_summary_file, Newtonsoft.Json.JsonConvert.SerializeObject(t));
    }

    #region 存档文件的工具方法
    //全部使用异步方法
    public static void WriteAllText(string path, string str)
    {
        File.WriteAllTextAsync(path, str);
    }
    public static void WriteAllText(string path, string str, Encoding encode)
    {
        File.WriteAllTextAsync(path, str, encode);
    }
    public static void WriteAllBytes(string path, byte[] bytes)
    {
        if (bytes == null){return;}
        File.WriteAllBytesAsync(path, bytes);
    }
    #endregion

    public static CityState Load()
    {
        if (File.Exists("D:\\looming_deubg_mark.txt"))
        {
            _root_dir = "E:\\game\\GameServer";
        }
        string file  = dir_path + "/" + terrain_base_file_name;

        if (File.Exists(file))
        {
            try
            {
                string str = File.ReadAllText(file);
                if (!string.IsNullOrEmpty(str))
                {
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<CityState>(str);
                }
            }
            catch (Exception e)
            {
                Log.e(e);
            }
        }
        return null;
    }
}