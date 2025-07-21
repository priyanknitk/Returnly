using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace Returnly
{
    public partial class Form16UploadPage : Page
    {
        public Form16UploadPage()
        {
            InitializeComponent();
        }

        private void UploadForm16_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Select Form 16 Document",
                Filter = "PDF Files (*.pdf)|*.pdf|Image Files (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png|All Files (*.*)|*.*",
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                FilePathTextBlock.Text = $"Selected: {System.IO.Path.GetFileName(filePath)}";
                ProcessButton.IsEnabled = true;
                
                // TODO: Handle file upload logic here
                MessageBox.Show($"File selected: {filePath}", "File Selected", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ProcessForm16_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement Form 16 processing logic
            MessageBox.Show("Processing Form 16... This feature will be implemented soon!", "Processing", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BackToHome_Click(object sender, RoutedEventArgs e)
        {
            // Navigate back to the landing page
            if (NavigationService != null)
            {
                NavigationService.Navigate(new LandingPage());
            }
        }
    }
}