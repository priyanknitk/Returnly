using ReturnlyWebApi.Models;
using System.Text.RegularExpressions;

namespace ReturnlyWebApi.Services;

public interface IForm16ProcessingService
{
    Task<Form16Data> ProcessForm16PdfAsync(Stream pdfStream, string? password = null);
    bool ValidateForm16Data(Form16Data form16Data);
}

public class Form16ProcessingService(ILogger<PdfProcessingService> logger) : PdfProcessingService(logger), IForm16ProcessingService
{
    private static readonly Dictionary<string, PdfRegion> regionBoundaries = new()
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
        { "GrossTotalIncome", new PdfRegion("GrossTotalIncome", 2, 515, 575, 330, 340) }
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

    public bool ValidateForm16Data(Form16Data form16Data)
    {
        var errors = new List<string>();

        // Basic validation
        if (string.IsNullOrWhiteSpace(form16Data.EmployeeName))
            errors.Add("Employee Name is required");

        if (string.IsNullOrWhiteSpace(form16Data.PAN) || !IsValidPAN(form16Data.PAN))
            errors.Add("Valid PAN is required");

        if (string.IsNullOrWhiteSpace(form16Data.FinancialYear))
            errors.Add("Financial Year is required");

        if (form16Data.GrossSalary <= 0)
            errors.Add("Gross Salary must be greater than zero");

        // TDS validation
        var quarterlyTotal = form16Data.Annexure.Q1TDS + form16Data.Annexure.Q2TDS +
                           form16Data.Annexure.Q3TDS + form16Data.Annexure.Q4TDS;

        if (Math.Abs(form16Data.TotalTaxDeducted - quarterlyTotal) > 1)
            errors.Add("Total TDS doesn't match quarterly breakdown");

        if (errors.Count != 0)
        {
            throw new ValidationException($"Form16 validation failed: {string.Join("; ", errors)}");
        }

        return true;
    }

    protected override Dictionary<string, PdfRegion> GetRegionBoundaries()
    {
        return regionBoundaries;
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
        try
        {
            // Parse assessment year (e.g., "2024-25" or "24-25")
            var years = assessmentYear.Split('-');
            if (years.Length != 2)
            {
                throw new ArgumentException($"Invalid assessment year format: {assessmentYear}");
            }

            int startYear;

            // Handle both 2-digit and 4-digit year formats
            if (years[0].Length == 2)
            {
                // 2-digit format like "24-25"
                if (!int.TryParse("20" + years[0], out startYear))
                {
                    throw new ArgumentException($"Invalid assessment year format: {assessmentYear}");
                }
            }
            else if (years[0].Length == 4)
            {
                // 4-digit format like "2024-25"
                if (!int.TryParse(years[0], out startYear))
                {
                    throw new ArgumentException($"Invalid assessment year format: {assessmentYear}");
                }
            }
            else
            {
                throw new ArgumentException($"Invalid assessment year format: {assessmentYear}");
            }

            // Financial year is one year before assessment year
            var fyStartYear = startYear - 1;
            return $"{fyStartYear}-{(fyStartYear + 1).ToString().Substring(2)}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting assessment year {AssessmentYear} to financial year", assessmentYear);
            return string.Empty; // Return empty string on error
        }
    }
}

public class ValidationException : Exception
{
    public ValidationException(string message) : base(message) { }
    public ValidationException(string message, Exception innerException) : base(message, innerException) { }
}
