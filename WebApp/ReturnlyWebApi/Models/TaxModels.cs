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
    public decimal RefundAmount { get; set; }
    public decimal AdditionalTaxDue { get; set; }
    public bool IsRefundDue { get; set; }
}

public class RegimeComparisonResult
{
    public TaxCalculationResult OldRegimeCalculation { get; set; } = new();
    public TaxCalculationResult NewRegimeCalculation { get; set; } = new();
    public TaxRegime RecommendedRegime { get; set; }
    public decimal TaxSavings { get; set; }
    public string ComparisonSummary { get; set; } = string.Empty;
}
