using System;
using System.Windows;

namespace Demo
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application app = new Application();
            app.StartupUri= new Uri("MainWindow.xaml", UriKind.Relative);
            app.Run();
        }
    }
}
