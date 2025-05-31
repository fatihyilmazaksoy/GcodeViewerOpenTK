using OpenTK.Graphics.OpenGL;
using OpenTK.GLControl;
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

        // View transformation state
        private float zoom = 1.0f;
        private float panX = 0.0f, panY = 0.0f;
        private float rotation = 0.0f;
        private Point lastMousePos;
        private bool isPanning = false;

        public Form1()
        {
            InitializeComponent();

            // GLControl'u panelin içine ekle
            glControl = new GLControl();
            glControl.Dock = DockStyle.Fill; // Paneli tamamen kaplar
            panel1.Controls.Add(glControl);

            // Eventleri baðla
            glControl.Paint += GlControl_Paint;
            glControl.Resize += GlControl_Resize;
            glControl.MouseWheel += GlControl_MouseWheel;
            glControl.MouseDown += GlControl_MouseDown;
            glControl.MouseMove += GlControl_MouseMove;
            glControl.MouseUp += GlControl_MouseUp;
            glControl.KeyDown += GlControl_KeyDown;
            glControl.TabStop = true;

            BtnLoadFile.Click += BtnLoadFile_Click;
        }

        private void BtnLoadFile_Click(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog
            {
                Filter = "Supported GCode Files (*.nc;*.tap;*.cnc;*.gcode;*.txt)|*.nc;*.tap;*.cnc;*.gcode;*.txt|All Files (*.*)|*.*"
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                LoadGCodeFile(ofd.FileName);
                glControl.Invalidate();
            }
        }

        private void LoadGCodeFile(string path)
        {
            toolPath.Clear();
            minX = minY = float.MaxValue;
            maxX = maxY = float.MinValue;

            float x = 0, y = 0;
            var lines = File.ReadAllLines(path);
            foreach (var line in lines)
            {
                var l = line.Trim();
                if (l.StartsWith("G0") || l.StartsWith("G1"))
                {
                    var parts = l.Split(' ');
                    foreach (var part in parts)
                    {
                        if (part.StartsWith("X", StringComparison.OrdinalIgnoreCase))
                            x = float.Parse(part.Substring(1), CultureInfo.InvariantCulture);
                        if (part.StartsWith("Y", StringComparison.OrdinalIgnoreCase))
                            y = float.Parse(part.Substring(1), CultureInfo.InvariantCulture);
                    }
                    toolPath.Add(new Vector2(x, y));
                    minX = Math.Min(minX, x); minY = Math.Min(minY, y);
                    maxX = Math.Max(maxX, x); maxY = Math.Max(maxY, y);
                }
            }
            CenterAndZoomToFit();
        }

        private void CenterAndZoomToFit()
        {
            float dx = maxX - minX;
            float dy = maxY - minY;
            float padding = 0.1f * Math.Max(dx, dy);
            float viewW = glControl.Width, viewH = glControl.Height;
            float dataW = dx + padding, dataH = dy + padding;
            zoom = 0.9f * Math.Min(viewW / dataW, viewH / dataH);
            panX = (viewW / 2f) - ((minX + maxX) / 2f) * zoom;
            panY = (viewH / 2f) - ((minY + maxY) / 2f) * zoom;
        }

        private void GlControl_Paint(object sender, PaintEventArgs e)
        {
            GL.Viewport(0, 0, glControl.Width, glControl.Height);
            //  GL.ClearColor(Color.White);
            GL.ClearColor(System.Drawing.Color.LightGray);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            // Pan & zoom & rotation
            GL.Translate(panX, panY, 0);
            GL.Translate(glControl.Width / 2f, glControl.Height / 2f, 0);
            GL.Rotate(rotation, 0, 0, 1);
            GL.Translate(-glControl.Width / 2f, -glControl.Height / 2f, 0);
            GL.Scale(zoom, zoom, 1);

            // Çizim
            if (toolPath.Count > 1)
            {
                GL.Color3(Color.Blue);
                GL.Begin(PrimitiveType.LineStrip);
                foreach (var pt in toolPath)
                {
                    GL.Vertex2(pt.X, pt.Y);
                }
                GL.End();
            }
            glControl.SwapBuffers();
        }

        private void GlControl_Resize(object sender, EventArgs e)
        {
            glControl.Invalidate();
        }

        private void GlControl_MouseWheel(object sender, MouseEventArgs e)
        {
            float oldZoom = zoom;
            zoom *= (float)Math.Pow(1.1, e.Delta / 120.0);
            panX = e.X - ((e.X - panX) * (zoom / oldZoom));
            panY = e.Y - ((e.Y - panY) * (zoom / oldZoom));
            glControl.Invalidate();
        }

        private void GlControl_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isPanning = true;
                lastMousePos = e.Location;
            }
        }

        private void GlControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (isPanning)
            {
                panX += e.X - lastMousePos.X;
                panY += e.Y - lastMousePos.Y;
                lastMousePos = e.Location;
                glControl.Invalidate();
            }
        }

        private void GlControl_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                isPanning = false;
        }

        private void GlControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left)
                rotation -= 5f;
            else if (e.KeyCode == Keys.Right)
                rotation += 5f;
            glControl.Invalidate();
        }
    }
}