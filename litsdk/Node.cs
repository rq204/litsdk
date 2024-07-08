using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace litsdk
{
    /// <summary>
    /// 节点配置
    /// </summary>
    public abstract class Node
    {
        public Guid Id { get; set; }

        public Rect Bounds { get; set; }

        public Activity Activity { get; set; }

        public List<Port> Ports { get; set; }

        protected Node()
        {
            this.Ports = new List<Port>();
            this.RenderedPorts = new Dictionary<Port, object>();
        }

        public static int GridSize = 20;
        public Node(Activity activity) : this()
        {
            this.Activity = activity;
            this.Id = Guid.NewGuid();
            this.Bounds = new Rectangle(
                GridSize * 0,
                GridSize * 0,
                GridSize * 4,
                GridSize * 2);
        }

        [JsonIgnore]
        public Dictionary<Port, object> RenderedPorts { get; set; }

        [JsonIgnore]
        public virtual bool CanStartLink => true;

        [JsonIgnore]
        public virtual bool CanEndLink => true;

        public abstract Guid Execute(ActivityContext context);
    }
}