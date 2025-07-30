using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using litsdk;

namespace litsqlite
{
    [litsdk.Action(Name = "Sqlite", IsLinux = true, RefFile = "FreeSql.dll,FreeSql.Provider.Sqlite.dll,System.Data.SQLite.dll,x86\\,x64\\", Order = 5, Category = "数据库", Description = "操作本地Sqlite数据库进行数据的读写")]
    public class Sqlitedb : litsdk.ProcessActivity
    {
        public override string Name { get; set; } = "Sqlite";

        /// <summary>
        /// 文件地址
        /// </summary>
        [Argument(Name = "文件地址", ControlType = ControlType.File, Order = 1, Description = ".db3的文件路径")]
        public string SqliteFile { get; set; }

        /// <summary>
        /// sql语句
        /// </summary>
        [Argument(Name = "Sql语句", ControlType = ControlType.TextArea, Order = 2, Description = "查询语句")]
        public string Sql { get; set; }

        /// <summary>
        /// 是否转义
        /// </summary>
        [Argument(Name = "对SQL语句中的变量进行转义", ControlType = ControlType.CheckBox, Order = 3, Description = "是否对SQL内容中的变量进行转义操作")]
        public bool Addslashes { get; set; } = true;

        [Argument(Name = "保持该数据库为打开状态", ControlType = ControlType.CheckBox, Order = 4, Description = "在当前进程当中，该数据库在打开后将一直保持打开状态，这样下次操作将更加快速")]
        public bool KeepAlive { get; set; }

        [Argument(Name = "保存查询结果至变量当中", ControlType = ControlType.CheckBox, Order = 5, Description = "当使用Select,Call等有返回结果的方式时，可以将结果保存至变量当中")]
        public bool SaveResult2Var { get; set; }

        [Argument(Name = "取查询结果第一条并赋值各字段同名变量值为字段值", ControlType = ControlType.CheckBox, Order = 6, Description = "选比如 select name,age as age2 from data 中，将查询结果值存入同名的name和age2变量当中")]
        public bool SaveMultStrVar { get; set; }

        /// <summary>
        /// 保存至变量
        /// </summary>
        [litsdk.Argument(Name = "保存至变量", Description = "选多个变量，支持字符和数字变量", Order = 8, ControlType = ControlType.Variable)]
        public List<string> SaveVarNames { get; set; }

        /// <summary>
        /// 保存至变量
        /// </summary>
        [litsdk.Argument(Name = "保存至变量", Description = "将Select查询结果保存至指定变量(可选字符数字字符列表变量)，\r\n字符数字取第一行第一列，列表取第一列，表格是查询结果。", Order = 15, ControlType = ControlType.Variable)]
        public string SaveVarName { get; set; }

        [litsdk.Argument(Name = "存入列表和表格变量时不清空原数据", Order = 18, ControlType = ControlType.CheckBox, Description = "默认存入变量时是将原值清空，选中该项后则列表和表格是将新数据添加至原数据后")]
        public bool NotClearVar { get; set; }

