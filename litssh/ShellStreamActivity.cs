using litsdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace litssh
{
    [litsdk.Action(Name = "ShellStream", RefFile = "Renci.SshNet.dll")]
    internal class ShellStreamActivity : litsdk.ProcessActivity
    {
        public override string Name { get; set; } = "ShellStream";

        public string Host = "";

        public string Port = "";

        public string UserName = "";

        public string Password = "";

        /// <summary>
        /// 交互配置
        /// </summary>
        public List<SSHEcho> Echos = new List<SSHEcho>();

        public override void Execute(ActivityContext context)
        {
            
        }

        public override void Validate(ActivityContext context)
        {
     
        }

        public override ControlStyle GetControlStyle(string field)
        {
            throw new NotImplementedException();
        }
    }
}
