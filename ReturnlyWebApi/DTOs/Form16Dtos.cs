using System.ComponentModel.DataAnnotations;

namespace ReturnlyWebApi.DTOs;

public class Form16UploadDto
{
    [Required]
    public IFormFile PdfFile { get; set; } = null!;
    
    public string? Password { get; set; }
}

public class Form16DataDto
{
    public string EmployeeName { get; set; } = string.Empty;
    public string PAN { get; set; } = string.Empty;
    public string AssessmentYear { get; set; } = string.Empty;
    public string FinancialYear { get; set; } = string.Empty;
    public string EmployerName { get; set; } = string.Empty;
    public string TAN { get; set; } = string.Empty;
    
    // Additional employer details
    public string EmployerCategory { get; set; } = string.Empty;
    public string EmployerPinCode { get; set; } = string.Empty;
    public string EmployerAddress { get; set; } = string.Empty;
    public string EmployerCountry { get; set; } = "India";
    public string EmployerState { get; set; } = string.Empty;
    public string EmployerCity { get; set; } = string.Empty;
    public decimal GrossSalary { get; set; }
    public decimal TotalTaxDeducted { get; set; }
    public decimal StandardDeduction { get; set; } = 75000;
    public decimal ProfessionalTax { get; set; }
    public Form16BDataDto Form16B { get; set; } = new();
    public AnnexureDataDto Annexure { get; set; } = new();
    
    // Business income fields (for manual tax data input)
    public decimal IntradayTradingIncome { get; set; } = 0;
    public decimal TradingBusinessExpenses { get; set; } = 0;
    public decimal ProfessionalIncome { get; set; } = 0;
    public decimal ProfessionalExpenses { get; set; } = 0;
    public decimal BusinessIncomeSmall { get; set; } = 0;
    public decimal BusinessExpensesSmall { get; set; } = 0;
    public decimal LargeBusinessIncome { get; set; } = 0;
    public decimal LargeBusinessExpenses { get; set; } = 0;
    public decimal OtherBusinessIncome { get; set; } = 0;
    public decimal BusinessExpenses { get; set; } = 0;
    
    // Capital Gains fields
    public decimal StocksSTCG { get; set; } = 0;
    public decimal StocksLTCG { get; set; } = 0;
    public decimal MutualFundsSTCG { get; set; } = 0;
    public decimal MutualFundsLTCG { get; set; } = 0;
    public decimal FnoGains { get; set; } = 0;
    public decimal RealEstateSTCG { get; set; } = 0;
    public decimal RealEstateLTCG { get; set; } = 0;
    public decimal BondsSTCG { get; set; } = 0;
    public decimal BondsLTCG { get; set; } = 0;
    public decimal GoldSTCG { get; set; } = 0;
    public decimal GoldLTCG { get; set; } = 0;
    public decimal CryptoGains { get; set; } = 0;
    public decimal UsStocksSTCG { get; set; } = 0;
    public decimal UsStocksLTCG { get; set; } = 0;
    public decimal OtherForeignAssetsGains { get; set; } = 0;
    public decimal RsuGains { get; set; } = 0;
    public decimal EsopGains { get; set; } = 0;
    public decimal EsspGains { get; set; } = 0;
    
    // Financial Particulars
    public bool IsPresumptiveTaxation { get; set; } = false;
    public decimal PresumptiveIncomeRate { get; set; } = 8;
    public decimal TotalTurnover { get; set; } = 0;
    public bool RequiresAudit { get; set; } = false;
    public string AuditorName { get; set; } = string.Empty;
    public string AuditReportDate { get; set; } = string.Empty;
    
    // Financial Statements & Disclosures
    public decimal TotalAssets { get; set; } = 0;
    public decimal TotalLiabilities { get; set; } = 0;
    public decimal GrossProfit { get; set; } = 0;
    public decimal NetProfit { get; set; } = 0;
    public bool MaintainsBooksOfAccounts { get; set; } = false;
    public bool HasQuantitativeDetails { get; set; } = false;
    public string QuantitativeDetails { get; set; } = string.Empty;
}

public class Form16BDataDto
{
    // Salary Information
    public decimal SalarySection17 { get; set; }
    public decimal Perquisites { get; set; }
    public decimal ProfitsInLieu { get; set; }
    
    // Salary Breakdown
    public decimal BasicSalary { get; set; }
    public decimal HRA { get; set; }
    public decimal SpecialAllowance { get; set; }
    public decimal OtherAllowances { get; set; }
    
    // Interest Income
    public decimal InterestOnSavings { get; set; }
    public decimal InterestOnFixedDeposits { get; set; }
    public decimal InterestOnBonds { get; set; }
    public decimal OtherInterestIncome { get; set; }
    
    // Dividend Income
    public decimal DividendIncomeAI { get; set; }
    public decimal DividendIncomeAII { get; set; }
    public decimal OtherDividendIncome { get; set; }
    
    // Deductions
    public decimal StandardDeduction { get; set; } = 75000;
    public decimal ProfessionalTax { get; set; }
    public decimal TaxableIncome { get; set; }
}

public class AnnexureDataDto
{
    public decimal Q1TDS { get; set; }
    public decimal Q2TDS { get; set; }
    public decimal Q3TDS { get; set; }
    public decimal Q4TDS { get; set; }
}

public class UpdateTaxDataRequestDto
{
    [Required]
    public Form16DataDto Form16Data { get; set; } = new();
    
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Taxable income must be a positive value")]
    public decimal TaxableIncome { get; set; }
}
