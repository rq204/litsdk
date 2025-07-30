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
    public class IEXPath
    {
        /// <summary>
        /// 按名称查找
        /// </summary>
        /// <param name="frameName"></param>
        /// <param name="htmlDocument"></param>
        /// <returns></returns>
        public static mshtml.IHTMLDocument2 FindFrame(string frameName, mshtml.IHTMLDocument2 doc2)
        {
            for (int i = 0; i < doc2.frames.length; i++)
            {

                mshtml.HTMLWindow2Class win2 = doc2.frames.item(i) as mshtml.HTMLWindow2Class;
                string domain = win2.document.url;// doc2.url;
                if (domain.IndexOf('/', 10) > -1) domain = domain.Substring(0, domain.IndexOf('/', 10));

                if (win2.frameElement.name == frameName || win2.frameElement.src == frameName || domain + win2.frameElement.src == frameName)
                {
                    return CrossFrameIE.GetDocumentFromWindow(win2);
                }
                mshtml.IHTMLDocument2 find = FindFrame(frameName, win2.document);
                if (find != null) return find;
            }
            return null;
        }


        public static mshtml.IHTMLDocument2 FindIFrame(string frameName, mshtml.IHTMLDocument2 doc2)
        {
            for (int i = 0; i < doc2.all.length; i++)
            {
                if (doc2.all.item(i) is HTMLIFrameClass fclass)
                {
                    string domain = doc2.url;
                    if (domain.IndexOf('/', 10) > -1) domain = domain.Substring(0, domain.IndexOf('/', 10));

                    if (fclass.name == frameName || fclass.src == frameName || domain + fclass.src == frameName)
                    {
                        return CrossFrameIE.GetDocumentFromWindow(fclass.contentWindow);
                    }
                    mshtml.IHTMLDocument2 find = FindIFrame(frameName, fclass.contentWindow.document);
                    if (find != null) return find;
                }
            }
            for (int i = 0; i < doc2.frames.length; i++)
            {
                mshtml.HTMLWindow2Class win2 = doc2.frames.item(i) as mshtml.HTMLWindow2Class;
                mshtml.IHTMLDocument2 hTMLDocument2 = CrossFrameIE.GetDocumentFromWindow(win2);
                mshtml.IHTMLDocument2 find = FindIFrame(frameName, hTMLDocument2);
                if (find != null) return find;
            }

            return null;
        }


        public static string GetAttbute(IHTMLElement element, string attname)
        {
            string txt = "";
            switch (attname)
            {
                case "innerHTML":
                    txt = element.innerHTML;
                    break;
                case "outerHTML":
                    txt = element.outerHTML;
                    break;
                case "innerText":
                    txt = element.innerText;
                    break;
                case "outerText":
                    txt = element.outerText;
                    break;
                case "textContent":
                    txt = element.innerText;
                    break;
                default:
                    txt = element.getAttribute(attname) == null ? "" : element.getAttribute(attname).ToString();
                    break;
            }
            if (txt == null) txt = "";
            return txt;
        }

        /// <summary>
        /// </summary>
        /// <param name="Browser_Select"></param>
        /// <param name="xpaths"></param>
        /// <returns></returns>
        public static List<IHTMLElement> GetHTMLElementsByXPath(mshtml.IHTMLDocument2 htmlDoc, List<string> xpaths)
        {
            List<IHTMLElement> ls = new List<IHTMLElement>();

            foreach (string x in xpaths)
            {
                List<string> xArr = x.Split('/').ToList();
                xArr.RemoveAt(0);

                List<IHTMLElement> roots = new List<IHTMLElement>() { null };
                foreach (string cur in xArr)
                {
                    IEXPath ix = new IEXPath();
                    if (!cur.Contains("["))
                    {
                        ix.TagName = cur;
                    }
                    else// && !cur.Contains("@") && !cur.Contains("=") && !cur.Contains(">") && !cur.Contains("<")
                    {
                        ix.TagName = cur.Substring(0, cur.IndexOf("["));
                        string filter = cur.Substring(cur.IndexOf("["), cur.Length - cur.IndexOf("["));
                        //[a][b]
                        ix.Filter = filter.TrimStart('[').TrimEnd(']').Replace("[", "").Split(']').ToList();
                    }
                    roots = GetElements(roots, ix, htmlDoc);
                    if (roots.Count == 0) break;
                }
                ls = roots;
                if (roots.Count > 0) break;
            }

            return ls;
        }


        public string TagName = "";

        public List<string> Filter = new List<string>();

        /// <summary>
        /// 从每一个上级获取符合条件的元素
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="iEXPath"></param>
        /// <returns></returns>
        private static List<IHTMLElement> GetElements(List<IHTMLElement> parent, IEXPath iEXPath, mshtml.IHTMLDocument2 htmlDoc)
        {
            List<IHTMLElement> finds = new List<IHTMLElement>();
            if (iEXPath.TagName == "" && iEXPath.Filter.Count == 0)// //这种相对模式，循环获取所有可能的元素列表
            {
                if (parent.Count == 1 && parent[0] == null)
                {
                    foreach (IHTMLElement ie in htmlDoc.all)
                    {
                        finds.Add(ie);
                    }
                }
                else
                {
                    foreach (IHTMLElement p in parent)
                    {
                        foreach (IHTMLElement ie in htmlDoc.all)
                        {
                            IHTMLElement ie2 = ie;
                            while (ie2.parentElement != null)
                            {
                                if (ie2.parentElement == p)
                                {
                                    finds.Add(ie2);
                                    break;
                                }
                                ie2 = ie2.parentElement;
                            }
                        }
                    }
                }
            }
            else
            {
                foreach (IHTMLElement element in parent)
                {
                    List<IHTMLElement> collect = new List<IHTMLElement>();
                    if (element == null) collect.Add(htmlDoc.body.parentElement);
                    else
                    {
                        foreach (IHTMLElement obj in (element.children as IHTMLElementCollection))
                        {
                            if (obj != null) collect.Add(obj);
                        }
                    }

                    Dictionary<string, int> posdic = new Dictionary<string, int>();//xpath从1开始
                    foreach (object obj in collect)
                    {
                        IHTMLElement hele = obj as IHTMLElement;
                        if (posdic.ContainsKey(hele.tagName))
                        {
                            posdic[hele.tagName]++;
                        }
                        else
                        {
                            posdic.Add(hele.tagName, 1);
                        }
                        if (iEXPath.TagName == "*" || hele.tagName.Equals(iEXPath.TagName, StringComparison.OrdinalIgnoreCase))
                        {
                            bool ok = true;

                            //分析属性
                            ///   //*[@id="formConfig"]/INPUT[1]  //div[@class="aaaa"]/a
                            //    /HTML/BODY/DIV[1]/DIV/DIV[1]/A[1]/SPAN
                            foreach (string f in iEXPath.Filter)
                            {
                                if (f.Contains("="))
                                {
                                    string[] arr = f.Split('=');
                                    if (arr.Length == 1)//@href 包含
                                    {
                                        throw new Exception("XPath语法错误");
                                    }
                                    else if (arr.Length == 2)
                                    {
                                        if (arr[0].StartsWith("@"))
                                        {
                                            string name = arr[0].TrimStart('@');
                                            string value = arr[1].Trim('"').Trim('\'');
                                            if (name.Equals("id", StringComparison.OrdinalIgnoreCase))
                                            {
                                                if (hele.id != value)
                                                {
                                                    ok = false;
                                                    break;
                                                }
                                                else
                                                {
                                                    //Console.WriteLine("xxx");
                                                }
                                            }
                                            else
                                            {
                                                object attvalue = hele.getAttribute(name);
                                                if (attvalue == null || attvalue.ToString() != value)
                                                {
                                                    ok = false;
                                                    break;
                                                }
                                            }
                                        }
                                        else//  这种先不处理//div[contains(@class, 'demo') and contains(@class, 'other')]
                                        {
                                            throw new Exception("该语法暂时不支持");
                                        }
                                    }
                                }
                                else if (f.Contains("contains("))
                                {
                                    throw new Exception("该语法暂时不支持");
                                }
                                else if (f.Contains("<"))
                                {
                                    throw new Exception("该语法暂时不支持");
                                }
                                else if (f.Contains(">"))
                                {
                                    throw new Exception("该语法暂时不支持");
                                }
                                else if (f.Contains(" and "))
                                {
                                    throw new Exception("该语法暂时不支持");
                                }
                                else if (f.Contains(" or "))
                                {
                                    throw new Exception("该语法暂时不支持");
                                }
                                else
                                {
                                    int index = -1;
                                    if (f.StartsWith("@"))
                                    {
                                        object attvalue = hele.getAttribute(f.Substring(1, f.Length - 1));
                                        if (attvalue == null)
                                        {
                                            ok = false;
                                            break;
                                        }
                                    }
                                    else if (int.TryParse(f, out index))
                                    {
                                        if (posdic[hele.tagName] != index)
                                        {
                                            ok = false;
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        throw new Exception("该语法暂时不支持");
                                    }
                                }
                            }
                            if (ok) finds.Add(hele);
                        }

                    }
                }
            }
            return finds;
        }
    }
}