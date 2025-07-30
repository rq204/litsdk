using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using litsdk;

namespace litapps
{
    [Serializable]
    [litsdk.Action(Name = "剪贴板操作", Category = "系统", Description = "剪贴板内容的设置，获取，清空等操作", Order = 20)]
    public class ClipboardActivity : litsdk.ProcessActivity
    {
        public override string Name { get; set; } = "剪贴板操作";

        [Argument(Name = "操作类型", ControlType = ControlType.ComboBox, Order = 1, Description = "剪贴板操作方式")]
        /// <summary>
        /// 设置剪贴板
        /// </summary>
        public ClipboardType ClipboardType { get; set; } = ClipboardType.SetStrToClipboard;

        [Argument(Name = "设置文本", ControlType = ControlType.TextArea, Order = 2, Description = "将剪贴板内容设置为该字符")]
        public string SetStr { get; set; }

        [Argument(Name = "设置文件", ControlType = ControlType.File, Order = 3, Description = "将剪贴板内容设置为该文件")]
        public string SetFile { get; set; }

        [Argument(Name = "设置图片", ControlType = ControlType.File, Order = 4, Description = "将剪贴板内容设置为该图片文件")]
        public string SetImage { get; set; }

        [Argument(Name = "存入变量", ControlType = ControlType.Variable, Order = 5, Description = "将剪贴板文本内容存入指定变量")]
        public string GetStrVarName { get; set; }

        [Argument(Name = "保存地址", ControlType = ControlType.File, Order = 6, Description = "将剪贴板图片内容存入指定地址")]
        public string SaveImagePath { get; set; }

        [Argument(Name = "将文件路径存入指定变量", ControlType = ControlType.CheckBox, Order = 7, Description = "将文件路径地址存入指定文本变量当中")]
        public bool SavePath2Var { get; set; }

        [Argument(Name = "变量名称", ControlType = ControlType.Variable, Order = 8, Description = "将文件路径地址存入指定文本变量当中")]
        public string PathVarName { get; set; }

        public override void Execute(ActivityContext context)
        {
            string debug = "";
            litsdk.API.GetMainForm().Invoke((EventHandler)delegate
            {
                switch (this.ClipboardType)
                {
                    case ClipboardType.SetStrToClipboard:
                        string txt = context.ReplaceVar(this.SetStr);
                        try
                        {
                            Clipboard.SetText(txt);
                        }
                        catch
                        {
                            System.Threading.Thread.Sleep(1000);
                            try
                            {
                                Clipboard.SetText(txt);
                            }
                            catch
                            {
                                System.Threading.Thread.Sleep(1000);
                                ClipboardRetry.SetText(txt);
                            }
                        }
                        debug = $"设置剪切板成功，字符长度为{txt.Length}";
                        break;
                    case ClipboardType.SetFileToClipboard:
                        string filepath = context.ReplaceVar(this.SetFile);
                        StringCollection paths = new StringCollection();
                        paths.Add(filepath);
                        try
                        {
                            Clipboard.SetFileDropList(paths);
                        }
                        catch
                        {
                            System.Threading.Thread.Sleep(1000);
                            Clipboard.SetFileDropList(paths);
                        }
                        debug = "设置文件路径至剪贴板：" + filepath;
                        break;
                    case ClipboardType.SetImageToClipboard:
                        string imgpath = context.ReplaceVar(this.SetImage);
                        System.Drawing.Image img = System.Drawing.Image.FromFile(imgpath);
                        try
                        {
                            Clipboard.SetImage(img);
                        }
                        catch
                        {
                            System.Threading.Thread.Sleep(1000);
                            try
                            {
                                Clipboard.SetImage(img);
                            }
                            catch
                            {
                                System.Threading.Thread.Sleep(1000);
                                ClipboardRetry.SetImage(img);
                            }
             
                        }
                        debug = "将图片设置到剪贴板：" + imgpath;
                        break;
                    case ClipboardType.GetStrFromClipboard:
                        string txt2 = "";
                        try
                        {
                            txt2 = Clipboard.GetText();
                        }
                        catch
                        {
                            System.Threading.Thread.Sleep(1000);
                            txt2 = Clipboard.GetText();
                        }
                        context.SetVarStr(this.GetStrVarName, txt2);
                        debug = $"获取剪切板内容成功，字符长度为{txt2.Length}";
                        break;
                    case ClipboardType.SaveImageFromClipboard:
                        string saveimgpath = context.ReplaceVar(this.SaveImagePath);
                        System.Drawing.Image image = null;
                        try
                        {
                            image = Clipboard.GetImage();
                        }
                        catch
                        {
                            System.Threading.Thread.Sleep(1000);
                            image = Clipboard.GetImage();
                        }
                        image.Save(saveimgpath, System.Drawing.Imaging.ImageFormat.Png);
                        debug = "从剪贴板保存图片成功：" + saveimgpath;
                        if (this.SavePath2Var) context.SetVarStr(this.PathVarName, saveimgpath);
                        break;
                    case ClipboardType.ClearClipboard:
                        Clipboard.Clear();
                        debug = "清空剪贴板成功";
                        break;
                }
            });
            context.WriteLog(debug);
        }

