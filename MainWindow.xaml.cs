using System.IO;
using System.Windows;

namespace WinArch
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
			// On exception, write error to a log file
            Dispatcher.UnhandledException += (s, e) =>
            {
                using StreamWriter sw = File.AppendText(Path.Combine(Path.GetTempPath() + "WinArch.log"));
                sw.WriteLine(e.Exception.ToString());
            };
            InitializeComponent();
        }
    }
}
