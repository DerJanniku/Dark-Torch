
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace DarkTorch.UI
{
    public class FileVisualizer : Panel
    {
        private Dictionary<string, List<string>> fileConnections;
        private Dictionary<string, Rectangle> fileRectangles;

        public event EventHandler<string> FileSelected;
        public string SelectedFile { get; private set; }

        public FileVisualizer()
        {
            this.DoubleBuffered = true;
            this.MouseClick += FileVisualizer_MouseClick;
        }

        public void SetFileConnections(Dictionary<string, List<string>> connections)
        {
            fileConnections = connections;
            CalculateFilePositions();
            this.Invalidate();
        }

        private void CalculateFilePositions()
        {
            fileRectangles = new Dictionary<string, Rectangle>();
            int x = 10;
            int y = 10;
            int width = 100;
            int height = 30;

            foreach (var file in fileConnections.Keys)
            {
                fileRectangles[file] = new Rectangle(x, y, width, height);
                y += height + 10;
                if (y > this.Height - height)
                {
                    y = 10;
                    x += width + 10;
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (fileConnections == null) return;

            using (Pen pen = new Pen(Color.Black))
            using (Font font = new Font(FontFamily.GenericSansSerif, 8))
            using (StringFormat sf = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                foreach (var file in fileRectangles.Keys)
                {
                    e.Graphics.DrawRectangle(pen, fileRectangles[file]);
                    e.Graphics.DrawString(System.IO.Path.GetFileName(file), font, Brushes.Black, fileRectangles[file], sf);

                    if (fileConnections.ContainsKey(file))
                    {
                        foreach (var connectedFile in fileConnections[file])
                        {
                            if (fileRectangles.ContainsKey(connectedFile))
                            {
                                Point start = new Point(fileRectangles[file].Right, fileRectangles[file].Top + fileRectangles[file].Height / 2);
                                Point end = new Point(fileRectangles[connectedFile].Left, fileRectangles[connectedFile].Top + fileRectangles[connectedFile].Height / 2);
                                e.Graphics.DrawLine(pen, start, end);
                            }
                        }
                    }
                }
            }
        }

        private void FileVisualizer_MouseClick(object sender, MouseEventArgs e)
        {
            foreach (var file in fileRectangles.Keys)
            {
                if (fileRectangles[file].Contains(e.Location))
                {
                    SelectedFile = file;
                    FileSelected?.Invoke(this, file);
                    break;
                }
            }
        }
    }
}
