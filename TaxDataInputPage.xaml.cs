using System.Windows;
using System.Windows.Controls;
using Returnly.Services;
using Returnly.Models;
using Returnly.ViewModels;
using System;

namespace Returnly
{
    public partial class TaxDataInputPage : Page
    {
        private readonly NotificationService _notificationService;
        private readonly TaxCalculationService _taxCalculationService;
        private readonly TaxConfigurationService _taxConfigurationService;
        private readonly TaxDataInputViewModel _viewModel;
        private Form16Data _form16Data;
        private TaxCalculationResult? _currentTaxCalculation;

        public TaxDataInputPage() : this(new Form16Data())
        {
        }

        public TaxDataInputPage(Form16Data form16Data)
        {
            InitializeComponent();
            
            _form16Data = form16Data ?? new Form16Data();
            _notificationService = new NotificationService(NotificationPanel, NotificationTextBlock);
            _taxCalculationService = new TaxCalculationService();
            _taxConfigurationService = new TaxConfigurationService();
            _viewModel = new TaxDataInputViewModel(_notificationService);
            
            // Set the DataContext for binding
            this.DataContext = _viewModel;
            
            // Load data from Form16Data into ViewModel
            _viewModel.LoadFromForm16Data(_form16Data);
        }

        private void SaveDataToModel()
        {
            try
            {
                // Personal Information
                _form16Data.EmployeeName = _viewModel.EmployeeName;
                _form16Data.PAN = _viewModel.PAN;
                _form16Data.AssessmentYear = _viewModel.AssessmentYear;
                _form16Data.FinancialYear = _viewModel.FinancialYear;
                _form16Data.EmployerName = _viewModel.EmployerName;
                _form16Data.TAN = _viewModel.TAN;

                // Update Form16A data
                _form16Data.Form16A.EmployeeName = _form16Data.EmployeeName;
                _form16Data.Form16A.PAN = _form16Data.PAN;
                _form16Data.Form16A.AssessmentYear = _form16Data.AssessmentYear;
                _form16Data.Form16A.FinancialYear = _form16Data.FinancialYear;
                _form16Data.Form16A.EmployerName = _form16Data.EmployerName;
                _form16Data.Form16A.TAN = _form16Data.TAN;
                _form16Data.Form16A.TotalTaxDeducted = _viewModel.TotalTaxDeducted;

                // Update Form16B data - Official Structure
                _form16Data.Form16B.SalarySection17 = _viewModel.SalarySection17;
                _form16Data.Form16B.Perquisites = _viewModel.Perquisites;
                _form16Data.Form16B.ProfitsInLieu = _viewModel.ProfitsInLieu;
                
                // Breakdown of Section 17(1)
                _form16Data.Form16B.BasicSalary = _viewModel.BasicSalary;
                _form16Data.Form16B.HRA = _viewModel.HRA;
                _form16Data.Form16B.SpecialAllowance = _viewModel.SpecialAllowance;
                _form16Data.Form16B.OtherAllowances = _viewModel.OtherAllowances;
                
                // New Tax Regime Deductions Only
                _form16Data.Form16B.StandardDeduction = _viewModel.StandardDeduction;
                _form16Data.Form16B.ProfessionalTax = _viewModel.ProfessionalTax;
                _form16Data.Form16B.TaxableIncome = _viewModel.TaxableIncome;

                // Update Annexure data (only TDS, no Section 80 deductions)
                _form16Data.Annexure.Q1TDS = _viewModel.Q1TDS;
                _form16Data.Annexure.Q2TDS = _viewModel.Q2TDS;
                _form16Data.Annexure.Q3TDS = _viewModel.Q3TDS;
                _form16Data.Annexure.Q4TDS = _viewModel.Q4TDS;

                // Update backward compatibility fields
                _form16Data.GrossSalary = _viewModel.GrossSalary;
                _form16Data.TotalTaxDeducted = _viewModel.TotalTaxDeducted;
                _form16Data.StandardDeduction = _viewModel.StandardDeduction;
                _form16Data.ProfessionalTax = _viewModel.ProfessionalTax;
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification($"Error saving data: {ex.Message}", NotificationType.Error);
            }
        }

