using litsdk;
using mshtml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace litie
{
    internal class IEElementClick
    {
        public static void Execute(litcore.browser.ElementClick activity, ActivityContext context)
        {
            System.Windows.Forms.WebBrowser Browser_Select = IELoad.Browser_Select.IEBrowser;

            SHDocVw.InternetExplorer browser = (SHDocVw.InternetExplorer)Browser_Select.ActiveXInstance;
            mshtml.IHTMLDocument2 htmlDoc = null;

            if (!string.IsNullOrEmpty(activity.FrameName))
            {
                htmlDoc = IEXPath.FindFrame(activity.FrameName, Browser_Select.Document.DomDocument as mshtml.IHTMLDocument2);
                if (htmlDoc == null) htmlDoc = IEXPath.FindIFrame(activity.FrameName, Browser_Select.Document.DomDocument as mshtml.IHTMLDocument2);
            }
            else
            {
                htmlDoc = browser.Document as mshtml.IHTMLDocument2;
            }
            if (htmlDoc == null) throw new Exception("不存在框架:" + activity.FrameName);

            //先按xpath找到元素
            string xpathstr = context.ReplaceVar(activity.XPathStr);
            List<string> xs = xpathstr.Replace("\r", "").Split('\n').ToList();
            List<IHTMLElement> eles = new List<IHTMLElement>();
            eles = IEXPath.GetHTMLElementsByXPath(htmlDoc, xs);
            if (eles.Count == 0)
            {
                throw new Exception("没有找到点击的元素");
            }

            foreach (IHTMLElement element in eles)
            {
                //if (activity.Equals("click"))
                //{
                element.click();
                //}
                //else
                //{
                //    todo 其它的事件 https://blog.csdn.net/Gbing1228/article/details/103949008
                //}
            }
            context.WriteLog("元素点击成功");
            System.Windows.Forms.Application.DoEvents();

            if (activity.Sleep > 0)
            {
                System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
                stopwatch.Start();
                while (stopwatch.ElapsedMilliseconds < activity.Sleep)
                {
                    System.Threading.Thread.Sleep(100);
                    System.Windows.Forms.Application.DoEvents();
                    context.ThrowIfCancellationRequested();
                }
                stopwatch.Stop();
            }
        }
    }
}