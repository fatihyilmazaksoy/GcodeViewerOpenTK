using OpenTK.GLControl;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;

namespace GcodeViewerOpenTK
{
    public partial class Form1 : Form
    {
        private readonly List<List<Vector2>> toolPaths = new();
        private float minX, minY, maxX, maxY;
        private bool fileLoaded = false;

        // View transformation variables
        private float zoom = 1.0f;
        private Vector2 panOffset = Vector2.Zero;
        private float rotationAngle;
        private Point lastMousePos;
        private bool isDragging;
        private bool isRotating;

        // Constants
        private static readonly char[] separator = { ' ', '\t' };

        public Form1()
        {
            InitializeComponent();
            SetupEventHandlers();
        }

        private void SetupEventHandlers()
        {
            glControl1.Load += GlControl_Load;
            glControl1.Paint += GlControl_Paint;
            glControl1.Resize += GlControl_Resize;
            BtnLoadFile.Click += BtnLoadFile_Click;

            // Mouse event handlers
            glControl1.MouseDown += GlControl_MouseDown;
            glControl1.MouseMove += GlControl_MouseMove;
            glControl1.MouseUp += GlControl_MouseUp;
            glControl1.MouseWheel += GlControl_MouseWheel;
            glControl1.DoubleClick += GlControl_DoubleClick;
        }

        private void GlControl_Load(object sender, EventArgs e)
        {
            InitializeOpenGLSettings();
        }

        private static void InitializeOpenGLSettings()
        {
            try
            {
                GL.ClearColor(Color.Black);
                GL.Enable(EnableCap.LineSmooth);
                GL.Hint(HintTarget.LineSmoothHint, HintMode.Nicest);
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"OpenGL initialization failed: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GlControl_Resize(object sender, EventArgs e)
        {
            if (!glControl1.IsDisposed && glControl1.Visible)
            {
                glControl1.MakeCurrent();
                GL.Viewport(0, 0, glControl1.Width, glControl1.Height);
                glControl1.Invalidate();
            }
        }

        #region Mouse Interaction Handlers
        private void GlControl_MouseDown(object sender, MouseEventArgs e)
        {
            lastMousePos = e.Location;

            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
            }
            else if (e.Button == MouseButtons.Right)
            {
                isRotating = true;
            }
        }
        private void GlControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (!fileLoaded) return;
            float dx = e.X - lastMousePos.X;
            float dy = e.Y - lastMousePos.Y;
            if (isDragging)
            {
                // Pan: Pan offset is after zoom, so divide by zoom
                panOffset.X += dx / (float)glControl1.Width * (maxX - minX);
                panOffset.Y -= dy / (float)glControl1.Height * (maxY - minY);
                glControl1.Invalidate();
            }
            else if (isRotating)
            {
                rotationAngle += dx * 0.5f;
                glControl1.Invalidate();
            }
            lastMousePos = e.Location;
        }

        private void GlControl_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
            isRotating = false;
        }

        private void GlControl_MouseWheel(object sender, MouseEventArgs e)
        {
            if (!fileLoaded) return;

            float zoomFactor = e.Delta > 0 ? 1.1f : 0.9f;
            zoom *= zoomFactor;
            zoom = Math.Max(0.1f, Math.Min(zoom, 10f));
            glControl1.Invalidate();
        }

        private void GlControl_DoubleClick(object sender, EventArgs e)
        {
            zoom = 1.0f;
            panOffset = Vector2.Zero;
            rotationAngle = 0f;
            glControl1.Invalidate();
        }
        #endregion

        private void BtnLoadFile_Click(object? sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog
            {
                Filter = "GCode Files (*.nc;*.tap;*.cnc;*.gcode;*.txt)|*.nc;*.tap;*.cnc;*.gcode;*.txt|All Files (*.*)|*.*",
                Title = "Open GCode File"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                LoadAndDisplayGCodeFile(ofd.FileName);
            }
        }

