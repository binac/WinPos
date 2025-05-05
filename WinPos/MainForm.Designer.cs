namespace WinPos
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            notifyIcon1 = new NotifyIcon(components);
            toolStrip1 = new ToolStrip();
            btnCtrl = new ToolStripButton();
            btnShift = new ToolStripButton();
            btnWin = new ToolStripButton();
            btnAlt = new ToolStripButton();
            txtKey = new ToolStripTextBox();
            btnOK = new Button();
            btnCancel = new Button();
            toolStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // notifyIcon1
            // 
            notifyIcon1.Text = "WinPos";
            notifyIcon1.Visible = true;
            // 
            // toolStrip1
            // 
            toolStrip1.AutoSize = false;
            toolStrip1.BackColor = Color.WhiteSmoke;
            toolStrip1.Font = new Font("Segoe UI", 11F);
            toolStrip1.GripStyle = ToolStripGripStyle.Hidden;
            toolStrip1.Items.AddRange(new ToolStripItem[] { btnCtrl, btnShift, btnWin, btnAlt, txtKey });
            toolStrip1.Location = new Point(10, 18);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.RenderMode = ToolStripRenderMode.Professional;
            toolStrip1.Size = new Size(270, 33);
            toolStrip1.TabIndex = 0;
            toolStrip1.Text = "toolStrip1";
            // 
            // btnCtrl
            // 
            btnCtrl.CheckOnClick = true;
            btnCtrl.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnCtrl.Font = new Font("Segoe UI", 9.75F);
            btnCtrl.ForeColor = Color.DarkGray;
            btnCtrl.Image = (Image)resources.GetObject("btnCtrl.Image");
            btnCtrl.ImageTransparentColor = Color.Magenta;
            btnCtrl.Name = "btnCtrl";
            btnCtrl.Size = new Size(32, 30);
            btnCtrl.Text = "Ctrl";
            btnCtrl.CheckedChanged += Btn_CheckedChanged;
            // 
            // btnShift
            // 
            btnShift.Checked = true;
            btnShift.CheckOnClick = true;
            btnShift.CheckState = CheckState.Checked;
            btnShift.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnShift.Font = new Font("Segoe UI", 9.75F);
            btnShift.ForeColor = Color.Black;
            btnShift.Image = (Image)resources.GetObject("btnShift.Image");
            btnShift.ImageTransparentColor = Color.Magenta;
            btnShift.Name = "btnShift";
            btnShift.Size = new Size(50, 30);
            btnShift.Text = "+ Shift";
            btnShift.CheckedChanged += Btn_CheckedChanged;
            // 
            // btnWin
            // 
            btnWin.Checked = true;
            btnWin.CheckOnClick = true;
            btnWin.CheckState = CheckState.Checked;
            btnWin.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnWin.Font = new Font("Segoe UI", 9.75F);
            btnWin.ForeColor = Color.Black;
            btnWin.Image = (Image)resources.GetObject("btnWin.Image");
            btnWin.ImageTransparentColor = Color.Magenta;
            btnWin.Name = "btnWin";
            btnWin.Size = new Size(47, 30);
            btnWin.Text = "+ Win";
            btnWin.CheckedChanged += Btn_CheckedChanged;
            // 
            // btnAlt
            // 
            btnAlt.CheckOnClick = true;
            btnAlt.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnAlt.Font = new Font("Segoe UI", 9.75F);
            btnAlt.ForeColor = Color.DarkGray;
            btnAlt.Image = (Image)resources.GetObject("btnAlt.Image");
            btnAlt.ImageTransparentColor = Color.Magenta;
            btnAlt.Name = "btnAlt";
            btnAlt.Size = new Size(40, 30);
            btnAlt.Text = "+ Alt";
            btnAlt.CheckedChanged += Btn_CheckedChanged;
            // 
            // txtKey
            // 
            txtKey.BackColor = Color.White;
            txtKey.Font = new Font("Segoe UI", 11F);
            txtKey.Name = "txtKey";
            txtKey.ReadOnly = true;
            txtKey.Size = new Size(85, 33);
            txtKey.Text = "+ Subtract";
            txtKey.KeyDown += TxtKey_KeyDown;
            // 
            // btnOK
            // 
            btnOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnOK.DialogResult = DialogResult.OK;
            btnOK.Location = new Point(112, 67);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(75, 23);
            btnOK.TabIndex = 2;
            btnOK.Text = "OK";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += BtnOK_Click;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(193, 67);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 23);
            btnCancel.TabIndex = 2;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += BtnCancel_Click;
            // 
            // MainForm
            // 
            AcceptButton = btnOK;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.WhiteSmoke;
            CancelButton = btnCancel;
            ClientSize = new Size(280, 102);
            ControlBox = false;
            Controls.Add(btnCancel);
            Controls.Add(btnOK);
            Controls.Add(toolStrip1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "MainForm";
            Padding = new Padding(10, 18, 0, 0);
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Set shortcut key";
            TopMost = true;
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private NotifyIcon notifyIcon1;
        private ToolStrip toolStrip1;
        private ToolStripButton btnCtrl;
        private ToolStripButton btnShift;
        private ToolStripButton btnWin;
        private ToolStripButton btnAlt;
        private ToolStripTextBox txtKey;
        private Button btnOK;
        private Button btnCancel;
    }
}
