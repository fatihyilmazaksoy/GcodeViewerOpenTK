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

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            BtnLoadFile = new Button();
            panel1 = new Panel();
            SuspendLayout();
            // 
            // BtnLoadFile
            // 
            BtnLoadFile.Location = new Point(35, 75);
            BtnLoadFile.Name = "BtnLoadFile";
            BtnLoadFile.Size = new Size(75, 23);
            BtnLoadFile.TabIndex = 2;
            BtnLoadFile.Text = "Dosya Aç";
            // 
            // panel1
            // 
            panel1.Location = new Point(169, 75);
            panel1.Name = "panel1";
            panel1.Size = new Size(529, 307);
            panel1.TabIndex = 1;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(panel1);
            Controls.Add(BtnLoadFile);
            Name = "Form1";
            RightToLeftLayout = true;
            Text = "GcodeViewerOpenTK";
            ResumeLayout(false);
        }

        #endregion

        private Button BtnLoadFile;
        private Panel panel1;
    }
}
