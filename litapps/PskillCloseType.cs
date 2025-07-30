using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace litapps
{
    public enum PskillCloseType
    {
        [Description("最晚打开的进程")]
        /// <summary>
        /// 最晚的
        /// </summary>
        Latest = 0,
        [Description("最早打开的进程")]
        /// <summary>
        /// 最早的
        /// </summary>
        Earliest = 1,
        [Description("所有打开的进程")]
        /// <summary>
        /// 所有
        /// </summary>
        All = 2
    }
}
