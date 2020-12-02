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
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace WinArch
{
    /// <summary>
    /// Interaction logic for User.xaml
    /// </summary>
    public partial class User : Page
    {
        public User()
        {
            InitializeComponent();
        }
        public void Previous(object sender, EventArgs e)
        {
            NavigationService nav = this.NavigationService;
            nav.Navigate(new Uri("Locale.xaml", UriKind.Relative));
        }
        public void Next(object sender, EventArgs e)
        {
            idempty.Visibility = Visibility.Hidden;
            idregex.Visibility = Visibility.Hidden;
            passwordmatch.Visibility = Visibility.Hidden;
            if (string.IsNullOrEmpty(unamesysBox.Text))
            {
                idempty.Visibility = Visibility.Visible;
                return;
            }
            if (!Regex.IsMatch(unamesysBox.Text, "^[0-9a-z]+$"))
            {
                idregex.Visibility = Visibility.Visible;
                return;
            }
            if (pwdBox1.Password != pwdBox2.Password)
            {
                passwordmatch.Visibility = Visibility.Visible;
                return;
            }
            if (!string.IsNullOrEmpty(pwdBox1.Password))
            {
                Application.Current.Properties["Password"] = pwdBox1.Password;
            }
            if (!string.IsNullOrEmpty(unameBox.Text))
            {
                Application.Current.Properties["Uname"] = unameBox.Text;
            }
            Application.Current.Properties["UnameSys"] = unamesysBox.Text;
            NavigationService nav = this.NavigationService;
            nav.Navigate(new Uri("Desktop.xaml", UriKind.Relative));
        }
    }
}
