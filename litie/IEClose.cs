using litsdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace litie
{
    internal class IEClose
    {
        public static void Execute(litcore.browser.Close activity, ActivityContext context)
        {
            string log = "";
            string data = context.ReplaceVar(activity.TabNameOrUrl);

            if (activity.CloseTabPage)
            {
                if (string.IsNullOrEmpty(data))
                {
                    if (IELoad.Browser_Select != null)
                    {
                        IELoad.Browser_Select.Dispose();

                        log = "关闭当前标签页成功";

                        if (IELoad.WebBrowsers.Count > 0)
                        {
                            IELoad.Browser_Select = IELoad.WebBrowsers[0];
                            log += "、切换当前标签页为：" + IELoad.Browser_Select.IEBrowser.DocumentTitle;
                        }
                        else
                        {
                            IELoad.Browser_Select = null;
                        }
                    }
                    else
                    {
                        log = "没有当前标签页，所有页面都已关闭，取消关闭操作";
                    }
                }
                else
                {
                    int old = IELoad.Browser_Select.IEBrowser.GetHashCode();
                    IELoad find = null;
                    foreach (IELoad el in IELoad.WebBrowsers)
                    {
                        if (el.IEBrowser.DocumentTitle == data || el.IEBrowser.Url.AbsoluteUri == data || el.IEBrowser.DocumentTitle.Contains(data) || el.IEBrowser.Url.AbsoluteUri.Contains(data))
                        {
                            find = el;
                            break;
                        }
                    }

                    if (find != null)
                    {
                        bool notchange = find.IEBrowser.GetHashCode() == old;
                        IELoad.WebBrowsers.Remove(find);
                        find.Dispose();

                        log = "找到指定标签页并关闭";
                        if (notchange)//当前标签页和关闭标签页一样
                        {
                            if (IELoad.WebBrowsers.Count > 0)
                            {
                                IELoad.Browser_Select = IELoad.WebBrowsers[0];
                                log += "、切换当前标签页为：" + IELoad.Browser_Select.IEBrowser.DocumentTitle;
                            }
                            else
                            {
                                IELoad.Browser_Select = null;
                            }
                        }
                    }
                    else
                    {
                        log = "没有找到需要关闭的标签页";
                    }
                }
            }
            else
            {
                foreach (IELoad el in IELoad.WebBrowsers)
                {
                    el.Dispose();
                }
                IELoad.WebBrowsers.Clear();
                IELoad.Browser_Select = null;
                log = "已成功关闭所有内置IE浏览器";
            }
            context.WriteLog(log);
        }
    }
}
