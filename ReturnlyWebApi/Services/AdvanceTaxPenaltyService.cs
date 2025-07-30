using ReturnlyWebApi.Models;

namespace ReturnlyWebApi.Services;

public interface IAdvanceTaxPenaltyService
{
    AdvanceTaxPenaltyCalculation CalculateAdvanceTaxPenalties(
        decimal totalTaxLiability, 
        decimal tdsDeducted, 
        AdvanceTaxInstallments advanceTaxPaid, 
        DateTime filingDate, 
        string financialYear);
}

public class AdvanceTaxPenaltyService : IAdvanceTaxPenaltyService
{
    private const decimal INTEREST_RATE_PER_MONTH = 1.0m; // 1% per month or part thereof
    
    public AdvanceTaxPenaltyCalculation CalculateAdvanceTaxPenalties(
        decimal totalTaxLiability, 
        decimal tdsDeducted, 
        AdvanceTaxInstallments advanceTaxPaid, 
        DateTime filingDate, 
        string financialYear)
    {
        var result = new AdvanceTaxPenaltyCalculation
        {
            TotalTaxLiability = totalTaxLiability,
            TDSDeducted = tdsDeducted,
            AdvanceTaxPaid = advanceTaxPaid,
            FilingDate = filingDate,
            FinancialYear = financialYear
        };

        // Calculate net advance tax liability (excluding TDS)
        var netAdvanceTaxLiability = totalTaxLiability - tdsDeducted;
        
        if (netAdvanceTaxLiability <= 10000)
        {
            // No advance tax liability if total tax less TDS is ≤ ₹10,000
            return result;
        }

        // Get financial year dates
        var fyDates = GetFinancialYearDates(financialYear);

        // Calculate Section 234B Interest (Failure to Pay Advance Tax)
        result.Section234BInterest = CalculateSection234BInterest(
            netAdvanceTaxLiability, 
            advanceTaxPaid.TotalAdvanceTaxPaid, 
            fyDates, 
            filingDate,
            result.PenaltyDetails);

        // Calculate Section 234C Interest (Deferment of Advance Tax)
        result.Section234CInterest = CalculateSection234CInterest(
            netAdvanceTaxLiability, 
            advanceTaxPaid, 
            fyDates,
            result.PenaltyDetails);

        return result;
    }

    private decimal CalculateSection234BInterest(
        decimal netAdvanceTaxLiability, 
        decimal totalAdvanceTaxPaid, 
        FinancialYearDates fyDates, 
        DateTime filingDate,
        List<AdvanceTaxPenaltyDetail> penaltyDetails)
    {
        // Section 234B: Interest on failure to pay advance tax
        // Applicable when advance tax paid is less than 90% of total liability
        var requiredAdvanceTax = netAdvanceTaxLiability * 0.90m; // 90% rule
        var shortfall = Math.Max(0, requiredAdvanceTax - totalAdvanceTaxPaid);
        
        if (shortfall <= 0)
        {
            return 0;
        }

        // Interest period: From 1st April of next financial year to filing date or due date
        var interestStartDate = fyDates.NextFYStart; // 1st April of assessment year
        var interestEndDate = filingDate;
        
        // For individuals, due date is 31st July of assessment year
        var dueDate = new DateTime(fyDates.NextFYStart.Year, 7, 31);
        if (filingDate > dueDate)
        {
            // If filed after due date, interest continues till filing date
            interestEndDate = filingDate;
        }
        else
        {
            // If filed before due date, interest only till due date
            interestEndDate = dueDate;
        }
        
        if (interestEndDate <= interestStartDate)
        {
            return 0;
        }

        var interestMonths = CalculateMonthsForInterest(interestStartDate, interestEndDate);
        var interestAmount = shortfall * (INTEREST_RATE_PER_MONTH / 100) * interestMonths;

        penaltyDetails.Add(new AdvanceTaxPenaltyDetail
        {
            InstallmentPeriod = "Annual Assessment",
            RequiredAmount = requiredAdvanceTax,
            ActualAmount = totalAdvanceTaxPaid,
            Shortfall = shortfall,
            InterestRate = INTEREST_RATE_PER_MONTH,
            InterestDays = (interestEndDate - interestStartDate).Days,
            InterestAmount = interestAmount,
            PenaltySection = "234B",
            Description = "Failure to Pay Advance Tax (90% rule violation)"
        });

        return Math.Round(interestAmount, 0);
    }

