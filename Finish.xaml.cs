using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;

namespace WinArch {
	/// <summary>
	/// Interaction logic for Finish.xaml
	/// </summary>
	public partial class Finish : Page {
		private bool secureboot;
		public Finish() {
			InitializeComponent();
			Main();
		}
		public void Main() {
			DisplaySecureBootStatus();
		}
		public void DisplaySecureBootStatus() {
			Process process = new();
			process.StartInfo.FileName = "powershell.exe";
			process.StartInfo.Arguments = "Confirm-SecureBootUEFI";
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.RedirectStandardError = true;
			process.StartInfo.CreateNoWindow = true;
			process.EnableRaisingEvents = true;
			process.Exited += (s, e) => {
				string output = process.StandardOutput.ReadToEnd().Trim();
				Debug.WriteLine("Secure Boot : " + output);
				if (output == "True") {
					secureboot = true;
					Reminder.Visibility = Visibility.Visible;
				} else {
					secureboot = false;
				}
				Dispatcher.Invoke(() => {
					ButtonRebootLater.IsEnabled = true;
					ButtonRebootNow.IsEnabled = true;
					progressBar2.Visibility = Visibility.Hidden;
					textBlock1.Visibility = Visibility.Hidden;
				});

			};
			_ = process.Start();
		}

		// Reboot in 1 minute (into firmware if secure boot is enabled)
		private void ButtonRebootNow_Click(object sender, RoutedEventArgs e) {
			Process process = new();
			process.StartInfo.FileName = "shutdown.exe";
			process.StartInfo.Arguments = secureboot ? "/r /fw /t 60" : "/r /t 60";
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.RedirectStandardError = true;
			process.StartInfo.CreateNoWindow = true;
			process.EnableRaisingEvents = true;
			process.Exited += (s, e) => {
				Application.Current.Shutdown();
			};
			_ = process.Start();
		}

		// Close the app without rebooting
		private void ButtonRebootLater_Click(object sender, RoutedEventArgs e) {
			Application.Current.Shutdown();
		}
	}
}
