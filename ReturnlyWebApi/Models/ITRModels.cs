using System.ComponentModel.DataAnnotations;

namespace ReturnlyWebApi.Models;

/// <summary>
/// Enum for different ITR form types
/// </summary>
public enum ITRType
{
    ITR1 = 1,  // Sahaj - for salary income up to 50L
    ITR2 = 2,  // For individuals/HUF with capital gains, multiple properties
    ITR3 = 3,  // For business/professional income
    ITR4 = 4   // Sugam - for presumptive business income
}

/// <summary>
/// Enum for taxpayer category
/// </summary>
public enum TaxpayerCategory
{
    Individual,
    HUF // Hindu Undivided Family
}

/// <summary>
/// Enum for residency status
/// </summary>
public enum ResidencyStatus
{
    Resident,
    NonResident,
    ResidentNotOrdinaryResident
}

/// <summary>
/// Result of ITR form generation
/// </summary>
public class ITRFormGenerationResult
{
    public bool IsSuccess { get; set; }
    public ITRType RecommendedITRType { get; set; }
    public string ITRFormXml { get; set; } = string.Empty;
    public string ITRFormJson { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public List<string> ValidationErrors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public string GenerationSummary { get; set; } = string.Empty;
}

/// <summary>
/// Base class for all ITR form data
/// </summary>
public abstract class BaseITRData
{
    public string AssessmentYear { get; set; } = "2024-25";
    public string FinancialYear { get; set; } = "2023-24";
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
    
    public abstract decimal CalculateTotalIncome();
    public abstract decimal CalculateTaxLiability();
    public abstract decimal CalculateRefundOrDemand();
    public abstract bool ValidateData();
    public abstract string GetFormName();
}

/// <summary>
/// ITR-1 (Sahaj) form data model
/// </summary>
public class ITR1Data : BaseITRData
{
    // Employer Details
    public string EmployerName { get; set; } = string.Empty;
    public string EmployerTAN { get; set; } = string.Empty;
    public string EmployerAddress { get; set; } = string.Empty;
    
    // Salary Income
    public decimal GrossSalary { get; set; }
    public decimal AllowanceExempt { get; set; }
    public decimal AllowanceTaxable { get; set; }
    public decimal Perquisites { get; set; }
    public decimal ProfitsInLieu { get; set; }
    
    // House Property (only one allowed)
    public decimal AnnualValue { get; set; }
    public decimal PropertyTax { get; set; }
    public decimal InterestOnHomeLoan { get; set; }
    
    // Other Sources
    public decimal InterestFromSavingsAccount { get; set; }
    public decimal InterestFromDeposits { get; set; }
    public decimal DividendIncome { get; set; }
    public decimal OtherIncome { get; set; }
    
    // Deductions
    public decimal StandardDeduction { get; set; } = 75000; // AY 2024-25
    public decimal ProfessionalTax { get; set; }
    public decimal EntertainmentAllowance { get; set; }
    
    // Tax Details
    public decimal TaxDeductedAtSource { get; set; }
    public decimal AdvanceTax { get; set; }
    public decimal SelfAssessmentTax { get; set; }
    
    // Quarterly TDS
    public decimal Q1TDS { get; set; }
    public decimal Q2TDS { get; set; }
    public decimal Q3TDS { get; set; }
    public decimal Q4TDS { get; set; }
    
    // Calculated properties
    public decimal TotalSalaryIncome => GrossSalary + AllowanceTaxable + Perquisites + ProfitsInLieu - AllowanceExempt;
    public decimal TotalHousePropertyIncome => AnnualValue - PropertyTax - (AnnualValue * 0.3m) - InterestOnHomeLoan;
    public decimal TotalOtherIncome => InterestFromSavingsAccount + InterestFromDeposits + DividendIncome + OtherIncome;
    public decimal TotalDeductions => StandardDeduction + ProfessionalTax + EntertainmentAllowance;
    public decimal TotalTaxPaid => TaxDeductedAtSource + AdvanceTax + SelfAssessmentTax;
    
    public override decimal CalculateTotalIncome()
    {
        return TotalSalaryIncome + Math.Max(0, TotalHousePropertyIncome) + TotalOtherIncome;
    }
    
    public override decimal CalculateTaxLiability()
    {
        var totalIncome = CalculateTotalIncome();
        var taxableIncome = Math.Max(0, totalIncome - TotalDeductions);
        return CalculateNewRegimeTax(taxableIncome);
    }
    
