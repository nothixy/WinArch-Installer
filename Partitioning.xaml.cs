using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;

namespace WinArch {
	/// <summary>
	/// Interaction logic for Partitioning.xaml
	/// </summary>
	public partial class Partitioning : Page {
		private float spaceleft;
		private float spaceleft_mb;
		private readonly int minimalSpaceRequired = 5000;
		private bool returncode;
		public Partitioning() {
			InitializeComponent();
			Mouse.OverrideCursor = Cursors.Wait;
			GetCPUArch();
			GetBIOSMode();
		}

		public void showError(string messageText, string caption) {
			MessageBoxButton button = MessageBoxButton.OK;
			MessageBoxImage icon = MessageBoxImage.Error;
			_ = MessageBox.Show(messageText, caption, button, icon);
			Dispatcher.Invoke(() =>
			{
				Application.Current.Shutdown();
			});
		}

		// Check if running on a x86_64 cpu else exit
		public void GetCPUArch() {
			switch (typeof(string).Assembly.GetName().ProcessorArchitecture) {
				case System.Reflection.ProcessorArchitecture.Amd64:
					break;
				default:
					showError("Error : this tool is made for AMD64/X86_64 machines only", "Requirement error");
					break;
			}
		}

		// Check if system booted in UEFI mode, else exit
		public void GetBIOSMode() {
			Process process = new();
			process.StartInfo.FileName = "powershell.exe";
			process.StartInfo.Arguments = "-Command echo $(Get-ComputerInfo).BiosFirmwareType";
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.RedirectStandardError = true;
			process.StartInfo.CreateNoWindow = true;
			process.EnableRaisingEvents = true;
			process.Exited += (s, e) => {
				string output = Regex.Replace(process.StandardOutput.ReadToEnd(), "\\s", "").ToUpper(System.Globalization.CultureInfo.CurrentUICulture);
				if (output != "UEFI") {
					showError("Error : this tool is made for UEFI machines only", "Requirement error");
				}
				IsDiskInstallable();
			};
			_ = process.Start();

		}

		// Check if the EFI partition has enough space left to install, else exit
		private void IsDiskInstallable() {
			Process process = new();
			process.StartInfo.FileName = "powershell.exe";
			process.StartInfo.Arguments = "(Get-Volume | where-object {$_.Path -eq ((Get-Partition -DiskNumber 0) | where-object {$_.GptType -eq '{c12a7328-f81f-11d2-ba4b-00a0c93ec93b}'}).AccessPaths[-1]}).SizeRemaining -gt 50000000";
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.RedirectStandardError = true;
			process.StartInfo.CreateNoWindow = true;
			process.EnableRaisingEvents = true;
			process.Exited += (s, e) => {
				returncode = bool.Parse(Regex.Replace(process.StandardOutput.ReadToEnd().ToLower(System.Globalization.CultureInfo.CurrentUICulture), "\\s", ""));
				if (!returncode) {
					showError("Error : not enough space left on the ESP, please make some and try again", "Requirement error");
				}
				MainFunction();
			};
			_ = process.Start();

		}
		public void MainFunction() {
			// Get all volumes with sufficient space
			DriveInfo[] allDrives = DriveInfo.GetDrives();
			foreach (DriveInfo d in allDrives) {
				if (d.DriveType != DriveType.Fixed) {
					continue;
				}
				if (((float)d.TotalSize / (1024 * 1024)) < minimalSpaceRequired) {
					continue;
				}
				Dispatcher.Invoke(() => {
					_ = comboBox.Items.Add(d.Name[..1]);
				});
			}
			// If there are no disks, exit
			if (comboBox.Items.Count == 0) {
				showError("Error : not enough space left on any partition, please make some and try again", "Requirement error");
			};
			// The user is now able to interact with the page
			Dispatcher.Invoke(() => {
				Mouse.OverrideCursor = Cursors.Arrow;
				textBlock1.Visibility = Visibility.Hidden;
				progressBar2.Visibility = Visibility.Hidden;
				page.IsEnabled = true;
				comboBox.SelectedIndex = 0;
				checkBox.IsChecked = true;
			});
		}

		private void Unit_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			SizeSlider.Minimum = Math.Round(minimalSpaceRequired / Math.Pow(1024, Unit.SelectedIndex), 0);
			SizeSlider.Maximum = Math.Round((long)spaceleft_mb / Math.Pow(1024, Unit.SelectedIndex), 0);
			SizeSlider.Value = SizeSlider.Minimum;
		}

