using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

            string langline;
            System.IO.StreamReader langfile =
            new System.IO.StreamReader(@"../../../Resources/langs.txt");
            while ((langline = langfile.ReadLine()) != null)
            {
                lang.Items.Add(langline);
            }
            langfile.Close();
            lang.SelectedItem = "en_US.UTF-8 UTF-8";

            string keymapline;
            System.IO.StreamReader keymapfile =
            new System.IO.StreamReader(@"../../../Resources/keymaps.txt");
            while ((keymapline = keymapfile.ReadLine()) != null)
            {
                keymap.Items.Add(keymapline);
            }
            keymapfile.Close();
            keymap.SelectedItem = "us";

            string tzline;
            System.IO.StreamReader tzfile =
            new System.IO.StreamReader(@"../../../Resources/timezones.txt");
            while ((tzline = tzfile.ReadLine()) != null)
            {
                timezone.Items.Add(tzline);
            }
            tzfile.Close();
            timezone.SelectedItem = "UTC";
        }
        public void Previous(object sender, EventArgs e)
        {
            NavigationService nav = this.NavigationService;
            nav.Navigate(new Uri("Partitionnig.xaml", UriKind.Relative));
        }
        public void Next(object sender, EventArgs e)
        {
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
            NavigationService nav = this.NavigationService;
            nav.Navigate(new Uri("User.xaml", UriKind.Relative));
        }
    }
}
