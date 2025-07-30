using litsdk;
using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;

namespace litapps
{
    [litsdk.Action(Name = "自定义对话框", Category = "对话框", RefFile = "Guna.UI2.dll", IsFront = true, Order = 145, Description = "创建一个对话框以方便用户输入或选择并获取操作结果")] 
    public class UserInputActivity : litsdk.ProcessActivity
    {
        public override string Name { get; set; } = "自定义对话框";

        /// <summary>
        /// 窗体标题
        /// </summary>
        [Argument(Name = "窗体标题", ControlType = ControlType.TextBox, Order = 1, Description = "显示的窗体的标题")]
        public string FormTitle { get; set; }

        /// <summary>
        /// 可关闭窗口
        /// </summary>
        [Argument(Name = "该窗体显示关闭按钮", ControlType = ControlType.CheckBox, Order = 2, Description = "如果不显示关闭按钮，则必须确定才可以结束")]
        public bool CanCloseForm { get; set; } = false;

        /// <summary>
        /// 超时关闭
        /// </summary>
        [Argument(Name = "超过指定秒数后关闭该窗体", Order = 3, ControlType = ControlType.CheckBox, Description = "超过时间后该窗体会被关闭")]
        public bool TimeOutClose { get; set; } = false;

        /// <summary>
        /// 超时时间秒
        /// </summary>
        [Argument(Name = "超时时间", ControlType = ControlType.NumericUpDown, Order = 4, Description = "超过时间秒后关闭窗体")]
        public int TimeOutSenconds { get; set; } = 30;

        /// <summary>
        /// 配置
        /// </summary>
        [Argument(Name = "控件配置", ControlType = ControlType.IObjectList, Order = 5, Description = "控件的配置数据")]
        public List<UserInputConfig> Configs { get; set; } = new List<UserInputConfig>();

        [Argument(Name = "显示名称", ControlType = ControlType.TextBox, Order = 6, Description = "显示在控件左边的标签内容")]
        public string Title { get; set; }

        [Argument(Name = "控件类型", ControlType = ControlType.ComboBox, Order = 7, Description = "控件类型")]
        public UserInputType Type { get; set; } = UserInputType.TextBox;

        [Argument(Name = "默认值", ControlType = ControlType.Variable, Order = 8, Description = "默认值,按控件可以用字符，数字，列表变量")]
        public string DefaultVarName { get; set; }

        [Argument(Name = "值可为空", ControlType = ControlType.CheckBox, Order = 9, Description = "该值可以为空")]
        public bool CanEmpty { get; set; }

        [Argument(Name = "保存值至", ControlType = ControlType.Variable, Order = 10, Description = "将用户填写的值写入指定变量当中")]
        public string ValueVarName { get; set; }

        [Argument(Name = "", Order = 11, ControlType = ControlType.IObjectAdd, Description = "点击添加，则可将当前配置添加到配置列表当中，如果变量名操作已存在，则更新记录")]
        public string AddButton { get; set; }

        public override void Execute(ActivityContext context)
        {
            litsdk.API.GetMainForm().Invoke((EventHandler)delegate
            {
                UserInputUI userInput = new UserInputUI(this, context);
                userInput.Owner = litsdk.API.GetMainForm();
                userInput.ShowDialog();
            });
        }

        public override void Validate(ActivityContext context)
        {
            if (string.IsNullOrEmpty(this.FormTitle)) throw new Exception("窗口标题不能为空");
            if (Configs.Count == 0) throw new Exception("用户输入配置不能为空");
            foreach (UserInputConfig config in this.Configs)
            {
                switch (config.Type)
                {
                    case UserInputType.TextBox:
                    case UserInputType.MulTextBox:
                    case UserInputType.DateTime:
                    case UserInputType.Password:
                        if (!context.ContainsStr(config.ValueVarName)) throw new Exception(config.Title + " 找不到保存字符变量：" + config.ValueVarName);
                        if (!string.IsNullOrEmpty(config.DefaultVarName) && !context.ContainsStr(config.DefaultVarName)) throw new Exception(config.Title + " 找不到默认字符变量：" + config.DefaultVarName);
                        break;
                    case UserInputType.ComboBox:
                    case UserInputType.RadioButton:
                        if (!context.ContainsStr(config.ValueVarName)) throw new Exception(config.Title + " 找不到保存字符变量：" + config.ValueVarName);
                        if (string.IsNullOrEmpty(config.DefaultVarName) || !context.ContainsList(config.DefaultVarName)) throw new Exception(config.Title + " 找不到默认列表变量：" + config.DefaultVarName);
                        break;
                    case UserInputType.CheckBox:
                        if (string.IsNullOrEmpty(config.DefaultVarName)) throw new Exception(config.Title+ " 默认变量不能为空：" + config.DefaultVarName);
                        if (string.IsNullOrEmpty(config.ValueVarName)) throw new Exception("保存变量不能为空：" + config.ValueVarName);
                        if (context.ContainsList(config.DefaultVarName) && !context.ContainsList(config.ValueVarName)) throw new Exception(config.Title + " 默认值为列表变量时，保存值也必须为列表变量");
                        if (context.ContainsStr(config.DefaultVarName) && !context.ContainsStr(config.ValueVarName)) throw new Exception(config.Title + " 默认值为字符变量时，保存值也必须字符变量");
                        break;
                    case UserInputType.NumericUpDwon:
                        if (!context.ContainsInt(config.ValueVarName)) throw new Exception(config.Title + " 找不到保存数字变量：" + config.ValueVarName);
                        if (!string.IsNullOrEmpty(config.DefaultVarName) && !context.ContainsInt(config.DefaultVarName)) throw new Exception(config.Title + " 找不到默认数字变量：" + config.DefaultVarName);
                        break;
                }

            }
        }

        public override ControlStyle GetControlStyle(string field)
        {
            ControlStyle style = new ControlStyle() { Variables = ControlStyle.GetVariables(true, false, true) };

            switch (field)
            {
                case "DefaultVarName":
                    switch (this.Type)
                    {
                        case UserInputType.TextBox:
                        case UserInputType.MulTextBox:
                        case UserInputType.DateTime:
                        case UserInputType.Password:
                            style.Variables = ControlStyle.GetVariables(true);
                            break;
                        case UserInputType.ComboBox:
                        case UserInputType.RadioButton:
                            style.Variables = ControlStyle.GetVariables(false, true);
                            break;
                        case UserInputType.CheckBox:
                            style.Variables = ControlStyle.GetVariables(true, true);
                            break;
                        case UserInputType.NumericUpDwon:
                            style.Variables = ControlStyle.GetVariables(false, false, true);
                            break;
                    }
                    break;
                case "ValueVarName":
                    switch (this.Type)
                    {
                        case UserInputType.TextBox:
                        case UserInputType.MulTextBox:
                        case UserInputType.ComboBox:
                        case UserInputType.RadioButton:
                        case UserInputType.DateTime:
                        case UserInputType.Password:
                            style.Variables = ControlStyle.GetVariables(true);
                            break;
                        case UserInputType.CheckBox:
                            style.Variables = ControlStyle.GetVariables(true, true);
                            break;
                        case UserInputType.NumericUpDwon:
                            style.Variables = ControlStyle.GetVariables(false, false, true);
                            break;
                    }
                    break;
                case "TimeOutSenconds":
                    style.Visible = this.TimeOutClose;
                    break;
            }

            return style;
        }
    }
}