using MySql.Data.MySqlClient;
using MySqlX.XDevAPI;
using Plugins.CityCommon.Data;
using Plugins.CityCommon.Server;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;

public class AgentEnterCityRequest
{
    public string sql;
    public MySqlControler mysql;

    public AgentEnterCityRequest(MySqlControler mysql, string sql)
    {
        this.sql = sql;
        this.mysql = mysql;
    }
}

public class SaveAgentOnDebugRequest
{
    public string sql;
    public MySqlControler mysql;

    public SaveAgentOnDebugRequest(MySqlControler mysql, string sql)
    {
        this.sql = sql;
        this.mysql = mysql;
    }
}

public class MySqlControler : IMySql
{
    private MySqlConnection _conn = null;
    private object lock_obj_of_conn = new object();

    public void Init()
    {
        _conn = new MySqlConnection(GameServerConfig.MYSQL_CONNECT_STR);
        _conn.Open();
    }
    private MySqlConnection GetConn()
    {
        return _conn;
    }

    public void AgentEnterCity(SimAgent agent)
    {
        ThreadPool.QueueUserWorkItem(DoGetAgentsToEnterCity, this);
        MyStringBuilder sb = MyStringBuilder.Create("UPDATE agent SET ");
        sb.Append(" is_enter_city=1");
        sb.Append(", enter_city_time=NOW()");
        sb.Append(", health="); sb.Append(agent.il_agent.attr[AttrInfo.HEALTH]);
        sb.Append(", energy="); sb.Append(agent.il_agent.attr[AttrInfo.ENERGY]);
        sb.Append(" where id="); sb.Append(agent.Guid);

        string sql = sb.ToStr();
        ThreadPool.QueueUserWorkItem(DoAgentEnterCity, new AgentEnterCityRequest(this, sql));
    }

    public void SaveAgentToDBForDebug(SimAgent agent)
    {
        MyStringBuilder sb = MyStringBuilder.Create("INSERT INTO agent SET");
        sb.Append(" id=");sb.Append(agent.Guid);
        sb.Append(", avatar='");sb.Append(agent.il_agent.body_name);sb.Append("'");
        sb.Append(", primitive=");sb.Append(agent.GetAttr(AttrInfo.PRIMITIVE));
        sb.Append(", `character`=");sb.Append(agent.GetAttr(AttrInfo.CHARACTER));
        sb.Append(", creativity=");sb.Append(agent.GetAttr(AttrInfo.CREATIVITY));
        sb.Append(", charm=");sb.Append(agent.GetAttr(AttrInfo.CHARM));
        sb.Append(", art_style=");sb.Append(agent.GetAttr(AttrInfo.ART_STYLE));
        sb.Append(", rebelliousness=");sb.Append(agent.GetAttr(AttrInfo.REBELLIOUSNESS));
        sb.Append(", energy=");sb.Append(agent.GetAttr(AttrInfo.ENERGY));
        sb.Append(", gold=");sb.Append(agent.GetAttr(AttrInfo.GOLD));
        sb.Append(", health=");sb.Append(agent.GetAttr(AttrInfo.HEALTH));
        sb.Append(", mood=");sb.Append(agent.GetAttr(AttrInfo.MOOD));
        sb.Append(", user_id=");sb.Append(agent.Guid);
        sb.Append(", is_unpack=TRUE");
        sb.Append(", is_pledged = TRUE");
        sb.Append(", is_enter_city=TRUE");
        sb.Append(", is_ai_handled=TRUE");
        sb.Append(", is_sleeping=FALSE");
        sb.Append(", is_dead=FALSE");
        sb.Append(", create_time=NOW()");
        sb.Append(", unpack_time=NOW()");
        sb.Append(", pledge_time=NOW()");
        sb.Append(", ai_handle_time=NOW()");
        sb.Append(", enter_city_time=NOW()");
        sb.Append(", modify_time=NOW()");
        sb.Append(", parent_agent_guid1=0");
        sb.Append(", parent_agent_guid2=0");
        sb.Append(";");
        
        ThreadPool.QueueUserWorkItem(DoSaveAgentToDBForDebug, new SaveAgentOnDebugRequest(this, sb.ToStr()));
    }

    private static void DoAgentEnterCity(object? state)
    {
        AgentEnterCityRequest t = state as AgentEnterCityRequest;
        lock (t.mysql.lock_obj_of_conn)
        {
            if (MySqlHelper.ExecuteSql(t.sql, t.mysql.GetConn()) == 1)
            {
                Log.d("更新成功" + t.sql);
            }
            else
            {
                Log.d("更新失败" + t.sql);
            }
        }
    }

    private List<AgentModel> agents_to_enter_city = new List<AgentModel>();
    public List<AgentModel> GetAgentsToEnterCity()
    {
        lock (agents_to_enter_city)
        {
            if (agents_to_enter_city.Count > 0)
            {
                List<AgentModel> r = new List<AgentModel>();
                r.AddRange(agents_to_enter_city);
                agents_to_enter_city.Clear();
                return r;
            }
            else
            {
                GetAgentsToEnterCityAsync();
                return null;
            }
        }
    }

    private void OnRequestAgentToEnterCityFinish(List<AgentModel> list)
    {
        if (list != null)
        {
            lock(agents_to_enter_city)
            {
                agents_to_enter_city.Clear();
                agents_to_enter_city.AddRange(list);
            }
        }
        is_requesting = false;
    }

