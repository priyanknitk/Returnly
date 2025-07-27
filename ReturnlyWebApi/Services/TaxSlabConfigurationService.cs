using ReturnlyWebApi.Models;

namespace ReturnlyWebApi.Services;

public class TaxSlab
{
    public decimal MinIncome { get; set; }
    public decimal? MaxIncome { get; set; }
    public decimal TaxRate { get; set; }
    public string Description { get; set; } = string.Empty;
}

public interface ITaxSlabConfigurationService
{
    List<TaxSlab> GetTaxSlabs(string financialYear, TaxRegime regime, int age);
}

public class TaxSlabConfigurationService : ITaxSlabConfigurationService
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

        // Financial Year 2023-24 (Assessment Year 2024-25)
        InitializeFY2023_24(slabs);
        
        // Financial Year 2024-25 (Assessment Year 2025-26)
        InitializeFY2024_25(slabs);
        
        // Financial Year 2025-26 (Assessment Year 2026-27)
        InitializeFY2025_26(slabs);

        return slabs;
    }

    private void InitializeFY2023_24(Dictionary<string, List<TaxSlab>> slabs)
    {
        // New Tax Regime FY 2023-24 - Individual Below 60
        slabs["2023-24_New_Individual"] = GetNewRegimeSlabsForYear("2023-24");

        // New Tax Regime FY 2023-24 - Senior Citizen (60-80)
        slabs["2023-24_New_SeniorCitizen"] = GetNewRegimeSlabsForYear("2023-24");

        // New Tax Regime FY 2023-24 - Super Senior Citizen (80+)
        slabs["2023-24_New_SuperSeniorCitizen"] = GetNewRegimeSlabsForYear("2023-24");

        // Old Tax Regime FY 2023-24 - Individual Below 60
        slabs["2023-24_Old_Individual"] = GetOldRegimeSlabsForYear("2023-24", 30);

        // Old Tax Regime FY 2023-24 - Senior Citizen (60-80)
        slabs["2023-24_Old_SeniorCitizen"] = GetOldRegimeSlabsForYear("2023-24", 65);

        // Old Tax Regime FY 2023-24 - Super Senior Citizen (80+)
        slabs["2023-24_Old_SuperSeniorCitizen"] = GetOldRegimeSlabsForYear("2023-24", 85);
    }

    private void InitializeFY2024_25(Dictionary<string, List<TaxSlab>> slabs)
    {
        // New Tax Regime FY 2024-25 - Individual Below 60
        slabs["2024-25_New_Individual"] = GetNewRegimeSlabsForYear("2024-25");

        // New Tax Regime FY 2024-25 - Senior Citizen (60-80)
        slabs["2024-25_New_SeniorCitizen"] = GetNewRegimeSlabsForYear("2024-25");

        // New Tax Regime FY 2024-25 - Super Senior Citizen (80+)
        slabs["2024-25_New_SuperSeniorCitizen"] = GetNewRegimeSlabsForYear("2024-25");

        // Old Tax Regime FY 2024-25 - Individual Below 60
        slabs["2024-25_Old_Individual"] = GetOldRegimeSlabsForYear("2024-25", 30);

        // Old Tax Regime FY 2024-25 - Senior Citizen (60-80)
        slabs["2024-25_Old_SeniorCitizen"] = GetOldRegimeSlabsForYear("2024-25", 65);

        // Old Tax Regime FY 2024-25 - Super Senior Citizen (80+)
        slabs["2024-25_Old_SuperSeniorCitizen"] = GetOldRegimeSlabsForYear("2024-25", 85);
    }

    private void InitializeFY2025_26(Dictionary<string, List<TaxSlab>> slabs)
    {
        // New Tax Regime FY 2025-26 - Individual Below 60
        slabs["2025-26_New_Individual"] = GetNewRegimeSlabsForYear("2025-26");

        // New Tax Regime FY 2025-26 - Senior Citizen (60-80)
        slabs["2025-26_New_SeniorCitizen"] = GetNewRegimeSlabsForYear("2025-26");

        // New Tax Regime FY 2025-26 - Super Senior Citizen (80+)
        slabs["2025-26_New_SuperSeniorCitizen"] = GetNewRegimeSlabsForYear("2025-26");

        // Old Tax Regime FY 2025-26 - Individual Below 60
        slabs["2025-26_Old_Individual"] = GetOldRegimeSlabsForYear("2025-26", 30);

        // Old Tax Regime FY 2025-26 - Senior Citizen (60-80)
        slabs["2025-26_Old_SeniorCitizen"] = GetOldRegimeSlabsForYear("2025-26", 65);

        // Old Tax Regime FY 2025-26 - Super Senior Citizen (80+)
        slabs["2025-26_Old_SuperSeniorCitizen"] = GetOldRegimeSlabsForYear("2025-26", 85);
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
        // Return hardcoded default slabs to avoid recursion
        if (regime == TaxRegime.New)
        {
            return GetNewRegimeSlabsForYear("2024-25");
        }
        else
        {
            return GetOldRegimeSlabsForYear("2024-25", age);
        }
    }

    private List<TaxSlab> GetNewRegimeSlabsForYear(string financialYear)
    {
        // New Tax Regime slabs - consistent across all years from 2023-24 onwards
        return new List<TaxSlab>
        {
            new TaxSlab { MinIncome = 0, MaxIncome = 300000, TaxRate = 0, Description = "Up to ₹3,00,000" },
            new TaxSlab { MinIncome = 300000, MaxIncome = 700000, TaxRate = 5, Description = "₹3,00,001 to ₹7,00,000" },
            new TaxSlab { MinIncome = 700000, MaxIncome = 1000000, TaxRate = 10, Description = "₹7,00,001 to ₹10,00,000" },
            new TaxSlab { MinIncome = 1000000, MaxIncome = 1200000, TaxRate = 15, Description = "₹10,00,001 to ₹12,00,000" },
            new TaxSlab { MinIncome = 1200000, MaxIncome = 1500000, TaxRate = 20, Description = "₹12,00,001 to ₹15,00,000" },
            new TaxSlab { MinIncome = 1500000, MaxIncome = null, TaxRate = 30, Description = "Above ₹15,00,000" }
        };
    }

    private List<TaxSlab> GetOldRegimeSlabsForYear(string financialYear, int age)
    {
        // Old Tax Regime slabs with age-based exemptions
        var basicExemptionLimit = GetBasicExemptionLimit(age);
        
        if (basicExemptionLimit == 500000) // Super Senior Citizens (80+)
        {
            return new List<TaxSlab>
            {
                new TaxSlab { MinIncome = 0, MaxIncome = 500000, TaxRate = 0, Description = "Up to ₹5,00,000" },
                new TaxSlab { MinIncome = 500000, MaxIncome = 1000000, TaxRate = 20, Description = "₹5,00,001 to ₹10,00,000" },
                new TaxSlab { MinIncome = 1000000, MaxIncome = null, TaxRate = 30, Description = "Above ₹10,00,000" }
            };
        }
        else
        {
            return new List<TaxSlab>
            {
                new TaxSlab { MinIncome = 0, MaxIncome = basicExemptionLimit, TaxRate = 0, Description = $"Up to ₹{basicExemptionLimit:N0}" },
                new TaxSlab { MinIncome = basicExemptionLimit, MaxIncome = 500000, TaxRate = 5, Description = $"₹{basicExemptionLimit + 1:N0} to ₹5,00,000" },
                new TaxSlab { MinIncome = 500000, MaxIncome = 1000000, TaxRate = 20, Description = "₹5,00,001 to ₹10,00,000" },
                new TaxSlab { MinIncome = 1000000, MaxIncome = null, TaxRate = 30, Description = "Above ₹10,00,000" }
            };
        }
    }

    private decimal GetBasicExemptionLimit(int age)
    {
        // Age-based exemption limits for old regime
        if (age >= 80) return 500000; // Super Senior Citizens
        if (age >= 60) return 300000; // Senior Citizens
        return 250000; // Below 60 years
    }
}
