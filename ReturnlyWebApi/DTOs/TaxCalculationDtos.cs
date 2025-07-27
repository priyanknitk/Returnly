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
}

public class TaxCalculationResponseDto
{
    public TaxCalculationResultDto TaxCalculation { get; set; } = new();
    public TaxRefundCalculationDto RefundCalculation { get; set; } = new();
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
    public decimal RefundAmount { get; set; }
    public decimal AdditionalTaxDue { get; set; }
    public bool IsRefundDue { get; set; }
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
