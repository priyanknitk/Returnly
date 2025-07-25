using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Returnly.Models;
using Returnly.Extensions;

namespace Returnly.Services
{
    /// <summary>
    /// Service responsible for generating ITR forms from Form16 data
    /// </summary>
    public class ITRFormGenerationService
    {
        private const string ITR1FileName = "ITR1_Sahaj.xml";
        private const string ITR2FileName = "ITR2.xml";
        private readonly ITRSelectionService _itrSelectionService;
        
        public ITRFormGenerationService(ITRSelectionService itrSelectionService)
        {
            _itrSelectionService = itrSelectionService ?? throw new ArgumentNullException(nameof(itrSelectionService));
        }

        /// <summary>
        /// Generates appropriate ITR form data from Form16 data
        /// </summary>
        /// <param name="form16Data">Source Form16 data</param>
        /// <param name="additionalInfo">Additional information not in Form16</param>
        /// <returns>Generated ITR form data</returns>
        public async Task<ITRFormGenerationResult> GenerateITRFormAsync(
            Form16Data form16Data, 
            AdditionalTaxpayerInfo additionalInfo)
        {
            if (form16Data == null)
                throw new ArgumentNullException(nameof(form16Data));
            
            if (additionalInfo == null)
                throw new ArgumentNullException(nameof(additionalInfo));

            var result = new ITRFormGenerationResult();
            
            try
            {
                // Step 1: Determine appropriate ITR type
                var selectionCriteria = form16Data.ToITRSelectionCriteria(
                    additionalInfo);
                
                // Override criteria with additional info
                UpdateSelectionCriteriaWithAdditionalInfo(selectionCriteria, additionalInfo);
                
                var itrSelection = _itrSelectionService.DetermineITRType(selectionCriteria);
                result.ITRSelection = itrSelection;
                
                // Step 2: Generate appropriate ITR form data
                switch (itrSelection.RecommendedITR)
                {
                    case ITRType.ITR1_Sahaj:
                        result.ITR1Data = await GenerateITR1DataAsync(form16Data, additionalInfo);
                        result.GeneratedFormType = ITRType.ITR1_Sahaj;
                        break;
                        
                    case ITRType.ITR2:
                        result.ITR2Data = await GenerateITR2DataAsync(form16Data, additionalInfo);
                        result.GeneratedFormType = ITRType.ITR2;
                        break;
                        
                    default:
                        result.ErrorMessage = $"ITR form type {itrSelection.RecommendedITR} is not supported for auto-generation";
                        result.IsSuccess = false;
                        return result;
                }
                
                // Step 3: Validate generated data
                var validationResult = result.GeneratedFormType == ITRType.ITR1_Sahaj 
                    ? ValidateITR1Data(result.ITR1Data!)
                    : ValidateITR2Data(result.ITR2Data!);
                
                result.ValidationWarnings.AddRange(validationResult.Warnings);
                result.ValidationErrors.AddRange(validationResult.Errors);
                
                // Step 4: Generate XML if validation passes
                if (validationResult.IsValid)
                {
                    var xmlExportService = new ITRXMLExportService();
                    
                    if (result.GeneratedFormType == ITRType.ITR1_Sahaj)
                    {
                        var xmlResult = await xmlExportService.ExportITR1ToXMLAsync(result.ITR1Data!, ITR1FileName);
                        result.XMLContent = xmlResult.XMLContent;
                        result.XMLFileName = xmlResult.FileName;
                    }
                    else if (result.GeneratedFormType == ITRType.ITR2)
                    {
                        var xmlResult = await xmlExportService.ExportITR2ToXMLAsync(result.ITR2Data!, ITR2FileName);
                        result.XMLContent = xmlResult.XMLContent;
                        result.XMLFileName = xmlResult.FileName;
                    }
                }
                
                result.IsSuccess = validationResult.IsValid;
                result.GeneratedAt = DateTime.Now;
                
                return result;
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = $"Error generating ITR form: {ex.Message}";
                return result;
            }
        }

