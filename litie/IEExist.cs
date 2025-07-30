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
    internal class IEExist
    {
        public static bool Execute(litcore.browser.Exist activity, ActivityContext context)
        {
            System.Windows.Forms.WebBrowser Browser_Select = IELoad.Browser_Select.IEBrowser;

            try
            {
                SHDocVw.InternetExplorer browser = (SHDocVw.InternetExplorer)Browser_Select.ActiveXInstance;
                mshtml.IHTMLDocument2 htmlDoc = null;

                if (!string.IsNullOrEmpty(activity.FrameName))
                {
                    string fname = context.ReplaceVar(activity.FrameName);
                    htmlDoc = IEXPath.FindFrame(fname, Browser_Select.Document.DomDocument as mshtml.IHTMLDocument2);
                }
                else
                {
                    htmlDoc = browser.Document as mshtml.IHTMLDocument2;
                }

                if (htmlDoc == null)
                {
                    if (activity.Reverse)
                    {
                        context.WriteLog("不存在框架:" + activity.FrameName + ",true");
                        return true;
                    }
                    else
                    {
                        context.WriteLog("不存在框架:" + activity.FrameName + ",结果为false");
                        return false;
                    }

                }
                //先按xpath找到元素
                string xpathstr = context.ReplaceVar(activity.XPathStr);
                List<string> xs = xpathstr.Replace("\r", "").Split('\n').ToList();
                List<IHTMLElement> eles = new List<IHTMLElement>();
                eles = IEXPath.GetHTMLElementsByXPath(htmlDoc, xs);
                System.Windows.Forms.Application.DoEvents();
                if (eles.Count == 0)
                {
                    if (activity.Reverse)
                    {
                        context.WriteLog("没有找到元素，取反为true");
                        return true;
                    }
                    else
                    {
                        context.WriteLog("没有找到元素");
                        return false;
                    }
                }
                else
                {
                    string log = "";
                    if (activity.SaveLocation)
                    {
                        int x = 0;
                        int y = 0;
                        IHTMLElement temp = eles[0];

                        while (temp != null)
                        {
                            x += temp.offsetLeft;
                            y += temp.offsetTop;
                            temp = temp.offsetParent as IHTMLElement;
                        }

                        // 加上页面的滚动位置
                        context.SetVarInt(activity.XPosVarName, x);
                        context.SetVarInt(activity.YPosVarName, y);
                        log = $"获取到首元素位置X{x}Y{y},";
                    }
                    if (activity.Reverse)
                    {
                        context.WriteLog($"{log}找到{eles.Count}个元素，取反为false");
                        return false;
                    }
                    else
                    {
                        context.WriteLog($"{log}找到{eles.Count}个元素");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                if (activity.Reverse)
                {
                    context.WriteLog("没有找到元素，取反为true：" + ex.Message);
                    return true;
                }
                else
                {
                    context.WriteLog("没有找到元素：" + ex.Message);
                    return false;
                }
            }
        }
    }
}