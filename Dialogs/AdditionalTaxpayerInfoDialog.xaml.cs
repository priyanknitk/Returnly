using System;
using System.Windows;
using System.Windows.Controls;
using Returnly.Services;
using Returnly.Models;

namespace Returnly.Dialogs
{
    /// <summary>
    /// Dialog to collect additional taxpayer information for ITR generation
    /// </summary>
    public class AdditionalTaxpayerInfoDialog : Window
    {
        public AdditionalTaxpayerInfo AdditionalInfo { get; private set; } = new();
        public bool WasAccepted { get; private set; }

        private TextBox _dobTextBox = new();
        private TextBox _addressTextBox = new();
        private TextBox _cityTextBox = new();
        private TextBox _stateTextBox = new();
        private TextBox _pincodeTextBox = new();
        private TextBox _emailTextBox = new();
        private TextBox _mobileTextBox = new();
        private TextBox _aadhaarTextBox = new();
        private TextBox _bankAccountTextBox = new();
        private TextBox _bankIFSCTextBox = new();
        private TextBox _bankNameTextBox = new();

        public AdditionalTaxpayerInfoDialog()
        {
            InitializeWindow();
            CreateContent();
        }

        private void InitializeWindow()
        {
            this.Title = "Additional Taxpayer Information";
            this.Width = 600;
            this.Height = 700;
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            this.ResizeMode = ResizeMode.CanResize;
        }

        private void CreateContent()
        {
            var mainPanel = new StackPanel
            {
                Margin = new Thickness(20),
                Orientation = Orientation.Vertical
            };

            // Header
            var header = new TextBlock
            {
                Text = "📋 Additional Information Required",
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 15),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            mainPanel.Children.Add(header);

            var infoText = new TextBlock
            {
                Text = "Please provide the following information to complete your ITR form generation:",
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 20),
                TextWrapping = TextWrapping.Wrap,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            mainPanel.Children.Add(infoText);

            // Personal Information Section
            mainPanel.Children.Add(CreateSectionHeader("👤 Personal Information"));
            mainPanel.Children.Add(CreateInputField("Date of Birth (DD/MM/YYYY):", _dobTextBox, "01/01/1990"));
            mainPanel.Children.Add(CreateInputField("Address:", _addressTextBox, "Enter your full address"));
            
            var locationPanel = new StackPanel { Orientation = Orientation.Horizontal };
            locationPanel.Children.Add(CreateInputField("City:", _cityTextBox, "City", 180));
            locationPanel.Children.Add(CreateInputField("State:", _stateTextBox, "State", 150));
            locationPanel.Children.Add(CreateInputField("Pincode:", _pincodeTextBox, "000000", 100));
            mainPanel.Children.Add(locationPanel);

            // Contact Information Section
            mainPanel.Children.Add(CreateSectionHeader("📞 Contact Information"));
            mainPanel.Children.Add(CreateInputField("Email Address:", _emailTextBox, "email@example.com"));
            mainPanel.Children.Add(CreateInputField("Mobile Number:", _mobileTextBox, "9876543210"));
            mainPanel.Children.Add(CreateInputField("Aadhaar Number:", _aadhaarTextBox, "1234-5678-9012"));

            // Bank Details Section
            mainPanel.Children.Add(CreateSectionHeader("🏦 Bank Details (for Refunds)"));
            mainPanel.Children.Add(CreateInputField("Bank Account Number:", _bankAccountTextBox, "Account number"));
            mainPanel.Children.Add(CreateInputField("Bank IFSC Code:", _bankIFSCTextBox, "IFSC code"));
            mainPanel.Children.Add(CreateInputField("Bank Name:", _bankNameTextBox, "Bank name"));

            // Note
            var noteText = new TextBlock
            {
                Text = "💡 Note: Some fields are mandatory for e-filing. Bank details are required if you expect a refund.",
                FontSize = 12,
                Margin = new Thickness(0, 15, 0, 15),
                TextWrapping = TextWrapping.Wrap,
                FontStyle = FontStyles.Italic,
                Foreground = System.Windows.Media.Brushes.DarkBlue
            };
            mainPanel.Children.Add(noteText);

            // Buttons
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 20, 0, 0)
            };

            var proceedButton = new Button
            {
                Content = "✅ Generate ITR Form",
                Padding = new Thickness(20, 10, 20, 10),
                Margin = new Thickness(0, 0, 15, 0),
                IsDefault = true,
                Background = System.Windows.Media.Brushes.LightGreen
            };
            proceedButton.Click += ProceedButton_Click;

            var cancelButton = new Button
            {
                Content = "❌ Cancel",
                Padding = new Thickness(20, 10, 20, 10),
                IsCancel = true
            };
            cancelButton.Click += CancelButton_Click;

