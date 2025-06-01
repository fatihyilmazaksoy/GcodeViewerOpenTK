namespace GcodeViewerOpenTK
{
    partial class Form1
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

        private void InitializeComponent()
        {
            this.BtnLoadFile = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // BtnLoadFile
            // 
            this.BtnLoadFile.Location = new System.Drawing.Point(12, 12);
            this.BtnLoadFile.Name = "BtnLoadFile";
            this.BtnLoadFile.Size = new System.Drawing.Size(100, 30);
            this.BtnLoadFile.TabIndex = 0;
            this.BtnLoadFile.Text = "Dosya Aç";
            this.BtnLoadFile.UseVisualStyleBackColor = true;
            this.BtnLoadFile.Click += new System.EventHandler(this.BtnLoadFile_Click);
            // 
            // panel1
            // 
            this.panel1.Location = new System.Drawing.Point(140, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(600, 400);
            this.panel1.TabIndex = 1;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(764, 431);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.BtnLoadFile);
            this.Name = "Form1";
            this.Text = "GcodeViewerOpenTK";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button BtnLoadFile;
        private System.Windows.Forms.Panel panel1;
    }
}