        /// <summary>
        /// 锁
        /// </summary>
        private static object strref = new object();
        private static Dictionary<string, IFreeSql> fdic = new Dictionary<string, IFreeSql>();
        public override void Execute(ActivityContext context)
        {
            string msg = "";
            string mksql = "";
            string filepath = context.ReplaceVar(this.SqliteFile);

            lock (strref)
            {
                if (this.Addslashes)
                {
                    mksql = context.AddSlashReplaceVar(this.Sql, SqliteAddSlash);
                }
                else
                {
                    mksql = context.ReplaceVar(this.Sql);
                }

                string connstr = SqliteConn(filepath);
                IFreeSql _fsql = null;

                try
                {
                    if (mksql.StartsWith("select", StringComparison.OrdinalIgnoreCase))
                    {
                        if (KeepAlive && fdic.ContainsKey(filepath))
                        {
                            _fsql = fdic[filepath];
                        }
                        else
                        {
                            _fsql = new FreeSql.FreeSqlBuilder()
                         .UseConnectionString(FreeSql.DataType.Sqlite, connstr)
                         .Build();
                        }
                        DataSet ds = new DataSet();
                        ds = _fsql.Ado.ExecuteDataSet(mksql);

                        if (this.SaveResult2Var)
                        {
                            if (this.SaveMultStrVar)
                            {
                                foreach (string name in this.SaveVarNames)
                                {
                                    context.Clear(name);
                                }
                                if (ds.Tables[0].Rows.Count > 0)
                                {
                                    foreach (System.Data.DataColumn dc in ds.Tables[0].Columns)
                                    {
                                        if (this.SaveVarNames.Contains(dc.ColumnName))
                                        {
                                            object odata = ds.Tables[0].Rows[0][dc.ColumnName];
                                            string restr = odata == DBNull.Value ? "" : odata.ToString();
                                            if (context.ContainsStr(dc.ColumnName))
                                            {
                                                context.SetVarStr(dc.ColumnName, restr);
                                            }
                                            else if (context.ContainsInt(dc.ColumnName))
                                            {
                                                int it = 0;
                                                if (int.TryParse(restr, out it))
                                                {
                                                    context.SetVarInt(dc.ColumnName, it);
                                                }
                                                else
                                                {
                                                    context.WriteLog($"{dc.ColumnName}转换{restr}为数字时出错置为0");
                                                }
                                            }
                                        }
                                    }
                                    msg = $"获取到结果并存入变量{string.Join(",", this.SaveVarNames.ToArray())}";
                                }
                                else
                                {
                                    msg = $"查询到结果记录数为空，置空变量{string.Join(",", this.SaveVarNames.ToArray())}";
                                }
                            }
                            else
                            {
                                if (context.ContainsStr(this.SaveVarName))
                                {
                                    if (ds.Tables[0].Rows.Count > 0)
                                    {
                                        object first = ds.Tables[0].Rows[0][0];
                                        string restr = first == DBNull.Value ? "" : first.ToString();
                                        context.SetVarStr(this.SaveVarName, restr);
                                        msg = $"获取到结果长度{restr.Length}并存入字符变量{this.SaveVarName}";
                                    }
                                    else
                                    {
                                        context.SetVarStr(this.SaveVarName, "");
                                        msg = $"查询到结果记录数为空，置空字符变量{this.SaveVarName}";
                                    }
                                }
                                else if (context.ContainsList(this.SaveVarName))
                                {
                                    List<string> ls = new List<string>();
                                    if (this.NotClearVar) ls = context.GetList(this.SaveVarName);

                                    foreach (System.Data.DataRow dr in ds.Tables[0].Rows)
                                    {
                                        string re2 = dr[0] == DBNull.Value ? "" : dr[0].ToString();
                                        ls.Add(re2);
                                    }
                                    context.SetVarList(this.SaveVarName, ls);
                                    msg = $"获取到记录数{ls.Count}并存入列表变量{this.SaveVarName}";
                                }
                                else if (context.ContainsInt(this.SaveVarName))
                                {
                                    if (ds.Tables[0].Rows.Count > 0)
                                    {
                                        object first2 = ds.Tables[0].Rows[0][0];
                                        int restr2 = first2 == DBNull.Value ? 0 : Convert.ToInt32(first2);
                                        context.SetVarInt(this.SaveVarName, restr2);
                                        msg = $"获取到数字变量{restr2}并存入数字变量{this.SaveVarName}";
                                    }
                                    else
                                    {
                                        context.SetVarInt(this.SaveVarName, 0);
                                        msg = $"获取到结果为空置空数字变量{this.SaveVarName}";
                                    }
                                }
                                else if (context.ContainsTable(this.SaveVarName))
                                {
                                    DataTable dataTable = new DataTable();
                                    if (this.NotClearVar) dataTable = context.GetTable(this.SaveVarName);
                                    if (dataTable.Columns.Count == 0) context.Variables.Find((f) => f.Name == this.SaveVarName).TableValue = ds.Tables[0];
                                    else
                                    {
                                        foreach (System.Data.DataRow dr in ds.Tables[0].Rows)
                                        {
                                            System.Data.DataRow add = dataTable.NewRow();
                                            foreach (System.Data.DataColumn dc in ds.Tables[0].Columns)
                                            {
                                                if (dataTable.Columns.Contains(dc.ColumnName)) add[dc.ColumnName] = dr[dc.ColumnName];
                                            }
                                            dataTable.Rows.Add(add);
                                        }
                                        context.Variables.Find((f) => f.Name == this.SaveVarName).TableValue = dataTable;
                                    }
                                    msg = this.NotClearVar ? $"获取到表格数据{ds.Tables[0].Rows.Count}并存入表格变量{this.SaveVarName}" : $"获取到表格数据{ds.Tables[0].Rows.Count}条";
                                }
                            }
                        }
                    }
                    else
                    {
                        if (mksql.StartsWith("create", StringComparison.OrdinalIgnoreCase) && !System.IO.File.Exists(filepath))
                        {
                            System.Data.SQLite.SQLiteConnection.CreateFile(filepath);
                            _fsql = new FreeSql.FreeSqlBuilder()
                     .UseConnectionString(FreeSql.DataType.Sqlite, connstr)
                     .Build();
                        }
                        else
                        {
                            if (KeepAlive && fdic.ContainsKey(filepath))
                            {
                                _fsql = fdic[filepath];
                            }
                            else
                            {
                                _fsql = new FreeSql.FreeSqlBuilder()
                             .UseConnectionString(FreeSql.DataType.Sqlite, connstr)
                             .Build();
                            }
                        }

                        _fsql.Ado.ExecuteNonQuery(mksql);

                        msg = $"成功执行Sql操作";
                    }
                    context.WriteLog(msg);
                }
                catch (Exception ex)
                {
                    msg = ex.Message + "\r\n" + ex.StackTrace + "\r\n" + mksql;
                    throw new Exception(msg);
                }
                finally
                {
                    try
                    {
                        if (_fsql != null)
                        {
                            if (this.KeepAlive)
                            {
                                if (!fdic.ContainsKey(filepath)) fdic.Add(filepath, _fsql);
                            }
                            else
                            {
                                _fsql.Dispose();
                                if (fdic.ContainsKey(filepath))
                                {
                                    fdic[filepath].Dispose();
                                    fdic.Remove(filepath);
                                }
                            }
                        }
                    }
                    catch { }
                }
            }
        }

