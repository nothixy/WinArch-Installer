using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace WinArch
{
    /// <summary>
    /// Interaction logic for Locale.xaml
    /// </summary>
    public partial class Locale : Page
    {
        public Locale()
        {
            InitializeComponent();

			// Append all languages to a menu
            string langline;
            StreamReader langfile = new(GetType().Assembly.GetManifestResourceStream("WinArch.Resources.langs.txt"));
            while ((langline = langfile.ReadLine()) != null)
            {
                _ = lang.Items.Add(langline);
            }
            langfile.Close();
            lang.SelectedItem = "en_US.UTF-8 UTF-8";

			// Append all keymaps to a menu
            string keymapline;
            StreamReader keymapfile = new(GetType().Assembly.GetManifestResourceStream("WinArch.Resources.keymaps.txt"));
            while ((keymapline = keymapfile.ReadLine()) != null)
            {
                _ = keymap.Items.Add(keymapline);
            }
            keymapfile.Close();
            keymap.SelectedItem = "us";

			// Append all timezones to a menu
            string tzline;
            StreamReader tzfile = new(GetType().Assembly.GetManifestResourceStream("WinArch.Resources.timezones.txt"));
            while ((tzline = tzfile.ReadLine()) != null)
            {
                _ = timezone.Items.Add(tzline);
            }
            tzfile.Close();
            timezone.SelectedItem = "UTC";
        }

		// Navigate back
        public void Previous(object sender, EventArgs e)
        {
            _ = NavigationService.Navigate(new Uri("Partitioning.xaml", UriKind.Relative));
        }

		// Navigate forward
        public void Next(object sender, EventArgs e)
        {
			// Check if everything is set before navigating
            hostmissing.Visibility = Visibility.Hidden;
            hostregex.Visibility = Visibility.Hidden;
            if (string.IsNullOrEmpty(hostname.Text))
            {
                hostmissing.Visibility = Visibility.Visible;
                return;
            }
            if (!Regex.IsMatch(hostname.Text, "^[0-9a-zA-Z]+(\\-[0-9a-zA-z]+)?$"))
            {
                hostregex.Visibility = Visibility.Visible;
                return;
            }
            Application.Current.Properties["Hostname"] = hostname.Text;
            Application.Current.Properties["Language"] = lang.SelectedItem;
            Application.Current.Properties["Keymap"] = keymap.SelectedItem;
            Application.Current.Properties["Timezone"] = timezone.SelectedItem;
            _ = NavigationService.Navigate(new Uri("User.xaml", UriKind.Relative));
        }
    }
}
