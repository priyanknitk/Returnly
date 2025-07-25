using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Returnly.Models;

namespace Returnly.Services
{
    /// <summary>
    /// Service for exporting ITR forms to XML format for government e-filing
    /// </summary>
    public class ITRXMLExportService
    {
        /// <summary>
        /// Exports ITR form data to XML format compatible with Income Tax Department
        /// </summary>
        /// <param name="itr1Data">ITR-1 form data</param>
        /// <param name="outputPath">Path where XML file will be saved</param>
        /// <returns>Export result with file path and status</returns>
        public async Task<XMLExportResult> ExportITR1ToXMLAsync(ITR1Data itr1Data, string outputPath)
        {
            ArgumentNullException.ThrowIfNull(itr1Data);

            var result = new XMLExportResult();
            
            try
            {
                // Validate data before export
                if (!itr1Data.ValidateData())
                {
                    result.IsSuccess = false;
                    result.ErrorMessage = "Validation failed: " + string.Join(", ", itr1Data.ValidationErrors);
                    return result;
                }

                var xml = await Task.Run(() => GenerateITR1XML(itr1Data));
                
                string xmlContent = xml.ToString(SaveOptions.DisableFormatting);
                // Save to file
                await File.WriteAllTextAsync(outputPath, xmlContent, Encoding.UTF8);

                result.IsSuccess = true;
                result.FilePath = outputPath;
                result.FormType = "ITR-1";
                result.XMLContent = xmlContent;
                result.ExportedAt = DateTime.Now;
                result.FileSize = new FileInfo(outputPath).Length;
                
                return result;
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = $"Error exporting ITR-1 to XML: {ex.Message}";
                return result;
            }
        }

        /// <summary>
        /// Exports ITR-2 form data to XML format
        /// </summary>
        /// <param name="itr2Data">ITR-2 form data</param>
        /// <param name="outputPath">Path where XML file will be saved</param>
        /// <returns>Export result with file path and status</returns>
        public async Task<XMLExportResult> ExportITR2ToXMLAsync(ITR2Data itr2Data, string outputPath)
        {
            if (itr2Data == null)
                throw new ArgumentNullException(nameof(itr2Data));

            var result = new XMLExportResult();
            
            try
            {
                // Validate data before export
                if (!itr2Data.ValidateData())
                {
                    result.IsSuccess = false;
                    result.ErrorMessage = "Validation failed: " + string.Join(", ", itr2Data.ValidationErrors);
                    return result;
                }

                var xml = await Task.Run(() => GenerateITR2XML(itr2Data));
                
                string xmlContent = xml.ToString(SaveOptions.DisableFormatting);

                // Save to file
                await File.WriteAllTextAsync(outputPath, xmlContent, Encoding.UTF8);

                result.IsSuccess = true;
                result.FilePath = outputPath;
                result.FormType = "ITR-2";
                result.XMLContent = xmlContent;
                result.ExportedAt = DateTime.Now;
                result.FileSize = new FileInfo(outputPath).Length;
                
                return result;
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = $"Error exporting ITR-2 to XML: {ex.Message}";
                return result;
            }
        }

        /// <summary>
        /// Generates ITR-1 XML structure following Income Tax Department schema
        /// </summary>
        private XDocument GenerateITR1XML(ITR1Data data)
        {
            var xml = new XDocument(
                new XDeclaration("1.0", "UTF-8", "yes"),
                new XElement("ITR1",
                    new XAttribute("schemaVersion", "1.0"),
                    new XAttribute("formName", "ITR-1"),
                    new XAttribute("assessmentYear", data.AssessmentYear),
                    
                    // Personal Information
                    CreatePersonalInfoElement(data),
                    
                    // Income Details
                    CreateITR1IncomeElement(data),
                    
                    // Deductions
                    CreateITR1DeductionsElement(data),
                    
                    // Tax Computation
                    CreateITR1TaxComputationElement(data),
                    
                    // Tax Payments
                    CreateITR1TaxPaymentsElement(data),
                    
                    // Bank Details
                    CreateBankDetailsElement(data)
                )
            );
            
            return xml;
        }

