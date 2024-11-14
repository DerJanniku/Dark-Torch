using System.Windows;
using System.IO;
using System.Text.Json;

namespace DarkTorch.UI
{
    public partial class SetupWindow : Window
    {
        public SetupWindow()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();
            this.Close();
        }

        private void LoadSettings()
        {
            if (File.Exists("settings.json"))
            {
                string json = File.ReadAllText("settings.json");
                var settings = JsonSerializer.Deserialize<Settings>(json);
                EditorComboBox.SelectedItem = settings.PreferredEditor;
                ThemeComboBox.SelectedItem = settings.Theme;
            }
        }

        private void SaveSettings()
        {
            var settings = new Settings
            {
                PreferredEditor = EditorComboBox.SelectedItem.ToString(),
                Theme = ThemeComboBox.SelectedItem.ToString()
            };

            string json = JsonSerializer.Serialize(settings);
            File.WriteAllText("settings.json", json);
        }
    }

    public class Settings
    {
        public string PreferredEditor { get; set; }
        public string Theme { get; set; }
    }
}
