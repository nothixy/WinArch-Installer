using System;
using System.IO;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows;

namespace WinArch {
	/// <summary>
	/// Interaction logic for Welcome.xaml
	/// </summary>
	public partial class Welcome : Page {
		public Welcome() {
			// On error, write to log file
			Dispatcher.UnhandledException += (s, e) => {
				StreamWriter sw = File.AppendText(Path.Combine(Path.GetTempPath() + "WinArch.txt"));
				sw.WriteLine(e.Exception.ToString());
			};
			InitializeComponent();
			// Do it once more (it doesn't work if InitializeComponent() was not successful)
			Dispatcher.UnhandledException += (s, e) => {
				StreamWriter sw = File.AppendText(Path.Combine(Path.GetTempPath() + "WinArch.txt"));
				sw.WriteLine(e.Exception.ToString());
			};
		}

		// Navigate forward
		public void Next(object sender, EventArgs e) {
			_ = NavigationService.Navigate(new Uri("About.xaml", UriKind.Relative));
		}

		private void Exit(object sender, RoutedEventArgs e) {
			Application.Current.Shutdown();
		}
	}
}
