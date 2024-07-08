using System;
using System.Drawing;

namespace litsdk
{
    /// <summary>
    /// 绘图多个对接端口
    /// </summary>
    public abstract class Port
    {
        public Guid Id { get; private set; }

        public Guid NextNodeId { get; set; }

        public Rect Bounds { get; set; }

        public abstract Point GetOffset(Rect r);

        public Port()
        {
            this.Id = Guid.NewGuid();
        }

        public void ReSetId()
        {
            this.Id = Guid.NewGuid();
        }

        public void SetId(Guid guid)
        {
            this.Id = guid;
        }
    }

    public sealed class NextPort : Port
    {
        public override Point GetOffset(Rect r) => r.CenterBottom;
    }

    public sealed class TruePort : Port
    {
        public override Point GetOffset(Rect r) => r.CenterBottom;
    }

    public sealed class FalsePort : Port
    {
        public override Point GetOffset(Rect r) => r.CenterRight;
    }
}
