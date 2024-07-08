using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using litsdk;

namespace litsqldb
{
    /// <summary>
    /// 执行SQL语句
    /// </summary>
    [Action(Name = "执行SQL语句", Category = "数据库", IsLinux = true, Order = 2, Description = "针对各数据库类型编写相应的SQL语句并执行")] 
    public class RunSqlActivity : litsdk.ProcessActivity
    {
        public override string Name { get; set; } = "执行SQL语句";

        /// <summary>
        /// 配置名称
        /// </summary>
        [Argument(Name = "数据库连接", ControlType = ControlType.ComboBox, Order = 1, Description = "要执行操作的数据库连接")]
        public string ConfigName { get; set; }//实际显示按ip:端口(数据库)，这里是guid

        /// <summary>
        /// 要执行的Sql语句
        /// </summary>
        [Argument(Name = "SQL语句", ControlType = ControlType.TextArea, Order = 2, Description = "要执行的SQL语句，可以")]
        public string Sql { get; set; }
        /// <summary>
        /// 是否转义
        /// </summary>
        [Argument(Name = "对SQL语句中的变量进行转义", ControlType = ControlType.CheckBox, Order = 3, Description = "是否对SQL内容中的变量进行转义操作")]
        public bool Addslashes { get; set; } = true;

        [Argument(Name = "保存查询结果至变量当中", ControlType = ControlType.CheckBox, Order = 4, Description = "当使用Select,Call等有返回结果的方式时，可以将结果保存至变量当中")]
        public bool SaveResult2Var { get; set; }

        /// <summary>
        /// 保存变量名称
        /// </summary>
        [Argument(Name = "结果保存至", ControlType = ControlType.Variable, Order = 5, Description = "查询的结果保存至变量")]
        public string SaveVarName { get; set; }

        [litsdk.Argument(Name = "存入列表和表格变量时不清空原数据", Order = 6, ControlType = ControlType.CheckBox, Description = "默认存入变量时是将原值清空，选中该项后则列表和表格是将新数据添加至原数据后")]
        public bool NotClearVar { get; set; }


        public static object dblk = new object();

        public override void Execute(ActivityContext context)
        {
            lock (dblk)
            {
                ConnectActivity connect = ConnectActivity.GetConnectActivity(this.ConfigName, context);
                if (connect == null) new Exception("不存在数据库配置：" + this.ConfigName);

                string connstr = context.ReplaceVar(connect.ToConnectStr());
                string mksql = "";

                if (!this.Addslashes)
                {
                    mksql = context.ReplaceVar(this.Sql);
                }
                else
                {
                    if (connect.DbType == DbType.MySql)
                    {
                        mksql = context.AddSlashReplaceVar(this.Sql, MysqlAddSlash);
                    }
                    else if (connect.DbType == DbType.SqlServer)
                    {
                        mksql = context.AddSlashReplaceVar(this.Sql, SqlserverAddSlash);
                    }
                    else
                    {
                        mksql = context.ReplaceVar(this.Sql);
                    }
                }
                IFreeSql _fsql = connect.fsql;
                if (connect.fsql == null) throw new Exception("数据库还未连接");

                SqlLoad.Execute(mksql, _fsql, context, this);
            }
        }

        public override void Validate(ActivityContext context)
        {
            ConnectActivity connect = ConnectActivity.GetConnectActivity(this.ConfigName, context);
            if (connect == null) new Exception("不存在数据库配置：" + this.ConfigName);

            if (string.IsNullOrEmpty(this.Sql)) throw new Exception("执行Sql语句不能为空");
            if (!string.IsNullOrEmpty(this.SaveVarName) && !context.Contains(this.SaveVarName))
            {
                throw new Exception($"保存变量名 {this.SaveVarName} 不存在，请检查");
            }
            if (this.Sql.StartsWith("select", StringComparison.OrdinalIgnoreCase) && string.IsNullOrEmpty(this.SaveVarName)) throw new Exception("使用查询时必须指定保存变量");
        }


        public static string SqlserverAddSlash(string str)
        {
            str = str != "" ? (str.Replace("'", @"''")) : "";
            return str;
        }
        /// <summary>
        /// MySql中\的转义
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string MysqlAddSlash(string str)
        {
            str = str.Replace(@"\", @"\\");
            str = str.Replace("'", @"\'");
            return str;
        }

        public override ControlStyle GetControlStyle(string field)
        {
            ControlStyle style = new ControlStyle() { Visible = true, Variables = ControlStyle.GetVariables(true, true, true, true) };

            switch (field)
            {
                case "ConfigName":
                    style.DropDownList = ConnectActivity.GetDesignConnectActivities();
                    break;
                case "Sql":
                    style.Variables = ControlStyle.GetVariables(true, false, true);
                    style.PlaceholderText = "当使用select查询时可以将结果保存至变量";
                    break;
                case "SaveVarName":
                case "NotClearVar":
                    style.Visible = this.SaveResult2Var;
                    break;
            }
            return style;
        }
    }
}