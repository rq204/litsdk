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
    [litsdk.Action(Name = "删除文件", Category = "FTP", Order = 3, IsLinux = true, RefFile = "FluentFTP.dll,Renci.SshNet.dll", Description = "删除服务端指定文件")] 
    public class DeleteFileActivity : litsdk.ProcessActivity
    {
        public override string Name { get; set; } = "删除文件";

        /// <summary>
        /// 配置名称
        /// </summary>
        [Argument(Name = "FTP连接", ControlType = ControlType.ComboBox, Order = 1, Description = "要执行操作的FTP连接")]
        public string ConfigName { get; set; }//实际显示按ip:端口(数据库)，这里是guid

        [litsdk.Argument(Name = "服务端文件", ControlType = ControlType.TextBox, Description = "需要操作的服务端文件路径，例如 /Month/0805/hi.txt", Order = 9)]
        public string ServerFile { get; set; }

        /// <summary>
        /// 常用操作 
        /// https://blog.csdn.net/yangwohenmai1/article/details/88571375
        /// https://github.com/robinrodricks/FluentFTP
        /// </summary>
        /// <param name="context"></param>
        public override void Execute(ActivityContext context)
        {
            string serverfile = context.ReplaceVar(this.ServerFile);
            FtpLoad load = FtpLoad.GetConnect(this.ConfigName, context);

            if (!load.Sftp)
            {
                if (!load.ftpClient.IsConnected) load.ftpClient.Connect();
                load.ftpClient.DeleteFile(serverfile);
                context.WriteLog($"成功删除远程文件{serverfile}");
            }
            else
            {
                if (!load.sftpClient.IsConnected) load.sftpClient.Connect();
                load.sftpClient.DeleteFile(serverfile);
                context.WriteLog($"成功删除远程文件{serverfile}");
            }
        }


        public override void Validate(ActivityContext context)
        {
            if (string.IsNullOrEmpty(this.ServerFile)) throw new Exception("删除文件不能为空");
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