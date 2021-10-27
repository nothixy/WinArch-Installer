using System.Windows;
using System.Windows.Controls;

namespace WinArch
{
    /// <summary>
    /// Interaction logic for Finish.xaml
    /// </summary>
    public partial class Finish : Page
    {
        bool secureboot;
        public Finish()
        {
            InitializeComponent();
            secureboot = (bool)Application.Current.Properties["secureboot"];
        }
        private void ButtonRebootNow_Click(object sender, RoutedEventArgs e)
        {
            if (secureboot)
            {
                System.Diagnostics.Process.Start("shutdown.exe", "/r /fw /t 120");
            }
            else
            {
                System.Diagnostics.Process.Start("shutdown.exe", "/r /t 120");
            }
            Application.Current.Shutdown();
        }

        private void ButtonRebootLater_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
