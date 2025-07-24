using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using Returnly.Services;
using Returnly.Services.Interfaces;
using Returnly.Dialogs;
using Microsoft.Win32;

namespace Returnly.ViewModels
{
    public enum Form16PartType
    {
        PartA,
        PartB,
        Annexure
    }

    public class Form16FileInfo : BaseViewModel
    {
        private string _filePath = string.Empty;
        private bool _isPasswordProtected;
        private string? _password;

        public string FilePath
        {
            get => _filePath;
            set => SetProperty(ref _filePath, value);
        }

        public Form16PartType PartType { get; set; }

        public bool IsPasswordProtected
        {
            get => _isPasswordProtected;
            set => SetProperty(ref _isPasswordProtected, value);
        }

        public string? Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public string FileName => Path.GetFileName(FilePath);
        public bool IsSelected => !string.IsNullOrEmpty(FilePath);
    }

    public class StatusItem : BaseViewModel
    {
        private string _icon = string.Empty;
        private string _text = string.Empty;

        public string Icon
        {
            get => _icon;
            set => SetProperty(ref _icon, value);
        }

        public string Text
        {
            get => _text;
            set => SetProperty(ref _text, value);
        }
    }

    public class Form16UploadPageViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;
        private readonly FileUploadService _fileUploadService;
        private readonly Form16ProcessingService _form16ProcessingService;
        private Form16Data? _processedData;
        private bool _isProcessing;
        private string _processingStatusText = string.Empty;
        private string _notificationText = string.Empty;
        private Visibility _notificationVisibility = Visibility.Collapsed;
        private Brush _notificationBackground = Brushes.Transparent;
        private Visibility _resultsVisibility = Visibility.Collapsed;
        private Visibility _uploadStatusVisibility = Visibility.Collapsed;
        private string _extractedDataText = string.Empty;

        public Form16UploadPageViewModel() : this(new Services.PageNavigationService())
        {
        }

        public Form16UploadPageViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            _fileUploadService = new FileUploadService();
            _form16ProcessingService = new Form16ProcessingService();

