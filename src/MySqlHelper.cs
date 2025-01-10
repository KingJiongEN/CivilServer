using MySql.Data.MySqlClient;
using Plugins.CityCommon.Server;
using System;
using System.Data;

public class MySqlHelper
{
    private static string connstr = GameServerConfig.MYSQL_CONNECT_STR;


    #region 执行查询语句，返回MySqlDataReader

    /// <summary>
    /// 执行查询语句，返回MySqlDataReader
    /// </summary>
    /// <param name="sqlString"></param>
    /// <returns></returns>
    public static MySqlDataReader ExecuteReader(string sqlString)
    {
        MySqlConnection connection = new MySqlConnection(connstr);
        MySqlCommand cmd = new MySqlCommand(sqlString, connection);
        MySqlDataReader myReader = null;
        try
        {
            connection.Open();
            myReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            return myReader;
        }
        catch (System.Data.SqlClient.SqlException e)
        {
            connection.Close();
            throw new Exception(e.Message);
        }
        finally
        {
            if (myReader == null)
            {
                cmd.Dispose();
                connection.Close();
            }
        }
    }
    #endregion

    #region 执行带参数的查询语句，返回 MySqlDataReader

    /// <summary>
    /// 执行带参数的查询语句，返回MySqlDataReader
    /// </summary>
    /// <param name="sqlString"></param>
    /// <param name="cmdParms"></param>
    /// <returns></returns>
    public static MySqlDataReader ExecuteReader(string sqlString, params MySqlParameter[] cmdParms)
    {
        MySqlConnection connection = new MySqlConnection(connstr);
        MySqlCommand cmd = new MySqlCommand();
        MySqlDataReader myReader = null;
        try
        {
            PrepareCommand(cmd, connection, null, sqlString, cmdParms);
            myReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            cmd.Parameters.Clear();
            return myReader;
        }
        catch (System.Data.SqlClient.SqlException e)
        {
            connection.Close();
            throw new Exception(e.Message);
        }
        finally
        {
            if (myReader == null)
            {
                cmd.Dispose();
                connection.Close();
            }
        }
    }
    #endregion

    #region 执行sql语句,返回执行行数

    /// <summary>
    /// 执行sql语句,返回执行行数
    /// </summary>
    /// <param name="sql"></param>
    /// <returns></returns>
    public static int ExecuteSql(string sql, MySqlConnection conn)
    {
        using (MySqlCommand cmd = new MySqlCommand(sql, conn))
        {
            try
            {
                int rows = cmd.ExecuteNonQuery();
                return rows;
            }
            catch (Exception ex)
            {
                //throw ex;
                Console.WriteLine(ex.Message);
            }
            finally
            {
                cmd.Dispose();
            }
        }
        return -1;
    }
    #endregion

    #region 执行带参数的sql语句，并返回执行行数

    /// <summary>
    /// 执行带参数的sql语句，并返回执行行数
    /// </summary>
    /// <param name="sqlString"></param>
    /// <param name="cmdParms"></param>
    /// <returns></returns>
    public static int ExecuteSql(string sqlString, params MySqlParameter[] cmdParms)
    {
        using (MySqlConnection connection = new MySqlConnection(connstr))
        {
            using (MySqlCommand cmd = new MySqlCommand())
            {
                try
                {
                    PrepareCommand(cmd, connection, null, sqlString, cmdParms);
                    int rows = cmd.ExecuteNonQuery();
                    cmd.Parameters.Clear();
                    return rows;
                }
                catch (System.Data.SqlClient.SqlException E)
                {
                    throw new Exception(E.Message);
                }
                finally
                {
                    cmd.Dispose();
                    connection.Close();
                }
            }
        }
    }
    #endregion

    #region 执行查询语句，返回DataSet

    /// <summary>
    /// 执行查询语句，返回DataSet
    /// </summary>
    /// <param name="sql"></param>
    /// <returns></returns>
    public static DataSet GetDataSet(string sql, MySqlConnection conn)
    {
        DataSet ds = new DataSet();
        MySqlDataAdapter data_adapter = null;
        try
        {
            data_adapter = new MySqlDataAdapter(sql, conn);
            data_adapter.Fill(ds);
        }
        catch (Exception ex)
        {
            //throw ex;
            Console.WriteLine(ex.Message);
        }
        finally
        {
            if (data_adapter != null)
            {
                data_adapter.Dispose();
                data_adapter = null;
            }
        }
        return ds;
    }
    #endregion

