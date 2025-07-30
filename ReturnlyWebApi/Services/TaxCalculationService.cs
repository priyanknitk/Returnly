using ReturnlyWebApi.Models;

namespace ReturnlyWebApi.Services;

public interface ITaxCalculationService
{
    TaxCalculationResult CalculateTax(decimal taxableIncome, string financialYear, TaxRegime regime = TaxRegime.New, int age = 30);
    TaxRefundCalculation CalculateRefund(TaxCalculationResult taxCalculation, decimal tdsDeducted, decimal advanceTaxPaid = 0, decimal selfAssessmentTaxPaid = 0);
    TaxRefundCalculation CalculateRefundWithAdvanceTaxPenalties(
        TaxCalculationResult taxCalculation, 
        decimal tdsDeducted, 
        AdvanceTaxInstallments advanceTaxPaid, 
        decimal selfAssessmentTaxPaid, 
        DateTime filingDate, 
        string financialYear);
    RegimeComparisonResult CompareTaxRegimes(decimal taxableIncomeOld, decimal taxableIncomeNew, string financialYear, int age = 30, decimal oldRegimeDeductions = 0);
}

public class TaxCalculationService : ITaxCalculationService
{
    private readonly ITaxSlabConfigurationService _taxSlabService;
    private readonly IAdvanceTaxPenaltyService _advanceTaxPenaltyService;

    public TaxCalculationService(ITaxSlabConfigurationService taxSlabService, IAdvanceTaxPenaltyService advanceTaxPenaltyService)
    {
        _taxSlabService = taxSlabService;
        _advanceTaxPenaltyService = advanceTaxPenaltyService;
    }

    public TaxCalculationResult CalculateTax(decimal taxableIncome, string financialYear, TaxRegime regime = TaxRegime.New, int age = 30)
    {
        try
        {
            var taxSlabs = _taxSlabService.GetTaxSlabs(financialYear, regime, age);
            
            if (taxSlabs == null || !taxSlabs.Any())
            {
                throw new InvalidOperationException($"No tax slabs found for financial year {financialYear}, regime {regime}, age {age}");
            }

            var result = new TaxCalculationResult
            {
                TaxableIncome = taxableIncome,
                FinancialYear = financialYear,
                TaxRegime = regime,
                Age = age,
                TaxBreakdown = new List<TaxSlabCalculation>()
            };

            decimal remainingIncome = taxableIncome;
            decimal totalTax = 0;

            foreach (var slab in taxSlabs.OrderBy(s => s.MinIncome))
            {
                if (remainingIncome <= 0) break;

                // Calculate the maximum income that can be taxed in this slab
                decimal slabUpperLimit = slab.MaxIncome ?? decimal.MaxValue;
                decimal slabWidth = slabUpperLimit - slab.MinIncome;
                
                // Calculate actual income falling in this slab
                decimal incomeInThisSlab = Math.Min(remainingIncome, slabWidth);
                
                if (incomeInThisSlab > 0 && taxableIncome > slab.MinIncome)
                {
                    decimal taxInThisSlab = incomeInThisSlab * (slab.TaxRate / 100);
                    totalTax += taxInThisSlab;

                    result.TaxBreakdown.Add(new TaxSlabCalculation
                    {
                        SlabDescription = slab.Description,
                        IncomeInSlab = incomeInThisSlab,
                        TaxRate = slab.TaxRate,
                        TaxAmount = taxInThisSlab,
                        MinIncome = slab.MinIncome,
                        MaxIncome = slab.MaxIncome
                    });

                    remainingIncome -= incomeInThisSlab;
                }
            }

            result.TotalTax = totalTax;
            
            // Calculate surcharge based on total income and tax regime
            var surchargeInfo = CalculateSurcharge(taxableIncome, totalTax, regime);
            result.Surcharge = surchargeInfo.Amount;
            result.SurchargeRate = surchargeInfo.Rate;
            result.TotalTaxWithSurcharge = result.TotalTax + result.Surcharge;
            
            // Calculate cess on tax + surcharge
            result.HealthAndEducationCess = result.TotalTaxWithSurcharge * 0.04m; // 4% cess
            result.TotalTaxWithCess = result.TotalTaxWithSurcharge + result.HealthAndEducationCess;
            
            result.EffectiveTaxRate = taxableIncome > 0 ? (result.TotalTaxWithCess / taxableIncome) * 100 : 0;

            return result;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error calculating tax for income ₹{taxableIncome:N0}, FY {financialYear}, {regime} regime: {ex.Message}", ex);
        }
    }

    public TaxRefundCalculation CalculateRefund(TaxCalculationResult taxCalculation, decimal tdsDeducted, decimal advanceTaxPaid = 0, decimal selfAssessmentTaxPaid = 0)
    {
        var totalTaxPaid = tdsDeducted + advanceTaxPaid + selfAssessmentTaxPaid;
        
        return new TaxRefundCalculation
        {
            TotalTaxLiability = taxCalculation.TotalTaxWithCess,
            TDSDeducted = tdsDeducted,
            AdvanceTaxPaid = advanceTaxPaid,
            SelfAssessmentTaxPaid = selfAssessmentTaxPaid,
            RefundAmount = Math.Max(0, totalTaxPaid - taxCalculation.TotalTaxWithCess),
            AdditionalTaxDue = Math.Max(0, taxCalculation.TotalTaxWithCess - totalTaxPaid),
            IsRefundDue = totalTaxPaid > taxCalculation.TotalTaxWithCess
        };
    }

