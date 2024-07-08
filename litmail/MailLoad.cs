using litsdk;
using MailKit.Net.Proxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace litmail
{
    internal class MailLoad
    {
        public const string RefFile = "MailKit.dll,MimeKit.dll,BouncyCastle.Crypto.dll,System.Memory.dll,System.Buffers.dll,System.Runtime.CompilerServices.Unsafe.dll,System.Threading.Tasks.dll,System.Threading.Tasks.Extensions.dll";

        public static List<string> GetMailConfigActivities()
        {
            List<string> css = new List<string>();
            List<Activity> acts = litsdk.API.GetDesignActivityContext().GetActivities(typeof(MailConfigActivity).FullName);
            foreach (Activity activity in acts)
            {
                if (activity is MailConfigActivity ca)
                {
                    if (!string.IsNullOrEmpty(ca.ConfigName)) css.Add(ca.ConfigName);
                }
            }
            css = css.Distinct().ToList();
            return css;
        }

        public static MailConfigActivity GetMailConfigActivity(string ConfigName, ActivityContext context)
        {
            List<Activity> acts = context.GetActivities(typeof(MailConfigActivity).FullName);
            MailConfigActivity connect = null;
            foreach (Activity activity in acts)
            {
                if (activity is MailConfigActivity ca)
                {
                    if (ca.ConfigName == ConfigName)
                    {
                        connect = ca;
                        break;
                    }
                }
            }
            return connect;
        }


        public static void ValidateMailConfig(string ConfigName, ActivityContext context)
        {
            List<Activity> acts = context.GetActivities(typeof(MailConfigActivity).FullName);
            List<MailConfigActivity> connects = new List<MailConfigActivity>();
            foreach (Activity activity in acts)
            {
                if (activity is MailConfigActivity ca)
                {
                    if (ca.ConfigName == ConfigName)
                    {
                        connects.Add(ca);
                    }
                }
            }
            if (connects.Count == 0) throw new Exception($"找不到邮件配置：{ConfigName}，请检查");
            if (connects.Count > 1) throw new Exception($"邮件配置：{ConfigName} 配置了{connects.Count}次，请检查");
        }


        public static IProxyClient GetProxyClient(litsdk.ActivityContext context, string Proxy)
        {
            string proxy = context.ReplaceVar(Proxy).Trim();
            if (string.IsNullOrEmpty(proxy)) throw new Exception("代理信息为空");

            Uri u = new Uri(proxy);
            System.Net.NetworkCredential nc = new System.Net.NetworkCredential("anonymous","");
    
            if (!string.IsNullOrEmpty(u.UserInfo))
            {
                string[] arr = u.UserInfo.Split(':');
                if (arr.Length == 2)
                {
                    nc = new System.Net.NetworkCredential(arr[0], arr[1]);
                }
                else if (arr.Length == 1)
                {
                    nc = new System.Net.NetworkCredential(arr[0], "");
                }
            }

            switch (u.Scheme)
            {
                case "http":
                    MailKit.Net.Proxy.HttpProxyClient http = new MailKit.Net.Proxy.HttpProxyClient(u.Host, u.Port, nc);
                    return http;
                case "https":
                    MailKit.Net.Proxy.HttpsProxyClient https = new MailKit.Net.Proxy.HttpsProxyClient(u.Host, u.Port, nc);
                    return https;
                case "socket5":
                    MailKit.Net.Proxy.Socks5Client socks5 = new MailKit.Net.Proxy.Socks5Client(u.Host, u.Port, nc);
                    return socks5;
                case "socket4":
                    MailKit.Net.Proxy.Socks4Client socks4 = new MailKit.Net.Proxy.Socks4Client(u.Host, u.Port, nc);
                    return socks4;
            }

            throw new Exception("不支持的代理协议:" + Proxy);
        }
    }
}