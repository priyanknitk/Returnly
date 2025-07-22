using System;

namespace Returnly.Models
{
    /// <summary>
    /// Tax configuration for a specific financial year including various limits and deductions
    /// </summary>
    public class TaxConfiguration
    {
        public string FinancialYear { get; set; } = string.Empty;
        public decimal StandardDeduction { get; set; }
        public decimal ProfessionalTaxLimit { get; set; }
        public decimal BasicExemptionLimit { get; set; }
        public decimal SeniorCitizenExemptionLimit { get; set; }
        public decimal SuperSeniorCitizenExemptionLimit { get; set; }
        
        // Surcharge thresholds for New Tax Regime
        public decimal SurchargeThreshold1 { get; set; } = 5000000;   // ₹50 Lakh
        public decimal SurchargeThreshold2 { get; set; } = 10000000;  // ₹1 Crore
        public decimal SurchargeThreshold3 { get; set; } = 20000000;  // ₹2 Crore
        
        // Surcharge rates for New Tax Regime
        public decimal SurchargeRate1 { get; set; } = 10;   // 10% for ₹50L-₹1Cr
        public decimal SurchargeRate2 { get; set; } = 15;   // 15% for ₹1Cr-₹2Cr
        public decimal SurchargeRate3 { get; set; } = 25;   // 25% for above ₹2Cr (capped)
        
        // Health and Education Cess
        public decimal HealthAndEducationCessRate { get; set; } = 4; // 4%
    }
}
