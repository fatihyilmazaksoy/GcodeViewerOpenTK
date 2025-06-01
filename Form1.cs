using OpenTK;
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
        private float minX = float.MaxValue, minY = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue;

        public Form1()
        {
            InitializeComponent();

            // GLControl'u panel1'e ekle
            glControl = new GLControl();
            glControl.Dock = DockStyle.Fill;
            glControl.BackColor = Color.Black;
            panel1.Controls.Add(glControl);

            // GLControl eventleri
            glControl.Paint += GlControl_Paint;
            glControl.Resize += (s, e) => glControl.Invalidate();

            // Test: glControl görünürlüðü ve boyutu
            this.Shown += (s, e) =>
            {
                MessageBox.Show(glControl.Visible ? "glControl görünür!" : "glControl görünmez!");
                MessageBox.Show($"glControl.Width={glControl.Width}, Height={glControl.Height}");
            };
        }

        private void BtnLoadFile_Click(object? sender, EventArgs e)
        {
            using OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "GCode Dosyalarý (*.nc;*.tap;*.cnc;*.gcode;*.txt)|*.nc;*.tap;*.cnc;*.gcode;*.txt|Tüm Dosyalar (*.*)|*.*"
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                LoadGCodeFile(ofd.FileName);
                MessageBox.Show($"toolPath.Count = {toolPath.Count}");
                glControl.Invalidate();
            }
        }

        // Hem X-Y hem X-Z hem de X veya Y/Z tek baþýna gelebilen GCode için genel loader
        private void LoadGCodeFile(string path)
        {
            toolPath.Clear();
            minX = minY = float.MaxValue;
            maxX = maxY = float.MinValue;

            float x = 0, y = 0, z = 0;
            bool useZ = false;
            var lines = File.ReadAllLines(path);

            foreach (var line in lines)
            {
                var l = line.Trim();

                // Yorum veya boþ satýr atla
                if (string.IsNullOrEmpty(l) || l.StartsWith(";") || l.StartsWith("(")) continue;

                var parts = l.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                bool foundXY = false, foundXZ = false, foundAny = false;

                foreach (var part in parts)
                {
                    if (part.StartsWith("X", StringComparison.OrdinalIgnoreCase))
                    {
                        x = float.Parse(part.Substring(1), CultureInfo.InvariantCulture);
                        foundAny = true;
                    }
                    if (part.StartsWith("Y", StringComparison.OrdinalIgnoreCase))
                    {
                        y = float.Parse(part.Substring(1), CultureInfo.InvariantCulture);
                        foundXY = true;
                        foundAny = true;
                    }
                    if (part.StartsWith("Z", StringComparison.OrdinalIgnoreCase))
                    {
                        z = float.Parse(part.Substring(1), CultureInfo.InvariantCulture);
                        foundXZ = true;
                        foundAny = true;
                    }
                }

                // Eðer hem X ve Y varsa XY düzlemine göre ekle
                if (foundXY && l.Contains("X"))
                {
                    toolPath.Add(new Vector2(x, y));
                    minX = Math.Min(minX, x); minY = Math.Min(minY, y);
                    maxX = Math.Max(maxX, x); maxY = Math.Max(maxY, y);
                }
                // Eðer hem X ve Z varsa XZ düzlemine göre ekle
                else if (foundXZ && l.Contains("X"))
                {
                    toolPath.Add(new Vector2(x, z));
                    minX = Math.Min(minX, x); minY = Math.Min(minY, z);
                    maxX = Math.Max(maxX, x); maxY = Math.Max(maxY, z);
                    useZ = true;
                }
                // Sadece X güncellenmiþse son Y/Z ile ekle (ör: "X..." satýrý)
                else if (foundAny && l.Contains("X") && !l.Contains("Y") && !l.Contains("Z"))
                {
                    float addYorZ = useZ ? z : y;
                    toolPath.Add(new Vector2(x, addYorZ));
                }
                // Sadece Y veya Z güncellenmiþse son X ile ekle (ör: "Y..." veya "Z..." satýrý)
                else if (foundAny && !l.Contains("X"))
                {
                    float addYorZ = useZ ? z : y;
                    toolPath.Add(new Vector2(x, addYorZ));
                }
            }
        }

        private void GlControl_Paint(object? sender, PaintEventArgs e)
        {
            // Arkaplaný kýrmýzýya boya (test1)
            GL.Viewport(0, 0, glControl.Width, glControl.Height);
            GL.ClearColor(Color.Red);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            // 2D ortografik projeksiyon
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, glControl.Width, glControl.Height, 0, -1, 1);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            // Ortada mavi kare (test2)
            int squareSize = 100;
            float centerX = glControl.Width / 2f;
            float centerY = glControl.Height / 2f;
            float half = squareSize / 2f;
            float x0 = centerX - half;
            float y0 = centerY - half;
            float x1 = centerX + half;
            float y1 = centerY + half;

            GL.Color3(Color.Blue);
            GL.Begin(PrimitiveType.LineLoop);
            GL.Vertex2(x0, y0); // Sol üst
            GL.Vertex2(x1, y0); // Sað üst
            GL.Vertex2(x1, y1); // Sað alt
            GL.Vertex2(x0, y1); // Sol alt
            GL.End();

            // toolPath varsa takým yolu çiz (test3)
            if (toolPath.Count > 1)
            {
                GL.Color3(Color.Lime);
                GL.Begin(PrimitiveType.LineStrip);
                foreach (var pt in toolPath)
                {
                    // Ekranýn sol üstü (0,0). Gerekirse scale/offset uygula.
                    GL.Vertex2(pt.X, pt.Y);
                }
                GL.End();
            }

            // Çizgi sayýsýný baþlýkta göster (test4)
            this.Text = $"GcodeViewerOpenTK - Nokta Sayýsý: {toolPath.Count}";

            glControl.SwapBuffers();
        }
    }
}