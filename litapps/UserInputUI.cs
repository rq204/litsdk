using litsdk;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Guna.UI2.WinForms;

namespace litapps
{
    internal partial class UserInputUI : Form
    {
        UserInputActivity inputActivity = null;
        ActivityContext context = null;

        public UserInputUI(UserInputActivity userInput, ActivityContext context)
        {
            InitializeComponent();
            this.inputActivity = userInput;
            this.context = context;
            this.Size = new Size(600, 500);
        }

        private void FrmUserInput_Load(object sender, EventArgs e)
        {
            this.Text=this.lbTitle.Text = context.ReplaceVar(inputActivity.FormTitle);
            //确定左侧右侧的最大宽度
            int leftwidth = 0;
            int rightwidth = 300;//宽度设置默认为200

            foreach (UserInputConfig config in this.inputActivity.Configs)
            {
                config.Label = new Label();//字节*6+5
                config.Label.Text = context.ReplaceVar(config.Title);
                int len = System.Text.ASCIIEncoding.Default.GetBytes(config.Label.Text).Length;
                if (len > leftwidth) leftwidth = len;
                config.Panel = new Panel();
                config.Panel.Controls.Add(config.Label);
                int labelHeightY = 18;

                switch (config.Type)
                {
                    case UserInputType.TextBox:
                        Guna.UI2.WinForms.Guna2TextBox textBox = new Guna.UI2.WinForms.Guna2TextBox();
                        textBox.BorderRadius = 5;
                        textBox.Height = 36;
                        config.Panel.Height = 36;
                        textBox.ForeColor = System.Drawing.SystemColors.InfoText;
                        textBox.Text = string.IsNullOrEmpty(config.DefaultVarName) ? "" : context.GetStr(config.DefaultVarName);
                        config.Control = textBox;
                        labelHeightY = 19;
                        break;
                    case UserInputType.MulTextBox:
                        Guna.UI2.WinForms.Guna2TextBox mtextBox = new Guna.UI2.WinForms.Guna2TextBox() { Multiline = true, ScrollBars = ScrollBars.Both, BorderRadius = 5, ForeColor = System.Drawing.SystemColors.InfoText };
                        mtextBox.Text = string.IsNullOrEmpty(config.DefaultVarName) ? "" : context.GetStr(config.DefaultVarName);
                        config.Control = mtextBox;
                        mtextBox.Height = 140;
                        config.Panel.Height = 140;
                        labelHeightY = 19;
                        break;
                    case UserInputType.ComboBox:
                        Guna.UI2.WinForms.Guna2ComboBox comboBox = new Guna.UI2.WinForms.Guna2ComboBox() { DropDownStyle = ComboBoxStyle.DropDownList, BorderRadius = 5, Height = 36 };
                        List<string> vs = context.GetList(config.DefaultVarName);
                        foreach (string v in vs) comboBox.Items.Add(v);
                        if (vs.Count > 0) comboBox.SelectedIndex = 0;
                        config.Control = comboBox;
                        config.Panel.Height = comboBox.Height;
                        break;
                    case UserInputType.RadioButton://字节*6+23
                        List<string> vdes = context.GetList(config.DefaultVarName);
                        int rlen = 0;
                        config.Controls = new List<Control>();
                        foreach (string rd in vdes)
                        {
                            Guna.UI2.WinForms.Guna2RadioButton radio = new Guna.UI2.WinForms.Guna2RadioButton();
                            radio.Text = rd;
                            int ll = getlen(rd) * 6 + 23;
                            radio.Width = ll;
                            rlen += ll;
                            config.Controls.Add(radio);
                            config.Panel.Height = radio.Height;
                        }
                        if (rlen > rightwidth) rightwidth = rlen;
                        break;
                    case UserInputType.CheckBox://字节*6+24
                        int clen = 0;
                        config.Controls = new List<Control>();
                        List<string> checks = new List<string>();
                        if (context.ContainsList(config.DefaultVarName)) checks = context.GetList(config.DefaultVarName);
                        else
                        {
                            checks.Add(context.GetStr(config.DefaultVarName));
                        }

                        foreach (string ck in checks)
                        {
                            Guna.UI2.WinForms.Guna2CheckBox cb = new Guna.UI2.WinForms.Guna2CheckBox();
                            cb.Text = ck;
                            config.Controls.Add(cb);
                            int ll = getlen(ck) * 6 + 24;
                            cb.Width = ll;
                            clen += ll;
                            config.Panel.Height = cb.Height;
                        }
                        if (clen > rightwidth) rightwidth = clen;
                        break;
                    case UserInputType.NumericUpDwon:
                        Guna.UI2.WinForms.Guna2NumericUpDown numericUp = new Guna.UI2.WinForms.Guna2NumericUpDown();
                        numericUp.Maximum = int.MaxValue;
                        numericUp.BorderRadius = 5;
                        numericUp.Height = 36;
                        labelHeightY = 20;
                        if (!string.IsNullOrEmpty(config.DefaultVarName))
                        {
                            int it = context.GetInt(config.DefaultVarName);
                            numericUp.Value = it;
                        }
                        config.Control = numericUp;
                        break;
                    case UserInputType.DateTime:
                        Guna.UI2.WinForms.Guna2DateTimePicker dateTimePicker = new Guna.UI2.WinForms.Guna2DateTimePicker();
                        dateTimePicker.BorderRadius = 5;
                        dateTimePicker.Height = 36;
                        config.Panel.Height = 36;
                        dateTimePicker.ForeColor = System.Drawing.SystemColors.InfoText;
                        DateTime dt = DateTime.Now;
                        if (!string.IsNullOrEmpty(config.DefaultVarName))
                        {
                            string dtstr = context.GetStr(config.DefaultVarName);
                            try
                            {
                                dt = Convert.ToDateTime(dtstr);
                            }
                            catch { }
                        }
                        dateTimePicker.Value = dt;
                        config.Control = dateTimePicker;
                        labelHeightY = 19;
                        break;
                    case UserInputType.Password:
                        Guna.UI2.WinForms.Guna2TextBox pwBox = new Guna.UI2.WinForms.Guna2TextBox();
                        pwBox.BorderRadius = 5;
                        pwBox.UseSystemPasswordChar = true;
                        pwBox.Height = 36;
                        config.Panel.Height = 36;
                        pwBox.ForeColor = System.Drawing.SystemColors.InfoText;
                        pwBox.Text = string.IsNullOrEmpty(config.DefaultVarName) ? "" : context.GetStr(config.DefaultVarName);
                        config.Control = pwBox;
                        labelHeightY = 19;
                        break;
                }

                if (config.Control != null) config.Control.Width = 280;
                if (config.Control != null) config.Panel.Controls.Add(config.Control);
                if (config.Controls != null)
                {
                    foreach (System.Windows.Forms.Control c in config.Controls)
                    {
                        config.Panel.Controls.Add(c);
                    }
                }
                config.Label.Location = new Point(config.Label.Location.X, labelHeightY);
                this.Controls.Add(config.Panel);
            }

            leftwidth = leftwidth * 6;//一个字符占用6计算,前后各加20
            int width = leftwidth + rightwidth + 20 * 2 + 10;
            int lastHeght = 40;
            foreach (UserInputConfig config in this.inputActivity.Configs)
            {
                config.Panel.Location = new Point(0, lastHeght);
                if (config.Type == UserInputType.MulTextBox)
                {
                    config.Panel.Size = new Size(width, 140);//单行的固定值
                }
                else
                {
                    config.Panel.Size = new Size(width, 45);//单行的固定值
                }

                int len = getlen(config.Label.Text) * 6;
                config.Label.Width = len + 5;
                config.Label.Location = new Point(leftwidth - len + 20,config.Label.Location.Y );

                int rowheght = getsize(config.Type);

                int start = leftwidth + 5 + 20 + 10;
                if (config.Control != null) config.Control.Location = new Point(start, (40 - rowheght) / 2);
                else
                {
                    foreach (Control child in config.Controls)
                    {
                        child.Location = new Point(start, (40 - rowheght) / 2);
                        start += child.Width;
                    }
                }

                lastHeght += config.Panel.Height;
            }


            this.Size = new Size(width, lastHeght + 50);
            this.Controls.Add(btnSave);
            btnSave.Location = new Point(width / 2 - btnSave.Width / 2, lastHeght + 5);

            if (this.inputActivity.CanCloseForm && this.inputActivity.TimeOutClose)
            {
                new System.Threading.Thread(() =>
                {
                    System.Threading.Thread.Sleep(this.inputActivity.TimeOutSenconds * 1000);
                    if (this.Disposing || this.IsDisposed) return;
                    try
                    {
                        this.Invoke((EventHandler)delegate { this.Close(); });
                        context.WriteLog($"超时窗口 {this.inputActivity.FormTitle} 被关闭");
                    }
                    catch { }
                }).Start();
            }
        }

