using Microsoft.AspNetCore.Mvc;
using ReturnlyWebApi.DTOs;
using ReturnlyWebApi.Services;
using ReturnlyWebApi.Models;

namespace ReturnlyWebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TaxCalculationController : ControllerBase
{
    private readonly ITaxCalculationService _taxCalculationService;
    private readonly ILogger<TaxCalculationController> _logger;

    public TaxCalculationController(
        ITaxCalculationService taxCalculationService,
        ILogger<TaxCalculationController> logger)
    {
        _taxCalculationService = taxCalculationService;
        _logger = logger;
    }

    /// <summary>
    /// Calculate tax based on taxable income and financial year
    /// </summary>
    [HttpPost("calculate")]
    public ActionResult<TaxCalculationResponseDto> CalculateTax([FromBody] TaxCalculationRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Calculate tax using New Tax Regime (default)
            var taxCalculation = _taxCalculationService.CalculateTax(
                request.TaxableIncome,
                request.FinancialYear,
                TaxRegime.New,
                request.Age
            );

            // Calculate refund/additional tax
            var refundCalculation = _taxCalculationService.CalculateRefund(
                taxCalculation,
                request.TdsDeducted
            );

            var response = new TaxCalculationResponseDto
            {
                TaxCalculation = MapToDto(taxCalculation),
                RefundCalculation = MapToDto(refundCalculation)
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating tax for income {Income}, FY {FinancialYear}", 
                request.TaxableIncome, request.FinancialYear);
            
            return StatusCode(500, new { error = "An error occurred while calculating tax", details = ex.Message });
        }
    }

    /// <summary>
    /// Compare tax calculations between old and new regime
    /// </summary>
    [HttpPost("compare-regimes")]
    public ActionResult<RegimeComparisonResponseDto> CompareTaxRegimes([FromBody] RegimeComparisonRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // For old regime, subtract old regime deductions from taxable income
            var oldRegimeTaxableIncome = Math.Max(0, request.TaxableIncome - request.OldRegimeDeductions);
            
            var comparison = _taxCalculationService.CompareTaxRegimes(
                oldRegimeTaxableIncome,
                request.TaxableIncome,
                request.FinancialYear,
                request.Age,
                request.OldRegimeDeductions
            );

            var response = new RegimeComparisonResponseDto
            {
                OldRegimeCalculation = MapToDto(comparison.OldRegimeCalculation),
                NewRegimeCalculation = MapToDto(comparison.NewRegimeCalculation),
                RecommendedRegime = comparison.RecommendedRegime.ToString(),
                TaxSavings = comparison.TaxSavings,
                ComparisonSummary = comparison.ComparisonSummary
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error comparing tax regimes for income {Income}, FY {FinancialYear}", 
                request.TaxableIncome, request.FinancialYear);
            
            return StatusCode(500, new { error = "An error occurred while comparing tax regimes", details = ex.Message });
        }
    }

    /// <summary>
    /// Get supported financial years
    /// </summary>
    [HttpGet("financial-years")]
    public ActionResult<List<string>> GetSupportedFinancialYears()
    {
        var currentYear = DateTime.Now.Year;
        var currentMonth = DateTime.Now.Month;
        var currentFinancialYear = currentMonth >= 4 ? currentYear : currentYear - 1;
        
        var financialYears = new List<string>();
        
        // Support current and past 5 financial years
        for (int i = 0; i >= -5; i--)
        {
            var year = currentFinancialYear + i;
            if (year >= 2023) // Minimum supported year
            {
                financialYears.Add($"{year}-{(year + 1).ToString().Substring(2)}");
            }
        }
        
        return Ok(financialYears);
    }

    /// <summary>
    /// Get supported assessment years
    /// </summary>
    [HttpGet("assessment-years")]
    public ActionResult<List<string>> GetSupportedAssessmentYears()
    {
        var currentYear = DateTime.Now.Year;
        var currentMonth = DateTime.Now.Month;
        var currentAssessmentYear = currentMonth >= 4 ? currentYear + 1 : currentYear;
        
        var assessmentYears = new List<string>();
        
        // Support current and past 5 assessment years
        for (int i = 0; i >= -5; i--)
        {
            var year = currentAssessmentYear + i;
            if (year >= 2024) // Minimum supported year (AY 2024-25)
            {
                assessmentYears.Add($"{year}-{(year + 1).ToString().Substring(2)}");
            }
        }
        
        return Ok(assessmentYears);
    }

    /// <summary>
    /// Get tax calculation breakdown as formatted text
    /// </summary>
    [HttpPost("breakdown")]
    public ActionResult<object> GetTaxBreakdown([FromBody] TaxCalculationRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var taxCalculation = _taxCalculationService.CalculateTax(
                request.TaxableIncome,
                request.FinancialYear,
                TaxRegime.New,
                request.Age
            );

            var refundCalculation = _taxCalculationService.CalculateRefund(
                taxCalculation,
                request.TdsDeducted
            );

            var breakdown = GenerateCalculationBreakdown(taxCalculation, refundCalculation);

            return Ok(new { breakdown });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating tax breakdown");
            return StatusCode(500, new { error = "An error occurred while generating tax breakdown", details = ex.Message });
        }
    }

    private TaxCalculationResultDto MapToDto(TaxCalculationResult result)
    {
        return new TaxCalculationResultDto
        {
            TaxableIncome = result.TaxableIncome,
            FinancialYear = result.FinancialYear,
            TaxRegime = result.TaxRegime.ToString(),
            Age = result.Age,
            TaxBreakdown = result.TaxBreakdown.Select(tb => new TaxSlabCalculationDto
            {
                SlabDescription = tb.SlabDescription,
                IncomeInSlab = tb.IncomeInSlab,
                TaxRate = tb.TaxRate,
                TaxAmount = tb.TaxAmount,
                MinIncome = tb.MinIncome,
                MaxIncome = tb.MaxIncome
            }).ToList(),
            TotalTax = result.TotalTax,
            Surcharge = result.Surcharge,
            SurchargeRate = result.SurchargeRate,
            TotalTaxWithSurcharge = result.TotalTaxWithSurcharge,
            HealthAndEducationCess = result.HealthAndEducationCess,
            TotalTaxWithCess = result.TotalTaxWithCess,
            EffectiveTaxRate = result.EffectiveTaxRate
        };
    }

    private TaxRefundCalculationDto MapToDto(TaxRefundCalculation refund)
    {
        return new TaxRefundCalculationDto
        {
            TotalTaxLiability = refund.TotalTaxLiability,
            TDSDeducted = refund.TDSDeducted,
            RefundAmount = refund.RefundAmount,
            AdditionalTaxDue = refund.AdditionalTaxDue,
            IsRefundDue = refund.IsRefundDue
        };
    }

    private string GenerateCalculationBreakdown(TaxCalculationResult taxCalculation, TaxRefundCalculation refundCalculation)
    {
        var breakdown = "Tax Calculation Breakdown:\n\n";
        
        foreach (var slab in taxCalculation.TaxBreakdown)
        {
            breakdown += $"• {slab.SlabDescription}: ₹{slab.IncomeInSlab:N2} @ {slab.TaxRate}% = ₹{slab.TaxAmount:N2}\n";
        }
        
        breakdown += $"\nSubtotal: ₹{taxCalculation.TotalTax:N2}\n";
        
        if (taxCalculation.Surcharge > 0)
        {
            breakdown += $"Surcharge ({taxCalculation.SurchargeRate}%): ₹{taxCalculation.Surcharge:N2}\n";
        }
        
        breakdown += $"Health & Education Cess (4%): ₹{taxCalculation.HealthAndEducationCess:N2}\n";
        breakdown += $"Total Tax Liability: ₹{taxCalculation.TotalTaxWithCess:N2}\n";
        breakdown += $"Effective Tax Rate: {taxCalculation.EffectiveTaxRate:F2}%\n\n";
        
        breakdown += "Refund/Additional Tax Analysis:\n";
        breakdown += $"TDS Deducted: ₹{refundCalculation.TDSDeducted:N2}\n";
        
        if (refundCalculation.IsRefundDue)
        {
            breakdown += $"Refund Due: ₹{refundCalculation.RefundAmount:N2} 🎉";
        }
        else if (refundCalculation.AdditionalTaxDue > 0)
        {
            breakdown += $"Additional Tax Due: ₹{refundCalculation.AdditionalTaxDue:N2} ⚠️";
        }
        else
        {
            breakdown += "Tax liability exactly matches TDS deducted ✅";
        }

        return breakdown;
    }
}
