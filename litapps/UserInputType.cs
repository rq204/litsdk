using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace litapps
{
    public enum UserInputType
    {
        [Description("单行文本框")]
        TextBox = 0,
        /// <summary>
        /// 多行文本框
        /// </summary>
        [Description("多行文本框")]
        MulTextBox = 1,
        /// <summary>
        /// 数字输入框
        /// </summary>
        [Description("数字输入框")]
        NumericUpDwon = 2,
        /// <summary>
        /// 下拉框
        /// </summary>
        [Description("下拉框")]
        ComboBox = 3,
        /// <summary>
        /// 单选
        /// </summary>
        [Description("单选框")]
        RadioButton = 4,
        /// <summary>
        /// 复选
        /// </summary>
        [Description("复选框")]
        CheckBox = 5,
        [Description("日期选择框")]
        DateTime = 6,
        [Description("密码框")]
        Password = 7
    }
}