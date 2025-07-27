using System.ComponentModel.DataAnnotations;

namespace ReturnlyWebApi.DTOs;

public class Form16UploadDto
{
    [Required]
    public IFormFile PdfFile { get; set; } = null!;
    
    public string? Password { get; set; }
}

public class Form16DataDto
{
    public string EmployeeName { get; set; } = string.Empty;
    public string PAN { get; set; } = string.Empty;
    public string AssessmentYear { get; set; } = string.Empty;
    public string FinancialYear { get; set; } = string.Empty;
    public string EmployerName { get; set; } = string.Empty;
    public string TAN { get; set; } = string.Empty;
    public decimal GrossSalary { get; set; }
    public decimal TotalTaxDeducted { get; set; }
    public decimal StandardDeduction { get; set; } = 75000;
    public decimal ProfessionalTax { get; set; }
    public Form16BDataDto Form16B { get; set; } = new();
    public AnnexureDataDto Annexure { get; set; } = new();
}

public class Form16BDataDto
{
    // Salary Information
    public decimal SalarySection17 { get; set; }
    public decimal Perquisites { get; set; }
    public decimal ProfitsInLieu { get; set; }
    
    // Salary Breakdown
    public decimal BasicSalary { get; set; }
    public decimal HRA { get; set; }
    public decimal SpecialAllowance { get; set; }
    public decimal OtherAllowances { get; set; }
    
    // Interest Income
    public decimal InterestOnSavings { get; set; }
    public decimal InterestOnFixedDeposits { get; set; }
    public decimal InterestOnBonds { get; set; }
    public decimal OtherInterestIncome { get; set; }
    
    // Dividend Income
    public decimal DividendIncomeAI { get; set; }
    public decimal DividendIncomeAII { get; set; }
    public decimal OtherDividendIncome { get; set; }
    
    // Deductions
    public decimal StandardDeduction { get; set; } = 75000;
    public decimal ProfessionalTax { get; set; }
    public decimal TaxableIncome { get; set; }
}

public class AnnexureDataDto
{
    public decimal Q1TDS { get; set; }
    public decimal Q2TDS { get; set; }
    public decimal Q3TDS { get; set; }
    public decimal Q4TDS { get; set; }
}

public class UpdateTaxDataRequestDto
{
    [Required]
    public Form16DataDto Form16Data { get; set; } = new();
    
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Taxable income must be a positive value")]
    public decimal TaxableIncome { get; set; }
}
