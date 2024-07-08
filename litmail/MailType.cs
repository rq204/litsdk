using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace litmail
{
    public enum MailType
    {
        [Description("POP3")]
        POP3=0,
        [Description("IMAP")]
        IMAP =1,
        //[Description("Exchange")]
        //Exchange =2,
    }
}
