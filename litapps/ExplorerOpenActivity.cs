using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using litsdk;

namespace litapps
{
    [litsdk.Action(Name = "打开文件(夹)", Category = "系统", Order = 30, IsFront = false, Description = "直接打开文件夹或文件、也可直接定位指定文件")] 
    public class ExplorerOpenActivity : litsdk.ProcessActivity
    {
        public override string Name { get; set; } = "打开文件(夹)";

        [Argument(Name = "打开方式", ControlType = ControlType.ComboBox, Order = 1, Description = "打开文件还是打开文件夹")]
        /// <summary>
        /// 打开文件夹
        /// </summary>
        public ExplorerOpenType OpenType { get; set; }

        [Argument(Name = "文件路径", ControlType = ControlType.File, Order = 2, Description = "需要打开的文件路径")]
        /// <summary>
        /// 要打开的文件路径
        /// </summary>
        public string FilePath { get; set; }

        [Argument(Name = "文件夹路径", ControlType = ControlType.Directory, Order = 3, Description = "需要打开的文件夹路径")]
        public string DirPath { get; set; }

        [Argument(Name = "不打开文件，仅打开文件目录同时并选中文件", ControlType = ControlType.CheckBox, Order = 4, Description = "打开文件所有文件夹，并同时选中文件但并不打开")]
        /// <summary>
        /// 打开目录和选中文件
        /// </summary>
        public bool OpenDirSelectFile { get; set; }

        public override void Execute(ActivityContext context)
        {
            if (OpenType == ExplorerOpenType.OpenDir)
            {
                string dirpath = context.ReplaceVar(this.DirPath);
                System.Diagnostics.Process.Start(dirpath);
                context.WriteLog("成功打开文件夹：" + dirpath);
            }
            else
            {
                string filepath = context.ReplaceVar(this.FilePath);
                if (!System.IO.File.Exists(filepath)) throw new Exception("不存在文件:" + filepath);
                if (this.OpenDirSelectFile)
                {
                    ExplorerFile(filepath);
                }
                else
                {
                    System.Diagnostics.Process.Start(filepath);
                    context.WriteLog("成功打开文件：" + filepath);
                }
            }
        }

        public override void Validate(ActivityContext context)
        {
            if (this.OpenType == ExplorerOpenType.OpenDir)
            {
                if (string.IsNullOrEmpty(this.DirPath)) throw new Exception("文件夹路径不能为空");
            }
            else
            {
                if (string.IsNullOrEmpty(this.FilePath)) throw new Exception("文件路径不能为空");
            }
        }

        //https://www.cnblogs.com/crwy/p/SHOpenFolderAndSelectItems.html

        /// <summary>
        /// 打开路径并定位文件...对于@"h:\Bleacher Report - Hardaway with the safe call ??.mp4"这样的，explorer.exe /select,d:xxx不认，用API整它
        /// </summary>
        /// <param name="filePath">文件绝对路径</param>
        [DllImport("shell32.dll", ExactSpelling = true)]
        private static extern void ILFree(IntPtr pidlList);

        [DllImport("shell32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        private static extern IntPtr ILCreateFromPathW(string pszPath);

        [DllImport("shell32.dll", ExactSpelling = true)]
        private static extern int SHOpenFolderAndSelectItems(IntPtr pidlList, uint cild, IntPtr children, uint dwFlags);

        public static void ExplorerFile(string filePath)
        {
            if (!File.Exists(filePath) && !Directory.Exists(filePath))
                return;

            if (Directory.Exists(filePath))
                Process.Start(@"explorer.exe", "/select,\"" + filePath + "\"");
            else
            {
                IntPtr pidlList = ILCreateFromPathW(filePath);
                if (pidlList != IntPtr.Zero)
                {
                    try
                    {
                        Marshal.ThrowExceptionForHR(SHOpenFolderAndSelectItems(pidlList, 0, IntPtr.Zero, 0));
                    }
                    finally
                    {
                        ILFree(pidlList);
                    }
                }
            }
        }

        public override ControlStyle GetControlStyle(string field)
        {
            ControlStyle style = new ControlStyle();
            switch (field)
            {
                case "FilePath":
                    style.Visible = this.OpenType == ExplorerOpenType.OpenFile;
                    break;
                case "DirPath":
                    style.Visible = this.OpenType == ExplorerOpenType.OpenDir;
                    break;
                case "OpenDirSelectFile":
                    style.Visible = this.OpenType == ExplorerOpenType.OpenFile;
                    break;
            }
            style.Variables = ControlStyle.GetVariables(true, false, true);
            return style;
        }
    }
}