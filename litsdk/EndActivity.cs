using System;

namespace litsdk
{
    /// <summary>
    /// 流程结束
    /// </summary>
    [litsdk.Action(Name = "结束", Order = 3, IsLinux = true, Category = "流控制", Description = "结束流程的运行，一个流程可以有多个结束")]
    public sealed class EndActivity : Activity
    {
        /// <summary>
        /// 组件名称
        /// </summary>
        public override string Name { set { } get => "结束"; }

        /// <summary>
        /// 流程组件配置检测
        /// </summary>
        /// <param name="context"></param>
        public override void Validate(ActivityContext context)
        {
        }

        public override ControlStyle GetControlStyle(string field) { return new ControlStyle() { Visible = true }; }
    }
}
