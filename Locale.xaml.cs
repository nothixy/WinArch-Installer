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

            string langline;
            StreamReader langfile = new(GetType().Assembly.GetManifestResourceStream("WinArch.Resources.langs.txt"));
            while ((langline = langfile.ReadLine()) != null)
            {
                _ = lang.Items.Add(langline);
            }
            langfile.Close();
            lang.SelectedItem = "en_US.UTF-8 UTF-8";

            string keymapline;
            StreamReader keymapfile = new(GetType().Assembly.GetManifestResourceStream("WinArch.Resources.keymaps.txt"));
            while ((keymapline = keymapfile.ReadLine()) != null)
            {
                _ = keymap.Items.Add(keymapline);
            }
            keymapfile.Close();
            keymap.SelectedItem = "us";

            string tzline;
            StreamReader tzfile = new(GetType().Assembly.GetManifestResourceStream("WinArch.Resources.timezones.txt"));
            while ((tzline = tzfile.ReadLine()) != null)
            {
                _ = timezone.Items.Add(tzline);
            }
            tzfile.Close();
            timezone.SelectedItem = "UTC";
        }
        public void Previous(object sender, EventArgs e)
        {
            _ = NavigationService.Navigate(new Uri("Partitioning.xaml", UriKind.Relative));
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
            _ = NavigationService.Navigate(new Uri("User.xaml", UriKind.Relative));
        }
    }
}
