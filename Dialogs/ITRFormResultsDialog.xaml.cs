using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Microsoft.Win32;
using Returnly.Models;
using Returnly.Services;

namespace Returnly.Dialogs
{
    /// <summary>
    /// Dialog to display ITR form generation results with export options
    /// </summary>
    public class ITRFormResultsDialog : Window
    {
        private readonly ITRFormGenerationResult _result;

        public ITRFormResultsDialog(ITRFormGenerationResult result)
        {
            _result = result ?? throw new ArgumentNullException(nameof(result));
            InitializeWindow();
            DisplayResults();
        }

        private void InitializeWindow()
        {
            this.Width = 700;
            this.Height = 800;
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            this.ResizeMode = ResizeMode.CanResize;
            this.Title = $"ITR Form Generation Results - {_result.GeneratedFormType}";
        }

        private void DisplayResults()
        {
            var mainPanel = new StackPanel
            {
                Margin = new Thickness(20),
                Orientation = Orientation.Vertical
            };

            // Header
            var header = new TextBlock
            {
                Text = _result.IsSuccess ? "✅ ITR Form Generated Successfully!" : "❌ ITR Form Generation Failed",
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 20),
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = _result.IsSuccess ? System.Windows.Media.Brushes.Green : System.Windows.Media.Brushes.Red
            };
            mainPanel.Children.Add(header);

            if (!_result.IsSuccess)
            {
                // Error section
                var errorSection = CreateSection("❌ Error Details", _result.ErrorMessage);
                mainPanel.Children.Add(errorSection);
            }
            else
            {
                // Success sections
                CreateSuccessContent(mainPanel);
            }

            // Close button
            var closeButton = new Button
            {
                Content = "Close",
                Padding = new Thickness(20, 10, 20, 10),
                Margin = new Thickness(0, 20, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Center,
                IsCancel = true
            };
            closeButton.Click += (s, e) => Close();
            mainPanel.Children.Add(closeButton);

            // Set content with scroll viewer
            this.Content = new ScrollViewer
            {
                Content = mainPanel,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };
        }

        private void CreateSuccessContent(StackPanel mainPanel)
        {
            // Form summary section
            var summaryContent = $"Form Type: {_result.GeneratedFormType}\n" +
                               $"Generated: {_result.GeneratedAt:yyyy-MM-dd HH:mm:ss}\n" +
                               $"Total Income: ₹{_result.TotalIncome:N2}\n" +
                               $"Tax Liability: ₹{_result.TaxLiability:N2}\n";

            if (_result.IsRefund)
                summaryContent += $"Refund Amount: ₹{_result.RefundAmount:N2} 💰";
            else if (_result.IsDemand)
                summaryContent += $"Tax Demand: ₹{_result.DemandAmount:N2} ⚠️";
            else
                summaryContent += "Tax Liability Fully Paid ✅";

            var summarySection = CreateSection("📋 Form Summary", summaryContent);
            mainPanel.Children.Add(summarySection);

            // Validation section
            if (_result.ValidationWarnings.Count > 0 || _result.ValidationErrors.Count > 0)
            {
                var validationContent = "";
                if (_result.ValidationErrors.Count > 0)
                {
                    validationContent += "Errors:\n" + string.Join("\n", _result.ValidationErrors.Select(e => $"• {e}")) + "\n\n";
                }
                if (_result.ValidationWarnings.Count > 0)
                {
                    validationContent += "Warnings:\n" + string.Join("\n", _result.ValidationWarnings.Select(w => $"• {w}"));
                }

                var validationSection = CreateSection("⚠️ Validation Results", validationContent.Trim());
                mainPanel.Children.Add(validationSection);
            }

            // ITR Selection Details
            if (_result.ITRSelection != null)
            {
                var selectionContent = $"Recommended: {_result.ITRSelection.RecommendedITR}\n" +
                                     $"Reason: {_result.ITRSelection.PrimaryReason}\n" +
                                     $"Explanation: {_result.ITRSelection.Explanation}";

                var selectionSection = CreateSection("🎯 ITR Selection", selectionContent);
                mainPanel.Children.Add(selectionSection);
            }

            // Export options
            if (_result.HasXMLContent)
            {
                var exportSection = CreateExportSection();
                mainPanel.Children.Add(exportSection);
            }

            // Form details section
            var detailsSection = CreateFormDetailsSection();
            mainPanel.Children.Add(detailsSection);
        }

