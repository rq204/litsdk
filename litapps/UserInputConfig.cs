using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace litapps
{
    /// <summary>
    /// 用户控件类型
    /// </summary>
    public class UserInputConfig
    {
        public string Title { get; set; }

        public UserInputType Type { get; set; } = UserInputType.TextBox;

        public string DefaultVarName { get; set; }

        public string ValueVarName { get; set; }

        public bool CanEmpty { get; set; }

        [JsonIgnore]
        public System.Windows.Forms.Label Label;

        /// <summary>
        /// 单个元素的
        /// </summary>
        [JsonIgnore]
        public System.Windows.Forms.Control Control;

        /// <summary>
        /// 多个元素的
        /// </summary>
        [JsonIgnore]
        public List<System.Windows.Forms.Control> Controls;

        [JsonIgnore]
        public System.Windows.Forms.Panel Panel;
    }
}
