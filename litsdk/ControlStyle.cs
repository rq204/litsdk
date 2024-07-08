using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace litsdk
{
    /// <summary>
    /// 控件的样式
    /// </summary>
    public class ControlStyle
    {
        /// <summary>
        /// 是否可见
        /// </summary>
        public bool Visible { get; set; } = true;

        /// <summary>
        /// 变量菜单
        /// </summary>
        public List<string> Variables { get; set; } = new List<string>();

        /// <summary>
        /// 建议值
        /// </summary>
        public List<string> DropDownList { get; set; } = new List<string>();

        /// <summary>
        /// 最小值
        /// </summary>
        public int Min { get; set; } = 0;

        /// <summary>
        /// 最大值
        /// </summary>
        public int Max { get; set; } = int.MaxValue;

        /// <summary>
        /// 自动完成
        /// </summary>
        public List<string> AutoCompleteSource { get; set; } = new List<string>();

        /// <summary>
        /// 打开文件时的过滤
        /// </summary>
        public string Filter { get; set; }

        /// <summary>
        /// 占位符
        /// </summary>
        public string PlaceholderText { get; set; }

        /// <summary>
        /// 重新设置界面
        /// </summary>
        public bool ReloadValue { get; set; }

        /// <summary>
        /// 获取变量列表，多个不同变量间用-分隔
        /// </summary>
        /// <param name="IsStr"></param>
        /// <param name="IsList"></param>
        /// <param name="IsInt"></param>
        /// <param name="IsTable"></param>
        /// <returns></returns>
        public static List<string> GetVariables(bool IsStr, bool IsList = false, bool IsInt = false, bool IsTable = false)
        {
            List<string> ls = new List<string>();
            if (litsdk.API.GetDesignActivityContext != null)
            {
                litsdk.ActivityContext context = litsdk.API.GetDesignActivityContext();
                List<Variable> lstr = context.Variables.FindAll(f => f.VariableType == VariableType.String);
                List<Variable> llist = context.Variables.FindAll(f => f.VariableType == VariableType.List);
                List<Variable> lint = context.Variables.FindAll(f => f.VariableType == VariableType.Int);
                List<Variable> ltable = context.Variables.FindAll(f => f.VariableType == VariableType.Table);
                if (IsStr)
                {
                    foreach (Variable v in lstr)
                    {
                        ls.Add(v.Name);
                    }
                    if (lstr.Count > 0) ls.Add("-");
                }

                if (IsList)
                {
                    foreach (Variable v in llist)
                    {
                        ls.Add(v.Name);
                    }
                    if (llist.Count > 0) ls.Add("-");
                }

                if (IsInt)
                {
                    foreach (Variable v in lint)
                    {
                        ls.Add(v.Name);
                    }
                    if (lint.Count > 0) ls.Add("-");
                }

                if (IsTable)
                {
                    foreach (Variable v in ltable)
                    {
                        ls.Add(v.Name);
                    }
                    if (ltable.Count > 0) ls.Add("-");
                }
            }

            if (ls.Count > 0 && ls.Last() == "-") ls.RemoveAt(ls.Count - 1);

            return ls;
        }
    }
}