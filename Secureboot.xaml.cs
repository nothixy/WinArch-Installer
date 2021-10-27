using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace WinArch
{
    /// <summary>
    /// Interaction logic for Secureboot.xaml
    /// </summary>
    public partial class Secureboot : Page
    {
        string biosmode;
        public Secureboot()
        {
            InitializeComponent();
            GetBIOSMode();
            if (biosmode == "UEFI")
            {
                GetSecureBootStatus();
            }
        }
        public void GetBIOSMode()
        {
            Process process = new Process();
            process.Exited += (s, e) =>
            {
                string output = Regex.Replace(process.StandardOutput.ReadToEnd(), "\\s", "").ToUpper();
                Application.Current.Properties["biosmode"] = output;
                biosmode = output;
            };
            process.StartInfo.FileName = "powershell.exe";
            process.StartInfo.Arguments = "-Command echo $(Get-ComputerInfo).BiosFirmwareType";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.EnableRaisingEvents = true;
            _ = process.Start();
            
        }

        public void GetSecureBootStatus()
        {
            Process process = new Process();
            process.Exited += (s, e) =>
            {
                string output = process.StandardOutput.ReadToEnd();
                if (output == "True")
                {
                    Application.Current.Properties["secureboot"] = true;
                } else
                {
                    Application.Current.Properties["secureboot"] = false;
                }
                _ = NavigationService.Navigate(new Uri("About.xaml", UriKind.Relative));
            };
            process.StartInfo.FileName = "powershell.exe";
            process.StartInfo.Arguments = "Get-SecureBootPolicy";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.EnableRaisingEvents = true;
            _ = process.Start();
        }

        private void Previous(object sender, RoutedEventArgs e)
        {
            _ = NavigationService.Navigate(new Uri("About.xaml", UriKind.Relative));
        }
    }
}
