using litsdk;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace litapps
{
    [litsdk.Action(Name = "获取视频信息", Category = "视频", Order = 2, IsFront = false)]
    public class FfmpegInfoActivity : litsdk.ProcessActivity
    {
        public override string Name { get; set; } = "获取视频信息";

        [Argument(Name = "ffmpeg路径", Order = 1, ControlType = ControlType.File, Description = "ffmpeg.exe文件路径")]
        public string FfmpegPath { get; set; }

        [Argument(Name = "视频文件", ControlType = ControlType.File, Order = 2, Description = "视频文件路径")]
        public string VideoPath { get; set; }

        /// <summary>
        /// 宽
        /// </summary>
        [Argument(Name = "视频宽度", ControlType = ControlType.Variable, Order = 3, Description = "视频宽度")]
        public string WidthVarName { get; set; }

        /// <summary>
        /// 高
        /// </summary>
        [Argument(Name = "视频高度", ControlType = ControlType.Variable, Order = 4, Description = "视频高度")]
        public string HeightVarName { get; set; }

        /// <summary>
        /// 时间长度
        /// </summary>
        [Argument(Name = "视频时长", ControlType = ControlType.Variable, Order = 5, Description = "视频时长")]
        public string DurationVarName { get; set; }

        /// <summary>
        /// fps参数
        /// </summary>
        [Argument(Name = "fps参数", ControlType = ControlType.Variable, Order = 6, Description = "fps参数")]
        public string FPSVarName { get; set; }

        /// <summary>
        /// 视频编码
        /// </summary>
        [Argument(Name = "视频编码", ControlType = ControlType.Variable, Order = 7, Description = "视频编码")]
        public string VideoCodingVarName { get; set; }

        public override void Execute(ActivityContext context)
        {
            string input = context.ReplaceVar(this.VideoPath);

            if (!System.IO.File.Exists(input)) throw new Exception("视频文件不存在:" + input);

            string @params = string.Format("-i \"{0}\"", input);
            string output = Run(context.ReplaceVar(this.FfmpegPath), @params);

            //get the video format
            Regex re = new Regex("\\D(\\d{2,4})x(\\d{2,4})\\D");
            Match m = re.Match(output);
            int width = 0; int height = 0, tbc = 0;
            string duration = "", videocoding = "";
            List<string> logs = new List<string>();
            if (m.Success)
            {
                int.TryParse(m.Groups[1].Value, out width);
                logs.Add($"宽度:{width}");
                int.TryParse(m.Groups[2].Value, out height);
                logs.Add($"高度:{height}");
            }
            m = System.Text.RegularExpressions.Regex.Match(output, ", (\\d{1,2}) tbc");
            if (m.Success)
            {
                int.TryParse(m.Groups[1].Value, out tbc);
                logs.Add($"视频帧率:{tbc}");
            }

            m = System.Text.RegularExpressions.Regex.Match(output, @"Duration: (\d\d:\d\d:\d\d\.\d\d),");
            if (m.Success)
            {
                duration = m.Groups[1].Value;
                logs.Add($"视频时长:{duration}");
            }

            m = System.Text.RegularExpressions.Regex.Match(output, @": Video: ([^\(]*?)\(");
            if (m.Success)
            {
                videocoding = m.Groups[1].Value;
                logs.Add($"编码格式:{videocoding}");
            }

            if (!string.IsNullOrEmpty(this.HeightVarName)) context.SetVarInt(this.HeightVarName, height);
            if (!string.IsNullOrEmpty(this.WidthVarName)) context.SetVarInt(this.WidthVarName, width);
            if (!string.IsNullOrEmpty(this.FPSVarName)) context.SetVarInt(this.FPSVarName, tbc);

            if (!string.IsNullOrEmpty(this.DurationVarName)) context.SetVarStr(this.DurationVarName, duration);
            if (!string.IsNullOrEmpty(this.VideoCodingVarName)) context.SetVarStr(this.VideoCodingVarName, videocoding);

            context.WriteLog($"视频信息 " + string.Join("，", logs.ToArray()));
        }

        public override void Validate(ActivityContext context)
        {
            if (string.IsNullOrEmpty(this.FfmpegPath)) throw new Exception("ffmpeg.exe路径不能为空");
            if (string.IsNullOrEmpty(this.VideoPath)) throw new Exception("视频路径不能为空");
            if (!string.IsNullOrEmpty(this.WidthVarName) && !context.ContainsInt(this.WidthVarName)) throw new Exception($"宽度数字变量{this.WidthVarName}不存在");
            if (!string.IsNullOrEmpty(this.HeightVarName) && !context.ContainsInt(this.HeightVarName)) throw new Exception($"宽度数字变量{this.HeightVarName}不存在");

            if (!string.IsNullOrEmpty(this.FPSVarName) && !context.ContainsInt(this.FPSVarName)) throw new Exception($"tbc数字变量{this.FPSVarName}不存在");

            if (!string.IsNullOrEmpty(this.DurationVarName) && !context.ContainsStr(this.DurationVarName)) throw new Exception($"时长字符变量{this.DurationVarName}不存在");
            if (!string.IsNullOrEmpty(this.VideoCodingVarName) && !context.ContainsStr(this.VideoCodingVarName)) throw new Exception($"时长字符变量{this.VideoCodingVarName}不存在");
        }

        public static string Run(string exePath, string parameters)
        {
            if (!System.IO.File.Exists(exePath)) throw new Exception("不存在ffmpeg.exe文件:" + exePath);
            //  Create a process info.
            ProcessStartInfo startInfo = new ProcessStartInfo(exePath, parameters);

            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            startInfo.StandardOutputEncoding = System.Text.Encoding.Default;

            Process p = System.Diagnostics.Process.Start(startInfo);
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();
            string outputData = "", errorData = "";
            p.OutputDataReceived += (ss, ee) =>

            {
                outputData += ee.Data;
            };

            p.ErrorDataReceived += (ss, ee) =>

            {
                errorData += ee.Data;
            };

            //等待执行结束后退出
            p.WaitForExit();

            //关闭进程
            p.Close();

            if (!string.IsNullOrEmpty(outputData)) return outputData;
            if (!string.IsNullOrEmpty(errorData)) return errorData;
            return "";
        }

        public override ControlStyle GetControlStyle(string field)
        {
            ControlStyle style = new ControlStyle() { Variables = ControlStyle.GetVariables(true, false, true) };
            switch (field)
            {
                case "HeightVarName":
                case "WidthVarName":
                case "FPSVarName":
                    style.Variables = ControlStyle.GetVariables(false, false, true);
                    break;
                case "DurationVarName":
                case "VideoCodingVarName":
                    style.Variables = ControlStyle.GetVariables(true);
                    break;
                case "FfmpegPath":
                    style.PlaceholderText = "请填写ffmpeg.exe的文件路径";
                    break;
            }
            return style;
        }
    }
}