using System;

namespace Returnly.Models
{
    public class RegimeComparisonResult
    {
        public TaxCalculationResult OldRegimeCalculation { get; set; } = new();
        public TaxCalculationResult NewRegimeCalculation { get; set; } = new();
        public decimal OldRegimeDeductions { get; set; }
        public decimal TaxSavings { get; set; }
        public TaxRegime RecommendedRegime { get; set; }
        public decimal SavingsPercentage { get; set; }
    }
}
