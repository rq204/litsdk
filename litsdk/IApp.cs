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
        /// 需要的输入参数
        /// </summary>
        List<litsdk.Variable> InputVars { get; }

        /// <summary>
        /// 需要的输出参数
        /// </summary>
        List<litsdk.Variable> OutputVars { get; }

        /// <summary>
        /// 要求内置浏览器
        /// </summary>
        bool RequiredEdge { get; }

        /// <summary>
        /// 引用的文件
        /// </summary>
        List<string> RefFiles { get; }

        /// <summary>
        /// 加载执行的
        /// </summary>
        void RunApp(string AppConfigJson, byte[] IconBytes, string HelpStr);
    }
}