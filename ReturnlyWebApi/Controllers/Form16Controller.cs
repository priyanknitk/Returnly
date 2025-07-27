using Microsoft.AspNetCore.Mvc;
using ReturnlyWebApi.DTOs;
using ReturnlyWebApi.Services;
using ReturnlyWebApi.Models;

namespace ReturnlyWebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class Form16Controller : ControllerBase
{
    private readonly IForm16ProcessingService _form16ProcessingService;
    private readonly ILogger<Form16Controller> _logger;
    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

    public Form16Controller(
        IForm16ProcessingService form16ProcessingService,
        ILogger<Form16Controller> logger)
    {
        _form16ProcessingService = form16ProcessingService;
        _logger = logger;
    }

    /// <summary>
    /// Upload and process Form16 PDF
    /// </summary>
    [HttpPost("upload")]
    [RequestSizeLimit(MaxFileSize)]
    public async Task<ActionResult<Form16DataDto>> UploadForm16([FromForm] Form16UploadDto uploadDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validate file
            if (uploadDto.PdfFile == null || uploadDto.PdfFile.Length == 0)
            {
                return BadRequest(new { error = "Please select a PDF file to upload" });
            }

            if (uploadDto.PdfFile.Length > MaxFileSize)
            {
                return BadRequest(new { error = $"File size cannot exceed {MaxFileSize / (1024 * 1024)}MB" });
            }

            if (!uploadDto.PdfFile.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { error = "Only PDF files are allowed" });
            }

            // Process PDF
            using var stream = uploadDto.PdfFile.OpenReadStream();
            var form16Data = await _form16ProcessingService.ProcessForm16PdfAsync(stream, uploadDto.Password);

            // Validate extracted data
            await _form16ProcessingService.ValidateForm16DataAsync(form16Data);

            var responseDto = MapToDto(form16Data);

            _logger.LogInformation("Successfully processed Form16 for employee {EmployeeName}, PAN {PAN}",
                form16Data.EmployeeName, form16Data.PAN);

