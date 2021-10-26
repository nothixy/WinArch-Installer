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
using System.Windows.Controls;
using System.Windows.Navigation;

namespace WinArch
{
    /// <summary>
    /// Interaction logic for Welcome.xaml
    /// </summary>
    public partial class Welcome : Page
    {
        public Welcome()
        {
            this.Dispatcher.UnhandledException += (s, e) =>
            {
                using (StreamWriter sw = File.AppendText(Path.Combine(Path.GetTempPath() + "WinArch.txt")))
                    sw.WriteLine(e.Exception.ToString());
            };
            InitializeComponent();
            this.Dispatcher.UnhandledException += (s, e) =>
            {
                using (StreamWriter sw = File.AppendText(Path.Combine(Path.GetTempPath() + "WinArch.txt")))
                    sw.WriteLine(e.Exception.ToString());
            };
        }
        public void Next(object sender, EventArgs e)
        {
            NavigationService nav = this.NavigationService;
            nav.Navigate(new Uri("About.xaml", UriKind.Relative));
            //nav.Navigate(new Uri("Slideshow.xaml", UriKind.Relative));
        }
    }
}
