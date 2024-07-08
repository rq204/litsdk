using MailKit.Net.Imap;
using MailKit.Net.Pop3;
using MailKit.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using litsdk;
using System.Security.Policy;
using MailKit.Net.Proxy;

namespace litmail
{
    /// <summary>
    /// 邮箱配置
    /// </summary>
    [Action(Name = "配置邮箱", Category = "邮件", IsLinux = true, Order = 1, RefFile = MailLoad.RefFile, Description = "配置收件协议，邮件服务器，帐号密码等基本信息")] 
    public class MailConfigActivity : litsdk.ProcessActivity
    {
        /// <summary>
        /// 配置名称
        /// </summary>
        public override string Name { get; set; } = "配置邮箱";

        /// <summary>
        /// 配置名称
        /// </summary>
        [Argument(Name = "配置名称", ControlType = ControlType.TextBox, Order = 1, Description = "该邮箱的配置信息，在其它地方调用")]
        public string ConfigName { get; set; }

        /// <summary>
        /// 收件类型
        /// </summary>
        [Argument(Name = "收件协议", ControlType = ControlType.ComboBox, Order = 2, Description = "收件协议，支持Pop3和Imap以及Exchange")]
        public MailType MailType { get; set; } = MailType.POP3;

        /// <summary>
        /// POP3或IMAP服务器地址
        /// </summary>
        [Argument(Name = "POP3服务器", ControlType = ControlType.TextBox, Order = 3, Description = "POP3服务器地址")]
        public string Pop3Host { get; set; }

        [Argument(Name = "IMAP服务器", ControlType = ControlType.TextBox, Order = 4, Description = "IMAP服务器地址")]
        public string IMAPHost { get; set; }

        //[Argument(Name = "Exchange服务器", ControlType = ControlType.TextBox, Order = 5, Description = "Exchange服务器地址")]
        //public string EXHost { get; set; }

        /// <summary>
        /// POP3或IMAP端口
        /// </summary>
        [Argument(Name = "POP3端口", ControlType = ControlType.NumericUpDown, Order = 6, Description = "POP3端口")]
        public int Pop3Port { get; set; } = 995;

        /// <summary>
        /// POP3或IMAP端口
        /// </summary>
        [Argument(Name = "IMAP端口", ControlType = ControlType.NumericUpDown, Order = 7, Description = "IMAP端口")]
        public int IMAPPort { get; set; } = 465;

        /// <summary>
        /// 是否SSL
        /// </summary>
        [Argument(Name = "POP3收件协议为SSL", ControlType = ControlType.CheckBox, Order = 8, Description = "POP3收件协议为SSL")]
        public bool Pop3SSL { get; set; } = true;

        /// <summary>
        /// 是否SSL
        /// </summary>
        [Argument(Name = "IMAP收件协议为SSL", ControlType = ControlType.CheckBox, Order = 9, Description = "IMAP收件协议为SSL")]
        public bool IMAPSSL { get; set; } = true;

        /// <summary>
        /// SMTP服务器
        /// </summary>
        [Argument(Name = "SMTP服务器", ControlType = ControlType.TextBox, Order = 10, Description = "SMTP服务器地址")]
        public string SMTPHost { get; set; }

        /// <summary>
        /// SMTP端口
        /// </summary>
        [Argument(Name = "SMTP端口", ControlType = ControlType.NumericUpDown, Order = 11, Description = "SMTP端口")]
        public int SMTPPort { get; set; } = 465;

        /// <summary>
        /// SMTP为SSL
        /// </summary>
        [Argument(Name = "SMTP为SSL", ControlType = ControlType.CheckBox, Order = 12, Description = "SMTP为SSL")]
        public bool SMTPSSL { get; set; } = true;

