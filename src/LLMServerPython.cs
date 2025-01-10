using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Plugins.CityCommon.Data;
using Plugins.CityCommon.Msg;
using Plugins.CityCommon.Server;
using Python.Runtime;

public class LLMServerPython : LLMServer
{
    private bool is_python_exit = false;
    //private bool is_python_init = false;
    public const string py_entry_name = "main";
    private static string python_config_file_name = "config_python.json";
    public override void StartPythonServer(string python_script_path = null)
    {
        //读取配置
        if(File.Exists(python_script_path))
        {
            //将路径配置，提取到配置文件中
        }


        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            string pathToVirtualEnv = @"C:\Python39";
            Runtime.PythonDLL = @"C:\Python39\python39.dll";
            Environment.SetEnvironmentVariable("PATH", pathToVirtualEnv, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("PYTHONHOME", pathToVirtualEnv, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("PYTHONPATH", $"{pathToVirtualEnv}\\Lib\\site-packages;{pathToVirtualEnv}\\Lib;{pathToVirtualEnv}\\DLLs", EnvironmentVariableTarget.Process);
            PythonEngine.PythonHome = pathToVirtualEnv;
            PythonEngine.PythonPath = "E:\\game\\Civils;" + PythonEngine.PythonPath + ";" + Environment.GetEnvironmentVariable("PYTHONPATH", EnvironmentVariableTarget.Process);
        }
        else if(RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            string pathToVirtualEnv = "/opt/homebrew/anaconda3/envs/Civil39";
            int pyversion = 9;
            // for macOS
            Runtime.PythonDLL = pathToVirtualEnv + $"/lib/libpython3.{pyversion}.dylib";

            // Set the environment variables for the process
            Environment.SetEnvironmentVariable("PATH", pathToVirtualEnv + "/bin" + ":" + Environment.GetEnvironmentVariable("PATH"), EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("PYTHONHOME", pathToVirtualEnv, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("PYTHONPATH",
        $"/opt/homebrew/anaconda3/envs/Civil39/lib/python3.9/lib-dynload:{pathToVirtualEnv}/lib/python3.{pyversion}/site-packages:{pathToVirtualEnv}/lib/python3.{pyversion}:/Users/admin/Documents/GitHub.nosynchr/Civils", // Adjust the Python version accordingly
        EnvironmentVariableTarget.Process);

            PythonEngine.PythonHome = pathToVirtualEnv;
            PythonEngine.PythonPath = "/opt/homebrew/anaconda3/envs/Civil39/lib/python3.9/lib-dynload" + ";" + "/Users/admin/Documents/GitHub.nosynchr/Civils" + ";" + PythonEngine.PythonPath + ";" + Environment.GetEnvironmentVariable("PYTHONPATH", EnvironmentVariableTarget.Process);
        }
        else
        {
            string pathToVirtualEnv = "/usr/local/python3";
            Runtime.PythonDLL = pathToVirtualEnv + $"/lib/libpython3.9.so";
            // Set the environment variables for the process
            Environment.SetEnvironmentVariable("PATH", pathToVirtualEnv + "/bin", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("PYTHONHOME", pathToVirtualEnv, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("PYTHONPATH",$"/usr/lib/python3.9/site-packages:/home/ec2-user/.local/lib/python3.9/site-packages:/usr/lib64/python3.9/site-packages:{pathToVirtualEnv}/lib/python3.9:{pathToVirtualEnv}/lib/python3.9/lib-dynload:/data/Civils", // Adjust the Python version accordingly
            EnvironmentVariableTarget.Process);

            PythonEngine.PythonHome = pathToVirtualEnv;
            PythonEngine.PythonPath = "/data/Civils" + ":" + PythonEngine.PythonPath + ":" + Environment.GetEnvironmentVariable("PYTHONPATH", EnvironmentVariableTarget.Process);
        }


        PythonEngine.Initialize();
        PythonEngine.BeginAllowThreads();//多线程初始化

        //dynamic os = Py.Import("os");
        Console.WriteLine("StartPythonServer============================");
        //Console.WriteLine(os.getcwd());

        using (Py.GIL())
        {
            dynamic py = Py.Import(py_entry_name); // 加载当前主模块
            //is_python_init = true;
            py.StartRun();
            is_python_exit = true;
            //退出
            PythonEngine.Shutdown();
        }
    }

    public override void OnTick()
    {
        base.OnTick();
        TryGetMsgToSend();
    }

    dynamic py_entry = null;
    private void TryGetMsgToSend()
    {
        //if (!is_python_init) { return; }
        if (is_python_exit) { return; }
        using (Py.GIL())
        {
            if (py_entry == null) {
                py_entry = Py.Import(py_entry_name); // 加载当前主模块
            }
            string msg = py_entry.GetMsgToSend();
            if(!string.IsNullOrEmpty(msg))
            {
                if (msg.StartsWith("#"))
                {
                    Log.d(msg);
                }
                else
                {
                    Log.d("[MSG FROM LLM]=====" + msg);
                    SendMsgToServer(msg);
                }
            }
        }
    }
   
    public override void DealMessage(string channel, string from, int msg_id, string msg)
    {
        //if (!is_python_init) { return; }
        if (is_python_exit) { return; }

        if (GetUserName().Equals(from))
        {
            //自己发出去的消息，不必处理
            return;
        }

        using (Py.GIL())
        {
            if (py_entry == null)
            {
                py_entry = Py.Import(py_entry_name); // 加载当前主模块
            }
            py_entry.DealMessage(from, msg_id, msg);
        }
    }
    
}