        /// <summary>
        /// Generates ITR-1 (Sahaj) form data
        /// </summary>
        private async Task<ITR1Data> GenerateITR1DataAsync(Form16Data form16Data, AdditionalTaxpayerInfo additionalInfo)
        {
            return await Task.Run(() =>
            {
                var itr1 = new ITR1Data
                {
                    // Basic Information
                    AssessmentYear = form16Data.AssessmentYear,
                    FinancialYear = form16Data.FinancialYear,
                    PAN = form16Data.PAN,
                    Name = form16Data.EmployeeName,
                    DateOfBirth = additionalInfo.DateOfBirth,
                    ResidencyStatus = additionalInfo.ResidencyStatus,
                    Address = additionalInfo.Address,
                    City = additionalInfo.City,
                    State = additionalInfo.State,
                    Pincode = additionalInfo.Pincode,
                    EmailAddress = additionalInfo.EmailAddress,
                    MobileNumber = additionalInfo.MobileNumber,
                    AadhaarNumber = additionalInfo.AadhaarNumber,
                    
                    // Bank Details
                    BankAccountNumber = additionalInfo.BankAccountNumber,
                    BankIFSCCode = additionalInfo.BankIFSCode,
                    BankName = additionalInfo.BankName,
                    
                    // Employer Details
                    EmployerName = form16Data.EmployerName,
                    EmployerTAN = form16Data.TAN,
                    EmployerAddress = additionalInfo.EmployerAddress,
                    
                    // Salary Income
                    GrossSalary = form16Data.Form16B.SalarySection17,
                    Perquisites = form16Data.Form16B.Perquisites,
                    ProfitsInLieu = form16Data.Form16B.ProfitsInLieu,
                    
                    // House Property (assuming single property)
                    AnnualValue = additionalInfo.HousePropertyAnnualValue,
                    PropertyTax = additionalInfo.PropertyTaxPaid,
                    InterestOnHomeLoan = additionalInfo.HomeLoanInterest,
                    
                    // Other Sources Income
                    InterestFromSavingsAccount = form16Data.Form16B.InterestOnSavings,
                    InterestFromDeposits = form16Data.Form16B.InterestOnFixedDeposits + form16Data.Form16B.InterestOnBonds,
                    DividendIncome = form16Data.Form16B.TotalDividendIncome,
                    OtherIncome = form16Data.Form16B.OtherInterestIncome,
                    
                    // Deductions
                    StandardDeduction = form16Data.Form16B.StandardDeduction,
                    ProfessionalTax = form16Data.Form16B.ProfessionalTax,
                    
                    // Tax Details
                    TaxDeductedAtSource = form16Data.TotalTaxDeducted,
                    Q1TDS = form16Data.Annexure.Q1TDS,
                    Q2TDS = form16Data.Annexure.Q2TDS,
                    Q3TDS = form16Data.Annexure.Q3TDS,
                    Q4TDS = form16Data.Annexure.Q4TDS,
                    
                    // Additional tax payments
                    AdvanceTax = additionalInfo.AdvanceTaxPaid,
                    SelfAssessmentTax = additionalInfo.SelfAssessmentTaxPaid
                };
                
                // Calculate standard deduction for house property (30% of annual value)
                if (itr1.AnnualValue > 0)
                {
                    itr1.StandardDeduction30Percent = itr1.AnnualValue * 0.3m;
                }
                
                return itr1;
            });
        }