        private Border CreateSection(string title, string content)
        {
            var section = new Border
            {
                BorderBrush = System.Windows.Media.Brushes.LightGray,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(5),
                Padding = new Thickness(15),
                Margin = new Thickness(0, 0, 0, 15),
                Background = System.Windows.Media.Brushes.WhiteSmoke
            };

            var stackPanel = new StackPanel();

            var titleBlock = new TextBlock
            {
                Text = title,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 10)
            };

            var contentBlock = new TextBlock
            {
                Text = content,
                FontSize = 14,
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 20
            };

            stackPanel.Children.Add(titleBlock);
            stackPanel.Children.Add(contentBlock);
            section.Child = stackPanel;

            return section;
        }

        private Border CreateExportSection()
        {
            var section = new Border
            {
                BorderBrush = System.Windows.Media.Brushes.DarkGreen,
                BorderThickness = new Thickness(2),
                CornerRadius = new CornerRadius(5),
                Padding = new Thickness(15),
                Margin = new Thickness(0, 0, 0, 15),
                Background = System.Windows.Media.Brushes.LightGreen
            };

            var stackPanel = new StackPanel();

            var titleBlock = new TextBlock
            {
                Text = "💾 Export Options",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 15)
            };
            stackPanel.Children.Add(titleBlock);

