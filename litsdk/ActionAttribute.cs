using System;

namespace litsdk
{
    /// <summary>
    /// 基类的属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ActionAttribute : Attribute
    {
        /// <summary>
        /// 中文名，目前不做其它语言版本
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 所属分类,比如  网络/FTP
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// 图标编号，如 network ,ftp
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// 是否要前端操作
        /// </summary>
        public bool IsFront { get; set; }

        /// <summary>
        /// 是否跨平台
        /// </summary>
        public bool IsLinux { get; set; }

        /// <summary>
        /// 索引，控件位置排序使用
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// 帮助链接，如本地链接或http链接
        /// </summary>
        public string HelpLink { get; set; }

        /// <summary>
        /// 引用文件，以逗号分割，如 a.dll,b.txt,c.exe
        /// </summary>
        public string RefFile { get; set; }

        /// <summary>
        /// 中文描述，说明这个组件的功能
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 配置提示，如 对字符 {a} 使用正则 {b} 提取数据并保存至 {c}
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// 关联的组件全名
        /// </summary>
        public string ActivityFullName { get; set; }
    }
}