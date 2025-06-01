using OpenTK.GLControl;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Windows.Forms;

namespace GcodeViewerOpenTK
{
    public partial class Form1 : Form
    {
        private GLControl glControl;
        private List<Vector2> toolPath = new List<Vector2>();
        private float minX, minY, maxX, maxY;
        private bool fileLoaded = false;

        public Form1()
        {
            InitializeComponent();

            glControl = new GLControl()
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Black
            };
            panel1.Controls.Add(glControl);

            glControl.Load += GlControl_Load;
            glControl.Paint += GlControl_Paint;
            glControl.Resize += (s, e) => glControl.Invalidate();

            BtnLoadFile.Click += BtnLoadFile_Click;
        }

        private void GlControl_Load(object sender, EventArgs e)
        {
            GL.ClearColor(Color.Black);
            GL.Enable(EnableCap.LineSmooth);
            GL.Hint(HintTarget.LineSmoothHint, HintMode.Nicest);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        }

        private void BtnLoadFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "GCode Files (*.nc;*.tap;*.cnc;*.gcode;*.txt)|*.nc;*.tap;*.cnc;*.gcode;*.txt|All Files (*.*)|*.*";
                ofd.Title = "Open GCode File";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    LoadGCodeFile(ofd.FileName);
                    this.Text = $"GCode Viewer - {Path.GetFileName(ofd.FileName)} (Points: {toolPath.Count})";
                    glControl.Invalidate();
                }
            }
        }

        private void LoadGCodeFile(string path)
        {
            toolPath.Clear();
            minX = minY = float.MaxValue;
            maxX = maxY = float.MinValue;
            fileLoaded = false;

            float x = 0, y = 0;
            foreach (var line in File.ReadLines(path))
            {
                var l = line.Trim();
                if (string.IsNullOrEmpty(l) || l.StartsWith(";") || l.StartsWith("(") || l.StartsWith("M") || l.StartsWith("S") || l.StartsWith("G21") || l.StartsWith("G90"))
                    continue;

                bool xSet = false, ySet = false;
                var parts = l.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var part in parts)
                {
                    if (part.StartsWith("X", StringComparison.OrdinalIgnoreCase) && float.TryParse(part.Substring(1), NumberStyles.Float, CultureInfo.InvariantCulture, out float xVal))
                    { x = xVal; xSet = true; }
                    if (part.StartsWith("Y", StringComparison.OrdinalIgnoreCase) && float.TryParse(part.Substring(1), NumberStyles.Float, CultureInfo.InvariantCulture, out float yVal))
                    { y = yVal; ySet = true; }
                }
                if (xSet || ySet)
                {
                    toolPath.Add(new Vector2(x, y));
                    minX = Math.Min(minX, x);
                    minY = Math.Min(minY, y);
                    maxX = Math.Max(maxX, x);
                    maxY = Math.Max(maxY, y);
                }
            }

            fileLoaded = toolPath.Count > 1;
            if (!fileLoaded)
                MessageBox.Show("No valid tool path data found in the file.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void GlControl_Paint(object? sender, PaintEventArgs e)
        {
            if (glControl.IsDisposed) return;
            GL.Viewport(0, 0, glControl.Width, glControl.Height);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            if (!fileLoaded || toolPath.Count <= 1)
                DrawEmptyState();
            else
                DrawToolPath();

            glControl.SwapBuffers();
        }

        private void DrawToolPath()
        {
            // Sýfýr aralýk hatasý varsa sabit sýnýrlar kullan:
            float width = maxX - minX;
            float height = maxY - minY;
            if (width == 0) { width = 1; minX -= 0.5f; maxX += 0.5f; }
            if (height == 0) { height = 1; minY -= 0.5f; maxY += 0.5f; }
            float marginX = width * 0.1f;
            float marginY = height * 0.1f;

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            float controlAspect = (float)glControl.Width / glControl.Height;
            float pathAspect = width / height;

            if (controlAspect > pathAspect)
            {
                float newWidth = height * controlAspect;
                marginX = (newWidth - width) / 2;
                GL.Ortho(minX - marginX, maxX + marginX, minY - marginY, maxY + marginY, -1, 1);
            }
            else
            {
                float newHeight = width / controlAspect;
                marginY = (newHeight - height) / 2;
                GL.Ortho(minX - marginX, maxX + marginX, minY - marginY, maxY + marginY, -1, 1);
            }

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            GL.Color3(Color.LimeGreen);
            GL.LineWidth(3f);
            GL.Begin(PrimitiveType.LineStrip);
            foreach (var p in toolPath)
                GL.Vertex2(p.X, p.Y);
            GL.End();

            DrawAxes(minX - marginX, maxX + marginX, minY - marginY, maxY + marginY);
        }

        private void DrawEmptyState()
        {
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, glControl.Width, glControl.Height, 0, -1, 1);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            // Grid
            GL.Color3(Color.FromArgb(40, 40, 40));
            GL.Begin(PrimitiveType.Lines);
            for (int x = 0; x < glControl.Width; x += 20)
            {
                GL.Vertex2(x, 0);
                GL.Vertex2(x, glControl.Height);
            }
            for (int y = 0; y < glControl.Height; y += 20)
            {
                GL.Vertex2(0, y);
                GL.Vertex2(glControl.Width, y);
            }
            GL.End();

            // Cross
            GL.Color3(Color.FromArgb(80, 80, 80));
            GL.Begin(PrimitiveType.Lines);
            GL.Vertex2(glControl.Width / 2, 0);
            GL.Vertex2(glControl.Width / 2, glControl.Height);
            GL.Vertex2(0, glControl.Height / 2);
            GL.Vertex2(glControl.Width, glControl.Height / 2);
            GL.End();

            // Info
            string msg = "GCode yükle ve takým yolunu gör";
            SizeF textSize = TextRenderer.MeasureText(msg, this.Font);
            DrawText(msg, (int)(glControl.Width / 2 - textSize.Width / 2), (int)(glControl.Height / 2 - textSize.Height / 2));
        }

        private void DrawAxes(float minX, float maxX, float minY, float maxY)
        {
            GL.Color3(Color.White);
            GL.Begin(PrimitiveType.Lines);
            GL.Vertex2(minX, 0); GL.Vertex2(maxX, 0);
            GL.Vertex2(0, minY); GL.Vertex2(0, maxY);
            GL.End();
        }

        private void DrawText(string text, int x, int y)
        {
            using Bitmap bmp = new Bitmap(1, 1);
            SizeF size;
            using (Graphics g = Graphics.FromImage(bmp))
                size = g.MeasureString(text, this.Font);
            using Bitmap bmp2 = new Bitmap((int)size.Width, (int)size.Height);
            using (Graphics g = Graphics.FromImage(bmp2))
            {
                g.Clear(Color.Transparent);
                g.DrawString(text, this.Font, Brushes.White, 0, 0);
            }
            GL.Enable(EnableCap.Texture2D);
            int texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, texture);
            var data = bmp2.LockBits(
                new Rectangle(0, 0, bmp2.Width, bmp2.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                data.Width, data.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra,
                PixelType.UnsignedByte, data.Scan0);

            bmp2.UnlockBits(data);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            GL.Begin(PrimitiveType.Quads);
            GL.TexCoord2(0, 1); GL.Vertex2(x, y);
            GL.TexCoord2(1, 1); GL.Vertex2(x + bmp2.Width, y);
            GL.TexCoord2(1, 0); GL.Vertex2(x + bmp2.Width, y + bmp2.Height);
            GL.TexCoord2(0, 0); GL.Vertex2(x, y + bmp2.Height);
            GL.End();

            GL.DeleteTexture(texture);
            GL.Disable(EnableCap.Texture2D);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (glControl != null && !glControl.IsDisposed)
                glControl.Dispose();
        }
    }
}