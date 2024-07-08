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
    [litsdk.Action(Name = "获取子文件夹", Category = "FTP", Order = 5, IsLinux = true, RefFile = "FluentFTP.dll,Renci.SshNet.dll", Description = "获取某文件夹下子文件夹列表")] 
    public class ListDirDirActivity : litsdk.ProcessActivity
    {
        public override string Name { get; set; } = "获取子文件夹";

        /// <summary>
        /// 配置名称
        /// </summary>
        [Argument(Name = "FTP连接", ControlType = ControlType.ComboBox, Order = 1, Description = "要执行操作的FTP连接")]
        public string ConfigName { get; set; }//实际显示按ip:端口(数据库)，这里是guid

        [litsdk.Argument(Name = "服务端目录", ControlType = ControlType.TextBox, Description = "上传下载或是要操作的服务端目录，例如 /Data/2023/", Order = 10)]
        public string ServerDir { get; set; }

        [litsdk.Argument(Name = "保存结果至", ControlType = ControlType.Variable, Description = "获取的文件夹列表至列表变量", Order = 13)]
        public string ListVarName { get; set; }

        /// <summary>
        /// 常用操作 
        /// https://blog.csdn.net/yangwohenmai1/article/details/88571375
        /// https://github.com/robinrodricks/FluentFTP
        /// </summary>
        /// <param name="context"></param>
        public override void Execute(ActivityContext context)
        {
            string serverDir = context.ReplaceVar(this.ServerDir);
            FtpLoad load = FtpLoad.GetConnect(this.ConfigName, context);

            if (!load.Sftp)
            {
                if (!load.ftpClient.IsConnected) load.ftpClient.Connect();

                load.ftpClient.SetWorkingDirectory(serverDir);

                List<string> ds = new List<string>();
                foreach (var v in load.ftpClient.GetListing())
                {
                    if (v.Type == FtpObjectType.Directory) ds.Add(v.FullName);
                }

                context.SetVarList(this.ListVarName, ds);
                context.WriteLog($"成功获取{ds.Count}个远程文件夹");
            }
            else
            {
                if (!load.sftpClient.IsConnected) load.sftpClient.Connect();
                List<string> ds = new List<string>();
                foreach (var v in load.sftpClient.ListDirectory(serverDir))
                {
                    if (v.IsDirectory) ds.Add(v.FullName);
                }
                context.WriteLog($"成功获取{ds.Count}个远程文件夹");
            }
        }


        public override void Validate(ActivityContext context)
        {
            if (string.IsNullOrEmpty(this.ServerDir)) throw new Exception("远程文件夹不能为空");
            FtpLoad.ValidateFtpConfig(this.ConfigName, context);
        }


        public override ControlStyle GetControlStyle(string field)
        {
            ControlStyle style = new ControlStyle() { Variables = litsdk.ControlStyle.GetVariables(true, false, true) };
            switch (field)
            {
                case "ConfigName":
                    style.DropDownList = FtpLoad.GetFtpList();
                    break;
                case "ListVarName":
                    style.Variables = ControlStyle.GetVariables(false, true);
                    break;
            }
            return style;
        }
    }
}