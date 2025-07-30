namespace ReturnlyWebApi.Models;

public enum TaxRegime
{
    Old,
    New
}

public enum NotificationType
{
    Info,
    Success,
    Warning,
    Error
}

public class TaxCalculationResult
{
    public decimal TaxableIncome { get; set; }
    public string FinancialYear { get; set; } = string.Empty;
    public TaxRegime TaxRegime { get; set; }
    public int Age { get; set; }
    public List<TaxSlabCalculation> TaxBreakdown { get; set; } = new();
    public decimal TotalTax { get; set; }
    public decimal Surcharge { get; set; }
    public decimal SurchargeRate { get; set; }
    public decimal TotalTaxWithSurcharge { get; set; }
    public decimal HealthAndEducationCess { get; set; }
    public decimal TotalTaxWithCess { get; set; }
    public decimal EffectiveTaxRate { get; set; }
}

public class TaxSlabCalculation
{
    public string SlabDescription { get; set; } = string.Empty;
    public decimal IncomeInSlab { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal MinIncome { get; set; }
    public decimal? MaxIncome { get; set; }
}

public class TaxRefundCalculation
{
    public decimal TotalTaxLiability { get; set; }
    public decimal TDSDeducted { get; set; }
    public decimal AdvanceTaxPaid { get; set; }
    public decimal SelfAssessmentTaxPaid { get; set; }
    public decimal RefundAmount { get; set; }
    public decimal AdditionalTaxDue { get; set; }
    public bool IsRefundDue { get; set; }
    
    // Advance tax penalty calculations
    public AdvanceTaxPenaltyCalculation? AdvanceTaxPenalties { get; set; }
    
    public decimal TotalTaxPaid => TDSDeducted + AdvanceTaxPaid + SelfAssessmentTaxPaid;
}

public class RegimeComparisonResult
{
    public TaxCalculationResult OldRegimeCalculation { get; set; } = new();
    public TaxCalculationResult NewRegimeCalculation { get; set; } = new();
    public TaxRegime RecommendedRegime { get; set; }
    public decimal TaxSavings { get; set; }
    public string ComparisonSummary { get; set; } = string.Empty;
}

/// <summary>
/// Advance Tax Penalty Calculation Model
/// </summary>
public class AdvanceTaxPenaltyCalculation
{
    // Input data for calculations
    public decimal TotalTaxLiability { get; set; }
    public decimal TDSDeducted { get; set; }
    public AdvanceTaxInstallments AdvanceTaxPaid { get; set; } = new();
    public DateTime FilingDate { get; set; }
    public string FinancialYear { get; set; } = string.Empty;
    
    // Calculated penalties
    public decimal Section234AInterest { get; set; }  // Default in Payment of Advance Tax
    public decimal Section234BInterest { get; set; }  // Failure to Pay Advance Tax
    public decimal Section234CInterest { get; set; }  // Deferment of Advance Tax
    public decimal TotalPenalties => Section234AInterest + Section234BInterest + Section234CInterest;
    
    // Breakdown details
    public List<AdvanceTaxPenaltyDetail> PenaltyDetails { get; set; } = new();
}

/// <summary>
/// Advance Tax Installments
/// </summary>
public class AdvanceTaxInstallments
{
    public decimal FirstInstallment { get; set; }    // By 15th June - 15%
    public decimal SecondInstallment { get; set; }   // By 15th September - 45%
    public decimal ThirdInstallment { get; set; }    // By 15th December - 75%
    public decimal FourthInstallment { get; set; }   // By 15th March - 100%
    
    public DateTime? FirstInstallmentDate { get; set; }
    public DateTime? SecondInstallmentDate { get; set; }
    public DateTime? ThirdInstallmentDate { get; set; }
    public DateTime? FourthInstallmentDate { get; set; }
    
    public decimal TotalAdvanceTaxPaid => FirstInstallment + SecondInstallment + ThirdInstallment + FourthInstallment;
}

/// <summary>
/// Penalty Detail for each installment
/// </summary>
public class AdvanceTaxPenaltyDetail
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
