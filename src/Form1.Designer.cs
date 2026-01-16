namespace kuber3d
{
    partial class FormMain
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            pnlTop = new Panel();
            pnlHost = new Panel();
            btnStart3D = new Button();
            SuspendLayout();
            // 
            // pnlTop
            // 
            pnlTop.Dock = DockStyle.Top;
            pnlTop.Location = new Point(0, 0);
            pnlTop.Name = "pnlTop";
            pnlTop.Size = new Size(1123, 100);
            pnlTop.TabIndex = 0;
            // 
            // pnlHost
            // 
            pnlHost.BackColor = SystemColors.ActiveCaption;
            pnlHost.Dock = DockStyle.Fill;
            pnlHost.Location = new Point(0, 100);
            pnlHost.Name = "pnlHost";
            pnlHost.Size = new Size(1123, 585);
            pnlHost.TabIndex = 1;
            // 
            // btnStart3D
            // 
            btnStart3D.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnStart3D.Location = new Point(1010, 23);
            btnStart3D.Name = "btnStart3D";
            btnStart3D.Size = new Size(75, 54);
            btnStart3D.TabIndex = 2;
            btnStart3D.Text = "3D";
            btnStart3D.UseVisualStyleBackColor = true;
            // 
            // FormMain
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1123, 685);
            Controls.Add(btnStart3D);
            Controls.Add(pnlHost);
            Controls.Add(pnlTop);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "FormMain";
            RightToLeft = RightToLeft.No;
            Text = "Kuber3D";
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlTop;
        private Panel pnlHost;
        private Button btnStart3D;
    }
}
