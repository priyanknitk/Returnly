using ReturnlyWebApi.Models;
using System.Text.Json;
using System.Xml.Linq;

namespace ReturnlyWebApi.Services;

public interface IITRFormGenerationService
{
    Task<ITRFormGenerationResult> GenerateITRFormAsync(Form16Data form16Data, AdditionalTaxpayerInfo additionalInfo, ITRType? preferredType = null);
    Task<ITRType> RecommendITRTypeAsync(Form16Data form16Data, AdditionalTaxpayerInfo additionalInfo);
    Task<string> GenerateITRXmlAsync(BaseITRData itrData);
    Task<string> GenerateITRJsonAsync(BaseITRData itrData);
}

public class ITRFormGenerationService : IITRFormGenerationService
{
    private readonly ILogger<ITRFormGenerationService> _logger;

    public ITRFormGenerationService(ILogger<ITRFormGenerationService> logger)
    {
        _logger = logger;
    }

    public async Task<ITRFormGenerationResult> GenerateITRFormAsync(
        Form16Data form16Data, 
        AdditionalTaxpayerInfo additionalInfo, 
        ITRType? preferredType = null)
    {
        var result = new ITRFormGenerationResult();
        
        try
        {
            // Step 1: Determine ITR type
            var recommendedType = preferredType ?? await RecommendITRTypeAsync(form16Data, additionalInfo);
            result.RecommendedITRType = recommendedType;

            // Step 2: Generate ITR data based on type
            BaseITRData itrData = recommendedType switch
            {
                ITRType.ITR1 => await GenerateITR1DataAsync(form16Data, additionalInfo),
                ITRType.ITR2 => await GenerateITR2DataAsync(form16Data, additionalInfo),
                _ => throw new NotSupportedException($"ITR type {recommendedType} is not supported yet")
            };

            // Step 3: Validate the data
            if (!itrData.ValidateData())
            {
                result.ValidationErrors.Add("Generated ITR data failed validation");
                result.IsSuccess = false;
                return result;
            }

            // Step 4: Generate XML and JSON
            result.ITRFormXml = await GenerateITRXmlAsync(itrData);
            result.ITRFormJson = await GenerateITRJsonAsync(itrData);
            result.FileName = $"{itrData.GetFormName()}_{itrData.PAN}_{itrData.AssessmentYear}.xml";

            // Step 5: Generate summary
            result.GenerationSummary = GenerateSummary(itrData);
            result.IsSuccess = true;

            _logger.LogInformation("Successfully generated {ITRType} for PAN {PAN}", 
                recommendedType, form16Data.PAN);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating ITR form for PAN {PAN}", form16Data.PAN);
            result.ValidationErrors.Add($"Error generating ITR form: {ex.Message}");
            result.IsSuccess = false;
        }

        return result;
    }

    public async Task<ITRType> RecommendITRTypeAsync(Form16Data form16Data, AdditionalTaxpayerInfo additionalInfo)
    {
        await Task.CompletedTask; // For async consistency

        var totalIncome = form16Data.GrossSalary + 
                         (additionalInfo.HouseProperties?.Sum(h => h.NetIncome) ?? 0) +
                         (additionalInfo.CapitalGains?.Sum(c => c.NetGain) ?? 0) +
                         additionalInfo.OtherInterestIncome +
                         additionalInfo.OtherDividendIncome +
                         additionalInfo.OtherSourcesIncome;

        // ITR-1 criteria
        if (totalIncome <= 5000000 && // 50L limit
            !additionalInfo.HasCapitalGains &&
            !additionalInfo.HasForeignIncome &&
            !additionalInfo.HasForeignAssets &&
            (additionalInfo.HouseProperties?.Count ?? 0) <= 1)
        {
            return ITRType.ITR1;
        }

        // ITR-2 for most other individual cases
        return ITRType.ITR2;
    }

    public async Task<string> GenerateITRXmlAsync(BaseITRData itrData)
    {
        await Task.CompletedTask; // For async consistency

        return itrData switch
        {
            ITR1Data itr1 => GenerateITR1Xml(itr1),
            ITR2Data itr2 => GenerateITR2Xml(itr2),
            _ => throw new NotSupportedException($"ITR type {itrData.GetType().Name} is not supported")
        };
    }