        /// <summary>
        /// Generates ITR-2 form data
        /// </summary>
        private async Task<ITR2Data> GenerateITR2DataAsync(Form16Data form16Data, AdditionalTaxpayerInfo additionalInfo)
        {
            return await Task.Run(() =>
            {
                var itr2 = new ITR2Data
                {
                    // Basic Information
                    AssessmentYear = form16Data.AssessmentYear,
                    FinancialYear = form16Data.FinancialYear,
                    PAN = form16Data.PAN,
                    Name = form16Data.EmployeeName,
                    DateOfBirth = additionalInfo.DateOfBirth,
                    ResidencyStatus = additionalInfo.ResidencyStatus,
                    Category = additionalInfo.TaxpayerCategory,
                    Address = additionalInfo.Address,
                    City = additionalInfo.City,
                    State = additionalInfo.State,
                    Pincode = additionalInfo.Pincode,
                    EmailAddress = additionalInfo.EmailAddress,
                    MobileNumber = additionalInfo.MobileNumber,
                    AadhaarNumber = additionalInfo.AadhaarNumber,
                    
                    // Bank Details
                    BankAccountNumber = additionalInfo.BankAccountNumber,
                    BankIFSCCode = additionalInfo.BankIFSCode,
                    BankName = additionalInfo.BankName,
                    
                    // Salary Details (can have multiple employers)
                    SalaryDetails = new List<SalaryDetails>
                    {
                        new SalaryDetails
                        {
                            EmployerName = form16Data.EmployerName,
                            EmployerTAN = form16Data.TAN,
                            GrossSalary = form16Data.Form16B.GrossSalary,
                            TaxDeducted = form16Data.TotalTaxDeducted,
                            CertificateNumber = form16Data.Form16A.CertificateNumber
                        }
                    },
                    
                    // House Properties
                    HouseProperties = CreateHousePropertyList(additionalInfo),
                    
                    // Capital Gains
                    CapitalGains = additionalInfo.CapitalGains ?? new List<CapitalGainDetails>(),
                    
                    // Other Sources
                    InterestIncome = form16Data.Form16B.TotalInterestIncome,
                    DividendIncome = form16Data.Form16B.TotalDividendIncome,
                    OtherSourcesIncome = 0,
                    
                    // Foreign Income/Assets
                    HasForeignIncome = additionalInfo.HasForeignIncome,
                    ForeignIncome = additionalInfo.ForeignIncome,
                    HasForeignAssets = additionalInfo.HasForeignAssets,
                    ForeignAssets = additionalInfo.ForeignAssets ?? new List<ForeignAssetDetails>(),
                    
                    // Deductions
                    StandardDeduction = form16Data.Form16B.StandardDeduction,
                    ProfessionalTax = form16Data.Form16B.ProfessionalTax,
                    
                    // Tax Details
                    TaxDeductedAtSource = form16Data.TotalTaxDeducted,
                    AdvanceTax = additionalInfo.AdvanceTaxPaid,
                    SelfAssessmentTax = additionalInfo.SelfAssessmentTaxPaid,
                    
                    // TDS Details
                    TDSDetails = new List<TDSDetails>
                    {
                        new TDSDetails
                        {
                            DeductorName = form16Data.EmployerName,
                            DeductorTAN = form16Data.TAN,
                            TaxDeducted = form16Data.TotalTaxDeducted,
                            CertificateNumber = form16Data.Form16A.CertificateNumber,
                            DateOfDeduction = DateTime.Parse($"31/03/{form16Data.FinancialYear.Split('-')[1]}")
                        }
                    }
                };
                
                return itr2;
            });
        }

        /// <summary>
        /// Updates ITR selection criteria with additional taxpayer information
        /// </summary>
        private void UpdateSelectionCriteriaWithAdditionalInfo(
            ITRSelectionCriteria criteria, 
            AdditionalTaxpayerInfo additionalInfo)
        {
            criteria.CapitalGains = additionalInfo.CapitalGains?.Sum(c => c.NetGain) ?? 0;
            criteria.HousePropertyIncome = additionalInfo.HousePropertyAnnualValue - 
                                          additionalInfo.PropertyTaxPaid - 
                                          (additionalInfo.HousePropertyAnnualValue * 0.3m) - 
                                          additionalInfo.HomeLoanInterest;
            
            criteria.HasMultipleHouseProperties = (additionalInfo.HouseProperties?.Count ?? 0) > 1;
            criteria.HasForeignIncome = additionalInfo.HasForeignIncome;
            criteria.HasForeignAssets = additionalInfo.HasForeignAssets;
            criteria.IsDirectorOfCompany = additionalInfo.IsDirectorOfCompany;
            criteria.HasUnlistedShares = additionalInfo.HasUnlistedShares;
            criteria.HasLossesFromPreviousYear = additionalInfo.HasLossesFromPreviousYear;
        }

        /// <summary>
        /// Creates house property list from additional info
        /// </summary>
        private List<HousePropertyDetails> CreateHousePropertyList(AdditionalTaxpayerInfo additionalInfo)
        {
            var properties = new List<HousePropertyDetails>();
            
            // Add primary house property if exists
            if (additionalInfo.HousePropertyAnnualValue > 0)
            {
                properties.Add(new HousePropertyDetails
                {
                    PropertyAddress = additionalInfo.PropertyAddress ?? "Primary Property",
                    AnnualValue = additionalInfo.HousePropertyAnnualValue,
                    PropertyTax = additionalInfo.PropertyTaxPaid,
                    InterestOnLoan = additionalInfo.HomeLoanInterest
                });
            }
            
            // Add additional properties if provided
            if (additionalInfo.HouseProperties != null)
            {
                properties.AddRange(additionalInfo.HouseProperties);
            }
            
            return properties;
        }

