using litsdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace litapps
{
    [litsdk.Action(Name = "播放声音", Category = "系统", Order = 60,Description ="播放系统声音或是播放用户指定的文件")]
    public class WinBeepActivity : litsdk.ProcessActivity
    {
        public override string Name { get; set; } = "播放声音";

        [Argument(Name = "播放方式", ControlType = ControlType.ComboBox, Order = 1, Description = "播放声音的方式")]
        public WinBeepType WinBeepType { get; set; } = WinBeepType.Beep;

        [Argument(Name = "音频文件", ControlType = ControlType.File, Order = 2, Description = "用户设置播放的音频文件地址,播放使用系统默认播放器")]
        public string PlayFile { get; set; }

        [Argument(Name = "播放次数", ControlType = ControlType.NumericUpDown, Order = 3, Description = "播放声音的次数")]
        public int PlayTimes { get; set; } = 1;

        public override void Execute(ActivityContext context)
        {
            for (int i = 0; i < this.PlayTimes; i++)
            {
                string txt = "开始播放声音";
                if (this.PlayTimes > 1) txt = $"开始第{i + 1}次播放声音，总共{this.PlayTimes}次";
                context.WriteLog(txt);
                switch (this.WinBeepType)
                {
                    case WinBeepType.Beep:
                        Beep(2000, 500);
                        break;
                    case WinBeepType.Asterisk:
                        System.Media.SystemSounds.Asterisk.Play();// 警示音
                        break;
                    case WinBeepType.UserConfig:
                        string file = context.ReplaceVar(this.PlayFile);
                        System.Diagnostics.Process.Start(file);
                        break;
                }
                System.Threading.Thread.Sleep(1000);
                if (Console.In == System.IO.StreamReader.Null) System.Windows.Forms.Application.DoEvents();
                context.ThrowIfCancellationRequested();
            }
        }

        public override void Validate(ActivityContext context)
        {
            if (this.WinBeepType == WinBeepType.UserConfig)
            {
                if (string.IsNullOrEmpty(this.PlayFile)) throw new Exception("用户设置时需要设定音频文件路径");
            }
        }

        [DllImport("kernel32.dll")]
        public static extern bool Beep(int frequency, int duration);

        public override ControlStyle GetControlStyle(string field)
        {
            ControlStyle style = new ControlStyle();
            switch (field)
            {
                case "PlayFile":
                    style.Visible = this.WinBeepType == WinBeepType.UserConfig;
                    style.Filter = "音频文件|*.mp3;*.wma;*.wav";
                    break;
            }

            style.Variables = ControlStyle.GetVariables(true, false, true);
            return style;
        }
    }
}