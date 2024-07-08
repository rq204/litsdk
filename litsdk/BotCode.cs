using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace litsdk
{
    /// <summary>
    /// 机器人运行结果
    /// 用不同的数字代表不同的结果
    /// </summary>
    public enum BotCode
    {
        /// <summary>
        /// 未启动
        /// </summary>
        UnStart = 0,

        /// <summary>
        /// 流程错误
        /// </summary>
        ProjectErr=120,

        /// <summary>
        /// 运行成功
        /// </summary>
        Sucess = 200,
        /// <summary>
        /// 用户关闭
        /// </summary>
        UserStop = 402,
        /// <summary>
        /// 试用到期
        /// </summary>
        TestEnd = 403,

        /// <summary>
        /// 运行器退出
        /// </summary>
        RunnerExit=406,
        /// <summary>
        /// 服务器错误
        /// </summary>
        ServerErr = 500,
        /// <summary>
        /// 传参错误
        /// </summary>
        CommendErr = 501,
        /// <summary>
        /// Boter参数错误
        /// </summary>
        BoterErr = 502,
        /// <summary>
        /// Boter参数错误
        /// </summary>
        CodeErr = 503,
        /// <summary>
        /// 未知错误
        /// </summary>
        UnKnown = 505,
    }
}