    private decimal CalculateSection234CInterest(
        decimal netAdvanceTaxLiability, 
        AdvanceTaxInstallments advanceTaxPaid, 
        FinancialYearDates fyDates,
        List<AdvanceTaxPenaltyDetail> penaltyDetails)
    {
        // Section 234C: Interest on deferment of advance tax installments
        // Interest = 1% per month (simple interest) on shortfall for fixed duration per installment
        
        decimal totalSection234CInterest = 0;
        
        // Define installment requirements (cumulative percentages)
        var installments = new[]
        {
            new { DueDate = fyDates.FirstInstallmentDue, RequiredPercent = 0.15m, InterestMonths = 3, Name = "1st Installment (15th June)" },
            new { DueDate = fyDates.SecondInstallmentDue, RequiredPercent = 0.45m, InterestMonths = 3, Name = "2nd Installment (15th September)" },
            new { DueDate = fyDates.ThirdInstallmentDue, RequiredPercent = 0.75m, InterestMonths = 3, Name = "3rd Installment (15th December)" },
            new { DueDate = fyDates.FourthInstallmentDue, RequiredPercent = 1.00m, InterestMonths = 1, Name = "4th Installment (15th March)" }
        };

        // Calculate shortfall and interest for each installment
        foreach (var installment in installments)
        {
            var requiredAmount = netAdvanceTaxLiability * installment.RequiredPercent;
            var actualPaidTillDate = GetAdvanceTaxPaidTillDate(advanceTaxPaid, installment.DueDate);
            var shortfall = Math.Max(0, requiredAmount - actualPaidTillDate);
            
            if (shortfall > 0)
            {
                // Calculate interest: 1% per month for fixed duration
                var interestAmount = shortfall * (INTEREST_RATE_PER_MONTH / 100) * installment.InterestMonths;
                totalSection234CInterest += interestAmount;

                penaltyDetails.Add(new AdvanceTaxPenaltyDetail
                {
                    InstallmentPeriod = installment.Name,
                    RequiredAmount = requiredAmount,
                    ActualAmount = actualPaidTillDate,
                    Shortfall = shortfall,
                    InterestRate = INTEREST_RATE_PER_MONTH,
                    InterestDays = installment.InterestMonths * 30, // Approximate days for display
                    InterestAmount = interestAmount,
                    PenaltySection = "234C",
                    Description = $"Deferment of {installment.Name} - {installment.RequiredPercent:P0} of total tax due"
                });
            }
        }

        return Math.Round(totalSection234CInterest, 0);
    }

    private decimal GetAdvanceTaxPaidTillDate(AdvanceTaxInstallments advanceTaxPaid, DateTime tillDate)
    {
        // Calculate total advance tax paid till a specific date
        decimal totalPaid = 0;
        
        // This is a simplified version - in reality, you'd need actual payment dates
        // For now, we'll assume all advance tax was paid evenly or use the total
        // In a real implementation, you'd track payment dates for each installment
        
        return advanceTaxPaid.TotalAdvanceTaxPaid;
    }

    private int CalculateMonthsForInterest(DateTime startDate, DateTime endDate)
    {
        // Calculate number of months for interest calculation
        // Any part of a month is counted as a full month
        
        var months = 0;
        var currentDate = startDate;
        
        while (currentDate < endDate)
        {
            months++;
            currentDate = currentDate.AddMonths(1);
            
            // If adding a month goes beyond the end date, 
            // but we haven't reached the end date yet, count it
            if (currentDate > endDate && currentDate.AddMonths(-1) < endDate)
            {
                break;
            }
        }
        
        return Math.Max(months, 1); // Minimum 1 month
    }

    private FinancialYearDates GetFinancialYearDates(string financialYear)
    {
        // Parse financial year (e.g., "2023-24" or "2024-25")
        var years = financialYear.Split('-');
        if (years.Length != 2)
        {
            throw new ArgumentException($"Invalid financial year format: {financialYear}");
        }

        int startYear;
        
        // Handle both 2-digit and 4-digit year formats
        if (years[0].Length == 2)
        {
            // 2-digit format like "23-24"
            if (!int.TryParse("20" + years[0], out startYear))
            {
                throw new ArgumentException($"Invalid financial year format: {financialYear}");
            }
        }
        else if (years[0].Length == 4)
        {
            // 4-digit format like "2023-24"
            if (!int.TryParse(years[0], out startYear))
            {
                throw new ArgumentException($"Invalid financial year format: {financialYear}");
            }
        }
        else
        {
            throw new ArgumentException($"Invalid financial year format: {financialYear}");
        }

        return new FinancialYearDates
        {
            FYStart = new DateTime(startYear, 4, 1),
            FirstInstallmentDue = new DateTime(startYear, 6, 15),
            SecondInstallmentDue = new DateTime(startYear, 9, 15),
            ThirdInstallmentDue = new DateTime(startYear, 12, 15),
            FourthInstallmentDue = new DateTime(startYear + 1, 3, 15),
            FYEnd = new DateTime(startYear + 1, 3, 31),
            NextFYStart = new DateTime(startYear + 1, 4, 1)
        };
    }

    private class FinancialYearDates
    {
        public DateTime FYStart { get; set; }
        public DateTime FirstInstallmentDue { get; set; }
        public DateTime SecondInstallmentDue { get; set; }
        public DateTime ThirdInstallmentDue { get; set; }
        public DateTime FourthInstallmentDue { get; set; }
        public DateTime FYEnd { get; set; }
        public DateTime NextFYStart { get; set; }
    }
}
