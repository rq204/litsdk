using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace litsdk
{
    /// <summary>
    /// 生成机器人的配置信息
    /// </summary>
    public interface IApp
    {
        /// <summary>
        /// 应用名称
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 功能描述，一些配置要注意的事项也写里边
        /// </summary>
        string Description { get; }

        /// <summary>
        /// 必需字符变量名
        /// </summary>
        List<string> RequiredStrs { get; }

        /// <summary>
        /// 必需数字变量名
        /// </summary>
        List<string> RequiredInts { get; }

        /// <summary>
        /// 要求内置浏览器
        /// </summary>
        bool RequiredEdge { get; }

        /// <summary>
        /// 为否则不显示配置界面
        /// </summary>
        bool JsonSetting { get; }

        /// <summary>
        /// 引用的文件
        /// </summary>
        List<string> RefFiles { get; }

        /// <summary>
        /// 加载执行的
        /// </summary>
        void RunApp(string AppConfigJson, byte[] IconBytes, string HelpStr, string JsonSetting);
    }
}