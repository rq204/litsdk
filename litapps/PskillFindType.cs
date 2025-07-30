using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace litapps
{

    public enum PskillFindType
    {
        [Description("进程路径")]
        FilePath = 0,

        [Description("进程名称")]
        ProcessName = 1,

        [Description("进程ID")]
        ProcessId = 2,
    }
}