        /// <summary>
        /// Validates ITR-1 form data
        /// </summary>
        private ITRFormValidationResult ValidateITR1Data(ITR1Data itr1Data)
        {
            var result = new ITRFormValidationResult();
            
            // Mandatory field validations
            if (string.IsNullOrWhiteSpace(itr1Data.PAN))
                result.Errors.Add("PAN is mandatory");
            else if (!System.Text.RegularExpressions.Regex.IsMatch(itr1Data.PAN, @"^[A-Z]{5}[0-9]{4}[A-Z]{1}$"))
                result.Errors.Add("PAN format is invalid");
                
            if (string.IsNullOrWhiteSpace(itr1Data.Name))
                result.Errors.Add("Name is mandatory");
                
            if (string.IsNullOrWhiteSpace(itr1Data.AssessmentYear))
                result.Errors.Add("Assessment Year is mandatory");
                
            if (string.IsNullOrWhiteSpace(itr1Data.FinancialYear))
                result.Errors.Add("Financial Year is mandatory");
                
            if (itr1Data.DateOfBirth == DateTime.MinValue)
                result.Errors.Add("Date of Birth is mandatory");
                
            // Business logic validations
            if (itr1Data.CalculateTotalIncome() > 5000000)
                result.Errors.Add("Total income exceeds ITR-1 limit of ₹50 lakhs");
                
            // Contact information warnings
            if (string.IsNullOrWhiteSpace(itr1Data.EmailAddress))
                result.Warnings.Add("Email address is recommended for e-filing");
                
            if (string.IsNullOrWhiteSpace(itr1Data.MobileNumber))
                result.Warnings.Add("Mobile number is recommended for OTP verification");
                
            // Bank details for refund
            if (itr1Data.CalculateRefundOrDemand() > 0)
            {
                if (string.IsNullOrWhiteSpace(itr1Data.BankAccountNumber))
                    result.Errors.Add("Bank account number is mandatory for refund");
                    
                if (string.IsNullOrWhiteSpace(itr1Data.BankIFSCCode))
                    result.Errors.Add("Bank IFSC code is mandatory for refund");
            }
            
            result.IsValid = result.Errors.Count == 0;
            return result;
        }

        /// <summary>
        /// Validates ITR-2 form data
        /// </summary>
        private ITRFormValidationResult ValidateITR2Data(ITR2Data itr2Data)
        {
            var result = new ITRFormValidationResult();
            
            // Mandatory field validations
            if (string.IsNullOrWhiteSpace(itr2Data.PAN))
                result.Errors.Add("PAN is mandatory");
            else if (!System.Text.RegularExpressions.Regex.IsMatch(itr2Data.PAN, @"^[A-Z]{5}[0-9]{4}[A-Z]{1}$"))
                result.Errors.Add("PAN format is invalid");
                
            if (string.IsNullOrWhiteSpace(itr2Data.Name))
                result.Errors.Add("Name is mandatory");
                
            if (string.IsNullOrWhiteSpace(itr2Data.AssessmentYear))
                result.Errors.Add("Assessment Year is mandatory");
                
            if (string.IsNullOrWhiteSpace(itr2Data.FinancialYear))
                result.Errors.Add("Financial Year is mandatory");
                
            if (itr2Data.DateOfBirth == DateTime.MinValue)
                result.Errors.Add("Date of Birth is mandatory");
                
            // Salary details validation
            if (itr2Data.SalaryDetails == null || itr2Data.SalaryDetails.Count == 0)
                result.Warnings.Add("No salary details provided");
            else
            {
                foreach (var salary in itr2Data.SalaryDetails)
                {
                    if (string.IsNullOrWhiteSpace(salary.EmployerTAN))
                        result.Warnings.Add($"TAN missing for employer: {salary.EmployerName}");
                }
            }
            
            // Capital gains validation
            if (itr2Data.CapitalGains != null && itr2Data.CapitalGains.Any())
            {
                foreach (var gain in itr2Data.CapitalGains)
                {
                    if (gain.DateOfSale <= gain.DateOfPurchase)
                        result.Errors.Add("Sale date must be after purchase date for capital gains");
                }
            }
            
            // Foreign income/assets validation
            if (itr2Data.HasForeignIncome && itr2Data.ForeignIncome <= 0)
                result.Warnings.Add("Foreign income flag is set but amount is zero");
                
            if (itr2Data.HasForeignAssets && (itr2Data.ForeignAssets == null || !itr2Data.ForeignAssets.Any()))
                result.Warnings.Add("Foreign assets flag is set but no assets listed");
            
            // Contact information warnings
            if (string.IsNullOrWhiteSpace(itr2Data.EmailAddress))
                result.Warnings.Add("Email address is recommended for e-filing");
                
            if (string.IsNullOrWhiteSpace(itr2Data.MobileNumber))
                result.Warnings.Add("Mobile number is recommended for OTP verification");
                
            // Bank details for refund
            if (itr2Data.CalculateRefundOrDemand() > 0)
            {
                if (string.IsNullOrWhiteSpace(itr2Data.BankAccountNumber))
                    result.Errors.Add("Bank account number is mandatory for refund");
                    
                if (string.IsNullOrWhiteSpace(itr2Data.BankIFSCCode))
                    result.Errors.Add("Bank IFSC code is mandatory for refund");
            }
            
            result.IsValid = result.Errors.Count == 0;
            return result;
        }
    }

