using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace litapps
{
    public enum ExplorerOpenType
    {
        [Description("选择文件")]
        OpenFile = 0,
        [Description("选择文件夹")]
        OpenDir = 1,
    }
}
