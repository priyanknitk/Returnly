// filepath: c:\Users\primanda\Returnly\Services\Form16ProcessingService.cs
using System.IO;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace Returnly.Services
{
    public class Form16ProcessingService
    {
        public async Task<Form16Data> ProcessForm16Async(string filePath, string? password = null)
        {
            return await Task.Run(() => ProcessForm16(filePath, password));
        }

        public async Task<bool> IsPasswordProtectedAsync(string filePath)
        {
            return await Task.Run(() => IsPasswordProtected(filePath));
        }

        public async Task<bool> ValidatePasswordAsync(string filePath, string password)
        {
            return await Task.Run(() => ValidatePassword(filePath, password));
        }

        private bool IsPasswordProtected(string filePath)
        {
            try
            {
                using var document = PdfDocument.Open(filePath);
                return false;
            }
            catch (Exception ex) when (ex.Message.Contains("password") || 
                                      ex.Message.Contains("encrypted") ||
                                      ex.Message.Contains("security"))
            {
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool ValidatePassword(string filePath, string password)
        {
            try
            {
                using var document = PdfDocument.Open(filePath, new ParsingOptions { Password = password });
                return true;
            }
            catch (Exception ex) when (ex.Message.Contains("password") || 
                                      ex.Message.Contains("encrypted") ||
                                      ex.Message.Contains("security"))
            {
                return false;
            }
            catch
            {
                return false;
            }
        }

        private Form16Data ProcessForm16(string filePath, string? password = null)
        {
            try
            {
                return ExtractStructuredDataFromPdf(filePath, password);
            }
            catch (Exception ex) when (ex.Message.Contains("password") || 
                                      ex.Message.Contains("encrypted") ||
                                      ex.Message.Contains("security"))
            {
                throw new UnauthorizedAccessException("Invalid password for PDF file.");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to process Form 16: {ex.Message}", ex);
            }
        }

        private Form16Data ExtractStructuredDataFromPdf(string filePath, string? password = null)
        {
            PdfDocument document;
            
            if (!string.IsNullOrEmpty(password))
            {
                document = PdfDocument.Open(filePath, new ParsingOptions { Password = password });
            }
            else
            {
                document = PdfDocument.Open(filePath);
            }

            using (document)
            {
                var form16Data = new Form16Data();
                var documentStructure = AnalyzeDocumentStructure(document);
                
                // Extract data using multiple strategies
                form16Data.Form16A = ExtractForm16AWithPositionalAnalysis(documentStructure);
                form16Data.Form16B = ExtractForm16BWithSemanticAnalysis(documentStructure);
                form16Data.Annexure = ExtractAnnexureWithTableDetection(documentStructure);

                // Populate backward compatibility fields
                PopulateBackwardCompatibilityFields(form16Data);

                return form16Data;
            }
        }

        private DocumentStructure AnalyzeDocumentStructure(PdfDocument document)
        {
            var structure = new DocumentStructure();
            
            foreach (var page in document.GetPages())
            {
                var pageStructure = new PageStructure
                {
                    PageNumber = page.Number,
                    Words = page.GetWords().ToList(),
                    Lines = ExtractLines(page),
                    Tables = DetectTables(page),
                    Headers = DetectHeaders(page)
                };
                
                structure.Pages.Add(pageStructure);
            }
            
            return structure;
        }

        private Form16AData ExtractForm16AWithPositionalAnalysis(DocumentStructure structure)
        {
            var form16A = new Form16AData();
            
            // Look for Form 16A indicators
            var form16APage = structure.Pages.FirstOrDefault(p => 
                p.Words.Any(w => w.Text.Contains("FORM", StringComparison.OrdinalIgnoreCase) && 
                               w.Text.Contains("16", StringComparison.OrdinalIgnoreCase))) ?? structure.Pages.First();

            // Use positional analysis to find data
            form16A.EmployeeName = ExtractFieldValueByPosition(form16APage, 
                new[] { "employee name", "name of employee", "employee" });
            
            form16A.PAN = ExtractPANByPattern(form16APage);
            form16A.EmployerName = ExtractFieldValueByPosition(form16APage, 
                new[] { "employer name", "name of employer", "company name" });
            
            form16A.TAN = ExtractTANByPattern(form16APage);
            form16A.AssessmentYear = ExtractYearByPattern(form16APage, "assessment");
            form16A.FinancialYear = ExtractYearByPattern(form16APage, "financial");
            
            form16A.TotalTaxDeducted = ExtractMonetaryValueByPosition(form16APage, 
                new[] { "total tax deducted", "tds", "tax deducted" });

            return form16A;
        }

        private Form16BData ExtractForm16BWithSemanticAnalysis(DocumentStructure structure)
        {
            var form16B = new Form16BData();
            
            // Find the salary breakdown section
            var salaryPage = structure.Pages.FirstOrDefault(p => 
                p.Words.Any(w => w.Text.Contains("salary", StringComparison.OrdinalIgnoreCase) ||
                               w.Text.Contains("income", StringComparison.OrdinalIgnoreCase))) ?? structure.Pages.First();

            // Extract salary components using semantic understanding
            var salaryTable = salaryPage.Tables.FirstOrDefault(t => 
                t.Headers.Any(h => h.Contains("salary", StringComparison.OrdinalIgnoreCase)));

            if (salaryTable != null)
            {
                form16B.BasicSalary = ExtractFromTable(salaryTable, "basic");
                form16B.HRA = ExtractFromTable(salaryTable, "hra", "house rent");
                form16B.SpecialAllowance = ExtractFromTable(salaryTable, "special", "allowance");
                form16B.OtherAllowances = ExtractFromTable(salaryTable, "other", "allowances");
                
                // Calculate SalarySection17 from breakdown
                form16B.SalarySection17 = form16B.BasicSalary + form16B.HRA + form16B.SpecialAllowance + form16B.OtherAllowances;
                
                // Extract perquisites and profits in lieu
                form16B.Perquisites = ExtractFromTable(salaryTable, "perquisites", "perks");
                form16B.ProfitsInLieu = ExtractFromTable(salaryTable, "profits", "lieu");
            }
            else
            {
                // Fallback to positional analysis
                form16B.BasicSalary = ExtractMonetaryValueByPosition(salaryPage, new[] { "basic salary", "basic pay" });
                form16B.HRA = ExtractMonetaryValueByPosition(salaryPage, new[] { "hra", "house rent allowance" });
                form16B.SpecialAllowance = ExtractMonetaryValueByPosition(salaryPage, new[] { "special allowance" });
                form16B.OtherAllowances = ExtractMonetaryValueByPosition(salaryPage, new[] { "other allowances" });
                
                // Calculate SalarySection17 from breakdown
                form16B.SalarySection17 = form16B.BasicSalary + form16B.HRA + form16B.SpecialAllowance + form16B.OtherAllowances;
                
                form16B.Perquisites = ExtractMonetaryValueByPosition(salaryPage, new[] { "perquisites", "perks" });
                form16B.ProfitsInLieu = ExtractMonetaryValueByPosition(salaryPage, new[] { "profits in lieu" });
            }

            // Extract deductions (New Tax Regime only)
            form16B.StandardDeduction = ExtractMonetaryValueByPosition(salaryPage, new[] { "standard deduction" });
            form16B.ProfessionalTax = ExtractMonetaryValueByPosition(salaryPage, new[] { "professional tax", "pt" });

            return form16B;
        }

        private AnnexureData ExtractAnnexureWithTableDetection(DocumentStructure structure)
        {
            var annexure = new AnnexureData();
            
            // Look for quarterly data in tables
            foreach (var page in structure.Pages)
            {
                foreach (var table in page.Tables)
                {
                    if (IsQuarterlyTable(table))
                    {
                        annexure.Q1TDS = ExtractQuarterlyValue(table, "q1", "quarter 1", "apr", "may", "jun");
                        annexure.Q2TDS = ExtractQuarterlyValue(table, "q2", "quarter 2", "jul", "aug", "sep");
                        annexure.Q3TDS = ExtractQuarterlyValue(table, "q3", "quarter 3", "oct", "nov", "dec");
                        annexure.Q4TDS = ExtractQuarterlyValue(table, "q4", "quarter 4", "jan", "feb", "mar");
                    }
                }
            }

            return annexure;
        }

        // Helper methods for advanced extraction
        private string ExtractFieldValueByPosition(PageStructure page, string[] fieldNames)
        {
            foreach (var fieldName in fieldNames)
            {
                var labelWord = page.Words.FirstOrDefault(w => 
                    w.Text.Contains(fieldName, StringComparison.OrdinalIgnoreCase));
                
                if (labelWord != null)
                {
                    // Look for the value to the right or below the label
                    var valueWord = page.Words
                        .Where(w => w.BoundingBox.Left > labelWord.BoundingBox.Left && 
                                   Math.Abs(w.BoundingBox.Bottom - labelWord.BoundingBox.Bottom) < 5)
                        .OrderBy(w => w.BoundingBox.Left)
                        .FirstOrDefault();
                    
                    if (valueWord != null && !string.IsNullOrWhiteSpace(valueWord.Text))
                    {
                        return CleanExtractedText(valueWord.Text);
                    }
                }
            }
            return "Not Found";
        }

        private string ExtractPANByPattern(PageStructure page)
        {
            var panPattern = @"[A-Z]{5}[0-9]{4}[A-Z]{1}";
            foreach (var word in page.Words)
            {
                var match = Regex.Match(word.Text, panPattern);
                if (match.Success)
                {
                    return match.Value;
                }
            }
            return "Not Found";
        }

        private string ExtractTANByPattern(PageStructure page)
        {
            var tanPattern = @"[A-Z]{4}[0-9]{5}[A-Z]{1}";
            foreach (var word in page.Words)
            {
                var match = Regex.Match(word.Text, tanPattern);
                if (match.Success)
                {
                    return match.Value;
                }
            }
            return "Not Found";
        }

        private decimal ExtractMonetaryValueByPosition(PageStructure page, string[] fieldNames)
        {
            foreach (var fieldName in fieldNames)
            {
                var labelWord = page.Words.FirstOrDefault(w => 
                    w.Text.Contains(fieldName, StringComparison.OrdinalIgnoreCase));
                
                if (labelWord != null)
                {
                    // Look for monetary values near the label
                    var nearbyWords = page.Words
                        .Where(w => Math.Abs(w.BoundingBox.Bottom - labelWord.BoundingBox.Bottom) < 10)
                        .ToList();
                    
                    foreach (var word in nearbyWords)
                    {
                        var cleanText = word.Text.Replace(",", "").Replace("₹", "").Replace("Rs", "").Trim();
                        if (decimal.TryParse(cleanText, out var value))
                        {
                            return value;
                        }
                    }
                }
            }
            return 0;
        }

        private List<LineStructure> ExtractLines(Page page)
        {
            var words = page.GetWords().ToList();
            var lines = new List<LineStructure>();
            
            // Group words by similar Y coordinates (lines)
            var wordGroups = words
                .GroupBy(w => Math.Round(w.BoundingBox.Bottom, 1))
                .OrderByDescending(g => g.Key);
            
            foreach (var group in wordGroups)
            {
                var line = new LineStructure
                {
                    Words = group.OrderBy(w => w.BoundingBox.Left).ToList(),
                    Text = string.Join(" ", group.OrderBy(w => w.BoundingBox.Left).Select(w => w.Text))
                };
                lines.Add(line);
            }
            
            return lines;
        }

        private List<TableStructure> DetectTables(Page page)
        {
            var tables = new List<TableStructure>();
            var words = page.GetWords().ToList();
            
            // Simple table detection based on alignment
            var alignedGroups = words
                .GroupBy(w => Math.Round(w.BoundingBox.Left, 1))
                .Where(g => g.Count() > 2)
                .ToList();
            
            if (alignedGroups.Count > 1)
            {
                var table = new TableStructure();
                foreach (var group in alignedGroups)
                {
                    var column = group.OrderByDescending(w => w.BoundingBox.Bottom).ToList();
                    table.Columns.Add(column);
                    
                    if (table.Headers.Count == 0)
                    {
                        table.Headers.Add(column.First().Text);
                    }
                }
                tables.Add(table);
            }
            
            return tables;
        }

        private List<string> DetectHeaders(Page page)
        {
            var headers = new List<string>();
            var words = page.GetWords().ToList();
            
            // Detect headers based on position and text content patterns
            // Since FontSize is not available in PdfPig Word class, we'll use alternative criteria
            
            foreach (var word in words)
            {
                // Headers are typically in the top portion of the page
                if (word.BoundingBox.Top > page.Height * 0.8) // Top 20% of page
                {
                    // Additional criteria for header detection
                    var text = word.Text?.Trim() ?? "";
                    
                    // Check for common header patterns
                    if (text.Length > 2 && 
                        (text.Contains("FORM", StringComparison.OrdinalIgnoreCase) ||
                         text.Contains("CERTIFICATE", StringComparison.OrdinalIgnoreCase) ||
                         text.Contains("INCOME", StringComparison.OrdinalIgnoreCase) ||
                         text.Contains("TAX", StringComparison.OrdinalIgnoreCase) ||
                         text.All(char.IsUpper) && text.Length > 4)) // All caps words longer than 4 chars
                    {
                        headers.Add(text);
                    }
                }
            }
            
            return headers;
        }

        private string CleanExtractedText(string text)
        {
            return text?.Trim().Replace("\n", " ").Replace("\r", "") ?? "";
        }

        private void PopulateBackwardCompatibilityFields(Form16Data form16Data)
        {
            form16Data.EmployeeName = form16Data.Form16A.EmployeeName;
            form16Data.PAN = form16Data.Form16A.PAN;
            form16Data.AssessmentYear = form16Data.Form16A.AssessmentYear;
            form16Data.EmployerName = form16Data.Form16A.EmployerName;
            form16Data.TAN = form16Data.Form16A.TAN;
            form16Data.FinancialYear = form16Data.Form16A.FinancialYear;
            form16Data.GrossSalary = form16Data.Form16B.GrossSalary;
            form16Data.TotalTaxDeducted = form16Data.Form16A.TotalTaxDeducted;
            form16Data.StandardDeduction = form16Data.Form16B.StandardDeduction;
            form16Data.ProfessionalTax = form16Data.Form16B.ProfessionalTax;
            
            // Note: HRAExemption removed for new tax regime
            // Set to 0 for backward compatibility
            form16Data.HRAExemption = 0;
        }

        // Additional helper methods for table processing
        private decimal ExtractFromTable(TableStructure table, params string[] searchTerms)
        {
            // Implementation for extracting values from detected tables
            return 0; // Placeholder
        }

        private bool IsQuarterlyTable(TableStructure table)
        {
            return table.Headers.Any(h => 
                h.Contains("quarter", StringComparison.OrdinalIgnoreCase) ||
                h.Contains("q1", StringComparison.OrdinalIgnoreCase) ||
                h.Contains("q2", StringComparison.OrdinalIgnoreCase));
        }

        private decimal ExtractQuarterlyValue(TableStructure table, params string[] searchTerms)
        {
            // Implementation for extracting quarterly values
            return 0; // Placeholder
        }

        private string ExtractYearByPattern(PageStructure page, string context)
        {
            var yearPattern = @"\d{4}-\d{2,4}";
            foreach (var word in page.Words)
            {
                var match = Regex.Match(word.Text, yearPattern);
                if (match.Success)
                {
                    return match.Value;
                }
            }
            return "Not Found";
        }
    }

    // Supporting data structures for document analysis
    public class DocumentStructure
    {
        public List<PageStructure> Pages { get; set; } = [];
    }

    public class PageStructure
    {
        public int PageNumber { get; set; }
        public List<Word> Words { get; set; } = [];
        public List<LineStructure> Lines { get; set; } = [];
        public List<TableStructure> Tables { get; set; } = [];
        public List<string> Headers { get; set; } = [];
    }

    public class LineStructure
    {
        public List<Word> Words { get; set; } = [];
        public string Text { get; set; } = "";
    }

    public class TableStructure
    {
        public List<string> Headers { get; set; } = [];
        public List<List<Word>> Columns { get; set; } = [];
    }

    // Existing data structures remain the same...
    public class Form16Data
    {
        // For backward compatibility
        public string EmployeeName { get; set; } = string.Empty;
        public string PAN { get; set; } = string.Empty;
        public string AssessmentYear { get; set; } = string.Empty;
        public string EmployerName { get; set; } = string.Empty;
        public string TAN { get; set; } = string.Empty;
        public string FinancialYear { get; set; } = string.Empty;
        public decimal GrossSalary { get; set; }
        public decimal TotalTaxDeducted { get; set; }
        public decimal StandardDeduction { get; set; }
        public decimal ProfessionalTax { get; set; }
        public decimal HRAExemption { get; set; }

        // New structured data
        public Form16AData Form16A { get; set; } = new();
        public Form16BData Form16B { get; set; } = new();
        public AnnexureData Annexure { get; set; } = new();

        public bool IsValid => !string.IsNullOrEmpty(EmployeeName) && !string.IsNullOrEmpty(PAN);
    }

    public class Form16AData
    {
        public string EmployeeName { get; set; } = string.Empty;
        public string PAN { get; set; } = string.Empty;
        public string AssessmentYear { get; set; } = string.Empty;
        public string FinancialYear { get; set; } = string.Empty;
        public string EmployerName { get; set; } = string.Empty;
        public string TAN { get; set; } = string.Empty;
        public string CertificateNumber { get; set; } = string.Empty;
        public decimal TotalTaxDeducted { get; set; }
    }

    public class Form16BData
    {
        // Form 16 Official Structure - Section 1: Gross Salary
        
        // 1(a) Salary as per section 17(1)
        public decimal SalarySection17 { get; set; }
        
        // 1(b) Perquisites under section 17(2)
        public decimal Perquisites { get; set; }
        
        // 1(c) Profits in lieu of salary under section 17(3)
        public decimal ProfitsInLieu { get; set; }
        
        // Breakdown of 1(a) - For detailed input
        public decimal BasicSalary { get; set; }
        public decimal HRA { get; set; }
        public decimal SpecialAllowance { get; set; }
        public decimal OtherAllowances { get; set; }
        
        // Legacy field - now calculated
        public decimal GrossSalary 
        { 
            get { return SalarySection17 + Perquisites + ProfitsInLieu; }
            set { /* For backward compatibility, but not used in calculations */ }
        }
        
        // New Tax Regime Deductions (Only applicable ones)
        public decimal StandardDeduction { get; set; } // ₹50,000 for FY 2023-24 onwards
        public decimal ProfessionalTax { get; set; }   // State-specific professional tax
        
        // Final Calculation
        public decimal TaxableIncome { get; set; }
    }

    public class AnnexureData
    {
        // Quarterly TDS Breakdown
        public decimal Q1TDS { get; set; }
        public decimal Q2TDS { get; set; }
        public decimal Q3TDS { get; set; }
        public decimal Q4TDS { get; set; }
        
        // No Section 80 deductions needed for new tax regime
    }
}