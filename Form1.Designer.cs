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
            BtnLoadFile = new Button();
            panel1 = new Panel();
            panel12 = new OpenTK.GLControl.GLControl();
            SuspendLayout();
            // 
            // BtnLoadFile
            // 
            BtnLoadFile.Location = new Point(12, 12);
            BtnLoadFile.Name = "BtnLoadFile";
            BtnLoadFile.Size = new Size(100, 30);
            BtnLoadFile.TabIndex = 0;
            BtnLoadFile.Text = "Dosya Aç";
            BtnLoadFile.UseVisualStyleBackColor = true;
            BtnLoadFile.Click += BtnLoadFile_Click;
            // 
            // panel1
            // 
            panel1.Location = new Point(385, 119);
            panel1.Name = "panel1";
            panel1.Size = new Size(355, 300);
            panel1.TabIndex = 1;
            panel1.Paint += GlControl_Paint;
            // 
            // panel12
            // 
            panel12.API = OpenTK.Windowing.Common.ContextAPI.OpenGL;
            panel12.APIVersion = new Version(3, 3, 0, 0);
            panel12.Flags = OpenTK.Windowing.Common.ContextFlags.Default;
            panel12.IsEventDriven = true;
            panel12.Location = new Point(63, 119);
            panel12.Name = "panel12";
            panel12.Profile = OpenTK.Windowing.Common.ContextProfile.Core;
            panel12.SharedContext = null;
            panel12.Size = new Size(291, 248);
            panel12.TabIndex = 2;
            panel12.Paint += GlControl_Paint;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(764, 431);
            Controls.Add(panel12);
            Controls.Add(panel1);
            Controls.Add(BtnLoadFile);
            Name = "Form1";
            Text = "GcodeViewerOpenTK";
            ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button BtnLoadFile;
        private System.Windows.Forms.Panel panel1;
        private OpenTK.GLControl.GLControl panel12;
    }
}