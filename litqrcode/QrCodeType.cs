using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace litqrcode
{
    public enum QrCodeType
    {
        [Description("二维码生成")]
        Encode = 0,
        [Description("二维码识别")]
        Decode = 1
    }
}
