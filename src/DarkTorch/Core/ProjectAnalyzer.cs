using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace DarkTorch.Core
{
    public class ProjectAnalyzer
    {
        private readonly string[] codeFileExtensions = { ".cs", ".java", ".py", ".js", ".html", ".css" };

        public List<FileInfo> AnalyzeProject(string projectPath)
        {
            List<FileInfo> files = new List<FileInfo>();
            try
            {
                foreach (string file in Directory.GetFiles(projectPath, "*.*", SearchOption.AllDirectories))
                {
                    if (Array.Exists(codeFileExtensions, ext => ext.Equals(Path.GetExtension(file), StringComparison.OrdinalIgnoreCase)))
                    {
                        files.Add(new FileInfo(file));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error analyzing project: {ex.Message}");
            }
            return files;
        }

        public Dictionary<string, List<string>> FindFileConnections(List<FileInfo> files)
        {
            Dictionary<string, List<string>> connections = new Dictionary<string, List<string>>();

            foreach (var file in files)
            {
                connections[file.FullName] = new List<string>();
                string content = File.ReadAllText(file.FullName);

                foreach (var otherFile in files)
                {
                    if (file != otherFile)
                    {
                        string fileName = Path.GetFileNameWithoutExtension(otherFile.Name);
                        // Enhanced regex patterns to match method calls and class references
                        if (Regex.IsMatch(content, $@"\b{Regex.Escape(fileName)}\b") || 
                            Regex.IsMatch(content, $@"\b{Regex.Escape(fileName)}\s*\(")) // Match method calls
                        {
                            connections[file.FullName].Add(otherFile.FullName);
                        }
                    }
                }
            }

            return connections;
        }
    }
}