		// Update the text value with the slider
		private void SizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
			TextBoxSize.Text = SizeSlider.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
		}

		// Update the slider with the text value
		private void TextBoxSize_TextChanged(object sender, TextChangedEventArgs e) {
			if (Regex.IsMatch(TextBoxSize.Text, "\\d+")) {
				SizeSlider.Value = Math.Round(float.Parse(TextBoxSize.Text, System.Globalization.CultureInfo.InvariantCulture), 0);
			}
		}

		// Navigate back
		public void Previous(object sender, EventArgs e) {
			_ = NavigationService.Navigate(new Uri("About.xaml", UriKind.Relative), System.Globalization.CultureInfo.InvariantCulture);
		}

		// Navigate forward
		public void Next(object sender, EventArgs e) {
			// Check that the space asked by the user is correct
			if (float.Parse(TextBoxSize.Text, System.Globalization.CultureInfo.InvariantCulture) < minimalSpaceRequired / Math.Pow(1024, Unit.SelectedIndex)) {
				TextBoxSize.Text = Math.Round(minimalSpaceRequired / Math.Pow(1024, Unit.SelectedIndex), 0).ToString(System.Globalization.CultureInfo.InvariantCulture);
			}
			if (float.Parse(TextBoxSize.Text, System.Globalization.CultureInfo.InvariantCulture) > spaceleft_mb / Math.Pow(1024, Unit.SelectedIndex)) {
				TextBoxSize.Text = Math.Round(spaceleft_mb / Math.Pow(1024, Unit.SelectedIndex), 0).ToString(System.Globalization.CultureInfo.InvariantCulture);
			}
			float spaceneeded = (float)(float.Parse(TextBoxSize.Text, System.Globalization.CultureInfo.InvariantCulture) * Math.Pow(1024, Unit.SelectedIndex));
			Application.Current.Properties["SpaceRequired"] = spaceneeded;
			Application.Current.Properties["Repartition"] = checkBox.IsChecked;
			Application.Current.Properties["Volume"] = comboBox.SelectedItem;
			_ = NavigationService.Navigate(new Uri("Locale.xaml", UriKind.Relative), System.Globalization.CultureInfo.InvariantCulture);
		}

		// When the drive changes, change all the disk space values
		private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			DriveInfo[] drives = DriveInfo.GetDrives();
			foreach (DriveInfo d in drives) {
				if (d.Name[..1] != comboBox.SelectedItem.ToString()) {
					continue;
				}
				spaceleft = d.AvailableFreeSpace;
				if (d.Name[..1] != "C" || (d.AvailableFreeSpace / (1024 * 1024)) >= minimalSpaceRequired) {
					continue;
				};
				comboBox.Items.Remove("C");
				if (comboBox.Items.Count != 0) {
					continue;
				}
				// We don't have any volume available to install
				showError("Error : not enough space left on any partition, please make some and try again", "Requirement error");
			}
			// Do not prompt to erase the C drive
			if ((string)comboBox.SelectedItem == "C") {
				checkBox.IsChecked = true;
				checkBox.IsEnabled = false;
			} else {
				checkBox.IsEnabled = true;
			}

			// Remove `TeraBytes` in setups with less than 1TB
			spaceleft_mb = spaceleft / (1024 * 1024);
			if (spaceleft_mb < 1024 * 1024) {
				Unit.Items.Remove("TB");
			}

			// If the space in a volume is too low, do not prompt to reduce it
			if (spaceleft_mb < minimalSpaceRequired) {
				checkBox.IsChecked = false;
				checkBox.IsEnabled = false;
			}
			SizeSlider.Maximum = spaceleft_mb;
		}

		// User wants to resize a volume
		private void CheckBox_Checked(object sender, RoutedEventArgs e) {
			SizeSlider.IsEnabled = true;
			Unit.IsEnabled = true;
			TextBoxSize.IsEnabled = true;
		}

		// User wants to use a full volume
		private void CheckBox_Unchecked(object sender, RoutedEventArgs e) {
			SizeSlider.IsEnabled = false;
			Unit.IsEnabled = false;
			TextBoxSize.IsEnabled = false;
		}
	}
}
