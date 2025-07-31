using Microsoft.AspNetCore.Mvc;
using ReturnlyWebApi.DTOs;
using ReturnlyWebApi.Services;
using ReturnlyWebApi.Models;

namespace ReturnlyWebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ITRController : ControllerBase
{
    private readonly IITRFormGenerationService _itrFormGenerationService;
    private readonly ILogger<ITRController> _logger;

    public ITRController(
        IITRFormGenerationService itrFormGenerationService,
        ILogger<ITRController> logger)
    {
        _itrFormGenerationService = itrFormGenerationService;
        _logger = logger;
    }

    /// <summary>
    /// Generate ITR form based on Form16 data and additional information
    /// </summary>
    [HttpPost("generate")]
    public async Task<ActionResult<ITRGenerationResponseDto>> GenerateITRForm([FromBody] ITRGenerationRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            // Check if TaxCalculationResult is provided
            ArgumentNullException.ThrowIfNull(request?.TaxCalculationResult);
            // Check if RefundCalculationResult is provided
            ArgumentNullException.ThrowIfNull(request?.RefundCalculationResult);


            // Convert DTOs to models
            var form16Data = MapForm16DtoToModel(request.Form16Data);
            var additionalInfo = MapAdditionalInfoDtoToModel(request.AdditionalInfo);

            // Extract pre-calculated tax values
            var (preCalculatedTotalIncome, preCalculatedTaxLiability, preCalculatedRefundOrDemand) = 
                ExtractPreCalculatedValues(request);

            // Generate ITR form
            var result = _itrFormGenerationService.GenerateITRForm(
                form16Data, 
                additionalInfo, 
                request.PreferredITRType,
                preCalculatedTotalIncome,
                preCalculatedTaxLiability,
                preCalculatedRefundOrDemand);

            // Convert result to response DTO
            var responseDto = new ITRGenerationResponseDto
            {
                IsSuccess = result.IsSuccess,
                RecommendedITRType = result.RecommendedITRType.ToString(),
                ITRFormXml = result.ITRFormXml,
                ITRFormJson = result.ITRFormJson,
                FileName = result.FileName,
                ValidationErrors = result.ValidationErrors,
                Warnings = result.Warnings,
                GenerationSummary = result.GenerationSummary
            };

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully generated ITR form {ITRType} for PAN {PAN}", 
                    result.RecommendedITRType, form16Data.PAN);
            }

            return Ok(responseDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating ITR form");
            return StatusCode(500, new { error = "An error occurred while generating ITR form", details = ex.Message });
        }
    }

    /// <summary>
    /// Get ITR type recommendation based on income sources
    /// </summary>
    [HttpPost("recommend")]
    public async Task<ActionResult<ITRRecommendationResponseDto>> RecommendITRType([FromBody] ITRRecommendationRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var form16Data = MapForm16DtoToModel(request.Form16Data);
            
            // Check for business income from both request and form16Data
            var hasBusinessIncome = request.HasBusinessIncome || form16Data.HasBusinessIncome;
            
            var additionalInfo = new AdditionalTaxpayerInfo
            {
                HasHouseProperty = request.HasHouseProperty,
                HasCapitalGains = request.HasCapitalGains,
                HasBusinessIncome = hasBusinessIncome,
                HasForeignIncome = request.HasForeignIncome,
                HasForeignAssets = request.HasForeignAssets
            };

            var recommendedType = _itrFormGenerationService.RecommendITRType(form16Data, additionalInfo);

            var response = new ITRRecommendationResponseDto
            {
                RecommendedITRType = recommendedType.ToString(),
                CanUseITR1 = CanUseITR1(request),
                CanUseITR2 = CanUseITR2(request),
                Reason = GetRecommendationReason(recommendedType, request),
                Requirements = GetRequirements(recommendedType),
                Limitations = GetLimitations(recommendedType),
                RecommendationSummary = GetRecommendationSummary(recommendedType, request)
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recommending ITR type");
            return StatusCode(500, new { error = "An error occurred while recommending ITR type", details = ex.Message });
        }
    }

    /// <summary>
    /// Download generated ITR form as XML file
    /// </summary>
    [HttpPost("download/xml")]
    public async Task<IActionResult> DownloadITRXml([FromBody] ITRGenerationRequestDto request)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(request?.TaxCalculationResult);
            ArgumentNullException.ThrowIfNull(request?.RefundCalculationResult);
            var form16Data = MapForm16DtoToModel(request.Form16Data);
            var additionalInfo = MapAdditionalInfoDtoToModel(request.AdditionalInfo);

            // Extract pre-calculated tax values
            var (preCalculatedTotalIncome, preCalculatedTaxLiability, preCalculatedRefundOrDemand) = 
                ExtractPreCalculatedValues(request);

            var result = _itrFormGenerationService.GenerateITRForm(
                form16Data, 
                additionalInfo, 
                request.PreferredITRType,
                preCalculatedTotalIncome,
                preCalculatedTaxLiability,
                preCalculatedRefundOrDemand);

            if (!result.IsSuccess)
            {
                return BadRequest(new { error = "Failed to generate ITR form", details = result.ValidationErrors });
            }

            var fileName = result.FileName;
            var xmlContent = System.Text.Encoding.UTF8.GetBytes(result.ITRFormXml);

            return File(xmlContent, "application/xml", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading ITR XML");
            return StatusCode(500, new { error = "An error occurred while downloading ITR XML", details = ex.Message });
        }
    }

    /// <summary>
    /// Download generated ITR form as JSON file
    /// </summary>
    [HttpPost("download/json")]
    public async Task<IActionResult> DownloadITRJson([FromBody] ITRGenerationRequestDto request)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(request?.TaxCalculationResult);
            ArgumentNullException.ThrowIfNull(request?.RefundCalculationResult);
            var form16Data = MapForm16DtoToModel(request.Form16Data);
            var additionalInfo = MapAdditionalInfoDtoToModel(request.AdditionalInfo);

            // Extract pre-calculated tax values
            var (preCalculatedTotalIncome, preCalculatedTaxLiability, preCalculatedRefundOrDemand) = 
                ExtractPreCalculatedValues(request);

            var result = _itrFormGenerationService.GenerateITRForm(
                form16Data, 
                additionalInfo, 
                request.PreferredITRType,
                preCalculatedTotalIncome,
                preCalculatedTaxLiability,
                preCalculatedRefundOrDemand);

            if (!result.IsSuccess)
            {
                return BadRequest(new { error = "Failed to generate ITR form", details = result.ValidationErrors });
            }

            var fileName = result.FileName.Replace(".xml", ".json");
            var jsonContent = System.Text.Encoding.UTF8.GetBytes(result.ITRFormJson);

            return File(jsonContent, "application/json", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading ITR JSON");
            return StatusCode(500, new { error = "An error occurred while downloading ITR JSON", details = ex.Message });
        }
    }

    /// <summary>
    /// Get sample ITR generation request for testing
    /// </summary>
    [HttpGet("sample")]
    public ActionResult<ITRGenerationRequestDto> GetSampleITRRequest()
    {
        var sampleRequest = new ITRGenerationRequestDto
        {
            Form16Data = new Form16DataDto
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
                    EmployeeName = "Sample Employee",
                    PAN = "ABCDE1234F",
                    AssessmentYear = "2024-25",
                    FinancialYear = "2023-24",
                    EmployerName = "Sample Company Pvt Ltd",
                    TAN = "ABCD12345E",
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
                Form16A = new Form16ADataDto
                {
                    CertificateNumber = "123456789",
                    TotalTaxDeducted = 120000,
                    Q1TDS = 30000,
                    Q2TDS = 30000,
                    Q3TDS = 30000,
                    Q4TDS = 30000
                }
            },
            AdditionalInfo = new AdditionalTaxpayerInfoDto
            {
                DateOfBirth = new DateTime(1990, 1, 15),
                Address = "123 Sample Street, Sample Area",
                City = "Mumbai",
                State = "Maharashtra",
                Pincode = "400001",
                EmailAddress = "sample@example.com",
                MobileNumber = "9876543210",
                AadhaarNumber = "123456789012",
                BankAccountNumber = "1234567890",
                BankIFSCCode = "HDFC0000123",
                BankName = "HDFC Bank",
                HasHouseProperty = false,
                HasCapitalGains = false,
                HasForeignIncome = false,
                HasForeignAssets = false
            }
        };

        return Ok(sampleRequest);
    }

    private Form16Data MapForm16DtoToModel(Form16DataDto dto)
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
            // Business income fields
            IntradayTradingIncome = dto.IntradayTradingIncome,
            TradingBusinessExpenses = dto.TradingBusinessExpenses,
            OtherBusinessIncome = dto.OtherBusinessIncome,
            BusinessExpenses = dto.BusinessExpenses,
            Form16B = new Form16BData
            {
                EmployeeName = dto.Form16B.EmployeeName,
                PAN = dto.Form16B.PAN,
                AssessmentYear = dto.Form16B.AssessmentYear,
                FinancialYear = dto.Form16B.FinancialYear,
                EmployerName = dto.Form16B.EmployerName,
                TAN = dto.Form16B.TAN,
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
            Form16A = new Form16AData
            {
                CertificateNumber = dto.Form16A.CertificateNumber,
                TotalTaxDeducted = dto.Form16A.TotalTaxDeducted,
            }
        };
    }

    private AdditionalTaxpayerInfo MapAdditionalInfoDtoToModel(AdditionalTaxpayerInfoDto dto)
    {
        return new AdditionalTaxpayerInfo
        {
            DateOfBirth = dto.DateOfBirth,
            Address = dto.Address,
            City = dto.City,
            State = dto.State,
            Pincode = dto.Pincode,
            EmailAddress = dto.EmailAddress,
            MobileNumber = dto.MobileNumber,
            AadhaarNumber = dto.AadhaarNumber,
            BankAccountNumber = dto.BankAccountNumber,
            BankIFSCCode = dto.BankIFSCCode,
            BankName = dto.BankName,
            HasHouseProperty = dto.HasHouseProperty,
            HouseProperties = dto.HouseProperties?.Select(h => new HousePropertyDetails
            {
                PropertyAddress = h.PropertyAddress,
                AnnualValue = h.AnnualValue,
                PropertyTax = h.PropertyTax,
                InterestOnLoan = h.InterestOnLoan
            }).ToList() ?? new List<HousePropertyDetails>(),
            HasCapitalGains = dto.HasCapitalGains,
            CapitalGains = dto.CapitalGains?.Select(c => new CapitalGainDetails
            {
                AssetType = c.AssetType,
                DateOfSale = c.DateOfSale,
                DateOfPurchase = c.DateOfPurchase,
                SalePrice = c.SalePrice,
                CostOfAcquisition = c.CostOfAcquisition,
                CostOfImprovement = c.CostOfImprovement,
                ExpensesOnTransfer = c.ExpensesOnTransfer
            }).ToList() ?? new List<CapitalGainDetails>(),
            HasForeignIncome = dto.HasForeignIncome,
            ForeignIncome = dto.ForeignIncome,
            HasForeignAssets = dto.HasForeignAssets,
            ForeignAssets = dto.ForeignAssets?.Select(f => new ForeignAssetDetails
            {
                AssetType = f.AssetType,
                Country = f.Country,
                Value = f.Value,
                Currency = f.Currency
            }).ToList() ?? new List<ForeignAssetDetails>(),
            HasBusinessIncome = dto.HasBusinessIncome,
            BusinessIncomes = dto.BusinessIncomes?.Select(b => new BusinessIncomeDetails
            {
                IncomeType = b.IncomeType,
                Description = b.Description,
                GrossReceipts = b.GrossReceipts,
                OtherIncome = b.OtherIncome
            }).ToList() ?? new List<BusinessIncomeDetails>(),
            BusinessExpenses = dto.BusinessExpenses?.Select(e => new BusinessExpenseDetails
            {
                ExpenseCategory = e.ExpenseCategory,
                Description = e.Description,
                Amount = e.Amount,
                Date = e.Date,
                IsCapitalExpense = e.IsCapitalExpense
            }).ToList() ?? new List<BusinessExpenseDetails>(),
            
            // Map new business income structures
            ProfessionalIncome = dto.ProfessionalIncome == null ? null : new Models.ProfessionalIncome
            {
                GrossReceipts = dto.ProfessionalIncome.GrossReceipts,
                OtherIncome = dto.ProfessionalIncome.OtherIncome,
                InterestOnSavings = dto.ProfessionalIncome.InterestOnSavings,
                DividendIncome = dto.ProfessionalIncome.DividendIncome,
                AdvertisingExpenses = dto.ProfessionalIncome.AdvertisingExpenses,
                TravelExpenses = dto.ProfessionalIncome.TravelExpenses,
                OfficeExpenses = dto.ProfessionalIncome.OfficeExpenses,
                OtherExpenses = dto.ProfessionalIncome.OtherExpenses
            },
            BusinessIncomeSmall = dto.BusinessIncomeSmall == null ? null : new Models.BusinessIncomeSmall
            {
                GrossReceipts = dto.BusinessIncomeSmall.GrossReceipts,
                OtherIncome = dto.BusinessIncomeSmall.OtherIncome,
                InterestOnSavings = dto.BusinessIncomeSmall.InterestOnSavings,
                DividendIncome = dto.BusinessIncomeSmall.DividendIncome,
                OpeningStock = dto.BusinessIncomeSmall.OpeningStock,
                Purchases = dto.BusinessIncomeSmall.Purchases,
                DirectExpenses = dto.BusinessIncomeSmall.DirectExpenses,
                ClosingStock = dto.BusinessIncomeSmall.ClosingStock,
                EmployeeCosts = dto.BusinessIncomeSmall.EmployeeCosts,
                OfficeExpenses = dto.BusinessIncomeSmall.OfficeExpenses,
                TravelExpenses = dto.BusinessIncomeSmall.TravelExpenses,
                OtherExpenses = dto.BusinessIncomeSmall.OtherExpenses
            },
            LargeBusinessIncome = dto.LargeBusinessIncome == null ? null : new Models.LargeBusinessIncome
            {
                GrossReceipts = dto.LargeBusinessIncome.GrossReceipts,
                OtherIncome = dto.LargeBusinessIncome.OtherIncome,
                InterestOnSavings = dto.LargeBusinessIncome.InterestOnSavings,
                DividendIncome = dto.LargeBusinessIncome.DividendIncome,
                OpeningStock = dto.LargeBusinessIncome.OpeningStock,
                Purchases = dto.LargeBusinessIncome.Purchases,
                DirectExpenses = dto.LargeBusinessIncome.DirectExpenses,
                ClosingStock = dto.LargeBusinessIncome.ClosingStock,
                EmployeeCosts = dto.LargeBusinessIncome.EmployeeCosts,
                OfficeExpenses = dto.LargeBusinessIncome.OfficeExpenses,
                TravelExpenses = dto.LargeBusinessIncome.TravelExpenses,
                AdministrativeExpenses = dto.LargeBusinessIncome.AdministrativeExpenses,
                SellingExpenses = dto.LargeBusinessIncome.SellingExpenses,
                FinancialCosts = dto.LargeBusinessIncome.FinancialCosts,
                DepreciationExpenses = dto.LargeBusinessIncome.DepreciationExpenses,
                OtherExpenses = dto.LargeBusinessIncome.OtherExpenses
            }
        };
    }

    private bool CanUseITR1(ITRRecommendationRequestDto request)
    {
        return request.TotalIncome <= 5000000 &&
               !request.HasCapitalGains &&
               !request.HasBusinessIncome &&
               !request.HasForeignIncome &&
               !request.HasForeignAssets &&
               !request.IsHUF;
    }

    private bool CanUseITR2(ITRRecommendationRequestDto request)
    {
        var form16Data = MapForm16DtoToModel(request.Form16Data);
        var hasBusinessIncome = request.HasBusinessIncome || form16Data.HasBusinessIncome;
        return !hasBusinessIncome; // ITR-2 is for individuals/HUF without business income
    }

    private string GetRecommendationReason(ITRType recommendedType, ITRRecommendationRequestDto request)
    {
        return recommendedType switch
        {
            ITRType.ITR1 => "Your income profile fits ITR-1 (Sahaj) criteria: salary income up to ₹50L with simple income sources.",
            ITRType.ITR2 => "ITR-2 is recommended due to capital gains, multiple income sources, or foreign income/assets.",
            ITRType.ITR3 => "ITR-3 is required for business or professional income including intraday trading.",
            _ => "This ITR type is recommended based on your income sources and complexity."
        };
    }

    private List<string> GetRequirements(ITRType itrType)
    {
        return itrType switch
        {
            ITRType.ITR1 => new List<string>
            {
                "Total income should not exceed ₹50,00,000",
                "Only salary income and one house property allowed",
                "No capital gains allowed",
                "No business/professional income",
                "Individual taxpayers only (not HUF)"
            },
            ITRType.ITR2 => new List<string>
            {
                "For individuals and HUFs",
                "Can have multiple income sources",
                "Allows capital gains and house property income",
                "Can declare foreign income and assets",
                "No business/professional income allowed"
            },
            ITRType.ITR3 => new List<string>
            {
                "For individuals and HUFs with business/professional income",
                "Required for intraday trading income",
                "Allows all types of income including business",
                "Balance Sheet and P&L statements may be required",
                "Audit may be required if business income exceeds certain limits"
            },
            _ => new List<string> { "Requirements depend on specific ITR type" }
        };
    }

    private List<string> GetLimitations(ITRType itrType)
    {
        return itrType switch
        {
            ITRType.ITR1 => new List<string>
            {
                "Income limit of ₹50,00,000",
                "Only one house property allowed",
                "Cannot report capital gains",
                "Limited deductions available (new tax regime focus)"
            },
            ITRType.ITR2 => new List<string>
            {
                "Cannot be used for business/professional income",
                "More complex than ITR-1",
                "Requires detailed reporting of all income sources"
            },
            ITRType.ITR3 => new List<string>
            {
                "Most complex ITR form",
                "Requires detailed business records",
                "May require professional accounting help",
                "Balance Sheet and P&L statements required for certain cases",
                "Audit compliance may be necessary"
            },
            _ => new List<string> { "Limitations depend on specific ITR type" }
        };
    }

    private string GetRecommendationSummary(ITRType recommendedType, ITRRecommendationRequestDto request)
    {
        var form16Data = MapForm16DtoToModel(request.Form16Data);
        var hasBusinessIncome = request.HasBusinessIncome || form16Data.HasBusinessIncome;
        
        var summary = $"Based on your income of ₹{request.TotalIncome:N2}, ";
        
        if (hasBusinessIncome)
        {
            summary += "ITR-3 is required for business/professional income including intraday trading. ";
        }
        else if (request.HasCapitalGains || request.HasForeignIncome || request.HasForeignAssets)
        {
            summary += "ITR-2 is required due to capital gains or foreign income/assets. ";
        }
        else if (request.TotalIncome > 5000000)
        {
            summary += "ITR-2 is required as income exceeds ₹50L limit for ITR-1. ";
        }
        else
        {
            summary += "ITR-1 (Sahaj) is the simplest option for your income profile. ";
        }

        summary += $"Recommended form: {recommendedType}";
        
        return summary;
    }

    private (decimal totalIncome, decimal taxLiability, decimal refundOrDemand) ExtractPreCalculatedValues(ITRGenerationRequestDto request)
    {
        decimal preCalculatedTotalIncome = 0;
        decimal preCalculatedTaxLiability = 0;
        decimal preCalculatedRefundOrDemand = 0;

        if (request.TaxCalculationResult != null)
        {
            preCalculatedTaxLiability = request.TaxCalculationResult.TotalTaxWithCess;
            
            // Calculate total income from taxable income + deductions
            var standardDeduction = request.Form16Data.StandardDeduction;
            var professionalTax = request.Form16Data.ProfessionalTax;
            preCalculatedTotalIncome = request.TaxCalculationResult.TaxableIncome + standardDeduction + professionalTax;
        }

        if (request.RefundCalculationResult != null)
        {
            preCalculatedRefundOrDemand = request.RefundCalculationResult.IsRefundDue 
                ? request.RefundCalculationResult.RefundAmount 
                : -request.RefundCalculationResult.AdditionalTaxDue;
        }

        return (preCalculatedTotalIncome, preCalculatedTaxLiability, preCalculatedRefundOrDemand);
    }
}
