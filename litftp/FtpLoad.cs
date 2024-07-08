using FluentFTP;
using litsdk;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace litftp
{
    internal class FtpLoad
    {
        public const string RefFile = "FluentFTP.dll,Renci.SshNet.dll,Microsoft.Extensions.Logging.Abstractions.dll";

        public SshClient sshClient = null;

        public FtpClient ftpClient = null;

        public SftpClient sftpClient = null;

        public string ConfigName = null;

        public bool Sftp = false;

        public static List<FtpLoad> FtpConnects = new List<FtpLoad>();

        public static FtpLoad GetConnect(string ConfigName,litsdk.ActivityContext context)
        {
            FtpLoad ftp = FtpConnects.Find(f => f.ConfigName == ConfigName);
            if (ftp == null)
            {
                List<Activity> acts = context.GetActivities(typeof(ConnectActivity).FullName);
                ConnectActivity connect = null;
                foreach (Activity activity in acts)
                {
                    if (activity is ConnectActivity ca)
                    {
                        if (ca.ConfigName == ConfigName)
                        {
                            connect = ca;
                            break;
                        }
                    }
                }
                if (connect == null) throw new Exception($"没有找到FTP配置:{ConfigName}");

                string server = context.ReplaceVar(connect.Server);
                string username = context.ReplaceVar(connect.UserName);
                string password = context.ReplaceVar(connect.Password);
                string portstr = context.ReplaceVar(connect.Port);

                int port = 0;
                if (!int.TryParse(portstr, out port))
                {
                    throw new Exception("FTP端口错误：" + portstr);
                }

                ftp = new FtpLoad() { ConfigName = ConfigName, Sftp = connect.Sftp };
                if (ftp.Sftp)
                {
                    ftp.sshClient = new SshClient(server, port, username, password);
                    ftp.sftpClient = new SftpClient(server, port, username, password);
                }
                else
                {
                    FtpConfig ftpConfig = new FtpConfig() { RetryAttempts = 5 };
                    if (connect.Passive) ftpConfig.DataConnectionType = FtpDataConnectionType.PASV;
                    ftp.ftpClient = new FtpClient(server, username, password, port, ftpConfig);
                }

                FtpConnects.Add(ftp);
            }

            return ftp;
        }

        public static void ValidateFtpConfig(string ConfigName, ActivityContext context)
        {
            List<Activity> acts = context.GetActivities(typeof(ConnectActivity).FullName);
            List<ConnectActivity> connects = new List<ConnectActivity>();
            foreach (Activity activity in acts)
            {
                if (activity is ConnectActivity ca)
                {
                    if (ca.ConfigName == ConfigName)
                    {
                        connects.Add(ca);
                    }
                }
            }
            if (connects.Count > 1) throw new Exception($"FTP配置：{ConfigName} 配置了{connects.Count}次，请检查");
        }

        /// <summary>
        /// 获取Excel列表
        /// </summary>
        /// <returns></returns>
        internal static List<string> GetFtpList()
        {
            List<string> results = new List<string>();
            litsdk.ActivityContext context = litsdk.API.GetDesignActivityContext();
            List<litsdk.Activity> lst = context.GetActivities(typeof(ConnectActivity).FullName);

            foreach (litsdk.Activity la in lst)
            {
                ConnectActivity oaa = la as ConnectActivity;
                if (oaa != null)
                {
                    if (!string.IsNullOrEmpty(oaa.ConfigName))
                    {
                        results.Add(oaa.ConfigName);
                    }
                }
            }
            results = results.Distinct().ToList();
            return results;
        }
    }
}
