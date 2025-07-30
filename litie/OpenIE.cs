using litcore.browser;
using litsdk;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace litie
{
    [Action(Name = "打开内置IE浏览器", Category = "浏览器", Order = 3,RefFile = "Interop.SHDocVw.dll,Microsoft.mshtml.dll",Description = "打开内置的IE浏览器，使用的是电脑系统已安装的IE组件")]
    public class OpenIE : litsdk.ProcessActivity, litcore.browser.IBrowser
    {
        [JsonIgnore]
        [Argument(Name = "浏览器类型", ControlType = ControlType.Example, Order = 1, Description = "内置IE浏览器，使用本机当前IE的版本及设置")]
        public string BrowserType => "内置IE浏览器";

        [Argument(Name = "初始网址", ControlType = ControlType.TextBox, Order = 2, Description = "打开浏览器时，默认打开的网址")]
        public string Url { get; set; }

        [Argument(Name = "允许新窗口打开", ControlType = ControlType.CheckBox, Order = 3, Description = "不选则所有网址都在一个窗口中打开")]
        public bool NewWindow { get; set; }

        public override string Name { get; set; } = "打开内置IE浏览器";
        public int TimeOutSenconds { get; set; } = 60;

        [JsonIgnore]
        public List<string> Supports
        {
            get
            {
                List<string> supported = litcore.browser.XPathJs.Supports.ToArray().ToList();
                return supported;
            }
        }

        public override void Execute(ActivityContext context)
        {
            string url = context.ReplaceVar(this.Url);
            url = litcore.Utility.FillUrlHttp(url);

            litsdk.API.GetMainForm().Invoke((EventHandler)delegate
            {
                IELoad load = new IELoad(this.NewWindow);
                IELoad.WebBrowsers.Add(load);
                IELoad.Browser_Select = load;
                litsdk.API.AddTabPage(IELoad.Browser_Select.IEBrowser);

                if (IELoad.Browser_Select.IEBrowser.Parent is TabPage tp)
                {
                    tp.Text = this.Name;
                    load.tabPage = tp;
                }

                IELoad.Browser_Select.IEBrowser.Navigate(url);

                context.WriteLog("开始打开网页:" + url);
                System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                sw.Start();
                System.Threading.Thread.Sleep(1000);
                while (IELoad.Browser_Select.IEBrowser.ReadyState != WebBrowserReadyState.Complete)
                {
                    context.ThrowIfCancellationRequested();
                    System.Threading.Thread.Sleep(300);
                    Application.DoEvents();
                    if (sw.ElapsedMilliseconds > this.TimeOutSenconds * 1000)
                    {
                        sw.Stop();
                        context.WriteLog($"打开网页{this.TimeOutSenconds}秒超时");
                        break;
                    }
                }
                sw.Stop();
                System.Windows.Forms.Application.DoEvents();
            });
        }

        public bool ExecuteDe(Activity activity, ActivityContext context)
        {
            bool result = false;
            switch (activity.GetType().Name)
            {
                case "Exist":
                    result = IEExist.Execute(activity as litcore.browser.Exist, context);
                    break;
            }
            return result;
        }

        public void ExecuteEx(Activity activity, ActivityContext context)
        {
            if (IELoad.Browser_Select == null && activity.GetType().Name != "Close" && activity.GetType().Name != "Navigate") throw new Exception("发生错误，当前标签页为空");

            litsdk.API.GetMainForm().Invoke((EventHandler)delegate
            {
                switch (activity.GetType().Name)
                {
                    case "ClickDown":

                        break;
                    case "ClickUpload":

                        break;
                    case "Close":
                        IEClose.Execute(activity as Close, context);
                        break;
                    case "Cookies":
                        IECookies.Execute(activity as Cookies, context);
                        break;
                    case "ElementClick":
                        IEElementClick.Execute(activity as ElementClick, context);
                        break;
                    case "ElementGet":
                        IEElementGet.Execute(activity as ElementGet, context);
                        break;
                    case "ElementSet":
                        IEElementSet.Execute(activity as ElementSet, context);
                        break;
                    case "Mouse":

                        break;
                    case "Navigate":
                        IENavigate.Execute(activity as Navigate, context);
                        break;
                    case "PageInfo":
                        IEPageInfo.Execute(activity as PageInfo, context);
                        break;
                    case "Proxy":
                        break;
                    case "RunJs":
                        IERunJs.Execute(activity as RunJs, context);
                        break;
                    case "ScreenShot":
                        IEScreenShot.Execute(activity as ScreenShot, context);
                        break;
                    case "Scroll":
                        IEScroll.Execute(activity as Scroll, context);
                        break;
                    case "Slide":

                        break;
                    case "Sniffer":

                        break;
                    case "ToPdf":
                        //EdgeToPdf.Execute(activity as litcore.browser.ToPdf, context);
                        break;
                    case "Wait":
                        IEWait.Execute(activity as litcore.browser.Wait, context);
                        break;
                }
            });
        }

        public override ControlStyle GetControlStyle(string field)
        {
            ControlStyle style = new ControlStyle();
            style.Variables = ControlStyle.GetVariables(true, false, true);
            return style;
        }

        public override void Validate(ActivityContext context)
        {
            if (string.IsNullOrEmpty(this.Url)) throw new Exception("默认打开网址不能为空");

        }
    }
}
