using System.Windows;
using System.Windows.Controls;
using Returnly.Services;
using Returnly.Models;
using System.Collections.ObjectModel;
using System;
using System.IO;
using System.Text;
using Microsoft.Win32;

namespace Returnly
{
    public partial class TaxCalculationResultsPage : Page
    {
        private readonly NotificationService _notificationService;
        private readonly TaxCalculationResult _taxCalculationResult;
        private readonly TaxRefundCalculation _refundCalculation;
        private readonly Form16Data _form16Data;
        private RegimeComparisonResult? _regimeComparison;

        public TaxCalculationResultsPage(TaxCalculationResult taxCalculationResult, 
                                        TaxRefundCalculation refundCalculation, 
                                        Form16Data form16Data,
                                        RegimeComparisonResult? regimeComparison = null)
        {
            InitializeComponent();
            
            _taxCalculationResult = taxCalculationResult;
            _refundCalculation = refundCalculation;
            _form16Data = form16Data;
            _regimeComparison = regimeComparison;
            _notificationService = new NotificationService(NotificationPanel, NotificationTextBlock);
            
            LoadTaxResults();
        }

        private void LoadTaxResults()
        {
            try
            {
                // Update subtitle
                SubtitleTextBlock.Text = $"Your tax calculation for Financial Year {_taxCalculationResult.FinancialYear} (New Tax Regime)";

                // Update summary cards
                TaxableIncomeTextBlock.Text = $"₹{_taxCalculationResult.TaxableIncome:N0}";
                TaxLiabilityTextBlock.Text = $"₹{_taxCalculationResult.TotalTaxWithCess:N0}";
                TDSDeductedTextBlock.Text = $"₹{_refundCalculation.TDSDeducted:N0}";

                // Update refund/due information
                if (_refundCalculation.IsRefundDue)
                {
                    RefundDueLabel.Text = "Refund Due";
                    RefundDueAmountTextBlock.Text = $"₹{_refundCalculation.RefundAmount:N0}";
                    RefundDueIcon.Symbol = Wpf.Ui.Controls.SymbolRegular.Money24;
                    RefundDueIcon.Foreground = Application.Current.Resources["SystemFillColorSuccessBrush"] as System.Windows.Media.Brush;
                }
                else if (_refundCalculation.AdditionalTaxDue > 0)
                {
                    RefundDueLabel.Text = "Additional Tax Due";
                    RefundDueAmountTextBlock.Text = $"₹{_refundCalculation.AdditionalTaxDue:N0}";
                    RefundDueIcon.Symbol = Wpf.Ui.Controls.SymbolRegular.Warning24;
                    RefundDueIcon.Foreground = Application.Current.Resources["SystemFillColorCautionBrush"] as System.Windows.Media.Brush;
                }
                else
                {
                    RefundDueLabel.Text = "Tax Balanced";
                    RefundDueAmountTextBlock.Text = "₹0";
                    RefundDueIcon.Symbol = Wpf.Ui.Controls.SymbolRegular.Checkmark24;
                    RefundDueIcon.Foreground = Application.Current.Resources["SystemFillColorSuccessBrush"] as System.Windows.Media.Brush;
                }

                // Load tax breakdown
                TaxSlabsListView.ItemsSource = _taxCalculationResult.TaxBreakdown;

                // Update tax summary
                SubtotalTextBlock.Text = $"₹{_taxCalculationResult.TotalTax:N0}";
                
                // Show/hide surcharge based on whether it's applicable
                if (_taxCalculationResult.Surcharge > 0)
                {
                    SurchargeLabel.Text = $"Surcharge ({_taxCalculationResult.SurchargeRate}%):";
                    SurchargeTextBlock.Text = $"₹{_taxCalculationResult.Surcharge:N0}";
                    SurchargeLabel.Visibility = System.Windows.Visibility.Visible;
                    SurchargeTextBlock.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    SurchargeLabel.Visibility = System.Windows.Visibility.Collapsed;
                    SurchargeTextBlock.Visibility = System.Windows.Visibility.Collapsed;
                }
                
                CessTextBlock.Text = $"₹{_taxCalculationResult.HealthAndEducationCess:N0}";
                TotalTaxTextBlock.Text = $"₹{_taxCalculationResult.TotalTaxWithCess:N0}";
                EffectiveRateTextBlock.Text = $"{_taxCalculationResult.EffectiveTaxRate:F2}%";

                // Load regime comparison if available
                if (_regimeComparison != null)
                {
                    LoadRegimeComparison();
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification($"Error loading tax results: {ex.Message}", NotificationType.Error);
            }
        }

        private void LoadRegimeComparison()
        {
            if (_regimeComparison == null) return;

            RegimeComparisonCard.Visibility = Visibility.Visible;

            OldRegimeTaxTextBlock.Text = $"Total Tax: ₹{_regimeComparison.OldRegimeCalculation.TotalTaxWithCess:N0}";
            OldRegimeRateTextBlock.Text = $"Effective Rate: {_regimeComparison.OldRegimeCalculation.EffectiveTaxRate:F2}%";

            NewRegimeTaxTextBlock.Text = $"Total Tax: ₹{_regimeComparison.NewRegimeCalculation.TotalTaxWithCess:N0}";
            NewRegimeRateTextBlock.Text = $"Effective Rate: {_regimeComparison.NewRegimeCalculation.EffectiveTaxRate:F2}%";

            RecommendationTextBlock.Text = $"Recommendation: {_regimeComparison.RecommendedRegime} Tax Regime";
            
            if (_regimeComparison.TaxSavings > 0)
            {
                SavingsTextBlock.Text = $"Savings: ₹{Math.Abs(_regimeComparison.TaxSavings):N0} ({Math.Abs(_regimeComparison.SavingsPercentage):F1}%)";
            }
            else if (_regimeComparison.TaxSavings < 0)
            {
                SavingsTextBlock.Text = $"Additional Cost: ₹{Math.Abs(_regimeComparison.TaxSavings):N0} ({Math.Abs(_regimeComparison.SavingsPercentage):F1}%)";
            }
            else
            {
                SavingsTextBlock.Text = "No difference between regimes";
            }
        }

        private void BackToTaxInput_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }

