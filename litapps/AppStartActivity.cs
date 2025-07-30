using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using litsdk;

namespace litapps
{
    [litsdk.Action(Name = "打开程序", Category = "系统", Order = 1, Description = "打开可执行文件并可以记录控制台输出")]
    public class AppStartActivity : litsdk.ProcessActivity
    {
        /// <summary>
        /// 操作类型 等待程序执行完成 （等待程序执行完成并保存输出到变量） 仅打开程序（保存pid到变量） 设置打超时
        /// </summary>
        public override string Name { get; set; } = "打开程序";

        [Argument(Name = "程序路径", ControlType = ControlType.File, Order = 1, Description = "需要打开的程序路径")]
        /// <summary>
        /// 路径
        /// </summary>
        public string AppPath { get; set; }

        [Argument(Name = "运行参数", ControlType = ControlType.TextBox, Order = 2, Description = "命令行参数")]
        /// <summary>
        /// 参数
        /// </summary>
        public string Arguments { get; set; }

        [Argument(Name = "工作目录", ControlType = ControlType.Directory, Order = 3, Description = "工作目录,可以为空")]
        /// <summary>
        /// 工作目录
        /// </summary>
        public string WorkingDirectory { get; set; }

        [Argument(Name = "隐藏启动", ControlType = ControlType.CheckBox, Order = 4, Description = "隐藏启动的软件界面")]
        /// <summary>
        /// 隐藏启动
        /// </summary>
        public bool Hide { get; set; } = false;

        [Argument(Name = "等待程序运行完成", Order = 5, ControlType = ControlType.CheckBox, Description = "等待程序运行完成，即等待程序关闭")]
        /// <summary>
        /// 是否等待进行执行完毕
        /// </summary>
        public bool WaitProcess { get; set; } = true;

        /// <summary>
        /// 暂停等待使用变量
        /// </summary>
        [Argument(Name = "超时秒数使用数字变量", Order = 7, ControlType = ControlType.CheckBox, Description = "等待程序运行完成，使用数字变量中秒数")]
        public bool WaitUseVar { get; set; }

        [Argument(Name = "超时秒数", ControlType = ControlType.NumericUpDown, Order = 9, Description = "当前程序运行超过这个时间后将被强制关闭")]
        /// <summary>
        /// 超时秒数
        /// </summary>
        public int TimeOutSeconds { get; set; } = 600;

        /// <summary>
        /// 超时秒数
        /// </summary>
        [Argument(Name = "超时秒数", ControlType = ControlType.Variable, Order = 11, Description = "当前程序运行超过这个时间后将被强制关闭")]
        public string TimeOutVarName { get; set; }

        [Argument(Name = "保存控制台输出信息至字符变量", ControlType = ControlType.CheckBox, Order = 13, Description = "可以将控制台程序输出的信息保存至字符变量当中")]
        /// <summary>
        /// 等待的话显示哪些返回信息
        /// </summary>
        public bool SaveConsole { get; set; }

        [Argument(Name = "输出编码", ControlType = ControlType.ComboBox, Order = 18, Description = "控制台输出的字符编码")]
        /// <summary>
        /// 输出编码
        /// </summary>
        public string OutputEncoding { get; set; } = "GB2312";

        [Argument(Name = "保存进程Id至数字变量", ControlType = ControlType.CheckBox, Order = 19, Description = "将进程的Id保存至数字变量当中")]
        public bool SaveProcessId { get; set; }

        [Argument(Name = "保存变量", ControlType = ControlType.Variable, Order = 22, Description = "将控制台程序输出的信息保存至文本变量当中")]
        public string SaveOutVarName { get; set; }

        [Argument(Name = "保存变量", ControlType = ControlType.Variable, Order = 24, Description = "将进程Id保存至数字变量当中")]
        public string SaveProcessIdVarName { get; set; }

        [Argument(Name = "超时或发生错误时忽略继续", ControlType = ControlType.CheckBox, Order = 66)]
        public bool SkipOnError { get; set; }

        private Process process;

        public override void Execute(ActivityContext context)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            string exeFile = context.ReplaceVar(this.AppPath);
            if (!File.Exists(exeFile)) throw new FileNotFoundException("找不到可执行文件: " + exeFile);

            var processInfo = new ProcessStartInfo()
            {
                FileName = exeFile,
                Arguments = context.ReplaceVar(this.Arguments),
                WorkingDirectory = !string.IsNullOrEmpty(this.WorkingDirectory) ? context.ReplaceVar(this.WorkingDirectory) : Path.GetDirectoryName(exeFile),
                UseShellExecute = false,
                RedirectStandardOutput = this.WaitProcess && this.SaveConsole,
                RedirectStandardError = this.WaitProcess && this.SaveConsole,
                CreateNoWindow = this.Hide,
                WindowStyle = this.Hide ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal
            };

            int timeout = this.TimeOutSeconds * 1000;
            if (this.WaitUseVar) timeout = context.GetInt(this.TimeOutVarName) * 1000;

            process = new Process { StartInfo = processInfo, EnableRaisingEvents = true };

