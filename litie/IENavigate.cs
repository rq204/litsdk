using litsdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace litie
{
    internal class IENavigate
    {
        public static void Execute(litcore.browser.Navigate activity, ActivityContext context)
        {
            WebBrowser Browser_Select = IELoad.Browser_Select.IEBrowser;

            switch (activity.NavigateType)
            {
                case litcore.ictype.NavigateType.Url:
                    string url = context.ReplaceVar(activity.Url);
                    url = litcore.Utility.FillUrlHttp(url);
                    string referer = "";// context.ReplaceVar(activity.Referer);
                    string header = string.IsNullOrEmpty(referer) ? null : "Referer:" + referer;
                    //Browser_Select.Tag = activity.NewWindow;
                    Browser_Select.Navigate(url, null, null, header);
                    context.WriteLog("开始打开网页:" + url);
                    System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                    sw.Start();
                    System.Threading.Thread.Sleep(1000);
                    while (Browser_Select.ReadyState != WebBrowserReadyState.Complete)
                    {
                        context.ThrowIfCancellationRequested();
                        System.Threading.Thread.Sleep(300);
                        Application.DoEvents();
                        if (sw.ElapsedMilliseconds > activity.TimeOutSenconds * 1000)
                        {
                            sw.Stop();
                            context.WriteLog("打开网页超时");
                            break;
                        }
                    }
                    sw.Stop();
                    break;
                case litcore.ictype.NavigateType.Reload:
                    Browser_Select.Refresh();
                    context.WriteLog("浏览器开始刷新");
                    System.Threading.Thread.Sleep(1000);
                    break;
                case litcore.ictype.NavigateType.GoBack:
                    if (Browser_Select.CanGoBack)
                    {
                        Browser_Select.GoBack();
                        context.WriteLog("浏览器后退成功");
                    }
                    else
                    {
                        context.WriteLog("浏览器当前状态无后退");
                    }
                    break;
                case litcore.ictype.NavigateType.GoForward:
                    if (Browser_Select.CanGoForward)
                    {
                        Browser_Select.GoForward();
                        context.WriteLog("浏览器前进成功");
                    }
                    else
                    {
                        context.WriteLog("浏览器当前状态无前进");
                    }
                    break;
                case litcore.ictype.NavigateType.ToTab:
                    string data = context.ReplaceVar(activity.TabNameOrUrl);
                    string old = IELoad.Browser_Select.IEBrowser.Url.AbsoluteUri;

                    IELoad find = null;

                    foreach (IELoad ie in IELoad.WebBrowsers)
                    {
                        if (ie.IEBrowser.DocumentTitle == data || ie.IEBrowser.Url.AbsoluteUri == data || ie.IEBrowser.DocumentTitle.Contains(data) || ie.IEBrowser.Url.AbsoluteUri.Contains(data))
                        {
                            find = ie;
                            break;
                        }
                    }

                    if (find != null)
                    {
                        IELoad.Browser_Select = find;
                        context.WriteLog("成功切换至标签页：" + find.IEBrowser.DocumentTitle);
                    }
                    else
                    {
                        throw new Exception("没有找到指定的标签页：" + data);
                    }
                    break;
            }
            System.Windows.Forms.Application.DoEvents();
        }

    }
}