    /// <summary>
    /// Validation result for ITR form data
    /// </summary>
    public class ITRFormValidationResult
    {
        public bool IsValid { get; set; } = true;
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
    }

    /// <summary>
    /// Result of ITR form generation process
    /// </summary>
    public class ITRFormGenerationResult
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public ITRType GeneratedFormType { get; set; }
        public DateTime GeneratedAt { get; set; }
        
        // ITR Selection Details
        public ITRSelectionResult? ITRSelection { get; set; }
        
        // Generated Form Data
        public ITR1Data? ITR1Data { get; set; }
        public ITR2Data? ITR2Data { get; set; }
        
        // XML Export Results
        public string? XMLContent { get; set; }
        public string? XMLFileName { get; set; }
        
        // Validation Results
        public List<string> ValidationWarnings { get; set; } = new();
        public List<string> ValidationErrors { get; set; } = new();
        
        // Summary Information
        public decimal TotalIncome => GeneratedFormType == ITRType.ITR1_Sahaj 
            ? ITR1Data?.CalculateTotalIncome() ?? 0
            : ITR2Data?.CalculateTotalIncome() ?? 0;
            
        public decimal TaxLiability => GeneratedFormType == ITRType.ITR1_Sahaj 
            ? ITR1Data?.CalculateTaxLiability() ?? 0
            : ITR2Data?.CalculateTaxLiability() ?? 0;
            
        public decimal RefundOrDemand => GeneratedFormType == ITRType.ITR1_Sahaj 
            ? ITR1Data?.CalculateRefundOrDemand() ?? 0
            : ITR2Data?.CalculateRefundOrDemand() ?? 0;
        
        public bool IsRefund => RefundOrDemand > 0;
        public bool IsDemand => RefundOrDemand < 0;
        public decimal RefundAmount => Math.Max(0, RefundOrDemand);
        public decimal DemandAmount => Math.Max(0, -RefundOrDemand);
        
        // File export helpers
        public bool HasXMLContent => !string.IsNullOrWhiteSpace(XMLContent);
        public string DisplayFileName => XMLFileName ?? $"ITR_{GeneratedFormType}_{DateTime.Now:yyyyMMdd}.xml";
    }

    /// <summary>
    /// Additional taxpayer information not available in Form16
    /// </summary>
    public class AdditionalTaxpayerInfo
    {
        // Personal Details
        public DateTime DateOfBirth { get; set; }
        public TaxpayerCategory TaxpayerCategory { get; set; } = TaxpayerCategory.Individual;
        public ResidencyStatus ResidencyStatus { get; set; } = ResidencyStatus.Resident;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Pincode { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty;
        public string AadhaarNumber { get; set; } = string.Empty;
        
        // Bank Details
        public string BankAccountNumber { get; set; } = string.Empty;
        public string BankIFSCode { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        
        // Employer Details
        public string EmployerAddress { get; set; } = string.Empty;
        
        // House Property Details
        public decimal HousePropertyAnnualValue { get; set; }
        public decimal PropertyTaxPaid { get; set; }
        public decimal HomeLoanInterest { get; set; }
        public string PropertyAddress { get; set; } = string.Empty;
        public List<HousePropertyDetails>? HouseProperties { get; set; }
        
        // Capital Gains
        public List<CapitalGainDetails>? CapitalGains { get; set; }
        
        // Foreign Income/Assets
        public bool HasForeignIncome { get; set; }
        public decimal ForeignIncome { get; set; }
        public bool HasForeignAssets { get; set; }
        public List<ForeignAssetDetails>? ForeignAssets { get; set; }
        
        // Other Circumstances
        public bool IsDirectorOfCompany { get; set; }
        public bool HasUnlistedShares { get; set; }
        public bool HasLossesFromPreviousYear { get; set; }
        
        // Additional Tax Payments
        public decimal AdvanceTaxPaid { get; set; }
        public decimal SelfAssessmentTaxPaid { get; set; }
    }
}
