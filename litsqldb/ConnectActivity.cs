using litsdk;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace litsqldb
{
    /// <summary>
    /// 数据库配置
    /// </summary>
    [Action(Name = "打开SQL连接", Order = 1, IsLinux = true, Category = "数据库", Description = "当使用异步多线程时，每个线程会生成独立的连接")]
    public class ConnectActivity : litsdk.ProcessActivity
    {
        public override string Name { get; set; } = "打开SQL连接";

        [Argument(Name = "配置名称", ControlType = ControlType.TextBox, Order = 1, Description = "创建的数据库配置名称，在接下来的数据库操作中使用")]
        public string ConfigName { get; set; }

        /// <summary>
        /// 数据库类型
        /// </summary>
        [Argument(Name = "数据库类型", ControlType = ControlType.ComboBox, Order = 2, Description = "数据库类型")]
        public DbType DbType { set; get; } = DbType.MySql;

        /// <summary>
        /// 服务器地址
        /// </summary>
        [Argument(Name = "服务器地址", ControlType = ControlType.TextBox, Order = 3, Description = "服务器地址")]
        public string Host { set; get; }

        /// <summary>
        /// 端口
        /// </summary>
        [Argument(Name = "服务器端口", ControlType = ControlType.TextBox, Order = 4, Description = "服务器地址")]
        public string Port { set; get; } = "3306";

        /// <summary>
        /// 用户名
        /// </summary>
        [Argument(Name = "用户名", ControlType = ControlType.TextBox, Order = 5, Description = "用户名")]
        public string UserName { set; get; }

        /// <summary>
        /// 密码
        /// </summary>
        [Argument(Name = "密码", ControlType = ControlType.Password, Order = 6, Description = "密码")]
        public string Password { set; get; }

        /// <summary>
        /// 数据库名
        /// </summary>
        [Argument(Name = "数据库名", ControlType = ControlType.TextBox, Order = 7, Description = "数据库名")]
        public string DBName { set; get; }

        /// <summary>
        /// 编码
        /// </summary>
        [Argument(Name = "编码", ControlType = ControlType.ComboBox, Order = 8, Description = "编码")]
        public string CharSet { set; get; } = "utf8mb4";

        public string ToConnectStr()
        {
            string conn = "";
            switch (this.DbType)
            {
                case DbType.MySql:
                    string mydb = string.IsNullOrEmpty(this.DBName) ? "information_schema" : this.DBName;
                    conn = $"server={this.Host};user id={this.UserName}; password={this.Password}; database={mydb}; pooling=true;charset={this.CharSet};Port={this.Port};Min pool size=1;max pool size=1";
                    break;
                case DbType.SqlServer:
                    string db = this.DBName == "" ? "master" : this.DBName;
                    conn = $"Data Source={this.Host},{this.Port};User ID={this.UserName};Password={this.Password};Initial Catalog={db};Connect Timeout=10;Encrypt=True;TrustServerCertificate=True;Min pool size=1;max pool size=1";
                    break;
                case DbType.PostgreSQL:
                    conn = $"Host={Host};Port={Port};Username={UserName};Password={Password}; Database={DBName};Pooling=true;Minimum Pool Size=1";
                    break;
                case DbType.Oracle:
                    conn = $"user id={UserName};password={Password}; data source=//{Host}:{Port}/{DBName};Pooling=true;Min Pool Size=1";
                    break;
                case DbType.Dameng:
                    conn = $"server={Host};port={Port};user id={UserName};password={Password};database={DBName};poolsize=1";
                    break;
                case DbType.KingbaseES:
                    conn = $"Server={Host};Port={Port};UID={UserName};PWD={Password};database={DBName};MAXPOOLSIZE=1";
                    break;
                case DbType.ShenTong:
                    conn = $"HOST={Host};PORT={Port};DATABASE={DBName};USERNAME={UserName};PASSWORD={Password};MAXPOOLSIZE=1";
                    break;
            }
            return conn;
        }

        internal IFreeSql fsql = null;
        internal string connstrOld = null;
        public override void Execute(ActivityContext context)
        {
            string connstr = context.ReplaceVar(this.ToConnectStr());
            if (fsql == null || connstr != connstrOld)
            {
                fsql = new FreeSql.FreeSqlBuilder()
              .UseConnectionString(SqlLoad.GetDataType(this.DbType), connstr)
              .Build();
                connstrOld = connstr;
            }

            bool re = fsql.Ado.ExecuteConnectTest();
            if (!re)
            {
                System.Threading.Thread.Sleep(2000);
                re = fsql.Ado.ExecuteConnectTest();
            }
            if (re)
            {
                context.WriteLog("成功连接至数据库：" + this.ConfigName);
            }
            else
            {
                throw new Exception("连接到数据库出错：" + fsql.Ado.MasterPool.UnavailableException.Message);
            }
        }

        public override void Validate(ActivityContext context)
        {
            if (string.IsNullOrEmpty(this.Host)) throw new Exception("服务器地址不能为空");
            if (string.IsNullOrEmpty(this.UserName)) throw new Exception("用户名不能为空");
            if (string.IsNullOrEmpty(this.Password)) throw new Exception("密码不能为空");
            if (string.IsNullOrEmpty(this.DBName)) throw new Exception("数据库不能为空");
            if (string.IsNullOrEmpty(this.Port)) throw new Exception("端口不能为空");
        }

        private DbType lastDbType = DbType.Dameng;
        public override ControlStyle GetControlStyle(string field)
        {
            ControlStyle style = new ControlStyle();
            switch (field)
            {
                case "CharSet":
                    style.Visible = this.DbType == DbType.MySql;
                    style.DropDownList = new List<string>() { "utf8mb4", "utf8", "gbk", "gb2312" };
                    break;
                case "Port":
                    style.Max = 65535;
                    style.Min = 1;
                    break;
                case "ConfigName":
                    break;
                default:
                    style.Variables = ControlStyle.GetVariables(true, false, true);
                    break;
            }
            return style;
        }

        public static ConnectActivity GetConnectActivity(string ConfigName, ActivityContext context)
        {
            List<Activity> acts = context.GetActivities(typeof(ConnectActivity).FullName);
            ConnectActivity connect = null;
            foreach (Activity activity in acts)
            {
                if (activity is ConnectActivity ca)
                {
                    if (ca.ConfigName == ConfigName)
                    {
                        connect = ca;
                        break;
                    }
                }
            }
            return connect;
        }

        public static List<string> GetDesignConnectActivities()
        {
            List<string> css = new List<string>();
            List<Activity> acts = litsdk.API.GetDesignActivityContext().GetActivities(typeof(ConnectActivity).FullName);
            foreach (Activity activity in acts)
            {
                if (activity is ConnectActivity ca)
                {
                    if (!string.IsNullOrEmpty(ca.ConfigName)) css.Add(ca.ConfigName);
                }
            }
            css = css.Distinct().ToList();
            return css;
        }
    }
}