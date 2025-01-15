using System;
using System.IO;
using System.Collections.Generic;

namespace DarkTorch.Core
{
    public class FileManager
    {
        private Dictionary<string, string> fileNotes;

        public FileManager()
        {
            fileNotes = new Dictionary<string, string>();
        }

        public string GetFilePreview(string filePath, int previewLines = 10)
        {
            try
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string preview = "";
                    for (int i = 0; i < previewLines; i++)
                    {
                        string line = reader.ReadLine();
                        if (line == null) break;
                        preview += line + Environment.NewLine;
                    }
                    return preview;
                }
            }
            catch (Exception ex)
            {
                return $"Error reading file: {ex.Message}";
            }
        }

        public void SaveNote(string filePath, string note)
        {
            string notePath = Path.ChangeExtension(filePath, ".dark");
            File.WriteAllText(notePath, note);
            fileNotes[filePath] = note;
        }

        public string GetNote(string filePath)
        {
            string notePath = Path.ChangeExtension(filePath, ".dark");
            if (File.Exists(notePath))
            {
                return File.ReadAllText(notePath);
            }
            return "";
        }

        public void LoadNotes(string projectPath)
        {
            fileNotes.Clear();
            string[] darkFiles = Directory.GetFiles(projectPath, "*.dark", SearchOption.AllDirectories);
            foreach (string darkFile in darkFiles)
            {
                string originalFile = Path.ChangeExtension(darkFile, Path.GetExtension(darkFile.Replace(".dark", "")));
                if (File.Exists(originalFile))
                {
                    fileNotes[originalFile] = File.ReadAllText(darkFile);
                }
            }
        }
    }
}