            return Ok(responseDto);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Form16 validation failed");
            return BadRequest(new { error = "Form16 validation failed", details = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return BadRequest(new { error = "Invalid password for PDF file" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Form16 upload");
            return StatusCode(500, new { error = "An error occurred while processing the Form16 PDF", details = ex.Message });
        }
    }

    /// <summary>
    /// Validate Form16 data
    /// </summary>
    [HttpPost("validate")]
    public async Task<ActionResult<object>> ValidateForm16Data([FromBody] Form16DataDto form16DataDto)
    {
        try
        {
            var form16Data = MapToModel(form16DataDto);
            var isValid = await _form16ProcessingService.ValidateForm16DataAsync(form16Data);

            return Ok(new { isValid, message = "Form16 data is valid" });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { isValid = false, error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating Form16 data");
            return StatusCode(500, new { error = "An error occurred while validating Form16 data" });
        }
    }

    /// <summary>
    /// Update tax data and calculate taxable income
    /// </summary>
    [HttpPost("update-tax-data")]
    public ActionResult<object> UpdateTaxData([FromBody] UpdateTaxDataRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var form16Data = MapToModel(request.Form16Data);
            
            // Calculate taxable income
            var totalIncome = form16Data.Form16B.GrossSalary + 
                             form16Data.Form16B.TotalInterestIncome + 
                             form16Data.Form16B.TotalDividendIncome;
            
            var taxableIncome = Math.Max(0, totalIncome - form16Data.Form16B.StandardDeduction - form16Data.Form16B.ProfessionalTax);

            return Ok(new 
            { 
                taxableIncome,
                totalIncome,
                grossSalary = form16Data.Form16B.GrossSalary,
                totalInterestIncome = form16Data.Form16B.TotalInterestIncome,
                totalDividendIncome = form16Data.Form16B.TotalDividendIncome,
                standardDeduction = form16Data.Form16B.StandardDeduction,
                professionalTax = form16Data.Form16B.ProfessionalTax
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating tax data");
            return StatusCode(500, new { error = "An error occurred while updating tax data" });
        }
    }

    /// <summary>
    /// Get sample Form16 structure for testing
    /// </summary>
    [HttpGet("sample")]
    public ActionResult<Form16DataDto> GetSampleForm16Data()
    {
        var sampleData = new Form16DataDto
        {
            EmployeeName = "Sample Employee",
            PAN = "ABCDE1234F",
            AssessmentYear = "2024-25",
            FinancialYear = "2023-24",
            EmployerName = "Sample Company Pvt Ltd",
            TAN = "ABCD12345E",
            GrossSalary = 1200000,
            TotalTaxDeducted = 120000,
            StandardDeduction = 75000,
            ProfessionalTax = 2400,
            Form16B = new Form16BDataDto
            {
                SalarySection17 = 1200000,
                Perquisites = 0,
                ProfitsInLieu = 0,
                BasicSalary = 600000,
                HRA = 300000,
                SpecialAllowance = 250000,
                OtherAllowances = 50000,
                InterestOnSavings = 15000,
                InterestOnFixedDeposits = 25000,
                InterestOnBonds = 5000,
                OtherInterestIncome = 0,
                DividendIncomeAI = 5000,
                DividendIncomeAII = 0,
                OtherDividendIncome = 0,
                StandardDeduction = 75000,
                ProfessionalTax = 2400,
                TaxableIncome = 1172600
            },
            Annexure = new AnnexureDataDto
            {
                Q1TDS = 30000,
                Q2TDS = 30000,
                Q3TDS = 30000,
                Q4TDS = 30000
            }
        };

        return Ok(sampleData);
    }

    private Form16DataDto MapToDto(Form16Data form16Data)
    {
        return new Form16DataDto
        {
            EmployeeName = form16Data.EmployeeName,
            PAN = form16Data.PAN,
            AssessmentYear = form16Data.AssessmentYear,
            FinancialYear = form16Data.FinancialYear,
            EmployerName = form16Data.EmployerName,
            TAN = form16Data.TAN,
            GrossSalary = form16Data.GrossSalary,
            TotalTaxDeducted = form16Data.TotalTaxDeducted,
            StandardDeduction = form16Data.StandardDeduction,
            ProfessionalTax = form16Data.ProfessionalTax,
            Form16B = new Form16BDataDto
            {
                SalarySection17 = form16Data.Form16B.SalarySection17,
                Perquisites = form16Data.Form16B.Perquisites,
                ProfitsInLieu = form16Data.Form16B.ProfitsInLieu,
                BasicSalary = form16Data.Form16B.BasicSalary,
                HRA = form16Data.Form16B.HRA,
                SpecialAllowance = form16Data.Form16B.SpecialAllowance,
                OtherAllowances = form16Data.Form16B.OtherAllowances,
                InterestOnSavings = form16Data.Form16B.InterestOnSavings,
                InterestOnFixedDeposits = form16Data.Form16B.InterestOnFixedDeposits,
                InterestOnBonds = form16Data.Form16B.InterestOnBonds,
                OtherInterestIncome = form16Data.Form16B.OtherInterestIncome,
                DividendIncomeAI = form16Data.Form16B.DividendIncomeAI,
                DividendIncomeAII = form16Data.Form16B.DividendIncomeAII,
                OtherDividendIncome = form16Data.Form16B.OtherDividendIncome,
                StandardDeduction = form16Data.Form16B.StandardDeduction,
                ProfessionalTax = form16Data.Form16B.ProfessionalTax,
                TaxableIncome = form16Data.Form16B.TaxableIncome
            },
            Annexure = new AnnexureDataDto
            {
                Q1TDS = form16Data.Annexure.Q1TDS,
                Q2TDS = form16Data.Annexure.Q2TDS,
                Q3TDS = form16Data.Annexure.Q3TDS,
                Q4TDS = form16Data.Annexure.Q4TDS
            }
        };
    }

    private Form16Data MapToModel(Form16DataDto dto)
    {
        return new Form16Data
        {
            EmployeeName = dto.EmployeeName,
            PAN = dto.PAN,
            AssessmentYear = dto.AssessmentYear,
            FinancialYear = dto.FinancialYear,
            EmployerName = dto.EmployerName,
            TAN = dto.TAN,
            GrossSalary = dto.GrossSalary,
            TotalTaxDeducted = dto.TotalTaxDeducted,
            StandardDeduction = dto.StandardDeduction,
            ProfessionalTax = dto.ProfessionalTax,
            Form16B = new Form16BData
            {
                SalarySection17 = dto.Form16B.SalarySection17,
                Perquisites = dto.Form16B.Perquisites,
                ProfitsInLieu = dto.Form16B.ProfitsInLieu,
                BasicSalary = dto.Form16B.BasicSalary,
                HRA = dto.Form16B.HRA,
                SpecialAllowance = dto.Form16B.SpecialAllowance,
                OtherAllowances = dto.Form16B.OtherAllowances,
                InterestOnSavings = dto.Form16B.InterestOnSavings,
                InterestOnFixedDeposits = dto.Form16B.InterestOnFixedDeposits,
                InterestOnBonds = dto.Form16B.InterestOnBonds,
                OtherInterestIncome = dto.Form16B.OtherInterestIncome,
                DividendIncomeAI = dto.Form16B.DividendIncomeAI,
                DividendIncomeAII = dto.Form16B.DividendIncomeAII,
                OtherDividendIncome = dto.Form16B.OtherDividendIncome,
                StandardDeduction = dto.Form16B.StandardDeduction,
                ProfessionalTax = dto.Form16B.ProfessionalTax,
                TaxableIncome = dto.Form16B.TaxableIncome
            },
            Annexure = new AnnexureData
            {
                Q1TDS = dto.Annexure.Q1TDS,
                Q2TDS = dto.Annexure.Q2TDS,
                Q3TDS = dto.Annexure.Q3TDS,
                Q4TDS = dto.Annexure.Q4TDS
            }
        };
    }
}
