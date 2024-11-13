
using System;
using System.Windows.Forms;
using DarkTorch.Forms;
using DarkTorch.Core;
using DarkTorch.UI;

namespace DarkTorch
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            string savedEditor = SetupForm.GetSavedEditor();
            if (string.IsNullOrEmpty(savedEditor))
            {
                using (var setupForm = new SetupForm())
                {
                    if (setupForm.ShowDialog() != DialogResult.OK)
                    {
                        return; // Exit the application if setup is cancelled
                    }
                }
            }

            Application.Run(new MainForm());
        }
    }
}
