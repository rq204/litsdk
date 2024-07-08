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
    [litsdk.Action(Name = "关闭FTP连接", Category = "FTP", Order = 30, IsLinux = true, RefFile = FtpLoad.RefFile, Description = "断开指定的FTP连接")] 
    public class DisConnectActivity : litsdk.ProcessActivity
    {
        public override string Name { get; set; } = "关闭FTP连接";

        /// <summary>
        /// 配置名称
        /// </summary>
        [Argument(Name = "FTP连接", ControlType = ControlType.ComboBox, Order = 1, Description = "选择要关闭的FTP连接")]
        public string ConfigName { get; set; }

        public override void Execute(ActivityContext context)
        {
            FtpLoad load = FtpLoad.GetConnect(this.ConfigName, context);

            if (!load.Sftp)
            {
                if (load.ftpClient.IsConnected) load.ftpClient.Disconnect();
                context.WriteLog($"成功关闭连接{ConfigName}");
            }
            else
            {
                if (load.sftpClient.IsConnected) load.sftpClient.Disconnect();
                context.WriteLog($"成功关闭连接{ConfigName}");
            }
        }

        public override ControlStyle GetControlStyle(string field)
        {
            ControlStyle style = new ControlStyle();
            switch (field)
            {
                case "ConfigName":
                    style.DropDownList = FtpLoad.GetFtpList();
                    break;
            }
            style.Variables = litsdk.ControlStyle.GetVariables(true, false, true);
            return style;
        }

        public override void Validate(ActivityContext context)
        {
            FtpLoad.ValidateFtpConfig(this.ConfigName, context);
        }
    }
}