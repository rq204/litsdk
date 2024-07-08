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
    [litsdk.Action(Name = "下载文件", Category = "FTP", Order = 2, IsLinux = true, RefFile = "FluentFTP.dll,Renci.SshNet.dll", Description = "下载服务端某文件至本地文件夹下")] 
    public class DownloadFileActivity : litsdk.ProcessActivity
    {
        public override string Name { get; set; } = "下载文件";

        /// <summary>
        /// 配置名称
        /// </summary>
        [Argument(Name = "FTP连接", ControlType = ControlType.ComboBox, Order = 1, Description = "要执行操作的FTP连接")]
        public string ConfigName { get; set; }

        [litsdk.Argument(Name = "下载服务端文件列表", ControlType = ControlType.CheckBox, Description = "", Order = 3)]
        public bool DownMultFile { get; set; }

        [Argument(Name = "文件列表", ControlType = ControlType.Variable, Order = 5, Description = "")]
        public string ListVarName { get; set; }

        [litsdk.Argument(Name = "服务端文件", ControlType = ControlType.TextBox, Description = "需要操作的服务端文件路径，例如 /Month/0805/hi.txt", Order = 9)]
        public string ServerFile { get; set; }

        [litsdk.Argument(Name = "本地目录", ControlType = ControlType.Directory, Description = "保存文件的本地目录，例如D:\\Data\\", Order = 11)]
        public string LocalDir { get; set; }

        [litsdk.Argument(Name = "文件存在时忽略上传或下载", ControlType = ControlType.CheckBox, Description = "上传或下载时，如果本地有文件，将会跳过，不选则直接覆盖原文件", Order = 12)]
        public bool ExistIgnore { get; set; } = true;

        /// <summary>
        /// 常用操作 
        /// https://blog.csdn.net/yangwohenmai1/article/details/88571375
        /// https://github.com/robinrodricks/FluentFTP
        /// </summary>
        /// <param name="context"></param>
        public override void Execute(ActivityContext context)
        {
            string serverfile = context.ReplaceVar(this.ServerFile);
            string localDir = context.ReplaceVar(this.LocalDir);

            FtpLoad load = FtpLoad.GetConnect(this.ConfigName, context);

            if (!load.Sftp)
            {
                if (!load.ftpClient.IsConnected) load.ftpClient.Connect();
                FtpLocalExists ftpLocalExists = this.ExistIgnore ? FtpLocalExists.Skip : FtpLocalExists.Overwrite;
                if (this.DownMultFile)
                {
                    List<string> files = context.GetList(this.ListVarName);
                    if (files.Count > 0)
                    {
                        context.WriteLog($"开始下载远程{files.Count}个文件至{localDir}");
                        foreach (string df in files)
                        {
                            string localf = System.IO.Path.Combine(localDir, System.IO.Path.GetFileName(df));
                            load.ftpClient.DownloadFile(localf, df, ftpLocalExists);
                            context.WriteLog($"成功下载远程文件{df}至{localf}");
                        }
                        context.WriteLog($"成功下载远程{files.Count}个文件至{localDir}");
                    }
                    else
                    {
                        context.WriteLog($"远程列表文件为空，跳过下载");
                    }
                }
                else
                {
                    string localsave = System.IO.Path.Combine(localDir, System.IO.Path.GetFileName(serverfile));
                    context.WriteLog($"开始下载远程文件{serverfile}至{localsave}");
                    load.ftpClient.DownloadFile(localsave, serverfile, ftpLocalExists);
                    context.WriteLog($"成功下载远程文件{serverfile}至{localsave}");
                }
            }
            else
            {
                if (!load.sftpClient.IsConnected) load.sftpClient.Connect();

                if (this.DownMultFile)
                {
                    List<string> files = context.GetList(this.ListVarName);
                    if (files.Count > 0)
                    {
                        context.WriteLog($"开始下载远程{files.Count}个文件至{localDir}");
                        foreach (string save in files)
                        {
                            using (var stream = System.IO.File.Open(save, FileMode.OpenOrCreate))
                            {
                                context.WriteLog($"开始下载 {serverfile} 到本地目录{save}");
                                load.sftpClient.DownloadFile(serverfile, stream);
                            }
                            context.WriteLog($"成功下载{serverfile} 文件到本地{save}");
                        }
                        context.WriteLog($"成功下载远程{files.Count}个文件至{localDir}");
                    }
                    else
                    {
                        context.WriteLog($"远程列表文件为空，跳过下载");
                    }
                }
                else
                {
                    string save = System.IO.Path.Combine(localDir, System.IO.Path.GetFileName(serverfile));
                    using (var stream = System.IO.File.Open(save, FileMode.OpenOrCreate))
                    {
                        context.WriteLog($"开始下载 {serverfile} 到本地目录{save}");
                        load.sftpClient.DownloadFile(serverfile, stream);
                    }
                    context.WriteLog($"成功下载{serverfile} 文件到本地{save}");
                }
            }
        }


        public override void Validate(ActivityContext context)
        {
            if (this.DownMultFile)
            {
                if (string.IsNullOrEmpty(this.ListVarName)) throw new Exception("文件列表变量不能为空");
                if (!context.ContainsList(this.ListVarName)) throw new Exception($"文件列表变量不存在：{ListVarName}");
            }
            else
            {
                if (string.IsNullOrEmpty(this.ServerFile)) throw new Exception("删除文件不能为空");
            }
            FtpLoad.ValidateFtpConfig(this.ConfigName, context);
        }


        public override ControlStyle GetControlStyle(string field)
        {
            ControlStyle style = new ControlStyle() { Variables = ControlStyle.GetVariables(true, false, true) };

            switch (field)
            {
                case "ConfigName":
                    style.DropDownList = FtpLoad.GetFtpList();
                    break;
                case "ListVarName":
                    style.Visible = this.DownMultFile;
                    style.Variables = ControlStyle.GetVariables(false, true);
                    break;
                case "ServerFile":
                    style.Visible = !this.DownMultFile;
                    break;
            }

            return style;
        }
    }
}