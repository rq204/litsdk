using litsdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace litapps
{
    [litsdk.Action(Name = "选择文件", Category = "对话框", IsFront = true, Order = 1, Description = "该指令是弹出一个选择文件或文件夹对话框")]
    public class SelectFileActivity : litsdk.ProcessActivity
    {
        public override string Name { get; set; } = "选择文件";

        [Argument(Name = "弹窗标题", ControlType = ControlType.TextBox, Order = 2, Description = "对话框标题")]
        public string Title { get; set; }

        [Argument(Name = "文件筛选", ControlType = ControlType.TextBox, Order = 3, Description = "选择哪些文件，多个之间用|号分割，如  *.txt|*.xlsx")]
        public string Filter { get; set; } = "*.*";

        [Argument(Name = "必须选择", ControlType = ControlType.CheckBox, Order = 4, Description = "必须选择后用户不能关闭或取消")]
        /// <summary>
        /// 必须选择
        /// </summary>
        public bool MustSelect { get; set; }

        [Argument(Name = "可以多选", ControlType = ControlType.CheckBox, Order = 5, Description = "可以选择多个文件")]
        /// <summary>
        /// 可以多选
        /// </summary>
        public bool FileCanMultSelect { get; set; }

        [Argument(Name = "必须多选", ControlType = ControlType.CheckBox, Order = 6, Description = "必须选择多个文件")]
        /// <summary>
        /// 必须多选
        /// </summary>
        public bool FileMustMultSelect { get; set; }

        [Argument(Name = "结果存入", ControlType = ControlType.Variable, Order = 7, Description = "将选择的文件列表路径存为变量")]
        public string SaveVarName { get; set; }

        public override void Execute(ActivityContext context)
        {
            string title = context.ReplaceVar(this.Title);

            litsdk.API.GetMainForm().Invoke((EventHandler)delegate
            {
                while (true)
                {
                    OpenFileDialog ofd = new OpenFileDialog();
                    ofd.CheckFileExists = true;
                    ofd.Title = title;

                    if (!string.IsNullOrEmpty(this.Filter))
                    {
                        List<string> add = new List<string>();
                        foreach (string filter in this.Filter.Split('|'))
                        {
                            add.Add(filter);
                        }
                        ofd.Filter = "文件类型|" + string.Join(";", add.ToArray());
                    }

                    if (this.FileCanMultSelect || this.FileMustMultSelect) ofd.Multiselect = true;
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        if (ofd.Multiselect)//多选
                        {
                            if (this.FileMustMultSelect)
                            {
                                if (ofd.FileNames.Length > 1)
                                {
                                    context.SetVarList(this.SaveVarName, ofd.FileNames.ToList());
                                    context.WriteLog($"成功选择{ofd.FileNames.Length}个文件");
                                    break;
                                }
                                else
                                {
                                    context.WriteLog("必须选择多个文件，请重新选择");
                                }
                            }
                            else
                            {
                                if (ofd.FileNames.Length > 0)
                                {
                                    if (context.ContainsStr(this.SaveVarName))
                                    {
                                        context.SetVarStr(this.SaveVarName, ofd.FileNames[0]);
                                        context.WriteLog($"成功选择文件:{ofd.FileNames[0]}");
                                    }
                                    else
                                    {
                                        context.SetVarList(this.SaveVarName, ofd.FileNames.ToList());
                                        context.WriteLog($"成功选择{ofd.FileNames.Length}个文件");
                                    }
                                    break;
                                }
                            }
                        }
                        else
                        {
                            context.SetVarStr(this.SaveVarName, ofd.FileName);
                            context.WriteLog("成功选择一个文件：" + ofd.FileName);
                            break;
                        }
                    }
                    else
                    {
                        if (this.MustSelect || this.FileMustMultSelect)
                        {
                            if (this.FileMustMultSelect)
                            {
                                context.WriteLog("必须选择多个文件，请重新选择");
                            }
                            else
                            {
                                context.WriteLog("必须选择一个或多个文件，请重新选择");
                            }
                        }
                        else
                        {
                            context.WriteLog("没有选择任何文件，继续下一步");
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

            if (this.FileMustMultSelect)
            {
                if (!context.ContainsList(this.SaveVarName)) throw new Exception($"不存在列表变量{this.SaveVarName}");
            }
            else
            {
                if (this.FileCanMultSelect)
                {
                    if (!context.ContainsList(this.SaveVarName) && !context.ContainsStr(this.SaveVarName)) throw new Exception($"不存在列表或字符变量{this.SaveVarName}");
                }
                else
                {
                    if (!context.ContainsStr(this.SaveVarName)) throw new Exception($"不存在字符变量{this.SaveVarName}");
                }
            }
        }

        public override ControlStyle GetControlStyle(string field)
        {
            litsdk.ControlStyle style = new ControlStyle();

            switch (field)
            {
                case "Title":
                    style.Variables = ControlStyle.GetVariables(true, false, true);
                    break;
                case "Filter":
                    style.Variables = ControlStyle.GetVariables(true, false, true);
                    style.PlaceholderText = "多个类型配置方法 *.txt|*.xlsx";
                    break;
                case "SaveVarName":
                    if (this.FileMustMultSelect)
                    {
                        style.Variables = ControlStyle.GetVariables(false, true);
                    }
                    else
                    {
                        if (this.FileCanMultSelect)
                        {
                            style.Variables = ControlStyle.GetVariables(true, true);
                        }
                        else
                        {
                            style.Variables = ControlStyle.GetVariables(true);
                        }
                    }
                    break;
                case "FileCanMultSelect":
                case "FileMustMultSelect":
                    break;
            }

            return style;
        }
    }
}