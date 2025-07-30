using litsdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace litie
{
    internal class IERunJs
    {
        public static void Execute(litcore.browser.RunJs activity, ActivityContext context)
        {
            WebBrowser Browser_Select = IELoad.Browser_Select.IEBrowser;

            string code = context.ReplaceVar(activity.JsCode);

            SHDocVw.InternetExplorer browser = (SHDocVw.InternetExplorer)Browser_Select.ActiveXInstance;
            mshtml.IHTMLDocument2 htmlDoc = null;

            htmlDoc = browser.Document as mshtml.IHTMLDocument2;

            mshtml.IHTMLWindow2 win = (mshtml.IHTMLWindow2)htmlDoc.parentWindow;
            var result = htmlDoc.parentWindow.execScript(code,"JavaScript");
            if (string.IsNullOrEmpty(activity.SaveVarName))
            {
                context.WriteLog("成功执行JS代码");
            }
            else
            {
                string data = "";
                if (result != null) data = result.ToString();
                context.SetVarStr(activity.SaveVarName, data);
                context.WriteLog($"成功执行JS代码并保存结果至变量{activity.SaveVarName}，返回结果长度{data.Length}");
            }
            System.Threading.Thread.Sleep(200);
            System.Windows.Forms.Application.DoEvents();
        }
    }
}