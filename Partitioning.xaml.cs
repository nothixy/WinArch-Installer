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
        float spaceleft;
        float spaceleft_mb;
        bool canInstall = false;
        string biosmode;
        int minimalSpaceRequired = 2500;
        bool returncode;
        public Partitioning()
        {
            InitializeComponent();
            Mouse.OverrideCursor = Cursors.Wait;
            Task.Run(() => getBIOSMode());
        }
        void IsDiskInstallable(string partname)
        {
            Process process = new Process();
            process.StartInfo.FileName = "powershell.exe";
            if (biosmode == "BIOS")
            {
                process.StartInfo.Arguments = "if ((Get-partition -DiskNumber ((Get-Partition -DriveLetter " + partname + ").DiskNumber)).PartitionNumber -contains 0) { ((Get-Partition -DriveLetter " + partname + ").Offset -ge (Get-Partition -PartitionNumber 0).Offset -and (Get-Partition -DriveLetter " + partname + ").Offset -lt ((Get-Partition -PartitionNumber 0).Offset + (Get-Partition -PartitionNumber 0).Size)) } else { if (((Get-partition -DiskNumber ((Get-Partition -DriveLetter C).DiskNumber)).PartitionNumber | Measure-Object -line).Lines -gt 3) { echo False } else { echo True }}";
            }
            else
            {
                process.StartInfo.Arguments = "-Command (get-partition (get-partition -DriveLetter " + partname + ").DiskNumber | where-object {$_.GptType -eq \"{C12A7328-F81F-11D2-BA4B-00A0C93EC93B}\"}) | set-partition -newdriveletter Z; if ((get-wmiobject win32_logicaldisk | where-object {$_.DeviceID -eq \"Z:\"}).FreeSpace -gt 2000) { echo True } else { echo False }; Get-PSDrive Z | Remove-PSDrive";
            }
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.EnableRaisingEvents = true;
            process.Start();
            process.WaitForExit();
            Debug.WriteLine(process.StandardOutput.ReadToEnd());
            /*returncode = Boolean.Parse(Regex.Replace(process.StandardOutput.ReadToEnd().ToLower(), "\\s", ""));
            if (returncode)
            {
                this.Dispatcher.Invoke(() =>
                {
                    comboBox.Items.Add(partname);
                });
                canInstall = true;
            }*/
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
                        Debug.WriteLine(d.Name);
                        IsDiskInstallable(d.Name.Substring(0, 1));
                    }
                }
            }
            this.Dispatcher.Invoke(() =>
            {
                Mouse.OverrideCursor = Cursors.Arrow;
                textBlock1.Visibility = Visibility.Hidden;
                page.IsEnabled = true;
            });
            this.Dispatcher.Invoke(() =>
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
            NavigationService nav = this.NavigationService;
            nav.Navigate(new Uri("About.xaml", UriKind.Relative));
        }
        public void Next(object sender, EventArgs e)
        {
            if (float.Parse(TextBoxSize.Text) < minimalSpaceRequired / Math.Pow(1024, Unit.SelectedIndex))
            {
                TextBoxSize.Text = Math.Round((minimalSpaceRequired / Math.Pow(1024, Unit.SelectedIndex)), 0).ToString();
            }
            if (float.Parse(TextBoxSize.Text) > spaceleft_mb / Math.Pow(1024, Unit.SelectedIndex))
            {
                TextBoxSize.Text = Math.Round((spaceleft_mb / Math.Pow(1024, Unit.SelectedIndex)), 0).ToString();
            }
            float spaceneeded = (float)(float.Parse(TextBoxSize.Text) * Math.Pow(1024, Unit.SelectedIndex));
            if ((bool)checkBox.IsChecked)
            {
                Application.Current.Properties["SpaceRequired"] = spaceneeded;
            }
            Application.Current.Properties["Repartition"] = checkBox.IsChecked;
            Application.Current.Properties["Volume"] = comboBox.SelectedItem;
            NavigationService nav = this.NavigationService;
            nav.Navigate(new Uri("Locale.xaml", UriKind.Relative));
        }

        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
                MessageBox.Show(messageBoxText, caption, button, icon);
            }
            SizeSlider.Maximum = spaceleft_mb;
        }
        public void getBIOSMode()
        {
            Process process = new Process();
            process.Exited += (s, e) =>
            {
                string output = Regex.Replace(process.StandardOutput.ReadToEnd(), "\\s", "").ToUpper();
                Application.Current.Properties["biosmode"] = output;
                biosmode = output;
                MainFunction();
            };
            process.StartInfo.FileName = "powershell.exe";
            process.StartInfo.Arguments = "-Command echo $(Get-ComputerInfo).BiosFirmwareType";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.EnableRaisingEvents = true;
            process.Start();
        }

        private void checkBox_Checked(object sender, RoutedEventArgs e)
        {
            SizeSlider.IsEnabled = true;
            Unit.IsEnabled = true;
            TextBoxSize.IsEnabled = true;
        }

        private void checkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            SizeSlider.IsEnabled = false;
            Unit.IsEnabled = false;
            TextBoxSize.IsEnabled = false;
        }
    }
}
