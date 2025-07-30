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
    internal class IEElementGet
    {
        public static void Execute(litcore.browser.ElementGet activity, ActivityContext context)
        {
            System.Windows.Forms.WebBrowser Browser_Select = IELoad.Browser_Select.IEBrowser;

            SHDocVw.InternetExplorer browser = (SHDocVw.InternetExplorer)Browser_Select.ActiveXInstance;
            mshtml.IHTMLDocument2 htmlDoc = null;

            string frameName = context.ReplaceVar(activity.FrameName);

            if (!string.IsNullOrEmpty(frameName))
            {
                htmlDoc = IEXPath.FindFrame(frameName, Browser_Select.Document.DomDocument as mshtml.IHTMLDocument2);
                if (htmlDoc == null) htmlDoc = IEXPath.FindIFrame(frameName, Browser_Select.Document.DomDocument as mshtml.IHTMLDocument2);
            }
            else
            {
                htmlDoc = browser.Document as mshtml.IHTMLDocument2;
            }
            if (htmlDoc == null) throw new Exception("不存在框架:" + frameName);

            //先按xpath找到元素
            string xpathstr = context.ReplaceVar(activity.XPathStr);
            List<string> xpaths = xpathstr.Replace("\r", "").Split('\n').ToList();
            List<IHTMLElement> eles = new List<IHTMLElement>();
            eles = IEXPath.GetHTMLElementsByXPath(htmlDoc, xpaths);
            if (eles.Count == 0)
            {
                context.WriteLog("没有找到符合条件的元素");
                return;
            }

            if (context.ContainsStr(activity.SaveVarName))
            {
                string value = IEXPath.GetAttbute(eles[0], activity.Attribute);
                context.SetVarStr(activity.SaveVarName, value);
                context.WriteLog($"元素字符取值成功长度{value.Length}");
            }
            else if (context.ContainsList(activity.SaveVarName))
            {
                List<string> ls = new List<string>();
                foreach (IHTMLElement element in eles)
                {
                    ls.Add(IEXPath.GetAttbute(element, activity.Attribute));
                }
                context.SetVarList(activity.SaveVarName, ls);//原AddVarListList
                context.WriteLog($"元素列表取值成功{ls.Count}条");
            }
            else if (context.ContainsInt(activity.SaveVarName))
            {
                string value = IEXPath.GetAttbute(eles[0], activity.Attribute);
                int it = 0;
                if (int.TryParse(value, out it))
                {
                    context.SetVarInt(activity.SaveVarName, it);
                    context.WriteLog($"元素数字取值成功值为{it}");
                }
                else
                {
                    context.SetVarInt(activity.SaveVarName, 0);
                    context.WriteLog($"元素数字取值失败，设置值为0");
                }
            }
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