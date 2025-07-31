using ReturnlyWebApi.Models;
using System.Text.RegularExpressions;

namespace ReturnlyWebApi.Services;

public interface IForm16ProcessingService
{
    Task<Form16Data> ProcessForm16PdfAsync(Stream pdfStream, string? password = null);
    Task<Form16Data> ProcessForm16MultipleFilesAsync(Stream? form16AStream, Stream? form16BStream, string? form16APassword = null, string? form16BPassword = null);
    Task<Form16AData> ProcessForm16AAsync(Stream pdfStream, string? password = null);
    Task<Form16BData> ProcessForm16BAsync(Stream pdfStream, string? password = null);
}

public class Form16ProcessingService(ILogger<PdfProcessingService> logger) : PdfProcessingService(logger), IForm16ProcessingService
{
    // Form16B region boundaries (combined form or separate Form16B)
    private static readonly Dictionary<string, PdfRegion> form16BRegionBoundaries = new()
    {
        { "Employer Details", new PdfRegion("Employer Details", 1, 39, 205, 614.63, 664.88) },
        { "Employee Details", new PdfRegion("Employee Details", 1, 320, 550, 618, 647)},
        { "Employee PAN", new PdfRegion("Employee PAN", 1, 450, 505, 562, 575) },
        { "Dedector TAN", new PdfRegion("Dedector TAN", 1, 266, 317, 562, 575) },
        { "AssessmentYear", new PdfRegion("AssessmentYear", 1, 352, 385, 505, 517) },
        { "Salary17(1)", new PdfRegion("Salary17(1)", 1, 410, 465, 380, 395) },
        { "SalaryPerquisites", new PdfRegion("SalaryPerquisites", 1, 410, 465, 352, 365) },
        { "SalaryProfits", new PdfRegion("SalaryProfits", 1, 440, 465, 325, 340) },
        { "StandardDeductions", new PdfRegion("StandardDeductions", 2, 420, 465, 580, 595) },
        { "GrossTotalIncome", new PdfRegion("GrossTotalIncome", 2, 515, 575, 330, 340) },
    };

    // Form16A region boundaries (TDS certificate specific fields)
    private static readonly Dictionary<string, PdfRegion> form16ARegionBoundaries = new()
    {
        { "Total TDS", new PdfRegion("Total TDS", 1, 530, 580, 300, 320) },
    };

    private readonly ILogger<PdfProcessingService> _logger = logger;

    public async Task<Form16Data> ProcessForm16PdfAsync(Stream pdfStream, string? password = null)
    {
        try
        {
            var extractedText = ExtractTextFromPdf(pdfStream, password);

            return await Task.FromResult(ParseForm16Data(extractedText));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Form16 PDF");
            throw new InvalidOperationException("Unable to process Form16 PDF. Please ensure the file is not corrupted and password is correct.", ex);
        }
    }

    public async Task<Form16Data> ProcessForm16MultipleFilesAsync(Stream? form16AStream, Stream? form16BStream, string? form16APassword = null, string? form16BPassword = null)
    {
        try
        {
            var form16Data = new Form16Data();

            // Process Form16A if provided
            if (form16AStream != null)
            {
                _logger.LogInformation("Processing Form16A file");
                form16Data.Form16A = await ProcessForm16AAsync(form16AStream, form16APassword);
                
                // Copy basic information from Form16A
                form16Data.EmployeeName = form16Data.Form16A.EmployeeName;
                form16Data.PAN = form16Data.Form16A.PAN;
                form16Data.AssessmentYear = form16Data.Form16A.AssessmentYear;
                form16Data.FinancialYear = form16Data.Form16A.FinancialYear;
                form16Data.EmployerName = form16Data.Form16A.EmployerName;
                form16Data.TAN = form16Data.Form16A.TAN;
                form16Data.TotalTaxDeducted = form16Data.Form16A.TotalTaxDeducted;
            }

            // Process Form16B if provided
            if (form16BStream != null)
            {
                _logger.LogInformation("Processing Form16B file");
                form16Data.Form16B = await ProcessForm16BAsync(form16BStream, form16BPassword);
                
                // If Form16A wasn't provided, try to get basic info from Form16B
                if (form16AStream == null)
                {
                    // Extract basic info from Form16B using existing logic
                    var extractedText = ExtractTextFromPdf(form16BStream, form16BPassword);
                    PopulateBasicInfoFromForm16B(extractedText, form16Data);
                }

                // Calculate derived values
                form16Data.GrossSalary = form16Data.Form16B.GrossSalary;
                form16Data.StandardDeduction = form16Data.Form16B.StandardDeduction;
                form16Data.ProfessionalTax = form16Data.Form16B.ProfessionalTax;
            }

            // If neither file was provided, throw an error
            if (form16AStream == null && form16BStream == null)
            {
                throw new ArgumentException("At least one of Form16A or Form16B file must be provided");
            }

            _logger.LogInformation("Successfully processed Form16 files for employee {EmployeeName}", form16Data.EmployeeName);
            return form16Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Form16 multiple files");
            throw new InvalidOperationException("Unable to process Form16 files. Please ensure the files are not corrupted and passwords are correct.", ex);
        }
    }

