using System.ComponentModel.DataAnnotations;

namespace ReturnlyWebApi.DTOs;

public class TaxCalculationRequestDto
{
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Taxable income must be a positive value")]
    public decimal TaxableIncome { get; set; }
    
    [Required]
    public string FinancialYear { get; set; } = string.Empty;
    
    [Range(0, double.MaxValue, ErrorMessage = "TDS deducted must be a positive value")]
    public decimal TdsDeducted { get; set; }
    
    [Range(18, 120, ErrorMessage = "Age must be between 18 and 120")]
    public int Age { get; set; } = 30;
    
    public string TaxRegime { get; set; } = "New";
    
    // Optional advance tax information
    public AdvanceTaxInstallmentsDto? AdvanceTaxPaid { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Self assessment tax must be a positive value")]
    public decimal SelfAssessmentTaxPaid { get; set; } = 0;
    
    public DateTime? FilingDate { get; set; }
}

public class TaxCalculationResponseDto
{
    public TaxCalculationResultDto TaxCalculation { get; set; } = new();
    public TaxRefundCalculationDto RefundCalculation { get; set; } = new();
    
    // Surface advance tax penalties for easy access in UI
    public decimal Section234AInterest { get; set; }  // Default in Payment of Advance Tax
    public decimal Section234BInterest { get; set; }  // Failure to Pay Advance Tax  
    public decimal Section234CInterest { get; set; }  // Deferment of Advance Tax
    public decimal TotalAdvanceTaxPenalties { get; set; }
    public bool HasAdvanceTaxPenalties { get; set; }
    
    // Detailed penalty breakdown (optional, for detailed views)
    public AdvanceTaxPenaltyCalculationDto? AdvanceTaxPenaltyDetails { get; set; }
}

public class TaxCalculationResultDto
{
    public decimal TaxableIncome { get; set; }
    public string FinancialYear { get; set; } = string.Empty;
    public string TaxRegime { get; set; } = string.Empty;
    public int Age { get; set; }
    public List<TaxSlabCalculationDto> TaxBreakdown { get; set; } = new();
    public decimal TotalTax { get; set; }
    public decimal Surcharge { get; set; }
    public decimal SurchargeRate { get; set; }
    public decimal TotalTaxWithSurcharge { get; set; }
    public decimal HealthAndEducationCess { get; set; }
    public decimal TotalTaxWithCess { get; set; }
    public decimal EffectiveTaxRate { get; set; }
    
    // Advance tax penalties (part of tax liability)
    public decimal Section234AInterest { get; set; }  // Default in Payment of Advance Tax
    public decimal Section234BInterest { get; set; }  // Failure to Pay Advance Tax
    public decimal Section234CInterest { get; set; }  // Deferment of Advance Tax
    public decimal TotalAdvanceTaxPenalties { get; set; }
    public decimal TotalTaxLiabilityWithPenalties { get; set; }  // TotalTaxWithCess + penalties
}

public class TaxSlabCalculationDto
{
    public string SlabDescription { get; set; } = string.Empty;
    public decimal IncomeInSlab { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal MinIncome { get; set; }
    public decimal? MaxIncome { get; set; }
}

public class TaxRefundCalculationDto
{
    public decimal TotalTaxLiability { get; set; }
    public decimal TDSDeducted { get; set; }
    public decimal AdvanceTaxPaid { get; set; }
    public decimal SelfAssessmentTaxPaid { get; set; }
    public decimal RefundAmount { get; set; }
    public decimal AdditionalTaxDue { get; set; }
    public bool IsRefundDue { get; set; }
    public decimal TotalTaxPaid { get; set; }
}

public class RegimeComparisonRequestDto
{
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Taxable income must be a positive value")]
    public decimal TaxableIncome { get; set; }
    
    [Required]
    public string FinancialYear { get; set; } = string.Empty;
    
    [Range(18, 120, ErrorMessage = "Age must be between 18 and 120")]
    public int Age { get; set; } = 30;
    
    [Range(0, double.MaxValue, ErrorMessage = "Old regime deductions must be a positive value")]
    public decimal OldRegimeDeductions { get; set; } = 0;
}

public class RegimeComparisonResponseDto
{
    public TaxCalculationResultDto OldRegimeCalculation { get; set; } = new();
    public TaxCalculationResultDto NewRegimeCalculation { get; set; } = new();
    public string RecommendedRegime { get; set; } = string.Empty;
    public decimal TaxSavings { get; set; }
    public string ComparisonSummary { get; set; } = string.Empty;
}

/// <summary>
/// Advance Tax Installments DTO
/// </summary>
public class AdvanceTaxInstallmentsDto
{
    [Range(0, double.MaxValue, ErrorMessage = "First installment must be a positive value")]
    public decimal FirstInstallment { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Second installment must be a positive value")]
    public decimal SecondInstallment { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Third installment must be a positive value")]
    public decimal ThirdInstallment { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Fourth installment must be a positive value")]
    public decimal FourthInstallment { get; set; }
    
    public DateTime? FirstInstallmentDate { get; set; }
    public DateTime? SecondInstallmentDate { get; set; }
    public DateTime? ThirdInstallmentDate { get; set; }
    public DateTime? FourthInstallmentDate { get; set; }
    
    public decimal TotalAdvanceTaxPaid => FirstInstallment + SecondInstallment + ThirdInstallment + FourthInstallment;
}

/// <summary>
/// Advance Tax Penalty Calculation DTO
/// </summary>
public class AdvanceTaxPenaltyCalculationDto
{
    public decimal TotalTaxLiability { get; set; }
    public decimal TDSDeducted { get; set; }
    public AdvanceTaxInstallmentsDto AdvanceTaxPaid { get; set; } = new();
    public DateTime FilingDate { get; set; }
    public string FinancialYear { get; set; } = string.Empty;
    
    // Calculated penalties
    public decimal Section234AInterest { get; set; }  // Default in Payment of Advance Tax
    public decimal Section234BInterest { get; set; }  // Failure to Pay Advance Tax
    public decimal Section234CInterest { get; set; }  // Deferment of Advance Tax
    public decimal TotalPenalties { get; set; }
    
    // Breakdown details
    public List<AdvanceTaxPenaltyDetailDto> PenaltyDetails { get; set; } = new();
}

/// <summary>
/// Penalty Detail DTO for each installment
/// </summary>
public class AdvanceTaxPenaltyDetailDto
{
    public string InstallmentPeriod { get; set; } = string.Empty;
    public decimal RequiredAmount { get; set; }
    public decimal ActualAmount { get; set; }
    public decimal Shortfall { get; set; }
    public decimal InterestRate { get; set; }
    public int InterestDays { get; set; }
    public decimal InterestAmount { get; set; }
    public string PenaltySection { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