    public override decimal CalculateRefundOrDemand()
    {
        return TotalTaxPaid - CalculateTaxLiability();
    }
    
    public override bool ValidateData()
    {
        if (string.IsNullOrEmpty(PAN) || PAN.Length != 10) return false;
        if (string.IsNullOrEmpty(Name)) return false;
        if (CalculateTotalIncome() > 5000000) return false; // 50L limit for ITR-1
        if (string.IsNullOrEmpty(EmployerTAN) || EmployerTAN.Length != 10) return false;
        
        var quarterlyTotal = Q1TDS + Q2TDS + Q3TDS + Q4TDS;
        if (Math.Abs(TaxDeductedAtSource - quarterlyTotal) > 1) return false;
        
        return true;
    }
    
    public override string GetFormName() => "ITR-1 (Sahaj)";
    
    private decimal CalculateNewRegimeTax(decimal taxableIncome)
    {
        decimal tax = 0;
        
        if (taxableIncome <= 300000) tax = 0;
        else if (taxableIncome <= 700000) tax = (taxableIncome - 300000) * 0.05m;
        else if (taxableIncome <= 1000000) tax = 20000 + (taxableIncome - 700000) * 0.10m;
        else if (taxableIncome <= 1200000) tax = 50000 + (taxableIncome - 1000000) * 0.15m;
        else if (taxableIncome <= 1500000) tax = 80000 + (taxableIncome - 1200000) * 0.20m;
        else tax = 140000 + (taxableIncome - 1500000) * 0.30m;
        
        // Add 4% Health and Education Cess
        tax += tax * 0.04m;
        
        return Math.Round(tax, 0);
    }
}

/// <summary>
/// ITR-2 form data model
/// </summary>
public class ITR2Data : BaseITRData
{
    public TaxpayerCategory Category { get; set; } = TaxpayerCategory.Individual;
    public string HUFName { get; set; } = string.Empty;
    
    // Multiple salary sources
    public List<SalaryDetails> SalaryDetails { get; set; } = new();
    
    // Multiple house properties
    public List<HousePropertyDetails> HouseProperties { get; set; } = new();
    
    // Capital gains
    public List<CapitalGainDetails> CapitalGains { get; set; } = new();
    
    // Other sources
    public decimal InterestIncome { get; set; }
    public decimal DividendIncome { get; set; }
    public decimal OtherSourcesIncome { get; set; }
    
    // Foreign income and assets
    public bool HasForeignIncome { get; set; }
    public decimal ForeignIncome { get; set; }
    public bool HasForeignAssets { get; set; }
    public List<ForeignAssetDetails> ForeignAssets { get; set; } = new();
    
    // Deductions
    public decimal StandardDeduction { get; set; } = 75000;
    public decimal ProfessionalTax { get; set; }
    
    // Tax details
    public decimal TaxDeductedAtSource { get; set; }
    public decimal AdvanceTax { get; set; }
    public decimal SelfAssessmentTax { get; set; }
    public List<TDSDetails> TDSDetails { get; set; } = new();
    
    // Calculated properties
    public decimal TotalSalaryIncome => SalaryDetails.Sum(s => s.GrossSalary);
    public decimal TotalHousePropertyIncome => HouseProperties.Sum(h => h.NetIncome);
    public decimal TotalCapitalGains => CapitalGains.Sum(c => c.NetGain);
    public decimal TotalOtherIncome => InterestIncome + DividendIncome + OtherSourcesIncome;
    public decimal TotalTaxPaid => TaxDeductedAtSource + AdvanceTax + SelfAssessmentTax;
    
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
        if (string.IsNullOrEmpty(PAN) || PAN.Length != 10) return false;
        if (string.IsNullOrEmpty(Name)) return false;
        
        if (Category == TaxpayerCategory.HUF && string.IsNullOrEmpty(HUFName)) return false;
        
        if (HasForeignIncome && ForeignIncome == 0) return false;
        if (HasForeignAssets && ForeignAssets.Count == 0) return false;
        
        var totalTDS = TDSDetails.Sum(t => t.TaxDeducted);
        if (Math.Abs(TaxDeductedAtSource - totalTDS) > 1) return false;
        
        return true;
    }
    
    public override string GetFormName() => "ITR-2";
    
