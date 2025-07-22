using System;
using System.Collections.Generic;
using System.Linq;
using Returnly.Models;

namespace Returnly.Services
{
    public class TaxSlabConfigurationService
    {
        private readonly Dictionary<string, List<TaxSlab>> _taxSlabsByYear;

        public TaxSlabConfigurationService()
        {
            _taxSlabsByYear = InitializeTaxSlabs();
        }

        public List<TaxSlab> GetTaxSlabs(string financialYear, TaxRegime regime, int age)
        {
            var key = $"{financialYear}_{regime}_{GetAgeCategory(age)}";
            
            if (_taxSlabsByYear.TryGetValue(key, out var slabs))
            {
                return slabs;
            }

            // Fallback to current year if specific year not found
            var currentYear = GetCurrentFinancialYear();
            var fallbackKey = $"{currentYear}_{regime}_{GetAgeCategory(age)}";
            
            return _taxSlabsByYear.TryGetValue(fallbackKey, out var fallbackSlabs) ? 
                fallbackSlabs : GetDefaultSlabs(regime, age);
        }

        private Dictionary<string, List<TaxSlab>> InitializeTaxSlabs()
        {
            var slabs = new Dictionary<string, List<TaxSlab>>();

            // Financial Year 2024-25 (Assessment Year 2025-26)
            InitializeFY2024_25(slabs);

            return slabs;
        }

        private void InitializeFY2024_25(Dictionary<string, List<TaxSlab>> slabs)
        {
            // New Tax Regime FY 2024-25 - Individual Below 60
            slabs["2024-25_New_Individual"] = new List<TaxSlab>
            {
                new TaxSlab { MinIncome = 0, MaxIncome = 300000, TaxRate = 0, Description = "Up to ₹3,00,000" },
                new TaxSlab { MinIncome = 300000, MaxIncome = 700000, TaxRate = 5, Description = "₹3,00,001 to ₹7,00,000" },
                new TaxSlab { MinIncome = 700000, MaxIncome = 1000000, TaxRate = 10, Description = "₹7,00,001 to ₹10,00,000" },
                new TaxSlab { MinIncome = 1000000, MaxIncome = 1200000, TaxRate = 15, Description = "₹10,00,001 to ₹12,00,000" },
                new TaxSlab { MinIncome = 1200000, MaxIncome = 1500000, TaxRate = 20, Description = "₹12,00,001 to ₹15,00,000" },
                new TaxSlab { MinIncome = 1500000, MaxIncome = null, TaxRate = 30, Description = "Above ₹15,00,000" }
            };

            // New Tax Regime FY 2024-25 - Senior Citizen (60-80)
            slabs["2024-25_New_SeniorCitizen"] = new List<TaxSlab>
            {
                new TaxSlab { MinIncome = 0, MaxIncome = 300000, TaxRate = 0, Description = "Up to ₹3,00,000" },
                new TaxSlab { MinIncome = 300000, MaxIncome = 700000, TaxRate = 5, Description = "₹3,00,001 to ₹7,00,000" },
                new TaxSlab { MinIncome = 700000, MaxIncome = 1000000, TaxRate = 10, Description = "₹7,00,001 to ₹10,00,000" },
                new TaxSlab { MinIncome = 1000000, MaxIncome = 1200000, TaxRate = 15, Description = "₹10,00,001 to ₹12,00,000" },
                new TaxSlab { MinIncome = 1200000, MaxIncome = 1500000, TaxRate = 20, Description = "₹12,00,001 to ₹15,00,000" },
                new TaxSlab { MinIncome = 1500000, MaxIncome = null, TaxRate = 30, Description = "Above ₹15,00,000" }
            };

            // New Tax Regime FY 2024-25 - Super Senior Citizen (80+)
            slabs["2024-25_New_SuperSeniorCitizen"] = new List<TaxSlab>
            {
                new TaxSlab { MinIncome = 0, MaxIncome = 300000, TaxRate = 0, Description = "Up to ₹3,00,000" },
                new TaxSlab { MinIncome = 300000, MaxIncome = 700000, TaxRate = 5, Description = "₹3,00,001 to ₹7,00,000" },
                new TaxSlab { MinIncome = 700000, MaxIncome = 1000000, TaxRate = 10, Description = "₹7,00,001 to ₹10,00,000" },
                new TaxSlab { MinIncome = 1000000, MaxIncome = 1200000, TaxRate = 15, Description = "₹10,00,001 to ₹12,00,000" },
                new TaxSlab { MinIncome = 1200000, MaxIncome = 1500000, TaxRate = 20, Description = "₹12,00,001 to ₹15,00,000" },
                new TaxSlab { MinIncome = 1500000, MaxIncome = null, TaxRate = 30, Description = "Above ₹15,00,000" }
            };

            // Old Tax Regime FY 2024-25 - Individual Below 60
            slabs["2024-25_Old_Individual"] = new List<TaxSlab>
            {
                new TaxSlab { MinIncome = 0, MaxIncome = 250000, TaxRate = 0, Description = "Up to ₹2,50,000" },
                new TaxSlab { MinIncome = 250000, MaxIncome = 500000, TaxRate = 5, Description = "₹2,50,001 to ₹5,00,000" },
                new TaxSlab { MinIncome = 500000, MaxIncome = 1000000, TaxRate = 20, Description = "₹5,00,001 to ₹10,00,000" },
                new TaxSlab { MinIncome = 1000000, MaxIncome = null, TaxRate = 30, Description = "Above ₹10,00,000" }
            };

            // Old Tax Regime FY 2024-25 - Senior Citizen (60-80)
            slabs["2024-25_Old_SeniorCitizen"] = new List<TaxSlab>
            {
                new TaxSlab { MinIncome = 0, MaxIncome = 300000, TaxRate = 0, Description = "Up to ₹3,00,000" },
                new TaxSlab { MinIncome = 300000, MaxIncome = 500000, TaxRate = 5, Description = "₹3,00,001 to ₹5,00,000" },
                new TaxSlab { MinIncome = 500000, MaxIncome = 1000000, TaxRate = 20, Description = "₹5,00,001 to ₹10,00,000" },
                new TaxSlab { MinIncome = 1000000, MaxIncome = null, TaxRate = 30, Description = "Above ₹10,00,000" }
            };

            // Old Tax Regime FY 2024-25 - Super Senior Citizen (80+)
            slabs["2024-25_Old_SuperSeniorCitizen"] = new List<TaxSlab>
            {
                new TaxSlab { MinIncome = 0, MaxIncome = 500000, TaxRate = 0, Description = "Up to ₹5,00,000" },
                new TaxSlab { MinIncome = 500000, MaxIncome = 1000000, TaxRate = 20, Description = "₹5,00,001 to ₹10,00,000" },
                new TaxSlab { MinIncome = 1000000, MaxIncome = null, TaxRate = 30, Description = "Above ₹10,00,000" }
            };
        }

        private string GetAgeCategory(int age)
        {
            if (age >= 80) return "SuperSeniorCitizen";
            if (age >= 60) return "SeniorCitizen";
            return "Individual";
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

        private List<TaxSlab> GetDefaultSlabs(TaxRegime regime, int age)
        {
            // Return current year slabs as default
            var currentYear = GetCurrentFinancialYear();
            return GetTaxSlabs(currentYear, regime, age);
        }
    }
}
