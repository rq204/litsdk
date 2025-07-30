using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Net;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Windows.Forms;
using litsdk;
using System.Web;
using System.Runtime.CompilerServices;

namespace litie
{
    internal class IECookies
    {
        public static void Execute(litcore.browser.Cookies activity, ActivityContext context)
        {
            WebBrowser Browser_Select = IELoad.Browser_Select.IEBrowser;

            string cookiestr = "";
            string slog = "";

            switch (activity.CookieType)
            {
                case litcore.ictype.CookieType.ExportFile:
                case litcore.ictype.CookieType.ExportAll2Var:
                    cookiestr = FullWebBrowserCookie.GetCookieInternal(Browser_Select.Url, false);
                    if (activity.CookieType == litcore.ictype.CookieType.ExportFile)
                    {
                        string path = context.ReplaceVar(activity.ExportFilePath);

                        string dir = System.IO.Path.GetDirectoryName(path);
                        if (!System.IO.Directory.Exists(dir)) System.IO.Directory.CreateDirectory(dir);

                        System.IO.File.WriteAllText(path, cookiestr, System.Text.Encoding.UTF8);
                        slog = "Cookie写入文件成功：" + path;
                    }
                    else
                    {
                        context.SetVarStr(activity.ExportVarName, cookiestr);
                        slog = $"Cookie存入变量{activity.ExportVarName}成功";
                    }
                    break;
                case litcore.ictype.CookieType.ClearUrl:
                case litcore.ictype.CookieType.ClearAll:
                    Browser_Select.Document.ExecCommand("ClearAuthenticationCache", false, null);
                    slog = "Cookie清除成功";
                    break;
                case litcore.ictype.CookieType.ImportFile:
                case litcore.ictype.CookieType.ImportVar:
                    if (activity.CookieType == litcore.ictype.CookieType.ImportFile)
                    {
                        string path = context.ReplaceVar(activity.ImportFilePath);
                        cookiestr = System.IO.File.ReadAllText(path, System.Text.Encoding.UTF8);
                        slog = $"从文件导入Cookie成功：{path}";
                    }
                    else
                    {
                        cookiestr = context.GetStr(activity.ImportVarName);
                        slog = $"从变量{activity.ImportVarName}导入Cookie成功";
                    }
                    //https://blog.csdn.net/hangom/article/details/52619394
                    foreach (string c in cookiestr.Split(';'))
                    {
                        string[] item = c.Split('=');
                        if (item.Length == 2)
                        {
                            InternetSetCookie(Browser_Select.Url.AbsoluteUri, null, new Cookie(HttpUtility.UrlEncode(item[0]).Replace("+", ""), HttpUtility.UrlEncode(item[1]), "; expires = Session GMT", "/").ToString());
                        }
                    }
                    break;
                case litcore.ictype.CookieType.ExportAb2Var:
                case litcore.ictype.CookieType.ExporAbAllVar:
                    cookiestr = FullWebBrowserCookie.GetCookieInternal(Browser_Select.Url, false);
                    slog = $"导出AB类型Cookie至变量 {activity.ExportVarName}成功";
                    context.SetVarStr(activity.ExportVarName, cookiestr);
                    break;
            }
            context.WriteLog(slog);
        }

        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool InternetSetCookie(string lpszUrlName, string lbszCookieName, string lpszCookieData);

    }
}

/// <summary>
/// 以下两个类来自 https://www.w3dev.cn/article/20130401/csharp-webBrowser-get-HttpOnly-Cookie.aspx
/// </summary>

internal sealed class NativeMethods
{
    #region enums     

    public enum ErrorFlags
    {
        ERROR_INSUFFICIENT_BUFFER = 122,
        ERROR_INVALID_PARAMETER = 87,
        ERROR_NO_MORE_ITEMS = 259
    }

    public enum InternetFlags
    {
        INTERNET_COOKIE_HTTPONLY = 8192, //Requires IE 8 or higher     
        INTERNET_COOKIE_THIRD_PARTY = 131072,
        INTERNET_FLAG_RESTRICTED_ZONE = 16
    }

    #endregion

    #region DLL Imports     

    [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("wininet.dll", EntryPoint = "InternetGetCookieExW", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
    internal static extern bool InternetGetCookieEx([In] string Url, [In] string cookieName, [Out] StringBuilder cookieData, [In, Out] ref uint pchCookieData, uint flags, IntPtr reserved);

    #endregion
}


/// <SUMMARY></SUMMARY>     
/// 取得WebBrowser的完整Cookie。     
/// 因为默认的webBrowser1.Document.Cookie取不到HttpOnly的Cookie     
///      
public class FullWebBrowserCookie
{

    [SecurityCritical]
    public static string GetCookieInternal(Uri uri, bool throwIfNoCookie)
    {
        uint pchCookieData = 0;
        string url = UriToString(uri);
        uint flag = (uint)NativeMethods.InternetFlags.INTERNET_COOKIE_HTTPONLY;

        //Gets the size of the string builder     
        if (NativeMethods.InternetGetCookieEx(url, null, null, ref pchCookieData, flag, IntPtr.Zero))
        {
            pchCookieData++;
            StringBuilder cookieData = new StringBuilder((int)pchCookieData);

            //Read the cookie     
            if (NativeMethods.InternetGetCookieEx(url, null, cookieData, ref pchCookieData, flag, IntPtr.Zero))
            {
                DemandWebPermission(uri);
                return cookieData.ToString();
            }
        }

        int lastErrorCode = Marshal.GetLastWin32Error();

        if (throwIfNoCookie || (lastErrorCode != (int)NativeMethods.ErrorFlags.ERROR_NO_MORE_ITEMS))
        {
            throw new Win32Exception(lastErrorCode);
        }

        return null;
    }

    private static void DemandWebPermission(Uri uri)
    {
        string uriString = UriToString(uri);

        if (uri.IsFile)
        {
            string localPath = uri.LocalPath;
            new FileIOPermission(FileIOPermissionAccess.Read, localPath).Demand();
        }
        else
        {
            new WebPermission(NetworkAccess.Connect, uriString).Demand();
        }
    }

    private static string UriToString(Uri uri)
    {
        if (uri == null)
        {
            throw new ArgumentNullException("uri");
        }

        UriComponents components = (uri.IsAbsoluteUri ? UriComponents.AbsoluteUri : UriComponents.SerializationInfoString);
        return new StringBuilder(uri.GetComponents(components, UriFormat.SafeUnescaped), 2083).ToString();
    }
}