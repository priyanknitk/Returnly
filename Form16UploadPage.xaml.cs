using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.Threading.Tasks;
using System.Diagnostics;
using Returnly.Services;
using Returnly.Handlers;
using Returnly.Dialogs;

namespace Returnly
{
    public partial class Form16UploadPage : Page
    {
        private string _selectedFilePath = string.Empty;
        private readonly FileUploadService _fileUploadService;
        private readonly NotificationService _notificationService;
        private readonly DragDropHandler _dragDropHandler;
        private readonly Form16ProcessingService _form16ProcessingService;
        private Form16Data? _processedData;
        private string? _pdfPassword;

        public Form16UploadPage()
        {
            InitializeComponent();
            
            // Initialize services
            _fileUploadService = new FileUploadService();
            _notificationService = new NotificationService(NotificationPanel, NotificationTextBlock);
            _dragDropHandler = new DragDropHandler(UploadBorder, ProcessSelectedFile);
            _form16ProcessingService = new Form16ProcessingService();
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

        private async void ProcessSelectedFile(string filePath)
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
                _pdfPassword = null; // Reset password when new file is selected

                // Check if PDF is password protected
                var isPasswordProtected = await _form16ProcessingService.IsPasswordProtectedAsync(filePath);
                
                var fileName = Path.GetFileName(filePath);
                var fileInfo = _fileUploadService.GetFileInfo(filePath);
                var fileSizeText = _fileUploadService.FormatFileSize(fileInfo.Length);

                UpdateUI(fileName, fileSizeText, fileInfo, isPasswordProtected);
                
                var statusMessage = isPasswordProtected 
                    ? $"Password-protected PDF '{fileName}' selected. Password will be requested during processing."
                    : $"PDF file '{fileName}' selected successfully!";
                
                _notificationService.ShowNotification(statusMessage, NotificationType.Success);
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification($"Error processing file: {ex.Message}", NotificationType.Error);
            }
        }

        private void UpdateUI(string fileName, string fileSizeText, FileInfo fileInfo, bool isPasswordProtected)
        {
            var statusIcon = isPasswordProtected ? "🔒" : "✓";
            FilePathTextBlock.Text = $"{statusIcon} {fileName} ({fileSizeText})";
            FilePathTextBlock.Foreground = System.Windows.Media.Brushes.Green;
            ProcessButton.IsEnabled = true;
            ProcessButton.Content = "Process Form 16";

            // Show PDF info
            PreviewImage.Visibility = Visibility.Collapsed;
            var protectionStatus = isPasswordProtected ? "Password Protected" : "Not Protected";
            PreviewTextBlock.Text = $"PDF Document Ready for Processing\n" +
                                  $"File size: {fileSizeText}\n" +
                                  $"Created: {fileInfo.CreationTime:MMM dd, yyyy}\n" +
                                  $"Protection: {protectionStatus}";
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
                _processedData = await ProcessForm16Document(_selectedFilePath);
                
                if (_processedData.IsValid)
                {
                    ShowProcessingResults(_processedData);
                    _notificationService.ShowNotification("Form 16 PDF processed successfully!", NotificationType.Success);
                }
                else
                {
                    _notificationService.ShowNotification("Could not extract valid data from the PDF. Please ensure it's a valid Form 16.", NotificationType.Warning);
                }
            }
            catch (UnauthorizedAccessException)
            {
                _notificationService.ShowNotification("Processing cancelled or invalid password provided.", NotificationType.Warning);
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification($"Error processing Form 16 PDF: {ex.Message}", NotificationType.Error);
                Debug.WriteLine($"Processing error: {ex}");
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

        private async Task<Form16Data> ProcessForm16Document(string filePath)
        {
            Debug.WriteLine($"Starting to process PDF file: {Path.GetFileName(filePath)}");
            
            // Check if password is needed
            var isPasswordProtected = await _form16ProcessingService.IsPasswordProtectedAsync(filePath);
            
            if (isPasswordProtected && string.IsNullOrEmpty(_pdfPassword))
            {
                _pdfPassword = await GetPasswordFromUser();
                if (string.IsNullOrEmpty(_pdfPassword))
                {
                    throw new UnauthorizedAccessException("Password required to process this PDF.");
                }
            }

            // Validate password if provided
            if (isPasswordProtected && !string.IsNullOrEmpty(_pdfPassword))
            {
                var isValidPassword = await _form16ProcessingService.ValidatePasswordAsync(filePath, _pdfPassword);
                if (!isValidPassword)
                {
                    _pdfPassword = null; // Reset invalid password
                    throw new UnauthorizedAccessException("Invalid password provided.");
                }
            }

            // Extract text and parse data from PDF
            var form16Data = await _form16ProcessingService.ProcessForm16Async(filePath, _pdfPassword);
            
            Debug.WriteLine($"Processing completed. Employee: {form16Data.EmployeeName}, PAN: {form16Data.PAN}");
            
            return form16Data;
        }

        private async Task<string?> GetPasswordFromUser()
        {
            return await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var passwordDialog = new PasswordInputDialog
                {
                    Owner = Window.GetWindow(this)
                };

                var result = passwordDialog.ShowDialog();
                return result == true && passwordDialog.IsPasswordProvided ? passwordDialog.Password : null;
            });
        }

        private void ShowProcessingResults(Form16Data data)
        {
            ResultsPanel.Visibility = Visibility.Visible;
            
            ExtractedDataTextBlock.Text = $@"Extracted Information from PDF:
• Employee Name: {data.EmployeeName}
• PAN: {data.PAN}
• Assessment Year: {data.AssessmentYear}
• Financial Year: {data.FinancialYear}
• Employer: {data.EmployerName}
• TAN: {data.TAN}
• Gross Salary: ₹{data.GrossSalary:N2}
• Total Tax Deducted: ₹{data.TotalTaxDeducted:N2}
• Standard Deduction: ₹{data.StandardDeduction:N2}
• Professional Tax: ₹{data.ProfessionalTax:N2}
• HRA Exemption: ₹{data.HRAExemption:N2}";
        }

        private void BackToHome_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new LandingPage());
        }

        private void ContinueToTaxForms_Click(object sender, RoutedEventArgs e)
        {
            if (_processedData != null && _processedData.IsValid)
            {
                try
                {
                    // Navigate to the TaxDataInputPage with the processed data
                    if (NavigationService != null)
                    {
                        NavigationService.Navigate(new TaxDataInputPage(_processedData));
                    }
                    else
                    {
                        MessageBox.Show("Navigation service not available", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    _notificationService.ShowNotification($"Error navigating to tax data input: {ex.Message}", NotificationType.Error);
                }
            }
            else
            {
                _notificationService.ShowNotification("Please process a Form 16 first.", NotificationType.Warning);
            }
        }

        private void ClearFile_Click(object sender, RoutedEventArgs e)
        {
            _selectedFilePath = string.Empty;
            _processedData = null;
            _pdfPassword = null; // Clear stored password
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