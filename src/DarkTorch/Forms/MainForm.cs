
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
        private Button selectFolderButton;
        private FileVisualizer fileVisualizer;
        private ProjectAnalyzer projectAnalyzer;
        private FileManager fileManager;
        private SplitContainer splitContainer;
        private TextBox previewTextBox;
        private TextBox noteTextBox;
        private Button saveNoteButton;
        private Button openInEditorButton;

        public MainForm()
        {
            InitializeComponent();
            projectAnalyzer = new ProjectAnalyzer();
            fileManager = new FileManager();
        }

        private void InitializeComponent()
        {
            this.Text = "DarkTorch";
            this.Size = new System.Drawing.Size(1000, 600);

            selectFolderButton = new Button
            {
                Text = "Select Project Folder",
                Location = new Point(10, 10),
                Size = new Size(150, 30)
            };
            selectFolderButton.Click += SelectFolderButton_Click;

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
            if (fileVisualizer.SelectedFile != null)
            {
                fileManager.SaveNote(fileVisualizer.SelectedFile, noteTextBox.Text);
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