        private int getlen(string txt)
        {
            return System.Text.ASCIIEncoding.Default.GetBytes(txt).Length;
        }


        private int getsize(UserInputType inputType)
        {
            int num = 22;
            if (inputType == UserInputType.RadioButton || inputType == UserInputType.CheckBox) num = 16;
            else if (inputType == UserInputType.ComboBox) num = 20;
            return num;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            Dictionary<string, object> adds = new Dictionary<string, object>();

            try
            {
                foreach (UserInputConfig config in this.inputActivity.Configs)
                {
                    if (config.Control != null)
                    {
                        if (config.Control is Guna2TextBox textBox)
                        {
                            if (!config.CanEmpty && string.IsNullOrEmpty(textBox.Text)) throw new Exception(config.Title + " 不能为空");
                            adds.Add(config.ValueVarName, textBox.Text);
                        }
                        else if (config.Control is Guna2ComboBox comboBox)
                        {
                            adds.Add(config.ValueVarName, comboBox.Text);
                        }
                        else if (config.Control is Guna2NumericUpDown numericUpDown)
                        {
                            adds.Add(config.ValueVarName, (int)(numericUpDown.Value));
                        }
                        else if (config.Control is Guna.UI2.WinForms.Guna2DateTimePicker 
 dtp)
                        {
                            adds.Add(config.ValueVarName, dtp.Value.ToString("yyyy-MM-dd"));
                        }
                    }
                    else if (config.Controls.Count > 0)
                    {
                        if (config.Controls[0] is Guna2CheckBox checkBoxC)
                        {
                            List<string> likes = new List<string>();
                            foreach (Guna2CheckBox guna2CheckBox in config.Controls)
                            {
                                if (guna2CheckBox.Checked) likes.Add(guna2CheckBox.Text);
                            }
                            if (context.ContainsStr(config.ValueVarName))
                            {
                                if (likes.Count == 0) adds.Add(config.ValueVarName, "");
                                else adds.Add(config.ValueVarName, likes[0]);
                            }
                            else
                            {
                                adds.Add(config.ValueVarName, likes);
                            }
                        }
                        else if (config.Controls[0] is Guna2RadioButton radioButtonC)
                        {
                            string checktxt = "";
                            foreach (Guna2RadioButton guna2RadioButton in config.Controls)
                            {
                                if (guna2RadioButton.Checked)
                                {
                                    checktxt = guna2RadioButton.Text;
                                    break;
                                }
                            }
                            if (string.IsNullOrEmpty(checktxt) && !config.CanEmpty) throw new Exception(config.ValueVarName + " 必须选择一个");
                            adds.Add(config.ValueVarName, checktxt);
                        }
                    }
                }

                foreach (KeyValuePair<string, object> kv in adds)
                {
                    if (kv.Value is List<string> ls)
                    {
                        context.SetVarList(kv.Key, ls);
                        context.WriteLog($"成功设置变量 {kv.Key} 值为列表\r\n{string.Join("\r\n", ls.ToArray())}\r\n");
                    }
                    else if (kv.Value is string s)
                    {
                        context.SetVarStr(kv.Key, s);
                        if (this.inputActivity.Configs.Find(f => f.Type == UserInputType.Password) == null)
                        {
                            context.WriteLog($"成功设置变量 {kv.Key} 值 {kv.Value}");
                        }
                        else
                        {
                            context.WriteLog($"成功设置变量 {kv.Key} 值为用户密码");
                        }
                    }
                    else if (kv.Value is int i)
                    {
                        context.SetVarInt(kv.Key, i);
                        context.WriteLog($"成功设置变量 {kv.Key} 值 {i}");
                    }

                }
                this.FormClosing -= FrmUserInput_FormClosing;
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "发生错误");
            }
        }

        private void FrmUserInput_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!this.inputActivity.CanCloseForm && e.CloseReason == CloseReason.UserClosing)
            {
                MessageBox.Show("请填写输入选项后点击确认关闭窗体", "禁止关闭");
                e.Cancel = true;
            }
        }
    }
}