        private void Recalculate_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new TaxDataInputPage(_form16Data));
        }

        private void ExportResults_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Title = "Export Tax Calculation Results",
                    Filter = "Text Files (*.txt)|*.txt|CSV Files (*.csv)|*.csv",
                    DefaultExt = "txt",
                    FileName = $"TaxCalculation_{_taxCalculationResult.FinancialYear}_{DateTime.Now:yyyyMMdd}"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var content = GenerateExportContent(saveFileDialog.FilterIndex == 2);
                    File.WriteAllText(saveFileDialog.FileName, content, Encoding.UTF8);
                    
                    _notificationService.ShowNotification($"Tax calculation results exported to {Path.GetFileName(saveFileDialog.FileName)}", NotificationType.Success);
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification($"Error exporting results: {ex.Message}", NotificationType.Error);
            }
        }

        private string GenerateExportContent(bool isCsv)
        {
            var separator = isCsv ? "," : "\t";
            var sb = new StringBuilder();

            if (isCsv)
            {
                sb.AppendLine("Tax Calculation Results Export");
                sb.AppendLine($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine($"Financial Year: {_taxCalculationResult.FinancialYear}");
                sb.AppendLine($"Tax Regime: {_taxCalculationResult.TaxRegime}");
                sb.AppendLine();

                sb.AppendLine("Summary");
                sb.AppendLine($"Taxable Income{separator}₹{_taxCalculationResult.TaxableIncome:N2}");
                sb.AppendLine($"Income Tax{separator}₹{_taxCalculationResult.TotalTax:N2}");
                if (_taxCalculationResult.Surcharge > 0)
                {
                    sb.AppendLine($"Surcharge ({_taxCalculationResult.SurchargeRate}%){separator}₹{_taxCalculationResult.Surcharge:N2}");
                }
                sb.AppendLine($"Health & Education Cess (4%){separator}₹{_taxCalculationResult.HealthAndEducationCess:N2}");
                sb.AppendLine($"Total Tax Liability{separator}₹{_taxCalculationResult.TotalTaxWithCess:N2}");
                sb.AppendLine($"TDS Deducted{separator}₹{_refundCalculation.TDSDeducted:N2}");
                sb.AppendLine($"Refund/Due{separator}₹{(_refundCalculation.IsRefundDue ? _refundCalculation.RefundAmount : -_refundCalculation.AdditionalTaxDue):N2}");
                sb.AppendLine();

                sb.AppendLine("Tax Breakdown");
                sb.AppendLine($"Slab Description{separator}Income in Slab{separator}Tax Rate{separator}Tax Amount");
                
                foreach (var slab in _taxCalculationResult.TaxBreakdown)
                {
                    sb.AppendLine($"{slab.SlabDescription}{separator}₹{slab.IncomeInSlab:N2}{separator}{slab.TaxRate}%{separator}₹{slab.TaxAmount:N2}");
                }
            }
            else
            {
                sb.AppendLine("TAX CALCULATION RESULTS");
                sb.AppendLine("=".PadRight(50, '='));
                sb.AppendLine($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine($"Financial Year: {_taxCalculationResult.FinancialYear}");
                sb.AppendLine($"Tax Regime: {_taxCalculationResult.TaxRegime}");
                sb.AppendLine();

                sb.AppendLine("SUMMARY");
                sb.AppendLine("-".PadRight(30, '-'));
                sb.AppendLine($"Taxable Income:           ₹{_taxCalculationResult.TaxableIncome:N2}");
                sb.AppendLine($"Income Tax:               ₹{_taxCalculationResult.TotalTax:N2}");
                if (_taxCalculationResult.Surcharge > 0)
                {
                    sb.AppendLine($"Surcharge ({_taxCalculationResult.SurchargeRate}%):        ₹{_taxCalculationResult.Surcharge:N2}");
                }
                sb.AppendLine($"Health & Education Cess:  ₹{_taxCalculationResult.HealthAndEducationCess:N2}");
                sb.AppendLine($"Total Tax Liability:      ₹{_taxCalculationResult.TotalTaxWithCess:N2}");
                sb.AppendLine($"TDS Deducted:             ₹{_refundCalculation.TDSDeducted:N2}");
                sb.AppendLine($"Effective Tax Rate:       {_taxCalculationResult.EffectiveTaxRate:F2}%");
                sb.AppendLine();

                if (_refundCalculation.IsRefundDue)
                {
                    sb.AppendLine($"REFUND DUE:               ₹{_refundCalculation.RefundAmount:N2}");
                }
                else if (_refundCalculation.AdditionalTaxDue > 0)
                {
                    sb.AppendLine($"ADDITIONAL TAX DUE:       ₹{_refundCalculation.AdditionalTaxDue:N2}");
                }
                else
                {
                    sb.AppendLine("TAX LIABILITY BALANCED");
                }
                sb.AppendLine();

                sb.AppendLine("TAX BREAKDOWN");
                sb.AppendLine("-".PadRight(80, '-'));
                
                foreach (var slab in _taxCalculationResult.TaxBreakdown)
                {
                    sb.AppendLine($"{slab.SlabDescription.PadRight(30)} ₹{slab.IncomeInSlab:N0} @ {slab.TaxRate}% = ₹{slab.TaxAmount:N2}");
                }
                
                sb.AppendLine("-".PadRight(80, '-'));
                sb.AppendLine($"{"Subtotal (Income Tax):".PadRight(30)} ₹{_taxCalculationResult.TotalTax:N2}");
                if (_taxCalculationResult.Surcharge > 0)
                {
                    sb.AppendLine($"{"Surcharge (" + _taxCalculationResult.SurchargeRate + "%):".PadRight(30)} ₹{_taxCalculationResult.Surcharge:N2}");
                }
                sb.AppendLine($"{"Health & Education Cess (4%):".PadRight(30)} ₹{_taxCalculationResult.HealthAndEducationCess:N2}");
                sb.AppendLine($"{"TOTAL TAX LIABILITY:".PadRight(30)} ₹{_taxCalculationResult.TotalTaxWithCess:N2}");
            }

            return sb.ToString();
        }

        private void ContinueToReturns_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Navigate to tax return generation page
            _notificationService.ShowNotification("Tax return generation feature coming soon!", NotificationType.Info);
        }
    }
}