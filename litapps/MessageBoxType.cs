using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace litapps
{
    /// <summary>
    /// 
    /// </summary>
    public enum MessageBoxType
    {
        [Description("提示")]
        Information = 0,
        [Description("警告")]
        Warning = 1,
        [Description("错误")]
        Error = 2,
    }
}
