using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace litsdk
{
    /// <summary>
    /// 所有组件的基类
    /// </summary>
    [Serializable]
    public abstract class Activity
    {
        /// <summary>
        /// 流程名，在列表中显示
        /// </summary>
        public abstract string Name { get; set; }

        /// <summary>
        /// 校验当前流程
        /// </summary>
        /// <returns></returns>
        public abstract void Validate(ActivityContext context);

        /// <summary>
        /// 获取控件样式
        /// </summary>
        public abstract ControlStyle GetControlStyle(string field);
    }
}