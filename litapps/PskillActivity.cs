using litsdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace litapps
{
    [litsdk.Action(Name = "关闭进程", Category = "系统", Order = 4, Description = "可以关闭指定的进程")]
    public class PskillActivity : litsdk.ProcessActivity
    {
        public override string Name { get; set; } = "关闭进程";

        [Argument(Name = "关闭方式", ControlType = ControlType.ComboBox, Order = 1, Description = "是按进程名还是路径或是进程id关闭进程")]
        public PskillFindType PskillFindType { get; set; } = PskillFindType.ProcessName;

        [Argument(Name = "进程名称", ControlType = ControlType.TextBox, Order = 2, Description = "按进程名关闭进程")]
        public string ProcessName { get; set; }

        [Argument(Name = "进程路径", Order = 3, ControlType = ControlType.File, Description = "按进程路径关闭进程")]
        public string FilePath { get; set; }

        [Argument(Name = "进程ID", Order = 4, ControlType = ControlType.Variable, Description = "按进程id关闭进程")]
        public string ProcIdVarName { get; set; }

        [Argument(Name = "关闭选项", ControlType = ControlType.ComboBox, Order = 5, Description = "有多个进程时，如何关闭")]
        public PskillCloseType PskillCloseType { set; get; } = PskillCloseType.Earliest;

        public override void Execute(ActivityContext context)
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
                    System.Diagnostics.Process p = System.Diagnostics.Process.GetProcessById(pid);
                    if (p != null) ps.Add(p);
                    break;
            }

            if (ps.Count == 0)
            {
                context.WriteLog("没有找到需要关闭的进程");
            }
            else
            {
                ps = ps.OrderByDescending((h) => h.StartTime).ToList();
                switch (this.PskillCloseType)
                {
                    case PskillCloseType.Earliest:
                        ps = new List<System.Diagnostics.Process>() { ps[ps.Count - 1] };
                        break;
                    case PskillCloseType.Latest:
                        ps = new List<System.Diagnostics.Process>() { ps[0] };
                        break;
                }

                foreach (System.Diagnostics.Process pk in ps)
                {
                    try
                    {
                        string log = $"尝试关闭进程 {pk.ProcessName}，PID = {pk.Id}";
                        context.WriteLog(log);

                        // 正常关闭
                        if (!pk.HasExited)
                        {
                            pk.CloseMainWindow();
                            if (pk.WaitForExit(3000))
                            {
                                context.WriteLog($"成功通过 CloseMainWindow 关闭进程 {pk.ProcessName}，PID = {pk.Id}");
                                continue;
                            }
                        }

                        // 强制 Kill
                        if (!pk.HasExited)
                        {
                            pk.Kill();
                            if (pk.WaitForExit(3000))
                            {
                                context.WriteLog($"成功通过 Kill 强制关闭进程 {pk.ProcessName}，PID = {pk.Id}");
                                continue;
                            }
                        }

                        // 最后一招：taskkill /F
                        if (!pk.HasExited)
                        {
                            var psi = new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = "taskkill",
                                Arguments = $"/PID {pk.Id} /F",
                                CreateNoWindow = true,
                                UseShellExecute = false,
                                RedirectStandardOutput = true,
                                RedirectStandardError = true,
                            };

                            using (var taskkillProc = System.Diagnostics.Process.Start(psi))
                            {
                                string output = taskkillProc.StandardOutput.ReadToEnd();
                                string error = taskkillProc.StandardError.ReadToEnd();
                                taskkillProc.WaitForExit(3000);

                                if (pk.HasExited)
                                {
                                    context.WriteLog($"成功通过 taskkill 关闭进程 {pk.ProcessName}，PID = {pk.Id}");
                                }
                                else
                                {
                                    context.WriteLog($"失败：即使通过 taskkill 仍无法终止进程 {pk.ProcessName}，PID = {pk.Id}");
                                }

                                if (!string.IsNullOrWhiteSpace(output))
                                    context.WriteLog($"taskkill 输出：{output}");
                                if (!string.IsNullOrWhiteSpace(error))
                                    context.WriteLog($"taskkill 错误：{error}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        context.WriteLog($"错误：关闭进程 {pk.ProcessName}（PID = {pk.Id}）失败：{ex.Message}");
                    }
                }

            }
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
                case "PskillCloseType":
                    style.Visible = this.PskillFindType != PskillFindType.ProcessId;
                    break;
            }

            return style;
        }
    }
}