    private decimal CalculateNewRegimeTax(decimal taxableIncome)
    {
        decimal tax = 0;
        
        if (taxableIncome <= 300000) tax = 0;
        else if (taxableIncome <= 700000) tax = (taxableIncome - 300000) * 0.05m;
        else if (taxableIncome <= 1000000) tax = 20000 + (taxableIncome - 700000) * 0.10m;
        else if (taxableIncome <= 1200000) tax = 50000 + (taxableIncome - 1000000) * 0.15m;
        else if (taxableIncome <= 1500000) tax = 80000 + (taxableIncome - 1200000) * 0.20m;
        else tax = 140000 + (taxableIncome - 1500000) * 0.30m;
        
        // Add 4% Health and Education Cess
        tax += tax * 0.04m;
        
        return Math.Round(tax, 0);
    }
}

/// <summary>
/// ITR-3 form data model for individuals and HUFs with business/professional income
/// </summary>
public class ITR3Data : BaseITRData
{
    public TaxpayerCategory Category { get; set; } = TaxpayerCategory.Individual;
    public string HUFName { get; set; } = string.Empty;
    
    // Business details
    public string BusinessName { get; set; } = string.Empty;
    public string BusinessAddress { get; set; } = string.Empty;
    public string NatureOfBusiness { get; set; } = string.Empty;
    public string AccountingMethod { get; set; } = "Cash"; // Cash or Mercantile
    public DateTime BusinessStartDate { get; set; }
    
    // Income sources
    public List<SalaryDetails> SalaryDetails { get; set; } = new();
    public List<HousePropertyDetails> HouseProperties { get; set; } = new();
    public List<BusinessIncomeDetails> BusinessIncomes { get; set; } = new();
    public List<CapitalGainDetails> CapitalGains { get; set; } = new();
    
    // Business expenses
    public List<BusinessExpenseDetails> BusinessExpenses { get; set; } = new();
    
    // Other income
    public decimal InterestIncome { get; set; }
    public decimal DividendIncome { get; set; }
    public decimal OtherSourcesIncome { get; set; }
    
    // Foreign income and assets
    public bool HasForeignIncome { get; set; }
    public decimal ForeignIncome { get; set; }
    public bool HasForeignAssets { get; set; }
    public List<ForeignAssetDetails> ForeignAssets { get; set; } = new();
    
    // Deductions
    public decimal StandardDeduction { get; set; } = 75000;
    public decimal ProfessionalTax { get; set; }
    
    // Tax details
    public decimal TaxDeductedAtSource { get; set; }
    public decimal AdvanceTax { get; set; }
    public decimal SelfAssessmentTax { get; set; }
    public List<TDSDetails> TDSDetails { get; set; } = new();
    
    // Audit details (if required)
    public bool RequiresAudit { get; set; }
    public string AuditorName { get; set; } = string.Empty;
    public string AuditorMembershipNumber { get; set; } = string.Empty;
    public DateTime AuditDate { get; set; }
    
    // Balance Sheet and P&L
    public BalanceSheetDetails BalanceSheet { get; set; } = new();
    public ProfitLossDetails ProfitLoss { get; set; } = new();
    
    // Calculated properties
    public decimal TotalSalaryIncome => SalaryDetails.Sum(s => s.GrossSalary);
    public decimal TotalHousePropertyIncome => HouseProperties.Sum(h => h.NetIncome);
    public decimal TotalBusinessIncome => BusinessIncomes.Sum(b => b.NetIncome);
    public decimal TotalCapitalGains => CapitalGains.Sum(c => c.NetGain);
    public decimal TotalOtherIncome => InterestIncome + DividendIncome + OtherSourcesIncome;
    public decimal TotalBusinessExpenses => BusinessExpenses.Sum(e => e.Amount);
    public decimal NetBusinessIncome => TotalBusinessIncome - TotalBusinessExpenses;
    public decimal TotalTaxPaid => TaxDeductedAtSource + AdvanceTax + SelfAssessmentTax;
    
