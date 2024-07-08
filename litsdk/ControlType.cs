using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace litsdk
{
    /// <summary>
    /// Argument控件类型
    /// 关于默认值和建议值，直接使用属性名做为入参执行方法返回Variable和List.Variable
    /// </summary>
    public enum ControlType
    {
        /// <summary>
        /// 文本框，只显示一行
        /// 支持读写文本属性，可直接输入文本，支持字符和数字变量写入
        /// 支持读写数字属性，可直接填写数字，支持字符和数字变量写入
        /// </summary>
        TextBox = 0,

        /// <summary>
        /// 下拉框
        /// 支持选取文本，选中的文本，存至文本属性值，其值可由建议值提供,不可以自行写入 
        /// 支持选取数字，选中的数字，存为数字属性值，其值可由建议值提供，不可自行写入
        /// 支持选取枚举,按上至下取索引存为枚举属性值，其值由Enum的属性提供
        ///  </summary>
        ComboBox = 1,

        /// <summary>
        /// 变量名，直接选择存在的变量名
        /// 具体显示哪些类型变量，由具体建议值方法来显示
        /// 如果是List值，则可以选多个变量名，字符只选一个
        /// </summary>
        Variable = 2,

        /// <summary>
        /// 复选框，使用boolen属性
        /// </summary>
        CheckBox = 3,

        /// <summary>
        /// 多行文本，默认显示多行，用户可拉成更多行
        /// 支持读写文本属性，可直接输入文本，支持字符和数字变量写入
        /// </summary>
        TextArea = 4,

        /// <summary>
        /// 文件，属性为字符类型，文本框加浏览文件按钮，包含当前目录
        /// </summary>
        File = 5,

        /// <summary>
        /// 文件夹，属性为字符类型，文本框加浏览文件夹按钮，包含当前目录
        /// </summary>
        Directory = 6,

        /// <summary>
        /// 数字下拉框,最大值和最小的选取，可用建议值索引01
        /// </summary>
        NumericUpDown = 7,

        /// <summary>
        /// 开关键，使用boolen属性
        /// </summary>
        ToggleSwitch = 8,

        /// <summary>
        /// 密码框，对应字符
        /// </summary>
        Password = 9,

        /// <summary>
        /// 演示框
        /// </summary>
        Example = 10,

        /// <summary>
        /// 选择元素
        /// </summary>
        UiSelect = 11,

        /// <summary>
        /// 组件注册
        /// 如果组件没有注册
        /// 则不显示下边的控件
        /// </summary>
        RegActivity = 12,

        /// <summary>
        /// 配置列表List<T>
        /// </summary>
        IObjectList = 13,

        /// <summary>
        /// 配置列表添加
        /// </summary>
        IObjectAdd = 14,

        /// <summary>
        /// 代码区域
        /// </summary>
        CodeArea = 15,

        /// <summary>
        /// 下拉框
        /// </summary>
        DropDown = 16,

        /// <summary>
        /// 颜色
        /// </summary>
        Color = 18,

        /// <summary>
        /// 用户自定义界面
        /// </summary>
        UserUI = 66,


    }
}