        private bool ValidateData()
        {
            return _viewModel.ValidateData(out string errorMessage);
        }

        private void CalculateTaxes_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateData())
                    return;

                SaveDataToModel();

                var taxableIncome = _viewModel.TaxableIncome;
                var financialYear = _viewModel.FinancialYear;
                var tdsDeducted = _viewModel.TotalTaxDeducted;

                if (taxableIncome <= 0)
                {
                    _notificationService.ShowNotification("Please enter a valid taxable income amount.", NotificationType.Warning);
                    return;
                }

                if (string.IsNullOrEmpty(financialYear))
                {
                    _notificationService.ShowNotification("Please select a financial year.", NotificationType.Warning);
                    return;
                }

                // Calculate tax using New Tax Regime (default for this app)
                _currentTaxCalculation = _taxCalculationService.CalculateTax(
                    taxableIncome, 
                    financialYear, 
                    TaxRegime.New, 
                    30 // Default age, can be made configurable
                );

                // Calculate refund/additional tax
                var refundCalculation = _taxCalculationService.CalculateRefund(_currentTaxCalculation, tdsDeducted);

                // Show results
                ShowTaxCalculationResults(_currentTaxCalculation, refundCalculation);

                _notificationService.ShowNotification(
                    $"Tax calculation completed! Total tax liability: ₹{_currentTaxCalculation.TotalTaxWithCess:N2}", 
                    NotificationType.Success
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}");
                _notificationService.ShowNotification($"Error calculating taxes: {ex.Message}", NotificationType.Error);
            }
        }

        private void ShowTaxCalculationResults(TaxCalculationResult taxCalculation, TaxRefundCalculation refundCalculation)
        {
            // Create a detailed breakdown string
            var breakdown = "Tax Calculation Breakdown:\n\n";
            
            foreach (var slab in taxCalculation.TaxBreakdown)
            {
                breakdown += $"• {slab.SlabDescription}: ₹{slab.IncomeInSlab:N2} @ {slab.TaxRate}% = ₹{slab.TaxAmount:N2}\n";
            }
            
            breakdown += $"\nSubtotal: ₹{taxCalculation.TotalTax:N2}\n";
            if (taxCalculation.Surcharge > 0)
            {
                breakdown += $"Surcharge ({taxCalculation.SurchargeRate}%): ₹{taxCalculation.Surcharge:N2}\n";
            }
            breakdown += $"Health & Education Cess (4%): ₹{taxCalculation.HealthAndEducationCess:N2}\n";
            breakdown += $"Total Tax Liability: ₹{taxCalculation.TotalTaxWithCess:N2}\n";
            breakdown += $"Effective Tax Rate: {taxCalculation.EffectiveTaxRate:F2}%\n\n";
            
            breakdown += "Refund/Additional Tax Analysis:\n";
            breakdown += $"TDS Deducted: ₹{refundCalculation.TDSDeducted:N2}\n";
            
            if (refundCalculation.IsRefundDue)
            {
                breakdown += $"Refund Due: ₹{refundCalculation.RefundAmount:N2} 🎉";
            }
            else if (refundCalculation.AdditionalTaxDue > 0)
            {
                breakdown += $"Additional Tax Due: ₹{refundCalculation.AdditionalTaxDue:N2} ⚠️";
            }
            else
            {
                breakdown += "Tax liability exactly matches TDS deducted ✅";
            }

            // Show in a message box for now (can be replaced with a dedicated results panel later)
            var messageBox = new Wpf.Ui.Controls.MessageBox
            {
                Title = "Tax Calculation Results",
                Content = breakdown,
                Width = 600,
                Height = 500
            };
            messageBox.ShowDialogAsync();
        }

        private void CompareRegimes_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateData())
                    return;

                var taxableIncome = _viewModel.TaxableIncome;
                var financialYear = _viewModel.FinancialYear;

                if (taxableIncome <= 0 || string.IsNullOrEmpty(financialYear))
                {
                    _notificationService.ShowNotification("Please enter valid income and select financial year.", NotificationType.Warning);
                    return;
                }

                // For comparison, assume old regime has some deductions (can be made configurable)
                var estimatedOldRegimeDeductions = Math.Min(taxableIncome * 0.2m, 150000); // Estimate 20% or max 1.5L
                var oldRegimeTaxableIncome = taxableIncome + estimatedOldRegimeDeductions; // Add back deductions for old regime calculation

                var comparison = _taxCalculationService.CompareTaxRegimes(
                    oldRegimeTaxableIncome,
                    taxableIncome,
                    financialYear,
                    30,
                    estimatedOldRegimeDeductions
                );

                ShowRegimeComparison(comparison);
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification($"Error comparing regimes: {ex.Message}", NotificationType.Error);
            }
        }

        private void ShowRegimeComparison(RegimeComparisonResult comparison)
        {
            var comparisonText = "Tax Regime Comparison:\n\n";
            
            comparisonText += $"OLD TAX REGIME:\n";
            comparisonText += $"Taxable Income: ₹{comparison.OldRegimeCalculation.TaxableIncome:N2}\n";
            comparisonText += $"Total Tax: ₹{comparison.OldRegimeCalculation.TotalTaxWithCess:N2}\n";
            comparisonText += $"Effective Rate: {comparison.OldRegimeCalculation.EffectiveTaxRate:F2}%\n\n";
            
            comparisonText += $"NEW TAX REGIME:\n";
            comparisonText += $"Taxable Income: ₹{comparison.NewRegimeCalculation.TaxableIncome:N2}\n";
            comparisonText += $"Total Tax: ₹{comparison.NewRegimeCalculation.TotalTaxWithCess:N2}\n";
            comparisonText += $"Effective Rate: {comparison.NewRegimeCalculation.EffectiveTaxRate:F2}%\n\n";
            
            comparisonText += $"RECOMMENDATION: {comparison.RecommendedRegime} Tax Regime\n";
            
            if (comparison.TaxSavings > 0)
            {
                comparisonText += $"Savings with {comparison.RecommendedRegime} Regime: ₹{Math.Abs(comparison.TaxSavings):N2} ({Math.Abs(comparison.SavingsPercentage):F1}%)";
            }
            else if (comparison.TaxSavings < 0)
            {
                comparisonText += $"Additional cost with New Regime: ₹{Math.Abs(comparison.TaxSavings):N2} ({Math.Abs(comparison.SavingsPercentage):F1}%)";
            }
            else
            {
                comparisonText += "Both regimes result in the same tax liability.";
            }

            var messageBox = new Wpf.Ui.Controls.MessageBox
            {
                Title = "Tax Regime Comparison",
                Content = comparisonText,
                Width = 500,
                Height = 400
            };
            messageBox.ShowDialogAsync();
        }

        private void BackToUpload_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (NavigationService != null)
                {
                    NavigationService.Navigate(new Form16UploadPage());
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification($"Error navigating back: {ex.Message}", NotificationType.Error);
            }
        }

        private void SaveData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateData())
                {
                    return;
                }

                // Save data to model
                SaveDataToModel();
                
                _notificationService.ShowNotification("Tax data saved successfully!", NotificationType.Success);
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification($"Error saving data: {ex.Message}", NotificationType.Error);
            }
        }

        private void ContinueToReturns_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateData())
                {
                    return;
                }

                // Save current data
                SaveDataToModel();

                // Check if tax calculation has been done
                if (_currentTaxCalculation == null)
                {
                    _notificationService.ShowNotification("Please calculate taxes first before proceeding.", NotificationType.Warning);
                    return;
                }

                // Calculate refund/additional tax
                var tdsDeducted = _viewModel.TotalTaxDeducted;
                var refundCalculation = _taxCalculationService.CalculateRefund(_currentTaxCalculation, tdsDeducted);

                // Navigate to results page
                NavigationService?.Navigate(new TaxCalculationResultsPage(_currentTaxCalculation, refundCalculation, _form16Data));
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification($"Error proceeding to returns: {ex.Message}", NotificationType.Error);
            }
        }
    }
}