using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace litapps
{
    /// <summary>
    /// 剪贴板操作类型
    /// </summary>
    public enum ClipboardType
    {
        [Description("设置文本到剪贴板")]
        /// <summary>
        /// 设置文本到剪贴板
        /// </summary>
        SetStrToClipboard = 0,

        [Description("设置文件到剪贴板")]
        /// <summary>
        /// 设置文件到剪贴板
        /// </summary>
        SetFileToClipboard = 1,

        [Description("设置图片到剪贴板")]
        /// <summary>
        /// 设置图片到剪贴板
        /// </summary>
        SetImageToClipboard = 2,

        [Description("获取剪贴板文本")]
        /// <summary>
        /// 获取剪贴板文本
        /// </summary>
        GetStrFromClipboard = 5,

        [Description("保存剪贴板图片至文件")]
        /// <summary>
        /// 保存剪贴板图片至路径
        /// </summary>
        SaveImageFromClipboard = 7,

        [Description("清空剪贴板")]
        ClearClipboard = 9,
    }
}