            var infoBlock = new TextBlock
            {
                Text = "Your ITR form has been generated and is ready for export:",
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 10)
            };
            stackPanel.Children.Add(infoBlock);

            // Export buttons
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 10, 0, 0)
            };

            var downloadXmlButton = new Button
            {
                Content = "📥 Download XML File",
                Padding = new Thickness(15, 8, 15, 8),
                Margin = new Thickness(0, 0, 10, 0),
                Background = System.Windows.Media.Brushes.LightBlue
            };
            downloadXmlButton.Click += DownloadXmlButton_Click;

            var previewXmlButton = new Button
            {
                Content = "👁️ Preview XML",
                Padding = new Thickness(15, 8, 15, 8),
                Margin = new Thickness(0, 0, 10, 0)
            };
            previewXmlButton.Click += PreviewXmlButton_Click;

            buttonPanel.Children.Add(downloadXmlButton);
            buttonPanel.Children.Add(previewXmlButton);
            stackPanel.Children.Add(buttonPanel);

            section.Child = stackPanel;
            return section;
        }

        private Border CreateFormDetailsSection()
        {
            var section = new Border
            {
                BorderBrush = System.Windows.Media.Brushes.LightGray,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(5),
                Padding = new Thickness(15),
                Margin = new Thickness(0, 0, 0, 15),
                Background = System.Windows.Media.Brushes.AliceBlue
            };

            var stackPanel = new StackPanel();

            var titleBlock = new TextBlock
            {
                Text = "📝 Form Details",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 10)
            };
            stackPanel.Children.Add(titleBlock);

            string detailsContent = "";

            if (_result.GeneratedFormType == ITRType.ITR1_Sahaj && _result.ITR1Data != null)
            {
                detailsContent = $"Taxpayer: {_result.ITR1Data.Name}\n" +
                               $"PAN: {_result.ITR1Data.PAN}\n" +
                               $"Assessment Year: {_result.ITR1Data.AssessmentYear}\n" +
                               $"Financial Year: {_result.ITR1Data.FinancialYear}\n\n" +
                               $"Income Details:\n" +
                               $"• Gross Salary: ₹{_result.ITR1Data.GrossSalary:N2}\n" +
                               $"• Interest Income: ₹{(_result.ITR1Data.InterestFromSavingsAccount + _result.ITR1Data.InterestFromDeposits):N2}\n" +
                               $"• Dividend Income: ₹{_result.ITR1Data.DividendIncome:N2}\n" +
                               $"• House Property Income: ₹{(_result.ITR1Data.AnnualValue - _result.ITR1Data.PropertyTax - _result.ITR1Data.StandardDeduction30Percent - _result.ITR1Data.InterestOnHomeLoan):N2}\n\n" +
                               $"Deductions:\n" +
                               $"• Standard Deduction: ₹{_result.ITR1Data.StandardDeduction:N2}\n" +
                               $"• Professional Tax: ₹{_result.ITR1Data.ProfessionalTax:N2}\n\n" +
                               $"Tax Information:\n" +
                               $"• TDS: ₹{_result.ITR1Data.TaxDeductedAtSource:N2}\n" +
                               $"• Advance Tax: ₹{_result.ITR1Data.AdvanceTax:N2}";
            }
            else if (_result.GeneratedFormType == ITRType.ITR2 && _result.ITR2Data != null)
            {
                detailsContent = $"Taxpayer: {_result.ITR2Data.Name}\n" +
                               $"PAN: {_result.ITR2Data.PAN}\n" +
                               $"Assessment Year: {_result.ITR2Data.AssessmentYear}\n" +
                               $"Financial Year: {_result.ITR2Data.FinancialYear}\n" +
                               $"Category: {_result.ITR2Data.Category}\n\n" +
                               $"Income Sources:\n" +
                               $"• Salary Employers: {_result.ITR2Data.SalaryDetails?.Count ?? 0}\n" +
                               $"• House Properties: {_result.ITR2Data.HouseProperties?.Count ?? 0}\n" +
                               $"• Capital Gains: {_result.ITR2Data.CapitalGains?.Count ?? 0}\n" +
                               $"• Interest Income: ₹{_result.ITR2Data.InterestIncome:N2}\n" +
                               $"• Dividend Income: ₹{_result.ITR2Data.DividendIncome:N2}\n\n" +
                               $"Special Circumstances:\n" +
                               $"• Foreign Income: {(_result.ITR2Data.HasForeignIncome ? "Yes" : "No")}\n" +
                               $"• Foreign Assets: {(_result.ITR2Data.HasForeignAssets ? "Yes" : "No")}\n\n" +
                               $"Tax Information:\n" +
                               $"• TDS: ₹{_result.ITR2Data.TaxDeductedAtSource:N2}\n" +
                               $"• Advance Tax: ₹{_result.ITR2Data.AdvanceTax:N2}";
            }

            var contentBlock = new TextBlock
            {
                Text = detailsContent,
                FontSize = 12,
                FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 18
            };
            stackPanel.Children.Add(contentBlock);

            section.Child = stackPanel;
            return section;
        }

        private void DownloadXmlButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*",
                    FileName = _result.DisplayFileName,
                    DefaultExt = ".xml",
                    Title = "Save ITR XML File"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    File.WriteAllText(saveFileDialog.FileName, _result.XMLContent);
                    MessageBox.Show($"ITR XML file saved successfully to:\n{saveFileDialog.FileName}", 
                                  "Export Successful", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving XML file: {ex.Message}", 
                              "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PreviewXmlButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var previewWindow = new Window
                {
                    Title = "ITR XML Preview",
                    Width = 800,
                    Height = 600,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = this
                };

                var textBox = new TextBox
                {
                    Text = _result.XMLContent,
                    IsReadOnly = true,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                    FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                    FontSize = 12,
                    Margin = new Thickness(10)
                };

                previewWindow.Content = textBox;
                previewWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error previewing XML: {ex.Message}", 
                              "Preview Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