    #region 执行带参数的查询语句，返回DataSet

    /// <summary>
    /// 执行带参数的查询语句，返回DataSet
    /// </summary>
    /// <param name="sqlString"></param>
    /// <param name="cmdParms"></param>
    /// <returns></returns>
    public static DataSet GetDataSet(string sqlString, params MySqlParameter[] cmdParms)
    {
        using (MySqlConnection connection = new MySqlConnection(connstr))
        {
            MySqlCommand cmd = new MySqlCommand();
            PrepareCommand(cmd, connection, null, sqlString, cmdParms);
            using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
            {
                DataSet ds = new DataSet();
                try
                {
                    da.Fill(ds, "ds");
                    cmd.Parameters.Clear();
                }
                catch (System.Data.SqlClient.SqlException ex)
                {
                    throw new Exception(ex.Message);
                }
                finally
                {
                    cmd.Dispose();
                    connection.Close();
                }
                return ds;
            }
        }
    }
    #endregion

    #region 执行带参数的sql语句，并返回 object

    /// <summary>
    /// 执行带参数的sql语句，并返回object
    /// </summary>
    /// <param name="sqlString"></param>
    /// <param name="cmdParms"></param>
    /// <returns></returns>
    public static object GetSingle(string sqlString, params MySqlParameter[] cmdParms)
    {
        using (MySqlConnection connection = new MySqlConnection(connstr))
        {
            using (MySqlCommand cmd = new MySqlCommand())
            {
                try
                {
                    PrepareCommand(cmd, connection, null, sqlString, cmdParms);
                    object obj = cmd.ExecuteScalar();
                    cmd.Parameters.Clear();
                    if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                    {
                        return null;
                    }
                    else
                    {
                        return obj;
                    }
                }
                catch (System.Data.SqlClient.SqlException e)
                {
                    throw new Exception(e.Message);
                }
                finally
                {
                    cmd.Dispose();
                    connection.Close();
                }
            }
        }
    }

    #endregion

    /// <summary>
    /// 执行存储过程,返回数据集
    /// </summary>
    /// <param name="storedProcName">存储过程名</param>
    /// <param name="parameters">存储过程参数</param>
    /// <returns>DataSet</returns>
    public static DataSet RunProcedureForDataSet(string storedProcName, IDataParameter[] parameters)
    {
        using (MySqlConnection connection = new MySqlConnection(connstr))
        {
            DataSet dataSet = new DataSet();
            connection.Open();
            MySqlDataAdapter sqlDA = new MySqlDataAdapter();
            sqlDA.SelectCommand = BuildQueryCommand(connection, storedProcName, parameters);
            sqlDA.Fill(dataSet);
            connection.Close();
            return dataSet;
        }
    }

    /// <summary>
    /// 构建 SqlCommand 对象(用来返回一个结果集，而不是一个整数值)
    /// </summary>
    /// <param name="connection">数据库连接</param>
    /// <param name="storedProcName">存储过程名</param>
    /// <param name="parameters">存储过程参数</param>
    /// <returns>SqlCommand</returns>
    private static MySqlCommand BuildQueryCommand(MySqlConnection connection, string storedProcName,
        IDataParameter[] parameters)
    {
        MySqlCommand command = new MySqlCommand(storedProcName, connection);
        command.CommandType = CommandType.StoredProcedure;
        foreach (MySqlParameter parameter in parameters)
        {
            command.Parameters.Add(parameter);
        }
        return command;
    }

    #region 装载MySqlCommand对象

    /// <summary>
    /// 装载MySqlCommand对象
    /// </summary>
    private static void PrepareCommand(MySqlCommand cmd, MySqlConnection conn, MySqlTransaction trans, string cmdText,
        MySqlParameter[] cmdParms)
    {
        if (conn.State != ConnectionState.Open)
        {
            conn.Open();
        }
        cmd.Connection = conn;
        cmd.CommandText = cmdText;
        if (trans != null)
        {
            cmd.Transaction = trans;
        }
        cmd.CommandType = CommandType.Text; //cmdType;
        if (cmdParms != null)
        {
            foreach (MySqlParameter parm in cmdParms)
            {
                cmd.Parameters.Add(parm);
            }
        }
    }
    #endregion

}