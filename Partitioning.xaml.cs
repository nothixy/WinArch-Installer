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
using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;

namespace WinArch
{
    /// <summary>
    /// Interaction logic for Partitioning.xaml
    /// </summary>
    public partial class Partitioning : Page
    {
        private float spaceleft;
        private float spaceleft_mb;
        private readonly int minimalSpaceRequired = 5000;
        private bool returncode;
        public Partitioning()
        {
            InitializeComponent();
            Mouse.OverrideCursor = Cursors.Wait;
            GetCPUArch();
            GetBIOSMode();
        }
        public void GetCPUArch()
        {
            switch (typeof(string).Assembly.GetName().ProcessorArchitecture)
            {
                case System.Reflection.ProcessorArchitecture.Amd64:
                    break;
                default:
                    string messageBoxText = "Error : this tool is made for AMD64/X86_64 machines only";
                    string caption = "Requirement error";
                    MessageBoxButton button = MessageBoxButton.OK;
                    MessageBoxImage icon = MessageBoxImage.Error;
                    _ = MessageBox.Show(messageBoxText, caption, button, icon);
                    Dispatcher.Invoke(() =>
                    {
                        Application.Current.Shutdown();
                    });
                    break;
            }
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
                string output = Regex.Replace(process.StandardOutput.ReadToEnd(), "\\s", "").ToUpper(System.Globalization.CultureInfo.CurrentUICulture);
                if (output != "UEFI")
                {
                    string messageBoxText = "Error : this tool is made for UEFI machines only";
                    string caption = "Requirement error";
                    MessageBoxButton button = MessageBoxButton.OK;
                    MessageBoxImage icon = MessageBoxImage.Error;
                    _ = MessageBox.Show(messageBoxText, caption, button, icon);
                    Dispatcher.Invoke(() =>
                    {
                        Application.Current.Shutdown();
                    });
                }
                IsDiskInstallable();
            };
            _ = process.Start();

        }
        private void IsDiskInstallable()
        {
            Process process = new();
            process.StartInfo.FileName = "powershell.exe";
            process.StartInfo.Arguments = "(Get-Volume | where-object {$_.Path -eq ((Get-Partition -DiskNumber 0) | where-object {$_.GptType -eq '{c12a7328-f81f-11d2-ba4b-00a0c93ec93b}'}).AccessPaths[-1]}).SizeRemaining -gt 50000000";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.EnableRaisingEvents = true;
            process.Exited += (s, e) =>
            {
                returncode = bool.Parse(Regex.Replace(process.StandardOutput.ReadToEnd().ToLower(System.Globalization.CultureInfo.CurrentUICulture), "\\s", ""));
                if (returncode)
                {
                    MainFunction();
                }
                else
                {
                    string messageBoxText = "Error : not enough space left on the ESP, please make some and try again";
                    string caption = "Requirement error";
                    MessageBoxButton button = MessageBoxButton.OK;
                    MessageBoxImage icon = MessageBoxImage.Error;
                    _ = MessageBox.Show(messageBoxText, caption, button, icon);
                    Dispatcher.Invoke(() =>
                    {
                        Application.Current.Shutdown();
                    });
                }
            };
            _ = process.Start();

        }
        public void MainFunction()
        {
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            foreach (DriveInfo d in allDrives)
            {
                if (d.DriveType == DriveType.Fixed)
                {
                    if (((float)d.TotalSize / (1024 * 1024)) >= minimalSpaceRequired)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            _ = comboBox.Items.Add(d.Name[..1]);
                        });
                    }
                }
            }
            if (comboBox.Items.Count == 0)
            {
                string messageBoxText = "Error : not enough space left on any partition, please make some and try again";
                string caption = "Requirement error";
                MessageBoxButton button = MessageBoxButton.OK;
                MessageBoxImage icon = MessageBoxImage.Error;
                _ = MessageBox.Show(messageBoxText, caption, button, icon);
                Dispatcher.Invoke(() =>
                {
                    Application.Current.Shutdown();
                });
            };
            Dispatcher.Invoke(() =>
            {
                Mouse.OverrideCursor = Cursors.Arrow;
                textBlock1.Visibility = Visibility.Hidden;
                progressBar2.Visibility = Visibility.Hidden;
                page.IsEnabled = true;
            });
            Dispatcher.Invoke(() =>
            {
                comboBox.SelectedIndex = 0;
                checkBox.IsChecked = true;
            });
        }

        private void Unit_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SizeSlider.Minimum = Math.Round(minimalSpaceRequired / Math.Pow(1024, Unit.SelectedIndex), 0);
            SizeSlider.Maximum = Math.Round((long)spaceleft_mb / Math.Pow(1024, Unit.SelectedIndex), 0);
            SizeSlider.Value = SizeSlider.Minimum;
        }

        private void SizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            TextBoxSize.Text = SizeSlider.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        private void TextBoxSize_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Regex.IsMatch(TextBoxSize.Text, "\\d+"))
            {
                SizeSlider.Value = Math.Round(float.Parse(TextBoxSize.Text, System.Globalization.CultureInfo.InvariantCulture), 0);
            }
        }
        public void Previous(object sender, EventArgs e)
        {
            _ = NavigationService.Navigate(new Uri("About.xaml", UriKind.Relative), System.Globalization.CultureInfo.InvariantCulture);
        }
        public void Next(object sender, EventArgs e)
        {
            if (float.Parse(TextBoxSize.Text, System.Globalization.CultureInfo.InvariantCulture) < minimalSpaceRequired / Math.Pow(1024, Unit.SelectedIndex))
            {
                TextBoxSize.Text = Math.Round(minimalSpaceRequired / Math.Pow(1024, Unit.SelectedIndex), 0).ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
            if (float.Parse(TextBoxSize.Text, System.Globalization.CultureInfo.InvariantCulture) > spaceleft_mb / Math.Pow(1024, Unit.SelectedIndex))
            {
                TextBoxSize.Text = Math.Round(spaceleft_mb / Math.Pow(1024, Unit.SelectedIndex), 0).ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
            float spaceneeded = (float)(float.Parse(TextBoxSize.Text, System.Globalization.CultureInfo.InvariantCulture) * Math.Pow(1024, Unit.SelectedIndex));
            Application.Current.Properties["SpaceRequired"] = spaceneeded;
            Application.Current.Properties["Repartition"] = checkBox.IsChecked;
            Application.Current.Properties["Volume"] = comboBox.SelectedItem;
            _ = NavigationService.Navigate(new Uri("Locale.xaml", UriKind.Relative), System.Globalization.CultureInfo.InvariantCulture);
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DriveInfo[] drives = DriveInfo.GetDrives();
            foreach (DriveInfo d in drives)
            {
                if (d.Name[..1] == comboBox.SelectedItem.ToString())
                {
                    spaceleft = d.AvailableFreeSpace;
                    if (d.Name[..1] == "C" && (d.AvailableFreeSpace / (1024 * 1024)) < minimalSpaceRequired)
                    {
                        comboBox.Items.Remove("C");
                        if (comboBox.Items.Count == 0)
                        {
                            string messageBoxText = "Error : not enough space left on any partition, please make some and try again";
                            string caption = "Requirement error";
                            MessageBoxButton button = MessageBoxButton.OK;
                            MessageBoxImage icon = MessageBoxImage.Error;
                            _ = MessageBox.Show(messageBoxText, caption, button, icon);
                            Dispatcher.Invoke(() =>
                            {
                                Application.Current.Shutdown();
                            });
                        }
                    };
                }
            }
            if ((string)comboBox.SelectedItem == "C")
            {
                checkBox.IsChecked = true;
                checkBox.IsEnabled = false;
            }
            else
            {
                checkBox.IsEnabled = true;
            }

            spaceleft_mb = spaceleft / (1024 * 1024);
            if (spaceleft_mb < 1024 * 1024)
            {
                Unit.Items.Remove("TB");
            }
            if (spaceleft_mb < minimalSpaceRequired)
            {
                checkBox.IsChecked = false;
                checkBox.IsEnabled = false;
            }
            SizeSlider.Maximum = spaceleft_mb;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            SizeSlider.IsEnabled = true;
            Unit.IsEnabled = true;
            TextBoxSize.IsEnabled = true;
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            SizeSlider.IsEnabled = false;
            Unit.IsEnabled = false;
            TextBoxSize.IsEnabled = false;
        }
    }
}
