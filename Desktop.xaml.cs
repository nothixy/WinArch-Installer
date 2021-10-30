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
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace WinArch
{
    /// <summary>
    /// Interaction logic for Desktop.xaml
    /// </summary>
    public partial class Desktop : Page
    {
        public Desktop()
        {
            InitializeComponent();
        }
        public void Previous(object sender, EventArgs e)
        {
            _ = NavigationService.Navigate(new Uri("User.xaml", UriKind.Relative));
        }
        public void Next(object sender, EventArgs e)
        {
            RadioButton btnchecked = GridTop.Children.OfType<RadioButton>().FirstOrDefault(r => r.IsChecked.Value);
            Application.Current.Properties["Desktop"] = btnchecked.Name;
            _ = NavigationService.Navigate(new Uri("Slideshow.xaml", UriKind.Relative));
        }
    }
}
