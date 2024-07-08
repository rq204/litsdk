using litsdk;
using MailKit.Net.Imap;
using MailKit.Net.Pop3;
using MimeKit;
using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace litmail
{
    /// <summary>
    /// http://www.mimekit.net/docs/html/T_MailKit_Net_Pop3_Pop3Client.htm
    /// http://www.mimekit.net/docs/html/T_MailKit_Net_Imap_ImapClient.htm
    /// </summary>
    [litsdk.Action(Name = "收取邮件", Category = "邮件", IsLinux = true, RefFile = MailLoad.RefFile, Order = 2, Description = "设置特定匹配规则获取符合条件的邮件信息")] 
    public class ReceiveMailActivity : litsdk.ProcessActivity
    {
        public override string Name { get; set; } = "收取邮件";

        [Argument(Name = "选择邮箱配置", ControlType = ControlType.ComboBox, Order = 1, Description = "选择已经配置好的邮箱配置")]
        public string ConfigName { get; set; }

        [Argument(Name = "邮件匹配条件", ControlType = ControlType.ComboBox, Order = 2, Description = "按匹配条件获取邮件")]
        public MailFilter MailFilter { get; set; }

        /// <summary>
        /// 邮件标题包含
        /// </summary>
        [Argument(Name = "邮件匹配字符", ControlType = ControlType.TextBox, Order = 3, Description = "邮件匹配字符")]
        public string ContainStr { get; set; }

        /// <summary>
        /// 将所有符合条件邮件存入表格变量
        /// </summary>
        [Argument(Name = "将所有符合条件邮件存入表格变量,不选只处理一封邮件", ControlType = ControlType.CheckBox, Order = 4, Description = "将所有符合条件邮件存入表格变量")]
        public bool SaveAllMail { get; set; } = false;

        /// <summary>
        /// 保存附件
        /// </summary>
        [Argument(Name = "下载所有附件到本地", ControlType = ControlType.CheckBox, Order = 5, Description = "将所有附件保存至本地")]
        public bool SaveAttachments { get; set; }

        /// <summary>
        /// 邮件标题存入
        /// </summary>
        [Argument(Name = "邮件标题存入", ControlType = ControlType.Variable, Order = 6, Description = "邮件标题存入")]
        public string VarSubject { get; set; }

        /// <summary>
        /// 收件人存入
        /// </summary>
        [Argument(Name = "收件人存入", ControlType = ControlType.Variable, Order = 7, Description = "收件人存入")]
        public string VarSender { get; set; }

        /// <summary>
        /// 收件人存入
        /// </summary>
        [Argument(Name = "收件时间存入", ControlType = ControlType.Variable, Order = 8, Description = "收件时间存入")]
        public string VarTime { get; set; }

        /// <summary>
        /// 邮件正文存入
        /// </summary>
        [Argument(Name = "邮件正文存入", ControlType = ControlType.Variable, Order = 9, Description = "邮件正文存入")]
        public string VarBody { get; set; }

        /// <summary>
        /// 邮件附件存入
        /// </summary>
        [Argument(Name = "邮件附件存入", ControlType = ControlType.Variable, Order = 10, Description = "邮件附件存入")]
        public string VarAttachments { get; set; }

        /// <summary>
        /// 所有邮件存入表格
        /// </summary>
        [Argument(Name = "表格变量", ControlType = ControlType.Variable, Order = 15, Description = "所有邮件信息保存至一个表格变量，表头信息为\"标题，发件人,收件时间，正文，附件\"，附件是下载到本地的路径，多个以|分割")]
        public string VarAllMail { get; set; }

        [Argument(Name = "邮件内容使用HTML方式读取", ControlType = ControlType.CheckBox, Order = 19, Description = "邮件内容使用HTML方式读取")]
        public bool UseHtmlBody { get; set; }

        [Argument(Name = "获取邮件成功后删除原邮件", ControlType = ControlType.CheckBox, Order = 22, Description = "将服务器上的邮件删除掉")]
        public bool DeleteAfterGet { get; set; }

        public override void Execute(ActivityContext context)
        {
            MailConfigActivity config = MailLoad.GetMailConfigActivity(this.ConfigName, context);

            string username = context.ReplaceVar(config.UserName);
            string password = context.ReplaceVar(config.Password);

            int total = 0;

            List<MimeKit.MimeMessage> msgs = new List<MimeKit.MimeMessage>();

            ImapClient imapClient = null;
            Pop3Client pop3Client = null;
            string result = "";

            try
            {
                if (config.MailType == MailType.POP3)
                {
                    pop3Client = new Pop3Client();

                    if (config.UseProxy)
                    {
                        pop3Client.ProxyClient = MailLoad.GetProxyClient(context, config.Proxy);
                    }
                    // For demo-purposes, accept all SSL certificates
                    pop3Client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                    string pop3_host = context.ReplaceVar(config.Pop3Host);

                    pop3Client.Connect(pop3_host, config.Pop3Port, config.Pop3SSL);
                    pop3Client.Authenticate(username, password);
                    total = pop3Client.Count;

                    for (int i = 0; i < pop3Client.Count; i++)
                    {
                        var message = pop3Client.GetMessage(i);
                        if (this.FindNeed(context, message))
                        {
                            msgs.Add(message);
                            if (!this.SaveAllMail) break;
                        }
                    }
                }
                else if (config.MailType == MailType.IMAP)
                {
                    imapClient = new ImapClient();

                    if (config.UseProxy)
                    {
                        imapClient.ProxyClient = MailLoad.GetProxyClient(context, config.Proxy);
                    }

                    string imap_host = context.ReplaceVar(config.IMAPHost);
                    // For demo-purposes, accept all SSL certificates
                    imapClient.ServerCertificateValidationCallback = (s, c, h, e) => true;
                    imapClient.Connect(imap_host, config.IMAPPort, config.IMAPSSL);

                    var clientImplementation = new ImapImplementation
                    {
                        Name = "MailKit4LitRPA",
                        Version = "1.0.0"
                    };
                    var serverImplementation = imapClient.Identify(clientImplementation);

                    imapClient.Authenticate(username, password);

                    var inbox = imapClient.Inbox;
                    inbox.Open(MailKit.FolderAccess.ReadWrite);

                    //Console.WriteLine("Total messages: {0}", inbox.Count);
                    //Console.WriteLine("Recent messages: {0}", inbox.Recent);
                    total = inbox.Count;

                    for (int i = 0; i < inbox.Count; i++)
                    {
                        var message = inbox.GetMessage(i);
                        if (this.FindNeed(context, message))
                        {
                            msgs.Add(message);
                            if (!this.SaveAllMail) break;
                        }
                    }
                }

                if (this.SaveAllMail)
                {
                    System.Data.DataTable table = new System.Data.DataTable();
                    foreach (string t in new string[] { "标题", "发件人", "收件时间", "正文", "附件" }) table.Columns.Add(t);
                    foreach (MimeMessage mm in msgs)
                    {
                        System.Data.DataRow dr = table.NewRow();
                        dr["标题"] = mm.Subject;
                        dr["发件人"] = this.getSender(mm);
                        dr["收件时间"] = mm.Date.DateTime.ToString("yyyy-MM-dd HH:mm:ss");
                        dr["正文"] = this.UseHtmlBody?mm.HtmlBody: mm.TextBody;

                        dr["附件"] = "";
                        if (this.SaveAttachments)
                        {
                            List<string> files = this.DownFiles(mm);
                            dr["附件"] = string.Join("|", files);
                        }

                        table.Rows.Add(dr);
                    }
                    context.Variables.Find((ff) => ff.Name == this.VarAllMail).TableValue = table;
                    result = $"总计获取到{total}条数据并保存{msgs.Count}条至表格变量{this.VarAllMail}";
                }
                else
                {
                    string varSubject = "", varBody = "", varSender = "",varTime="";
                    List<string> varAttachments = new List<string>();

                    if (msgs.Count > 0)
                    {
                        varSubject = msgs[0].Subject;

                        if (this.UseHtmlBody) varBody = msgs[0].HtmlBody;
                        else
                        {
                            if (msgs[0].TextBody is null)
                            {
                                varBody = msgs[0].GetTextBody(MimeKit.Text.TextFormat.Html);
                            }
                            else
                            {
                                varBody = msgs[0].TextBody;
                            }
                        }

                        varSender = this.getSender(msgs[0]);

                        varTime = msgs[0].Date.DateTime.ToString("yyyy-MM-dd HH:mm:ss");

                        if (this.SaveAttachments)
                        {
                            varAttachments = this.DownFiles(msgs[0]);
                        }
                        result = $"找到{total}条数据并取符合条件1条";
                    }
                    else
                    {
                        result = "没有找到符合条件的邮件，所有变量设置为空";
                    }
                    if (!string.IsNullOrEmpty(this.VarSubject)) context.SetVarStr(this.VarSubject, varSubject);
                    if (!string.IsNullOrEmpty(this.VarSender)) context.SetVarStr(this.VarSender, varSender);
                    if (!string.IsNullOrEmpty(this.VarBody)) context.SetVarStr(this.VarBody, varBody);
                    if (!string.IsNullOrEmpty(this.VarTime)) context.SetVarStr(this.VarTime, varTime);

                    if (this.SaveAttachments)
                    {
                        if (context.ContainsStr(this.VarAttachments))
                        {
                            string f = varAttachments.Count > 0 ? varAttachments[0] : "";
                            context.SetVarStr(this.VarAttachments, f);
                        }
                        else if (context.ContainsList(this.VarAttachments)) context.SetVarList(this.VarAttachments, varAttachments);
                    }
                }

                if (this.DeleteAfterGet)
                {
                    context.WriteLog("获取后删除功能开发中..");
                    foreach (MimeMessage mm in msgs)
                    {
                        // pop3Client.DeleteMessage()
                    }
                }
            }
            finally
            {
                try
                {
                    if (pop3Client != null) pop3Client.Disconnect(true);
                }
                catch { }
                try
                {
                    if (imapClient != null) imapClient.Disconnect(true);
                }
                catch { }
            }
            context.WriteLog(result);
        }

        private string getSender(MimeKit.MimeMessage mm)
        {
            string varSender = "";
            if (mm.Sender != null)
            {
                varSender = mm.Sender.Address;
            }
            else
            {
                IEnumerator myEnum = mm.From.GetEnumerator();
                while (myEnum.MoveNext())
                {
                    if (myEnum.Current != null && myEnum.Current is MimeKit.InternetAddress mi)
                    {
                        if (mi.ToString().Contains("@"))
                        {
                            varSender = mi.GetType().GetProperty("Address").GetValue(mi).ToString();
                            break;
                        }
                    }
                }
            }
            return varSender;
        }

        private List<string> DownFiles(MimeKit.MimeMessage mine)
        {
            List<string> downs = new List<string>();
            string outDirPath = AppDomain.CurrentDomain.BaseDirectory + "Temp~\\";
            foreach (var attachment in mine.Attachments)
            {
                if (attachment.IsAttachment) continue;
                var fileName = attachment.ContentDisposition?.FileName ?? attachment.ContentType.Name;
                string filePath = Path.Combine(outDirPath, fileName);

                using (var stream = File.Create(filePath))
                {
                    if (attachment is MessagePart)
                    {
                        var rfc822 = (MessagePart)attachment;
                        rfc822.Message.WriteTo(stream);
                    }
                    else
                    {
                        var part = (MimePart)attachment;
                        part.Content.DecodeTo(stream);
                    }
                }
                downs.Add(filePath);
            }
            return downs;
        }

        /// <summary>
        /// 查找符合条件邮件
        /// </summary>
        /// <param name="context"></param>
        /// <param name="mime"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private bool FindNeed(litsdk.ActivityContext context, MimeKit.MimeMessage mime)
        {
            ///现在对结果邮件进行筛选
            List<MimeMessage> finds = new List<MimeMessage>();
            string filterStr = context.ReplaceVar(this.ContainStr);
            bool find = false;
            switch (this.MailFilter)
            {
                case MailFilter.SubjectContains:
                    find = mime.Subject.Contains(filterStr);
                    break;
                case MailFilter.SenderContains:
                    find = mime.Sender.Address.Contains(filterStr);
                    break;
                case MailFilter.BodyContains:
                    find = mime.TextBody.Contains(filterStr);
                    break;
                case MailFilter.LaterThan:
                    DateTime later = DateTime.Now;
                    if (!DateTime.TryParse(filterStr, out later))
                    {
                        throw new Exception("时间过滤设置错误");
                    }
                    if (mime.Date.DateTime > later) find = true;
                    break;
            }
            return find;
        }

        public override void Validate(ActivityContext context)
        {
            if (string.IsNullOrEmpty(this.ConfigName)) throw new Exception("邮件配置不能为空");
            if (string.IsNullOrEmpty(this.ContainStr)) throw new Exception("邮件匹配条件不能为空");
            if (this.SaveAllMail)
            {
                if (string.IsNullOrEmpty(this.VarAllMail)) throw new Exception("所有邮件存入变量不能为空");
                if (!context.ContainsTable(this.VarAllMail)) throw new Exception($"不存在表格变量 {this.VarAllMail}");
            }
            else
            {
                if (string.IsNullOrEmpty(this.VarSubject) && string.IsNullOrEmpty(this.VarSender) && string.IsNullOrEmpty(this.VarBody)) throw new Exception("获取的数据必须选择至少一个保存变量");

                if (!string.IsNullOrEmpty(this.VarSubject) && !context.ContainsStr(this.VarSubject)) throw new Exception($"不存标题存入字符变量 {this.VarSubject}");

                if (!string.IsNullOrEmpty(this.VarSender) && !context.ContainsStr(this.VarSender)) throw new Exception($"不存发件人存入字符变量 {this.VarSender}");

                if (!string.IsNullOrEmpty(this.VarTime) && !context.ContainsStr(this.VarTime)) throw new Exception($"不存发件时间存入字符变量 {this.VarTime}");

                if (!string.IsNullOrEmpty(this.VarBody) && !context.ContainsStr(this.VarBody)) throw new Exception($"不存内容存入字符变量 {this.VarBody}");

                if (this.SaveAttachments && !string.IsNullOrEmpty(this.VarAttachments))
                {
                    if (!context.ContainsStr(this.VarAttachments) && !context.ContainsList(this.VarAttachments)) throw new Exception("附件必须保存至字符或列表变量当中");
                }
            }
            MailLoad.ValidateMailConfig(ConfigName, context);
        }

        public override ControlStyle GetControlStyle(string field)
        {
            ControlStyle style = new ControlStyle() { Variables = ControlStyle.GetVariables(true, false, true) };
            switch (field)
            {
                case "ConfigName":
                    style.DropDownList = MailLoad.GetMailConfigActivities();
                    break;
                case "VarSubject":
                case "VarSender":
                case "VarBody":
                case "VarTime":
                    style.Visible = !this.SaveAllMail;
                    style.Variables = ControlStyle.GetVariables(true);
                    break;
                case "VarAttachments":
                    style.Visible = !this.SaveAllMail && this.SaveAttachments;
                    style.Variables = ControlStyle.GetVariables(true, true);
                    break;
                case "VarAllMail":
                    style.Visible = this.SaveAllMail;
                    style.Variables = ControlStyle.GetVariables(false, false, false, true);
                    break;
            }
            return style;
        }

    }
}