using Microsoft.AspNetCore.Mvc;
using ReturnlyWebApi.Services;
using ReturnlyWebApi.Models;

namespace ReturnlyWebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DebugController : ControllerBase
{
    private readonly ITaxSlabConfigurationService _taxSlabService;
    private readonly ITaxCalculationService _taxCalculationService;

    public DebugController(ITaxSlabConfigurationService taxSlabService, ITaxCalculationService taxCalculationService)
    {
        _taxSlabService = taxSlabService;
        _taxCalculationService = taxCalculationService;
    }

    [HttpGet("slabs")]
    public IActionResult GetTaxSlabs([FromQuery] string financialYear = "2023-24", [FromQuery] TaxRegime regime = TaxRegime.New, [FromQuery] int age = 30)
    {
        try
        {
            var slabs = _taxSlabService.GetTaxSlabs(financialYear, regime, age);
            return Ok(new 
            {
                FinancialYear = financialYear,
                Regime = regime.ToString(),
                Age = age,
                Slabs = slabs.Select(s => new 
                {
                    s.MinIncome,
                    s.MaxIncome,
                    s.TaxRate,
                    s.Description
                })
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet("calculate")]
    public IActionResult CalculateTax([FromQuery] decimal income, [FromQuery] string financialYear = "2023-24", [FromQuery] TaxRegime regime = TaxRegime.New, [FromQuery] int age = 30)
    {
        try
        {
            var result = _taxCalculationService.CalculateTax(income, financialYear, regime, age);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }
}
