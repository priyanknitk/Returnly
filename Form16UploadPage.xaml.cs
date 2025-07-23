using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.Threading.Tasks;
using System.Diagnostics;
using Returnly.Services;
using Returnly.Handlers;
using Returnly.Dialogs;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Returnly
{
    public enum Form16PartType
    {
        PartA,
        PartB,
        Annexure
    }

    public class Form16FileInfo
    {
        public string FilePath { get; set; } = string.Empty;
        public Form16PartType PartType { get; set; }
        public bool IsPasswordProtected { get; set; }
        public string? Password { get; set; }
        public string FileName => Path.GetFileName(FilePath);
        public bool IsSelected => !string.IsNullOrEmpty(FilePath);
    }

    public partial class Form16UploadPage : Page
    {
        private readonly Dictionary<Form16PartType, Form16FileInfo> _selectedFiles;
        private readonly FileUploadService _fileUploadService;
        private readonly NotificationService _notificationService;
        private readonly Form16ProcessingService _form16ProcessingService;
        private Form16Data? _processedData;

        public Form16UploadPage()
        {
            InitializeComponent();
            
            // Initialize services
            _selectedFiles = new Dictionary<Form16PartType, Form16FileInfo>
            {
                { Form16PartType.PartA, new Form16FileInfo { PartType = Form16PartType.PartA } },
                { Form16PartType.PartB, new Form16FileInfo { PartType = Form16PartType.PartB } },
                { Form16PartType.Annexure, new Form16FileInfo { PartType = Form16PartType.Annexure } }
            };
            
            _fileUploadService = new FileUploadService();
            _notificationService = new NotificationService(NotificationPanel, NotificationTextBlock);
            _form16ProcessingService = new Form16ProcessingService();
            
            // Initialize drag drop handlers for each section
            InitializeDragDropHandlers();
        }

        private void InitializeDragDropHandlers()
        {
            // Initialize drag drop handlers for each upload area
            new DragDropHandler(PartAUploadBorder, filePath => ProcessSelectedFile(filePath, Form16PartType.PartA));
            new DragDropHandler(PartBUploadBorder, filePath => ProcessSelectedFile(filePath, Form16PartType.PartB));
            new DragDropHandler(AnnexureUploadBorder, filePath => ProcessSelectedFile(filePath, Form16PartType.Annexure));
        }

        #region File Upload Event Handlers

        private void UploadPartA_Click(object sender, RoutedEventArgs e)
        {
            SelectFile(Form16PartType.PartA, "Select Form 16 Part A PDF Document");
        }

        private void UploadPartB_Click(object sender, RoutedEventArgs e)
        {
            SelectFile(Form16PartType.PartB, "Select Form 16 Part B PDF Document");
        }

        private void UploadAnnexure_Click(object sender, RoutedEventArgs e)
        {
            SelectFile(Form16PartType.Annexure, "Select Form 16 Part B Annexure PDF Document");
        }

        private void SelectFile(Form16PartType partType, string dialogTitle)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = dialogTitle,
                Filter = "PDF Files (*.pdf)|*.pdf",
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() == true)
            {
                ProcessSelectedFile(openFileDialog.FileName, partType);
            }
        }

        private async void ProcessSelectedFile(string filePath, Form16PartType partType)
        {
            try
            {
                var validationResult = _fileUploadService.ValidateFile(filePath);
                if (!validationResult.IsValid)
                {
                    _notificationService.ShowNotification(validationResult.Message, NotificationType.Error);
                    return;
                }

                // Update the selected file info
                var fileInfo = _selectedFiles[partType];
                fileInfo.FilePath = filePath;
                fileInfo.Password = null; // Reset password when new file is selected

                // Check if PDF is password protected
                fileInfo.IsPasswordProtected = await _form16ProcessingService.IsPasswordProtectedAsync(filePath);
                
                var fileName = Path.GetFileName(filePath);
                var systemFileInfo = _fileUploadService.GetFileInfo(filePath);
                var fileSizeText = _fileUploadService.FormatFileSize(systemFileInfo.Length);

                UpdateFileUI(partType, fileName, fileSizeText, fileInfo.IsPasswordProtected);
                UpdateUploadStatusPanel();
                UpdateProcessButtonState();
                
                var statusMessage = fileInfo.IsPasswordProtected 
                    ? $"Password-protected PDF '{fileName}' selected for {GetPartDisplayName(partType)}. Password will be requested during processing."
                    : $"PDF file '{fileName}' selected successfully for {GetPartDisplayName(partType)}!";
                
                _notificationService.ShowNotification(statusMessage, NotificationType.Success);
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification($"Error processing file: {ex.Message}", NotificationType.Error);
            }
        }

        #endregion

        #region Clear File Event Handlers

        private void ClearPartA_Click(object sender, RoutedEventArgs e)
        {
            ClearFile(Form16PartType.PartA);
        }

        private void ClearPartB_Click(object sender, RoutedEventArgs e)
        {
            ClearFile(Form16PartType.PartB);
        }

        private void ClearAnnexure_Click(object sender, RoutedEventArgs e)
        {
            ClearFile(Form16PartType.Annexure);
        }

        private void ClearAllFiles_Click(object sender, RoutedEventArgs e)
        {
            ClearFile(Form16PartType.PartA);
            ClearFile(Form16PartType.PartB);
            ClearFile(Form16PartType.Annexure);
            
            _processedData = null;
            UpdateUploadStatusPanel();
            UpdateProcessButtonState();
            
            _notificationService.ShowNotification("All files cleared successfully.", NotificationType.Info);
        }

        private void ClearFile(Form16PartType partType)
        {
            var fileInfo = _selectedFiles[partType];
            fileInfo.FilePath = string.Empty;
            fileInfo.Password = null;
            fileInfo.IsPasswordProtected = false;
            
            UpdateFileUI(partType, string.Empty, string.Empty, false);
            UpdateUploadStatusPanel();
            UpdateProcessButtonState();
            
            _notificationService.ShowNotification($"{GetPartDisplayName(partType)} file cleared successfully.", NotificationType.Info);
        }

        #endregion

        #region UI Update Methods

        private void UpdateFileUI(Form16PartType partType, string fileName, string fileSizeText, bool isPasswordProtected)
        {
            var statusIcon = isPasswordProtected ? "🔒" : "✓";
            var displayText = string.IsNullOrEmpty(fileName) 
                ? GetDefaultText(partType) 
                : $"{statusIcon} {fileName} ({fileSizeText})";
            
            var foregroundBrush = string.IsNullOrEmpty(fileName) 
                ? System.Windows.Media.Brushes.Gray 
                : System.Windows.Media.Brushes.Green;

            switch (partType)
            {
                case Form16PartType.PartA:
                    PartAFilePathTextBlock.Text = displayText;
                    PartAFilePathTextBlock.Foreground = foregroundBrush;
                    ClearPartAButton.Visibility = string.IsNullOrEmpty(fileName) ? Visibility.Collapsed : Visibility.Visible;
                    break;
                case Form16PartType.PartB:
                    PartBFilePathTextBlock.Text = displayText;
                    PartBFilePathTextBlock.Foreground = foregroundBrush;
                    ClearPartBButton.Visibility = string.IsNullOrEmpty(fileName) ? Visibility.Collapsed : Visibility.Visible;
                    break;
                case Form16PartType.Annexure:
                    AnnexureFilePathTextBlock.Text = displayText;
                    AnnexureFilePathTextBlock.Foreground = foregroundBrush;
                    ClearAnnexureButton.Visibility = string.IsNullOrEmpty(fileName) ? Visibility.Collapsed : Visibility.Visible;
                    break;
            }
        }

        private void UpdateUploadStatusPanel()
        {
            StatusItemsPanel.Children.Clear();
            
            var hasAnyFiles = _selectedFiles.Values.Any(f => f.IsSelected);
            
            if (hasAnyFiles)
            {
                UploadStatusPanel.Visibility = Visibility.Visible;
                
                foreach (var fileInfo in _selectedFiles.Values.Where(f => f.IsSelected))
                {
                    var statusItem = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 2, 0, 2) };
                    
                    var icon = new TextBlock 
                    { 
                        Text = fileInfo.IsPasswordProtected ? "🔒" : "✅", 
                        Margin = new Thickness(0, 0, 8, 0) 
                    };
                    
                    var text = new TextBlock 
                    { 
                        Text = $"{GetPartDisplayName(fileInfo.PartType)}: {fileInfo.FileName}",
                        FontSize = 12
                    };
                    
                    statusItem.Children.Add(icon);
                    statusItem.Children.Add(text);
                    StatusItemsPanel.Children.Add(statusItem);
                }
            }
            else
            {
                UploadStatusPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void UpdateProcessButtonState()
        {
            // Require at least Part A and Part B
            var hasPartA = _selectedFiles[Form16PartType.PartA].IsSelected;
            var hasPartB = _selectedFiles[Form16PartType.PartB].IsSelected;
            
            ProcessButton.IsEnabled = hasPartA && hasPartB;
            
            if (hasPartA && hasPartB)
            {
                var hasAnnexure = _selectedFiles[Form16PartType.Annexure].IsSelected;
                ProcessButton.Content = hasAnnexure 
                    ? "Process Form 16 Documents (3 files)" 
                    : "Process Form 16 Documents (2 files)";
            }
            else
            {
                ProcessButton.Content = "Process Form 16 Documents";
            }
        }

        private string GetPartDisplayName(Form16PartType partType) => partType switch
        {
            Form16PartType.PartA => "Part A",
            Form16PartType.PartB => "Part B",
            Form16PartType.Annexure => "Annexure",
            _ => partType.ToString()
        };

        private string GetDefaultText(Form16PartType partType) => partType switch
        {
            Form16PartType.PartA => "No Part A file selected",
            Form16PartType.PartB => "No Part B file selected", 
            Form16PartType.Annexure => "No Annexure file selected (optional)",
            _ => "No file selected"
        };

        #endregion

        #region Processing Methods

        private async void ProcessForm16_Click(object sender, RoutedEventArgs e)
        {
            var requiredFiles = new[] { Form16PartType.PartA, Form16PartType.PartB };
            var missingFiles = requiredFiles.Where(part => !_selectedFiles[part].IsSelected).ToList();
            
            if (missingFiles.Any())
            {
                var missingFileNames = string.Join(" and ", missingFiles.Select(GetPartDisplayName));
                _notificationService.ShowNotification($"Please select {missingFileNames} files first.", NotificationType.Warning);
                return;
            }

            try
            {
                SetProcessingState(true);
                ProcessingStatusTextBlock.Text = "Checking for password protection...";

                // Check all selected files for password protection upfront
                var selectedFiles = _selectedFiles.Values.Where(f => f.IsSelected).ToList();
                var protectedFiles = new List<Form16FileInfo>();

                foreach (var fileInfo in selectedFiles)
                {
                    if (fileInfo.IsPasswordProtected && string.IsNullOrEmpty(fileInfo.Password))
                    {
                        protectedFiles.Add(fileInfo);
                    }
                }

                // If there are protected files without passwords, collect them all at once
                if (protectedFiles.Any())
                {
                    ProcessingStatusTextBlock.Text = "Waiting for password input...";
                    
                    var success = await CollectPasswordsForFiles(protectedFiles);
                    if (!success)
                    {
                        _notificationService.ShowNotification("Password input cancelled. Processing aborted.", NotificationType.Info);
                        return;
                    }
                }

                // Now process all documents
                _processedData = await ProcessForm16Documents();
                
                if (_processedData?.IsValid == true)
                {
                    ShowProcessingResults(_processedData);
                    _notificationService.ShowNotification("Form 16 documents processed successfully!", NotificationType.Success);
                }
                else
                {
                    _notificationService.ShowNotification("Could not extract valid data from the PDF files. Please ensure they are valid Form 16 documents.", NotificationType.Warning);
                }
            }
            catch (UnauthorizedAccessException)
            {
                _notificationService.ShowNotification("Processing cancelled or invalid password provided.", NotificationType.Warning);
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification($"Error processing Form 16 documents: {ex.Message}", NotificationType.Error);
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
            ProgressIndicator.Visibility = isProcessing ? Visibility.Visible : Visibility.Collapsed;
            ProcessingStatusTextBlock.Text = isProcessing ? "Processing documents..." : "";
            
            if (!isProcessing)
            {
                UpdateProcessButtonState(); // Restore proper button state
            }
        }

        private async Task<Form16Data> ProcessForm16Documents()
        {
            var combinedData = new Form16Data();
            
            // Process Part A (required)
            if (_selectedFiles[Form16PartType.PartA].IsSelected)
            {
                ProcessingStatusTextBlock.Text = "Processing Part A (Certificate)...";
                var partAData = await ProcessSingleDocument(_selectedFiles[Form16PartType.PartA]);
                
                // Copy Part A specific data
                combinedData.Form16A = partAData.Form16A;
                combinedData.EmployeeName = partAData.EmployeeName;
                combinedData.PAN = partAData.PAN;
                combinedData.AssessmentYear = partAData.AssessmentYear;
                combinedData.FinancialYear = partAData.FinancialYear;
                combinedData.EmployerName = partAData.EmployerName;
                combinedData.TAN = partAData.TAN;
                combinedData.TotalTaxDeducted = partAData.TotalTaxDeducted;
            }
            
            // Process Part B (required)
            if (_selectedFiles[Form16PartType.PartB].IsSelected)
            {
                ProcessingStatusTextBlock.Text = "Processing Part B (Statement of Income)...";
                var partBData = await ProcessSingleDocument(_selectedFiles[Form16PartType.PartB]);
                
                // Copy Part B specific data
                combinedData.Form16B = partBData.Form16B;
                combinedData.GrossSalary = partBData.GrossSalary;
                combinedData.StandardDeduction = partBData.StandardDeduction;
                combinedData.ProfessionalTax = partBData.ProfessionalTax;
            }
            
            // Process Annexure (optional)
            if (_selectedFiles[Form16PartType.Annexure].IsSelected)
            {
                ProcessingStatusTextBlock.Text = "Processing Annexure (Quarterly Details)...";
                var annexureData = await ProcessSingleDocument(_selectedFiles[Form16PartType.Annexure]);
                
                // Copy Annexure specific data
                combinedData.Annexure = annexureData.Annexure;
            }
            
            ProcessingStatusTextBlock.Text = "Finalizing data...";
            
            // Ensure backward compatibility
            combinedData.HRAExemption = 0; // Not applicable in new tax regime
            
            return combinedData;
        }

        private async Task<Form16Data> ProcessSingleDocument(Form16FileInfo fileInfo)
        {
            Debug.WriteLine($"Starting to process PDF file: {fileInfo.FileName} for {fileInfo.PartType}");
            
            // At this point, password should already be collected and validated if needed
            if (fileInfo.IsPasswordProtected && string.IsNullOrEmpty(fileInfo.Password))
            {
                throw new UnauthorizedAccessException($"Password required but not provided for {GetPartDisplayName(fileInfo.PartType)} PDF.");
            }

            // Extract text and parse data from PDF
            var form16Data = await _form16ProcessingService.ProcessForm16Async(fileInfo.FilePath, fileInfo.Password);
            
            Debug.WriteLine($"Processing completed for {fileInfo.PartType}. Employee: {form16Data.EmployeeName}, PAN: {form16Data.PAN}");
            
            return form16Data;
        }

        private async Task<string?> GetPasswordFromUser(Form16PartType partType)
        {
            return await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var fileInfo = _selectedFiles[partType];
                var passwordDialog = new PasswordInputDialog(fileInfo.FileName, GetPartDisplayName(partType))
                {
                    Owner = Window.GetWindow(this)
                };

                var result = passwordDialog.ShowDialog();
                return result == true && passwordDialog.IsPasswordProvided ? passwordDialog.Password : null;
            });
        }

        private async Task<bool> CollectPasswordsForFiles(List<Form16FileInfo> protectedFiles)
        {
            if (protectedFiles.Count == 1)
            {
                // Use single file dialog for one file
                var fileInfo = protectedFiles[0];
                var password = await GetPasswordFromUser(fileInfo.PartType);
                
                if (string.IsNullOrEmpty(password))
                {
                    return false;
                }

                // Validate the password
                var isValid = await _form16ProcessingService.ValidatePasswordAsync(fileInfo.FilePath, password);
                if (!isValid)
                {
                    _notificationService.ShowNotification($"Invalid password for {GetPartDisplayName(fileInfo.PartType)}.", NotificationType.Error);
                    return false;
                }

                fileInfo.Password = password;
                return true;
            }
            else
            {
                // Use multi-file dialog for multiple files
                var fileTypePairs = protectedFiles.ToDictionary(
                    f => f.FilePath, 
                    f => GetPartDisplayName(f.PartType)
                );

                return await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    var multiPasswordDialog = new MultiFilePasswordDialog(fileTypePairs, _form16ProcessingService)
                    {
                        Owner = Window.GetWindow(this)
                    };

                    var result = multiPasswordDialog.ShowDialog();
                    
                    if (result == true && multiPasswordDialog.AllPasswordsValidated)
                    {
                        // Apply the validated passwords to our file info objects
                        foreach (var fileInfo in protectedFiles)
                        {
                            if (multiPasswordDialog.FilePasswords.TryGetValue(fileInfo.FilePath, out var password))
                            {
                                fileInfo.Password = password;
                            }
                        }
                        return true;
                    }

                    return false;
                });
            }
        }

        private void ShowProcessingResults(Form16Data data)
        {
            ResultsPanel.Visibility = Visibility.Visible;
            
            var uploadedFiles = _selectedFiles.Values.Where(f => f.IsSelected).ToList();
            var filesList = string.Join(", ", uploadedFiles.Select(f => GetPartDisplayName(f.PartType)));
            
            ExtractedDataTextBlock.Text = $@"Extracted Information from {uploadedFiles.Count} PDF file(s) ({filesList}):
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

        #endregion

        #region Navigation Methods

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
                _notificationService.ShowNotification("Please process Form 16 documents first.", NotificationType.Warning);
            }
        }

        #endregion
    }
}