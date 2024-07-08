using FluentFTP;
using litsdk;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace litftp
{
    [litsdk.Action(Name = "建立FTP连接", Category = "FTP", IsLinux = true, RefFile = FtpLoad.RefFile, Description = "设置FTP基本信息并创建连接")] 
    public class ConnectActivity : litsdk.ProcessActivity
    {
        public override string Name { get; set; } = "建立FTP连接";

        [Argument(Name = "配置名称", ControlType = ControlType.TextBox, Order = 1, Description = "创建的FTP配置名称，在接下来的操作中使用")]
        public string ConfigName { get; set; }

        [litsdk.Argument(Name = "主机地址", ControlType = ControlType.TextBox, Description = "支持IP和域名，形如 127.0.0.1或 test.ftp.com", Order = 2)]
        public string Server { get; set; }

        [litsdk.Argument(Name = "主机端口", ControlType = ControlType.TextBox, Description = "一般ftp端口为21", Order = 3)]
        public string Port { get; set; } = "21";

        [litsdk.Argument(Name = "主机帐号", ControlType = ControlType.TextBox, Description = "FTP帐号，匿名用", Order = 4)]
        public string UserName { get; set; }

        [litsdk.Argument(Name = "主机密码", ControlType = ControlType.Password, Description = "FTP或SFTP密码", Order = 5)]
        public string Password { get; set; }

        [litsdk.Argument(Name = "使用SFTP", ControlType = ControlType.CheckBox, Description = "SFTP模式", Order = 6)]
        public bool Sftp { get; set; }

        /// <summary>
        ///  被动模式
        /// </summary>
        [litsdk.Argument(Name = "被动模式", ControlType = ControlType.CheckBox, Description = "选中后，将使用被动模式上传下载文件", Order = 7)]
        public bool Passive { get; set; }

        /// <summary>
        /// 常用操作 
        /// https://blog.csdn.net/yangwohenmai1/article/details/88571375
        /// https://github.com/robinrodricks/FluentFTP
        /// </summary>
        /// <param name="context"></param>
        public override void Execute(ActivityContext context)
        {
            string server = context.ReplaceVar(this.Server);
            string username = context.ReplaceVar(this.UserName);
            string password = context.ReplaceVar(this.Password);
            string portstr = context.ReplaceVar(this.Port);

            int port = 0;
            if (!int.TryParse(portstr, out port))
            {
                throw new Exception("FTP端口错误：" + portstr);
            }

            FtpLoad.GetConnect(this.ConfigName, context);

            context.WriteLog($"FTP配置{this.ConfigName}成功加载");
        }

        public override void Validate(ActivityContext context)
        {
            if (string.IsNullOrEmpty(this.Server)) throw new Exception("主机地址不能为空");
            if (string.IsNullOrEmpty(this.Port)) throw new Exception("端口不能为空");
            if (string.IsNullOrEmpty(this.UserName)) throw new Exception("用户名不能为空");
            if (string.IsNullOrEmpty(this.Password)) throw new Exception("密码不能为空");
        }


        public override ControlStyle GetControlStyle(string field)
        {
            ControlStyle style = new ControlStyle();
            style.Variables = litsdk.ControlStyle.GetVariables(true, false, true);

            switch (field)
            {
                case "Passive":
                    style.Visible = !this.Sftp;
                    break;
            }

            return style;
        }
    }
}