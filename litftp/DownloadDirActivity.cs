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
    [litsdk.Action(Name = "下载文件夹", Category = "FTP", Order = 7, IsLinux = true, RefFile = "FluentFTP.dll,Renci.SshNet.dll", Description = "下载服务端某文件夹下所有文件或文件夹至本地某文件夹下")] 
    public class DownloadDirActivity : litsdk.ProcessActivity
    {
        public override string Name { get; set; } = "下载文件夹";

        /// <summary>
        /// 配置名称
        /// </summary>
        [Argument(Name = "FTP连接", ControlType = ControlType.ComboBox, Order = 1, Description = "要执行操作的FTP连接")]
        public string ConfigName { get; set; }//实际显示按ip:端口(数据库)，这里是guid

        [litsdk.Argument(Name = "服务端目录", ControlType = ControlType.TextBox, Description = "上传下载或是要操作的服务端目录，例如 /Data/2023/", Order = 10)]
        public string ServerDir { get; set; }

        [litsdk.Argument(Name = "本地目录", ControlType = ControlType.Directory, Description = "保存文件的本地目录，例如D:\\Data\\", Order = 11)]
        public string LocalDir { get; set; }

        [litsdk.Argument(Name = "文件存在时忽略下载", ControlType = ControlType.CheckBox, Description = "下载时，如果本地有文件，将会跳过，不选则直接覆盖原文件", Order = 12)]
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
                FtpLocalExists localExists = this.ExistIgnore ? FtpLocalExists.Skip : FtpLocalExists.Overwrite;

                if (!load.ftpClient.IsConnected) load.ftpClient.Connect();
                context.WriteLog($"开始下载文件夹 {serverdir} 至本地 {localdir}");
                List<string> ds = new List<string>();
                foreach (var v in load.ftpClient.GetListing(serverdir, FtpListOption.Recursive | FtpListOption.Auto))
                {
                    if (v.Type == FtpObjectType.File) ds.Add(v.FullName);
                }
                foreach (string df in ds)
                {
                    string dsave = System.IO.Path.Combine(localdir, df.Substring(serverdir.Length, df.Length - serverdir.Length));
                    load.ftpClient.DownloadFile(dsave, df, localExists);
                }

                context.WriteLog($"成功下载文件夹 {serverdir} 至本地 {localdir}");
            }
            else
            {
                if (!load.sftpClient.IsConnected) load.sftpClient.Connect();

                context.WriteLog($"开始下载文件夹 {serverdir} 至本地 {localdir}");
                foreach (var v in load.sftpClient.ListDirectory(serverdir))
                {
                    if (!v.IsDirectory)
                    {
                        string save = System.IO.Path.Combine(localdir, System.IO.Path.GetFileName(v.FullName));
                        using (var stream = System.IO.File.Open(save, FileMode.OpenOrCreate))
                        {
                            context.WriteLog($"开始下载 {v.FullName} 到本地目录{save}");
                            load.sftpClient.DownloadFile(v.FullName, stream);
                        }
                        context.WriteLog($"成功下载{v.FullName} 文件到本地{save}");
                    }
                }
                context.WriteLog($"成功下载文件夹 {serverdir} 至本地 {localdir}");
            }
        }

        public override void Validate(ActivityContext context)
        {
            if (string.IsNullOrEmpty(this.ServerDir)) throw new Exception("远程文件夹不能为空");
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