using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace litapps
{
    public enum WinBeepType
    {
        [Description("系统蜂鸣")]
        /// <summary>
        /// 蜂鸣
        /// </summary>
        Beep = 0,
        [Description("系统提示")]
        /// <summary>
        /// 系统提示
        /// </summary>
        Asterisk = 1,
        [Description("用户设置")]
        UserConfig = 2,
    }
}