    private bool is_requesting = false;
    public void GetAgentsToEnterCityAsync()
    {
        if (is_requesting) { return; }
        if (agents_to_enter_city.Count > 0) { return; }

        is_requesting = true;

        ThreadPool.QueueUserWorkItem(DoGetAgentsToEnterCity, this);

    }

    private static void DoSaveAgentToDBForDebug(object? state)
    {
        SaveAgentOnDebugRequest request = state as SaveAgentOnDebugRequest;
        lock (request.mysql.lock_obj_of_conn)
        {
            if (MySqlHelper.ExecuteSql(request.sql, request.mysql.GetConn()) == 1)
            {
                Log.d("插入数据成功" + request.sql);
            }
            else
            {
                Log.d("插入数据失败" + request.sql);
            }
        }
    }

    private static void DoGetAgentsToEnterCity(object? state)
    {
        MySqlControler mysql = state as MySqlControler;
        List<AgentModel> list = null;
        lock (mysql.lock_obj_of_conn)
        {
            string sql = "SELECT id,avatar,agent_name,primitive,`character`,creativity,charm,art_style,rebelliousness,energy,gold,health,user_id,is_unpack,is_enter_city,mood,is_pledged,is_sleeping FROM agent where is_unpack=TRUE and is_pledged=TRUE and is_enter_city=FALSE and is_ai_handled=FALSE";
            DataSet dataSet = MySqlHelper.GetDataSet(sql, mysql.GetConn());
            if (dataSet.Tables.Count > 0)
            {
                DataTable dt = dataSet.Tables[0];
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        AgentModel model = new AgentModel();
                        if (list == null)
                        {
                            list = new List<AgentModel>();
                        }
                        list.Add(model);

                        model.agent_id = (int)row["id"];
                        model.avatar = (string)row["avatar"];
                        model.agent_name = (string)row["agent_name"];
                        model.primitive = (int)row["primitive"];
                        model.character = (int)row["character"];
                        model.creativity = (int)row["creativity"];
                        model.charm = (int)row["charm"];
                        model.art_style = (int)row["art_style"];
                        model.rebelliousness = (int)row["rebelliousness"];
                        model.energy = (int)row["energy"];
                        model.gold = (int)row["gold"];
                        model.health = (int)row["health"];
                        model.user_id = (string)row["user_id"];
                        model.is_unpack = (byte)row["is_unpack"] == 1;
                        model.is_enter_city = (byte)row["is_enter_city"] == 1;
                        model.mood = (int)row["mood"];
                        model.is_pledged = (bool)row["is_pledged"];
                        model.is_sleeping = (bool)row["is_sleeping"];
                    }
                }
            }
        }
        mysql.OnRequestAgentToEnterCityFinish(list);
    }
    
    public List<AgentModel> GetAgentsInCity(int count_limit)
    {
        MySqlControler mysql = this;
        List<AgentModel> list = null;
        lock (mysql.lock_obj_of_conn)
        {
            string sql = "SELECT id,avatar,agent_name,primitive,`character`,creativity,charm,art_style,rebelliousness,energy,gold,health,user_id,is_unpack,is_enter_city,mood,is_pledged,is_sleeping FROM agent where is_unpack=TRUE and is_pledged=TRUE and is_enter_city=TRUE and is_ai_handled=TRUE LIMIT " + count_limit + ";";
            DataSet dataSet = MySqlHelper.GetDataSet(sql, mysql.GetConn());
            if (dataSet.Tables.Count > 0)
            {
                DataTable dt = dataSet.Tables[0];
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        AgentModel model = new AgentModel();
                        if (list == null)
                        {
                            list = new List<AgentModel>();
                        }
                        list.Add(model);

                        model.agent_id = (int)row["id"];
                        model.avatar = (string)row["avatar"];
                        model.agent_name = (string)row["agent_name"];
                        model.primitive = (int)row["primitive"];
                        model.character = (int)row["character"];
                        model.creativity = (int)row["creativity"];
                        model.charm = (int)row["charm"];
                        model.art_style = (int)row["art_style"];
                        model.rebelliousness = (int)row["rebelliousness"];
                        model.energy = (int)row["energy"];
                        model.gold = (int)row["gold"];
                        model.health = (int)row["health"];
                        model.user_id = (string)row["user_id"];
                        model.is_unpack = (byte)row["is_unpack"] == 1;
                        model.is_enter_city = (byte)row["is_enter_city"] == 1;
                        model.mood = (int)row["mood"];
                        model.is_pledged = (bool)row["is_pledged"];
                        model.is_sleeping = (bool)row["is_sleeping"];
                    }
                }
            }
        }
        return list;
    }

    public MyListDic<int, string> GetAllArtworks()
    {
        MyListDic<int, string> r = new MyListDic<int, string>();
        
        MySqlControler mysql = this;
        lock (mysql.lock_obj_of_conn)
        {
            string sql = "SELECT id, owner_id FROM artwork";
            DataSet dataSet = MySqlHelper.GetDataSet(sql, mysql.GetConn());
            if (dataSet.Tables.Count > 0)
            {
                DataTable dt = dataSet.Tables[0];
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        int agent_id = (int)row["owner_id"];
                        string artwork_id = (string)row["id"];
                        r.Add(agent_id, artwork_id);
                    }
                }
            }
        }
        return r;
    }
}