        /// <summary>
        /// Generates ITR-2 XML structure following Income Tax Department schema
        /// </summary>
        private XDocument GenerateITR2XML(ITR2Data data)
        {
            var xml = new XDocument(
                new XDeclaration("1.0", "UTF-8", "yes"),
                new XElement("ITR2",
                    new XAttribute("schemaVersion", "1.0"),
                    new XAttribute("formName", "ITR-2"),
                    new XAttribute("assessmentYear", data.AssessmentYear),
                    
                    // Personal Information
                    CreatePersonalInfoElement(data),
                    
                    // Income Details
                    CreateITR2IncomeElement(data),
                    
                    // Deductions
                    CreateITR2DeductionsElement(data),
                    
                    // Tax Computation
                    CreateITR2TaxComputationElement(data),
                    
                    // Tax Payments
                    CreateITR2TaxPaymentsElement(data),
                    
                    // Bank Details
                    CreateBankDetailsElement(data),
                    
                    // Foreign Assets (if applicable)
                    CreateForeignAssetsElement(data)
                )
            );
            
            return xml;
        }

        // Helper methods for creating XML elements

        private XElement CreatePersonalInfoElement(BaseITRData data)
        {
            return new XElement("PersonalInfo",
                new XElement("Name", data.Name),
                new XElement("PAN", data.PAN),
                new XElement("DateOfBirth", data.DateOfBirth.ToString("dd/MM/yyyy")),
                new XElement("ResidencyStatus", data.ResidencyStatus.ToString()),
                new XElement("Address",
                    new XElement("AddressLine", data.Address),
                    new XElement("City", data.City),
                    new XElement("State", data.State),
                    new XElement("Pincode", data.Pincode)
                ),
                new XElement("ContactInfo",
                    new XElement("Email", data.EmailAddress),
                    new XElement("Mobile", data.MobileNumber),
                    new XElement("Aadhaar", data.AadhaarNumber)
                )
            );
        }

        private XElement CreateITR1IncomeElement(ITR1Data data)
        {
            return new XElement("IncomeDetails",
                new XElement("SalaryIncome",
                    new XElement("GrossSalary", data.TotalSalaryIncome),
                    new XElement("EmployerDetails",
                        new XElement("Name", data.EmployerName),
                        new XElement("TAN", data.EmployerTAN),
                        new XElement("Address", data.EmployerAddress)
                    )
                ),
                new XElement("HousePropertyIncome",
                    new XElement("AnnualValue", data.AnnualValue),
                    new XElement("PropertyTax", data.PropertyTax),
                    new XElement("StandardDeduction", data.StandardDeduction30Percent),
                    new XElement("InterestOnLoan", data.InterestOnHomeLoan),
                    new XElement("NetIncome", data.TotalHousePropertyIncome)
                ),
                new XElement("OtherSources",
                    new XElement("InterestFromSavings", data.InterestFromSavingsAccount),
                    new XElement("InterestFromDeposits", data.InterestFromDeposits),
                    new XElement("DividendIncome", data.DividendIncome),
                    new XElement("OtherIncome", data.OtherIncome),
                    new XElement("TotalOtherSources", data.TotalOtherIncome)
                ),
                new XElement("TotalIncome", data.CalculateTotalIncome())
            );
        }

        private XElement CreateITR1DeductionsElement(ITR1Data data)
        {
            return new XElement("Deductions",
                new XElement("StandardDeduction", data.StandardDeduction),
                new XElement("ProfessionalTax", data.ProfessionalTax),
                new XElement("EntertainmentAllowance", data.EntertainmentAllowance),
                new XElement("TotalDeductions", data.TotalDeductions)
            );
        }

