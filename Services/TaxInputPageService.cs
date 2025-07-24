using System;
using System.Threading.Tasks;
using Returnly.Models;
using Returnly.ViewModels;

namespace Returnly.Services
{
    /// <summary>
    /// Service to handle tax input page operations and business logic
    /// </summary>
    public class TaxInputPageService
    {
        private readonly TaxCalculationService _taxCalculationService;
        private readonly NotificationService _notificationService;

        public TaxInputPageService(TaxCalculationService taxCalculationService, NotificationService notificationService)
        {
            _taxCalculationService = taxCalculationService ?? throw new ArgumentNullException(nameof(taxCalculationService));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        }

        /// <summary>
        /// Save ViewModel data to Form16Data model
        /// </summary>
        public void SaveDataToModel(TaxDataInputViewModel viewModel, Form16Data form16Data)
        {
            try
            {
                // Personal Information
                form16Data.EmployeeName = viewModel.EmployeeName;
                form16Data.PAN = viewModel.PAN;
                form16Data.AssessmentYear = viewModel.AssessmentYear;
                form16Data.FinancialYear = viewModel.FinancialYear;
                form16Data.EmployerName = viewModel.EmployerName;
                form16Data.TAN = viewModel.TAN;

                // Update Form16A data
                form16Data.Form16A.EmployeeName = form16Data.EmployeeName;
                form16Data.Form16A.PAN = form16Data.PAN;
                form16Data.Form16A.AssessmentYear = form16Data.AssessmentYear;
                form16Data.Form16A.FinancialYear = form16Data.FinancialYear;
                form16Data.Form16A.EmployerName = form16Data.EmployerName;
                form16Data.Form16A.TAN = form16Data.TAN;
                form16Data.Form16A.TotalTaxDeducted = viewModel.TotalTaxDeducted;

                // Update Form16B data - Official Structure
                form16Data.Form16B.SalarySection17 = viewModel.SalarySection17;
                form16Data.Form16B.Perquisites = viewModel.Perquisites;
                form16Data.Form16B.ProfitsInLieu = viewModel.ProfitsInLieu;
                
                // Breakdown of Section 17(1)
                form16Data.Form16B.BasicSalary = viewModel.BasicSalary;
                form16Data.Form16B.HRA = viewModel.HRA;
                form16Data.Form16B.SpecialAllowance = viewModel.SpecialAllowance;
                form16Data.Form16B.OtherAllowances = viewModel.OtherAllowances;
                
                // Interest Income
                form16Data.Form16B.InterestOnSavings = viewModel.InterestOnSavings;
                form16Data.Form16B.InterestOnFixedDeposits = viewModel.InterestOnFixedDeposits;
                form16Data.Form16B.InterestOnBonds = viewModel.InterestOnBonds;
                form16Data.Form16B.OtherInterestIncome = viewModel.OtherInterestIncome;
                
                // Dividend Income
                form16Data.Form16B.DividendIncomeAI = viewModel.DividendIncomeAI;
                form16Data.Form16B.DividendIncomeAII = viewModel.DividendIncomeAII;
                form16Data.Form16B.OtherDividendIncome = viewModel.OtherDividendIncome;
                
                // New Tax Regime Deductions Only
                form16Data.Form16B.StandardDeduction = viewModel.StandardDeduction;
                form16Data.Form16B.ProfessionalTax = viewModel.ProfessionalTax;
                form16Data.Form16B.TaxableIncome = viewModel.TaxableIncome;

                // Update Annexure data (only TDS, no Section 80 deductions)
                form16Data.Annexure.Q1TDS = viewModel.Q1TDS;
                form16Data.Annexure.Q2TDS = viewModel.Q2TDS;
                form16Data.Annexure.Q3TDS = viewModel.Q3TDS;
                form16Data.Annexure.Q4TDS = viewModel.Q4TDS;

                // Update backward compatibility fields
                form16Data.GrossSalary = viewModel.GrossSalary;
                form16Data.TotalTaxDeducted = viewModel.TotalTaxDeducted;
                form16Data.StandardDeduction = viewModel.StandardDeduction;
                form16Data.ProfessionalTax = viewModel.ProfessionalTax;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error saving data to model: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Calculate tax for the given inputs
        /// </summary>
        public async Task<(TaxCalculationResult TaxCalculation, TaxRefundCalculation RefundCalculation)> CalculateTaxAsync(
            decimal taxableIncome, string financialYear, decimal tdsDeducted, int age = 30)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (taxableIncome <= 0)
                    {
                        throw new ArgumentException("Please enter a valid taxable income amount.");
                    }

                    if (string.IsNullOrEmpty(financialYear))
                    {
                        throw new ArgumentException("Please select a financial year.");
                    }

                    // Calculate tax using New Tax Regime (default for this app)
                    var taxCalculation = _taxCalculationService.CalculateTax(
                        taxableIncome, 
                        financialYear, 
                        TaxRegime.New, 
                        age
                    );

                    // Calculate refund/additional tax
                    var refundCalculation = _taxCalculationService.CalculateRefund(taxCalculation, tdsDeducted);

                    return (taxCalculation, refundCalculation);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Error calculating taxes: {ex.Message}", ex);
                }
            });
        }

        /// <summary>
        /// Compare tax regimes
        /// </summary>
        public async Task<RegimeComparisonResult> CompareTaxRegimesAsync(
            decimal taxableIncome, string financialYear, int age = 30)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (taxableIncome <= 0 || string.IsNullOrEmpty(financialYear))
                    {
                        throw new ArgumentException("Please enter valid income and select financial year.");
                    }

                    // For comparison, assume old regime has some deductions (can be made configurable)
                    var estimatedOldRegimeDeductions = Math.Min(taxableIncome * 0.2m, 150000); // Estimate 20% or max 1.5L
                    var oldRegimeTaxableIncome = taxableIncome + estimatedOldRegimeDeductions; // Add back deductions for old regime calculation

                    var comparison = _taxCalculationService.CompareTaxRegimes(
                        oldRegimeTaxableIncome,
                        taxableIncome,
                        financialYear,
                        age,
                        estimatedOldRegimeDeductions
                    );

                    return comparison;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Error comparing regimes: {ex.Message}", ex);
                }
            });
        }

        /// <summary>
        /// Generate tax calculation result breakdown text
        /// </summary>
        public string GenerateCalculationBreakdown(TaxCalculationResult taxCalculation, TaxRefundCalculation refundCalculation)
        {
            try
            {
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

                return breakdown;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error generating calculation breakdown: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Generate regime comparison text
        /// </summary>
        public string GenerateRegimeComparisonText(RegimeComparisonResult comparison)
        {
            try
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

                return comparisonText;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error generating regime comparison text: {ex.Message}", ex);
            }
        }
    }
}
