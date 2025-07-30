using litsdk;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;
using System.Drawing.Imaging;
using SHDocVw;
using mshtml;
using System.Net;
using System.Runtime.CompilerServices;

namespace litie
{
    /// <summary>
    /// ie截图
    /// </summary>
    internal class IEScreenShot
    {
        public static void Execute(litcore.browser.ScreenShot activity, ActivityContext context)
        {
            System.Windows.Forms.WebBrowser Browser_Select = IELoad.Browser_Select.IEBrowser;

            Form form = IELoad.Browser_Select.tabPage.FindForm();
            bool oldVisiable = form.Visible;
            if (IELoad.Browser_Select.tabPage != null)//需要置顶，不然就黑屏的
            {
                TabControl tc = IELoad.Browser_Select.tabPage.Parent as TabControl;
                if (tc != null)
                {
                    tc.SelectedTab = IELoad.Browser_Select.tabPage;
                }
                if (!oldVisiable)
                {
                    form.Show();
                }
            }

            Bitmap bitmap = null;

            if (activity.ScreenShotType == litcore.ictype.ScreenShotType.FullSCreen)
            {
                System.Drawing.Imaging.PixelFormat pixelFormat = activity.ImageFormat == "bmp" ? System.Drawing.Imaging.PixelFormat.Format24bppRgb : System.Drawing.Imaging.PixelFormat.Format32bppPArgb;
                bitmap = GetPageImage(Browser_Select, pixelFormat);
            }
            else
            {
                SHDocVw.InternetExplorer browser = (SHDocVw.InternetExplorer)Browser_Select.ActiveXInstance;
                mshtml.IHTMLDocument2 htmlDoc = null;

                if (!string.IsNullOrEmpty(activity.FrameName))
                {
                    htmlDoc = IEXPath.FindFrame(activity.FrameName, Browser_Select.Document.DomDocument as mshtml.IHTMLDocument2);
                }
                else
                {
                    htmlDoc = browser.Document as mshtml.IHTMLDocument2;
                }
                if (htmlDoc == null) throw new Exception("不存在框架:" + activity.FrameName);

                string xpathstr = context.ReplaceVar(activity.XPathStr);
                if (xpathstr == "") throw new Exception("XPath不能为空");
                List<string> xps = xpathstr.Replace("\r", "").Split('\n').ToList();

                List<IHTMLElement> elements = IEXPath.GetHTMLElementsByXPath(htmlDoc, xps);
                if (elements.Count > 0)//https://www.iteye.com/blog/roy9494-1379165
                {
                    //获取网页所有内容
                    HTMLDocument hdoc = (HTMLDocument)Browser_Select.Document.DomDocument;
                    //获取网页body标签中的内容
                    HTMLBody hbody = (HTMLBody)hdoc.body;
                    //创建一个接口
                    IHTMLControlRange hcr = (IHTMLControlRange)hbody.createControlRange();

                    if (activity.ScreenShotType == litcore.ictype.ScreenShotType.ImgElement)
                    {
                        //获取图片地址
                        IHTMLControlElement hImg = (IHTMLControlElement)elements[0];

                        if (hImg == null || hImg.GetType() != typeof(mshtml.HTMLImgClass)) throw new Exception("当前元素非图片");

                        IHTMLTxtRange txtRange = hbody.createTextRange();
                        txtRange.execCommand("Unselect");

                        //将图片添加到接口中
                        hcr.add(hImg);
                        //将图片复制到内存
                        hcr.execCommand("Copy", false, null);
                        //从粘贴板得到图片
                        Image CodeImage = Clipboard.GetImage();
                        //返回得到的验证码
                        bitmap = new Bitmap(CodeImage);
                    }
                    else
                    {
                        var element = elements[0];
                        bitmap = new Bitmap(element.offsetWidth, element.offsetHeight);

                        // 创建一个矩形，指定要截图的元素在WebBrowser控件中的位置和大小
                        Rectangle rect = new Rectangle(element.offsetLeft, element.offsetTop, element.offsetWidth, element.offsetHeight);

                        // 将WebBrowser控件绘制到Bitmap对象中
                        Browser_Select.DrawToBitmap(bitmap, rect);
                    }
                }
                else
                {
                    throw new Exception("没有找到网页元素");
                }
            }

            string savepath = "";
            if (activity.UseTempPath)
            {
                savepath = System.IO.Path.GetTempFileName();
            }
            else
            {
                savepath = context.ReplaceVar(activity.SaveFilePath);
                string dir = System.IO.Path.GetDirectoryName(savepath);
                if (!System.IO.Directory.Exists(dir)) System.IO.Directory.CreateDirectory(dir);
            }

            System.Drawing.Imaging.ImageFormat imageFormat = System.Drawing.Imaging.ImageFormat.Jpeg;
            switch (activity.ImageFormat)
            {
                case "jpg":
                    imageFormat = System.Drawing.Imaging.ImageFormat.Jpeg;
                    break;
                case "bmp":
                    imageFormat = System.Drawing.Imaging.ImageFormat.Bmp;
                    break;
                case "png":
                    imageFormat = System.Drawing.Imaging.ImageFormat.Png;
                    break;
            }
            bitmap.Save(savepath, imageFormat);
            bitmap.Dispose();

            if (!oldVisiable) form.Hide();

            if (activity.SavePathToStrVar)
            {
                context.SetVarStr(activity.SaveVarName, savepath);
            }

            context.WriteLog("成功生成截图:" + savepath);
            System.Windows.Forms.Application.DoEvents();
        }
        
