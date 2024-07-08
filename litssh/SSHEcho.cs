using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace litssh
{
    public class SSHEcho
    {
        /// <summary>
        /// 写入的字符
        /// </summary>
        public string WriteStr = "";

        /// <summary>
        /// 成功标记
        /// </summary>
        public string ScuessStr = "";

        /// <summary>
        /// 超时
        /// </summary>
        public int TimeOutSeconds = 1;
    }
}
