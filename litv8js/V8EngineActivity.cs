using litsdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace litv8js
{
    [litsdk.Action(Name = "执行JS代码", IsLinux = true, RefFile = "ClearScript.Core.dll,ClearScript.V8.dll,ClearScript.V8.ICUData.dll,ClearScript.Windows.Core.dll,ClearScript.Windows.dll,ClearScriptV8.win-x64.dll,ClearScriptV8.win-x86.dll,System.Net.Http.dll,System.Runtime.InteropServices.RuntimeInformation.dll,System.Security.Cryptography.Algorithms.dll,System.Security.Cryptography.Encoding.dll,System.Security.Cryptography.Primitives.dll,System.Security.Cryptography.X509Certificates.dll,System.ValueTuple.dll", Category = "代码", Order = 3, Description = "可以对变量进行读取和修改")]
    public class V8EngineActivity : litsdk.ProcessActivity
    {
        public override string Name { get; set; } = "执行JS代码";

        /// <summary>
        /// 要执行的js
        /// </summary>
        [litsdk.Argument(Name = "Js代码", ControlType = ControlType.CodeArea, Order = 1, Description = "使用v8引擎进行js计算，输出日志为 context.WriteLog(\"日志内容\")，\r\n获取字符变量为 context.GetStr(\"变量名\")，获取列表变量为 context.GetList(\"变量名\")，\r\n获取数字变量为 context.GetInt(\"变量名\")，设置字符变量为 context.SetVarStr(\"变量名\",\"变量值\") ，\r\n设置列表变量为 context.SetVarList(\"变量名\",列表值)，设置数字变量为 context.SetVarInt(\"变量名\",123) \r\n浏览器操作请使用浏览器分组下的执行Js代码")]
        public string JsCode { get; set; }

        public override void Execute(ActivityContext context)
        {
            string jsCode = context.ReplaceVar(this.JsCode);

            using (var engine = new Microsoft.ClearScript.V8.V8ScriptEngine())
            {
                try
                {
                    engine.AddHostObject("context", context);
                    engine.Execute(jsCode);
                    context.WriteLog("成功执行JS代码");
                }
                catch (Exception ex)
                {
                    throw new Exception($"执行js代码出错，错误信息：{ex.Message}，执行JS:{jsCode}");
                }
            }
        }

        public override ControlStyle GetControlStyle(string field)
        {
            litsdk.ControlStyle style = new litsdk.ControlStyle();
            style.Variables = litsdk.ControlStyle.GetVariables(true, false, true);
            style.PlaceholderText = "读写字符变量context.GetStr(\"name\")，context.SetVarStr(\"name\",\"value\") \r\n读写数字变量context.GetInt(\"name\")，context.SetVarInt(\"name\",123)\r\n读写列表变量context.GetList(\"name\")，context.SetVarList(\"name\", array)\r\n输出日志context.WriteLog(\"日志内容\")";
            return style;
        }

        public override void Validate(ActivityContext context)
        {
            if (string.IsNullOrEmpty(this.JsCode)) throw new Exception("执行js代码不能为空");
            if (this.JsCode.Contains("document.")) throw new Exception("Dom操作请在浏览器分组中执行js代码");
        }
    }
}