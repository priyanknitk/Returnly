using ReturnlyWebApi.Models;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;

namespace ReturnlyWebApi.Services;

public interface IITRFormGenerationService
{
    ITRFormGenerationResult GenerateITRForm(Form16Data form16Data, AdditionalTaxpayerInfo additionalInfo, ITRType? preferredType, 
        decimal preCalculatedTotalIncome, decimal preCalculatedTaxLiability, decimal preCalculatedRefundOrDemand);
    ITRType RecommendITRType(Form16Data form16Data, AdditionalTaxpayerInfo additionalInfo);
    string GenerateITRXml(BaseITRData itrData);
    string GenerateITRJson(BaseITRData itrData);
}

public class ITRFormGenerationService : IITRFormGenerationService
{
    private readonly static JsonSerializerOptions jsonSerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    private readonly ILogger<ITRFormGenerationService> _logger;

    public ITRFormGenerationService(ILogger<ITRFormGenerationService> logger)
    {
        _logger = logger;
    }

    public ITRFormGenerationResult GenerateITRForm(
        Form16Data form16Data,
        AdditionalTaxpayerInfo additionalInfo,
        ITRType? preferredType,
        decimal preCalculatedTotalIncome,
        decimal preCalculatedTaxLiability,
        decimal preCalculatedRefundOrDemand)
    {
        var result = new ITRFormGenerationResult();

        try
        {
            // Step 1: Determine ITR type
            var recommendedType = preferredType ?? RecommendITRType(form16Data, additionalInfo);
            result.RecommendedITRType = recommendedType;

            // Step 2: Generate ITR data based on type
            BaseITRData itrData = recommendedType switch
            {
                ITRType.ITR1 => GenerateITR1Data(form16Data, additionalInfo, preCalculatedTotalIncome, preCalculatedTaxLiability, preCalculatedRefundOrDemand),
                ITRType.ITR2 => GenerateITR2Data(form16Data, additionalInfo, preCalculatedTotalIncome, preCalculatedTaxLiability, preCalculatedRefundOrDemand),
                ITRType.ITR3 => GenerateITR3Data(form16Data, additionalInfo, preCalculatedTotalIncome, preCalculatedTaxLiability, preCalculatedRefundOrDemand),
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
            result.ITRFormXml = GenerateITRXml(itrData);
            result.ITRFormJson = GenerateITRJson(itrData);
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

    public ITRType RecommendITRType(Form16Data form16Data, AdditionalTaxpayerInfo additionalInfo)
    {
        // Log business income detection for debugging
        _logger.LogInformation("ITR Recommendation - Business Income Check: " +
            "HasBusinessIncomeFromAdditionalInfo={HasBusinessIncomeFromAdditionalInfo}, " +
            "BusinessIncomesCount={BusinessIncomesCount}, " +
            "IntradayTradingIncome={IntradayTradingIncome}, " +
            "OtherBusinessIncome={OtherBusinessIncome}",
            additionalInfo.HasBusinessIncome,
            additionalInfo.BusinessIncomes.Count,
            form16Data.IntradayTradingIncome,
            form16Data.OtherBusinessIncome);

        // ITR-3 is required for business/professional income
        // Check both additionalInfo and form16Data for business income
        var hasBusinessIncomeFromAdditionalInfo = additionalInfo.HasBusinessIncome || additionalInfo.BusinessIncomes.Any() ||
                                                 additionalInfo.ProfessionalIncome != null ||
                                                 additionalInfo.BusinessIncomeSmall != null ||
                                                 additionalInfo.LargeBusinessIncome != null;
        var hasBusinessIncomeFromForm16 = form16Data.HasBusinessIncome;

        if (hasBusinessIncomeFromAdditionalInfo || hasBusinessIncomeFromForm16)
        {
            _logger.LogInformation("Recommending ITR-3 due to business income");
            return ITRType.ITR3;
        }

        // Calculate other income from business income categories in AdditionalTaxpayerInfo
        var otherInterestIncome = (additionalInfo.ProfessionalIncome?.InterestOnSavings ?? 0) +
                                 (additionalInfo.BusinessIncomeSmall?.InterestOnSavings ?? 0) +
                                 (additionalInfo.LargeBusinessIncome?.InterestOnSavings ?? 0);

        var otherDividendIncome = (additionalInfo.ProfessionalIncome?.DividendIncome ?? 0) +
                                 (additionalInfo.BusinessIncomeSmall?.DividendIncome ?? 0) +
                                 (additionalInfo.LargeBusinessIncome?.DividendIncome ?? 0);

        var otherSourcesIncome = (additionalInfo.ProfessionalIncome?.OtherIncome ?? 0) +
                                (additionalInfo.BusinessIncomeSmall?.OtherIncome ?? 0) +
                                (additionalInfo.LargeBusinessIncome?.OtherIncome ?? 0);

        var totalIncome = form16Data.GrossSalary +
                         (additionalInfo.HouseProperties?.Sum(h => h.NetIncome) ?? 0) +
                         (additionalInfo.CapitalGains?.Sum(c => c.NetGain) ?? 0) +
                         otherInterestIncome +
                         otherDividendIncome +
                         otherSourcesIncome;

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

    public string GenerateITRXml(BaseITRData itrData)
    {
        return itrData switch
        {
            ITR1Data itr1 => GenerateITR1Xml(itr1),
            ITR2Data itr2 => GenerateITR2Xml(itr2),
            ITR3Data itr3 => GenerateITR3Xml(itr3),
            _ => throw new NotSupportedException($"ITR type {itrData.GetType().Name} is not supported")
        };
    }

    public string GenerateITRJson(BaseITRData itrData)
    {
        return JsonSerializer.Serialize(itrData, jsonSerializerOptions);
    }

    private ITR1Data GenerateITR1Data(Form16Data form16Data, AdditionalTaxpayerInfo additionalInfo, 
        decimal preCalculatedTotalIncome, decimal preCalculatedTaxLiability, decimal preCalculatedRefundOrDemand)
    {
        var itr1 = new ITR1Data
        {
            // Basic information
            AssessmentYear = form16Data.AssessmentYear,
            FinancialYear = form16Data.FinancialYear,
            PAN = form16Data.PAN,
            Name = form16Data.EmployeeName,
            DateOfBirth = additionalInfo.DateOfBirth,
            FatherName = additionalInfo.FatherName,
            Gender = additionalInfo.Gender,
            MaritalStatus = additionalInfo.MaritalStatus,
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
            EmployerAddress = form16Data.EmployerAddress,
            EmployerCategory = form16Data.EmployerCategory,
            EmployerPinCode = form16Data.EmployerPinCode,
            EmployerCountry = form16Data.EmployerCountry,
            EmployerState = form16Data.EmployerState,
            EmployerCity = form16Data.EmployerCity,

            // Salary income
            GrossSalary = form16Data.GrossSalary,
            Perquisites = form16Data.Form16B.Perquisites,
            ProfitsInLieu = form16Data.Form16B.ProfitsInLieu,

            // House property (only first one for ITR-1)
            AnnualValue = additionalInfo.HouseProperties?.FirstOrDefault()?.AnnualValue ?? 0,
            PropertyTax = additionalInfo.HouseProperties?.FirstOrDefault()?.PropertyTax ?? 0,
            InterestOnHomeLoan = additionalInfo.HouseProperties?.FirstOrDefault()?.InterestOnLoan ?? 0,

            // Other sources - derive from business income categories in AdditionalTaxpayerInfo
            InterestFromSavingsAccount = form16Data.Form16B.InterestOnSavings,
            InterestFromDeposits = form16Data.Form16B.InterestOnFixedDeposits + form16Data.Form16B.InterestOnBonds,
            DividendIncome = form16Data.Form16B.DividendIncomeAI + form16Data.Form16B.DividendIncomeAII,
            OtherIncome = (additionalInfo.ProfessionalIncome?.OtherIncome ?? 0) +
                         (additionalInfo.BusinessIncomeSmall?.OtherIncome ?? 0) +
                         (additionalInfo.LargeBusinessIncome?.OtherIncome ?? 0),

            // Deductions
            StandardDeduction = form16Data.StandardDeduction,
            ProfessionalTax = form16Data.ProfessionalTax,

            // Tax details
            TaxDeductedAtSource = form16Data.TotalTaxDeducted,

            // Pre-calculated values (use these instead of recalculating)
            PreCalculatedTotalIncome = preCalculatedTotalIncome,
            PreCalculatedTaxLiability = preCalculatedTaxLiability,
            PreCalculatedRefundOrDemand = preCalculatedRefundOrDemand
        };

        return itr1;
    }

    private ITR2Data GenerateITR2Data(Form16Data form16Data, AdditionalTaxpayerInfo additionalInfo,
        decimal preCalculatedTotalIncome, decimal preCalculatedTaxLiability, decimal preCalculatedRefundOrDemand)
    {
        var itr2 = new ITR2Data
        {
            // Basic information
            AssessmentYear = form16Data.AssessmentYear,
            FinancialYear = form16Data.FinancialYear,
            PAN = form16Data.PAN,
            Name = form16Data.EmployeeName,
            DateOfBirth = additionalInfo.DateOfBirth,
            FatherName = additionalInfo.FatherName,
            Gender = additionalInfo.Gender,
            MaritalStatus = additionalInfo.MaritalStatus,
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
            SalaryDetails =
            [
                new SalaryDetails
                {
                    EmployerName = form16Data.EmployerName,
                    EmployerTAN = form16Data.TAN,
                    EmployerAddress = form16Data.EmployerAddress,
                    EmployerCategory = form16Data.EmployerCategory,
                    EmployerPinCode = form16Data.EmployerPinCode,
                    EmployerCountry = form16Data.EmployerCountry,
                    EmployerState = form16Data.EmployerState,
                    EmployerCity = form16Data.EmployerCity,
                    GrossSalary = form16Data.GrossSalary,
                    TaxDeducted = form16Data.TotalTaxDeducted
                }
            ],

            // House properties
            HouseProperties = additionalInfo.HouseProperties?.Select(h => new HousePropertyDetails
            {
                PropertyAddress = h.PropertyAddress,
                AnnualValue = h.AnnualValue,
                PropertyTax = h.PropertyTax,
                InterestOnLoan = h.InterestOnLoan
            }).ToList() ?? [],

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
            }).ToList() ?? [],

            // Other sources - derive from business income categories in AdditionalTaxpayerInfo
            InterestIncome = form16Data.Form16B.TotalInterestIncome +
                           (additionalInfo.ProfessionalIncome?.InterestOnSavings ?? 0) +
                           (additionalInfo.BusinessIncomeSmall?.InterestOnSavings ?? 0) +
                           (additionalInfo.LargeBusinessIncome?.InterestOnSavings ?? 0),
            DividendIncome = form16Data.Form16B.TotalDividendIncome +
                           (additionalInfo.ProfessionalIncome?.DividendIncome ?? 0) +
                           (additionalInfo.BusinessIncomeSmall?.DividendIncome ?? 0) +
                           (additionalInfo.LargeBusinessIncome?.DividendIncome ?? 0),
            OtherSourcesIncome = (additionalInfo.ProfessionalIncome?.OtherIncome ?? 0) +
                               (additionalInfo.BusinessIncomeSmall?.OtherIncome ?? 0) +
                               (additionalInfo.LargeBusinessIncome?.OtherIncome ?? 0),

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
            }).ToList() ?? [],

            // Deductions
            StandardDeduction = form16Data.StandardDeduction,
            ProfessionalTax = form16Data.ProfessionalTax,

            // Tax details
            TaxDeductedAtSource = form16Data.TotalTaxDeducted,
            TDSDetails =
            [
                new TDSDetails
                {
                    DeductorName = form16Data.EmployerName,
                    DeductorTAN = form16Data.TAN,
                    TaxDeducted = form16Data.TotalTaxDeducted,
                    CertificateNumber = "Form16"
                }
            ],

            // Pre-calculated values (use these instead of recalculating)
            PreCalculatedTotalIncome = preCalculatedTotalIncome,
            PreCalculatedTaxLiability = preCalculatedTaxLiability,
            PreCalculatedRefundOrDemand = preCalculatedRefundOrDemand
        };

        return itr2;
    }