            try
            {
                if (this.WaitProcess && this.SaveConsole && IsGuiApp(exeFile)) throw new Exception("非控制台程序不能保存输出内容");

                process.Start();

                if (this.SaveProcessId) context.SetVarInt(this.SaveProcessIdVarName, process.Id);

                if (this.WaitProcess)
                {
                    if (this.SaveConsole)
                    {
                        Encoding encoding = Encoding.GetEncoding(this.OutputEncoding);
                        string output = process.StandardOutput.ReadToEnd();
                        string error = process.StandardError.ReadToEnd();
                        context.SetVarStr(this.SaveOutVarName, string.IsNullOrEmpty(output) ? error : output);
                    }
                    if (!process.WaitForExit(timeout))
                    {
                        process.Kill();
                        throw new TimeoutException("进程运行超时，已被强制终止。");
                    }
                    context.WriteLog($"程序运行完成，用时{sw.ElapsedMilliseconds}毫秒");
                }
                else
                {
                    context.WriteLog("程序启动成功");
                }
            }
            catch (Exception ex)
            {
                context.WriteLog("执行错误: " + ex.Message);
                if (!this.SkipOnError) throw;
            }
            finally
            {
                process?.Dispose();
            }
        }

        public override void Validate(ActivityContext context)
        {
            if (string.IsNullOrEmpty(this.AppPath)) throw new Exception("应用程序不能为空");
            if (this.WaitProcess && this.SaveConsole && !context.ContainsStr(this.SaveOutVarName)) throw new Exception("保存输出变量不存在");
            if (!this.WaitProcess && this.SaveProcessId && !context.ContainsInt(this.SaveProcessIdVarName)) throw new Exception("进程Id保存数字变量不存在");
            if (this.WaitProcess && this.WaitUseVar && !context.ContainsInt(this.TimeOutVarName)) throw new Exception("运行超时使用数字变量不存在");
        }

        public override ControlStyle GetControlStyle(string field)
        {
            ControlStyle style = new ControlStyle() { Variables = ControlStyle.GetVariables(true, false, true) };
            switch (field)
            {
                case "AppPath":
                    style.Variables = ControlStyle.GetVariables(true, false, true);
                    style.Filter = "可执行文件|*.exe";
                    break;
                case "SaveConsole":
                    style.Visible = this.WaitProcess;
                    break;
                case "SaveProcessId":
                    style.Visible = !this.WaitProcess;
                    style.Variables = ControlStyle.GetVariables(false, false, true);
                    break;
                case "SaveOutVarName":
                    style.Visible = this.WaitProcess && this.SaveConsole;
                    style.Variables = ControlStyle.GetVariables(true);
                    break;
                case "SaveProcessIdVarName":
                    style.Visible = !this.WaitProcess && this.SaveProcessId;
                    style.Variables = ControlStyle.GetVariables(false, false, true);
                    break;
                case "OutputEncoding":
                    style.Visible = this.WaitProcess && this.SaveConsole;
                    style.DropDownList = new List<string>() { "GB2312", "UTF-8", "ASCII", "Unicode" };
                    break;
                case "TimeOutSeconds":
                    style.Visible = this.WaitProcess && !WaitUseVar;
                    break;
                case "WaitUseVar":
                    style.Visible = this.WaitProcess;
                    break;
                case "TimeOutVarName":
                    style.Visible = this.WaitProcess && this.WaitUseVar;
                    style.Variables = ControlStyle.GetVariables(false, false, true);
                    break;
            }
            return style;
        }

        /// <summary>
        /// https://www.xin3721.com/ArticlePrograme/C_biancheng/15552.html
        /// </summary>
        /// <param name="pFilePath"></param>
        /// <returns></returns>
        public static bool IsGuiApp(string pFilePath)

        {
            UInt16 subSystem;
            ushort architecture = 0;

            subSystem = 0;

            try

            {
                using (System.IO.FileStream fStream = new System.IO.FileStream(pFilePath, System.IO.FileMode.Open, System.IO.FileAccess.Read))

                {

                    using (System.IO.BinaryReader bReader = new System.IO.BinaryReader(fStream))

                    {

                        if (bReader.ReadUInt16() == 23117) //check the MZ signature

                        {

                            fStream.Seek(0x3A, System.IO.SeekOrigin.Current); //seek to e_lfanew.

                            fStream.Seek(bReader.ReadUInt32(), System.IO.SeekOrigin.Begin); //seek to the start of the NT header.

                            if (bReader.ReadUInt32() == 17744) //check the PE\0\0 signature.

                            {

                                fStream.Seek(20, System.IO.SeekOrigin.Current); //seek past the file header,

                                architecture = bReader.ReadUInt16(); //read the magic number of the optional header.

                                fStream.Seek(0x42, System.IO.SeekOrigin.Current); //0x44h

                                subSystem = bReader.ReadUInt16();

                            }

                        }

                    }

                }

            }

            catch (Exception) { /* TODO: Any exception handling you want to do, personally I just take 0 as a sign of failure */}

            //if architecture returns 0, there has been an error.
            //SubSystem = subSystem == 2 ? "GUI" : (subSystem == 3 ? "CUI" : ""),

            return subSystem == 2;

            //return architecture;
        }
    }
}