            buttonPanel.Children.Add(proceedButton);
            buttonPanel.Children.Add(cancelButton);
            mainPanel.Children.Add(buttonPanel);

            // Set content with scroll viewer
            this.Content = new ScrollViewer
            {
                Content = mainPanel,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };
        }

        private TextBlock CreateSectionHeader(string text)
        {
            return new TextBlock
            {
                Text = text,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 15, 0, 10),
                Foreground = System.Windows.Media.Brushes.DarkBlue
            };
        }

        private StackPanel CreateInputField(string label, TextBox textBox, string placeholder, double width = 0)
        {
            var panel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(5, 0, 5, 10)
            };

            if (width > 0)
                panel.Width = width;

            var labelBlock = new TextBlock
            {
                Text = label,
                FontSize = 12,
                FontWeight = FontWeights.Medium,
                Margin = new Thickness(0, 0, 0, 3)
            };

            textBox.Text = placeholder;
            textBox.Padding = new Thickness(5);
            textBox.FontSize = 12;
            textBox.GotFocus += (s, e) =>
            {
                if (textBox.Text == placeholder)
                {
                    textBox.Text = "";
                    textBox.Foreground = System.Windows.Media.Brushes.Black;
                }
            };
            textBox.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    textBox.Text = placeholder;
                    textBox.Foreground = System.Windows.Media.Brushes.Gray;
                }
            };
            textBox.Foreground = System.Windows.Media.Brushes.Gray;

            panel.Children.Add(labelBlock);
            panel.Children.Add(textBox);

            return panel;
        }

        private void ProceedButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validate and collect data
                if (!ValidateAndCollectData())
                    return;

                WasAccepted = true;
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error processing information: {ex.Message}", 
                              "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            WasAccepted = false;
            DialogResult = false;
            Close();
        }

        private bool ValidateAndCollectData()
        {
            var errors = new System.Collections.Generic.List<string>();

            // Parse date of birth
            if (DateTime.TryParseExact(GetTextBoxValue(_dobTextBox), "dd/MM/yyyy", null, 
                System.Globalization.DateTimeStyles.None, out var dob))
            {
                AdditionalInfo.DateOfBirth = dob;
            }
            else
            {
                errors.Add("Please enter a valid date of birth (DD/MM/YYYY)");
            }

            // Collect text fields
            AdditionalInfo.Address = GetTextBoxValue(_addressTextBox);
            AdditionalInfo.City = GetTextBoxValue(_cityTextBox);
            AdditionalInfo.State = GetTextBoxValue(_stateTextBox);
            AdditionalInfo.Pincode = GetTextBoxValue(_pincodeTextBox);
            AdditionalInfo.EmailAddress = GetTextBoxValue(_emailTextBox);
            AdditionalInfo.MobileNumber = GetTextBoxValue(_mobileTextBox);
            AdditionalInfo.AadhaarNumber = GetTextBoxValue(_aadhaarTextBox);
            AdditionalInfo.BankAccountNumber = GetTextBoxValue(_bankAccountTextBox);
            AdditionalInfo.BankIFSCode = GetTextBoxValue(_bankIFSCTextBox);
            AdditionalInfo.BankName = GetTextBoxValue(_bankNameTextBox);

            // Basic validations
            if (string.IsNullOrWhiteSpace(AdditionalInfo.Address))
                errors.Add("Address is required");

            if (string.IsNullOrWhiteSpace(AdditionalInfo.City))
                errors.Add("City is required");

            if (string.IsNullOrWhiteSpace(AdditionalInfo.EmailAddress) || !AdditionalInfo.EmailAddress.Contains("@"))
                errors.Add("Valid email address is required");

            if (string.IsNullOrWhiteSpace(AdditionalInfo.MobileNumber) || AdditionalInfo.MobileNumber.Length != 10)
                errors.Add("Valid 10-digit mobile number is required");

            // Set defaults
            AdditionalInfo.TaxpayerCategory = TaxpayerCategory.Individual;
            AdditionalInfo.ResidencyStatus = ResidencyStatus.Resident;

            if (errors.Count > 0)
            {
                MessageBox.Show($"Please fix the following errors:\n\n• {string.Join("\n• ", errors)}", 
                              "Validation Errors", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private string GetTextBoxValue(TextBox textBox)
        {
            var value = textBox.Text?.Trim() ?? "";
            var placeholder = textBox.Tag?.ToString() ?? "";
            
            // Return empty if it's still showing placeholder
            return textBox.Foreground == System.Windows.Media.Brushes.Gray ? "" : value;
        }
    }
}
