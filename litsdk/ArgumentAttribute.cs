using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace litsdk
{
    /// <summary>
    /// 操作参数的属性,在属性上添加
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ArgumentAttribute : Attribute
    {
        /// <summary>
        /// 名称，显示的名子
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 描述，详细说明这个字段的使用方法
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 控件类型
        /// </summary>
        public ControlType ControlType { get; set; }

        /// <summary>
        /// 次序，控件的次序
        /// </summary>
        public int Order { get; set; }
    }
}
