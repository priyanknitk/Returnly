using System.Windows;
using System.Windows.Controls;

namespace Returnly
{
    public partial class LandingPage : Page
    {
        public LandingPage()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error in LandingPage constructor: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GetStarted_Click(object sender, RoutedEventArgs e)
        {
            // Navigate to Form16UploadPage
            try
            {
                if (NavigationService != null)
                {
                    NavigationService.Navigate(new Form16UploadPage());
                }
                else
                {
                    MessageBox.Show("Navigation service not available", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error navigating to Form16UploadPage: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void LearnMore_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Show information about the app
            var messageBox = new Wpf.Ui.Controls.MessageBox
            {
                Title = "About Returnly",
                Content = "Returnly is your intelligent tax filing assistant designed to make tax season stress-free."
            };
            messageBox.ShowDialogAsync();
        }
    }
}