
namespace litapps
{
    partial class UserInputUI
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.btnSave = new Guna.UI2.WinForms.Guna2Button();
            this.gbfMain = new Guna.UI2.WinForms.Guna2BorderlessForm(this.components);
            this.gpHeader = new Guna.UI2.WinForms.Guna2Panel();
            this.lbTitle = new System.Windows.Forms.Label();
            this.gcbClose = new Guna.UI2.WinForms.Guna2ControlBox();
            this.gdcMain = new Guna.UI2.WinForms.Guna2DragControl(this.components);
            this.gpHeader.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnSave
            // 
            this.btnSave.BorderRadius = 5;
            this.btnSave.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnSave.ForeColor = System.Drawing.Color.White;
            this.btnSave.Location = new System.Drawing.Point(130, 284);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(80, 36);
            this.btnSave.TabIndex = 7;
            this.btnSave.Text = "确定";
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // gbfMain
            // 
            this.gbfMain.ContainerControl = this;
            this.gbfMain.DockIndicatorTransparencyValue = 0.6D;
            this.gbfMain.TransparentWhileDrag = true;
            // 
            // gpHeader
            // 
            this.gpHeader.Controls.Add(this.lbTitle);
            this.gpHeader.Controls.Add(this.gcbClose);
            this.gpHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.gpHeader.Location = new System.Drawing.Point(0, 0);
            this.gpHeader.Name = "gpHeader";
            this.gpHeader.Size = new System.Drawing.Size(339, 40);
            this.gpHeader.TabIndex = 8;
            // 
            // lbTitle
            // 
            this.lbTitle.AutoSize = true;
            this.lbTitle.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbTitle.Location = new System.Drawing.Point(12, 11);
            this.lbTitle.Name = "lbTitle";
            this.lbTitle.Size = new System.Drawing.Size(92, 16);
            this.lbTitle.TabIndex = 2;
            this.lbTitle.Text = "这里是标题";
            // 
            // gcbClose
            // 
            this.gcbClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.gcbClose.FillColor = System.Drawing.SystemColors.Control;
            this.gcbClose.IconColor = System.Drawing.Color.Black;
            this.gcbClose.Location = new System.Drawing.Point(299, 6);
            this.gcbClose.Name = "gcbClose";
            this.gcbClose.Size = new System.Drawing.Size(31, 29);
            this.gcbClose.TabIndex = 1;
            // 
            // gdcMain
            // 
            this.gdcMain.DockIndicatorTransparencyValue = 0.6D;
            this.gdcMain.TargetControl = this.gpHeader;
            this.gdcMain.UseTransparentDrag = true;
            // 
            // UserInputUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(339, 354);
            this.Controls.Add(this.gpHeader);
            this.Controls.Add(this.btnSave);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "UserInputUI";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "FrmUserInput";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmUserInput_FormClosing);
            this.Load += new System.EventHandler(this.FrmUserInput_Load);
            this.gpHeader.ResumeLayout(false);
            this.gpHeader.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private Guna.UI2.WinForms.Guna2Button btnSave;
        private Guna.UI2.WinForms.Guna2BorderlessForm gbfMain;
        private Guna.UI2.WinForms.Guna2Panel gpHeader;
        private Guna.UI2.WinForms.Guna2DragControl gdcMain;
        private Guna.UI2.WinForms.Guna2ControlBox gcbClose;
        private System.Windows.Forms.Label lbTitle;
    }
}