        /// <summary>
        /// 如果服务器支持，用STARTTLS加密传输
        /// </summary>
        [Argument(Name = "STARTTLS加密传输", ControlType = ControlType.CheckBox, Order = 13, Description = "如果服务器支持，用STARTTLS加密传输")]
        public bool STARTTLS { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        [Argument(Name = "用户名", ControlType = ControlType.TextBox, Order = 14, Description = "用户名")]
        public string UserName { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        [Argument(Name = "密码", ControlType = ControlType.Password, Order = 15, Description = "密码")]
        public string Password { get; set; }

        [Argument(Name = "启用代理", ControlType = ControlType.ToggleSwitch, Order = 18, Description = "通过代理发送邮件")]
        public bool UseProxy { get; set; }

        [Argument(Name = "代理信息", ControlType = ControlType.TextBox, Order = 20, Description = "支持http,socket5,socket4三种代理协议，填写方式如 http://127.0.0.1:8080 或 socket5://proxy.com:6666")]
        public string Proxy { get; set; }

        //public void TestConnect()
        //{
        //    litsdk.ActivityContext context = litsdk.API.GetDesignActivityContext();
        //    string smtp_host = context.ReplaceVar(this.SMTPHost);
        //    string username = context.ReplaceVar(this.UserName);
        //    string password = context.ReplaceVar(this.Password);

        //    using (var smtp = new MailKit.Net.Smtp.SmtpClient())
        //    {
        //        smtp.ServerCertificateValidationCallback = (s, c, h, e) => true;
        //        if (this.STARTTLS)
        //        {
        //            smtp.Connect(smtp_host, this.SMTPPort, SecureSocketOptions.StartTls);
        //        }
        //        else
        //        {
        //            if (this.SMTPSSL)
        //            {
        //                smtp.Connect(smtp_host, this.SMTPPort, SecureSocketOptions.SslOnConnect);
        //            }
        //            else
        //            {
        //                smtp.Connect(smtp_host, this.SMTPPort, SecureSocketOptions.Auto);
        //            }
        //        }

        //        smtp.Authenticate(username, password);
        //        smtp.Disconnect(true);
        //    };

        //    string pie_host = context.ReplaceVar(this.Pop3IMAPEXHost);
        //    //string pie_ports = context.ReplaceVar(this.Pop3IMAPPort);
        //    //int pie_port = 0;
        //    //if (!int.TryParse(pie_ports, out pie_port)) throw new Exception("收件端口错误：" + pie_ports);

        //    if (this.MailType == MailType.POP3)
        //    {
        //        using (var client = new Pop3Client())
        //        {
        //            // For demo-purposes, accept all SSL certificates
        //            client.ServerCertificateValidationCallback = (s, c, h, e) => true;
        //            client.Connect(pie_host, this.Pop3IMAPPort, this.Pop3IMAPSSL);
        //            client.Authenticate(username, password);
        //            client.Disconnect(true);
        //        }
        //    }
        //    else if (this.MailType == MailType.IMAP)
        //    {
        //        using (var client = new ImapClient())
        //        {
        //            // For demo-purposes, accept all SSL certificates
        //            client.ServerCertificateValidationCallback = (s, c, h, e) => true;
        //            client.Connect(pie_host, this.Pop3IMAPPort, this.Pop3IMAPSSL);
        //            client.Authenticate(username, password);
        //            client.Disconnect(true);
        //        }
        //    }
        //}


        public override void Execute(ActivityContext context)
        {
            switch (this.MailType)
            {
                case MailType.POP3:
                    string pop3host = context.ReplaceVar(this.Pop3Host);
                    if (string.IsNullOrEmpty(pop3host)) throw new Exception("POP3服务器值不能为空");
                    if (this.Pop3Port == 0) throw new Exception("POP3服务器端口不能为空");
                    break;
                case MailType.IMAP:
                    string imaphost = context.ReplaceVar(this.IMAPHost);
                    if (string.IsNullOrEmpty(imaphost)) throw new Exception("IMAP服务器值不能为空");
                    if (this.IMAPPort == 0) throw new Exception("IMAP服务器端口不能为空");
                    break;
            }
            context.WriteLog($"邮箱配置:{this.Name}载入成功");
        }

        public override void Validate(ActivityContext context)
        {
            switch (this.MailType)
            {
                case MailType.POP3:
                    if (string.IsNullOrEmpty(this.Pop3Host)) throw new Exception("POP3服务器不能为空");
                    if (this.Pop3Port == 0) throw new Exception("POP3服务器端口不能为空");
                    break;
                case MailType.IMAP:
                    if (string.IsNullOrEmpty(this.IMAPHost)) throw new Exception("IMAP服务器不能为空");
                    if (this.IMAPPort == 0) throw new Exception("IMAP服务器端口不能为空");
                    break;
                    //case MailType.Exchange:
                    //    if (string.IsNullOrEmpty(this.EXHost)) throw new Exception("Exchange服务器不能为空");
                    //    break;
            }

            if (this.UseProxy && string.IsNullOrEmpty(Proxy)) throw new Exception("设置了代理但是代理配置为空");
        }

        public override ControlStyle GetControlStyle(string field)
        {
            ControlStyle style = new ControlStyle() { Variables = ControlStyle.GetVariables(true, false, true) };
            switch (field)
            {
                case "Pop3Host":
                case "Pop3Port":
                case "Pop3SSL":
                    style.Visible = this.MailType == MailType.POP3;
                    break;
                case "IMAPHost":
                case "IMAPSSL":
                case "IMAPPort":
                    style.Visible = this.MailType == MailType.IMAP;
                    break;
                case "Proxy":
                    style.Visible = this.UseProxy;
                    style.PlaceholderText = "http://127.0.0.1:8080 或 socket5://proxy.com:6666";
                    break;
                    //case "EXHost":
                    //    style.Visible = this.MailType == MailType.Exchange;
                    //    break;
            }
            return style;
        }
    }
}