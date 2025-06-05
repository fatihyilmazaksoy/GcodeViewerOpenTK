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
            glControl1 = new OpenTK.GLControl.GLControl();
            openFileDialog1 = new OpenFileDialog();
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
            // glControl1
            // 
            glControl1.API = OpenTK.Windowing.Common.ContextAPI.OpenGL;
            glControl1.APIVersion = new Version(3, 3, 0, 0);
            glControl1.Flags = OpenTK.Windowing.Common.ContextFlags.Default;
            glControl1.IsEventDriven = true;
            glControl1.Location = new Point(12, 71);
            glControl1.Name = "glControl1";
            glControl1.Profile = OpenTK.Windowing.Common.ContextProfile.Core;
            glControl1.SharedContext = null;
            glControl1.Size = new Size(467, 316);
            glControl1.TabIndex = 2;
            // 
        
            // openFileDialog1
            // 
            openFileDialog1.FileName = "openFileDialog1";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(534, 411);
            Controls.Add(glControl1);
            Controls.Add(glControl1);
            Controls.Add(BtnLoadFile);
            Name = "Form1";
            Text = "GcodeViewerOpenTK";
            ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button BtnLoadFile;
        private OpenTK.GLControl.GLControl glControl1;
        private OpenFileDialog openFileDialog1;
    }
}