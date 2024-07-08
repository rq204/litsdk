using litsdk;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace litmail
{
    [litsdk.Action(Name = "发送邮件", Category = "邮件", IsLinux = true, RefFile = MailLoad.RefFile, Order = 3, Description = "设置收件人，邮件标题内容附件等信息并发送邮件")] 
    public class SendMailActivity : litsdk.ProcessActivity
    {
        public override string Name { get; set; } = "发送邮件";

        /// <summary>
        /// 配置名称
        /// </summary>
        [Argument(Name = "选择邮箱配置", ControlType = ControlType.ComboBox, Order = 1, Description = "选择已经配置好的邮箱配置")]
        public string ConfigName { get; set; }

        /// <summary>
        /// 收件人邮箱
        /// </summary>
        [Argument(Name = "收件人邮箱", ControlType = ControlType.TextBox, Order = 2, Description = "收件人邮箱")]
        public string MailTo { get; set; }

        [Argument(Name = "设置收件人和发件人昵称", ControlType = ControlType.CheckBox, Order = 3, Description = "默认收件人和发件人昵称为空的，选中可以设置")]
        public bool SetNick { get; set; }

        /// <summary>
        /// 发送者昵称
        /// </summary>
        [Argument(Name = "发件人昵称", ControlType = ControlType.TextBox, Order = 5, Description = "发件人昵称")]
        public string SenderNick { get; set; }

        /// <summary>
        /// 收件人昵称
        /// </summary>
        [Argument(Name = "收件人昵称", ControlType = ControlType.TextBox, Order = 7, Description = "收件人昵称")]
        public string ReceiverNick { get; set; }

        /// <summary>
        /// 邮件标题
        /// </summary>
        [Argument(Name = "邮件标题", ControlType = ControlType.TextBox, Order = 9, Description = "邮件标题")]
        public string Subject { get; set; }

        /// <summary>
        /// 邮件内容
        /// </summary>
        [Argument(Name = "邮件内容", ControlType = ControlType.TextArea, Order = 11, Description = "邮件内容，以Html的形式发送，支持Html代码")]
        public string Body { get; set; }

        [Argument(Name = "发送附件", ControlType = ControlType.CheckBox, Order = 13, Description = "列表变量可以一次发一个或多个附件")]
        public bool SendAttachments { get; set; }

        [Argument(Name = "邮件附件", ControlType = ControlType.Variable, Order = 15, Description = "邮件附件，可以设置字符或列表变量,注意值必须是文件的本地完整路径")]
        /// <summary>
        /// 附件变量，可字符列表
        /// </summary>
        public string AttachmentsVarName { get; set; }

        public override void Execute(ActivityContext context)
        {
            MailConfigActivity config = MailLoad.GetMailConfigActivity(this.ConfigName, context);

            string subject = context.ReplaceVar(this.Subject);
            string senderNidck = context.ReplaceVar(this.SenderNick);
            string body = context.ReplaceVar(this.Body);

            TextFormat format = (body.Contains("<") || body.Contains("\n")) ? TextFormat.Html : TextFormat.Text;
            MimeEntity mbody = new TextPart(format) { Text = body };

            var msgSend = new MimeMessage
            {
                Subject = subject,
            };

            string username = context.ReplaceVar(config.UserName);
            string password = context.ReplaceVar(config.Password);

            msgSend.From.Add(new MailboxAddress(senderNidck, username));

            if (!string.IsNullOrEmpty(this.AttachmentsVarName))
            {
                List<string> attachs = new List<string>();
                var multipart = new Multipart("mixed");

                if (context.ContainsStr(this.AttachmentsVarName))
                {
                    attachs.Add(context.GetStr(this.AttachmentsVarName));
                }
                else
                {
                    attachs = context.GetList(this.AttachmentsVarName);
                }

                if (attachs.Count == 0) throw new Exception("附件数不能为空");

                List<string> images = new List<string>() { "gif", "png", "jpg", "bmp" };
                foreach (string f in attachs)
                {
                    if (!System.IO.File.Exists(f)) throw new Exception("附件文件不存在：" + f);
                    string fname = Path.GetFileName(f);
                    // create an image attachment for the file located at path
                    var attachment = new MimePart();
                    string ext = images.Find(x => fname.EndsWith("." + x));
                    if (!string.IsNullOrEmpty(ext))
                    {
                        attachment = new MimePart("image", ext);
                    }
                    attachment.Content = new MimeContent(File.OpenRead(f), ContentEncoding.Binary);
                    attachment.ContentDisposition = new ContentDisposition(ContentDisposition.Attachment);
                    attachment.ContentTransferEncoding = ContentEncoding.Base64;
                    //attachment.FileName = fname;

                    //https://www.bbsmax.com/A/mo5kkeQ45w/
                    var charset = "GB18030";
                    attachment.ContentType.Parameters.Add(charset, "name", fname);
                    attachment.ContentDisposition.Parameters.Add(charset, "filename", fname);

                    foreach (var param in attachment.ContentDisposition.Parameters)
                        param.EncodingMethod = ParameterEncodingMethod.Rfc2047;
                    foreach (var param in attachment.ContentType.Parameters)
                        param.EncodingMethod = ParameterEncodingMethod.Rfc2047;

                    multipart.Add(attachment);
                }

                // now create the multipart/mixed container to hold the message text and the
                // image attachment
                multipart.Add(mbody);
                msgSend.Body = multipart;
            }
            else
            {
                msgSend.Body = mbody;
            }

            string receiverNick = context.ReplaceVar(this.ReceiverNick);
            string receiverMail = context.ReplaceVar(this.MailTo);

            msgSend.To.Add(new MailboxAddress(receiverNick, receiverMail));

            string host = context.ReplaceVar(config.SMTPHost);

            int port = config.SMTPPort;

            using (var smtp = new MailKit.Net.Smtp.SmtpClient())
            {
                smtp.ServerCertificateValidationCallback = (s, c, h, e) => true;

                if (config.UseProxy)
                {
                    smtp.ProxyClient = MailLoad.GetProxyClient(context, config.Proxy);
                }

                if (config.STARTTLS)
                {
                    smtp.Connect(host, port, SecureSocketOptions.StartTls);
                }
                else
                {
                    if (config.SMTPSSL)
                    {
                        smtp.Connect(host, port, SecureSocketOptions.SslOnConnect);
                    }
                    else
                    {
                        smtp.Connect(host, port, SecureSocketOptions.Auto);
                    }
                }

                smtp.Authenticate(username, password);
                smtp.Send(msgSend);
                smtp.Disconnect(true);
                context.WriteLog("成功发送邮件");
            };
        }

        public override void Validate(ActivityContext context)
        {
            if (string.IsNullOrEmpty(this.ConfigName)) throw new Exception("邮件配置不能为空");
            if (string.IsNullOrEmpty(this.MailTo)) throw new Exception("收件人邮箱不能为空");
            if (string.IsNullOrEmpty(this.Subject)) throw new Exception("邮件标题不能为空");
            if (string.IsNullOrEmpty(this.Body)) throw new Exception("邮件内容不能为空");
            if (this.SetNick)
            {
                if (string.IsNullOrEmpty(this.SenderNick)) throw new Exception("发件人昵称不能为空");
                if (string.IsNullOrEmpty(this.ReceiverNick)) throw new Exception("收件人昵称不能为空");
            }
            MailLoad.ValidateMailConfig(ConfigName, context);
            if (this.SendAttachments)
            {
                if (string.IsNullOrEmpty(this.AttachmentsVarName))
                {
                    throw new Exception("附件变量不能为空");
                }
                else
                {
                    if (!context.ContainsStr(this.AttachmentsVarName) && !context.ContainsList(this.AttachmentsVarName)) throw new Exception("不存在字符或列表变量：" + this.AttachmentsVarName);
                }
            }
        }

        public override ControlStyle GetControlStyle(string field)
        {
            ControlStyle style = new ControlStyle() { Variables = ControlStyle.GetVariables(true, false, true) };
            switch (field)
            {
                case "ConfigName":
                    style.DropDownList = MailLoad.GetMailConfigActivities();
                    break;
                case "SenderNick":
                case "ReceiverNick":
                    style.Visible = this.SetNick;
                    break;
                case "AttachmentsVarName":
                    style.Visible = this.SendAttachments;
                    style.Variables = ControlStyle.GetVariables(true, true);
                    break;
            }
            return style;
        }
    }
}