using litsdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace litapps
{
    [litsdk.Action(Name = "视频转换命令", IsFront = false, Category = "视频", Order = 1)]
    public class FfmpegConvertActivity : litsdk.ProcessActivity
    {
        public override string Name { get; set; } = "视频转换命令";

        [Argument(Name = "ffmpeg路径", Order = 1, ControlType = ControlType.File, Description = "ffmpeg.exe文件路径")]
        public string FfmpegPath { get; set; }

        /// <summary>
        /// 命令行
        /// </summary>
        [Argument(Name = "命令行内容", ControlType = ControlType.TextArea, Order = 2, Description = "命令行")]
        public string Cmd { get; set; }

        /// <summary>
        /// https://blog.csdn.net/xuyankuanrong/article/details/78286265
        /// 码率的设置
        /// </summary>
        /// <param name="context"></param>
        public override void Execute(ActivityContext context)
        {
            string cmd = context.ReplaceVar(this.Cmd);
            context.WriteLog("开始执行命令：" + cmd);
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            string output = FfmpegInfoActivity.Run(context.ReplaceVar(this.FfmpegPath), cmd);
            stopwatch.Stop();
            if (output.Contains("ffmpeg version")) context.WriteLog($"命令行成功执行，用时 {(int)(stopwatch.ElapsedMilliseconds / 1000)}秒");
            else throw new Exception("执行命令出错：" + output);
        }
        public override void Validate(ActivityContext context)
        {
            if (string.IsNullOrEmpty(this.FfmpegPath)) throw new Exception("ffmpeg.exe路径不能为空");
            if (string.IsNullOrEmpty(this.Cmd)) throw new Exception("命令行参数不能为空");
        }

        public override ControlStyle GetControlStyle(string field)
        {
            ControlStyle style = new ControlStyle() { Variables = ControlStyle.GetVariables(true, false, true) };
            switch (field)
            {
                case "FfmpegPath":
                    style.PlaceholderText = "请填写ffmpeg.exe的文件路径";
                    break;
            }
            return style;
        }


        //static void Main()
        //{
        //    InfoActivity.Run("-i \"E:\\LitAcivity\\Debug\\通往地下河的深坑，网友疑问是否有鱼，镜头记录极品靓货追逐鱼群.flv\" -vf scale=-1:1080 -c:v libx264 -r 60 -y -crf 18 -b:a 512k \"E:\\LitAcivity\\Debug\\通往地下河的深坑，网友疑问是否有鱼，镜头记录极品靓货追逐鱼群.mp4\"");
        //}
    }
}