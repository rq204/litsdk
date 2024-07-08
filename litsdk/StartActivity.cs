using System;
using System.Linq;

namespace litsdk
{
    /// <summary>
    /// 开始
    /// </summary>
    public sealed class StartActivity : Activity
    {
        /// <summary>
        /// 
        /// </summary>
        public override string Name { set { } get => "开始"; }

        public override ControlStyle GetControlStyle(string field) { return new ControlStyle() { Visible = true }; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public override void Validate(ActivityContext context)
        {
        }
    }
}