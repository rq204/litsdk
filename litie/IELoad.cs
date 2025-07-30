using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using litsdk;
using static System.Net.Mime.MediaTypeNames;

namespace litie
{
    /// <summary>
    /// ie浏览器,调试时因为有保护，加密后就没有了
    /// https://blog.csdn.net/CharlesSimonyi/article/details/30479131
    /// </summary>
    public class IELoad
    {
        /// <summary>
        /// 选中的浏览器
        /// </summary>
        internal static IELoad Browser_Select;

        /// <summary>
        /// 所有的浏览器
        /// </summary>
        public static List<IELoad> WebBrowsers = new List<IELoad>();

        internal const string IEFile = "Interop.SHDocVw.dll,Microsoft.mshtml.dll";

        private SHDocVw.InternetExplorer browser;

        public WebBrowser IEBrowser = new WebBrowser();

        public bool NewWindow = false;

        public IELoad(bool NewWindow)
        {
            IEBrowser.ScriptErrorsSuppressed = true; //禁用错误脚本提示
            IEBrowser.IsWebBrowserContextMenuEnabled = false; // 禁用右键菜单
            IEBrowser.WebBrowserShortcutsEnabled = false; //禁用快捷键
            this.browser = (SHDocVw.InternetExplorer)this.IEBrowser.ActiveXInstance;
            this.browser.NewWindow3 += Browser_NewWindow3;
            this.browser.DocumentComplete += Browser_DocumentComplete;
            this.browser.TitleChange += Browser_TitleChange;
            this.NewWindow = NewWindow;
            if (litsdk.API.GetDesignActivityContext() != null)//设计器
            {
                this.tmXPath = new System.Threading.Timer(new System.Threading.TimerCallback(tmXPath_Tick), null, 300, 300);
            }
        }

        private void Browser_TitleChange(string Text)
        {
            if (this.tabPage == null) return;
            this.tabPage.ToolTipText = Text;
            if (Text.Length > 25) Text = Text.Substring(0, 25);
            tabPage.Text = Text;
        }

        private void Browser_DocumentComplete(object pDisp, ref object URL)
        {
            if (URL.Equals("about:blank")) return;

            mshtml.IHTMLDocument2 htmlDoc = browser.Document as mshtml.IHTMLDocument2;

            mshtml.IHTMLWindow2 win = (mshtml.IHTMLWindow2)htmlDoc.parentWindow;
            //https://zhidao.baidu.com/question/1689719994073203468.html
            win.execScript("function alert(s){return true;} ", "javaScript");
            win.execScript("function confirm(s){return true;} ", "javaScript");
            win.execScript("function close() { } ", "javaScript");

            this.IEBrowser.Document.ContextMenuShowing -= Document_ContextMenuShowing;
            this.IEBrowser.Document.ContextMenuShowing += Document_ContextMenuShowing;
        }

        private void Document_ContextMenuShowing(object sender, HtmlElementEventArgs e)
        {
            StopXPath = !StopXPath;
            if (StopXPath) this.findXPath.clearBorderHint();
        }

        public bool StopXPath = false;
        public TabPage tabPage = null;

        private void Browser_NewWindow3(ref object ppDisp, ref bool Cancel, uint dwFlags, string bstrUrlContext, string bstrUrl)
        {
            Cancel = true;
            if (NewWindow)
            {
                string header = string.IsNullOrEmpty(bstrUrlContext) ? null : "Referer:" + bstrUrlContext;
                IELoad iE = new IELoad(this.NewWindow);
                IELoad.WebBrowsers.Add(iE);
                iE.browser.Navigate(bstrUrl, null, null, null, header);
                litsdk.API.AddTabPage(iE);
                if (iE.IEBrowser.Parent != null && iE.IEBrowser.Parent.GetType() == typeof(TabPage))
                {
                    iE.IEBrowser.Parent.Text = bstrUrlContext;
                    this.tabPage = iE.IEBrowser.Parent as TabPage;
                }
            }
            else
            {
                this.browser.Navigate(bstrUrl);
            }
        }

        public void Dispose()
        {
            if (this.tmXPath != null) this.tmXPath.Dispose();
            this.IEBrowser.Dispose();
            if (this.tabPage != null) this.tabPage.Dispose();
        }

        private FindXPath findXPath = new FindXPath();
        private System.Threading.Timer tmXPath;

        private void tmXPath_Tick(object sender)
        {
            if (StopXPath) return;
            if (litsdk.API.SetXPath == null) return;
            if (this.tabPage == null) return;
            if (!this.tabPage.ContainsFocus) return;
            litsdk.API.GetMainForm().Invoke((EventHandler)delegate
            {
                if (!StopXPath && this.tabPage.ClientRectangle.Contains(this.tabPage.PointToClient(Control.MousePosition)))
                {
                    findXPath.captureIE(litsdk.API.SetXPath);
                }
            });
        }

        ///// <summary>
        ///// https://stackoverflow.com/questions/8302933/how-to-get-around-the-memory-leak-in-the-net-webbrowser-control
        ///// </summary>
        ///// <param name="_browser"></param>
        //public void Dispose(WebBrowser _browser)
        //{
        //    mshtml.IHTMLDocument2 htmldoc = _browser.Document.DomDocument as mshtml.IHTMLDocument2;
        //    _browser.DocumentText = "";
        //    mshtml.IHTMLWindow2 win = (mshtml.IHTMLWindow2)htmldoc.parentWindow;
        //    _browser.Dispose();
        //}
    }
}
