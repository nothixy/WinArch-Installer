using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace WinArch {
	/// <summary>
	/// Interaction logic for Desktop.xaml
	/// </summary>
	public partial class Desktop : Page {
		public Desktop() {
			InitializeComponent();
		}

		// Navigate back
		public void Previous(object sender, EventArgs e) {
			_ = NavigationService.Navigate(new Uri("User.xaml", UriKind.Relative));
		}

		// Navigate forward
		public void Next(object sender, EventArgs e) {
			RadioButton btnchecked = GridTop.Children.OfType<RadioButton>().FirstOrDefault(r => r.IsChecked.Value);
			Application.Current.Properties["Desktop"] = btnchecked.Name;
			_ = NavigationService.Navigate(new Uri("Slideshow.xaml", UriKind.Relative));
		}
	}
}
