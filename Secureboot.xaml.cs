using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace WinArch
{
    /// <summary>
    /// Interaction logic for Secureboot.xaml
    /// </summary>
    public partial class Secureboot : Page
    {
        private string biosmode;
        public Secureboot()
        {
            InitializeComponent();
            GetBIOSMode();
        }
        public void GetBIOSMode()
        {
            Process process = new();
            process.StartInfo.FileName = "powershell.exe";
            process.StartInfo.Arguments = "-Command echo $(Get-ComputerInfo).BiosFirmwareType";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.EnableRaisingEvents = true;
            process.Exited += (s, e) =>
            {
                string output = Regex.Replace(process.StandardOutput.ReadToEnd(), "\\s", "").ToUpper(System.Globalization.CultureInfo.InvariantCulture);
                Application.Current.Properties["biosmode"] = output;
                biosmode = output;
                Debug.WriteLine(biosmode);
                if (biosmode == "UEFI")
                {
                    GetSecureBootStatus();
                }
                else
                {
                    Dispatcher.Invoke(() =>
                    {
                        _ = NavigationService.Navigate(new Uri("Partitioning.xaml", UriKind.Relative));

                    });
                }
            };
            _ = process.Start();
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
                Application.Current.Properties["secureboot"] = output == "True" ? true : (object)false;
                Dispatcher.Invoke(() =>
                {
                    _ = NavigationService.Navigate(new Uri("Partitioning.xaml", UriKind.Relative));
                });
            };
            _ = process.Start();
        }

        private void Previous(object sender, RoutedEventArgs e)
        {
            _ = NavigationService.Navigate(new Uri("About.xaml", UriKind.Relative));
        }
    }
}
