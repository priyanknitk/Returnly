using System;
using System.Collections.Generic;
using Returnly.Models;

namespace Returnly.Services
{
    public class TaxConfigurationService
    {
        private readonly Dictionary<string, TaxConfiguration> _taxConfigurations;

        public TaxConfigurationService()
        {
            _taxConfigurations = InitializeTaxConfigurations();
        }

        public TaxConfiguration GetTaxConfiguration(string financialYear)
        {
            if (_taxConfigurations.TryGetValue(financialYear, out var config))
            {
                return config;
            }

            // Fallback to latest available configuration
            var currentYear = GetCurrentFinancialYear();
            if (_taxConfigurations.TryGetValue(currentYear, out var fallbackConfig))
            {
                return fallbackConfig;
            }

            // Return default configuration
            return GetDefaultTaxConfiguration();
        }

        private Dictionary<string, TaxConfiguration> InitializeTaxConfigurations()
        {
            var configurations = new Dictionary<string, TaxConfiguration>();

            // Financial Year 2023-24 (Assessment Year 2024-25)
            configurations["2023-24"] = new TaxConfiguration
            {
                FinancialYear = "2023-24",
                StandardDeduction = 50000,  // ₹50,000 for FY 2023-24
                ProfessionalTaxLimit = 2500, // Typical professional tax limit
                BasicExemptionLimit = 250000,
                SeniorCitizenExemptionLimit = 300000,
                SuperSeniorCitizenExemptionLimit = 500000
            };

            // Financial Year 2024-25 (Assessment Year 2025-26)
            configurations["2024-25"] = new TaxConfiguration
            {
                FinancialYear = "2024-25",
                StandardDeduction = 75000,  // ₹75,000 for FY 2024-25 onwards
                ProfessionalTaxLimit = 2500,
                BasicExemptionLimit = 250000,
                SeniorCitizenExemptionLimit = 300000,
                SuperSeniorCitizenExemptionLimit = 500000
            };

            // Financial Year 2025-26 (Assessment Year 2026-27)
            configurations["2025-26"] = new TaxConfiguration
            {
                FinancialYear = "2025-26",
                StandardDeduction = 75000,  // ₹75,000 continues for FY 2025-26
                ProfessionalTaxLimit = 2500,
                BasicExemptionLimit = 250000,
                SeniorCitizenExemptionLimit = 300000,
                SuperSeniorCitizenExemptionLimit = 500000
            };

            return configurations;
        }

        private string GetCurrentFinancialYear()
        {
            var now = DateTime.Now;
            var currentYear = now.Year;
            var currentMonth = now.Month;
            
            if (currentMonth >= 4) // April onwards is new financial year
            {
                return $"{currentYear}-{(currentYear + 1).ToString().Substring(2)}";
            }
            else
            {
                return $"{currentYear - 1}-{currentYear.ToString().Substring(2)}";
            }
        }

        private TaxConfiguration GetDefaultTaxConfiguration()
        {
            return new TaxConfiguration
            {
                FinancialYear = "2024-25",
                StandardDeduction = 75000,
                ProfessionalTaxLimit = 2500,
                BasicExemptionLimit = 250000,
                SeniorCitizenExemptionLimit = 300000,
                SuperSeniorCitizenExemptionLimit = 500000
            };
        }

        /// <summary>
        /// Get all available financial years
        /// </summary>
        public IEnumerable<string> GetAvailableFinancialYears()
        {
            return _taxConfigurations.Keys;
        }
    }
}