    public async Task<string> GenerateITRJsonAsync(BaseITRData itrData)
    {
        await Task.CompletedTask; // For async consistency

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        return JsonSerializer.Serialize(itrData, options);
    }

    private async Task<ITR1Data> GenerateITR1DataAsync(Form16Data form16Data, AdditionalTaxpayerInfo additionalInfo)
    {
        await Task.CompletedTask; // For async consistency

        var itr1 = new ITR1Data
        {
            // Basic information
            AssessmentYear = form16Data.AssessmentYear,
            FinancialYear = form16Data.FinancialYear,
            PAN = form16Data.PAN,
            Name = form16Data.EmployeeName,
            DateOfBirth = additionalInfo.DateOfBirth,
            Address = additionalInfo.Address,
            City = additionalInfo.City,
            State = additionalInfo.State,
            Pincode = additionalInfo.Pincode,
            EmailAddress = additionalInfo.EmailAddress,
            MobileNumber = additionalInfo.MobileNumber,
            AadhaarNumber = additionalInfo.AadhaarNumber,

            // Bank details
            BankAccountNumber = additionalInfo.BankAccountNumber,
            BankIFSCCode = additionalInfo.BankIFSCCode,
            BankName = additionalInfo.BankName,

            // Employer details
            EmployerName = form16Data.EmployerName,
            EmployerTAN = form16Data.TAN,

            // Salary income
            GrossSalary = form16Data.GrossSalary,
            Perquisites = form16Data.Form16B.Perquisites,
            ProfitsInLieu = form16Data.Form16B.ProfitsInLieu,

            // House property (only first one for ITR-1)
            AnnualValue = additionalInfo.HouseProperties?.FirstOrDefault()?.AnnualValue ?? 0,
            PropertyTax = additionalInfo.HouseProperties?.FirstOrDefault()?.PropertyTax ?? 0,
            InterestOnHomeLoan = additionalInfo.HouseProperties?.FirstOrDefault()?.InterestOnLoan ?? 0,

            // Other sources
            InterestFromSavingsAccount = form16Data.Form16B.InterestOnSavings,
            InterestFromDeposits = form16Data.Form16B.InterestOnFixedDeposits + form16Data.Form16B.InterestOnBonds,
            DividendIncome = form16Data.Form16B.DividendIncomeAI + form16Data.Form16B.DividendIncomeAII,
            OtherIncome = additionalInfo.OtherSourcesIncome,

            // Deductions
            StandardDeduction = form16Data.StandardDeduction,
            ProfessionalTax = form16Data.ProfessionalTax,

            // Tax details
            TaxDeductedAtSource = form16Data.TotalTaxDeducted,
            Q1TDS = form16Data.Annexure.Q1TDS,
            Q2TDS = form16Data.Annexure.Q2TDS,
            Q3TDS = form16Data.Annexure.Q3TDS,
            Q4TDS = form16Data.Annexure.Q4TDS
        };

        return itr1;
    }

