using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace litsdk
{
    /// <summary>
    /// 机器人运行控制参数
    /// </summary>
    public class Boter
    {
        /// <summary>
        /// 机器人ID,一般用guid
        /// </summary>
        public string BotId { get; set; }

        /// <summary>
        /// 任务Id
        /// </summary>
        public int JobId { get; set; }

        /// <summary>
        /// 流程Id
        /// </summary>
        public int ProjectId { get; set; }

        /// <summary>
        /// WebSocket端口，0为不启用
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// sdk的key
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 启动时间，方便判断超时
        /// </summary>
        public DateTime TimeStart { get; private set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime TimeEnd { get; private set; }

        /// <summary>
        /// litrobot.exe 运行时窗口的标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 设置运行流程指定字符变量初始值
        /// object类型的变量类型一致
        /// </summary>
        public Dictionary<string, object> InPuts { get; set; }

        /// <summary>
        /// 服务端的进程id，当litrobot发现主进程不存在时，会退出运行,0为不限
        /// </summary>
        public int ThreadId { get; set; }

        /// <summary>
        /// 返回的参数变量
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public List<litsdk.Variable> OutPuts = new List<litsdk.Variable>();

        private System.Diagnostics.Process process = null;

        /// <summary>
        /// 只启动一次的
        /// </summary>
        public void Start()
        {
            if (this.process != null) return;//只启动一次
            this.TimeStart = DateTime.Now;
            System.Diagnostics.ProcessStartInfo processStartInfo = new System.Diagnostics.ProcessStartInfo();
            processStartInfo.FileName = BotFile;
            processStartInfo.UseShellExecute = false;
            processStartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            string args = Newtonsoft.Json.JsonConvert.SerializeObject(this);
            args = System.Web.HttpUtility.UrlEncode(args, System.Text.Encoding.UTF8);
            processStartInfo.Arguments = args;
            this.process = System.Diagnostics.Process.Start(processStartInfo);
            this.process.EnableRaisingEvents = true;
            this.process.Exited += P_Exited;
            this.BotState = BotState.Running;
        }

        /// <summary>
        /// 机器人运行状态
        /// </summary>
        public BotState BotState { get; set; } = BotState.Idle;

        /// <summary>
        /// 运行结果
        /// </summary>
        public BotCode BotCode { get; set; } = BotCode.Sucess;

        private void P_Exited(object sender, EventArgs e)
        {
            this.TimeEnd = DateTime.Now;
            this.BotState = BotState.Completed;
            try
            {
                this.BotCode = (BotCode)this.process.ExitCode;
            }
            catch
            {
                this.BotCode = BotCode.UnKnown;
            }
            if (BotEnd != null) BotEnd(this);
        }

        /// <summary>
        /// 关闭资源
        /// </summary>
        public void Dispose()
        {
            if (this.process.HasExited) return;
            this.process.Exited -= P_Exited;
            try
            {
                if (!this.process.HasExited) this.process.Kill();
            }
            catch
            {
                System.Threading.Thread.Sleep(100);
                try
                {
                    if (!this.process.HasExited) this.process.Kill();
                }
                catch { }
            }
            finally
            {
                this.BotState = BotState.Completed;
            }
            this.process = null;
        }

        [Newtonsoft.Json.JsonIgnore]
        /// <summary>
        /// 结束事件
        /// </summary>
        public Action<Boter> BotEnd = null;

        /// <summary>
        /// 机器人地址
        /// </summary>
        public static string BotFile = AppDomain.CurrentDomain.BaseDirectory + "litrobot.exe";

        public static bool BotFileExist
        {
            get
            {
                return System.IO.File.Exists(BotFile);
            }
        }
    }
}