            InitializeCommands();
            InitializeFiles();
        }

        #region Properties

        public ObservableCollection<Form16FileInfo> SelectedFiles { get; } = new();
        public ObservableCollection<StatusItem> StatusItems { get; } = new();

        public Form16FileInfo PartAFile => SelectedFiles.First(f => f.PartType == Form16PartType.PartA);
        public Form16FileInfo PartBFile => SelectedFiles.First(f => f.PartType == Form16PartType.PartB);
        public Form16FileInfo AnnexureFile => SelectedFiles.First(f => f.PartType == Form16PartType.Annexure);

        public string PartAFileText => GetFileDisplayText(PartAFile);
        public string PartBFileText => GetFileDisplayText(PartBFile);
        public string AnnexureFileText => GetFileDisplayText(AnnexureFile);

        public Visibility PartAClearButtonVisibility => PartAFile.IsSelected ? Visibility.Visible : Visibility.Collapsed;
        public Visibility PartBClearButtonVisibility => PartBFile.IsSelected ? Visibility.Visible : Visibility.Collapsed;
        public Visibility AnnexureClearButtonVisibility => AnnexureFile.IsSelected ? Visibility.Visible : Visibility.Collapsed;

        public bool IsProcessing
        {
            get => _isProcessing;
            set => SetProperty(ref _isProcessing, value);
        }

        public string ProcessingStatusText
        {
            get => _processingStatusText;
            set => SetProperty(ref _processingStatusText, value);
        }

        public string NotificationText
        {
            get => _notificationText;
            set => SetProperty(ref _notificationText, value);
        }

        public Visibility NotificationVisibility
        {
            get => _notificationVisibility;
            set => SetProperty(ref _notificationVisibility, value);
        }

        public Brush NotificationBackground
        {
            get => _notificationBackground;
            set => SetProperty(ref _notificationBackground, value);
        }

        public Visibility ResultsVisibility
        {
            get => _resultsVisibility;
            set => SetProperty(ref _resultsVisibility, value);
        }

        public Visibility UploadStatusVisibility
        {
            get => _uploadStatusVisibility;
            set => SetProperty(ref _uploadStatusVisibility, value);
        }

        public string ExtractedDataText
        {
            get => _extractedDataText;
            set => SetProperty(ref _extractedDataText, value);
        }

        public bool CanProcess => PartAFile.IsSelected && PartBFile.IsSelected && !IsProcessing;

        public string ProcessButtonText
        {
            get
            {
                if (!CanProcess) return "Process Form 16 Documents";
                
                var fileCount = SelectedFiles.Count(f => f.IsSelected);
                return $"Process Form 16 Documents ({fileCount} files)";
            }
        }

        #endregion

        #region Commands

        public ICommand BackToHomeCommand { get; private set; } = null!;
        public ICommand UploadPartACommand { get; private set; } = null!;
        public ICommand UploadPartBCommand { get; private set; } = null!;
        public ICommand UploadAnnexureCommand { get; private set; } = null!;
        public ICommand ClearPartACommand { get; private set; } = null!;
        public ICommand ClearPartBCommand { get; private set; } = null!;
        public ICommand ClearAnnexureCommand { get; private set; } = null!;
        public ICommand ClearAllFilesCommand { get; private set; } = null!;
        public ICommand ProcessForm16Command { get; private set; } = null!;
        public ICommand ContinueToTaxFormsCommand { get; private set; } = null!;
        public ICommand SkipToManualEntryCommand { get; private set; } = null!;

        #endregion

        public void SetNavigationService(NavigationService wpfNavigationService)
        {
            if (_navigationService is Services.PageNavigationService pageNavService)
            {
                pageNavService.SetNavigationService(wpfNavigationService);
            }
        }

        private void InitializeCommands()
        {
            BackToHomeCommand = new RelayCommand(ExecuteBackToHome);
            UploadPartACommand = new RelayCommand(() => ExecuteUploadFile(Form16PartType.PartA));
            UploadPartBCommand = new RelayCommand(() => ExecuteUploadFile(Form16PartType.PartB));
            UploadAnnexureCommand = new RelayCommand(() => ExecuteUploadFile(Form16PartType.Annexure));
            ClearPartACommand = new RelayCommand(() => ExecuteClearFile(Form16PartType.PartA));
            ClearPartBCommand = new RelayCommand(() => ExecuteClearFile(Form16PartType.PartB));
            ClearAnnexureCommand = new RelayCommand(() => ExecuteClearFile(Form16PartType.Annexure));
            ClearAllFilesCommand = new RelayCommand(ExecuteClearAllFiles);
            ProcessForm16Command = new RelayCommand(async () => await ExecuteProcessForm16(), () => CanProcess);
            ContinueToTaxFormsCommand = new RelayCommand(ExecuteContinueToTaxForms, () => _processedData?.IsValid == true);
            SkipToManualEntryCommand = new RelayCommand(ExecuteSkipToManualEntry);
        }

        private void InitializeFiles()
        {
            SelectedFiles.Add(new Form16FileInfo { PartType = Form16PartType.PartA });
            SelectedFiles.Add(new Form16FileInfo { PartType = Form16PartType.PartB });
            SelectedFiles.Add(new Form16FileInfo { PartType = Form16PartType.Annexure });

            // Subscribe to property changes to update UI
            foreach (var file in SelectedFiles)
            {
                file.PropertyChanged += (s, e) => UpdateUI();
            }
        }

        private void ExecuteBackToHome()
        {
            try
            {
                _navigationService.NavigateTo<LandingPage>();
            }
            catch (Exception ex)
            {
                ShowNotification($"Error navigating to home: {ex.Message}", NotificationType.Error);
            }
        }

        private void ExecuteUploadFile(Form16PartType partType)
        {
            var dialogTitle = partType switch
            {
                Form16PartType.PartA => "Select Form 16 Part A PDF Document",
                Form16PartType.PartB => "Select Form 16 Part B PDF Document",
                Form16PartType.Annexure => "Select Form 16 Part B Annexure PDF Document",
                _ => "Select PDF Document"
            };

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

        private void ExecuteClearFile(Form16PartType partType)
        {
            var fileInfo = SelectedFiles.First(f => f.PartType == partType);
            fileInfo.FilePath = string.Empty;
            fileInfo.Password = null;
            fileInfo.IsPasswordProtected = false;

            ShowNotification($"{GetPartDisplayName(partType)} file cleared successfully.", NotificationType.Info);
        }

        private void ExecuteClearAllFiles()
        {
            foreach (var file in SelectedFiles)
            {
                file.FilePath = string.Empty;
                file.Password = null;
                file.IsPasswordProtected = false;
            }

            _processedData = null;
            ResultsVisibility = Visibility.Collapsed;
            ShowNotification("All files cleared successfully.", NotificationType.Info);
        }

        private async Task ExecuteProcessForm16()
        {
            var requiredFiles = new[] { Form16PartType.PartA, Form16PartType.PartB };
            var missingFiles = requiredFiles.Where(part => !SelectedFiles.First(f => f.PartType == part).IsSelected).ToList();

            if (missingFiles.Any())
            {
                var missingFileNames = string.Join(" and ", missingFiles.Select(GetPartDisplayName));
                ShowNotification($"Please select {missingFileNames} files first.", NotificationType.Warning);
                return;
            }

            try
            {
                IsProcessing = true;
                ProcessingStatusText = "Checking for password protection...";

                // Check all selected files for password protection upfront
                var selectedFiles = SelectedFiles.Where(f => f.IsSelected).ToList();
                var protectedFiles = new List<Form16FileInfo>();

                foreach (var fileInfo in selectedFiles)
                {
                    fileInfo.IsPasswordProtected = await _form16ProcessingService.IsPasswordProtectedAsync(fileInfo.FilePath);
                    if (fileInfo.IsPasswordProtected && string.IsNullOrEmpty(fileInfo.Password))
                    {
                        protectedFiles.Add(fileInfo);
                    }
                }

                // If there are protected files without passwords, collect them all at once
                if (protectedFiles.Any())
                {
                    ProcessingStatusText = "Waiting for password input...";

                    var success = await CollectPasswordsForFiles(protectedFiles);
                    if (!success)
                    {
                        ShowNotification("Password input cancelled. Processing aborted.", NotificationType.Info);
                        return;
                    }
                }

                // Now process all documents
                _processedData = await ProcessForm16Documents();

                if (_processedData?.IsValid == true)
                {
                    ShowProcessingResults(_processedData);
                    ShowNotification("Form 16 documents processed successfully!", NotificationType.Success);
                }
                else
                {
                    ShowNotification("Could not extract valid data from the PDF files. Please ensure they are valid Form 16 documents.", NotificationType.Warning);
                }
            }
            catch (UnauthorizedAccessException)
            {
                ShowNotification("Processing cancelled or invalid password provided.", NotificationType.Warning);
            }
            catch (Exception ex)
            {
                ShowNotification($"Error processing Form 16 documents: {ex.Message}", NotificationType.Error);
            }
            finally
            {
                IsProcessing = false;
                ProcessingStatusText = "";
            }
        }

        private void ExecuteContinueToTaxForms()
        {
            if (_processedData != null && _processedData.IsValid)
            {
                try
                {
                    _navigationService.NavigateTo(new TaxDataInputPage(_processedData));
                }
                catch (Exception ex)
                {
                    ShowNotification($"Error navigating to tax data input: {ex.Message}", NotificationType.Error);
                }
            }
            else
            {
                ShowNotification("Please process Form 16 documents first.", NotificationType.Warning);
            }
        }

        private void ExecuteSkipToManualEntry()
        {
            try
            {
                // Create empty Form16Data for manual entry
                var emptyData = new Form16Data();
                _navigationService.NavigateTo(new TaxDataInputPage(emptyData));
                ShowNotification("Proceeding to manual data entry.", NotificationType.Info);
            }
            catch (Exception ex)
            {
                ShowNotification($"Error navigating to manual entry: {ex.Message}", NotificationType.Error);
            }
        }

        public async void ProcessSelectedFile(string filePath, Form16PartType partType)
        {
            try
            {
                var validationResult = _fileUploadService.ValidateFile(filePath);
                if (!validationResult.IsValid)
                {
                    ShowNotification(validationResult.Message, NotificationType.Error);
                    return;
                }

                // Update the selected file info
                var fileInfo = SelectedFiles.First(f => f.PartType == partType);
                fileInfo.FilePath = filePath;
                fileInfo.Password = null; // Reset password when new file is selected

                // Check if PDF is password protected
                fileInfo.IsPasswordProtected = await _form16ProcessingService.IsPasswordProtectedAsync(filePath);

                var fileName = Path.GetFileName(filePath);
                var systemFileInfo = _fileUploadService.GetFileInfo(filePath);
                var fileSizeText = _fileUploadService.FormatFileSize(systemFileInfo.Length);

                var statusMessage = fileInfo.IsPasswordProtected
                    ? $"Password-protected PDF '{fileName}' selected for {GetPartDisplayName(partType)}. Password will be requested during processing."
                    : $"PDF file '{fileName}' selected successfully for {GetPartDisplayName(partType)}!";

                ShowNotification(statusMessage, NotificationType.Success);
            }
            catch (Exception ex)
            {
                ShowNotification($"Error processing file: {ex.Message}", NotificationType.Error);
            }
        }

        private async Task<Form16Data> ProcessForm16Documents()
        {
            var combinedData = new Form16Data();

            // Process Part A (required)
            if (PartAFile.IsSelected)
            {
                ProcessingStatusText = "Processing Part A (Certificate)...";
                var partAData = await ProcessSingleDocument(PartAFile);

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
            if (PartBFile.IsSelected)
            {
                ProcessingStatusText = "Processing Part B (Statement of Income)...";
                var partBData = await ProcessSingleDocument(PartBFile);

                // Copy Part B specific data
                combinedData.Form16B = partBData.Form16B;
                combinedData.GrossSalary = partBData.GrossSalary;
                combinedData.StandardDeduction = partBData.StandardDeduction;
                combinedData.ProfessionalTax = partBData.ProfessionalTax;
            }

            // Process Annexure (optional)
            if (AnnexureFile.IsSelected)
            {
                ProcessingStatusText = "Processing Annexure (Quarterly Details)...";
                var annexureData = await ProcessSingleDocument(AnnexureFile);

                // Copy Annexure specific data
                combinedData.Annexure = annexureData.Annexure;
            }

            ProcessingStatusText = "Finalizing data...";

            // Ensure backward compatibility
            combinedData.HRAExemption = 0; // Not applicable in new tax regime

            return combinedData;
        }

        private async Task<Form16Data> ProcessSingleDocument(Form16FileInfo fileInfo)
        {
            // At this point, password should already be collected and validated if needed
            if (fileInfo.IsPasswordProtected && string.IsNullOrEmpty(fileInfo.Password))
            {
                throw new UnauthorizedAccessException($"Password required but not provided for {GetPartDisplayName(fileInfo.PartType)} PDF.");
            }

            // Extract text and parse data from PDF
            var form16Data = await _form16ProcessingService.ProcessForm16Async(fileInfo.FilePath, fileInfo.Password);

            return form16Data;
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
                    ShowNotification($"Invalid password for {GetPartDisplayName(fileInfo.PartType)}.", NotificationType.Error);
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
                        Owner = Application.Current.MainWindow
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

        private async Task<string?> GetPasswordFromUser(Form16PartType partType)
        {
            return await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var fileInfo = SelectedFiles.First(f => f.PartType == partType);
                var passwordDialog = new PasswordInputDialog(fileInfo.FileName, GetPartDisplayName(partType))
                {
                    Owner = Application.Current.MainWindow
                };

                var result = passwordDialog.ShowDialog();
                return result == true && passwordDialog.IsPasswordProvided ? passwordDialog.Password : null;
            });
        }

        private void ShowProcessingResults(Form16Data data)
        {
            ResultsVisibility = Visibility.Visible;

            var uploadedFiles = SelectedFiles.Where(f => f.IsSelected).ToList();
            var filesList = string.Join(", ", uploadedFiles.Select(f => GetPartDisplayName(f.PartType)));

            ExtractedDataText = $@"Extracted Information from {uploadedFiles.Count} PDF file(s) ({filesList}):
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

        private void ShowNotification(string message, NotificationType type)
        {
            NotificationText = message;
            NotificationBackground = type switch
            {
                NotificationType.Success => new SolidColorBrush(Colors.Green),
                NotificationType.Error => new SolidColorBrush(Colors.Red),
                NotificationType.Warning => new SolidColorBrush(Colors.Orange),
                NotificationType.Info => new SolidColorBrush(Colors.Blue),
                _ => new SolidColorBrush(Colors.Gray)
            };
            NotificationVisibility = Visibility.Visible;

            // Auto-hide notification after 5 seconds
            Task.Run(async () =>
            {
                await Task.Delay(5000);
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    NotificationVisibility = Visibility.Collapsed;
                });
            });
        }

        private void UpdateUI()
        {
            OnPropertyChanged(nameof(CanProcess));
            OnPropertyChanged(nameof(ProcessButtonText));
            OnPropertyChanged(nameof(PartAFileText));
            OnPropertyChanged(nameof(PartBFileText));
            OnPropertyChanged(nameof(AnnexureFileText));
            OnPropertyChanged(nameof(PartAClearButtonVisibility));
            OnPropertyChanged(nameof(PartBClearButtonVisibility));
            OnPropertyChanged(nameof(AnnexureClearButtonVisibility));
            
            var hasAnyFiles = SelectedFiles.Any(f => f.IsSelected);
            UploadStatusVisibility = hasAnyFiles ? Visibility.Visible : Visibility.Collapsed;

            // Update status items
            UpdateStatusItems();
        }

        private void UpdateStatusItems()
        {
            StatusItems.Clear();
            
            foreach (var fileInfo in SelectedFiles.Where(f => f.IsSelected))
            {
                StatusItems.Add(new StatusItem
                {
                    Icon = fileInfo.IsPasswordProtected ? "🔒" : "✅",
                    Text = $"{GetPartDisplayName(fileInfo.PartType)}: {fileInfo.FileName}"
                });
            }
        }

        private string GetFileDisplayText(Form16FileInfo fileInfo)
        {
            if (!fileInfo.IsSelected)
            {
                return GetDefaultText(fileInfo.PartType);
            }

            var statusIcon = fileInfo.IsPasswordProtected ? "🔒" : "✓";
            var systemFileInfo = _fileUploadService.GetFileInfo(fileInfo.FilePath);
            var fileSizeText = _fileUploadService.FormatFileSize(systemFileInfo.Length);
            return $"{statusIcon} {fileInfo.FileName} ({fileSizeText})";
        }

        private string GetDefaultText(Form16PartType partType) => partType switch
        {
            Form16PartType.PartA => "No Part A file selected",
            Form16PartType.PartB => "No Part B file selected",
            Form16PartType.Annexure => "No Annexure file selected (optional)",
            _ => "No file selected"
        };

        private string GetPartDisplayName(Form16PartType partType) => partType switch
        {
            Form16PartType.PartA => "Part A",
            Form16PartType.PartB => "Part B", 
            Form16PartType.Annexure => "Annexure",
            _ => partType.ToString()
        };
    }
}
