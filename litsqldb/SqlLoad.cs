using litsdk;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace litsqldb
{
    internal class SqlLoad
    {
        public static void Execute(string mksql, IFreeSql _fsql, ActivityContext context, RunSqlActivity dBActivity)
        {
            string msg = "";
            try
            {
                if (dBActivity.SaveResult2Var)
                {
                    DataSet ds = new DataSet();

                    try
                    {
                        ds = _fsql.Ado.ExecuteDataSet(mksql);
                    }
                    catch
                    {
                        System.Threading.Thread.Sleep(1000);
                        ds = _fsql.Ado.ExecuteDataSet(mksql);
                    }

                    if (context.ContainsStr(dBActivity.SaveVarName))
                    {
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            object first = ds.Tables[0].Rows[0][0];
                            string restr = first == DBNull.Value ? "" : first.ToString();
                            context.SetVarStr(dBActivity.SaveVarName, restr);
                            msg = $"获取到结果长度{restr.Length}并存入字符变量{dBActivity.SaveVarName}";
                        }
                        else
                        {
                            context.SetVarStr(dBActivity.SaveVarName, "");
                            msg = $"查询到0例数据并设置字符变量{dBActivity.SaveVarName}为空";
                        }
                    }
                    else if (context.ContainsList(dBActivity.SaveVarName))
                    {
                        List<string> ls = new List<string>();
                        if (dBActivity.NotClearVar) ls = context.GetList(dBActivity.SaveVarName);

                        foreach (System.Data.DataRow dr in ds.Tables[0].Rows)
                        {
                            string re2 = dr[0] == DBNull.Value ? "" : dr[0].ToString();
                            ls.Add(re2);
                        }
                        context.SetVarList(dBActivity.SaveVarName, ls);
                        msg = $"获取到记录数{ds.Tables[0].Rows.Count}并存入列表变量{dBActivity.SaveVarName}";
                    }
                    else if (context.ContainsInt(dBActivity.SaveVarName))
                    {
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            object first2 = ds.Tables[0].Rows[0][0];
                            int restr2 = first2 == DBNull.Value ? 0 : Convert.ToInt32(first2);
                            context.SetVarInt(dBActivity.SaveVarName, restr2);
                            msg = $"获取到数字变量{restr2}并存入数字变量{dBActivity.SaveVarName}";
                        }
                        else
                        {
                            context.SetVarInt(dBActivity.SaveVarName, 0);
                            msg = $"查询到0例数据并设置数字变量{dBActivity.SaveVarName}为0";
                        }
                    }
                    else if (context.ContainsTable(dBActivity.SaveVarName))
                    {
                        DataTable dataTable = new DataTable();
                        if (dBActivity.NotClearVar) dataTable = context.GetTable(dBActivity.SaveVarName);
                        if (dataTable == null) dataTable = new DataTable();
                        if (dataTable.Columns.Count == 0) context.Variables.Find((f) => f.Name == dBActivity.SaveVarName).TableValue = ds.Tables[0];
                        else
                        {
                            foreach (System.Data.DataRow dr in ds.Tables[0].Rows)
                            {
                                foreach (System.Data.DataColumn dc in ds.Tables[0].Columns)
                                {
                                    if (!dataTable.Columns.Contains(dc.ColumnName))
                                    {
                                        dataTable.Columns.Add(dc.ColumnName);
                                    }
                                }

                                System.Data.DataRow add = dataTable.NewRow();
                                foreach (System.Data.DataColumn dc in ds.Tables[0].Columns)
                                {
                                    if (dataTable.Columns.Contains(dc.ColumnName)) add[dc.ColumnName] = dr[dc.ColumnName];
                                }
                                dataTable.Rows.Add(add);
                            }
                        }
                        msg = $"获取到表格数据{ds.Tables[0].Rows.Count}并存入表格变量{dBActivity.SaveVarName}";
                    }
                }
                else
                {
                    for (int err = 0; err < 3; err++)
                    {
                        try
                        {
                            _fsql.Ado.ExecuteNonQuery(mksql);

                            break;
                        }
                        catch
                        {
                            if (err == 2) throw;
                            System.Threading.Thread.Sleep(1000);
                        }
                    }
                    msg = $"成功执行Sql";
                }
                context.WriteLog(msg);
            }
            catch (Exception ex)
            {
                msg = ex.Message + "\r\n" + ex.StackTrace + "\r\n" + mksql;
                throw new Exception(msg);
            }
        }

        public static FreeSql.DataType GetDataType(DbType dbType)
        {
            switch (dbType)
            {
                case DbType.MySql:
                    return FreeSql.DataType.MySql;
                case DbType.SqlServer:
                    return FreeSql.DataType.SqlServer;
                case DbType.Oracle:
                    return FreeSql.DataType.Oracle;
                case DbType.PostgreSQL:
                    return FreeSql.DataType.PostgreSQL;
            }
            throw new Exception("不支持的数据库类型");
        }

        /// <summary>
        /// 当前的数据库配置
        /// </summary>
        //static ConcurrentDictionary<string, IFreeSql> dsql = new ConcurrentDictionary<string, IFreeSql>();

        //public static IFreeSql GetFreeSql(DbType dbType, string connstr, int hash)
        //{
        //    IFreeSql _fsql = null;

        //    string key = connstr + "_" + hash.ToString();
        //    if (dsql.ContainsKey(key)) _fsql = dsql[key];
        //    else
        //    {
        //        _fsql = new FreeSql.FreeSqlBuilder()
        //       .UseConnectionString(SqlLoad.GetDataType(dbType), connstr)
        //       .Build();
        //        dsql.TryAdd(key, _fsql);
        //    }

        //    return _fsql;
        //}

        //public static void DisConnect(string connstr, int hash)
        //{
        //    string key = connstr + "_" + hash.ToString();
        //    if (dsql.ContainsKey(key))
        //    {
        //        dsql[key].Dispose();
        //        IFreeSql free = null;
        //        dsql.TryRemove(key, out free);
        //    }
        //}

        public const string MySqlReFile = "FreeSql.dll,FreeSql.Provider.MySql.dll,MySql.Data.dll,Google.Protobuf.dll,K4os.Hash.xxHash.dll,K4os.Compression.LZ4.dll,K4os.Compression.LZ4.Streams.dll,Ubiety.Dns.Core.dll,Zstandard.Net.dll";

        public const string MsSqlReFile = "FreeSql.dll,FreeSql.Provider.SqlServer.dll";

        public const string OracleReFile = "FreeSql.dll,FreeSql.Provider.Oracle.dll,Oracle.ManagedDataAccess.dll";

        public const string PostgreReFile = "FreeSql.dll,FreeSql.Provider.PostgreSQL.dll,NetTopologySuite.dll,NetTopologySuite.IO.PostGis.dll,Npgsql.dll,Npgsql.LegacyPostgis.dll,Npgsql.NetTopologySuite.dll";
    }
}