        /// <summary>
        /// https://www.cnblogs.com/freezesoul/archive/2009/08/21/1551736.html
        /// </summary>
        /// <param name="m_browser"></param>
        /// <returns></returns>
        public static Bitmap GetPageImage(System.Windows.Forms.WebBrowser m_browser, PixelFormat pixelFormat)
        {
            mshtml.IHTMLDocument2 myDoc = (mshtml.IHTMLDocument2)m_browser.Document.DomDocument;

            //处理由于webbrowser上、左边框阴影带来的拼接bug
            int URLExtraHeight = 3;
            int URLExtraLeft = 3;

            (myDoc as HTMLDocumentClass).documentElement.setAttribute("scroll", "yes", 0);

            //document完整高度
            int heightsize = (int)(myDoc as HTMLDocumentClass).documentElement.getAttribute("scrollHeight", 0);
            int widthsize = (int)(myDoc as HTMLDocumentClass).documentElement.getAttribute("scrollWidth", 0);

            ////Get Screen Height
            int screenHeight = (int)(myDoc as HTMLDocumentClass).documentElement.getAttribute("clientHeight", 0);
            int screenWidth = (int)(myDoc as HTMLDocumentClass).documentElement.getAttribute("clientWidth", 0);

            IntPtr myIntptr = (IntPtr)m_browser.Handle;

            //寻找IE对象句柄
            IntPtr wbHandle = FindWindowEx(myIntptr, IntPtr.Zero, "Shell Embedding", null);
            wbHandle = FindWindowEx(wbHandle, IntPtr.Zero, "Shell DocObject View", null);
            wbHandle = FindWindowEx(wbHandle, IntPtr.Zero, "Internet Explorer_Server", null);
            IntPtr hwnd = wbHandle;

            Bitmap bm = new Bitmap(screenWidth, screenHeight, pixelFormat);
            Bitmap bm2 = new Bitmap(widthsize, heightsize, pixelFormat);
            Graphics g = null;
            Graphics g2 = Graphics.FromImage(bm2);
            //切割用的临时对象
            Bitmap tempbm = null;
            Graphics tempg = null;

            IntPtr hdc;
            Image screenfrag = null;

            #region 拼接
            int brwTop = 0;
            int brwLeft = 0;
            int myPage = 0;

            //Get Screen Height (for bottom up screen drawing)
            while ((myPage * screenHeight) < heightsize)
            {
                (myDoc as HTMLDocumentClass).documentElement.setAttribute("scrollTop", (screenHeight - 5) * myPage, 0);
                ++myPage;
            }
            //Rollback the page count by one
            --myPage;

            int myPageWidth = 0;
            //screenWidth+ URLExtraLeft
            while ((myPageWidth * screenWidth) < widthsize)
            {
                (myDoc as HTMLDocumentClass).documentElement.setAttribute("scrollLeft", (screenWidth - 5) * myPageWidth, 0);
                brwLeft = (int)(myDoc as HTMLDocumentClass).documentElement.getAttribute("scrollLeft", 0);
                for (int i = myPage; i >= 0; --i)
                {
                    //Shoot visible window
                    g = Graphics.FromImage(bm);
                    hdc = g.GetHdc();
                    (myDoc as HTMLDocumentClass).documentElement.setAttribute("scrollTop", (screenHeight - 5) * i, 0);
                    brwTop = (int)(myDoc as HTMLDocumentClass).documentElement.getAttribute("scrollTop", 0);
                    PrintWindow(hwnd, hdc, 0);
                    g.ReleaseHdc(hdc);
                    g.Flush();
                    screenfrag = Image.FromHbitmap(bm.GetHbitmap());

                    //切除URLExtraLeft、URLExtraHeight长度的webbrowser带来的bug
                    tempbm = new Bitmap(screenWidth - URLExtraLeft, screenHeight - URLExtraHeight, pixelFormat);
                    tempg = Graphics.FromImage(tempbm);
                    tempg.DrawImage(screenfrag, -URLExtraLeft, -URLExtraHeight);

                    //拼接到g2
                    g2.DrawImage(tempbm, brwLeft + URLExtraLeft, brwTop + URLExtraLeft);
                }

                ++myPageWidth;
            }

            int finalWidth = (int)widthsize;
            int finalHeight = (int)heightsize;
            Bitmap finalImage = new Bitmap(finalWidth, finalHeight, pixelFormat);
            Graphics gFinal = Graphics.FromImage((Image)finalImage);
            gFinal.DrawImage(bm2, 0, 0, finalWidth, finalHeight);

            #endregion


            #region Clean
            //Clean Up.
            myDoc.close();
            myDoc = null;
            g.Dispose();
            g2.Dispose();
            gFinal.Dispose();
            bm.Dispose();
            bm2.Dispose();
            screenfrag.Dispose();
            //finalImage.Dispose();
            tempbm.Dispose();
            tempg.Dispose();
            #endregion
            return finalImage;
        }

        #region DLLImport

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr FindWindowEx(IntPtr parent /*HWND*/, IntPtr next /*HWND*/, string sClassName, IntPtr sWindowTitle);

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        private static extern IntPtr GetWindow(IntPtr hWnd, int uCmd);

        [DllImport("user32.dll")]
        private static extern void GetClassName(int h, StringBuilder s, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern bool PrintWindow(IntPtr hwnd, IntPtr hdcBlt, uint nFlags);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        public const int GW_CHILD = 5;
        public const int GW_HWNDNEXT = 2;

        #endregion
    }
}