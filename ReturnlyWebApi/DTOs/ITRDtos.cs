using ReturnlyWebApi.Models;
using System.ComponentModel.DataAnnotations;

namespace ReturnlyWebApi.DTOs;

/// <summary>
/// DTO for ITR form generation request
/// </summary>
public class ITRGenerationRequestDto
{
    [Required]
    public Form16DataDto Form16Data { get; set; } = new();
    
    [Required]
    public AdditionalTaxpayerInfoDto AdditionalInfo { get; set; } = new();
    
    public ITRType? PreferredITRType { get; set; }
}

/// <summary>
/// DTO for additional taxpayer information
/// </summary>
public class AdditionalTaxpayerInfoDto
{
    [Required]
    public DateTime DateOfBirth { get; set; }
    
    [Required]
    [MaxLength(500)]
    public string Address { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string City { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string State { get; set; } = string.Empty;
    
    [Required]
    [RegularExpression(@"^\d{6}$")]
    public string Pincode { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    public string EmailAddress { get; set; } = string.Empty;
    
    [Required]
    [RegularExpression(@"^\d{10}$")]
    public string MobileNumber { get; set; } = string.Empty;
    
    [RegularExpression(@"^\d{12}$")]
    public string AadhaarNumber { get; set; } = string.Empty;
    
    // Bank details for refund
    [Required]
    public string BankAccountNumber { get; set; } = string.Empty;
    
    [Required]
    [RegularExpression(@"^[A-Z]{4}0[A-Z0-9]{6}$")]
    public string BankIFSCCode { get; set; } = string.Empty;
    
    [Required]
    public string BankName { get; set; } = string.Empty;
    
    // Additional income sources
    public bool HasHouseProperty { get; set; }
    public List<HousePropertyDetailsDto> HouseProperties { get; set; } = new();
    
    public bool HasCapitalGains { get; set; }
    public List<CapitalGainDetailsDto> CapitalGains { get; set; } = new();
    
    public bool HasOtherIncome { get; set; }
    public decimal OtherInterestIncome { get; set; }
    public decimal OtherDividendIncome { get; set; }
    public decimal OtherSourcesIncome { get; set; }
    
    public bool HasForeignIncome { get; set; }
    public decimal ForeignIncome { get; set; }
    
    public bool HasForeignAssets { get; set; }
    public List<ForeignAssetDetailsDto> ForeignAssets { get; set; } = new();
}

/// <summary>
/// DTO for house property details
/// </summary>
public class HousePropertyDetailsDto
{
    [Required]
    [MaxLength(500)]
    public string PropertyAddress { get; set; } = string.Empty;
    
    [Range(0, double.MaxValue)]
    public decimal AnnualValue { get; set; }
    
    [Range(0, double.MaxValue)]
    public decimal PropertyTax { get; set; }
    
    [Range(0, double.MaxValue)]
    public decimal InterestOnLoan { get; set; }
}

/// <summary>
/// DTO for capital gain details
/// </summary>
public class CapitalGainDetailsDto
{
    [Required]
    [MaxLength(100)]
    public string AssetType { get; set; } = string.Empty;
    
    [Required]
    public DateTime DateOfSale { get; set; }
    
    [Required]
    public DateTime DateOfPurchase { get; set; }
    
    [Range(0, double.MaxValue)]
    public decimal SalePrice { get; set; }
    
    [Range(0, double.MaxValue)]
    public decimal CostOfAcquisition { get; set; }
    
    [Range(0, double.MaxValue)]
    public decimal CostOfImprovement { get; set; }
    
    [Range(0, double.MaxValue)]
    public decimal ExpensesOnTransfer { get; set; }
}

/// <summary>
/// DTO for foreign asset details
/// </summary>
public class ForeignAssetDetailsDto
{
    [Required]
    [MaxLength(100)]
    public string AssetType { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string Country { get; set; } = string.Empty;
    
    [Range(0, double.MaxValue)]
    public decimal Value { get; set; }
    
    [Required]
    [MaxLength(10)]
    public string Currency { get; set; } = string.Empty;
}

/// <summary>
/// DTO for ITR form generation response
/// </summary>
public class ITRGenerationResponseDto
{
    public bool IsSuccess { get; set; }
    public string RecommendedITRType { get; set; } = string.Empty;
    public string ITRFormXml { get; set; } = string.Empty;
    public string ITRFormJson { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public List<string> ValidationErrors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public string GenerationSummary { get; set; } = string.Empty;
    public ITRFormDataDto? FormData { get; set; }
}

/// <summary>
/// DTO for ITR form data (can be ITR-1 or ITR-2)
/// </summary>
public class ITRFormDataDto
{
    public string FormType { get; set; } = string.Empty;
    public string AssessmentYear { get; set; } = string.Empty;
    public string FinancialYear { get; set; } = string.Empty;
    
    // Personal Information
    public string PAN { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string ResidencyStatus { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Pincode { get; set; } = string.Empty;
    public string EmailAddress { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public string AadhaarNumber { get; set; } = string.Empty;
    
    // Bank Details
    public string BankAccountNumber { get; set; } = string.Empty;
    public string BankIFSCCode { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    
    // Income Summary
    public decimal TotalIncome { get; set; }
    public decimal TaxLiability { get; set; }
    public decimal RefundOrDemand { get; set; }
    public bool IsRefundDue { get; set; }
    
    // Tax Payment Details
    public decimal TaxDeductedAtSource { get; set; }
    public decimal AdvanceTax { get; set; }
    public decimal SelfAssessmentTax { get; set; }
    
    // ITR-1 specific fields (null for ITR-2)
    public ITR1SpecificDataDto? ITR1Data { get; set; }
    
    // ITR-2 specific fields (null for ITR-1)
    public ITR2SpecificDataDto? ITR2Data { get; set; }
}

/// <summary>
/// DTO for ITR-1 specific data
/// </summary>
public class ITR1SpecificDataDto
{
    // Employer Details
    public string EmployerName { get; set; } = string.Empty;
    public string EmployerTAN { get; set; } = string.Empty;
    public string EmployerAddress { get; set; } = string.Empty;
    
    // Income breakdown
    public decimal SalaryIncome { get; set; }
    public decimal HousePropertyIncome { get; set; }
    public decimal OtherSourcesIncome { get; set; }
    
    // Deductions
    public decimal StandardDeduction { get; set; }
    public decimal ProfessionalTax { get; set; }
    
    // Quarterly TDS
    public decimal Q1TDS { get; set; }
    public decimal Q2TDS { get; set; }
    public decimal Q3TDS { get; set; }
    public decimal Q4TDS { get; set; }
}

/// <summary>
/// DTO for ITR-2 specific data
/// </summary>
public class ITR2SpecificDataDto
{
    public string TaxpayerCategory { get; set; } = string.Empty;
    public string HUFName { get; set; } = string.Empty;
    
    // Income breakdown
    public decimal SalaryIncome { get; set; }
    public decimal HousePropertyIncome { get; set; }
    public decimal CapitalGainsIncome { get; set; }
    public decimal OtherSourcesIncome { get; set; }
    public decimal ForeignIncome { get; set; }
    
    // Multiple sources
    public List<SalaryDetailsDto> SalaryDetails { get; set; } = new();
    public List<HousePropertyDetailsDto> HouseProperties { get; set; } = new();
    public List<CapitalGainDetailsDto> CapitalGains { get; set; } = new();
    public List<ForeignAssetDetailsDto> ForeignAssets { get; set; } = new();
    public List<TDSDetailsDto> TDSDetails { get; set; } = new();
    
    // Flags
    public bool HasForeignIncome { get; set; }
    public bool HasForeignAssets { get; set; }
}

/// <summary>
/// DTO for salary details in ITR-2
/// </summary>
public class SalaryDetailsDto
{
    public string EmployerName { get; set; } = string.Empty;
    public string EmployerTAN { get; set; } = string.Empty;
    public decimal GrossSalary { get; set; }
    public decimal TaxDeducted { get; set; }
    public string CertificateNumber { get; set; } = string.Empty;
}

/// <summary>
/// DTO for TDS details
/// </summary>
public class TDSDetailsDto
{
    public string DeductorName { get; set; } = string.Empty;
    public string DeductorTAN { get; set; } = string.Empty;
    public decimal TaxDeducted { get; set; }
    public string CertificateNumber { get; set; } = string.Empty;
    public DateTime DateOfDeduction { get; set; }
}

/// <summary>
/// DTO for ITR type recommendation request
/// </summary>
public class ITRRecommendationRequestDto
{
    [Required]
    public Form16DataDto Form16Data { get; set; } = new();
    
    public bool HasHouseProperty { get; set; }
    public bool HasCapitalGains { get; set; }
    public bool HasBusinessIncome { get; set; }
    public bool HasForeignIncome { get; set; }
    public bool HasForeignAssets { get; set; }
    public bool IsHUF { get; set; }
    public decimal TotalIncome { get; set; }
}

/// <summary>
/// DTO for ITR type recommendation response
/// </summary>
public class ITRRecommendationResponseDto
{
    public string RecommendedITRType { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public List<string> Requirements { get; set; } = new();
    public List<string> Limitations { get; set; } = new();
    public bool CanUseITR1 { get; set; }
    public bool CanUseITR2 { get; set; }
    public string RecommendationSummary { get; set; } = string.Empty;
}
