using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Wpf.Ui.Controls;
using Returnly.Services;
using WpfPasswordBox = System.Windows.Controls.PasswordBox;
using WpfTextBlock = System.Windows.Controls.TextBlock;
using WpfStackPanel = System.Windows.Controls.StackPanel;
using WpfBorder = System.Windows.Controls.Border;
using WpfGrid = System.Windows.Controls.Grid;

namespace Returnly.Dialogs
{
    public class FilePasswordInfo
    {
        public string FilePath { get; set; } = string.Empty;
        public string FileName => Path.GetFileName(FilePath);
        public string FileType { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool IsValidated { get; set; } = false;
        public string? ErrorMessage { get; set; }
    }

    public partial class MultiFilePasswordDialog : FluentWindow
    {
        private readonly List<FilePasswordInfo> _fileInfos;
        private readonly Form16ProcessingService _processingService;
        private readonly Dictionary<string, WpfPasswordBox> _passwordBoxes;
        private readonly Dictionary<string, WpfTextBlock> _errorMessages;
        private readonly Dictionary<string, WpfTextBlock> _statusIcons;

        public Dictionary<string, string> FilePasswords { get; private set; } = new();
        public bool AllPasswordsValidated { get; private set; } = false;

        public MultiFilePasswordDialog(Dictionary<string, string> fileTypePairs, Form16ProcessingService processingService)
        {
            InitializeComponent();
            
            _processingService = processingService;
            _passwordBoxes = new Dictionary<string, WpfPasswordBox>();
            _errorMessages = new Dictionary<string, WpfTextBlock>();
            _statusIcons = new Dictionary<string, WpfTextBlock>();
            
            _fileInfos = fileTypePairs.Select(kvp => new FilePasswordInfo 
            { 
                FilePath = kvp.Key, 
                FileType = kvp.Value 
            }).ToList();

            SetupPasswordInputs();
            UpdateSubtitle();
        }

        private void SetupPasswordInputs()
        {
            PasswordInputsPanel.Children.Clear();

            foreach (var fileInfo in _fileInfos)
            {
                var container = new WpfBorder
                {
                    Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(25, 0, 0, 0)),
                    CornerRadius = new CornerRadius(8),
                    Padding = new Thickness(15),
                    Margin = new Thickness(0, 0, 0, 10)
                };

                var grid = new WpfGrid();
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                // File info header
                var headerPanel = new WpfStackPanel { Orientation = Orientation.Horizontal };
                
                var statusIcon = new WpfTextBlock
                {
                    Text = "🔒",
                    FontSize = 16,
                    Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Orange),
                    Margin = new Thickness(0, 0, 8, 0),
                    VerticalAlignment = VerticalAlignment.Center
                };
                _statusIcons[fileInfo.FilePath] = statusIcon;

                var fileTypeLabel = new WpfTextBlock
                {
                    Text = $"{fileInfo.FileType}:",
                    FontWeight = FontWeights.SemiBold,
                    FontSize = 14,
                    Margin = new Thickness(0, 0, 8, 0)
                };

                var fileNameLabel = new WpfTextBlock
                {
                    Text = fileInfo.FileName,
                    FontSize = 12,
                    Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(200, 128, 128, 128)),
                    VerticalAlignment = VerticalAlignment.Center
                };

                headerPanel.Children.Add(statusIcon);
                headerPanel.Children.Add(fileTypeLabel);
                headerPanel.Children.Add(fileNameLabel);

                Grid.SetRow(headerPanel, 0);
                Grid.SetColumn(headerPanel, 0);
                Grid.SetColumnSpan(headerPanel, 2);

                // Password input
                var passwordLabel = new WpfTextBlock
                {
                    Text = "Password:",
                    FontSize = 12,
                    Margin = new Thickness(0, 10, 0, 5)
                };

                var passwordBox = new WpfPasswordBox
                {
                    FontSize = 14,
                    Padding = new Thickness(10, 8, 10, 8),
                    Margin = new Thickness(0, 0, 0, 5)
                };

                passwordBox.KeyDown += async (s, e) =>
                {
                    if (e.Key == Key.Enter)
                    {
                        await ValidatePasswords();
                    }
                };

                _passwordBoxes[fileInfo.FilePath] = passwordBox;

                var passwordPanel = new WpfStackPanel();
                passwordPanel.Children.Add(passwordLabel);
                passwordPanel.Children.Add(passwordBox);

                Grid.SetRow(passwordPanel, 1);
                Grid.SetColumn(passwordPanel, 0);
                Grid.SetColumnSpan(passwordPanel, 2);

