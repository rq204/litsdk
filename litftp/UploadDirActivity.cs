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
    [litsdk.Action(Name = "上传文件夹", Category = "FTP", Order = 6, IsLinux = true, RefFile = "FluentFTP.dll,Renci.SshNet.dll", Description = "上传本地文件夹下所有文件或文件夹至服务端某文件夹下")]
    public class UploadDirActivity : litsdk.ProcessActivity
    {
        public override string Name { get; set; } = "上传文件夹";

        /// <summary>
        /// 配置名称
        /// </summary>
        [Argument(Name = "FTP连接", ControlType = ControlType.ComboBox, Order = 1, Description = "要执行操作的FTP连接")]
        public string ConfigName { get; set; }//实际显示按ip:端口(数据库)，这里是guid

        [litsdk.Argument(Name = "服务端目录", ControlType = ControlType.TextBox, Description = "上传要操作的服务端目录，例如 /Data/2023/", Order = 10)]
        public string ServerDir { get; set; }

        [litsdk.Argument(Name = "本地目录", ControlType = ControlType.Directory, Description = "保存文件的本地目录，例如D:\\Data\\", Order = 11)]
        public string LocalDir { get; set; }

        [litsdk.Argument(Name = "文件存在时忽略下载", ControlType = ControlType.CheckBox, Description = "上传时，如果远程有文件，将会跳过，不选则直接覆盖原文件", Order = 12)]
        public bool ExistIgnore { get; set; } = true;

        /// <summary>
        /// 常用操作 
        /// https://blog.csdn.net/yangwohenmai1/article/details/88571375
        /// https://github.com/robinrodricks/FluentFTP
        /// </summary>
        /// <param name="context"></param>
        public override void Execute(ActivityContext context)
        {
            string localdir = context.ReplaceVar(this.LocalDir);
            string serverdir = context.ReplaceVar(this.ServerDir);

            FtpLoad load = FtpLoad.GetConnect(this.ConfigName, context);

            if (!load.Sftp)
            {
                FtpRemoteExists remoteExists = this.ExistIgnore ? FtpRemoteExists.Skip : FtpRemoteExists.Overwrite;

                if (!load.ftpClient.IsConnected) load.ftpClient.Connect();

                string[] files = System.IO.Directory.GetFiles(localdir, "*.*", SearchOption.AllDirectories);
                context.WriteLog($"开始上传本地文件夹 {localdir} 中{files.Length}个文件至服务器 {serverdir}");
                foreach (string ufile in files)
                {
                    string serPath = serverdir.TrimEnd('/') + "/" + System.IO.Path.GetFileName(localdir) + "/" + ufile.Substring(localdir.Length, ufile.Length - localdir.Length).TrimStart('\\');
                    context.WriteLog($"开始上传本地文件：{ufile}至 ：{serPath}");
                    load.ftpClient.UploadFile(ufile, serPath, remoteExists, true);
                    context.WriteLog($"成功上传本地文件：{ufile}至 ：{serPath}");
                }
                context.WriteLog($"成功上传本地文件夹 {localdir} 中{files.Length}个文件至服务器 {serverdir}");
            }
            else
            {
                if (!load.sftpClient.IsConnected) load.sftpClient.Connect();

                context.WriteLog($"开始上传本地文件夹 {localdir} 至服务器 {serverdir}");

                foreach (var v in System.IO.Directory.GetFiles(localdir))
                {
                    string save = serverdir.TrimEnd('/') + "/" + System.IO.Path.GetFileName(v);
                    using (var stream = System.IO.File.Open(v, FileMode.Open))
                    {
                        context.WriteLog($"开始上传 {v} 到服务器{save}");
                        load.sftpClient.UploadFile(stream, save);
                    }
                    context.WriteLog($"成功上传{v} 文件到服务器{save}");
                }
                context.WriteLog($"成功上传本地文件夹 {localdir} 至服务器 {serverdir}");
            }
        }


        public override void Validate(ActivityContext context)
        {
            if (string.IsNullOrEmpty(this.ServerDir)) throw new Exception("服务器文件夹不能为空");
            if (string.IsNullOrEmpty(this.LocalDir)) throw new Exception("本地文件夹不能为空");
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