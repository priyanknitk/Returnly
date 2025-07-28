namespace ReturnlyWebApi.Models;

public class Form16Data
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
    public Form16AData Form16A { get; set; } = new();
    public Form16BData Form16B { get; set; } = new();
    public AnnexureData Annexure { get; set; } = new();
    
    // Business income fields (for manual tax data input)
    public decimal IntradayTradingIncome { get; set; } = 0;
    public decimal TradingBusinessExpenses { get; set; } = 0;
    public decimal OtherBusinessIncome { get; set; } = 0;
    public decimal BusinessExpenses { get; set; } = 0;
    
    // Helper property to check if there's any business income
    public bool HasBusinessIncome => IntradayTradingIncome > 0 || OtherBusinessIncome > 0;
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
    // Salary Information
    public decimal SalarySection17 { get; set; }
    public decimal Perquisites { get; set; }
    public decimal ProfitsInLieu { get; set; }
    public decimal GrossSalary => SalarySection17 + Perquisites + ProfitsInLieu;
    
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
    public decimal TotalInterestIncome => InterestOnSavings + InterestOnFixedDeposits + InterestOnBonds + OtherInterestIncome;
    
    // Dividend Income
    public decimal DividendIncomeAI { get; set; }
    public decimal DividendIncomeAII { get; set; }
    public decimal OtherDividendIncome { get; set; }
    public decimal TotalDividendIncome => DividendIncomeAI + DividendIncomeAII + OtherDividendIncome;
    
    // New Tax Regime Deductions
    public decimal StandardDeduction { get; set; } = 75000;
    public decimal ProfessionalTax { get; set; }
    public decimal TaxableIncome { get; set; }
}

public class AnnexureData
{
    // Quarterly TDS Breakdown
    public decimal Q1TDS { get; set; }
    public decimal Q2TDS { get; set; }
    public decimal Q3TDS { get; set; }
    public decimal Q4TDS { get; set; }
    public decimal TotalTDS => Q1TDS + Q2TDS + Q3TDS + Q4TDS;
}
