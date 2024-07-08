using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace litsdk
{
    /// <summary>
    ///  litrpa对外公开api
    /// </summary>
    public class API
    {
        /// <summary>
        /// 得到设计器中当前上下文内容
        /// </summary>
        public static Func<ActivityContext> GetDesignActivityContext;

        /// <summary>
        /// 设计器当前流程名
        /// </summary>
        public static Func<string> GetDesignProjectName;

        /// <summary>
        /// 当在设计器当中切换流程时,主要是关闭相关的资源
        /// </summary>
        public static Action OnDesignProjectChange;

        /// <summary>
        /// 显示设计器中的变量编辑器
        /// </summary>
        public static Action ShowDesignVariableForm { get; set; }

        /// <summary>
        /// 显示添加变量的界面
        /// </summary>
        public static Action ShowAddVariableForm { get; set; }

        /// <summary>
        /// 获取引用流程的内容
        /// </summary>
        public static Func<string, string> GetRefProjectStr;

        /// <summary>
        /// 获取可用引用流程列表
        /// </summary>
        public static Func<List<string>> GetRefProjectList;

        /// <summary>
        /// 设置 xpath
        /// </summary>
        public static Action<string, List<string>> SetXPath;

        /// <summary>
        /// 设置ui
        /// </summary>
        public static Action<litsdk.UIConfig> SetUIXPath;

        /// <summary>
        /// 安装webview2
        /// </summary>
        public static Action InstallWebView2;

        /// <summary>
        /// 退出整个程序时要做的事情
        /// </summary>
        public static Action OnExit;

#if NET461
        /// <summary>
        /// 设计器或是winform主窗口
        /// </summary>
        public static Func<System.Windows.Forms.Form> GetMainForm;

        /// <summary>
        /// 添加新的tabpage
        /// </summary>
        public static Action<object> AddTabPage;
#endif
    }
}
