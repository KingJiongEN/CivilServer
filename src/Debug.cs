using System;

namespace GameServer.src
{
    public class Debug
    {
        internal static void Log(string v)
        {
            Console.WriteLine(v);
        }

        internal static void LogError(string v)
        {
            Console.WriteLine(v);
        }

        internal static void LogError(Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

}
