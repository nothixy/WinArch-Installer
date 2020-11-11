using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
    /// Interaction logic for Partitioning.xaml
    /// </summary>
    public partial class Partitioning : Page
    {
        float spaceleft;
        float spaceleft_mb;
        public Partitioning()
        {
            InitializeComponent();
            DriveInfo[] allDrives = DriveInfo.GetDrives();

            foreach (DriveInfo d in allDrives)
            {
                if (d.Name == "C:\\")
                {
                    spaceleft = d.AvailableFreeSpace;
                    spaceleft_mb = spaceleft / (1024 * 1024);
                    if (spaceleft_mb < 1024 * 1024)
                    {
                        Unit.Items.RemoveAt(2);
                    }
                    if (spaceleft_mb < 3600)
                    {
                        string messageBoxText = "Error : you don't have enough free space on your C disk, please make space and try again";
                        string caption = "Reauirement error";
                        MessageBoxButton button = MessageBoxButton.OK;
                        MessageBoxImage icon = MessageBoxImage.Error;
                        MessageBox.Show(messageBoxText, caption, button, icon);
                        Environment.Exit(1);
                    }
                    SizeSlider.Maximum = spaceleft_mb;
                }
            }
        }

        private void Unit_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SizeSlider.Minimum = Math.Round((float)3600 / Math.Pow(1024, Unit.SelectedIndex), 1);
            SizeSlider.Maximum = Math.Round((float)spaceleft_mb / Math.Pow(1024, Unit.SelectedIndex), 1);
            SizeSlider.Value = SizeSlider.Minimum;
        }

        private void SizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            TextBoxSize.Text = SizeSlider.Value.ToString();
        }

        private void TextBoxSize_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Regex.IsMatch(TextBoxSize.Text, "\\d+(\\.\\d)?"))
            {
                SizeSlider.Value = Math.Round(float.Parse(TextBoxSize.Text), 1);
            }
        }
        public void Previous(object sender, EventArgs e)
        {
            NavigationService nav = this.NavigationService;
            nav.Navigate(new Uri("About.xaml", UriKind.Relative));
        }
        public void Next(object sender, EventArgs e)
        {
            if (float.Parse(TextBoxSize.Text) < 3600 / Math.Pow(1024, Unit.SelectedIndex))
            {
                TextBoxSize.Text = Math.Round((3600 / Math.Pow(1024, Unit.SelectedIndex)), 1).ToString();
            }
            if (float.Parse(TextBoxSize.Text) > spaceleft_mb / Math.Pow(1024, Unit.SelectedIndex))
            {
                TextBoxSize.Text = Math.Round((spaceleft_mb / Math.Pow(1024, Unit.SelectedIndex)), 1).ToString();
            }
            float spaceneeded = (float)(float.Parse(TextBoxSize.Text) * Math.Pow(1024, Unit.SelectedIndex));
            Application.Current.Properties["SpaceRequired"] = spaceneeded;
            NavigationService nav = this.NavigationService;
            nav.Navigate(new Uri("Locale.xaml", UriKind.Relative));
        }
    }
}