    public async Task<Form16AData> ProcessForm16AAsync(Stream pdfStream, string? password = null)
    {
        try
        {
            var extractedText = ExtractTextFromPdf(pdfStream, password, form16ARegionBoundaries);
            return await Task.FromResult(ParseForm16AData(extractedText));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Form16A PDF");
            throw new InvalidOperationException("Unable to process Form16A PDF. Please ensure the file is not corrupted and password is correct.", ex);
        }
    }

    public async Task<Form16BData> ProcessForm16BAsync(Stream pdfStream, string? password = null)
    {
        try
        {
            var extractedText = ExtractTextFromPdf(pdfStream, password, form16BRegionBoundaries);
            return await Task.FromResult(ParseForm16BData(extractedText));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Form16B PDF");
            throw new InvalidOperationException("Unable to process Form16B PDF. Please ensure the file is not corrupted and password is correct.", ex);
        }
    }

    protected override Dictionary<string, PdfRegion> GetRegionBoundaries()
    {
        return form16BRegionBoundaries;
    }

    private Form16AData ParseForm16AData(Dictionary<string, string[]> extractedText)
    {
        var form16A = new Form16AData();

        try
        {
            // Extract basic information
            form16A.CertificateNumber = GetSafeTextValue(extractedText, "Certificate Number", 0);
            // Extract TDS information
            form16A.TotalTaxDeducted = GetSafeDecimalValue(extractedText, "Total TDS", 0, 0);

            _logger.LogInformation("Successfully parsed Form16A data for employee {EmployeeName}", form16A.EmployeeName);
            return form16A;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing Form16A data from extracted text");
            throw new InvalidOperationException("Unable to parse Form16A data. The PDF format may not be supported.", ex);
        }
    }

    private Form16BData ParseForm16BData(Dictionary<string, string[]> extractedText)
    {
        var form16B = new Form16BData();

        try
        {
            // Extract salary information
            form16B.SalarySection17 = GetSafeDecimalValue(extractedText, "Salary17(1)", 0, 0);
            form16B.Perquisites = GetSafeDecimalValue(extractedText, "SalaryPerquisites", 0, 0);
            form16B.ProfitsInLieu = GetSafeDecimalValue(extractedText, "SalaryProfits", 0, 0);

            // Extract salary breakdown
            form16B.BasicSalary = GetSafeDecimalValue(extractedText, "BasicSalary", 0, 0);
            form16B.HRA = GetSafeDecimalValue(extractedText, "HRA", 0, 0);
            form16B.SpecialAllowance = GetSafeDecimalValue(extractedText, "SpecialAllowance", 0, 0);

            // Extract interest income
            form16B.InterestOnSavings = GetSafeDecimalValue(extractedText, "InterestOnSavings", 0, 0);
            form16B.InterestOnFixedDeposits = GetSafeDecimalValue(extractedText, "InterestOnFD", 0, 0);

            // Extract dividend income
            form16B.DividendIncomeAI = GetSafeDecimalValue(extractedText, "DividendIncome", 0, 0);

            // Extract deductions
            form16B.StandardDeduction = GetSafeDecimalValue(extractedText, "StandardDeductions", 0, 75000);
            form16B.ProfessionalTax = GetSafeDecimalValue(extractedText, "ProfessionalTax", 0, 0);

            // Calculate taxable income
            var totalIncome = form16B.GrossSalary + form16B.TotalInterestIncome + form16B.TotalDividendIncome;
            form16B.TaxableIncome = Math.Max(0, totalIncome - form16B.StandardDeduction - form16B.ProfessionalTax);

            _logger.LogInformation("Successfully parsed Form16B data with gross salary {GrossSalary}", form16B.GrossSalary);
            return form16B;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing Form16B data from extracted text");
            throw new InvalidOperationException("Unable to parse Form16B data. The PDF format may not be supported.", ex);
        }
    }

