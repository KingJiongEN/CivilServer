using System;

namespace Pomelo.DotNetClient
{
    public class HeartBeatService
    {
        int interval;
        DateTime lastTime;
        ProtocolHelper protocol_helper;

        public HeartBeatService(int interval, ProtocolHelper protocol_helper)
        {
            this.interval = Math.Max((interval-1) * 1000, 1000);
            this.protocol_helper = protocol_helper;
        }

        internal void resetTimeout()
        {
            lastTime = DateTime.Now;
        }
        
        public void sendHeartBeat()
        {
            TimeSpan span = DateTime.Now - lastTime;
            int timeout = (int)span.TotalMilliseconds;

            //check timeout
            if (timeout > interval * 2)
            {
                // protocol.getPomeloClient().disconnect();
                // return;
            }
            
            //Debug.Log("心跳 timeout"+ timeout);
            //Send heart beat
            protocol_helper.send(PackageType.PKG_HEARTBEAT);
            lastTime = DateTime.Now;
        }

        public void start()
        {
            if (interval < 1000)
            {       
                interval = 1000;
            }

            //Set timeout
            resetTimeout();

            //初始化定时器后，立即执行一次
            sendHeartBeat();
        }

        public void Tick()
        {
            TimeSpan span = DateTime.Now - lastTime;
            int timeout = (int)span.TotalMilliseconds;
            if (timeout > interval)
            {
                sendHeartBeat();
            }
        }
    }
}