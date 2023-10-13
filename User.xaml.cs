using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace WinArch {
	/// <summary>
	/// Interaction logic for User.xaml
	/// </summary>
	public partial class User : Page {
		public User() {
			InitializeComponent();
		}

		// Navigate back
		public void Previous(object sender, EventArgs e) {
			_ = NavigationService.Navigate(new Uri("Locale.xaml", UriKind.Relative));
		}

		// Navigate forward
		public void Next(object sender, EventArgs e) {
			// Reset all warnings
			idempty.Visibility = Visibility.Hidden;
			idregex.Visibility = Visibility.Hidden;
			passwordmatch.Visibility = Visibility.Hidden;
			passwordempty.Visibility = Visibility.Hidden;
			// Check that all fields are valid and display warnings if needed
			if (string.IsNullOrEmpty(unamesysBox.Text)) {
				idempty.Visibility = Visibility.Visible;
				return;
			}
			if (!Regex.IsMatch(unamesysBox.Text, "^[0-9a-z]+$")) {
				idregex.Visibility = Visibility.Visible;
				return;
			}
			if (pwdBox1.Password != pwdBox2.Password) {
				passwordmatch.Visibility = Visibility.Visible;
				return;
			}
			if (string.IsNullOrEmpty(pwdBox1.Password)) {
				passwordempty.Visibility = Visibility.Visible;
				return;
			}
			Application.Current.Properties["Password"] = pwdBox1.Password;
			Application.Current.Properties["Uname"] = unameBox.Text;
			Application.Current.Properties["UnameSys"] = unamesysBox.Text;
			_ = NavigationService.Navigate(new Uri("Desktop.xaml", UriKind.Relative));
		}
	}
}
