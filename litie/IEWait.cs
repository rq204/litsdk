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
    internal class IEWait
    {
        static DateTime lastLogTime = DateTime.Now;

        public static void Execute(litcore.browser.Wait activity, ActivityContext context)
        {
            WebBrowser Browser_Select = IELoad.Browser_Select.IEBrowser;

            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            lastLogTime = DateTime.Now;

            try
            {
                string xpahtstr = context.ReplaceVar(activity.XPathStr).Trim();
                if (xpahtstr == "")
                {
                    throw new Exception("xpath为空请检查");
                }
                List<string> xpaths = xpahtstr.Replace("\r", "").Split('\n').ToList();

                while (true)
                {
                    bool ok = false;

                    SHDocVw.InternetExplorer browser = (SHDocVw.InternetExplorer)Browser_Select.ActiveXInstance;
                    mshtml.IHTMLDocument2 htmlDoc = null;

                    if (!string.IsNullOrEmpty(activity.FrameName))
                    {
                        htmlDoc = IEXPath.FindFrame(activity.FrameName, Browser_Select.Document.DomDocument as mshtml.IHTMLDocument2);
                        if (htmlDoc == null)
                        {
                            if (!activity.WaitIframe)
                            {
                                throw new Exception("不存在框架:" + activity.FrameName);
                            }
                            else
                            {
                                if (showLog()) context.WriteLog($"没找到框架：{ activity.FrameName}，等待中");
                                System.Threading.Thread.Sleep(100);
                                System.Windows.Forms.Application.DoEvents();
                                context.ThrowIfCancellationRequested();
                                continue;
                            }
                        }
                    }
                    else
                    {
                        htmlDoc = browser.Document as mshtml.IHTMLDocument2;
                    }

                    //先按xpath找到元素
                    string xpathstr = context.ReplaceVar(activity.XPathStr);
                    List<string> xs = xpathstr.Replace("\r", "").Split('\n').ToList();
                    List<IHTMLElement> eles = new List<IHTMLElement>();
                    eles = IEXPath.GetHTMLElementsByXPath(htmlDoc, xs);
                    if (eles.Count == 0)
                    {
                        if (activity.WaitType== litcore.ictype.WaitType.Appear)
                        {
                            if (showLog()) context.WriteLog("没有找到元素，继续等待");
                        }
                        else
                        {
                            context.WriteLog("元素已经消失，跳出等待");
                            ok = true;
                        }
                    }
                    else
                    {
                        if (activity.WaitType == litcore.ictype.WaitType.Appear)
                        {
                            context.WriteLog("成功找到元素，跳出等待");
                            ok = true;
                        }
                        else
                        {
                            if (showLog()) context.WriteLog("元素没有消失，继续等待");
                        }
                    }

                    if (ok) break;
                    if (stopwatch.ElapsedMilliseconds > activity.TimeOutMillisecond)
                    {
                        throw new Exception("元素等待超时跳出");
                    }
                    else
                    {
                        System.Threading.Thread.Sleep(100);
                        System.Windows.Forms.Application.DoEvents();
                        context.ThrowIfCancellationRequested();
                    }
                }
            }
            catch (Exception ex)
            {
                context.WriteLog("发生错误:" + ex.Message);
                throw ex;
            }
            finally
            {
                stopwatch.Stop();
            }
            System.Windows.Forms.Application.DoEvents();
        }

        private static bool showLog()
        {
            if (lastLogTime.AddSeconds(5) > DateTime.Now)
            {
                return false;
            }
            else
            {
                lastLogTime = DateTime.Now;
                return true;
            }
        }
    }
}