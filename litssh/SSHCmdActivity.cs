using litsdk;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace litssh
{
    [litsdk.Action(Name = "SSH命令行", Category = "网络", IsLinux = true, Order = 30, Description = "可以登录使用SSH执行操作", RefFile = "Renci.SshNet.dll")]
    public class SSHCmdActivity : litsdk.ProcessActivity
    {
        public override string Name { get; set; } = "SSH命令行";

        [Argument(Name = "主机地址", ControlType = ControlType.TextBox, Order = 1, Description = "服务器地址")]
        public string Host { get; set; }

        [Argument(Name = "端口", ControlType = ControlType.NumericUpDown, Order = 2, Description = "远程端口")]
        public int Port { get; set; } = 22;

        [Argument(Name = "用户名", ControlType = ControlType.TextBox, Order = 3, Description = "登录帐号")]
        public string UserName { get; set; }

        [Argument(Name = "密码", ControlType = ControlType.Password, Order = 4, Description = "登录密码")]
        public string Password { get; set; }

        [Argument(Name = "执行命令", ControlType = ControlType.TextArea, Order = 5, Description = "执行的SSH命令")]
        public string CmdStr { get; set; }

        [Argument(Name = "结果存入", ControlType = ControlType.Variable, Order = 6, Description = "执行ssh命令后，系统返回的结果")]
        public string SaveVarName { get; set; }

        /// <summary>
        /// https://blog.csdn.net/weixin_30580943/article/details/99622989
        /// </summary>
        /// <param name="context"></param>
        public override void Execute(ActivityContext context)
        {
            string host = context.ReplaceVar(this.Host);
            string username = context.ReplaceVar(this.UserName);
            string password = context.ReplaceVar(this.Password);
            string cmdstr = context.ReplaceVar(this.CmdStr);

            using (var sshClient = new SshClient(host, this.Port, username, password))
            {
                sshClient.Connect();
                using (var cmd = sshClient.CreateCommand(cmdstr))
                {
                    var res = cmd.Execute();
                    if (!string.IsNullOrEmpty(this.SaveVarName)) context.SetVarStr(this.SaveVarName, res);
                    context.WriteLog($"成功执行命令 {cmdstr} 结果 {res}");
                }
            }
        }

        public override void Validate(ActivityContext context)
        {
            if (string.IsNullOrEmpty(this.Host)) throw new Exception("主机地址不能为空");
            if (this.Port == 0) throw new Exception("端口不能为0");
            if (string.IsNullOrEmpty(this.UserName)) throw new Exception("用户名不能为空");
            if (string.IsNullOrEmpty(this.Password)) throw new Exception("密码不能为空");
            if (string.IsNullOrEmpty(this.CmdStr)) throw new Exception("命令行不能为空");
            if (!string.IsNullOrEmpty(this.SaveVarName) && !context.ContainsStr(this.SaveVarName)) throw new Exception("保存字符串不能为空");
        }

        public override ControlStyle GetControlStyle(string field)
        {
            ControlStyle style = new ControlStyle();
            switch (field)
            {
                case "CmdStr":
                    style.Variables = ControlStyle.GetVariables(true, false, true);
                    break;
                case "SaveVarName":
                    style.Variables = ControlStyle.GetVariables(true);
                    break;
                case "Port":
                    style.Max = 655535;
                    style.Min = 1;
                    break;
                default:
                    style.Variables = ControlStyle.GetVariables(true, false, true);
                    break;
            }
            return style;
        }
    }
}