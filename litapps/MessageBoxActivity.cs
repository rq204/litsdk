using litsdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace litapps
{
    [litsdk.Action(Name = "确认取消弹窗", Category = "对话框", IsFront = true, Order = 40, Description = "消息弹窗，用来弹出确认取消的提示信息，返回True或False结果")]
    /// <summary>
    /// 确认取消
    /// </summary>
    public class MessageBoxActivity : litsdk.DecisionActivity
    {
        public override string Name { get; set; } = "确认取消弹窗";

        [Argument(Name = "弹窗标题", ControlType = ControlType.TextBox, Order = 1, Description = "显示的提示信息内容")]
        /// <summary>
        /// 弹窗标题
        /// </summary>
        public string Caption { set; get; }

        [Argument(Name = "弹窗内容", ControlType = ControlType.TextArea, Order = 2, Description = "显示的提示信息内容")]
        /// <summary>
        /// 弹窗内容
        /// </summary>
        public string Text { get; set; }

        [Argument(Name = "消息类型", ControlType = ControlType.ComboBox, Description = "消费类型，主要影响弹出窗口的图标", Order = 3)]
        public MessageBoxType MessageBoxType { get; set; }

        [Argument(Name = "超时秒数", ControlType = ControlType.NumericUpDown, Description = "超时秒，如果超时则点击取消", Order = 3)]
        /// <summary>
        /// 超时时间
        /// </summary>
        public int TimeOutSenconds { get; set; }


        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        static extern bool EndDialog(IntPtr hDlg, out IntPtr nResult);

        private string Result = null;
        public override bool Execute(ActivityContext context)
        {
            string txt = context.ReplaceVar(Text).Trim();
            string caption = context.ReplaceVar(this.Caption).Trim();
            System.Windows.Forms.MessageBoxIcon icon = MessageBoxIcon.Information;
            switch (this.MessageBoxType)
            {
                case MessageBoxType.Warning:
                    icon = MessageBoxIcon.Warning;
                    break;
                case MessageBoxType.Error:
                    icon = MessageBoxIcon.Error;
                    break;
            }

            System.Threading.Thread thread = null;
            if (this.TimeOutSenconds > 0)
            {
                this.Result = null;
                thread = new System.Threading.Thread(() =>
             {
                 litsdk.API.GetMainForm().Invoke((EventHandler)delegate
                 {
                     DialogResult dr = System.Windows.Forms.MessageBox.Show(txt, caption, System.Windows.Forms.MessageBoxButtons.OKCancel, icon);
                     this.Result = dr == DialogResult.OK ? "true" : "false";
                 });
             });
                thread.Start();
                System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
                stopwatch.Start();
                while (this.Result == null)
                {
                    if (stopwatch.ElapsedMilliseconds > this.TimeOutSenconds * 1000)//超时了
                    {
                        IntPtr dlg = FindWindow(null, caption);

                        if (dlg != IntPtr.Zero)
                        {
                            IntPtr result;
                            EndDialog(dlg, out result);
                            this.Result = "false";
                        }
                        break;
                    }
                    System.Threading.Thread.Sleep(300);
                }
                stopwatch.Stop();
                return Convert.ToBoolean(this.Result);
            }
            else
            {
                DialogResult dr = System.Windows.Forms.MessageBox.Show(txt, caption, System.Windows.Forms.MessageBoxButtons.OKCancel, icon);
                return dr == DialogResult.OK;
            }
        }

        public override void Validate(ActivityContext context)
        {
            if (string.IsNullOrEmpty(this.Text)) throw new Exception("提示内容不能为空");
            if (string.IsNullOrEmpty(this.Caption)) throw new Exception("提示标题不能为空");
        }

        public override ControlStyle GetControlStyle(string field)
        {
            ControlStyle style = new ControlStyle();
            style.Variables = ControlStyle.GetVariables(true, false, true);
            return style;
        }
    }
}
