using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace DarkTorch.UI
{
    [DataContract]
    public class FileVisualizer : Panel
    {
        /*
         * FileVisualizer.cs
         * 
         * The FileVisualizer class is responsible for visualizing file connections
         * within a project. It provides a graphical interface to display files and
         * their relationships, allowing users to select files and view their contents.
         * 
         * Features:
         * - Visualize file connections in a project
         * - Display file names in a graphical format
         * - Handle user interactions for file selection
         */

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
    {
        int centerX = this.Width / 2;
        int centerY = this.Height / 2;
        int radius = 100; // Adjust radius as needed
        int index = 0;

        foreach (var file in fileConnections.Keys)
        {
            // Calculate position based on a spider web layout
            double angle = (2 * Math.PI / fileConnections.Count) * index; // Calculate angle based on file index
            int x = centerX + (int)(radius * Math.Cos(angle));
            int y = centerY + (int)(radius * Math.Sin(angle));

            Rectangle rect = new Rectangle(x, y, 100, 30); // Adjust size as needed
            e.Graphics.DrawRectangle(pen, rect);
            e.Graphics.DrawString(System.IO.Path.GetFileName(file), font, Brushes.Black, rect);

            // Draw connections
            foreach (var connectedFile in fileConnections[file])
            {
                if (fileRectangles.ContainsKey(connectedFile))
                {
                    // Calculate positions for lines
                    Point start = new Point(x + 50, y + 15); // Center of the current file
                    Rectangle connectedRect = fileRectangles[connectedFile];
                    Point end = new Point(connectedRect.X + 50, connectedRect.Y + 15); // Center of the connected file
                    e.Graphics.DrawLine(pen, start, end);
                }
            }
            index++;
        }
    }
{
    base.OnPaint(e);
    if (fileConnections == null) return;

    using (Pen pen = new Pen(Color.Black))
    using (Font font = new Font(FontFamily.GenericSansSerif, 8))
    {
        int centerX = this.Width / 2;
        int centerY = this.Height / 2;
        int radius = 100; // Adjust radius as needed
        int index = 0;

        foreach (var file in fileConnections.Keys)
        {
            // Calculate position based on a spider web layout
            double angle = (2 * Math.PI / fileConnections.Count) * index; // Calculate angle based on file index
            int x = centerX + (int)(radius * Math.Cos(angle));
            int y = centerY + (int)(radius * Math.Sin(angle));

            Rectangle rect = new Rectangle(x, y, 100, 30); // Adjust size as needed
            e.Graphics.DrawRectangle(pen, rect);
            e.Graphics.DrawString(System.IO.Path.GetFileName(file), font, Brushes.Black, rect);

            // Draw connections
            foreach (var connectedFile in fileConnections[file])
            {
                if (fileRectangles.ContainsKey(connectedFile))
                {
                    // Calculate positions for lines
                    Point start = new Point(x + 50, y + 15); // Center of the current file
                    // Calculate the position of the connected file
                    Rectangle connectedRect = fileRectangles[connectedFile];
                    Point end = new Point(connectedRect.X + 50, connectedRect.Y + 15); // Center of the connected file
                    e.Graphics.DrawLine(pen, start, end);
                }
            }
            index++;
        }
    }
}
        }

        private void FileVisualizer_MouseClick(object sender, MouseEventArgs e)
        {
            if (fileRectangles == null) return; // Ensure fileRectangles is populated

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
