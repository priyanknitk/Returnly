using System;
using System.Collections.Generic;

namespace Returnly.Models
{
    public class TaxCalculationResult
    {
        public decimal TaxableIncome { get; set; }
        public string FinancialYear { get; set; } = string.Empty;
        public TaxRegime TaxRegime { get; set; }
        public int Age { get; set; }
        public decimal TotalTax { get; set; }
        public decimal Surcharge { get; set; }
        public decimal SurchargeRate { get; set; }
        public decimal TotalTaxWithSurcharge { get; set; }
        public decimal HealthAndEducationCess { get; set; }
        public decimal TotalTaxWithCess { get; set; }
        public decimal EffectiveTaxRate { get; set; }
        public List<TaxSlabCalculation> TaxBreakdown { get; set; } = [];
    }
}
