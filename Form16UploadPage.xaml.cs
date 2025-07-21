using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Returnly
{
    public partial class Form16UploadPage : Page
    {
        private string _selectedFilePath = string.Empty;
        private readonly string[] _supportedExtensions = { ".pdf", ".jpg", ".jpeg", ".png", ".tiff", ".tif" };
        private readonly long _maxFileSize = 10 * 1024 * 1024; // 10MB

        public Form16UploadPage()
        {
            InitializeComponent();
            SetupDragAndDrop();
        }

        private void SetupDragAndDrop()
        {
            // Enable drag and drop on the upload border
            UploadBorder.AllowDrop = true;
            UploadBorder.DragEnter += UploadBorder_DragEnter;
            UploadBorder.DragOver += UploadBorder_DragOver;
            UploadBorder.DragLeave += UploadBorder_DragLeave;
            UploadBorder.Drop += UploadBorder_Drop;
        }

        private void UploadBorder_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
                UploadBorder.Opacity = 0.8;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void UploadBorder_DragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }

        private void UploadBorder_DragLeave(object sender, DragEventArgs e)
        {
            UploadBorder.Opacity = 1.0;
        }

        private void UploadBorder_Drop(object sender, DragEventArgs e)
        {
            UploadBorder.Opacity = 1.0;
            
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0)
                {
                    ProcessSelectedFile(files[0]);
                }
            }
        }

        private void UploadForm16_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Select Form 16 Document",
                Filter = "PDF Files (*.pdf)|*.pdf|Image Files (*.jpg;*.jpeg;*.png;*.tiff;*.tif)|*.jpg;*.jpeg;*.png;*.tiff;*.tif|All Files (*.*)|*.*",
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() == true)
            {
                ProcessSelectedFile(openFileDialog.FileName);
            }
        }

        private void ProcessSelectedFile(string filePath)
        {
            try
            {
                // Validate file
                if (!ValidateFile(filePath))
                    return;

                _selectedFilePath = filePath;
                var fileName = Path.GetFileName(filePath);
                var fileSize = new FileInfo(filePath).Length;
                var fileSizeText = FormatFileSize(fileSize);

                // Update UI
                FilePathTextBlock.Text = $"✓ {fileName} ({fileSizeText})";
                FilePathTextBlock.Foreground = System.Windows.Media.Brushes.Green;
                
                ProcessButton.IsEnabled = true;
                ProcessButton.Content = "Process Form 16";
                
                // Show file preview if it's an image
                ShowFilePreview(filePath);

                // Show success message
                ShowNotification($"File '{fileName}' selected successfully!", NotificationType.Success);
            }
            catch (Exception ex)
            {
                ShowNotification($"Error processing file: {ex.Message}", NotificationType.Error);
            }
        }

        private bool ValidateFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                ShowNotification("File does not exist.", NotificationType.Error);
                return false;
            }

            var extension = Path.GetExtension(filePath).ToLower();
            if (!Array.Exists(_supportedExtensions, ext => ext == extension))
            {
                ShowNotification($"Unsupported file format. Please select: {string.Join(", ", _supportedExtensions)}", NotificationType.Error);
                return false;
            }

            var fileInfo = new FileInfo(filePath);
            if (fileInfo.Length > _maxFileSize)
            {
                ShowNotification($"File size too large. Maximum allowed: {FormatFileSize(_maxFileSize)}", NotificationType.Error);
                return false;
            }

            if (fileInfo.Length == 0)
            {
                ShowNotification("File is empty.", NotificationType.Error);
                return false;
            }

            return true;
        }

        private void ShowFilePreview(string filePath)
        {
            try
            {
                var extension = Path.GetExtension(filePath).ToLower();
                
                if (extension == ".jpg" || extension == ".jpeg" || extension == ".png" || extension == ".tiff" || extension == ".tif")
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(filePath);
                    bitmap.DecodePixelWidth = 200; // Resize for preview
                    bitmap.EndInit();
                    
                    PreviewImage.Source = bitmap;
                    PreviewImage.Visibility = Visibility.Visible;
                    PreviewTextBlock.Text = "Image Preview";
                }
                else if (extension == ".pdf")
                {
                    PreviewImage.Visibility = Visibility.Collapsed;
                    PreviewTextBlock.Text = "PDF Document Ready for Processing";
                }
            }
            catch (Exception ex)
            {
                PreviewImage.Visibility = Visibility.Collapsed;
                PreviewTextBlock.Text = $"Preview not available: {ex.Message}";
            }
        }

        private async void ProcessForm16_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedFilePath))
            {
                ShowNotification("Please select a file first.", NotificationType.Warning);
                return;
            }

            try
            {
                // Disable the button and show progress
                ProcessButton.IsEnabled = false;
                ProcessButton.Content = "Processing...";
                ProgressIndicator.Visibility = Visibility.Visible;

                // Simulate processing with actual file operations
                await ProcessForm16Document(_selectedFilePath);

                ShowNotification("Form 16 processed successfully!", NotificationType.Success);
                
                // Show results panel
                ShowProcessingResults();
            }
            catch (Exception ex)
            {
                ShowNotification($"Error processing Form 16: {ex.Message}", NotificationType.Error);
            }
            finally
            {
                ProcessButton.IsEnabled = true;
                ProcessButton.Content = "Process Form 16";
                ProgressIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private async Task ProcessForm16Document(string filePath)
        {
            // Simulate document processing
            await Task.Delay(2000); // Simulate processing time
            
            // Here you would implement actual OCR and data extraction
            // For now, we'll simulate the process
            
            var fileName = Path.GetFileName(filePath);
            var fileSize = new FileInfo(filePath).Length;
            
            // Log processing details
            Debug.WriteLine($"Processing file: {fileName}");
            Debug.WriteLine($"File size: {FormatFileSize(fileSize)}");
            Debug.WriteLine($"File type: {Path.GetExtension(filePath)}");
            
            // Simulate OCR extraction
            await Task.Delay(1000);
            
            // Simulate data validation
            await Task.Delay(500);
        }

        private void ShowProcessingResults()
        {
            ResultsPanel.Visibility = Visibility.Visible;
            
            // Simulate extracted data
            ExtractedDataTextBlock.Text = @"Extracted Information:
• Employee Name: John Doe
• PAN: ABCDE1234F
• Assessment Year: 2024-25
• Total Income: ₹8,50,000
• Tax Deducted: ₹85,000
• Employer: ABC Technologies Ltd.";
        }

        private void BackToHome_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService != null)
            {
                NavigationService.Navigate(new LandingPage());
            }
        }

        private void ContinueToTaxForms_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Navigate to tax forms page
            ShowNotification("Navigating to tax forms... (Feature coming soon!)", NotificationType.Info);
        }

        private void ClearFile_Click(object sender, RoutedEventArgs e)
        {
            _selectedFilePath = string.Empty;
            FilePathTextBlock.Text = "No file selected";
            FilePathTextBlock.Foreground = System.Windows.Media.Brushes.Gray;
            ProcessButton.IsEnabled = false;
            PreviewImage.Visibility = Visibility.Collapsed;
            PreviewImage.Source = null;
            PreviewTextBlock.Text = "";
            ResultsPanel.Visibility = Visibility.Collapsed;
            
            ShowNotification("File cleared successfully.", NotificationType.Info);
        }

        private string FormatFileSize(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB" };
            int counter = 0;
            decimal number = bytes;
            
            while (Math.Round(number / 1024) >= 1)
            {
                number /= 1024;
                counter++;
            }
            
            return $"{number:n1} {suffixes[counter]}";
        }

        private void ShowNotification(string message, NotificationType type)
        {
            NotificationTextBlock.Text = message;
            NotificationPanel.Visibility = Visibility.Visible;
            
            // Set color based on type
            switch (type)
            {
                case NotificationType.Success:
                    NotificationPanel.Background = System.Windows.Media.Brushes.LightGreen;
                    break;
                case NotificationType.Error:
                    NotificationPanel.Background = System.Windows.Media.Brushes.LightCoral;
                    break;
                case NotificationType.Warning:
                    NotificationPanel.Background = System.Windows.Media.Brushes.LightYellow;
                    break;
                case NotificationType.Info:
                    NotificationPanel.Background = System.Windows.Media.Brushes.LightBlue;
                    break;
            }

            // Auto-hide notification after 3 seconds
            var timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(3);
            timer.Tick += (s, e) =>
            {
                NotificationPanel.Visibility = Visibility.Collapsed;
                timer.Stop();
            };
            timer.Start();
        }
    }

    public enum NotificationType
    {
        Success,
        Error,
        Warning,
        Info
    }
}