        private XElement CreateITR1TaxComputationElement(ITR1Data data)
        {
            var totalIncome = data.CalculateTotalIncome();
            var taxableIncome = Math.Max(0, totalIncome - data.TotalDeductions);
            var taxLiability = data.CalculateTaxLiability();
            
            return new XElement("TaxComputation",
                new XElement("TotalIncome", totalIncome),
                new XElement("TotalDeductions", data.TotalDeductions),
                new XElement("TaxableIncome", taxableIncome),
                new XElement("TaxLiability", taxLiability),
                new XElement("HealthEducationCess", taxLiability * 0.04m / 1.04m), // Reverse calculate cess
                new XElement("TotalTaxLiability", taxLiability)
            );
        }

        private XElement CreateITR1TaxPaymentsElement(ITR1Data data)
        {
            return new XElement("TaxPayments",
                new XElement("TDS",
                    new XElement("TotalTDS", data.TaxDeductedAtSource),
                    new XElement("QuarterlyBreakdown",
                        new XElement("Q1", data.Q1TDS),
                        new XElement("Q2", data.Q2TDS),
                        new XElement("Q3", data.Q3TDS),
                        new XElement("Q4", data.Q4TDS)
                    )
                ),
                new XElement("AdvanceTax", data.AdvanceTax),
                new XElement("SelfAssessmentTax", data.SelfAssessmentTax),
                new XElement("TotalTaxPaid", data.TotalTaxPaid),
                new XElement("RefundOrDemand",
                    new XElement("Amount", Math.Abs(data.CalculateRefundOrDemand())),
                    new XElement("Type", data.CalculateRefundOrDemand() >= 0 ? "Refund" : "Demand")
                )
            );
        }

        private XElement CreateITR2IncomeElement(ITR2Data data)
        {
            return new XElement("IncomeDetails",
                new XElement("SalaryIncome",
                    new XElement("TotalSalary", data.TotalSalaryIncome),
                    new XElement("Employers",
                        data.SalaryDetails.Select(s => new XElement("Employer",
                            new XElement("Name", s.EmployerName),
                            new XElement("TAN", s.EmployerTAN),
                            new XElement("Salary", s.GrossSalary),
                            new XElement("TDS", s.TaxDeducted)
                        ))
                    )
                ),
                new XElement("HousePropertyIncome",
                    new XElement("TotalIncome", data.TotalHousePropertyIncome),
                    new XElement("Properties",
                        data.HouseProperties.Select(p => new XElement("Property",
                            new XElement("Address", p.PropertyAddress),
                            new XElement("AnnualValue", p.AnnualValue),
                            new XElement("PropertyTax", p.PropertyTax),
                            new XElement("InterestOnLoan", p.InterestOnLoan),
                            new XElement("NetIncome", p.NetIncome)
                        ))
                    )
                ),
                new XElement("CapitalGains",
                    new XElement("TotalGains", data.TotalCapitalGains),
                    new XElement("Assets",
                        data.CapitalGains.Select(c => new XElement("Asset",
                            new XElement("Type", c.AssetType),
                            new XElement("SaleDate", c.DateOfSale.ToString("dd/MM/yyyy")),
                            new XElement("PurchaseDate", c.DateOfPurchase.ToString("dd/MM/yyyy")),
                            new XElement("SalePrice", c.SalePrice),
                            new XElement("CostOfAcquisition", c.CostOfAcquisition),
                            new XElement("NetGain", c.NetGain),
                            new XElement("IsLongTerm", c.IsLongTerm)
                        ))
                    )
                ),
                new XElement("OtherSources",
                    new XElement("InterestIncome", data.InterestIncome),
                    new XElement("DividendIncome", data.DividendIncome),
                    new XElement("OtherIncome", data.OtherSourcesIncome),
                    new XElement("ForeignIncome", data.ForeignIncome),
                    new XElement("TotalOtherSources", data.TotalOtherIncome)
                ),
                new XElement("TotalIncome", data.CalculateTotalIncome())
            );
        }

