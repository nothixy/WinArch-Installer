/*    WinArch installer - a Windows executable to install Archlinux on your PC
    Copyright (C) 2020  srgoti

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.*/

using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;

namespace WinArch
{
    /// <summary>
    /// Interaction logic for Finish.xaml
    /// </summary>
    public partial class Finish : Page
    {
        private bool secureboot;
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
                string output = process.StandardOutput.ReadToEnd().Trim();
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
                    progressBar2.Visibility = Visibility.Hidden;
                    textBlock1.Visibility = Visibility.Hidden;
                });

            };
            _ = process.Start();

        }

        private void ButtonRebootNow_Click(object sender, RoutedEventArgs e)
        {
            Process process = new();
            process.StartInfo.FileName = "shutdown.exe";
            process.StartInfo.Arguments = secureboot ? "/r /fw /t 60" : "/r /t 60";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.EnableRaisingEvents = true;
            process.Exited += (s, e) =>
            {
                Application.Current.Shutdown();
            };
            _ = process.Start();
        }

        private void ButtonRebootLater_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
