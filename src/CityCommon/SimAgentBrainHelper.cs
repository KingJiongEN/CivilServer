using Plugins.CityCommon.Msg;
using Plugins.CityCommon.Server;
using System;
using System.Collections.Generic;

public class SimAgentBrainHelper
{
    public CityMap city_map;
    private GameLogicServer game_logic_server;

    public SimAgentBrainHelper(CityMap city_map, GameLogicServer game_logic_server)
    {
        this.city_map = city_map;
        this.game_logic_server = game_logic_server;
    }

    public void OnNewDay()
    {
        foreach (var t in city_map.sim_agent_list)
        {
            t.brain.OnNewDay();
        }
    }

    internal bool RequestPlan(SimAgent sim_agent)
    {
        if (game_logic_server != null && game_logic_server.IsConnected())
        {
            game_logic_server.SendMsgToServer(ServerRequestAgentPlan.GetMsg(sim_agent.Guid));
            return true;
        }
        return false;
    }

    internal bool RequestDecision(SimAgent sim_agent)
    {
        if (game_logic_server != null && game_logic_server.IsConnected())
        {
            game_logic_server.SendMsgToServer(ServerRequestAgentDecision.GetMsg(sim_agent.Guid, sim_agent.GetCurAreaName(), sim_agent.GetCurPlaceGuid(), sim_agent.GetAgentsIdleAround()));
            return true;
        }
        return false;
    }
}