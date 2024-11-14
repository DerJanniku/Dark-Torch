using System;
using DarkTorch.UI;

namespace DarkTorch
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var app = new System.Windows.Application();
            app.Run(new SetupWindow());
        }
    }
}
