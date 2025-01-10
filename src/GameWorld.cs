using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Plugins.CityCommon.Server;

namespace GameServer.src
{
    public class Game
    {
        public static Clock clock;

    }

    internal class GameWorld
    {
        public CancellationToken stoppingToken;
        public LLMServer llm_server = new LLMServerPython();
        public GameLogicServer game_logic_server = new GameLogicServer();
        public CityMap city_map = new CityMap();
        public RedisControler redis_controler = new RedisControler();
        public MySqlControler mysql_controler = new MySqlControler();

        public async Task InitWorld()
        {
            //初始寻路网格
            city_map.nav_manager.UpdateNavGrid();

            //初始化LLM
            await llm_server.Init();
            //初始化GameServer
            await game_logic_server.Init();
        }

        public bool Load()
        {
            CityState city_state_loaded = ArchiveManager.Load();
            if (city_state_loaded != null)
            {
                //初始时钟
                Game.clock = new Clock();
                Game.clock.OnNewDay = OnNewDay;
                Game.clock.OnNewMonth = OnNewMonth;
                Game.clock.OnNewYear = OnNewYear;
                
                InitCity(city_state_loaded);

                //开起时钟
                Game.clock.Recover(0);
                return true;
            }
            else
            {
                Log.e("错误：加载城市数据错误");
            }
            return false;
        }
                    
        int frameCount = 0;
        DateTime last_tick_time = DateTime.Now;
        float unscaledTime = 0;

        public void BeforeRun(CancellationToken stoppingToken)
        {
            this.stoppingToken = stoppingToken;

            //在子线程中，运行python实例
            var thread = new System.Threading.Thread(() =>
            {
                //启动python
                llm_server.StartPythonServer();
            });
            thread.Start();
        }


        public async Task RunTick()
        {
            Tick();
            //Thread.Sleep(1000 / 60);
            await Task.Delay(1000 / 60, stoppingToken);

            //时钟
            DateTime now = DateTime.Now;
            TimeSpan timeDifference = now - last_tick_time;
            last_tick_time = now;
                
            float unscaledDeltaTime = (float)(timeDifference.TotalMilliseconds / 1000f);
            unscaledTime += unscaledDeltaTime;
            Clock.Update(frameCount, unscaledDeltaTime * GameServerConfig.TIME_SCALE, unscaledDeltaTime, unscaledTime * GameServerConfig.TIME_SCALE, unscaledTime, GameServerConfig.TIME_SCALE);
            frameCount++;
           
        }

        public void AfterRun()
        {
            llm_server.OnApplicationQuit();
            game_logic_server.OnApplicationQuit();
        }

        public void InitCity(CityState city_state_loaded)
        {
            //agent的决策入口
            city_map.agent_brain_helper = new SimAgentBrainHelper(city_map, game_logic_server);

            //redis控制器
            redis_controler.Init();
            city_map.SetRedis(redis_controler);

            mysql_controler.Init();
            city_map.SetMySql(mysql_controler);
            
            //从数据库读取数据，以补全存档状态数据
            //从数据库读取所有艺术品
            MyListDic<int, string> artwork_list_dic = city_map.mysql.GetAllArtworks();

            //调试时，只开50个，正时运行时，可能到2000个
            int max_count = GameServerConfig.AGENT_MAX_COUNT;
            List<AgentModel> list = city_map.mysql.GetAgentsInCity(max_count);
            city_state_loaded.intelligent_agent_dic.Clear();
            foreach (var t in list)
            {
                IntelligentAgent il_agent =  new IntelligentAgent(t);
                city_state_loaded.intelligent_agent_dic[t.agent_id] = il_agent;
                    
                if (artwork_list_dic.TryGetValue(t.agent_id, out List<string> artwork_list))
                {
                    il_agent.artwork_list = artwork_list;
                }
                else
                {
                    il_agent.artwork_list = new List<string>();
                }
                
                //随机初始放置位置
                List<Point> interactive_pos_list = null;
                do
                {
                    interactive_pos_list = GetRandomMapObjExceptBed(city_state_loaded).interactive_pos_list;

                    if (interactive_pos_list != null && interactive_pos_list.Count > 0)
                    {
                        il_agent.postion = MyRandom.Choose(interactive_pos_list).ToMyVector2();
                        break;
                    }
                } while (interactive_pos_list == null || interactive_pos_list.Count == 0);
                
            }
            
            //初始城市
            city_map.InitCity(city_state_loaded);
            city_map.RecreateSimAgent();
            
            //补充智能体数据到2000个
            List<SimAgent> new_agent_list = new List<SimAgent>();
            for (int i = city_map.sim_agent_list.Count; i < max_count; i++)
            {
                int agent_id = i + 1;
                new_agent_list.Add(city_map.CreateSimAgentForDebug(agent_id));
            }
            SaveAgentToDBForDebug(new_agent_list);

            //初始化伪LLM
            llm_server.BindCtiyMap(city_map);
            //初始化伪GameServer
            game_logic_server.BindCtiyMap(city_map);
            
            //调试功能：将游戏中的数据，同步到数据库中
            //SaveAgentToDBForDebug();
        }

        private List<string> place_list = new List<string>();
        public MapObj GetRandomMapObjExceptBed(CityState city_state_loaded)
        {
            if (place_list.Count == 0)
            {
                place_list.AddRange(city_state_loaded.map_obj_list_dic_of_interactive.Keys);
                place_list.Remove("small bed");
                place_list.Remove("big bed");
                place_list.Remove("Entrance");
            }
            return MyRandom.Choose(city_state_loaded.map_obj_list_dic_of_interactive[MyRandom.Choose(place_list)]);
        }

        
        private void SaveAgentToDBForDebug(List<SimAgent> sim_agent_list)
        {
            foreach (var t in sim_agent_list)
            {
                mysql_controler.SaveAgentToDBForDebug(t);
            }
        }

        private void Tick()
        {
            try { Game.clock.Tick(); }catch(Exception e) { Log.e(e); }
            try { city_map.Tick(); } catch (Exception e) { Log.e(e); } 

            //Socket通讯驱动
            try { llm_server.Tick(); } catch (Exception e) { Log.e(e); }
            try { game_logic_server.Tick(); } catch (Exception e) { Log.e(e); }
        }

        private void OnNewDay()
        {
            Log.d("GameWorld: OnNewDay " + DateTime.Now);
            try { city_map.OnNewDay(); } catch (Exception e) { Log.e(e); }
            try { llm_server.OnNewDay(); } catch (Exception e) { Log.e(e); }
            try { game_logic_server.OnNewDay(); } catch (Exception e) { Log.e(e); }
        }
        private void OnNewMonth()
        {
            try{ city_map.OnNewMonth(); } catch (Exception e) { Log.e(e); }
            try{ llm_server.OnNewMonth(); } catch (Exception e) { Log.e(e); }
            try { game_logic_server.OnNewMonth(); } catch (Exception e) { Log.e(e); }
        }
        private void OnNewYear()
        {
            try{ city_map.OnNewYear(); } catch (Exception e) { Log.e(e); }
            try{ llm_server.OnNewYear(); } catch (Exception e) { Log.e(e); }
            try{ game_logic_server.OnNewYear(); } catch (Exception e) { Log.e(e); }
        }
    }
}
