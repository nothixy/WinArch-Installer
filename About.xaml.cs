using System;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace WinArch
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : Page
    {
        public About()
        {
            InitializeComponent();
        }

		// Navigate back
        public void Previous(object sender, EventArgs e)
        {
            _ = NavigationService.Navigate(new Uri("Welcome.xaml", UriKind.Relative));
        }

		// Navigate forward
        public void Next(object sender, EventArgs e)
        {
            _ = NavigationService.Navigate(new Uri("Partitioning.xaml", UriKind.Relative));
        }

		// If scroll bottom reached, allow going to next page
        public void ScrollerScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ScrollViewer scrollViewer = (ScrollViewer)sender;
            if (scrollViewer.VerticalOffset == scrollViewer.ScrollableHeight)
            {
                ButtonNext.IsEnabled = true;
            }
        }
    }
}
