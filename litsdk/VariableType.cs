using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace litsdk
{
    /// <summary>
    /// 变量类型
    /// </summary>
    public enum VariableType
    {
        /// <summary>
        /// 字符
        /// </summary>
        [Description("字符")]
        String = 0,
        /// <summary>
        /// 数字
        /// </summary>
        [Description("数字")]
        Int = 1,
        /// <summary>
        /// 列表
        /// </summary>
        [Description("列表")]
        List = 2,
        ///// <summary>
        ///// 布尔
        ///// </summary>
        //[Description("布尔")]
        //Boolen = 3,
        /// <summary>
        /// 表格
        /// </summary>
        [Description("表格")]
        Table = 4
    }
}