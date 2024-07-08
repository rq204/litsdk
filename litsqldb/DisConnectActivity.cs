using litsdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace litsqldb
{
    [litsdk.Action(Name = "关闭SQL连接", Order = 3, IsLinux = true, Category = "数据库", Description = "关闭指定配置的数据库连接")] 
    public class DisConnectActivity : litsdk.ProcessActivity
    {
        public override string Name { get; set; } = "关闭SQL连接";

        /// <summary>
        /// 配置名称
        /// </summary>
        [Argument(Name = "数据库连接", ControlType = ControlType.ComboBox, Order = 1, Description = "选择要关闭的数据库连接")]
        public string ConfigName { get; set; }

        public override void Execute(ActivityContext context)
        {
            ConnectActivity connect = ConnectActivity.GetConnectActivity(this.ConfigName, context);
            if (connect == null) new Exception("不存在数据库配置：" + this.ConfigName);
            connect.fsql.Dispose();
            connect.fsql = null;
            context.WriteLog($"数据库连接 {this.ConfigName} 关闭成功");
        }

        public override ControlStyle GetControlStyle(string field)
        {
            return new ControlStyle() { DropDownList = ConnectActivity.GetDesignConnectActivities() };
        }

        public override void Validate(ActivityContext context)
        {
            if (string.IsNullOrEmpty(ConfigName)) throw new Exception("关闭连接名称不能为空");
        }
    }
}