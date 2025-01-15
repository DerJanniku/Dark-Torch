using System;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using DarkTorch.Core;
using DarkTorch.UI;

namespace DarkTorch.Forms
{
    public class MainForm : Form
    {
        /*
         * MainForm.cs
         * 
         * This form serves as the main interface for the DarkTorch application.
         * It allows users to visualize file connections, preview file contents,
         * and manage notes for individual files.
         * 
         * Features:
         * - Visualize file connections in a project
         * - Preview file contents
         * - Add and manage notes for individual files
         * 
         * Requirements:
         * - .NET Framework 4.7.2 or later
         * - Windows operating system
         */

        private FileVisualizer fileVisualizer;
        private ProjectAnalyzer projectAnalyzer;
        private FileManager fileManager;
        private SplitContainer splitContainer;
        private TextBox previewTextBox;
        private TextBox noteTextBox;
        private Button saveNoteButton;
        private Button openInEditorButton;
        private Button selectFolderButton;

        public MainForm()
        {
            InitializeComponent();
            projectAnalyzer = new ProjectAnalyzer();
            fileManager = new FileManager();
            this.KeyDown += MainForm_KeyDown; // Add key down event handler
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.K) // Check for Ctrl + K
            {
                SelectFolderButton_Click(sender, e); // Trigger folder selection
            }
        }

        private void InitializeComponent()
        {
            this.Text = "DarkTorch";
            this.Size = new System.Drawing.Size(1000, 600);

            // Initialize the Select Folder Button
            selectFolderButton = new Button
            {
                Text = "Select Project Folder",
                Location = new Point(10, 10), // Adjust the location as needed
                Size = new Size(150, 30) // Adjust the size as needed
            };
            selectFolderButton.Click += SelectFolderButton_Click; // Attach the click event handler

            splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 500
            };

            fileVisualizer = new FileVisualizer
            {
                Dock = DockStyle.Fill
            };
            fileVisualizer.FileSelected += FileVisualizer_FileSelected;

            previewTextBox = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                Dock = DockStyle.Top,
                Height = 200
            };

            noteTextBox = new TextBox
            {
                Multiline = true,
                Dock = DockStyle.Fill
            };

            saveNoteButton = new Button
            {
                Text = "Save Note",
                Dock = DockStyle.Bottom
            };
            saveNoteButton.Click += SaveNoteButton_Click;

            openInEditorButton = new Button
            {
                Text = "Open in Editor",
                Dock = DockStyle.Bottom
            };
            openInEditorButton.Click += OpenInEditorButton_Click;

            splitContainer.Panel1.Controls.Add(fileVisualizer);
            splitContainer.Panel2.Controls.Add(noteTextBox);
            splitContainer.Panel2.Controls.Add(previewTextBox);
            splitContainer.Panel2.Controls.Add(saveNoteButton);
            splitContainer.Panel2.Controls.Add(openInEditorButton);

            this.Controls.Add(splitContainer);
            this.Controls.Add(selectFolderButton);
        }

        private void SelectFolderButton_Click(object sender, EventArgs e)
        {
            using (var folderBrowser = new FolderBrowserDialog())
            {
                DialogResult result = folderBrowser.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(folderBrowser.SelectedPath))
                {
                    List<FileInfo> files = projectAnalyzer.AnalyzeProject(folderBrowser.SelectedPath);
                    Dictionary<string, List<string>> connections = projectAnalyzer.FindFileConnections(files);
                    fileVisualizer.SetFileConnections(connections);
                    fileManager.LoadNotes(folderBrowser.SelectedPath);
                }
            }
        }

        private void FileVisualizer_FileSelected(object sender, string filePath)
        {
            previewTextBox.Text = fileManager.GetFilePreview(filePath);
            noteTextBox.Text = fileManager.GetNote(filePath);
        }

        private void SaveNoteButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(fileVisualizer.SelectedFile)) // Check if SelectedFile is not null or empty
            {
                fileManager.SaveNote(fileVisualizer.SelectedFile, noteTextBox.Text);
            }
            else
            {
                MessageBox.Show("Please select a file before saving a note.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void OpenInEditorButton_Click(object sender, EventArgs e)
        {
            if (fileVisualizer.SelectedFile != null)
            {
                string editor = SetupForm.GetSavedEditor();
                if (!string.IsNullOrEmpty(editor))
                {
                    try
                    {
                        Process.Start(editor, fileVisualizer.SelectedFile);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error opening file in editor: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("No text editor selected. Please run the setup again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
