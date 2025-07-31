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
            // Extract basic information using regex patterns
            form16Data.EmployeeName = extractedText["Employee Details"][0];
            form16Data.PAN = extractedText["Employee PAN"][0];
            form16Data.EmployerName = extractedText["Employer Details"][0];
            form16Data.EmployerAddress = extractedText["Employer Details"].Length > 1 ? $"{extractedText["Employer Details"][1]},{extractedText["Employer Details"][2]}" : extractedText["Employer Details"][1];
            form16Data.TAN = extractedText["Dedector TAN"][0];

            //// Extract financial year and assessment year
            form16Data.AssessmentYear = extractedText["AssessmentYear"][0];
            // FY should be one year lesser than assessment year
            form16Data.FinancialYear = ConvertAssessmentYearToFinancialYear(form16Data.AssessmentYear);

            //// Extract salary information
            if (decimal.TryParse(extractedText["Salary17(1)"][0], out decimal salary17))
            {
                form16Data.Form16B.SalarySection17 = salary17;
            }
            else
            {
                throw new InvalidDataException("Salary details not found");
            }
            if (decimal.TryParse(extractedText["SalaryPerquisites"][0], out decimal salaryPerquisites))
            {
                form16Data.Form16B.Perquisites = salaryPerquisites;
            }
            else
            {
                throw new InvalidDataException("Perquisite details not found");
            }
            if (decimal.TryParse(extractedText["SalaryProfits"][0], out decimal salaryProfits))
            {
                form16Data.Form16B.ProfitsInLieu = salaryProfits;
            }
            else
            {
                throw new InvalidDataException("salaryProfits details not found");
            }

            //// Extract deductions
            if (decimal.TryParse(extractedText["StandardDeductions"][0], out decimal standardDeductions))
            {
                form16Data.Form16B.StandardDeduction = standardDeductions;
            }
            else
            {
                throw new InvalidDataException("StandardDeductions details not found");
            }

            //// Extract TDS information
            //form16Data.TotalTaxDeducted = ExtractDecimalValue(extractedText, @"Total.*Tax.*Deducted.*:\s*([0-9,\.]+)");

            //// Extract quarterly TDS
            //form16Data.Annexure.Q1TDS = ExtractDecimalValue(extractedText, @"Q1.*TDS.*:\s*([0-9,\.]+)");
            //form16Data.Annexure.Q2TDS = ExtractDecimalValue(extractedText, @"Q2.*TDS.*:\s*([0-9,\.]+)");
            //form16Data.Annexure.Q3TDS = ExtractDecimalValue(extractedText, @"Q3.*TDS.*:\s*([0-9,\.]+)");
            //form16Data.Annexure.Q4TDS = ExtractDecimalValue(extractedText, @"Q4.*TDS.*:\s*([0-9,\.]+)");

            // Calculate derived values
            if (decimal.TryParse(extractedText["GrossTotalIncome"][0], out decimal grossIncome))
            {
                form16Data.GrossSalary = grossIncome;
            }
            else
            {
                throw new InvalidDataException("GrossTotalIncome details not found");
            }
            form16Data.StandardDeduction = form16Data.Form16B.StandardDeduction;


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
