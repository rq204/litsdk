using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.ComponentModel;

namespace litmail
{
    public enum MailFilter
    {
        [Description("邮件标题包含")]
        SubjectContains = 0,
        [Description("发件人包含")]
        SenderContains = 1,
        [Description("收件内容包含")]
        BodyContains = 2,
        [Description("收件时间大于")]
        LaterThan = 3,

    }
}