    private void PopulateBasicInfoFromForm16B(Dictionary<string, string[]> extractedText, Form16Data form16Data)
    {
        // Extract basic information from Form16B when Form16A is not available
        form16Data.EmployeeName = GetSafeTextValue(extractedText, "Employee Details", 0);
        form16Data.PAN = GetSafeTextValue(extractedText, "Employee PAN", 0);
        form16Data.EmployerName = GetSafeTextValue(extractedText, "Employer Details", 0);
        form16Data.EmployerAddress = BuildEmployerAddress(extractedText);
        form16Data.TAN = GetSafeTextValue(extractedText, "Dedector TAN", 0);
        form16Data.AssessmentYear = GetSafeTextValue(extractedText, "AssessmentYear", 0);
        form16Data.FinancialYear = ConvertAssessmentYearToFinancialYear(form16Data.AssessmentYear);
    }

    private Form16Data ParseForm16Data(Dictionary<string, string[]> extractedText)
    {
        var form16Data = new Form16Data();

        try
        {
            // Extract basic information with safe accessors
            form16Data.EmployeeName = GetSafeTextValue(extractedText, "Employee Details", 0);
            form16Data.PAN = GetSafeTextValue(extractedText, "Employee PAN", 0);
            form16Data.EmployerName = GetSafeTextValue(extractedText, "Employer Details", 0);
            form16Data.EmployerAddress = BuildEmployerAddress(extractedText);
            form16Data.TAN = GetSafeTextValue(extractedText, "Dedector TAN", 0);

            // Extract financial year and assessment year
            form16Data.AssessmentYear = GetSafeTextValue(extractedText, "AssessmentYear", 0);
            form16Data.FinancialYear = ConvertAssessmentYearToFinancialYear(form16Data.AssessmentYear);

            // Extract salary information using field mappings
            ExtractSalaryInformation(extractedText, form16Data);

            // Extract deductions
            ExtractDeductions(extractedText, form16Data);

            // Calculate derived values
            form16Data.GrossSalary = GetSafeDecimalValue(extractedText, "GrossTotalIncome", 0, form16Data.Form16B.GrossSalary);
            form16Data.StandardDeduction = form16Data.Form16B.StandardDeduction;

            // Copy data to Form16A using a mapper method
            MapToForm16A(form16Data);

            return form16Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing Form16 data from extracted text");
            throw new InvalidOperationException("Unable to parse Form16 data. The PDF format may not be supported.", ex);
        }
    }

    private void ExtractSalaryInformation(Dictionary<string, string[]> extractedText, Form16Data form16Data)
    {
        var salaryMappings = new Dictionary<string, Action<decimal>>
        {
            { "Salary17(1)", value => form16Data.Form16B.SalarySection17 = value },
            { "SalaryPerquisites", value => form16Data.Form16B.Perquisites = value },
            { "SalaryProfits", value => form16Data.Form16B.ProfitsInLieu = value }
        };

        foreach (var mapping in salaryMappings)
        {
            var value = GetSafeDecimalValue(extractedText, mapping.Key, 0, 0);
            mapping.Value(value);
        }
    }

    private void ExtractDeductions(Dictionary<string, string[]> extractedText, Form16Data form16Data)
    {
        form16Data.Form16B.StandardDeduction = GetSafeDecimalValue(extractedText, "StandardDeductions", 0, 75000);
        
        // Add other deduction extractions here as needed
        // form16Data.Form16B.ProfessionalTax = GetSafeDecimalValue(extractedText, "ProfessionalTax", 0, 0);
    }

