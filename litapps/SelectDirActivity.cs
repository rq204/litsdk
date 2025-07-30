using litsdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace litapps
{
    [litsdk.Action(Name = "选择文件夹", Category = "对话框", IsFront = true, Order = 3, Description = "该指令是弹出一个选择文件夹对话框")]
    public class SelectDirActivity : litsdk.ProcessActivity
    {
        public override string Name { get; set; } = "选择文件夹";


        [Argument(Name = "弹窗标题", ControlType = ControlType.TextBox, Order = 2, Description = "对话框标题")]
        public string Title { get; set; }

        //[Argument(Name = "文件筛选", ControlType = ControlType.TextBox, Order = 3, Description = "选择哪些文件")]
        //public string Filter { get; set; } = "*.*";

        [Argument(Name = "必须选择", ControlType = ControlType.CheckBox, Order = 4, Description = "必须选择后用户不能关闭或取消")]
        /// <summary>
        /// 必须选择
        /// </summary>
        public bool MustSelect { get; set; }

        [Argument(Name = "结果存入", ControlType = ControlType.Variable, Order = 7, Description = "将选择的文件列表或是文件夹路径存入")]
        public string SaveVarName { get; set; }

        public override void Execute(ActivityContext context)
        {
            string title = context.ReplaceVar(this.Title);

            litsdk.API.GetMainForm().Invoke((EventHandler)delegate
            {
                while (true)
                {
                    FolderBrowserDialog folder = new FolderBrowserDialog();
                    folder.Description = title;
                    //folder.ShowNewFolderButton = true;
                    if (folder.ShowDialog() == DialogResult.OK)
                    {
                        context.SetVarStr(this.SaveVarName, folder.SelectedPath);
                        context.WriteLog("成功选择文件夹：" + folder.SelectedPath);
                        break;
                    }
                    else
                    {
                        if (this.MustSelect)
                        {
                            context.WriteLog("必须选择一个文件夹，请重新选择");
                        }
                        else
                        {
                            context.WriteLog("没有选择任何文件夹，继续下一步");
                            break;
                        }
                    }
                }

            });
        }

        public override void Validate(ActivityContext context)
        {
            if (string.IsNullOrEmpty(this.Title))
            {
                throw new Exception("提示信息不能为空");
            }

            if (string.IsNullOrEmpty(this.SaveVarName)) throw new Exception("保存变量名不能为空");

            if (!context.ContainsStr(this.SaveVarName)) throw new Exception($"不存在字符变量{this.SaveVarName}");
        }

        public override ControlStyle GetControlStyle(string field)
        {
            litsdk.ControlStyle style = new ControlStyle();

            switch (field)
            {
                case "Title":
                    style.Variables = ControlStyle.GetVariables(true, false, true);
                    break;
                case "SaveVarName":
                    style.Variables = ControlStyle.GetVariables(true);
                    break;
            }

            return style;
        }
    }
}