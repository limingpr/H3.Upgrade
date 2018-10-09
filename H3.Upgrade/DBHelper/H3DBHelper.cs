using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using OThinker.H3.Instance;
using OThinker.H3.WorkItem;
using System.Configuration;
using OThinker.Data.Database;
using BpmHelper;

public class H3DBHelper
{
    public H3DBHelper()
    {
        //
        // TODO: 在此处添加构造函数逻辑
        //
    }


    #region  获取V9系统数据
    public static string _MSData = ConfigurationManager.AppSettings["H3Cloud9"].ToString();

    /// <summary>
    /// 执行SQL语句，并返回ds数据集
    /// </summary>
    /// <param name="strSql">SQL语句</param>
    /// <returns></returns>
    public static DataSet ExeV9DataSet(string strSql)
    {
        SqlConnection con = new SqlConnection(_MSData);
        SqlCommand cmd = new SqlCommand(strSql);
        cmd.Connection = con;
        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
        {
            DataSet ds = new DataSet();
            da.Fill(ds);
            cmd.Parameters.Clear();
            return ds;
        }
    }

    /// <summary>
    /// 执行SQL语句，并返回第一行第一列结果
    /// </summary>
    /// <param name="strSql">SQL语句</param>
    /// <returns></returns>
    public static object ExeV9Scalar(string strSql)
    {
        SqlCommand cmd = new SqlCommand(strSql);
        SqlConnection con = new SqlConnection(_MSData);
        cmd.Connection = con;
        con.Open();
        object retval = cmd.ExecuteScalar();


        cmd.Parameters.Clear();
        con.Close();

        return retval;
    }


    public static void ExeV9Nonquery(string strSql)
    {
        SqlConnection con = new SqlConnection(_MSData);
        SqlCommand cmd = new SqlCommand(strSql);
        cmd.Connection = con;
        con.Open();


        cmd.ExecuteNonQuery();
        cmd.Parameters.Clear();
        con.Close();

    }


    public static DataTable GetDataTable(string strSql)
    {
        SqlConnection con = new SqlConnection(_MSData);
        SqlCommand cmd = new SqlCommand(strSql);
        cmd.Connection = con;
        con.Open();
        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
        {
            DataSet ds = new DataSet();
            da.Fill(ds);
            cmd.Parameters.Clear();
            return ds.Tables[0]; ;
        }
    }


    #endregion

    #region 执行V10SQL

    public static int ExecuteNonQuery(string insert)
    {
        try
        {
            CommandFactory factory = OThinker.H3.Controllers.AppUtility.Engine.EngineConfig.CommandFactory;
            ICommand command = factory.CreateCommand();
            return command.ExecuteNonQuery(insert);
        }
        catch (Exception ex)
        {
            LogTextHelper.WriteLine(ex.Message);
            return -1;
        }

    }

    /// <summary>
    ///     执行存储过程，返回影响行数
    /// </summary>
    /// <param name="commandText"></param>
    /// <param name="parms"></param>
    /// <returns></returns>
    public static int ExecuteProcNonQuery(string commandText, List<Parameter> parms)
    {
        CommandFactory factory = OThinker.H3.Controllers.AppUtility.Engine.EngineConfig.CommandFactory;
        ICommand command = factory.CreateCommand();
        try
        {
            return command.ExecuteProcedure(commandText, parms.ToArray());
        }
        catch (Exception e)
        {
            string msg = e.ToString();
        }
        return 0;
    }

    public static int ExecuteProcNonQuery(string commandText, Parameter parm)
    {
        var list = new List<Parameter>();
        list.Add(parm);
        return ExecuteProcNonQuery(commandText, list);


    }
    public static int ExecuteProcNonQuery(string spName, List<SqlParameter> parameterValues)
    {
        CommandFactory factory = OThinker.H3.Controllers.AppUtility.Engine.EngineConfig.CommandFactory;

        string connectionString = factory.ConnectionString;
        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            conn.Open();
            SqlCommand cmd = new SqlCommand(spName, conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = 0;
            foreach (SqlParameter p in parameterValues)
            {
                //eck for derived output value with no value assigned
                if ((p.Direction == ParameterDirection.InputOutput) && (p.Value == null))
                {
                    p.Value = DBNull.Value;
                }

                cmd.Parameters.Add(p);
            }
            return cmd.ExecuteNonQuery();
        }
    }
    #endregion
    #region 获取数据库表结构

    public static List<Field> GetTableSchema(string TableName, string Version)
    {
        var dtColumns = new DataTable();
        List<Field> Fields = new List<Field>();
        var sqlColumns = string.Format(@"SELECT
	*
FROM INFORMATION_SCHEMA.COLUMNS t
WHERE t.TABLE_NAME = '{0}'
ORDER BY t.ORDINAL_POSITION", TableName);
        switch (Version)
        {
            case "V9":
                dtColumns = GetDataTable(sqlColumns);
                break;
            default:
                dtColumns = OThinker.H3.Controllers.AppUtility.Engine.Query.QueryTable(sqlColumns);
                break;
        }
        foreach (DataRow ItemCol in dtColumns.Rows)
        {
            Field field = new Field();
            field.Name =ItemCol["column_name"].ToString();
            field.Type = ItemCol["data_type"].ToString();
            if (ItemCol["DATA_TYPE"].ToString().ToLower() == "char" || ItemCol["DATA_TYPE"].ToString().ToLower() == "nvarchar")
            {
                field.Length = ItemCol["CHARACTER_MAXIMUM_LENGTH"].ToString();

            }
            Fields.Add(field);

        }

        return Fields;
    }


    public static string GetTableColumns(string TableName)
    {
        var ProCols = new List<string>();
        var Schema10 = H3DBHelper.GetTableSchema(TableName, "V10");
        var Schema9 = H3DBHelper.GetTableSchema(TableName, "V9");
        foreach (var filed in Schema10)
        {
            var tt = Schema9.Where(a => a.Name == filed.Name);
            if (Schema9.Contains(filed))
            {
                ProCols.Add("["+filed.Name+"]");

            }


        }
        return string.Join(",\n", ProCols.ToArray());
    }
    #endregion
}

public struct Field
{
    public string Name;
    public string Type;
    public string Length;
}

