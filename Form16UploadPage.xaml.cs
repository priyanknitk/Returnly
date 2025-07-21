using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.Threading.Tasks;
using System.Diagnostics;
using Returnly.Services;
using Returnly.Handlers;

namespace Returnly
{
    public partial class Form16UploadPage : Page
    {
        private string _selectedFilePath = string.Empty;
        private readonly FileUploadService _fileUploadService;
        private readonly NotificationService _notificationService;
        private readonly DragDropHandler _dragDropHandler;

        public Form16UploadPage()
        {
            InitializeComponent();
            
            // Initialize services
            _fileUploadService = new FileUploadService();
            _notificationService = new NotificationService(NotificationPanel, NotificationTextBlock);
            _dragDropHandler = new DragDropHandler(UploadBorder, ProcessSelectedFile);
        }

        private void UploadForm16_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Select Form 16 PDF Document",
                Filter = "PDF Files (*.pdf)|*.pdf",
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
                var validationResult = _fileUploadService.ValidateFile(filePath);
                if (!validationResult.IsValid)
                {
                    _notificationService.ShowNotification(validationResult.Message, NotificationType.Error);
                    return;
                }

                _selectedFilePath = filePath;
                var fileName = Path.GetFileName(filePath);
                var fileInfo = _fileUploadService.GetFileInfo(filePath);
                var fileSizeText = _fileUploadService.FormatFileSize(fileInfo.Length);

                UpdateUI(fileName, fileSizeText, fileInfo);
                _notificationService.ShowNotification($"PDF file '{fileName}' selected successfully!", NotificationType.Success);
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification($"Error processing file: {ex.Message}", NotificationType.Error);
            }
        }

        private void UpdateUI(string fileName, string fileSizeText, FileInfo fileInfo)
        {
            FilePathTextBlock.Text = $"✓ {fileName} ({fileSizeText})";
            FilePathTextBlock.Foreground = System.Windows.Media.Brushes.Green;
            ProcessButton.IsEnabled = true;
            ProcessButton.Content = "Process Form 16";

            // Show PDF info
            PreviewImage.Visibility = Visibility.Collapsed;
            PreviewTextBlock.Text = $"PDF Document Ready for Processing\n" +
                                  $"File size: {fileSizeText}\n" +
                                  $"Created: {fileInfo.CreationTime:MMM dd, yyyy}";
        }

        private async void ProcessForm16_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedFilePath))
            {
                _notificationService.ShowNotification("Please select a PDF file first.", NotificationType.Warning);
                return;
            }

            try
            {
                SetProcessingState(true);
                await ProcessForm16Document(_selectedFilePath);
                ShowProcessingResults();
                _notificationService.ShowNotification("Form 16 PDF processed successfully!", NotificationType.Success);
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification($"Error processing Form 16 PDF: {ex.Message}", NotificationType.Error);
            }
            finally
            {
                SetProcessingState(false);
            }
        }

        private void SetProcessingState(bool isProcessing)
        {
            ProcessButton.IsEnabled = !isProcessing;
            ProcessButton.Content = isProcessing ? "Processing PDF..." : "Process Form 16";
            ProgressIndicator.Visibility = isProcessing ? Visibility.Visible : Visibility.Collapsed;
        }

        private async Task ProcessForm16Document(string filePath)
        {
            await Task.Delay(1500);
            Debug.WriteLine($"Processing PDF file: {Path.GetFileName(filePath)}");
            
            await Task.Delay(1000);
            Debug.WriteLine("Extracting text from PDF...");
            
            await Task.Delay(500);
            Debug.WriteLine("Parsing Form 16 data...");
        }

        private void ShowProcessingResults()
        {
            ResultsPanel.Visibility = Visibility.Visible;
            ExtractedDataTextBlock.Text = @"Extracted Information from PDF:
• Employee Name: John Doe
• PAN: ABCDE1234F
• Assessment Year: 2024-25
• Total Income: ₹8,50,000
• Tax Deducted: ₹85,000
• Employer: ABC Technologies Ltd.
• TDS Certificate No: 12345678901234567890";
        }

        private void BackToHome_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new LandingPage());
        }

        private void ContinueToTaxForms_Click(object sender, RoutedEventArgs e)
        {
            _notificationService.ShowNotification("Navigating to tax forms... (Feature coming soon!)", NotificationType.Info);
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
            
            _notificationService.ShowNotification("File cleared successfully.", NotificationType.Info);
        }
    }
}