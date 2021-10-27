using System.Windows;
using System.Windows.Controls;

namespace WinArch
{
    /// <summary>
    /// Interaction logic for Finish.xaml
    /// </summary>
    public partial class Finish : Page
    {
        public Finish()
        {
            InitializeComponent();
        }
        private void ButtonRebootNow_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("shutdown.exe", "/r /t 120");
            Application.Current.Shutdown();
        }

        private void ButtonRebootLater_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
