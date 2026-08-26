using System.Windows.Forms;

namespace reverse_log_viewer
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.pnlSettings = new System.Windows.Forms.Panel();
            this.chkShowFullPaths = new System.Windows.Forms.CheckBox();
            this.chkPreserveScrollLocation = new System.Windows.Forms.CheckBox();
            this.lblFilter = new System.Windows.Forms.Label();
            this.ctlFilterCombo = new System.Windows.Forms.ComboBox();
            this.filterTypeBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.pnlBody = new System.Windows.Forms.Panel();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pnlSettings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.filterTypeBindingSource)).BeginInit();
            this.pnlBody.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlSettings
            // 
            this.pnlSettings.BackColor = System.Drawing.SystemColors.ControlLight;
            this.pnlSettings.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pnlSettings.Controls.Add(this.chkShowFullPaths);
            this.pnlSettings.Controls.Add(this.chkPreserveScrollLocation);
            this.pnlSettings.Controls.Add(this.lblFilter);
            this.pnlSettings.Controls.Add(this.ctlFilterCombo);
            this.pnlSettings.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlSettings.Location = new System.Drawing.Point(0, 24);
            this.pnlSettings.MaximumSize = new System.Drawing.Size(0, 56);
            this.pnlSettings.MinimumSize = new System.Drawing.Size(0, 56);
            this.pnlSettings.Name = "pnlSettings";
            this.pnlSettings.Size = new System.Drawing.Size(1176, 56);
            this.pnlSettings.TabIndex = 1;
            // 
            // chkShowFullPaths
            // 
            this.chkShowFullPaths.AutoSize = true;
            this.chkShowFullPaths.Font = new System.Drawing.Font("Lucida Sans Unicode", 9F);
            this.chkShowFullPaths.Location = new System.Drawing.Point(428, 16);
            this.chkShowFullPaths.Name = "chkShowFullPaths";
            this.chkShowFullPaths.Size = new System.Drawing.Size(113, 20);
            this.chkShowFullPaths.TabIndex = 3;
            this.chkShowFullPaths.Text = "Show Full Paths";
            this.chkShowFullPaths.UseVisualStyleBackColor = true;
            this.chkShowFullPaths.CheckedChanged += new System.EventHandler(this.ShowFullPaths_CheckedChanged);
            // 
            // chkPreserveScrollLocation
            // 
            this.chkPreserveScrollLocation.AutoSize = true;
            this.chkPreserveScrollLocation.Checked = true;
            this.chkPreserveScrollLocation.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkPreserveScrollLocation.Font = new System.Drawing.Font("Lucida Sans Unicode", 9F);
            this.chkPreserveScrollLocation.Location = new System.Drawing.Point(220, 16);
            this.chkPreserveScrollLocation.Name = "chkPreserveScrollLocation";
            this.chkPreserveScrollLocation.Size = new System.Drawing.Size(158, 20);
            this.chkPreserveScrollLocation.TabIndex = 2;
            this.chkPreserveScrollLocation.Text = "Preserve Scroll Position";
            this.chkPreserveScrollLocation.UseVisualStyleBackColor = true;
            // 
            // lblFilter
            // 
            this.lblFilter.AutoSize = true;
            this.lblFilter.Font = new System.Drawing.Font("Lucida Sans Unicode", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFilter.Location = new System.Drawing.Point(12, 15);
            this.lblFilter.Name = "lblFilter";
            this.lblFilter.Size = new System.Drawing.Size(39, 16);
            this.lblFilter.TabIndex = 1;
            this.lblFilter.Text = "Filter:";
            this.lblFilter.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // ctlFilterCombo
            // 
            this.ctlFilterCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ctlFilterCombo.FormattingEnabled = true;
            this.ctlFilterCombo.Location = new System.Drawing.Point(68, 14);
            this.ctlFilterCombo.Name = "ctlFilterCombo";
            this.ctlFilterCombo.Size = new System.Drawing.Size(121, 21);
            this.ctlFilterCombo.TabIndex = 0;
            this.ctlFilterCombo.SelectedIndexChanged += new System.EventHandler(this.FilterCombo_SelectedIndexChanged);
            // 
            // pnlBody
            // 
            this.pnlBody.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlBody.BackColor = System.Drawing.SystemColors.ControlLight;
            this.pnlBody.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pnlBody.Controls.Add(this.richTextBox1);
            this.pnlBody.Location = new System.Drawing.Point(0, 85);
            this.pnlBody.Margin = new System.Windows.Forms.Padding(0);
            this.pnlBody.Name = "pnlBody";
            this.pnlBody.Padding = new System.Windows.Forms.Padding(3);
            this.pnlBody.Size = new System.Drawing.Size(1176, 495);
            this.pnlBody.TabIndex = 2;
            // 
            // richTextBox1
            // 
            this.richTextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBox1.Font = new System.Drawing.Font("Consolas", 10F);
            this.richTextBox1.Location = new System.Drawing.Point(3, 3);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ReadOnly = true;
            this.richTextBox1.Size = new System.Drawing.Size(1166, 485);
            this.richTextBox1.TabIndex = 1;
            this.richTextBox1.Text = "";
            this.richTextBox1.WordWrap = false;
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1176, 24);
            this.menuStrip1.TabIndex = 3;
            this.menuStrip1.Text = "File";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(46, 20);
            this.fileToolStripMenuItem.Text = "File...";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(112, 22);
            this.openToolStripMenuItem.Text = "Open...";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.OpenMenuItem_Click);
            // 
            // Form1
            // 
            this.ClientSize = new System.Drawing.Size(1176, 580);
            this.Controls.Add(this.pnlBody);
            this.Controls.Add(this.pnlSettings);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "Choose a File";
            this.pnlSettings.ResumeLayout(false);
            this.pnlSettings.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.filterTypeBindingSource)).EndInit();
            this.pnlBody.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Panel pnlSettings;
        private System.Windows.Forms.Label lblFilter;
        private System.Windows.Forms.ComboBox ctlFilterCombo;
        private System.Windows.Forms.CheckBox chkPreserveScrollLocation;
        private System.Windows.Forms.BindingSource filterTypeBindingSource;
        private System.Windows.Forms.Panel pnlBody;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem openToolStripMenuItem;
        private CheckBox chkShowFullPaths;
    }
}
