using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Returnly.Models
{
    /// <summary>
    /// Base class for all ITR form data models
    /// </summary>
    public abstract class BaseITRData
    {
        // Common fields across all ITR forms
        public string AssessmentYear { get; set; } = string.Empty;
        public string FinancialYear { get; set; } = string.Empty;
        public string PAN { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public ResidencyStatus ResidencyStatus { get; set; } = ResidencyStatus.Resident;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Pincode { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty;
        public string AadhaarNumber { get; set; } = string.Empty;
        
        // Bank details for refund
        public string BankAccountNumber { get; set; } = string.Empty;
        public string BankIFSCCode { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        
        // Validation errors
        public List<string> ValidationErrors { get; set; } = new();
        
        // Abstract methods that each ITR type must implement
        public abstract decimal CalculateTotalIncome();
        public abstract decimal CalculateTaxLiability();
        public abstract decimal CalculateRefundOrDemand();
        public abstract bool ValidateData();
        public abstract string GetFormName();
        public abstract string GetFormVersion();
    }

    /// <summary>
    /// ITR-1 (Sahaj) form data model
    /// For individuals with income up to Rs 50 lakh from salary, one house property, and other sources
    /// </summary>
    public class ITR1Data : BaseITRData
    {
        // Section 1: Basic Information
        public string EmployerName { get; set; } = string.Empty;
        public string EmployerTAN { get; set; } = string.Empty;
        public string EmployerAddress { get; set; } = string.Empty;
        
        // Section 2: Income Details
        
        // 2.1 Salary Income (as per Form 16)
        public decimal GrossSalary { get; set; }
        public decimal AllowanceExempt { get; set; }
        public decimal AllowanceTaxable { get; set; }
        public decimal Perquisites { get; set; }
        public decimal ProfitsInLieu { get; set; }
        
        // 2.2 Income from House Property (only one house property allowed)
        public decimal AnnualValue { get; set; }
        public decimal PropertyTax { get; set; }
        public decimal StandardDeduction30Percent { get; set; }
        public decimal InterestOnHomeLoan { get; set; }
        
        // 2.3 Income from Other Sources
        public decimal InterestFromSavingsAccount { get; set; }
        public decimal InterestFromDeposits { get; set; }
        public decimal DividendIncome { get; set; }
        public decimal OtherIncome { get; set; }
        
        // Section 3: Deductions (New Tax Regime - Limited)
        public decimal StandardDeduction { get; set; } = 50000; // Standard deduction for new regime
        public decimal ProfessionalTax { get; set; }
        public decimal EntertainmentAllowance { get; set; }
        
        // Section 4: Tax Details
        public decimal TaxDeductedAtSource { get; set; }
        public decimal AdvanceTax { get; set; }
        public decimal SelfAssessmentTax { get; set; }
        
        // Quarterly TDS breakdown
        public decimal Q1TDS { get; set; }
        public decimal Q2TDS { get; set; }
        public decimal Q3TDS { get; set; }
        public decimal Q4TDS { get; set; }
        
        // Calculated fields
        public decimal TotalSalaryIncome => GrossSalary + AllowanceTaxable + Perquisites + ProfitsInLieu - AllowanceExempt;
        public decimal TotalHousePropertyIncome => AnnualValue - PropertyTax - StandardDeduction30Percent - InterestOnHomeLoan;
        public decimal TotalOtherIncome => InterestFromSavingsAccount + InterestFromDeposits + DividendIncome + OtherIncome;
        public decimal TotalDeductions => StandardDeduction + ProfessionalTax + EntertainmentAllowance;
        public decimal TotalTaxPaid => TaxDeductedAtSource + AdvanceTax + SelfAssessmentTax;
        
        // Validation specific to ITR-1
        private readonly List<string> _validationRules = new()
        {
            "Total income should not exceed Rs 50,00,000",
            "Only one house property is allowed",
            "No capital gains allowed",
            "No business/professional income allowed",
            "Must be an individual (not HUF)"
        };

        public override decimal CalculateTotalIncome()
        {
            return TotalSalaryIncome + Math.Max(0, TotalHousePropertyIncome) + TotalOtherIncome;
        }

        public override decimal CalculateTaxLiability()
        {
            var totalIncome = CalculateTotalIncome();
            var taxableIncome = Math.Max(0, totalIncome - TotalDeductions);
            
            // New Tax Regime slabs for AY 2024-25
            return CalculateNewRegimeTax(taxableIncome);
        }

        public override decimal CalculateRefundOrDemand()
        {
            var taxLiability = CalculateTaxLiability();
            var taxPaid = TotalTaxPaid;
            
            // Positive means refund, negative means demand
            return taxPaid - taxLiability;
        }

        public override bool ValidateData()
        {
            ValidationErrors.Clear();
            
            // Basic validations
            if (string.IsNullOrEmpty(PAN) || PAN.Length != 10)
                ValidationErrors.Add("Valid PAN is required (10 characters)");
            
            if (string.IsNullOrEmpty(Name))
                ValidationErrors.Add("Name is required");
            
            if (CalculateTotalIncome() > 5000000)
                ValidationErrors.Add("Total income exceeds Rs 50,00,000 limit for ITR-1");
            
            if (string.IsNullOrEmpty(EmployerTAN) || EmployerTAN.Length != 10)
                ValidationErrors.Add("Valid Employer TAN is required");
            
            // Validate TDS matches quarterly breakdown
            var quarterlyTotal = Q1TDS + Q2TDS + Q3TDS + Q4TDS;
            if (Math.Abs(TaxDeductedAtSource - quarterlyTotal) > 1)
                ValidationErrors.Add("TDS amount doesn't match quarterly breakdown");
            
            // Bank details validation for refund cases
            var refundAmount = CalculateRefundOrDemand();
            if (refundAmount > 0)
            {
                if (string.IsNullOrEmpty(BankAccountNumber))
                    ValidationErrors.Add("Bank account number required for refund");
                if (string.IsNullOrEmpty(BankIFSCCode))
                    ValidationErrors.Add("Bank IFSC code required for refund");
            }
            
            return ValidationErrors.Count == 0;
        }

        public override string GetFormName() => "ITR-1 (Sahaj)";
        public override string GetFormVersion() => "Ver 1.0 (AY 2024-25)";
        
        private decimal CalculateNewRegimeTax(decimal taxableIncome)
        {
            decimal tax = 0;
            
            // New Tax Regime slabs for AY 2024-25
            if (taxableIncome <= 300000) tax = 0;
            else if (taxableIncome <= 600000) tax = (taxableIncome - 300000) * 0.05m;
            else if (taxableIncome <= 900000) tax = 15000 + (taxableIncome - 600000) * 0.10m;
            else if (taxableIncome <= 1200000) tax = 45000 + (taxableIncome - 900000) * 0.15m;
            else if (taxableIncome <= 1500000) tax = 90000 + (taxableIncome - 1200000) * 0.20m;
            else tax = 150000 + (taxableIncome - 1500000) * 0.30m;
            
            // Add 4% Health and Education Cess
            tax += tax * 0.04m;
            
            return Math.Round(tax, 0);
        }
    }

    /// <summary>
    /// ITR-2 form data model
    /// For individuals and HUFs not having income from business or profession
    /// </summary>
    public class ITR2Data : BaseITRData
    {
        // Section 1: Additional Basic Information
        public TaxpayerCategory Category { get; set; } = TaxpayerCategory.Individual;
        public string HUFName { get; set; } = string.Empty; // For HUF cases
        
        // Section 2: Income Details
        
        // 2.1 Salary Income (can have multiple employers)
        public List<SalaryDetails> SalaryDetails { get; set; } = new();
        
        // 2.2 Income from House Property (multiple properties allowed)
        public List<HousePropertyDetails> HouseProperties { get; set; } = new();
        
        // 2.3 Capital Gains
        public List<CapitalGainDetails> CapitalGains { get; set; } = new();
        
        // 2.4 Income from Other Sources
        public decimal InterestIncome { get; set; }
        public decimal DividendIncome { get; set; }
        public decimal OtherSourcesIncome { get; set; }
        
        // 2.5 Foreign Income and Assets (if applicable)
        public bool HasForeignIncome { get; set; }
        public decimal ForeignIncome { get; set; }
        public bool HasForeignAssets { get; set; }
        public List<ForeignAssetDetails> ForeignAssets { get; set; } = new();
        
        // Section 3: Deductions
        public decimal StandardDeduction { get; set; } = 50000; // For new regime
        public decimal ProfessionalTax { get; set; }
        
        // Section 4: Tax Details
        public decimal TaxDeductedAtSource { get; set; }
        public decimal AdvanceTax { get; set; }
        public decimal SelfAssessmentTax { get; set; }
        public decimal TaxDeductedCollectedAtSource { get; set; }
        
        // TDS from multiple sources
        public List<TDSDetails> TDSDetails { get; set; } = new();
        
        // Calculated properties
        public decimal TotalSalaryIncome => SalaryDetails.Sum(s => s.GrossSalary);
        public decimal TotalHousePropertyIncome => HouseProperties.Sum(h => h.NetIncome);
        public decimal TotalCapitalGains => CapitalGains.Sum(c => c.NetGain);
        public decimal TotalOtherIncome => InterestIncome + DividendIncome + OtherSourcesIncome + ForeignIncome;
        public decimal TotalTaxPaid => TaxDeductedAtSource + AdvanceTax + SelfAssessmentTax + TaxDeductedCollectedAtSource;

        public override decimal CalculateTotalIncome()
        {
            return TotalSalaryIncome + 
                   Math.Max(0, TotalHousePropertyIncome) + 
                   TotalCapitalGains + 
                   TotalOtherIncome;
        }

        public override decimal CalculateTaxLiability()
        {
            var totalIncome = CalculateTotalIncome();
            var taxableIncome = Math.Max(0, totalIncome - StandardDeduction - ProfessionalTax);
            
            return CalculateNewRegimeTax(taxableIncome);
        }

        public override decimal CalculateRefundOrDemand()
        {
            return TotalTaxPaid - CalculateTaxLiability();
        }

        public override bool ValidateData()
        {
            ValidationErrors.Clear();
            
            // Basic validations
            if (string.IsNullOrEmpty(PAN) || PAN.Length != 10)
                ValidationErrors.Add("Valid PAN is required");
            
            if (string.IsNullOrEmpty(Name))
                ValidationErrors.Add("Name is required");
            
            // Category-specific validations
            if (Category == TaxpayerCategory.HUF && string.IsNullOrEmpty(HUFName))
                ValidationErrors.Add("HUF name is required for HUF category");
            
            // Foreign income/asset validations
            if (HasForeignIncome && ForeignIncome == 0)
                ValidationErrors.Add("Foreign income amount required when foreign income is declared");
            
            if (HasForeignAssets && ForeignAssets.Count == 0)
                ValidationErrors.Add("Foreign asset details required when foreign assets are declared");
            
            // Validate TDS details
            var totalTDS = TDSDetails.Sum(t => t.TaxDeducted);
            if (Math.Abs(TaxDeductedAtSource - totalTDS) > 1)
                ValidationErrors.Add("TDS amount doesn't match TDS details breakdown");
            
            // Bank details for refund
            if (CalculateRefundOrDemand() > 0)
            {
                if (string.IsNullOrEmpty(BankAccountNumber))
                    ValidationErrors.Add("Bank account required for refund");
                if (string.IsNullOrEmpty(BankIFSCCode))
                    ValidationErrors.Add("Bank IFSC required for refund");
            }
            
            return ValidationErrors.Count == 0;
        }

        public override string GetFormName() => "ITR-2";
        public override string GetFormVersion() => "Ver 1.0 (AY 2024-25)";
        
        private decimal CalculateNewRegimeTax(decimal taxableIncome)
        {
            decimal tax = 0;
            
            // New Tax Regime slabs for AY 2024-25
            if (taxableIncome <= 300000) tax = 0;
            else if (taxableIncome <= 600000) tax = (taxableIncome - 300000) * 0.05m;
            else if (taxableIncome <= 900000) tax = 15000 + (taxableIncome - 600000) * 0.10m;
            else if (taxableIncome <= 1200000) tax = 45000 + (taxableIncome - 900000) * 0.15m;
            else if (taxableIncome <= 1500000) tax = 90000 + (taxableIncome - 1200000) * 0.20m;
            else tax = 150000 + (taxableIncome - 1500000) * 0.30m;
            
            // Add 4% Health and Education Cess
            tax += tax * 0.04m;
            
            return Math.Round(tax, 0);
        }
    }

    // Supporting data models for ITR-2
    public class SalaryDetails
    {
        public string EmployerName { get; set; } = string.Empty;
        public string EmployerTAN { get; set; } = string.Empty;
        public decimal GrossSalary { get; set; }
        public decimal TaxDeducted { get; set; }
        public string CertificateNumber { get; set; } = string.Empty;
    }

    public class HousePropertyDetails
    {
        public string PropertyAddress { get; set; } = string.Empty;
        public decimal AnnualValue { get; set; }
        public decimal PropertyTax { get; set; }
        public decimal InterestOnLoan { get; set; }
        public decimal NetIncome => AnnualValue - PropertyTax - (AnnualValue * 0.3m) - InterestOnLoan;
    }

    public class CapitalGainDetails
    {
        public string AssetType { get; set; } = string.Empty;
        public DateTime DateOfSale { get; set; }
        public DateTime DateOfPurchase { get; set; }
        public decimal SalePrice { get; set; }
        public decimal CostOfAcquisition { get; set; }
        public decimal CostOfImprovement { get; set; }
        public decimal ExpensesOnTransfer { get; set; }
        public bool IsLongTerm => (DateOfSale - DateOfPurchase).TotalDays > 365;
        public decimal NetGain => SalePrice - CostOfAcquisition - CostOfImprovement - ExpensesOnTransfer;
    }

    public class ForeignAssetDetails
    {
        public string AssetType { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public string Currency { get; set; } = string.Empty;
    }

    public class TDSDetails
    {
        public string DeductorName { get; set; } = string.Empty;
        public string DeductorTAN { get; set; } = string.Empty;
        public decimal TaxDeducted { get; set; }
        public string CertificateNumber { get; set; } = string.Empty;
        public DateTime DateOfDeduction { get; set; }
    }
}