                // Error message
                var errorMessage = new WpfTextBlock
                {
                    Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red),
                    FontSize = 11,
                    Visibility = Visibility.Collapsed,
                    Margin = new Thickness(0, 5, 0, 0),
                    TextWrapping = TextWrapping.Wrap
                };
                _errorMessages[fileInfo.FilePath] = errorMessage;

                Grid.SetRow(errorMessage, 2);
                Grid.SetColumn(errorMessage, 0);
                Grid.SetColumnSpan(errorMessage, 2);

                grid.Children.Add(headerPanel);
                grid.Children.Add(passwordPanel);
                grid.Children.Add(errorMessage);

                container.Child = grid;
                PasswordInputsPanel.Children.Add(container);
            }

            // Focus first password box
            if (_passwordBoxes.Count > 0)
            {
                _passwordBoxes.First().Value.Focus();
            }
        }

        private void UpdateSubtitle()
        {
            SubtitleText.Text = _fileInfos.Count == 1 
                ? "One file requires a password" 
                : $"{_fileInfos.Count} files require passwords";
        }

        private async void Validate_Click(object sender, RoutedEventArgs e)
        {
            await ValidatePasswords();
        }

        private async Task ValidatePasswords()
        {
            ValidateButton.IsEnabled = false;
            ValidateButton.Content = "Validating...";
            GlobalErrorMessage.Visibility = Visibility.Collapsed;

            try
            {
                // Clear previous errors
                foreach (var errorMsg in _errorMessages.Values)
                {
                    errorMsg.Visibility = Visibility.Collapsed;
                }

                // Collect passwords
                var passwordPairs = new Dictionary<string, string>();
                var hasEmptyPasswords = false;

                foreach (var fileInfo in _fileInfos)
                {
                    var password = _passwordBoxes[fileInfo.FilePath].Password;
                    if (string.IsNullOrEmpty(password))
                    {
                        ShowFileError(fileInfo.FilePath, "Password is required");
                        hasEmptyPasswords = true;
                    }
                    else
                    {
                        passwordPairs[fileInfo.FilePath] = password;
                    }
                }

                if (hasEmptyPasswords)
                {
                    ShowGlobalError("Please enter passwords for all files.");
                    return;
                }

                // Validate passwords
                var validationResults = await _processingService.ValidateMultiplePasswordsAsync(passwordPairs);
                
                bool allValid = true;
                foreach (var result in validationResults)
                {
                    var fileInfo = _fileInfos.First(f => f.FilePath == result.Key);
                    
                    if (result.Value)
                    {
                        fileInfo.IsValidated = true;
                        fileInfo.Password = passwordPairs[result.Key];
                        fileInfo.ErrorMessage = null;
                        
                        // Update status icon to success
                        _statusIcons[result.Key].Text = "✅";
                        _statusIcons[result.Key].Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Green);
                    }
                    else
                    {
                        fileInfo.IsValidated = false;
                        fileInfo.ErrorMessage = "Incorrect password";
                        ShowFileError(result.Key, "Incorrect password. Please try again.");
                        
                        // Clear the password box
                        _passwordBoxes[result.Key].Clear();
                        allValid = false;
                    }
                }

                if (allValid)
                {
                    // All passwords are valid
                    FilePasswords = passwordPairs;
                    AllPasswordsValidated = true;
                    OKButton.IsEnabled = true;
                    
                    ShowGlobalSuccess($"All passwords validated successfully! You can now continue.");
                }
                else
                {
                    ShowGlobalError("Some passwords are incorrect. Please correct them and try again.");
                    
                    // Focus first invalid password box
                    var firstInvalid = _fileInfos.FirstOrDefault(f => !f.IsValidated);
                    if (firstInvalid != null)
                    {
                        _passwordBoxes[firstInvalid.FilePath].Focus();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowGlobalError($"Error validating passwords: {ex.Message}");
            }
            finally
            {
                ValidateButton.IsEnabled = true;
                ValidateButton.Content = "Validate Passwords";
            }
        }

        private void ShowFileError(string filePath, string message)
        {
            var errorTextBlock = _errorMessages[filePath];
            errorTextBlock.Text = message;
            errorTextBlock.Visibility = Visibility.Visible;
        }

        private void ShowGlobalError(string message)
        {
            GlobalErrorMessage.Text = message;
            GlobalErrorMessage.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
            GlobalErrorMessage.Visibility = Visibility.Visible;
        }

        private void ShowGlobalSuccess(string message)
        {
            GlobalErrorMessage.Text = message;
            GlobalErrorMessage.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Green);
            GlobalErrorMessage.Visibility = Visibility.Visible;
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            if (!AllPasswordsValidated)
            {
                ShowGlobalError("Please validate all passwords first.");
                return;
            }

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
