using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace DarkTorch.UI
{
    [DataContract]
    public class GraphView : Panel
    {
        private Dictionary<string, List<string>> fileConnections;
        private Dictionary<string, Rectangle> fileRectangles;

        public event EventHandler<string> FileSelected;

        [DataMember]
        private string _selectedFile = string.Empty; // Initialize the backing field
        [DataMember(Name = "SelectedFile")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string SelectedFile
        {
            get => _selectedFile;
            set
            {
                _selectedFile = value;
                // Additional logic can be added here if needed
            }
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            SelectedFile = _selectedFile;
        }

        public GraphView()
        {
            this.DoubleBuffered = true;
            this.MouseClick += GraphView_MouseClick;
            this.MouseWheel += GraphView_MouseWheel;
            fileRectangles = new Dictionary<string, Rectangle>();
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
            int index = 0;
            int radius = 100; // Adjust radius as needed
            int centerX = this.Width / 2;
            int centerY = this.Height / 2;

            foreach (var file in fileConnections.Keys)
            {
                double angle = (2 * Math.PI / fileConnections.Count) * index;
                int x = centerX + (int)(radius * Math.Cos(angle));
                int y = centerY + (int)(radius * Math.Sin(angle));
                Rectangle rect = new Rectangle(x, y, 100, 30);
                fileRectangles[file] = rect;
                index++;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (fileConnections == null) return;

            using (Pen pen = new Pen(Color.Black))
            using (Font font = new Font(FontFamily.GenericSansSerif, 8))
            {
                foreach (var file in fileConnections.Keys)
                {
                    Rectangle rect = fileRectangles[file];
                    e.Graphics.DrawRectangle(pen, rect);
                    e.Graphics.DrawString(System.IO.Path.GetFileName(file), font, Brushes.Black, rect);

                    foreach (var connectedFile in fileConnections[file])
                    {
                        if (fileRectangles.ContainsKey(connectedFile))
                        {
                            Point start = new Point(rect.X + 50, rect.Y + 15);
                            Rectangle connectedRect = fileRectangles[connectedFile];
                            Point end = new Point(connectedRect.X + 50, connectedRect.Y + 15);
                            e.Graphics.DrawLine(pen, start, end);
                        }
                    }
                }
            }
        }

        private void GraphView_MouseClick(object sender, MouseEventArgs e)
        {
            if (fileRectangles == null) return;

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

        private void GraphView_MouseWheel(object sender, MouseEventArgs e)
        {
            // Logic to handle zooming in and out
            if (e.Delta > 0)
            {
                // Zoom in
            }
            else
            {
                // Zoom out
            }
        }
    }
}
