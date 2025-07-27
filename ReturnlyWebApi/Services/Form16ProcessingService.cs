using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using ReturnlyWebApi.Models;
using System.Text.RegularExpressions;

namespace ReturnlyWebApi.Services;

public interface IForm16ProcessingService
{
    Task<Form16Data> ProcessForm16PdfAsync(Stream pdfStream, string? password = null);
    Task<bool> ValidateForm16DataAsync(Form16Data form16Data);
}

public class Form16ProcessingService : IForm16ProcessingService
{
    private readonly ILogger<Form16ProcessingService> _logger;

    public Form16ProcessingService(ILogger<Form16ProcessingService> logger)
    {
        _logger = logger;
    }

    public async Task<Form16Data> ProcessForm16PdfAsync(Stream pdfStream, string? password = null)
    {
        try
        {
            using var document = PdfDocument.Open(pdfStream, new ParsingOptions { Password = password ?? string.Empty });
            var extractedText = ExtractTextFromPdf(document);
            
            return await Task.FromResult(ParseForm16Data(extractedText));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Form16 PDF");
            throw new InvalidOperationException("Unable to process Form16 PDF. Please ensure the file is not corrupted and password is correct.", ex);
        }
    }

    public async Task<bool> ValidateForm16DataAsync(Form16Data form16Data)
    {
        await Task.CompletedTask; // Placeholder for async validation if needed
        
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

        if (errors.Any())
        {
            throw new ValidationException($"Form16 validation failed: {string.Join("; ", errors)}");
        }

        return true;
    }

    private string ExtractTextFromPdf(PdfDocument document)
    {
        var text = string.Empty;
        
        foreach (var page in document.GetPages())
        {
            text += page.Text + "\n";
        }
        
        return text;
    }

    private Form16Data ParseForm16Data(string extractedText)
    {
        var form16Data = new Form16Data();

        try
        {
            // Extract basic information using regex patterns
            form16Data.EmployeeName = ExtractValue(extractedText, @"Name\s*:\s*([^\n\r]+)", "Employee Name");
            form16Data.PAN = ExtractValue(extractedText, @"PAN\s*:\s*([A-Z]{5}[0-9]{4}[A-Z]{1})", "PAN");
            form16Data.EmployerName = ExtractValue(extractedText, @"Employer\s*Name\s*:\s*([^\n\r]+)", "Employer Name");
            form16Data.TAN = ExtractValue(extractedText, @"TAN\s*:\s*([A-Z]{4}[0-9]{5}[A-Z]{1})", "TAN");
            
            // Extract financial year and assessment year
            form16Data.FinancialYear = ExtractValue(extractedText, @"Financial\s*Year\s*:\s*([0-9]{4}-[0-9]{2})", "2023-24");
            form16Data.AssessmentYear = ExtractValue(extractedText, @"Assessment\s*Year\s*:\s*([0-9]{4}-[0-9]{2})", "2024-25");

            // Extract salary information
            form16Data.Form16B.SalarySection17 = ExtractDecimalValue(extractedText, @"Salary.*Section.*17.*:\s*([0-9,\.]+)");
            form16Data.Form16B.Perquisites = ExtractDecimalValue(extractedText, @"Perquisites.*:\s*([0-9,\.]+)");
            form16Data.Form16B.ProfitsInLieu = ExtractDecimalValue(extractedText, @"Profits.*lieu.*:\s*([0-9,\.]+)");

            // Extract deductions
            form16Data.Form16B.StandardDeduction = ExtractDecimalValue(extractedText, @"Standard.*Deduction.*:\s*([0-9,\.]+)", 75000);
            form16Data.Form16B.ProfessionalTax = ExtractDecimalValue(extractedText, @"Professional.*Tax.*:\s*([0-9,\.]+)");

            // Extract TDS information
            form16Data.TotalTaxDeducted = ExtractDecimalValue(extractedText, @"Total.*Tax.*Deducted.*:\s*([0-9,\.]+)");
            
            // Extract quarterly TDS
            form16Data.Annexure.Q1TDS = ExtractDecimalValue(extractedText, @"Q1.*TDS.*:\s*([0-9,\.]+)");
            form16Data.Annexure.Q2TDS = ExtractDecimalValue(extractedText, @"Q2.*TDS.*:\s*([0-9,\.]+)");
            form16Data.Annexure.Q3TDS = ExtractDecimalValue(extractedText, @"Q3.*TDS.*:\s*([0-9,\.]+)");
            form16Data.Annexure.Q4TDS = ExtractDecimalValue(extractedText, @"Q4.*TDS.*:\s*([0-9,\.]+)");

            // Calculate derived values
            form16Data.GrossSalary = form16Data.Form16B.GrossSalary;
            form16Data.StandardDeduction = form16Data.Form16B.StandardDeduction;
            form16Data.ProfessionalTax = form16Data.Form16B.ProfessionalTax;

            // Copy data to Form16A
            form16Data.Form16A.EmployeeName = form16Data.EmployeeName;
            form16Data.Form16A.PAN = form16Data.PAN;
            form16Data.Form16A.FinancialYear = form16Data.FinancialYear;
            form16Data.Form16A.AssessmentYear = form16Data.AssessmentYear;
            form16Data.Form16A.EmployerName = form16Data.EmployerName;
            form16Data.Form16A.TAN = form16Data.TAN;
            form16Data.Form16A.TotalTaxDeducted = form16Data.TotalTaxDeducted;

            return form16Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing Form16 data from extracted text");
            throw new InvalidOperationException("Unable to parse Form16 data. The PDF format may not be supported.", ex);
        }
    }

    private string ExtractValue(string text, string pattern, string defaultValue = "")
    {
        var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value.Trim() : defaultValue;
    }

    private decimal ExtractDecimalValue(string text, string pattern, decimal defaultValue = 0)
    {
        var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
        if (match.Success)
        {
            var valueString = match.Groups[1].Value.Replace(",", "").Trim();
            if (decimal.TryParse(valueString, out decimal result))
            {
                return result;
            }
        }
        return defaultValue;
    }

    private bool IsValidPAN(string pan)
    {
        if (string.IsNullOrWhiteSpace(pan) || pan.Length != 10)
            return false;

        var panPattern = @"^[A-Z]{5}[0-9]{4}[A-Z]{1}$";
        return Regex.IsMatch(pan, panPattern);
    }
}

public class ValidationException : Exception
{
    public ValidationException(string message) : base(message) { }
    public ValidationException(string message, Exception innerException) : base(message, innerException) { }
}
