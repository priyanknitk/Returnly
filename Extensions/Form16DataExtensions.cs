using Returnly.Models;
using Returnly.Services;

namespace Returnly.Extensions
{
    /// <summary>
    /// Extension methods to convert Form16Data to ITR selection criteria
    /// </summary>
    public static class Form16DataExtensions
    {
        /// <summary>
        /// Convert Form16Data to ITRSelectionCriteria for ITR type determination
        /// </summary>
        public static ITRSelectionCriteria ToITRSelectionCriteria(this Form16Data form16Data, AdditionalTaxpayerInfo? additionalInfo = null)
        {
            ArgumentNullException.ThrowIfNull(form16Data);

            // Calculate gross salary properly
            decimal grossSalary = 0;
            if (form16Data.Form16B != null)
            {
                // Use the calculated GrossSalary from Form16B (which sums SalarySection17 + Perquisites + ProfitsInLieu)
                grossSalary = form16Data.Form16B.GrossSalary;
            }
            else
            {
                // Fallback to legacy field
                grossSalary = form16Data.GrossSalary;
            }

            return new ITRSelectionCriteria
            {
                // Basic information
                TaxpayerCategory = additionalInfo?.TaxpayerCategory ?? TaxpayerCategory.Individual, // Form16 is only for individuals
                ResidencyStatus = additionalInfo?.ResidencyStatus ?? ResidencyStatus.Resident, // Assume resident unless specified
                Age = additionalInfo == null || additionalInfo.DateOfBirth == DateTime.MinValue ? 0 : DateTime.Now.Year - additionalInfo.DateOfBirth.Year,

                // Income from Form16
                SalaryIncome = grossSalary,
                InterestIncome = form16Data.Form16B?.TotalInterestIncome ?? 0,
                DividendIncome = form16Data.Form16B?.TotalDividendIncome ?? 0,

                // These are typically not in Form16, default to 0
                CapitalGains = 0,
                BusinessIncome = 0,
                HousePropertyIncome = 0,
                OtherIncome = 0,

                // Special circumstances - defaulted for Form16 users
                IsDirectorOfCompany = false, // Can be made configurable
                HasUnlistedShares = false,
                HasForeignAssets = false,
                HasForeignIncome = false,
                HasMultipleHouseProperties = false,
                HasLossesFromPreviousYear = false
            };
        }

        /// <summary>
        /// Convert TaxCalculationResult to ITRSelectionCriteria
        /// </summary>
        public static ITRSelectionCriteria ToITRSelectionCriteria(this TaxCalculationResult taxResult, Form16Data form16Data)
        {
            if (taxResult == null)
                throw new ArgumentNullException(nameof(taxResult));
            
            if (form16Data == null)
                throw new ArgumentNullException(nameof(form16Data));

            var criteria = form16Data.ToITRSelectionCriteria();
            
            // Use the calculated gross salary properly
            decimal grossSalary = 0;
            if (form16Data.Form16B != null)
            {
                grossSalary = form16Data.Form16B.GrossSalary;
            }
            else
            {
                grossSalary = form16Data.GrossSalary;
            }

            criteria.SalaryIncome = grossSalary;
            criteria.InterestIncome = form16Data.Form16B?.TotalInterestIncome ?? 0;
            criteria.DividendIncome = form16Data.Form16B?.TotalDividendIncome ?? 0;

            return criteria;
        }
    }
}
