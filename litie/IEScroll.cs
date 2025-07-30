using litsdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace litie
{
    internal class IEScroll
    {
        public static void Execute(litcore.browser.Scroll activity, ActivityContext context)
        {
            System.Windows.Forms.WebBrowser Browser_Select = IELoad.Browser_Select.IEBrowser;

            switch (activity.ScrollType)
            {
                case litcore.ictype.ScrollType.Top:
                    Browser_Select.Document.Window.ScrollTo(0, 0);
                    context.WriteLog($"滚动条移至顶端成功");
                    break;
                case litcore.ictype.ScrollType.Botton:
                    Browser_Select.Document.Window.ScrollTo(0, Browser_Select.Document.Body.ScrollRectangle.Height);
                    context.WriteLog($"滚动条移至底部成功");
                    break;
                case litcore.ictype.ScrollType.DownBy:
                case litcore.ictype.ScrollType.UpBy:
                    int inum = activity.ScrollBy;
                    if (activity.ScrollType == litcore.ictype.ScrollType.UpBy) inum = 0 - inum;

                    SHDocVw.InternetExplorer browser = (SHDocVw.InternetExplorer)Browser_Select.ActiveXInstance;
                    mshtml.IHTMLDocument2 htmlDoc = null;

                    htmlDoc = browser.Document as mshtml.IHTMLDocument2;
                    mshtml.IHTMLWindow2 win = (mshtml.IHTMLWindow2)htmlDoc.parentWindow;
                    var result = win.execScript($"window.scrollBy(0,{inum})");

                    if (activity.ScrollType == litcore.ictype.ScrollType.DownBy)
                    {
                        context.WriteLog($"滚动条向下{activity.ScrollBy}像素滚动成功");
                    }
                    else
                    {
                        context.WriteLog($"滚动条向上{activity.ScrollBy}像素滚动成功");
                    }

                    break;
            }

            if (activity.Sleep > 0)
            {
                System.Threading.Thread.Sleep(activity.Sleep);
            }
            System.Windows.Forms.Application.DoEvents();
        }
        
    }
}