        private XElement CreateITR2DeductionsElement(ITR2Data data)
        {
            return new XElement("Deductions",
                new XElement("StandardDeduction", data.StandardDeduction),
                new XElement("ProfessionalTax", data.ProfessionalTax),
                new XElement("TotalDeductions", data.StandardDeduction + data.ProfessionalTax)
            );
        }

        private XElement CreateITR2TaxComputationElement(ITR2Data data)
        {
            var totalIncome = data.CalculateTotalIncome();
            var totalDeductions = data.StandardDeduction + data.ProfessionalTax;
            var taxableIncome = Math.Max(0, totalIncome - totalDeductions);
            var taxLiability = data.CalculateTaxLiability();
            
            return new XElement("TaxComputation",
                new XElement("TotalIncome", totalIncome),
                new XElement("TotalDeductions", totalDeductions),
                new XElement("TaxableIncome", taxableIncome),
                new XElement("TaxLiability", taxLiability),
                new XElement("HealthEducationCess", taxLiability * 0.04m / 1.04m),
                new XElement("TotalTaxLiability", taxLiability)
            );
        }

        private XElement CreateITR2TaxPaymentsElement(ITR2Data data)
        {
            return new XElement("TaxPayments",
                new XElement("TDS",
                    new XElement("TotalTDS", data.TaxDeductedAtSource),
                    new XElement("TDSDetails",
                        data.TDSDetails.Select(t => new XElement("TDSEntry",
                            new XElement("DeductorName", t.DeductorName),
                            new XElement("DeductorTAN", t.DeductorTAN),
                            new XElement("Amount", t.TaxDeducted),
                            new XElement("CertificateNumber", t.CertificateNumber),
                            new XElement("DateOfDeduction", t.DateOfDeduction.ToString("dd/MM/yyyy"))
                        ))
                    )
                ),
                new XElement("AdvanceTax", data.AdvanceTax),
                new XElement("SelfAssessmentTax", data.SelfAssessmentTax),
                new XElement("TCS", data.TaxDeductedCollectedAtSource),
                new XElement("TotalTaxPaid", data.TotalTaxPaid),
                new XElement("RefundOrDemand",
                    new XElement("Amount", Math.Abs(data.CalculateRefundOrDemand())),
                    new XElement("Type", data.CalculateRefundOrDemand() >= 0 ? "Refund" : "Demand")
                )
            );
        }

        private XElement CreateBankDetailsElement(BaseITRData data)
        {
            return new XElement("BankDetails",
                new XElement("AccountNumber", data.BankAccountNumber),
                new XElement("IFSCCode", data.BankIFSCCode),
                new XElement("BankName", data.BankName)
            );
        }

        private XElement CreateForeignAssetsElement(ITR2Data data)
        {
            if (!data.HasForeignAssets || data.ForeignAssets.Count == 0)
            {
                return new XElement("ForeignAssets",
                    new XElement("HasForeignAssets", "false")
                );
            }

            return new XElement("ForeignAssets",
                new XElement("HasForeignAssets", "true"),
                new XElement("Assets",
                    data.ForeignAssets.Select(a => new XElement("Asset",
                        new XElement("Type", a.AssetType),
                        new XElement("Country", a.Country),
                        new XElement("Value", a.Value),
                        new XElement("Currency", a.Currency)
                    ))
                )
            );
        }
    }

    /// <summary>
    /// Result of XML export operation
    /// </summary>
    public class XMLExportResult
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string FormType { get; set; } = string.Empty;
        public DateTime ExportedAt { get; set; }
        public long FileSize { get; set; }
        public string XMLContent { get; set; } = string.Empty;

        // Additional metadata
        public string FileName => Path.GetFileName(FilePath);
        public string FileSizeFormatted => FormatFileSize(FileSize);
        
        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}
