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
    public List<TaxSlab> GetTaxSlabs(string financialYear, TaxRegime regime, int age)
    {
        // For AY 2024-25 (FY 2023-24) and onwards - New Tax Regime (default)
        if (regime == TaxRegime.New)
        {
            return GetNewRegimeSlabs(financialYear, age);
        }
        else
        {
            return GetOldRegimeSlabs(financialYear, age);
        }
    }

    private List<TaxSlab> GetNewRegimeSlabs(string financialYear, int age)
    {
        // New Tax Regime slabs for AY 2024-25 onwards
        return new List<TaxSlab>
        {
            new TaxSlab { MinIncome = 0, MaxIncome = 300000, TaxRate = 0, Description = "Up to ₹3,00,000 - Nil" },
            new TaxSlab { MinIncome = 300000, MaxIncome = 600000, TaxRate = 5, Description = "₹3,00,001 to ₹6,00,000 - 5%" },
            new TaxSlab { MinIncome = 600000, MaxIncome = 900000, TaxRate = 10, Description = "₹6,00,001 to ₹9,00,000 - 10%" },
            new TaxSlab { MinIncome = 900000, MaxIncome = 1200000, TaxRate = 15, Description = "₹9,00,001 to ₹12,00,000 - 15%" },
            new TaxSlab { MinIncome = 1200000, MaxIncome = 1500000, TaxRate = 20, Description = "₹12,00,001 to ₹15,00,000 - 20%" },
            new TaxSlab { MinIncome = 1500000, MaxIncome = null, TaxRate = 30, Description = "Above ₹15,00,000 - 30%" }
        };
    }

    private List<TaxSlab> GetOldRegimeSlabs(string financialYear, int age)
    {
        // Old Tax Regime slabs with age-based exemptions
        var basicExemptionLimit = GetBasicExemptionLimit(age);
        
        return new List<TaxSlab>
        {
            new TaxSlab { MinIncome = 0, MaxIncome = basicExemptionLimit, TaxRate = 0, Description = $"Up to ₹{basicExemptionLimit:N0} - Nil" },
            new TaxSlab { MinIncome = basicExemptionLimit, MaxIncome = 500000, TaxRate = 5, Description = $"₹{basicExemptionLimit + 1:N0} to ₹5,00,000 - 5%" },
            new TaxSlab { MinIncome = 500000, MaxIncome = 1000000, TaxRate = 20, Description = "₹5,00,001 to ₹10,00,000 - 20%" },
            new TaxSlab { MinIncome = 1000000, MaxIncome = null, TaxRate = 30, Description = "Above ₹10,00,000 - 30%" }
        };
    }

    private decimal GetBasicExemptionLimit(int age)
    {
        // Age-based exemption limits for old regime
        if (age >= 80) return 500000; // Super Senior Citizens
        if (age >= 60) return 300000; // Senior Citizens
        return 250000; // Below 60 years
    }
}
