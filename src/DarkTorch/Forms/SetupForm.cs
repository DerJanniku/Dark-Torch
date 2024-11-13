
using System;
using System.Windows.Forms;
using System.IO;

namespace DarkTorch.Forms
{
    public class SetupForm : Form
    {
        private ComboBox editorComboBox;
        private Button saveButton;

        public SetupForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "DarkTorch Setup";
            this.Size = new System.Drawing.Size(300, 150);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterScreen;

            Label label = new Label
            {
                Text = "Select your preferred text editor:",
                Location = new System.Drawing.Point(10, 10),
                Size = new System.Drawing.Size(280, 20)
            };

            editorComboBox = new ComboBox
            {
                Location = new System.Drawing.Point(10, 40),
                Size = new System.Drawing.Size(280, 20),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            editorComboBox.Items.AddRange(new string[] { "Notepad", "Notepad++", "Visual Studio Code" });

            saveButton = new Button
            {
                Text = "Save",
                Location = new System.Drawing.Point(110, 80),
                Size = new System.Drawing.Size(80, 30)
            };
            saveButton.Click += SaveButton_Click;

            this.Controls.Add(label);
            this.Controls.Add(editorComboBox);
            this.Controls.Add(saveButton);
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (editorComboBox.SelectedItem != null)
            {
                string selectedEditor = editorComboBox.SelectedItem.ToString();
                SaveEditorPreference(selectedEditor);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Please select a text editor.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveEditorPreference(string editor)
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string darkTorchPath = Path.Combine(appDataPath, "DarkTorch");
            Directory.CreateDirectory(darkTorchPath);
            File.WriteAllText(Path.Combine(darkTorchPath, "editor.txt"), editor);
        }

        public static string GetSavedEditor()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string darkTorchPath = Path.Combine(appDataPath, "DarkTorch");
            string editorPath = Path.Combine(darkTorchPath, "editor.txt");

            if (File.Exists(editorPath))
            {
                return File.ReadAllText(editorPath);
            }

            return null;
        }
    }
}
