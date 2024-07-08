using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace litsdk
{
    /// <summary>
    /// 模块的描述,是对当前的dll文件主要功能进行介绍
    /// </summary>
    [AttributeUsage(AttributeTargets.Module)]
    public class ModuleAttribute : Attribute
    {
        /// <summary>
        /// 作者
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// 网址
        /// </summary>
        public string SiteUrl { get; set; }

        /// <summary>
        /// 中文描述，说明这个模块的功能
        /// </summary>
        public string Description { get; set; }
    }
}
