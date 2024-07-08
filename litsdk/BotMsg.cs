using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace litsdk
{
    /// <summary>
    /// 发送的消息
    /// </summary>
    public class BotMsg
    {
        /// <summary>
        /// 消息类型
        /// </summary>
        public BotMsgType BotMsgType = BotMsgType.DownProject;

        /// <summary>
        /// 消息值
        /// </summary>
        public string Data = "";

        /// <summary>
        /// 多条日志记录
        /// </summary>
        public List<string> Logs = new List<string>();
    }
}