    private async Task<ITR2Data> GenerateITR2DataAsync(Form16Data form16Data, AdditionalTaxpayerInfo additionalInfo)
    {
        await Task.CompletedTask; // For async consistency

        var itr2 = new ITR2Data
        {
            // Basic information
            AssessmentYear = form16Data.AssessmentYear,
            FinancialYear = form16Data.FinancialYear,
            PAN = form16Data.PAN,
            Name = form16Data.EmployeeName,
            DateOfBirth = additionalInfo.DateOfBirth,
            Address = additionalInfo.Address,
            City = additionalInfo.City,
            State = additionalInfo.State,
            Pincode = additionalInfo.Pincode,
            EmailAddress = additionalInfo.EmailAddress,
            MobileNumber = additionalInfo.MobileNumber,
            AadhaarNumber = additionalInfo.AadhaarNumber,

            // Bank details
            BankAccountNumber = additionalInfo.BankAccountNumber,
            BankIFSCCode = additionalInfo.BankIFSCCode,
            BankName = additionalInfo.BankName,

            // Salary details (can have multiple employers)
            SalaryDetails = new List<SalaryDetails>
            {
                new SalaryDetails
                {
                    EmployerName = form16Data.EmployerName,
                    EmployerTAN = form16Data.TAN,
                    GrossSalary = form16Data.GrossSalary,
                    TaxDeducted = form16Data.TotalTaxDeducted
                }
            },

            // House properties
            HouseProperties = additionalInfo.HouseProperties?.Select(h => new HousePropertyDetails
            {
                PropertyAddress = h.PropertyAddress,
                AnnualValue = h.AnnualValue,
                PropertyTax = h.PropertyTax,
                InterestOnLoan = h.InterestOnLoan
            }).ToList() ?? new List<HousePropertyDetails>(),

            // Capital gains
            CapitalGains = additionalInfo.CapitalGains?.Select(c => new CapitalGainDetails
            {
                AssetType = c.AssetType,
                DateOfSale = c.DateOfSale,
                DateOfPurchase = c.DateOfPurchase,
                SalePrice = c.SalePrice,
                CostOfAcquisition = c.CostOfAcquisition,
                CostOfImprovement = c.CostOfImprovement,
                ExpensesOnTransfer = c.ExpensesOnTransfer
            }).ToList() ?? new List<CapitalGainDetails>(),

            // Other sources
            InterestIncome = form16Data.Form16B.TotalInterestIncome + additionalInfo.OtherInterestIncome,
            DividendIncome = form16Data.Form16B.TotalDividendIncome + additionalInfo.OtherDividendIncome,
            OtherSourcesIncome = additionalInfo.OtherSourcesIncome,

            // Foreign income and assets
            HasForeignIncome = additionalInfo.HasForeignIncome,
            ForeignIncome = additionalInfo.ForeignIncome,
            HasForeignAssets = additionalInfo.HasForeignAssets,
            ForeignAssets = additionalInfo.ForeignAssets?.Select(f => new ForeignAssetDetails
            {
                AssetType = f.AssetType,
                Country = f.Country,
                Value = f.Value,
                Currency = f.Currency
            }).ToList() ?? new List<ForeignAssetDetails>(),

            // Deductions
            StandardDeduction = form16Data.StandardDeduction,
            ProfessionalTax = form16Data.ProfessionalTax,

            // Tax details
            TaxDeductedAtSource = form16Data.TotalTaxDeducted,
            TDSDetails = new List<TDSDetails>
            {
                new TDSDetails
                {
                    DeductorName = form16Data.EmployerName,
                    DeductorTAN = form16Data.TAN,
                    TaxDeducted = form16Data.TotalTaxDeducted,
                    CertificateNumber = "Form16"
                }
            }
        };

        return itr2;
    }