    private ITR3Data GenerateITR3Data(Form16Data form16Data, AdditionalTaxpayerInfo additionalInfo,
        decimal preCalculatedTotalIncome, decimal preCalculatedTaxLiability, decimal preCalculatedRefundOrDemand)
    {

        var itr3 = new ITR3Data
        {
            // Basic information
            AssessmentYear = form16Data.AssessmentYear,
            FinancialYear = form16Data.FinancialYear,
            PAN = form16Data.PAN,
            Name = form16Data.EmployeeName,
            DateOfBirth = additionalInfo.DateOfBirth,
            FatherName = additionalInfo.FatherName,
            Gender = additionalInfo.Gender,
            MaritalStatus = additionalInfo.MaritalStatus,
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

            // Business information (default values - can be enhanced later)
            BusinessName = $"{form16Data.EmployeeName} - Trading Business",
            BusinessAddress = additionalInfo.Address,
            NatureOfBusiness = "Intraday Trading and Investment",
            AccountingMethod = "Cash",
            BusinessStartDate = new DateTime(DateTime.Now.Year - 1, 4, 1), // Start of previous financial year

            // Income collections (calculated properties will be derived from these)
            BusinessIncomes = [],
            BusinessExpenses = [],

            // Other sources - derive from business income categories in AdditionalTaxpayerInfo
            InterestIncome = (additionalInfo.ProfessionalIncome?.InterestOnSavings ?? 0) +
                           (additionalInfo.BusinessIncomeSmall?.InterestOnSavings ?? 0) +
                           (additionalInfo.LargeBusinessIncome?.InterestOnSavings ?? 0),
            DividendIncome = (additionalInfo.ProfessionalIncome?.DividendIncome ?? 0) +
                           (additionalInfo.BusinessIncomeSmall?.DividendIncome ?? 0) +
                           (additionalInfo.LargeBusinessIncome?.DividendIncome ?? 0),
            OtherSourcesIncome = (additionalInfo.ProfessionalIncome?.OtherIncome ?? 0) +
                               (additionalInfo.BusinessIncomeSmall?.OtherIncome ?? 0) +
                               (additionalInfo.LargeBusinessIncome?.OtherIncome ?? 0),

            // Deductions
            StandardDeduction = Math.Min(form16Data.StandardDeduction, 75000),
            ProfessionalTax = form16Data.ProfessionalTax,

            // Foreign income and assets
            HasForeignIncome = additionalInfo.HasForeignIncome,
            ForeignIncome = additionalInfo.ForeignIncome,
            HasForeignAssets = additionalInfo.HasForeignAssets,

            // Tax details
            TaxDeductedAtSource = form16Data.TotalTaxDeducted,
            AdvanceTax = 0, // Can be enhanced later
            SelfAssessmentTax = 0, // Can be enhanced later
            TDSDetails =
            [
                new TDSDetails
                {
                    DeductorName = form16Data.EmployerName,
                    DeductorTAN = form16Data.TAN,
                    TaxDeducted = form16Data.TotalTaxDeducted,
                    CertificateNumber = "Form16"
                }
            ],

            // Pre-calculated values (use these instead of recalculating)
            PreCalculatedTotalIncome = preCalculatedTotalIncome,
            PreCalculatedTaxLiability = preCalculatedTaxLiability,
            PreCalculatedRefundOrDemand = preCalculatedRefundOrDemand
        };

        // Add salary details if available
        if (form16Data.GrossSalary > 0)
        {
            itr3.SalaryDetails.Add(new SalaryDetails
            {
                EmployerName = form16Data.EmployerName,
                EmployerTAN = form16Data.TAN,
                EmployerAddress = form16Data.EmployerAddress,
                EmployerCategory = form16Data.EmployerCategory,
                EmployerPinCode = form16Data.EmployerPinCode,
                EmployerCountry = form16Data.EmployerCountry,
                EmployerState = form16Data.EmployerState,
                EmployerCity = form16Data.EmployerCity,
                GrossSalary = form16Data.GrossSalary,
                TaxDeducted = form16Data.TotalTaxDeducted
            });
        }

        // Add house property details if available
        if (additionalInfo.HouseProperties?.Count > 0)
        {
            foreach (var property in additionalInfo.HouseProperties)
            {
                itr3.HouseProperties.Add(property);
            }
        }

        // Add capital gains if available
        if (additionalInfo.CapitalGains?.Count > 0)
        {
            foreach (var capitalGain in additionalInfo.CapitalGains)
            {
                itr3.CapitalGains.Add(capitalGain);
            }
        }

        // Add business income from Form16Data (intraday trading)
        if (form16Data.IntradayTradingIncome > 0)
        {
            itr3.BusinessIncomes.Add(new BusinessIncomeDetails
            {
                IncomeType = "Intraday Trading",
                Description = "Income from intraday trading activities",
                GrossReceipts = form16Data.IntradayTradingIncome,
                OtherIncome = 0
            });
        }

        // Add other business income from Form16Data
        if (form16Data.OtherBusinessIncome > 0)
        {
            itr3.BusinessIncomes.Add(new BusinessIncomeDetails
            {
                IncomeType = "Other Business Income",
                Description = "Other business income",
                GrossReceipts = form16Data.OtherBusinessIncome,
                OtherIncome = 0
            });
        }

        // Add business income from AdditionalInfo
        if (additionalInfo.BusinessIncomes?.Count > 0)
        {
            foreach (var businessIncome in additionalInfo.BusinessIncomes)
            {
                itr3.BusinessIncomes.Add(new BusinessIncomeDetails
                {
                    IncomeType = businessIncome.IncomeType,
                    Description = businessIncome.Description,
                    GrossReceipts = businessIncome.GrossReceipts,
                    OtherIncome = businessIncome.OtherIncome
                });
            }
        }

        // Add business expenses from Form16Data
        if (form16Data.TradingBusinessExpenses > 0)
        {
            itr3.BusinessExpenses.Add(new BusinessExpenseDetails
            {
                ExpenseCategory = "Trading Expenses",
                Description = "Expenses related to trading activities",
                Amount = form16Data.TradingBusinessExpenses,
                Date = DateTime.Now.Date,
                IsCapitalExpense = false
            });
        }

        if (form16Data.BusinessExpenses > 0)
        {
            itr3.BusinessExpenses.Add(new BusinessExpenseDetails
            {
                ExpenseCategory = "Other Business Expenses",
                Description = "Other business expenses",
                Amount = form16Data.BusinessExpenses,
                Date = DateTime.Now.Date,
                IsCapitalExpense = false
            });
        }

        // Add business expenses from AdditionalInfo
        if (additionalInfo.BusinessExpenses?.Count > 0)
        {
            foreach (var expense in additionalInfo.BusinessExpenses)
            {
                itr3.BusinessExpenses.Add(new BusinessExpenseDetails
                {
                    ExpenseCategory = expense.ExpenseCategory,
                    Description = expense.Description,
                    Amount = expense.Amount,
                    Date = expense.Date,
                    IsCapitalExpense = expense.IsCapitalExpense
                });
            }
        }

        // Note: Calculated properties like TotalBusinessIncome, TotalBusinessExpenses, 
        // NetBusinessIncome, and TotalOtherIncome are automatically calculated
        // from the collections and individual properties we've set above

        return itr3;
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
                    new XElement("EmployerAddress", itr1.EmployerAddress),
                    new XElement("EmployerCategory", itr1.EmployerCategory),
                    new XElement("EmployerPinCode", itr1.EmployerPinCode),
                    new XElement("EmployerCountry", itr1.EmployerCountry),
                    new XElement("EmployerState", itr1.EmployerState),
                    new XElement("EmployerCity", itr1.EmployerCity),
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
                new XElement("TaxLiability", itr1.PreCalculatedTaxLiability),
                new XElement("TDSDetails",
                    new XElement("Q1TDS", itr1.Q1TDS),
                    new XElement("Q2TDS", itr1.Q2TDS),
                    new XElement("Q3TDS", itr1.Q3TDS),
                    new XElement("Q4TDS", itr1.Q4TDS),
                    new XElement("TotalTDS", itr1.TaxDeductedAtSource)
                ),
                new XElement("RefundOrDemand", itr1.PreCalculatedRefundOrDemand)
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
                            new XElement("EmployerAddress", s.EmployerAddress),
                            new XElement("EmployerCategory", s.EmployerCategory),
                            new XElement("EmployerPinCode", s.EmployerPinCode),
                            new XElement("EmployerCountry", s.EmployerCountry),
                            new XElement("EmployerState", s.EmployerState),
                            new XElement("EmployerCity", s.EmployerCity),
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
                new XElement("TaxLiability", itr2.PreCalculatedTaxLiability),
                new XElement("TotalTaxPaid", itr2.TotalTaxPaid),
                new XElement("RefundOrDemand", itr2.PreCalculatedRefundOrDemand)
            )
        );

        return xml.ToString();
    }

    private string GenerateITR3Xml(ITR3Data itr3)
    {
        var xml = new StringBuilder();
        xml.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        xml.AppendLine("<ITR3>");

        // Personal Information
        xml.AppendLine("  <PersonalInfo>");
        xml.AppendLine($"    <AssessmentYear>{itr3.AssessmentYear}</AssessmentYear>");
        xml.AppendLine($"    <FinancialYear>{itr3.FinancialYear}</FinancialYear>");
        xml.AppendLine($"    <PAN>{itr3.PAN}</PAN>");
        xml.AppendLine($"    <Name>{SecurityElement.Escape(itr3.Name)}</Name>");
        xml.AppendLine($"    <Category>{itr3.Category}</Category>");
        if (itr3.Category == TaxpayerCategory.HUF)
        {
            xml.AppendLine($"    <HUFName>{SecurityElement.Escape(itr3.HUFName)}</HUFName>");
        }
        xml.AppendLine($"    <ResidencyStatus>{itr3.ResidencyStatus}</ResidencyStatus>");
        xml.AppendLine($"    <DateOfBirth>{itr3.DateOfBirth:yyyy-MM-dd}</DateOfBirth>");
        xml.AppendLine($"    <Address>{SecurityElement.Escape(itr3.Address)}</Address>");
        xml.AppendLine($"    <City>{SecurityElement.Escape(itr3.City)}</City>");
        xml.AppendLine($"    <State>{SecurityElement.Escape(itr3.State)}</State>");
        xml.AppendLine($"    <Pincode>{itr3.Pincode}</Pincode>");
        xml.AppendLine($"    <EmailAddress>{SecurityElement.Escape(itr3.EmailAddress)}</EmailAddress>");
        xml.AppendLine($"    <MobileNumber>{itr3.MobileNumber}</MobileNumber>");
        xml.AppendLine($"    <AadhaarNumber>{itr3.AadhaarNumber}</AadhaarNumber>");
        xml.AppendLine("  </PersonalInfo>");

        // Business Information
        xml.AppendLine("  <BusinessInfo>");
        xml.AppendLine($"    <BusinessName>{SecurityElement.Escape(itr3.BusinessName)}</BusinessName>");
        xml.AppendLine($"    <BusinessAddress>{SecurityElement.Escape(itr3.BusinessAddress)}</BusinessAddress>");
        xml.AppendLine($"    <NatureOfBusiness>{SecurityElement.Escape(itr3.NatureOfBusiness)}</NatureOfBusiness>");
        xml.AppendLine($"    <AccountingMethod>{itr3.AccountingMethod}</AccountingMethod>");
        xml.AppendLine($"    <BusinessStartDate>{itr3.BusinessStartDate:yyyy-MM-dd}</BusinessStartDate>");
        xml.AppendLine("  </BusinessInfo>");

        // Income Details
        xml.AppendLine("  <IncomeDetails>");
        xml.AppendLine($"    <TotalSalaryIncome>{itr3.TotalSalaryIncome}</TotalSalaryIncome>");
        xml.AppendLine($"    <TotalHousePropertyIncome>{itr3.TotalHousePropertyIncome}</TotalHousePropertyIncome>");
        xml.AppendLine($"    <TotalBusinessIncome>{itr3.TotalBusinessIncome}</TotalBusinessIncome>");
        xml.AppendLine($"    <NetBusinessIncome>{itr3.NetBusinessIncome}</NetBusinessIncome>");
        xml.AppendLine($"    <TotalCapitalGains>{itr3.TotalCapitalGains}</TotalCapitalGains>");
        xml.AppendLine($"    <TotalOtherIncome>{itr3.TotalOtherIncome}</TotalOtherIncome>");
        xml.AppendLine($"    <InterestIncome>{itr3.InterestIncome}</InterestIncome>");
        xml.AppendLine($"    <DividendIncome>{itr3.DividendIncome}</DividendIncome>");
        xml.AppendLine($"    <OtherSourcesIncome>{itr3.OtherSourcesIncome}</OtherSourcesIncome>");
        if (itr3.HasForeignIncome)
        {
            xml.AppendLine($"    <ForeignIncome>{itr3.ForeignIncome}</ForeignIncome>");
        }
        xml.AppendLine($"    <TotalIncome>{itr3.CalculateTotalIncome()}</TotalIncome>");
        xml.AppendLine("  </IncomeDetails>");

        // Business Income Details
        if (itr3.BusinessIncomes.Any())
        {
            xml.AppendLine("  <BusinessIncomes>");
            foreach (var income in itr3.BusinessIncomes)
            {
                xml.AppendLine("    <BusinessIncome>");
                xml.AppendLine($"      <IncomeType>{SecurityElement.Escape(income.IncomeType)}</IncomeType>");
                xml.AppendLine($"      <Description>{SecurityElement.Escape(income.Description)}</Description>");
                xml.AppendLine($"      <GrossReceipts>{income.GrossReceipts}</GrossReceipts>");
                xml.AppendLine($"      <OtherIncome>{income.OtherIncome}</OtherIncome>");
                xml.AppendLine($"      <NetIncome>{income.NetIncome}</NetIncome>");
                xml.AppendLine("    </BusinessIncome>");
            }
            xml.AppendLine("  </BusinessIncomes>");
        }

        // Business Expenses
        if (itr3.BusinessExpenses.Any())
        {
            xml.AppendLine("  <BusinessExpenses>");
            foreach (var expense in itr3.BusinessExpenses)
            {
                xml.AppendLine("    <BusinessExpense>");
                xml.AppendLine($"      <ExpenseCategory>{SecurityElement.Escape(expense.ExpenseCategory)}</ExpenseCategory>");
                xml.AppendLine($"      <Description>{SecurityElement.Escape(expense.Description)}</Description>");
                xml.AppendLine($"      <Amount>{expense.Amount}</Amount>");
                xml.AppendLine($"      <Date>{expense.Date:yyyy-MM-dd}</Date>");
                xml.AppendLine($"      <IsCapitalExpense>{expense.IsCapitalExpense}</IsCapitalExpense>");
                xml.AppendLine("    </BusinessExpense>");
            }
            xml.AppendLine($"    <TotalBusinessExpenses>{itr3.TotalBusinessExpenses}</TotalBusinessExpenses>");
            xml.AppendLine("  </BusinessExpenses>");
        }

        // Tax Details
        xml.AppendLine("  <TaxDetails>");
        xml.AppendLine($"    <TaxDeductedAtSource>{itr3.TaxDeductedAtSource}</TaxDeductedAtSource>");
        xml.AppendLine($"    <AdvanceTax>{itr3.AdvanceTax}</AdvanceTax>");
        xml.AppendLine($"    <SelfAssessmentTax>{itr3.SelfAssessmentTax}</SelfAssessmentTax>");
        xml.AppendLine($"    <TotalTaxPaid>{itr3.TotalTaxPaid}</TotalTaxPaid>");
        xml.AppendLine($"    <TaxLiability>{itr3.PreCalculatedTaxLiability}</TaxLiability>");
        xml.AppendLine($"    <RefundOrDemand>{itr3.PreCalculatedRefundOrDemand}</RefundOrDemand>");
        xml.AppendLine("  </TaxDetails>");

        xml.AppendLine("</ITR3>");

        return xml.ToString();
    }

    private string GenerateSummary(BaseITRData itrData)
    {
        var summary = $"ITR Form: {itrData.GetFormName()}\n";
        summary += $"Assessment Year: {itrData.AssessmentYear}\n";
        summary += $"PAN: {itrData.PAN}\n";
        summary += $"Name: {itrData.Name}\n\n";

        summary += $"Total Income: ₹{itrData.CalculateTotalIncome():N2}\n";
        summary += $"Tax Liability: ₹{itrData.PreCalculatedTaxLiability:N2}\n";

        var refundOrDemand = itrData.PreCalculatedRefundOrDemand;
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