        private void LoadAndDisplayGCodeFile(string filePath)
        {
            try
            {
                LoadGCodeFile(filePath);
                UpdateWindowTitle(filePath);

                zoom = 1.0f;
                panOffset = Vector2.Zero;
                rotationAngle = 0f;

                glControl1.Invalidate();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading file: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateWindowTitle(string filePath)
        {
            int pointCount = toolPaths.Sum(path => path.Count);
            Text = $"GCode Viewer - {Path.GetFileName(filePath)} (Points: {pointCount})";
        }

        private void LoadGCodeFile(string path)
        {
            ResetState();

            float x = 0, y = 0, z = 0;
            bool toolDown = false;
            List<Vector2> currentPath = null;

            foreach (var line in File.ReadLines(path))
            {
                ProcessGCodeLine(line, ref x, ref y, ref z, ref toolDown, ref currentPath);
            }

            ValidateLoadedData();
        }

        private void ResetState()
        {
            toolPaths.Clear();
            minX = minY = float.MaxValue;
            maxX = maxY = float.MinValue;
            fileLoaded = false;
        }

        private void ProcessGCodeLine(string line, ref float x, ref float y, ref float z,
                                    ref bool toolDown, ref List<Vector2> currentPath)
        {
            var l = line.Trim();
            if (ShouldSkipLine(l)) return;

            bool xSet = false, ySet = false, zSet = false;
            ParseCoordinates(l, ref x, ref y, ref z, ref xSet, ref ySet, ref zSet);

            UpdateToolPath(ref toolDown, ref currentPath, x, y, z, zSet, xSet, ySet);
        }

        private static bool ShouldSkipLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return true;
            if (line.StartsWith(";") || line.StartsWith("("))
                return true;
            if (line.StartsWith("M3") || line.StartsWith("S"))
                return true;
            if (line.StartsWith("G21") || line.StartsWith("G90"))
                return true;
            if (line.StartsWith("T") || line.StartsWith("M30"))
                return true;
            if (line.StartsWith("F") || line.StartsWith("G54"))
                return true;
            if (line.StartsWith("17") || line.StartsWith("G94"))
                return true;
            return false;
        }

        private static void ParseCoordinates(string line, ref float x, ref float y, ref float z,
                                           ref bool xSet, ref bool ySet, ref bool zSet)
        {
            var parts = line.Split(separator, StringSplitOptions.RemoveEmptyEntries);

            foreach (var part in parts.AsSpan())
            {
                if (part.Length == 0) continue;

                char firstChar = char.ToUpperInvariant(part[0]);
                ReadOnlySpan<char> rest = part.AsSpan(1);

                switch (firstChar)
                {
                    case 'X' when float.TryParse(rest, NumberStyles.Float, CultureInfo.InvariantCulture, out float xVal):
                        x = xVal; xSet = true;
                        break;
                    case 'Y' when float.TryParse(rest, NumberStyles.Float, CultureInfo.InvariantCulture, out float yVal):
                        y = yVal; ySet = true;
                        break;
                    case 'Z' when float.TryParse(rest, NumberStyles.Float, CultureInfo.InvariantCulture, out float zVal):
                        z = zVal; zSet = true;
                        break;
                }
            }
        }

        private void UpdateToolPath(ref bool toolDown, ref List<Vector2> currentPath,
                                  float x, float y, float z, bool zSet, bool xSet, bool ySet)
        {
            if (zSet)
            {
                bool newToolDown = z <= 0.0f;
                if (newToolDown != toolDown)
                {
                    toolDown = newToolDown;
                    if (toolDown)
                    {
                        currentPath = new List<Vector2>();
                        toolPaths.Add(currentPath);
                        AddPointToPath(currentPath, x, y);
                    }
                    else
                    {
                        currentPath = null;
                    }
                }
            }

            if (toolDown && currentPath != null && (xSet || ySet))
            {
                AddPointToPath(currentPath, x, y);
            }
        }

        private void AddPointToPath(List<Vector2> path, float x, float y)
        {
            path.Add(new Vector2(x, y));
            UpdateMinMax(x, y);
        }

        private void UpdateMinMax(float x, float y)
        {
            minX = Math.Min(minX, x);
            minY = Math.Min(minY, y);
            maxX = Math.Max(maxX, x);
            maxY = Math.Max(maxY, y);
        }

        private void ValidateLoadedData()
        {
            fileLoaded = toolPaths.Count > 0 && toolPaths.Exists(path => path.Count > 1);
            if (!fileLoaded)
            {
                MessageBox.Show("No valid tool path data found in the file.", "Information",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void GlControl_Paint(object sender, PaintEventArgs e)
        {
            if (glControl1.IsDisposed) return;

            try
            {
                InitializeViewport();
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                if (!fileLoaded)
                {
                    DrawEmptyState();
                }
                else
                {
                    DrawToolPaths();
                }

                glControl1.SwapBuffers();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Rendering error: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeViewport()
        {
            GL.Viewport(0, 0, glControl1.Width, glControl1.Height);
        }

        private void DrawToolPaths()
        {
            SetupProjectionMatrix();

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            // Transform order: Zoom, then Pan (for more intuitive behavior)
            float centerX = (minX + maxX) / 2;
            float centerY = (minY + maxY) / 2;

            GL.Translate(centerX, centerY, 0);
            GL.Rotate(rotationAngle, 0, 0, 1);
            GL.Scale(zoom, zoom, 1.0f);
            GL.Translate(panOffset.X, panOffset.Y, 0);
            GL.Translate(-centerX, -centerY, 0);

            GL.Color3(Color.LimeGreen);
            GL.LineWidth(3f);

            foreach (var path in toolPaths)
            {
                if (path.Count < 2) continue;

                GL.Begin(PrimitiveType.LineStrip);
                foreach (var p in path)
                {
                    GL.Vertex2(p.X, p.Y);
                }
                GL.End();
            }

            DrawAxes();
        }

        private void SetupProjectionMatrix()
        {
            if (glControl1.Height == 0) return; // Prevent division by zero

            float width = Math.Max(1, maxX - minX);
            float height = Math.Max(1, maxY - minY);
            float marginX = width * 0.1f;
            float marginY = height * 0.1f;

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();

            float controlAspect = (float)glControl1.Width / glControl1.Height;
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
        }

        private static void DrawAxes()
        {
            GL.Color3(Color.White);
            GL.Begin(PrimitiveType.Lines);
            GL.Vertex2(-1, 0); GL.Vertex2(1, 0);
            GL.Vertex2(0, -1); GL.Vertex2(0, 1);
            GL.End();
        }

        private void DrawEmptyState()
        {
            SetupEmptyStateProjection();
            DrawGrid();
            DrawCross();
            DrawInfoText();
        }

        private static void SetupEmptyStateProjection()
        {
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(-1, 1, -1, 1, -1, 1);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
        }

        private void DrawGrid()
        {
            GL.Color3(Color.FromArgb(40, 40, 40));
            GL.Begin(PrimitiveType.Lines);

            for (int x = -10; x <= 10; x++)
            {
                GL.Vertex2(x * 0.1f, -1);
                GL.Vertex2(x * 0.1f, 1);
            }

            for (int y = -10; y <= 10; y++)
            {
                GL.Vertex2(-1, y * 0.1f);
                GL.Vertex2(1, y * 0.1f);
            }

            GL.End();
        }

        private static void DrawCross()
        {
            GL.Color3(Color.FromArgb(80, 80, 80));
            GL.Begin(PrimitiveType.Lines);
            GL.Vertex2(-1, 0); GL.Vertex2(1, 0);
            GL.Vertex2(0, -1); GL.Vertex2(0, 1);
            GL.End();
        }

        // --- Text Drawing Helpers ---
        private void Prepare2DTextRendering()
        {
            GL.MatrixMode(MatrixMode.Projection);
            GL.PushMatrix();
            GL.LoadIdentity();
            GL.Ortho(0, glControl1.Width, glControl1.Height, 0, -1, 1); // Top-left is (0,0)

            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();
            GL.LoadIdentity();

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        }

        private void Cleanup2DTextRendering()
        {
            GL.MatrixMode(MatrixMode.Modelview);
            GL.PopMatrix();

            GL.MatrixMode(MatrixMode.Projection);
            GL.PopMatrix();

            GL.Disable(EnableCap.Blend);
        }

        private void DrawInfoText()
        {
            string msg = "Load GCode to view tool path";
            SizeF textSize = TextRenderer.MeasureText(msg, Font);
            int x = (int)(glControl1.Width / 2 - textSize.Width / 2);
            int y = (int)(glControl1.Height / 2 - textSize.Height / 2);
            DrawText(msg, x, y);
        }

        private void DrawText(string text, int x, int y)
        {
            using var bmp = new Bitmap(1, 1);
            using var g = Graphics.FromImage(bmp);
            var size = g.MeasureString(text, SystemFonts.DefaultFont);

            using var textBmp = new Bitmap((int)size.Width, (int)size.Height);
            using (var textG = Graphics.FromImage(textBmp))
            {
                textG.Clear(Color.Transparent);
                textG.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                textG.DrawString(text, SystemFonts.DefaultFont, Brushes.White, 0, 0);
            }

            Prepare2DTextRendering();
            UploadTextureAndDraw(textBmp, x, y);
            Cleanup2DTextRendering();
        }

        private static void UploadTextureAndDraw(Bitmap bitmap, int x, int y)
        {
            GL.Enable(EnableCap.Texture2D);
            int texture = GL.GenTexture();

            try
            {
                GL.BindTexture(TextureTarget.Texture2D, texture);
                var data = bitmap.LockBits(
                    new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    System.Drawing.Imaging.ImageLockMode.ReadOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                    data.Width, data.Height, 0,
                    OpenTK.Graphics.OpenGL.PixelFormat.Bgra,
                    PixelType.UnsignedByte, data.Scan0);

                bitmap.UnlockBits(data);

                SetTextureParameters();
                RenderTexturedQuad(x, y, bitmap.Width, bitmap.Height);
            }
            finally
            {
                GL.DeleteTexture(texture);
                GL.Disable(EnableCap.Texture2D);
            }
        }

        private static void SetTextureParameters()
        {
            GL.TexParameter(TextureTarget.Texture2D,
                TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D,
                TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        }

        private static void RenderTexturedQuad(int x, int y, int width, int height)
        {
            GL.Begin(PrimitiveType.Quads);
            GL.TexCoord2(0, 1); GL.Vertex2(x, y);
            GL.TexCoord2(1, 1); GL.Vertex2(x + width, y);
            GL.TexCoord2(1, 0); GL.Vertex2(x + width, y + height);
            GL.TexCoord2(0, 0); GL.Vertex2(x, y + height);
            GL.End();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            glControl1?.Dispose();
        }
    }
}