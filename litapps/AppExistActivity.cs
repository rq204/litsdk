using litsdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace litapps
{
    [litsdk.Action(Name = "进程存在", Category = "系统", Order = 8,Description ="查看进程是否存在")]
    public class AppExistActivity : litsdk.DecisionActivity
    {
        public override string Name { get; set; } = "进程存在";

        [Argument(Name = "查找方式",ControlType = ControlType.ComboBox, Order = 1, Description = "是按进程名还是路径或是进程id关闭进程")]
        public PskillFindType PskillFindType { get; set; } = PskillFindType.ProcessName;

        [Argument(Name = "进程名称",ControlType = ControlType.TextBox, Order = 2, Description = "按进程名关闭进程")]
        public string ProcessName { get; set; }

        [Argument(Name = "进程路径", Order = 3, ControlType = ControlType.File, Description = "按进程路径关闭进程")]
        public string FilePath { get; set; }

        [Argument(Name = "进程ID", Order = 4,  ControlType = ControlType.Variable, Description = "按进程id关闭进程")]
        public string ProcIdVarName { get; set; }

        /// <summary>
        /// 取相反值
        /// </summary>
        [Argument(Name = "取相反值", ControlType = ControlType.ToggleSwitch, Order = 14, Description = "取当前运行结果的相反值")]
        public bool Reverse { get; set; }

        public override bool Execute(ActivityContext context)
        {
            string value = "";// context.ReplaceVar(this.PskillValue);

            List<System.Diagnostics.Process> ps = new List<System.Diagnostics.Process>();
            switch (this.PskillFindType)
            {
                case PskillFindType.FilePath:
                    value = context.ReplaceVar(this.FilePath);
                    if (string.IsNullOrEmpty(value)) throw new Exception("进程路径参数值不能为空");
                    foreach (System.Diagnostics.Process pc in System.Diagnostics.Process.GetProcesses())
                    {
                        try
                        {
                            if (pc.MainModule.FileName.Equals(value, StringComparison.OrdinalIgnoreCase))
                            {
                                ps.Add(pc);
                            }
                        }
                        catch { }
                    }
                    break;
                case PskillFindType.ProcessName:
                    value = context.ReplaceVar(this.ProcessName);
                    if (string.IsNullOrEmpty(value)) throw new Exception("进程名参数值不能为空");
                    if (value.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)) value = value.Substring(0, value.Length - 4);
                    ps = System.Diagnostics.Process.GetProcessesByName(value).ToList();
                    break;
                case PskillFindType.ProcessId:
                    int pid = context.GetInt(this.ProcIdVarName);
                    value = pid.ToString();
                    System.Diagnostics.Process p = null;
                    try
                    {
                        p = System.Diagnostics.Process.GetProcessById(pid);
                    }
                    catch { }
                    if (p != null) ps.Add(p);
                    break;
            }

            string log = ps.Count > 0 ? $"发现进程{value}存在{ps.Count}个" : $"进程不存在：{value}";
            bool exist = ps.Count > 0;
            if (this.Reverse)
            {
                exist = !exist;
                log += $"，取相反值为：{exist}";
            }
            context.WriteLog(log);

            return exist;
        }

        public override void Validate(ActivityContext context)
        {
            switch (this.PskillFindType)
            {
                case PskillFindType.FilePath:
                    if (string.IsNullOrEmpty(this.FilePath)) throw new Exception("进程路径参数值不能为空");
                    break;
                case PskillFindType.ProcessName:
                    if (string.IsNullOrEmpty(this.ProcessName)) throw new Exception("进程名参数值不能为空");
                    break;
                case PskillFindType.ProcessId:
                    if (string.IsNullOrEmpty(this.ProcIdVarName)) throw new Exception("进程ID参数值不能为空");
                    if (!context.ContainsInt(this.ProcIdVarName)) throw new Exception($"进程ID变量{this.ProcIdVarName}不存在");
                    break;
            }
        }

        public override ControlStyle GetControlStyle(string field)
        {
            ControlStyle style = new ControlStyle();
            switch (field)
            {
                case "FilePath":
                    style.Visible = this.PskillFindType == PskillFindType.FilePath;
                    style.Variables = ControlStyle.GetVariables(true, false, true);
                    break;
                case "ProcessName":
                    style.Visible = this.PskillFindType == PskillFindType.ProcessName;
                    style.Variables = ControlStyle.GetVariables(true, false, true);
                    break;
                case "ProcIdVarName":
                    style.Visible = this.PskillFindType == PskillFindType.ProcessId;
                    style.Variables = ControlStyle.GetVariables(false, false, true);
                    break;
            }

            return style;
        }
    }
}
