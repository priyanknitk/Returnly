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

            // Parse tax regime
            var taxRegime = Enum.TryParse<TaxRegime>(request.TaxRegime, true, out var regime) ? regime : TaxRegime.New;

            // Calculate tax
            var taxCalculation = _taxCalculationService.CalculateTax(
                request.TaxableIncome,
                request.FinancialYear,
                taxRegime,
                request.Age
            );

            TaxRefundCalculation refundCalculation;

            // Always check for advance tax penalties if tax liability > ₹10,000
            var netTaxLiability = taxCalculation.TotalTaxWithCess - request.TdsDeducted;
            
            if (netTaxLiability > 10000)
            {
                // Create advance tax installments object (defaults to 0 if not provided)
                var advanceTaxInstallments = new AdvanceTaxInstallments();
                if (request.AdvanceTaxPaid != null)
                {
                    advanceTaxInstallments = new AdvanceTaxInstallments
                    {
                        FirstInstallment = request.AdvanceTaxPaid.FirstInstallment,
                        SecondInstallment = request.AdvanceTaxPaid.SecondInstallment,
                        ThirdInstallment = request.AdvanceTaxPaid.ThirdInstallment,
                        FourthInstallment = request.AdvanceTaxPaid.FourthInstallment,
                        FirstInstallmentDate = request.AdvanceTaxPaid.FirstInstallmentDate,
                        SecondInstallmentDate = request.AdvanceTaxPaid.SecondInstallmentDate,
                        ThirdInstallmentDate = request.AdvanceTaxPaid.ThirdInstallmentDate,
                        FourthInstallmentDate = request.AdvanceTaxPaid.FourthInstallmentDate
                    };
                }
                
                // Use current date as filing date if not provided (for demo purposes)
                var filingDate = request.FilingDate ?? DateTime.Now;
                
                // Calculate refund with advance tax penalties
                refundCalculation = _taxCalculationService.CalculateRefundWithAdvanceTaxPenalties(
                    taxCalculation,
                    request.TdsDeducted,
                    advanceTaxInstallments,
                    request.SelfAssessmentTaxPaid,
                    filingDate,
                    request.FinancialYear
                );
            }
            else
            {
                // Simple refund calculation without advance tax penalties (tax liability ≤ ₹10,000)
                refundCalculation = _taxCalculationService.CalculateRefund(
                    taxCalculation,
                    request.TdsDeducted,
                    request.AdvanceTaxPaid?.TotalAdvanceTaxPaid ?? 0,
                    request.SelfAssessmentTaxPaid
                );
            }

            var response = new TaxCalculationResponseDto
            {
                TaxCalculation = MapToDto(taxCalculation, refundCalculation),
                RefundCalculation = MapToDto(refundCalculation)
            };

            // Surface advance tax penalties at top level for easy UI access
            if (refundCalculation.AdvanceTaxPenalties != null)
            {
                response.Section234AInterest = refundCalculation.AdvanceTaxPenalties.Section234AInterest;
                response.Section234BInterest = refundCalculation.AdvanceTaxPenalties.Section234BInterest;
                response.Section234CInterest = refundCalculation.AdvanceTaxPenalties.Section234CInterest;
                response.TotalAdvanceTaxPenalties = refundCalculation.AdvanceTaxPenalties.TotalPenalties;
                response.HasAdvanceTaxPenalties = refundCalculation.AdvanceTaxPenalties.TotalPenalties > 0;
                
                // Include detailed penalty breakdown for advanced views
                response.AdvanceTaxPenaltyDetails = new AdvanceTaxPenaltyCalculationDto
                {
                    TotalTaxLiability = refundCalculation.AdvanceTaxPenalties.TotalTaxLiability,
                    TDSDeducted = refundCalculation.AdvanceTaxPenalties.TDSDeducted,
                    AdvanceTaxPaid = new AdvanceTaxInstallmentsDto
                    {
                        FirstInstallment = refundCalculation.AdvanceTaxPenalties.AdvanceTaxPaid.FirstInstallment,
                        SecondInstallment = refundCalculation.AdvanceTaxPenalties.AdvanceTaxPaid.SecondInstallment,
                        ThirdInstallment = refundCalculation.AdvanceTaxPenalties.AdvanceTaxPaid.ThirdInstallment,
                        FourthInstallment = refundCalculation.AdvanceTaxPenalties.AdvanceTaxPaid.FourthInstallment,
                        FirstInstallmentDate = refundCalculation.AdvanceTaxPenalties.AdvanceTaxPaid.FirstInstallmentDate,
                        SecondInstallmentDate = refundCalculation.AdvanceTaxPenalties.AdvanceTaxPaid.SecondInstallmentDate,
                        ThirdInstallmentDate = refundCalculation.AdvanceTaxPenalties.AdvanceTaxPaid.ThirdInstallmentDate,
                        FourthInstallmentDate = refundCalculation.AdvanceTaxPenalties.AdvanceTaxPaid.FourthInstallmentDate
                    },
                    FilingDate = refundCalculation.AdvanceTaxPenalties.FilingDate,
                    FinancialYear = refundCalculation.AdvanceTaxPenalties.FinancialYear,
                    Section234AInterest = refundCalculation.AdvanceTaxPenalties.Section234AInterest,
                    Section234BInterest = refundCalculation.AdvanceTaxPenalties.Section234BInterest,
                    Section234CInterest = refundCalculation.AdvanceTaxPenalties.Section234CInterest,
                    TotalPenalties = refundCalculation.AdvanceTaxPenalties.TotalPenalties,
                    PenaltyDetails = [.. refundCalculation.AdvanceTaxPenalties.PenaltyDetails.Select(pd => new AdvanceTaxPenaltyDetailDto
                    {
                        InstallmentPeriod = pd.InstallmentPeriod,
                        RequiredAmount = pd.RequiredAmount,
                        ActualAmount = pd.ActualAmount,
                        Shortfall = pd.Shortfall,
                        InterestRate = pd.InterestRate,
                        InterestDays = pd.InterestDays,
                        InterestAmount = pd.InterestAmount,
                        PenaltySection = pd.PenaltySection,
                        Description = pd.Description
                    })]
                };
            }

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

    private TaxCalculationResultDto MapToDto(TaxCalculationResult result, TaxRefundCalculation? refundCalculation = null)
    {
        var dto = new TaxCalculationResultDto
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

        // Add penalty information if available
        if (refundCalculation?.AdvanceTaxPenalties != null)
        {
            dto.Section234AInterest = refundCalculation.AdvanceTaxPenalties.Section234AInterest;
            dto.Section234BInterest = refundCalculation.AdvanceTaxPenalties.Section234BInterest;
            dto.Section234CInterest = refundCalculation.AdvanceTaxPenalties.Section234CInterest;
            dto.TotalAdvanceTaxPenalties = refundCalculation.AdvanceTaxPenalties.TotalPenalties;
            dto.TotalTaxLiabilityWithPenalties = result.TotalTaxWithCess + refundCalculation.AdvanceTaxPenalties.TotalPenalties;
        }
        else
        {
            dto.TotalTaxLiabilityWithPenalties = result.TotalTaxWithCess;
        }

        return dto;
    }

    private TaxRefundCalculationDto MapToDto(TaxRefundCalculation refund)
    {
        return new TaxRefundCalculationDto
        {
            TotalTaxLiability = refund.TotalTaxLiability,
            TDSDeducted = refund.TDSDeducted,
            AdvanceTaxPaid = refund.AdvanceTaxPaid,
            SelfAssessmentTaxPaid = refund.SelfAssessmentTaxPaid,
            RefundAmount = refund.RefundAmount,
            AdditionalTaxDue = refund.AdditionalTaxDue,
            IsRefundDue = refund.IsRefundDue,
            TotalTaxPaid = refund.TotalTaxPaid
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
        
        if (refundCalculation.AdvanceTaxPaid > 0)
        {
            breakdown += $"Advance Tax Paid: ₹{refundCalculation.AdvanceTaxPaid:N2}\n";
        }
        
        if (refundCalculation.SelfAssessmentTaxPaid > 0)
        {
            breakdown += $"Self Assessment Tax: ₹{refundCalculation.SelfAssessmentTaxPaid:N2}\n";
        }
        
        // Add advance tax penalties if applicable
        if (refundCalculation.AdvanceTaxPenalties != null && refundCalculation.AdvanceTaxPenalties.TotalPenalties > 0)
        {
            breakdown += "\nAdvance Tax Penalties:\n";
            
            if (refundCalculation.AdvanceTaxPenalties.Section234AInterest > 0)
            {
                breakdown += $"• Section 234A (Default in Payment): ₹{refundCalculation.AdvanceTaxPenalties.Section234AInterest:N0}\n";
            }
            
            if (refundCalculation.AdvanceTaxPenalties.Section234BInterest > 0)
            {
                breakdown += $"• Section 234B (Failure to Pay): ₹{refundCalculation.AdvanceTaxPenalties.Section234BInterest:N0}\n";
            }
            
            if (refundCalculation.AdvanceTaxPenalties.Section234CInterest > 0)
            {
                breakdown += $"• Section 234C (Deferment): ₹{refundCalculation.AdvanceTaxPenalties.Section234CInterest:N0}\n";
            }
            
            breakdown += $"Total Penalties: ₹{refundCalculation.AdvanceTaxPenalties.TotalPenalties:N0}\n";
        }
        
        breakdown += $"\nTotal Tax Paid: ₹{refundCalculation.TotalTaxPaid:N2}\n";
        
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
