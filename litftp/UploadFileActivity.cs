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
    [litsdk.Action(Name = "上传文件", Category = "FTP", Order = 1, IsLinux = true, RefFile = FtpLoad.RefFile, Description = "上传本地文件至服务端某个文件夹下")] 
    public class UploadFileActivity : litsdk.ProcessActivity
    {
        public override string Name { get; set; } = "上传文件";

        /// <summary>
        /// 配置名称
        /// </summary>
        [Argument(Name = "FTP连接", ControlType = ControlType.ComboBox, Order = 1, Description = "要执行操作的FTP连接")]
        public string ConfigName { get; set; }

        [litsdk.Argument(Name = "本地文件", ControlType = ControlType.File, Description = "需要操作的本地文件路径，形如 d:\\Ambe\\wf.jpg", Order = 8)]
        public string LocalFile { get; set; }
   
        [litsdk.Argument(Name = "服务端目录", ControlType = ControlType.TextBox, Description = "上传下载或是要操作的服务端目录，例如 /Data/2023/", Order = 10)]
        public string ServerDir { get; set; }

        [litsdk.Argument(Name = "文件存在时忽略上传", ControlType = ControlType.CheckBox, Description = "上传时，如果服务端有文件，将会跳过，不选则直接覆盖原文件", Order = 12)]
        public bool ExistIgnore { get; set; } = true;

        /// <summary>
        /// 常用操作 
        /// https://blog.csdn.net/yangwohenmai1/article/details/88571375
        /// https://github.com/robinrodricks/FluentFTP
        /// </summary>
        /// <param name="context"></param>
        public override void Execute(ActivityContext context)
        {
            string serverdir = context.ReplaceVar(this.ServerDir);
            string localfile = context.ReplaceVar(this.LocalFile);

            FtpLoad load = FtpLoad.GetConnect(this.ConfigName, context);

            context.WriteLog($"开始上传 {localfile} 到远程目录{serverdir}");

            if (!load.Sftp)
            {
                if (!load.ftpClient.IsConnected) load.ftpClient.Connect();

                FtpRemoteExists remoteExists = this.ExistIgnore ? FtpRemoteExists.Skip : FtpRemoteExists.Overwrite;

                string serverfile = System.IO.Path.Combine(serverdir, System.IO.Path.GetFileName(localfile));
                load.ftpClient.UploadFile(localfile, serverfile, remoteExists);
                context.WriteLog($"成功上传文件{localfile}到远程目录{serverdir}");
            }
            else
            {
                if (!load.sftpClient.IsConnected) load.sftpClient.Connect();
                System.IO.FileStream stream = System.IO.File.Open(localfile, FileMode.Open);
                string tofile = serverdir.TrimEnd() + "/" + System.IO.Path.GetFileName(localfile);
                load.sftpClient.UploadFile(stream, tofile, !this.ExistIgnore);
                context.WriteLog($"成功上传文件 {localfile}  至远程目录 {serverdir}");
            }
        }


        public override void Validate(ActivityContext context)
        {
            if (string.IsNullOrEmpty(this.LocalFile)) throw new Exception("本地文件不能为空");
            if (string.IsNullOrEmpty(this.ServerDir)) throw new Exception("远程目录不能为空");
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