using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace litsdk
{
    /// <summary>
    /// ui选择的设置
    /// </summary>
    public class UIConfig
    {
        /// <summary>
        /// 进程名
        /// </summary>
        public string ProcessName { get; set; }

        /// <summary>
        /// 元素的XPath
        /// </summary>
        public string XPath { get; set; }

        /// <summary>
        /// 选中元素的图片
        /// </summary>
        public string ImgBase64 { get; set; }

        /// <summary>
        /// 是否选窗口
        /// </summary>
        public bool IsWindow { get; set; }

        /// <summary>
        /// 窗口标题
        /// </summary>
        public string WindowTitle { get; set; }

        public void Validate(ActivityContext context)
        {
            if (string.IsNullOrEmpty(this.ProcessName)) throw new Exception("进程名不能为空");
            if (this.IsWindow)
            {
                if (string.IsNullOrEmpty(this.WindowTitle)) throw new Exception("窗口标题不能为空");
            }
            else
            {
                if (string.IsNullOrEmpty(this.XPath)) throw new Exception("XPath不能为空");
            }
        }

        [Newtonsoft.Json.JsonIgnore]
        /// <summary>
        /// 重新加载界面
        /// </summary>
        public Action Config2UI;
    }
}