    private string GenerateITR1Xml(ITR1Data itr1)
    {
        var xml = new XElement("ITR1",
            new XAttribute("schemaVersion", "1.0"),
            new XAttribute("formName", "ITR-1"),
            new XAttribute("assessmentYear", itr1.AssessmentYear),
            
            new XElement("PersonalInfo",
                new XElement("Name", itr1.Name),
                new XElement("PAN", itr1.PAN),
                new XElement("DateOfBirth", itr1.DateOfBirth.ToString("dd-MM-yyyy")),
                new XElement("ResidencyStatus", itr1.ResidencyStatus.ToString()),
                new XElement("Address",
                    new XElement("AddressLine", itr1.Address),
                    new XElement("City", itr1.City),
                    new XElement("State", itr1.State),
                    new XElement("Pincode", itr1.Pincode)
                ),
                new XElement("Contact",
                    new XElement("Email", itr1.EmailAddress),
                    new XElement("Mobile", itr1.MobileNumber),
                    new XElement("Aadhaar", itr1.AadhaarNumber)
                ),
                new XElement("BankDetails",
                    new XElement("AccountNumber", itr1.BankAccountNumber),
                    new XElement("IFSCCode", itr1.BankIFSCCode),
                    new XElement("BankName", itr1.BankName)
                )
            ),
            
            new XElement("IncomeDetails",
                new XElement("SalaryIncome",
                    new XElement("EmployerName", itr1.EmployerName),
                    new XElement("EmployerTAN", itr1.EmployerTAN),
                    new XElement("GrossSalary", itr1.GrossSalary),
                    new XElement("TotalSalaryIncome", itr1.TotalSalaryIncome)
                ),
                new XElement("HousePropertyIncome",
                    new XElement("AnnualValue", itr1.AnnualValue),
                    new XElement("PropertyTax", itr1.PropertyTax),
                    new XElement("InterestOnLoan", itr1.InterestOnHomeLoan),
                    new XElement("NetIncome", itr1.TotalHousePropertyIncome)
                ),
                new XElement("OtherSourcesIncome",
                    new XElement("InterestFromSavings", itr1.InterestFromSavingsAccount),
                    new XElement("InterestFromDeposits", itr1.InterestFromDeposits),
                    new XElement("DividendIncome", itr1.DividendIncome),
                    new XElement("OtherIncome", itr1.OtherIncome),
                    new XElement("TotalOtherIncome", itr1.TotalOtherIncome)
                ),
                new XElement("TotalIncome", itr1.CalculateTotalIncome())
            ),
            
            new XElement("Deductions",
                new XElement("StandardDeduction", itr1.StandardDeduction),
                new XElement("ProfessionalTax", itr1.ProfessionalTax),
                new XElement("TotalDeductions", itr1.TotalDeductions)
            ),
            
            new XElement("TaxComputation",
                new XElement("TaxableIncome", itr1.CalculateTotalIncome() - itr1.TotalDeductions),
                new XElement("TaxLiability", itr1.CalculateTaxLiability()),
                new XElement("TDSDetails",
                    new XElement("Q1TDS", itr1.Q1TDS),
                    new XElement("Q2TDS", itr1.Q2TDS),
                    new XElement("Q3TDS", itr1.Q3TDS),
                    new XElement("Q4TDS", itr1.Q4TDS),
                    new XElement("TotalTDS", itr1.TaxDeductedAtSource)
                ),
                new XElement("RefundOrDemand", itr1.CalculateRefundOrDemand())
            )
        );

        return xml.ToString();
    }

