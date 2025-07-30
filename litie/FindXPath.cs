using mshtml;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace litie
{
    internal class FindXPath
    {
        public void captureIE(Action<string, List<string>> action)
        {
            IntPtr hWnd = Win32API.WindowFromPoint(System.Windows.Forms.Control.MousePosition);

            mshtml.IHTMLDocument2 doc2 = (mshtml.IHTMLDocument2)GetHtmlDocumentByHandle(hWnd);

            if (doc2 != null)
            {
                Point point = System.Windows.Forms.Control.MousePosition;

                Win32API.ScreenToClient(hWnd, ref point);

                IHTMLElement element = doc2.elementFromPoint(point.X, point.Y);

                string frameName = "";
                while (true)
                {
                    if (element is HTMLFrameElementClass frame)
                    {
                        frameName = frame.name;
                        if (string.IsNullOrEmpty(frameName)) frameName = frame.src;
                        if (string.IsNullOrEmpty(frameName)) frameName = frame.id;
                        doc2 = CrossFrameIE.GetDocumentFromWindow( frame.contentWindow);
                        element = doc2.elementFromPoint(point.X - element.offsetLeft, point.Y - element.offsetTop);
                    }
                    else if (element is HTMLIFrameClass fclass)//这个是iframe的，还需要研究 todo
                    {
                        doc2 = CrossFrameIE.GetDocumentFromWindow(fclass.contentWindow);
                        frameName = fclass.name;
                        if (string.IsNullOrEmpty(frameName)) frameName = fclass.src;
                        if (string.IsNullOrEmpty(frameName)) frameName = fclass.id;
                        IHTMLRect rect = fclass.getBoundingClientRect();
                        element = doc2.elementFromPoint(point.X - rect.left, point.Y - rect.top);
                    }
                    else
                    {
                        break;
                    }
                }
                readIEElement(element);
                if (action != null) action(frameName, this.Xpaths);
            }
        }

        private IHTMLElement lastEle = null;

        /**
         * 读取元素基本信息和提取XPATH
         */
        private void readIEElement(IHTMLElement e)
        {
            if (lastEle == e)
            {
                return;
            }

            clearBorderHint();

            this.lastEle = e;

            if (e == null)
            {
                return;
            }

            //画红框
            e.style.setAttribute("outline", "1px solid red");

            //// 基本信息
            //this.txtID.Text = e.id;
            //this.txtName.Text = getElementAttribute(e, "NAME");
            //this.txtTag.Text = e.tagName;
            //this.txtText.Text = getElementAttribute(e, "VALUE");
            //this.txtClass.Text = e.className;
            //this.txtHTML.Text = e.innerHTML;
            //this.txtOuterHtml.Text = e.outerHTML;

            // xpath
            extractXPath(e);

        }

        private char SPLIT = '"';
        private List<string> Xpaths = new List<string>();

        private void extractXPath(IHTMLElement e)
        {
            Xpaths.Clear();

            // id 
            if (e.id != null)
            {
                addXPath("//*[@id=" + SPLIT + e.id + SPLIT + "]");
                return;
            }

            //往上找
            addXPath(getXPathEx(e));

            // name
            string name = getElementAttribute(e, "NAME");

            if (name != "")
            {
                addXPath("//" + e.tagName + "[@name=" + SPLIT + name + SPLIT + "]");
            }

            //class
            if (e.className != null)
            {
                /*
                String[] classnames = e.className.Split(' ');

                foreach (string cls in classnames)
                {
                    addXPath("//" + e.tagName + "[@class=" + SPLIT + cls + SPLIT + "]");
                }
                 */

                addXPath("//" + e.tagName + "[@class=" + SPLIT + e.className + SPLIT + "]");
            }
        }

        /**
         * 一直往上找，找到有id的父元素为止，如果没有，就到html为止。
         * 
         * 返回格式如：
         * //*[@id="formConfig"]/INPUT[1]
         * /HTML/BODY/DIV[1]/DIV/DIV[1]/A[1]/SPAN
         */
        private string getXPathEx(IHTMLElement e)
        {
            IHTMLElement current = e;
            
            string xpath = "";

            while (current != null)
            {
                // 如果有id，结束
                if (current.id != null)
                {
                    xpath = "//*[@id=" + SPLIT + current.id + SPLIT + "]" + xpath;
                    break;
                }
                else
                {
                    string currentXpath = extractCurrentXpath(current);
                    xpath = currentXpath + xpath;
                }

                current = current.parentElement;
            }

            return xpath;
        }

        /**
         * 当前节点的xpath
         * 返回结果如: /INPUT[2]
         */
        private string extractCurrentXpath(IHTMLElement current)
        {
            string currentXpath = "/" + current.tagName;

            // 计算index
            int index = calculate(current);

            if (index >= 1)
            {
                currentXpath += "[" + index + "]";
            }

            return currentXpath;
        }

        /**
         * 计算当前元素在父元素中相同的tag中的index
         * xpath的index是从1开始的
         */
        private int calculate(IHTMLElement current)
        {
            if (current.parentElement == null)
            {
                return 0;
            }

            IHTMLElementCollection collection = (IHTMLElementCollection)current.parentElement.children;

            int length = collection.length;

            int index = 0, all = 0;

            for (var i = 0; i < length; i++)
            {
                IHTMLElement item = collection.item(i) as IHTMLElement;

                // 实际测试中发生过
                if (item == null)
                {
                    break;
                }

                if (item.tagName == current.tagName)
                {
                    all++;

                    if (item == current)
                    {
                        index = all;
                    }
                }
            }

            // 只有一个元素，就不需要[1]
            if (all == 1)
            {
                return 0;
            }

            // xpath不是从0开始
            return index;
        }

        private void addXPath(string str)
        {
            Xpaths.Add(str);
        }

        private string getElementAttribute(IHTMLElement e, string name)
        {
            dynamic value = e.getAttribute(name);
            return value is System.DBNull ? "" : value + "";
        }

        /**
         * 去掉最后一个元素的红框 
         **/
        public void clearBorderHint()
        {
            // 
            if (this.lastEle != null)
            {
                try
                {
                    this.lastEle.style.setAttribute("outline", "");
                }
                catch
                {
                    //上一个元素可能不存在了
                }
            }
        }

        public static object GetComObjectByHandle(int Msg, Guid riid, IntPtr hWnd)
        {
            object _ComObject;
            int lpdwResult = 0;
            if (!Win32API.SendMessageTimeout(hWnd, Msg, 0, 0, Win32API.SMTO_ABORTIFHUNG, 1000, ref lpdwResult))
                return null;
            if (Win32API.ObjectFromLresult(lpdwResult, ref riid, 0, out _ComObject))
                return null;
            return _ComObject;
        }

        public object GetHtmlDocumentByHandle(IntPtr hWnd)
        {
            string buffer = new string('\0', 24);
            Win32API.GetClassName(hWnd, ref buffer, 25);

            if (buffer != "Internet Explorer_Server")
                return null;

            return GetComObjectByHandle(Win32API.WM_HTML_GETOBJECT, Win32API.IID_IHTMLDocument, hWnd);
        }
    }
}