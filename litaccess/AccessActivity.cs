using litsdk;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace litaccess
{
    [litsdk.Action(Name = "Access", Category = "数据库", Order = 45, IsLinux = true, RefFile = "FreeSql.dll,FreeSql.Provider.MsAccess.dll", Description = "查询数据库结果，如果发生错误，流程会报异常")]
    public class AccessActivity : litsdk.ProcessActivity
    {
        public override string Name { get; set; } = "Access";

        [litsdk.Argument(Name = "数据库路径", Description = ".mdb文件本地文件路径", Order = 1, ControlType = ControlType.File)]
        /// <summary>
        /// 文件地址
        /// </summary>
        public string AccessFile { get; set; }

        [litsdk.Argument(Name = "执行Sql语句", Description = "可以执行增删改查语句，可以保存查询结果至变量", Order = 2, ControlType = ControlType.TextArea)]
        /// <summary>
        /// sql语句
        /// </summary>
        public string Sql { get; set; }

        /// <summary>
        /// 是否转义
        /// </summary>
        [Argument(Name = "对SQL语句中的变量进行转义", ControlType = ControlType.CheckBox, Order = 3, Description = "是否对SQL内容中的变量进行转义操作")]
        public bool Addslashes { get; set; } = true;

        [Argument(Name = "保持该数据库为打开状态", ControlType = ControlType.CheckBox, Order = 4, Description = "在当前进程当中，该数据库在打开后将一直保持打开状态，这样下次操作将更加快速")]
        public bool KeepAlive { get; set; }

        [litsdk.Argument(Name = "存入变量", Description = "将Select查询结果保存至指定变量(可选字符数字字符列表变量)，\r\n字符数字取第一行第一列，列表取第一列，表格是查询结果。", Order = 5, ControlType = ControlType.Variable)]
        /// <summary>
        /// 保存至变量
        /// </summary>
        public string SaveVarName { get; set; }

        [litsdk.Argument(Name = "存入列表和表格变量时不清空原数据", Order = 6, ControlType = ControlType.CheckBox, Description = "默认存入变量时是将原值清空，选中该项后则列表和表格是将新数据添加至原数据后")]
        public bool NotClearVar { get; set; }

        private static object strref = new object();
        private static Dictionary<string, IFreeSql> fdic = new Dictionary<string, IFreeSql>();
        public override void Execute(ActivityContext context)
        {
            string path = context.ReplaceVar(this.AccessFile);
            string conn = AccessConn(path);

            lock (strref)
            {
                IFreeSql _fsql = null;
                if (this.KeepAlive)
                {
                    if (fdic.ContainsKey(path))
                    {
                        _fsql = fdic[path];
                    }
                }

                if (_fsql == null) _fsql = new FreeSql.FreeSqlBuilder()
                       .UseConnectionString(FreeSql.DataType.MsAccess, conn)
                       .Build();

                string msg = "";
                string mksql = "";
                if (this.Addslashes)
                {
                    mksql = context.AddSlashReplaceVar(this.Sql, AccessAddSlash);
                }
                else
                {
                    mksql = context.ReplaceVar(this.Sql);
                }
                try
                {
                    if (mksql.ToLower().StartsWith("select", StringComparison.OrdinalIgnoreCase))
                    {
                        DataSet ds = new DataSet();
                        ds = _fsql.Ado.ExecuteDataSet(mksql);

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
                            msg = this.NotClearVar ? $"获取到记录数{ls.Count}并存入列表变量{this.SaveVarName}" : $"获取到记录数{ls.Count}";
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
                    else
                    {
                        int num = _fsql.Ado.ExecuteNonQuery(mksql);

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
                                if (!fdic.ContainsKey(path)) fdic.Add(path, _fsql);
                            }
                            else
                            {
                                _fsql.Dispose();
                                if (fdic.ContainsKey(path))
                                {
                                    fdic[path].Dispose();
                                    fdic.Remove(path);
                                }
                            }
                        }
                    }
                    catch { }
                }
            }
        }


        public override void Validate(ActivityContext context)
        {
            if (string.IsNullOrEmpty(this.AccessFile)) throw new Exception("数据库文件不能为空");
            if (string.IsNullOrEmpty(this.Sql)) throw new Exception("执行Sql语句不能为空");
            if (!string.IsNullOrEmpty(this.SaveVarName) && !context.Contains(this.SaveVarName))
            {
                throw new Exception($"保存变量名 {this.SaveVarName} 不存在，请检查");
            }
            if (this.Sql.ToLower().StartsWith("select", StringComparison.OrdinalIgnoreCase) && string.IsNullOrEmpty(this.SaveVarName)) throw new Exception("使用查询时必须指定保存变量");
        }

        public override ControlStyle GetControlStyle(string field)
        {
            ControlStyle style = new ControlStyle();
            switch (field)
            {
                case "AccessFile":
                    style.Variables = ControlStyle.GetVariables(true, false, true);
                    break;
                case "Sql":
                    style.Variables = ControlStyle.GetVariables(true, false, true);
                    style.PlaceholderText = "当使用select查询时可以将结果保存至变量";
                    break;
                case "SaveVarName":
                    style.Variables = ControlStyle.GetVariables(true, true, true, true);
                    break;
            }
            return style;
        }

        internal static string AccessConn(string path)
        {
            if (IntPtr.Size == 8)
            {
                return $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={path}";
            }
            else
            {
                return $"Provider=Microsoft.Jet.OleDb.4.0;Data Source={path}";
            }
        }

        internal static string AccessAddSlash(string str)
        {
            str = str != "" ? (str.Replace("'", @"''")) : "";
            return str;
        }
    }
}