    public override decimal CalculateTotalIncome()
    {
        return TotalSalaryIncome +
               Math.Max(0, TotalHousePropertyIncome) +
               Math.Max(0, NetBusinessIncome) +
               TotalCapitalGains +
               TotalOtherIncome +
               (HasForeignIncome ? ForeignIncome : 0);
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
        if (string.IsNullOrEmpty(PAN) || PAN.Length != 10) return false;
        if (string.IsNullOrEmpty(Name)) return false;
        
        if (Category == TaxpayerCategory.HUF && string.IsNullOrEmpty(HUFName)) return false;
        
        // Business validation
        if (BusinessIncomes.Any() && string.IsNullOrEmpty(BusinessName)) return false;
        if (BusinessIncomes.Any() && string.IsNullOrEmpty(NatureOfBusiness)) return false;
        
        // Audit validation
        if (RequiresAudit)
        {
            if (string.IsNullOrEmpty(AuditorName)) return false;
            if (string.IsNullOrEmpty(AuditorMembershipNumber)) return false;
        }
        
        if (HasForeignIncome && ForeignIncome == 0) return false;
        if (HasForeignAssets && ForeignAssets.Count == 0) return false;
        
        var totalTDS = TDSDetails.Sum(t => t.TaxDeducted);
        if (Math.Abs(TaxDeductedAtSource - totalTDS) > 1) return false;
        
        return true;
    }
    
    public override string GetFormName() => "ITR-3";
    
    private decimal CalculateNewRegimeTax(decimal taxableIncome)
    {
        decimal tax = 0;
        
        if (taxableIncome <= 300000) tax = 0;
        else if (taxableIncome <= 700000) tax = (taxableIncome - 300000) * 0.05m;
        else if (taxableIncome <= 1000000) tax = 20000 + (taxableIncome - 700000) * 0.10m;
        else if (taxableIncome <= 1200000) tax = 50000 + (taxableIncome - 1000000) * 0.15m;
        else if (taxableIncome <= 1500000) tax = 80000 + (taxableIncome - 1200000) * 0.20m;
        else tax = 140000 + (taxableIncome - 1500000) * 0.30m;
        
        // Add 4% Health and Education Cess
        tax += tax * 0.04m;
        
        return Math.Round(tax, 0);
    }
}

// Supporting classes for ITR-2
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

/// <summary>
/// Additional taxpayer information not available in Form16
/// </summary>
public class AdditionalTaxpayerInfo
{
    public DateTime DateOfBirth { get; set; }
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Pincode { get; set; } = string.Empty;
    public string EmailAddress { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public string AadhaarNumber { get; set; } = string.Empty;
    public string BankAccountNumber { get; set; } = string.Empty;
    public string BankIFSCCode { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    
    // Additional income sources
    public bool HasHouseProperty { get; set; }
    public List<HousePropertyDetails> HouseProperties { get; set; } = new();
    
    public bool HasCapitalGains { get; set; }
    public List<CapitalGainDetails> CapitalGains { get; set; } = new();
    
    public bool HasOtherIncome { get; set; }
    public decimal OtherInterestIncome { get; set; }
    public decimal OtherDividendIncome { get; set; }
    public decimal OtherSourcesIncome { get; set; }
    
    public bool HasForeignIncome { get; set; }
    public decimal ForeignIncome { get; set; }
    
    public bool HasForeignAssets { get; set; }
    public List<ForeignAssetDetails> ForeignAssets { get; set; } = new();
    
    // Business/Professional income
    public bool HasBusinessIncome { get; set; }
    public List<BusinessIncomeDetails> BusinessIncomes { get; set; } = new();
    public List<BusinessExpenseDetails> BusinessExpenses { get; set; } = new();
}

// Additional supporting classes for ITR-3
public class BusinessIncomeDetails
{
    public string IncomeType { get; set; } = string.Empty; // Trading, Services, Professional, etc.
    public string Description { get; set; } = string.Empty;
    public decimal GrossReceipts { get; set; }
    public decimal OtherIncome { get; set; }
    public decimal NetIncome => GrossReceipts + OtherIncome;
}

public class BusinessExpenseDetails
{
    public string ExpenseCategory { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public bool IsCapitalExpense { get; set; }
}

public class BalanceSheetDetails
{
    // Assets
    public decimal FixedAssets { get; set; }
    public decimal CurrentAssets { get; set; }
    public decimal Investments { get; set; }
    public decimal TotalAssets => FixedAssets + CurrentAssets + Investments;
    
    // Liabilities
    public decimal CapitalAccount { get; set; }
    public decimal CurrentLiabilities { get; set; }
    public decimal LongTermLiabilities { get; set; }
    public decimal TotalLiabilities => CapitalAccount + CurrentLiabilities + LongTermLiabilities;
}

public class ProfitLossDetails
{
    // Income
    public decimal TotalIncome { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal NetProfit => TotalIncome - TotalExpenses;
    
    // Depreciation
    public decimal DepreciationAmount { get; set; }
    
    // Other details
    public decimal TaxProvision { get; set; }
    public decimal ProfitAfterTax => NetProfit - TaxProvision;
}
