using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
            Application.Current.Properties["Uname"] = unameBox.Text;
            foreach(DictionaryEntry de in Application.Current.Properties)
            {
                Debug.WriteLine(de.Key + ": " + de.Value.ToString());
            }
            NavigationService nav = this.NavigationService;
            nav.Navigate(new Uri("Desktop.xaml", UriKind.Relative));
        }
    }
}
 