    private void MapToForm16A(Form16Data form16Data)
    {
        var form16A = form16Data.Form16A;
        
        form16A.EmployeeName = form16Data.EmployeeName;
        form16A.PAN = form16Data.PAN;
        form16A.FinancialYear = form16Data.FinancialYear;
        form16A.AssessmentYear = form16Data.AssessmentYear;
        form16A.EmployerName = form16Data.EmployerName;
        form16A.TAN = form16Data.TAN;
        form16A.TotalTaxDeducted = form16Data.TotalTaxDeducted;
    }
    private decimal GetSafeDecimalValue(Dictionary<string, string[]> extractedText, string key, int index, decimal defaultValue = 0)
    {
        var textValue = GetSafeTextValue(extractedText, key, index);
        
        if (string.IsNullOrEmpty(textValue))
        {
            _logger.LogWarning("Using default value {DefaultValue} for missing key: {Key}", defaultValue, key);
            return defaultValue;
        }

        // Clean the text value - remove commas, currency symbols, etc.
        var cleanedValue = CleanNumericText(textValue);
        
        if (decimal.TryParse(cleanedValue, out var result))
        {
            return result;
        }
        
        _logger.LogWarning("Failed to parse decimal value '{TextValue}' for key: {Key}, using default: {DefaultValue}", 
            textValue, key, defaultValue);
        return defaultValue;
    }

    private string CleanNumericText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;
        
        // Remove common formatting characters that might appear in PDF extracted numbers
        return text
            .Replace(",", "")           // Remove thousand separators
            .Replace("₹", "")           // Remove currency symbol
            .Replace("Rs.", "")         // Remove currency abbreviation
            .Replace("Rs", "")          // Remove currency abbreviation
            .Replace(" ", "")           // Remove spaces
            .Trim();
    }

    private string BuildEmployerAddress(Dictionary<string, string[]> extractedText)
    {
        if (!extractedText.TryGetValue("Employer Details", out var employerDetails) || 
            employerDetails == null || 
            employerDetails.Length <= 1)
        {
            return string.Empty;
        }
        
        // Join available address parts, skipping the first element (employer name)
        var addressParts = employerDetails
            .Skip(1)
            .Where(part => !string.IsNullOrWhiteSpace(part))
            .Select(part => part.Trim());
        
        return string.Join(", ", addressParts);
    }

    private bool IsValidPAN(string pan)
    {
        if (string.IsNullOrWhiteSpace(pan) || pan.Length != 10)
            return false;

        var panPattern = @"^[A-Z]{5}[0-9]{4}[A-Z]{1}$";
        return Regex.IsMatch(pan, panPattern);
    }

    private string ConvertAssessmentYearToFinancialYear(string assessmentYear)
    {
        if (string.IsNullOrWhiteSpace(assessmentYear))
            return string.Empty;

        // Assessment year format: "2024-25" -> Financial year: "2023-24"
        if (assessmentYear.Contains('-'))
        {
            var parts = assessmentYear.Split('-');
            if (parts.Length == 2 && int.TryParse(parts[0], out var startYear))
            {
                var previousYear = startYear - 1;
                var previousYearShort = (previousYear % 100).ToString("D2");
                var currentYearShort = parts[1];
                return $"{previousYear}-{currentYearShort}";
            }
        }

        return assessmentYear; // Return as-is if parsing fails
    }

    private string GetSafeTextValue(Dictionary<string, string[]> extractedText, string key, int index)
    {
        if (!extractedText.TryGetValue(key, out var values) || 
            values == null || 
            values.Length <= index || 
            string.IsNullOrWhiteSpace(values[index]))
        {
            _logger.LogWarning("Missing or empty text value for key: {Key}, index: {Index}", key, index);
            return string.Empty;
        }
        
        return values[index].Trim();
    }
}

public class ValidationException : Exception
{
    public ValidationException(string message) : base(message) { }
    public ValidationException(string message, Exception innerException) : base(message, innerException) { }
}
