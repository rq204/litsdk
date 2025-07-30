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
    internal class IEElementSet
    {
        public static void Execute(litcore.browser.ElementSet activity, ActivityContext context)
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
                context.WriteLog("没有找到符合条件的元素");
                return;
            }

            string v = context.ReplaceVar(activity.SetValue);
            foreach (IHTMLElement element in eles)
            {
                switch (activity.Attribute)
                {
                    case "innerHTML":
                        element.innerHTML = v;
                        break;
                    case "outerHTML":
                        element.outerHTML = v;
                        break;
                    case "innerText":
                        element.innerText = v;
                        break;
                    case "outerText":
                        element.outerText = v;
                        break;
                    case "textContent":
                        element.innerText = v;
                        break;
                    default:
                        element.setAttribute(activity.Attribute, v);

                        ////vue的处理下边的无效果
                        //if (element.tagName == "INPUT")
                        //{
                        //    mshtml.IHTMLDocument4 docEv = element.document as mshtml.IHTMLDocument4;
                        //    docEv.FireEvent("change", null);

                        //    //object input = null;
                        //    //IHTMLElement3 hTMLElement3 = element as IHTMLElement3;
                        //    //hTMLElement3.FireEvent("oninput", ref input);
                        //}
                        break;
                }
            }
            context.WriteLog($"{eles.Count}个元素设置值成功");
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