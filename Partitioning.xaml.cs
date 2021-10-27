﻿/*    WinArch installer - a Windows executable to install Archlinux on your PC
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
using System.Threading.Tasks;
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
        private bool canInstall;
        private string biosmode;
        private readonly int minimalSpaceRequired = 2500;
        private bool returncode;
        public Partitioning()
        {
            InitializeComponent();
            Mouse.OverrideCursor = Cursors.Wait;
            biosmode = (string)Application.Current.Properties["biosmode"];
            MainFunction();
        }

        private void IsDiskInstallable(string partname)
        {
            Process process = new Process();
            process.StartInfo.FileName = "powershell.exe";
            process.StartInfo.Arguments = biosmode == "BIOS"
                ? "if ((Get-partition -DiskNumber ((Get-Partition -DriveLetter " + partname + ").DiskNumber)).PartitionNumber -contains 0) { ((Get-Partition -DriveLetter " + partname + ").Offset -ge (Get-Partition -PartitionNumber 0).Offset -and (Get-Partition -DriveLetter " + partname + ").Offset -lt ((Get-Partition -PartitionNumber 0).Offset + (Get-Partition -PartitionNumber 0).Size)) } else { if (((Get-partition -DiskNumber ((Get-Partition -DriveLetter C).DiskNumber)).PartitionNumber | Measure-Object -line).Lines -gt 3) { echo False } else { echo True }}"
                : @"-executionpolicy unrestricted (Get-Volume | where-object {$_.Path -eq ((Get-Partition -DiskNumber 0) | where-object {$_.GptType -eq '{c12a7328-f81f-11d2-ba4b-00a0c93ec93b}'}).AccessPaths[-1]}).SizeRemaining -gt 50000000";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.EnableRaisingEvents = true;
            process.Start();
            process.WaitForExit();
            returncode = bool.Parse(Regex.Replace(process.StandardOutput.ReadToEnd().ToLower(), "\\s", ""));
            if (returncode)
            {
                Dispatcher.Invoke(() =>
                {
                    _ = comboBox.Items.Add(partname);
                });
                canInstall = true;
            }
        }
        public void MainFunction()
        {
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            Dispatcher.Invoke(() =>
            {
                progressBar2.IsIndeterminate = false;
                progressBar2.Maximum = allDrives.Length;
                progressBar2.Value = 0;
            });
            foreach (DriveInfo d in allDrives)
            {
                Dispatcher.Invoke(() =>
                {
                    progressBar2.Value = progressBar2.Value + 1;
                });
                if (d.DriveType == DriveType.Fixed)
                {
                    if (((float)d.TotalSize / (1024 * 1024)) >= minimalSpaceRequired)
                    {
                        IsDiskInstallable(d.Name.Substring(0, 1));
                    }
                }
            }
            if (!canInstall)
            {
                Dispatcher.Invoke(() =>
                {
                    err.Visibility = Visibility.Visible;
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
            SizeSlider.Minimum = Math.Round((long)minimalSpaceRequired / Math.Pow(1024, Unit.SelectedIndex), 0);
            SizeSlider.Maximum = Math.Round((long)spaceleft_mb / Math.Pow(1024, Unit.SelectedIndex), 0);
            SizeSlider.Value = SizeSlider.Minimum;
        }

        private void SizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            TextBoxSize.Text = SizeSlider.Value.ToString();
        }

        private void TextBoxSize_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Regex.IsMatch(TextBoxSize.Text, "\\d+"))
            {
                SizeSlider.Value = Math.Round(float.Parse(TextBoxSize.Text), 0);
            }
        }
        public void Previous(object sender, EventArgs e)
        {
            NavigationService nav = NavigationService;
            _ = nav.Navigate(new Uri("About.xaml", UriKind.Relative));
        }
        public void Next(object sender, EventArgs e)
        {
            if (float.Parse(TextBoxSize.Text) < minimalSpaceRequired / Math.Pow(1024, Unit.SelectedIndex))
            {
                TextBoxSize.Text = Math.Round(minimalSpaceRequired / Math.Pow(1024, Unit.SelectedIndex), 0).ToString();
            }
            if (float.Parse(TextBoxSize.Text) > spaceleft_mb / Math.Pow(1024, Unit.SelectedIndex))
            {
                TextBoxSize.Text = Math.Round(spaceleft_mb / Math.Pow(1024, Unit.SelectedIndex), 0).ToString();
            }
            float spaceneeded = (float)(float.Parse(TextBoxSize.Text) * Math.Pow(1024, Unit.SelectedIndex));
            if ((bool)checkBox.IsChecked)
            {
                Application.Current.Properties["SpaceRequired"] = spaceneeded;
            }
            Application.Current.Properties["Repartition"] = checkBox.IsChecked;
            Application.Current.Properties["Volume"] = comboBox.SelectedItem;
            NavigationService nav = NavigationService;
            _ = nav.Navigate(new Uri("Locale.xaml", UriKind.Relative));
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBox.SelectedItem.ToString() == "C")
            {
                checkBox.IsChecked = true;
                checkBox.IsEnabled = false;
            }
            else
            {
                checkBox.IsEnabled = true;
            }
            DriveInfo[] drives = DriveInfo.GetDrives();
            foreach (DriveInfo d in drives)
            {
                if (d.Name.Substring(0, 1).ToString() == comboBox.SelectedItem.ToString())
                {
                    spaceleft = d.AvailableFreeSpace;
                }
            }

            spaceleft_mb = spaceleft / (1024 * 1024);
            if (spaceleft_mb < 1024 * 1024)
            {
                Unit.Items.Remove("TB");
            }
            if (spaceleft_mb < minimalSpaceRequired)
            {
                string messageBoxText = "Error : you don't have enough free space on this partition, please make space and try again";
                string caption = "Requirement error";
                MessageBoxButton button = MessageBoxButton.OK;
                MessageBoxImage icon = MessageBoxImage.Error;
                _ = MessageBox.Show(messageBoxText, caption, button, icon);
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