        public override void Validate(ActivityContext context)
        {
            switch (this.ClipboardType)
            {
                case ClipboardType.SetStrToClipboard:
                    if (string.IsNullOrEmpty(this.SetStr)) throw new Exception("设置文本不能为空");
                    break;
                case ClipboardType.SetFileToClipboard:
                    if (string.IsNullOrEmpty(this.SetFile)) throw new Exception("设置文件不能为空");
                    break;
                case ClipboardType.SetImageToClipboard:
                    if (string.IsNullOrEmpty(this.SetImage)) throw new Exception("设置图片不能为空");
                    break;
                case ClipboardType.GetStrFromClipboard:
                    if (string.IsNullOrEmpty(this.GetStrVarName)) throw new Exception("保存文本变量不能为空");
                    break;
                case ClipboardType.SaveImageFromClipboard:
                    if (string.IsNullOrEmpty(this.SaveImagePath)) throw new Exception("保存图片路径不能为空");
                    break;
                case ClipboardType.ClearClipboard:
                    break;
            }
        }

        public override ControlStyle GetControlStyle(string field)
        {
            ControlStyle style = new ControlStyle();
            switch (field)
            {
                case "SetStr":
                    style.Visible = this.ClipboardType == ClipboardType.SetStrToClipboard;
                    style.Variables = ControlStyle.GetVariables(true, false, true);
                    break;
                case "SetFile":
                    style.Visible = this.ClipboardType == ClipboardType.SetFileToClipboard;
                    style.Variables = ControlStyle.GetVariables(true, false, true);
                    break;
                case "SetImage":
                    style.Visible = this.ClipboardType == ClipboardType.SetImageToClipboard;
                    style.Variables = ControlStyle.GetVariables(true, false, true);
                    style.Filter = "图片文件|*.jpg;*.png;*.jpeg;*.bmp;*.gif";
                    break;
                case "GetStrVarName":
                    style.Visible = this.ClipboardType == ClipboardType.GetStrFromClipboard;
                    style.Variables = ControlStyle.GetVariables(true);
                    break;
                case "SaveImagePath":
                    style.Visible = this.ClipboardType == ClipboardType.SaveImageFromClipboard;
                    style.Variables = ControlStyle.GetVariables(true, false, true);
                    style.Filter = "图片文件|*.jpg;*.png;*.jpeg;*.bmp;*.gif";
                    break;
                case "SavePath2Var":
                    style.Visible = this.ClipboardType == ClipboardType.SaveImageFromClipboard;
                    break;
                case "PathVarName":
                    style.Visible = this.SavePath2Var;
                    style.Variables = ControlStyle.GetVariables(true);
                    break;
            }
            return style;
        }


        class ClipboardRetry
        {
            // Windows API 函数声明
            [DllImport("user32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool OpenClipboard(IntPtr hWndNewOwner);

            [DllImport("user32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool CloseClipboard();

            [DllImport("user32.dll", SetLastError = true)]
            private static extern IntPtr SetClipboardData(uint uFormat, IntPtr hMem);

            [DllImport("user32.dll", SetLastError = true)]
            private static extern IntPtr GetClipboardData(uint uFormat);

            [DllImport("user32.dll", SetLastError = true)]
            private static extern bool EmptyClipboard();

            // 剪贴板数据格式
            private const uint CF_TEXT = 1; // 文本格式
            private const uint CF_BITMAP = 2; // 位图格式
            private const uint CF_DIB = 8; // 设备无关位图格式
            private const uint CF_UNICODETEXT = 13; // Unicode 文本格式

            public static bool SetText(string txt)
            {
                if (OpenClipboard(IntPtr.Zero))
                {
                    IntPtr hGlobal = Marshal.StringToHGlobalUni(txt);
                    SetClipboardData(CF_UNICODETEXT, hGlobal);
                    if (CloseClipboard())
                    {
                        return true;
                    }
                }
                return false;
            }

            public static bool SetImage(System.Drawing.Image img)
            {
                if (OpenClipboard(IntPtr.Zero))
                {
                    // 写入位图到剪贴板
                    Bitmap bitmap = img as Bitmap;
                    Graphics graphics = Graphics.FromImage(bitmap);
                    graphics.Clear(Color.Red);
                    SetClipboardData(CF_BITMAP, bitmap.GetHbitmap());
                    if (CloseClipboard())
                    {
                        return true;
                    }
                }
                return false;
            }

            //// 尝试剪贴板操作的方法
            //private static bool TryClipboardOperation()
            //{
            //    try
            //    {
            //        // 尝试打开剪贴板
            //        if (!OpenClipboard(IntPtr.Zero))
            //        {
            //            throw new Exception("Unable to open clipboard.");
            //        }

            //        // 清空剪贴板
            //        if (!EmptyClipboard())
            //        {
            //            throw new Exception("Unable to empty clipboard.");
            //        }

            //        // 在这里执行你的剪贴板操作，例如写入文本、图片或文件

            //        // 写入文本到剪贴板
            //        string textData = "Sample text";
            //        IntPtr hGlobal = Marshal.StringToHGlobalUni(textData);
            //        SetClipboardData(CF_UNICODETEXT, hGlobal);

            //       // 写入位图到剪贴板
            //        Bitmap bitmap = new Bitmap(100, 100);
            //        Graphics graphics = Graphics.FromImage(bitmap);
            //        graphics.Clear(Color.Red);
            //        SetClipboardData(CF_BITMAP, bitmap.GetHbitmap()) ;

            //        // 写入文件路径到剪贴板
            //        string filePath = @"C:\example.txt";
            //        hGlobal = Marshal.StringToHGlobalUni(filePath);
            //        SetClipboardData(CF_UNICODETEXT, hGlobal);

            //        // 关闭剪贴板
            //        if (!CloseClipboard())
            //        {
            //            throw new Exception("Unable to close clipboard.");
            //        }

            //        // 如果一切顺利，返回操作成功
            //        return true;
            //    }
            //    catch (Exception ex)
            //    {
            //        Console.WriteLine($"Clipboard operation failed: {ex.Message}");
            //        return false;
            //    }
            //}
        }

    }
}