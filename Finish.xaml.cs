using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System;

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
            Main();
        }
        public void Main()
        {
            GetSecureBootStatus();
        }
        public void GetSecureBootStatus()
        {
            Process process = new();
            process.StartInfo.FileName = "powershell.exe";
            process.StartInfo.Arguments = "Confirm-SecureBootUEFI";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.EnableRaisingEvents = true;
            process.Exited += (s, e) =>
            {
                string output = process.StandardOutput.ReadToEnd();
                Debug.WriteLine("Secure Boot : " + output);
                if (output == "True")
                {
                    secureboot = true;
                    Reminder.Visibility = Visibility.Visible;
                }
                else
                {
                    secureboot = false;
                }
                Dispatcher.Invoke(() =>
                {
                    ButtonRebootLater.IsEnabled = true;
                    ButtonRebootNow.IsEnabled = true;
                });

            };
            _ = process.Start();

        }

        private void ButtonRebootNow_Click(object sender, RoutedEventArgs e)
        {
            if (secureboot)
            {
                Process.Start("shutdown.exe", "/r /fw /t 60");
            }
            else
            {
                Process.Start("shutdown.exe", "/r /t 60");
            }
            Application.Current.Shutdown();
        }

        private void ButtonRebootLater_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
