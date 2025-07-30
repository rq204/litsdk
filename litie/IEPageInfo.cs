using litsdk;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace litie
{
    internal class IEPageInfo
    {
        public static void Execute(litcore.browser.PageInfo activity, ActivityContext context)
        {
            WebBrowser Browser_Select = IELoad.Browser_Select.IEBrowser;

            if (!string.IsNullOrEmpty(activity.HtmlVarName))
            {
                string html = Browser_Select.Document.Body.OuterHtml;
                try
                {
                    html = GetHtml(Browser_Select);
                }
                catch { }
                context.SetVarStr(activity.HtmlVarName, html);
            }
            if (!string.IsNullOrEmpty(activity.UrlVarName))
            {
                context.SetVarStr(activity.UrlVarName, Browser_Select.Url.AbsoluteUri);
            }
            if (!string.IsNullOrEmpty(activity.TitleVarName))
            {
                context.SetVarStr(activity.TitleVarName, Browser_Select.Document.Title);
            }
            if (!string.IsNullOrEmpty(activity.ImagesVarName))
            {
                List<string> imgs = new List<string>();
                foreach (HtmlElement link in Browser_Select.Document.Images)
                {
                    string src = link.GetAttribute("src");
                    if (!string.IsNullOrEmpty(src)) imgs.Add(src);
                }
                imgs = litcore.browser.PageInfo.FillUrl(imgs, Browser_Select.Url.AbsoluteUri);
                context.SetVarList(activity.ImagesVarName, imgs);
            }

            if (!string.IsNullOrEmpty(activity.HrefsVarName))
            {
                List<string> urls = new List<string>();
                foreach (HtmlElement link in Browser_Select.Document.Links)
                {
                    string lk = link.GetAttribute("href");
                    urls.Add(lk);
                }
                urls = litcore.browser.PageInfo.FillUrl(urls, Browser_Select.Url.AbsoluteUri);
                context.SetVarList(activity.HrefsVarName, urls);
            }
            context.WriteLog($"获取页面信息成功");
            System.Windows.Forms.Application.DoEvents();
        }


        public static string GetHtml(WebBrowser Browser_Select)
        {
            var documentRootNodes = Browser_Select.Document.GetElementsByTagName("*");
            var documentHtml = string.Empty;
            foreach (HtmlElement node in documentRootNodes)
            {
                if (node.Parent == null)
                {
                    documentHtml += node.OuterHtml;
                }
            }

            foreach (HtmlWindow frame in Browser_Select.Document.Window.Frames)
            {
                gethtml(frame, ref documentHtml);
            }

            return documentHtml;
        }

        private static void gethtml(HtmlWindow frame, ref string documentHtml)
        {
            var frameRootNodes = frame.Document.GetElementsByTagName("*");
            var frameHtml = string.Empty;
            foreach (HtmlElement node in frameRootNodes)
            {
                if (node.Parent == null)
                {
                    frameHtml += node.OuterHtml;
                }
            }
            documentHtml += frameHtml;
            foreach (HtmlWindow f in frame.Document.Window.Frames)
            {
                gethtml(f, ref documentHtml);
            }
        }

        
    }
}