    private string GenerateITR2Xml(ITR2Data itr2)
    {
        var xml = new XElement("ITR2",
            new XAttribute("schemaVersion", "1.0"),
            new XAttribute("formName", "ITR-2"),
            new XAttribute("assessmentYear", itr2.AssessmentYear),
            
            new XElement("PersonalInfo",
                new XElement("Name", itr2.Name),
                new XElement("PAN", itr2.PAN),
                new XElement("DateOfBirth", itr2.DateOfBirth.ToString("dd-MM-yyyy")),
                new XElement("ResidencyStatus", itr2.ResidencyStatus.ToString()),
                new XElement("Category", itr2.Category.ToString()),
                new XElement("Address",
                    new XElement("AddressLine", itr2.Address),
                    new XElement("City", itr2.City),
                    new XElement("State", itr2.State),
                    new XElement("Pincode", itr2.Pincode)
                ),
                new XElement("Contact",
                    new XElement("Email", itr2.EmailAddress),
                    new XElement("Mobile", itr2.MobileNumber),
                    new XElement("Aadhaar", itr2.AadhaarNumber)
                ),
                new XElement("BankDetails",
                    new XElement("AccountNumber", itr2.BankAccountNumber),
                    new XElement("IFSCCode", itr2.BankIFSCCode),
                    new XElement("BankName", itr2.BankName)
                )
            ),
            
            new XElement("IncomeDetails",
                new XElement("SalaryIncome",
                    new XElement("TotalSalaryIncome", itr2.TotalSalaryIncome),
                    new XElement("SalaryDetails",
                        itr2.SalaryDetails.Select(s => new XElement("Salary",
                            new XElement("EmployerName", s.EmployerName),
                            new XElement("EmployerTAN", s.EmployerTAN),
                            new XElement("GrossSalary", s.GrossSalary),
                            new XElement("TaxDeducted", s.TaxDeducted)
                        ))
                    )
                ),
                new XElement("HousePropertyIncome",
                    new XElement("TotalHousePropertyIncome", itr2.TotalHousePropertyIncome),
                    new XElement("Properties",
                        itr2.HouseProperties.Select(h => new XElement("Property",
                            new XElement("Address", h.PropertyAddress),
                            new XElement("AnnualValue", h.AnnualValue),
                            new XElement("PropertyTax", h.PropertyTax),
                            new XElement("InterestOnLoan", h.InterestOnLoan),
                            new XElement("NetIncome", h.NetIncome)
                        ))
                    )
                ),
                new XElement("CapitalGains",
                    new XElement("TotalCapitalGains", itr2.TotalCapitalGains),
                    new XElement("CapitalGainDetails",
                        itr2.CapitalGains.Select(c => new XElement("CapitalGain",
                            new XElement("AssetType", c.AssetType),
                            new XElement("DateOfSale", c.DateOfSale.ToString("dd-MM-yyyy")),
                            new XElement("DateOfPurchase", c.DateOfPurchase.ToString("dd-MM-yyyy")),
                            new XElement("SalePrice", c.SalePrice),
                            new XElement("CostOfAcquisition", c.CostOfAcquisition),
                            new XElement("NetGain", c.NetGain),
                            new XElement("IsLongTerm", c.IsLongTerm)
                        ))
                    )
                ),
                new XElement("OtherSourcesIncome",
                    new XElement("InterestIncome", itr2.InterestIncome),
                    new XElement("DividendIncome", itr2.DividendIncome),
                    new XElement("OtherIncome", itr2.OtherSourcesIncome),
                    new XElement("TotalOtherIncome", itr2.TotalOtherIncome)
                ),
                new XElement("ForeignIncome",
                    new XElement("HasForeignIncome", itr2.HasForeignIncome),
                    new XElement("ForeignIncomeAmount", itr2.ForeignIncome)
                ),
                new XElement("TotalIncome", itr2.CalculateTotalIncome())
            ),
            
            new XElement("ForeignAssets",
                new XElement("HasForeignAssets", itr2.HasForeignAssets),
                new XElement("AssetDetails",
                    itr2.ForeignAssets.Select(f => new XElement("Asset",
                        new XElement("AssetType", f.AssetType),
                        new XElement("Country", f.Country),
                        new XElement("Value", f.Value),
                        new XElement("Currency", f.Currency)
                    ))
                )
            ),
            
            new XElement("TaxComputation",
                new XElement("TaxableIncome", itr2.CalculateTotalIncome() - itr2.StandardDeduction - itr2.ProfessionalTax),
                new XElement("TaxLiability", itr2.CalculateTaxLiability()),
                new XElement("TotalTaxPaid", itr2.TotalTaxPaid),
                new XElement("RefundOrDemand", itr2.CalculateRefundOrDemand())
            )
        );

        return xml.ToString();
    }

    private string GenerateSummary(BaseITRData itrData)
    {
        var summary = $"ITR Form: {itrData.GetFormName()}\n";
        summary += $"Assessment Year: {itrData.AssessmentYear}\n";
        summary += $"PAN: {itrData.PAN}\n";
        summary += $"Name: {itrData.Name}\n\n";
        
        summary += $"Total Income: ₹{itrData.CalculateTotalIncome():N2}\n";
        summary += $"Tax Liability: ₹{itrData.CalculateTaxLiability():N2}\n";
        
        var refundOrDemand = itrData.CalculateRefundOrDemand();
        if (refundOrDemand > 0)
        {
            summary += $"Refund Due: ₹{refundOrDemand:N2} 🎉\n";
        }
        else if (refundOrDemand < 0)
        {
            summary += $"Additional Tax Due: ₹{Math.Abs(refundOrDemand):N2} ⚠️\n";
        }
        else
        {
            summary += "Tax liability exactly matches payments ✅\n";
        }
        
        return summary;
    }
}
