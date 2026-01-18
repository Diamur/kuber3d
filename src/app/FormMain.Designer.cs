namespace kuber3d
{
    partial class FormMain
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            pnlTop = new Panel();
            btnStart3D = new Button();
            chkAxes = new CheckBox();
            chkGrid = new CheckBox();
            splitMain = new SplitContainer();
            tvScene = new TreeView();
            pnlViewport = new Panel();
            pnlTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitMain).BeginInit();
            splitMain.Panel1.SuspendLayout();
            splitMain.Panel2.SuspendLayout();
            splitMain.SuspendLayout();
            SuspendLayout();
            // 
            // pnlTop
            // 
            pnlTop.BackColor = SystemColors.ControlLight;
            pnlTop.Controls.Add(btnStart3D);
            pnlTop.Controls.Add(chkAxes);
            pnlTop.Controls.Add(chkGrid);
            pnlTop.Dock = DockStyle.Top;
            pnlTop.Location = new Point(0, 0);
            pnlTop.Name = "pnlTop";
            pnlTop.Size = new Size(1130, 100);
            pnlTop.TabIndex = 0;
            // 
            // btnStart3D
            // 
            btnStart3D.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnStart3D.BackColor = SystemColors.ButtonHighlight;
            btnStart3D.Location = new Point(1058, 21);
            btnStart3D.Name = "btnStart3D";
            btnStart3D.Size = new Size(60, 60);
            btnStart3D.TabIndex = 2;
            btnStart3D.Text = "3D";
            btnStart3D.UseVisualStyleBackColor = false;
            // 
            // chkAxes
            // 
            chkAxes.AutoSize = true;
            chkAxes.Checked = true;
            chkAxes.CheckState = CheckState.Checked;
            chkAxes.Location = new Point(9, 32);
            chkAxes.Name = "chkAxes";
            chkAxes.Size = new Size(48, 19);
            chkAxes.TabIndex = 1;
            chkAxes.Text = "Оси";
            chkAxes.UseVisualStyleBackColor = true;
            // 
            // chkGrid
            // 
            chkGrid.AutoSize = true;
            chkGrid.Checked = true;
            chkGrid.CheckState = CheckState.Checked;
            chkGrid.Location = new Point(10, 10);
            chkGrid.Name = "chkGrid";
            chkGrid.Size = new Size(57, 19);
            chkGrid.TabIndex = 0;
            chkGrid.Text = "Сетка";
            chkGrid.UseVisualStyleBackColor = true;
            // 
            // splitMain
            // 
            splitMain.Dock = DockStyle.Fill;
            splitMain.Location = new Point(0, 100);
            splitMain.Name = "splitMain";
            // 
            // splitMain.Panel1
            // 
            splitMain.Panel1.Controls.Add(tvScene);
            splitMain.Panel1.RightToLeft = RightToLeft.No;
            splitMain.Panel1MinSize = 180;
            // 
            // splitMain.Panel2
            // 
            splitMain.Panel2.Controls.Add(pnlViewport);
            splitMain.Panel2.RightToLeft = RightToLeft.No;
            splitMain.Panel2MinSize = 360;
            splitMain.Size = new Size(1130, 567);
            splitMain.SplitterDistance = 240;
            splitMain.TabIndex = 1;
            // 
            // tvScene
            // 
            tvScene.Dock = DockStyle.Fill;
            tvScene.Location = new Point(0, 0);
            tvScene.Name = "tvScene";
            tvScene.Size = new Size(240, 567);
            tvScene.TabIndex = 0;
            // 
            // pnlViewport
            // 
            pnlViewport.Dock = DockStyle.Fill;
            pnlViewport.Location = new Point(0, 0);
            pnlViewport.Name = "pnlViewport";
            pnlViewport.Size = new Size(886, 567);
            pnlViewport.TabIndex = 0;
            // 
            // FormMain
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1130, 667);
            Controls.Add(splitMain);
            Controls.Add(pnlTop);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "FormMain";
            Text = "Kuber3D";
            pnlTop.ResumeLayout(false);
            pnlTop.PerformLayout();
            splitMain.Panel1.ResumeLayout(false);
            splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitMain).EndInit();
            splitMain.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlTop;
        private Button btnStart3D;
        private CheckBox chkAxes;
        private CheckBox chkGrid;
        private SplitContainer splitMain;
        private TreeView tvScene;
        private Panel pnlViewport;
    }
}