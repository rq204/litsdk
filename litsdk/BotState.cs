using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace litsdk
{
    /// <summary>
    /// 机器人状态
    /// </summary>
    public enum BotState
    {
        /// <summary>
        /// 空闲
        /// </summary>
        Idle = 0,
        /// <summary>
        /// 运行中
        /// </summary>
        Running = 1,
        /// <summary>
        /// 暂停
        /// </summary>
        Paused = 2,
        /// <summary>
        /// 运行完成
        /// </summary>
        Completed = 3
    }
}