    public TaxRefundCalculation CalculateRefundWithAdvanceTaxPenalties(
        TaxCalculationResult taxCalculation, 
        decimal tdsDeducted, 
        AdvanceTaxInstallments advanceTaxPaid, 
        decimal selfAssessmentTaxPaid, 
        DateTime filingDate, 
        string financialYear)
    {
        var refundCalculation = CalculateRefund(taxCalculation, tdsDeducted, advanceTaxPaid.TotalAdvanceTaxPaid, selfAssessmentTaxPaid);
        
        // Calculate advance tax penalties
        var penaltyCalculation = _advanceTaxPenaltyService.CalculateAdvanceTaxPenalties(
            taxCalculation.TotalTaxWithCess,
            tdsDeducted,
            advanceTaxPaid,
            filingDate,
            financialYear);
        
        // Add penalties to the tax liability
        refundCalculation.AdvanceTaxPenalties = penaltyCalculation;
        
        // Recalculate refund/demand considering penalties
        var totalTaxLiabilityWithPenalties = taxCalculation.TotalTaxWithCess + penaltyCalculation.TotalPenalties;
        var totalTaxPaid = refundCalculation.TotalTaxPaid;
        
        refundCalculation.RefundAmount = Math.Max(0, totalTaxPaid - totalTaxLiabilityWithPenalties);
        refundCalculation.AdditionalTaxDue = Math.Max(0, totalTaxLiabilityWithPenalties - totalTaxPaid);
        refundCalculation.IsRefundDue = totalTaxPaid > totalTaxLiabilityWithPenalties;
        
        return refundCalculation;
    }

    public RegimeComparisonResult CompareTaxRegimes(decimal taxableIncomeOld, decimal taxableIncomeNew, string financialYear, int age = 30, decimal oldRegimeDeductions = 0)
    {
        var oldRegimeResult = CalculateTax(taxableIncomeOld, financialYear, TaxRegime.Old, age);
        var newRegimeResult = CalculateTax(taxableIncomeNew, financialYear, TaxRegime.New, age);
        
        var taxSavings = oldRegimeResult.TotalTaxWithCess - newRegimeResult.TotalTaxWithCess;
        var recommendedRegime = taxSavings > 0 ? TaxRegime.New : TaxRegime.Old;
        
        return new RegimeComparisonResult
        {
            OldRegimeCalculation = oldRegimeResult,
            NewRegimeCalculation = newRegimeResult,
            RecommendedRegime = recommendedRegime,
            TaxSavings = Math.Abs(taxSavings),
            ComparisonSummary = GenerateComparisonSummary(oldRegimeResult, newRegimeResult, recommendedRegime, Math.Abs(taxSavings))
        };
    }

    private (decimal Amount, decimal Rate) CalculateSurcharge(decimal totalIncome, decimal incomeTax, TaxRegime regime = TaxRegime.New)
    {
        decimal surchargeRate = 0;

        if (regime == TaxRegime.New)
        {
            // New Tax Regime surcharge rates (capped at 25%)
            if (totalIncome > 20000000) // Above ₹2 crores
                surchargeRate = 25;
            else if (totalIncome > 10000000) // Above ₹1 crore to ₹2 crores
                surchargeRate = 15;
            else if (totalIncome > 5000000) // Above ₹50 lakhs to ₹1 crore
                surchargeRate = 10;
        }
        else
        {
            // Old Tax Regime surcharge rates
            if (totalIncome > 50000000) // Above ₹5 crores
                surchargeRate = 37;
            else if (totalIncome > 20000000) // Above ₹2 crores to ₹5 crores
                surchargeRate = 25;
            else if (totalIncome > 10000000) // Above ₹1 crore to ₹2 crores
                surchargeRate = 15;
            else if (totalIncome > 5000000) // Above ₹50 lakhs to ₹1 crore
                surchargeRate = 10;
        }

        decimal surchargeAmount = incomeTax * (surchargeRate / 100);
        return (surchargeAmount, surchargeRate);
    }

    private string GenerateComparisonSummary(TaxCalculationResult oldRegime, TaxCalculationResult newRegime, TaxRegime recommended, decimal savings)
    {
        var summary = $"Old Regime Tax: ₹{oldRegime.TotalTaxWithCess:N2}\n";
        summary += $"New Regime Tax: ₹{newRegime.TotalTaxWithCess:N2}\n";
        summary += $"Recommended: {recommended} Tax Regime\n";
        
        if (savings > 0)
        {
            summary += $"Tax Savings: ₹{savings:N2}";
        }
        else
        {
            summary += "Both regimes result in similar tax liability";
        }

        return summary;
    }
}