        internal static string SqliteConn(string path)
        {
            //return "Provider=SQLiteOLEDB;Data Source=" + path;
            return $"Data Source={path}; Pooling=true;Min Pool Size=1";
        }

        internal static string SqliteAddSlash(string str)
        {
            str = str != "" ? (str.Replace("'", @"''")) : "";
            return str;
        }

        public override void Validate(ActivityContext context)
        {
            if (string.IsNullOrEmpty(this.SqliteFile)) throw new Exception("数据库文件不能为空");
            if (string.IsNullOrEmpty(this.Sql)) throw new Exception("执行Sql语句不能为空");

            if (this.SaveMultStrVar)
            {
                if (!this.Sql.StartsWith("select", StringComparison.OrdinalIgnoreCase)) throw new Exception("使用查询一列时必须以select开始");
                if (this.SaveVarNames.Count <= 1) throw new Exception("使用保存多变量时，保存变量数要大于1");
                foreach (string name in this.SaveVarNames)
                {
                    if (!context.ContainsStr(name) && !context.ContainsInt(name)) throw new Exception($"保存变量名必须为字符或数字:{name}");
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(this.SaveVarName) && !context.Contains(this.SaveVarName))
                {
                    throw new Exception($"保存变量名 {this.SaveVarName} 不存在，请检查");
                }
                if (this.Sql.StartsWith("select", StringComparison.OrdinalIgnoreCase) && string.IsNullOrEmpty(this.SaveVarName)) throw new Exception("使用查询时必须指定保存变量");
            }
        }

        public override ControlStyle GetControlStyle(string field)
        {
            ControlStyle style = new ControlStyle();
            switch (field)
            {
                case "SqliteFile":
                    style.Variables = ControlStyle.GetVariables(true, false, true);
                    break;
                case "Sql":
                    style.Variables = ControlStyle.GetVariables(true, false, true);
                    style.PlaceholderText = "当使用select查询时可以将结果保存至变量";
                    break;
                case "SaveVarNames":
                    style.Visible = this.SaveResult2Var && this.SaveMultStrVar;
                    style.Variables = ControlStyle.GetVariables(true, false, true, false);
                    break;
                case "SaveVarName":
                case "NotClearVar":
                    style.Visible = this.SaveResult2Var && !this.SaveMultStrVar;
                    style.Variables = ControlStyle.GetVariables(true, true, true, true);
                    break;
                case "SaveMultStrVar":
                    style.Visible = this.SaveResult2Var;
                    break;
            }
            return style;
        }
    }
}