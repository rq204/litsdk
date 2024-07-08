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
    [litsdk.Action(Name = "删除文件夹", Category = "FTP", Order = 8, IsLinux = true, RefFile = "FluentFTP.dll,Renci.SshNet.dll", Description = "删除服务端指定文件夹")] 
    public class DeleteDirActivity : litsdk.ProcessActivity
    {
        public override string Name { get; set; } = "删除文件夹";

        /// <summary>
        /// 配置名称
        /// </summary>
        [Argument(Name = "FTP连接", ControlType = ControlType.ComboBox, Order = 1, Description = "要执行操作的FTP连接")]
        public string ConfigName { get; set; }//实际显示按ip:端口(数据库)，这里是guid

        [litsdk.Argument(Name = "服务端目录", ControlType = ControlType.TextBox, Description = "上传下载或是要操作的服务端目录，例如 /Data/2023/", Order = 10)]
        public string ServerDir { get; set; }

        /// <summary>
        /// 常用操作 
        /// https://blog.csdn.net/yangwohenmai1/article/details/88571375
        /// https://github.com/robinrodricks/FluentFTP
        /// </summary>
        /// <param name="context"></param>
        public override void Execute(ActivityContext context)
        {
            FtpLoad load = FtpLoad.GetConnect(this.ConfigName, context);
            string serverdir = context.ReplaceVar(this.ServerDir);

            if (!load.Sftp)
            {
                if (!load.ftpClient.IsConnected) load.ftpClient.Connect();
                load.ftpClient.DeleteDirectory(serverdir);
                context.WriteLog($"成功删除远程目录{serverdir}");
            }
            else
            {
                if (!load.sftpClient.IsConnected) load.sftpClient.Connect();
                load.sftpClient.DeleteDirectory(serverdir);
                context.WriteLog($"成功删除远程目录{serverdir}");
            }
        }

        public override void Validate(ActivityContext context)
        {
            if (string.IsNullOrEmpty(this.ConfigName)) throw new Exception("必须选择一个FTP连接");
            if (string.IsNullOrEmpty(this.ServerDir)) throw new Exception("服务端目录不能为空");
            FtpLoad.ValidateFtpConfig(this.ConfigName, context);
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
    }
}