using System;
using System.Collections.Generic;
using System.Linq;
using Returnly.Models;

namespace Returnly.Services
{
    /// <summary>
    /// Service to determine the appropriate ITR form based on taxpayer's income and circumstances
    /// </summary>
    public class ITRSelectionService
    {
        // ITR-1 income limit (₹50 lakhs)
        private const decimal ITR1_INCOME_LIMIT = 5000000;

        /// <summary>
        /// Determine the appropriate ITR form for the given criteria
        /// </summary>
        public ITRSelectionResult DetermineITRType(ITRSelectionCriteria criteria)
        {
            try
            {
                var result = new ITRSelectionResult
                {
                    TotalIncome = criteria.TotalIncome,
                    IncomeBreakdown = BuildIncomeBreakdown(criteria)
                };

                // Check ITR-1 eligibility first (most restrictive)
                var itr1Check = CheckITR1Eligibility(criteria);
                if (itr1Check.IsEligible)
                {
                    result.RecommendedITR = ITRType.ITR1_Sahaj;
                    result.PrimaryReason = itr1Check.PrimaryReason;
                    result.AllReasons = itr1Check.Reasons;
                    result.Explanation = BuildITR1Explanation(criteria);
                }
                else
                {
                    // Check ITR-2 eligibility
                    var itr2Check = CheckITR2Eligibility(criteria);
                    if (itr2Check.IsEligible)
                    {
                        result.RecommendedITR = ITRType.ITR2;
                        result.PrimaryReason = itr2Check.PrimaryReason;
                        result.AllReasons = itr2Check.Reasons;
                        result.Explanation = BuildITR2Explanation(criteria, itr1Check.Reasons);
                        
                        // Add ITR-1 rejection reasons to help user understand
                        result.AllReasons.AddRange(itr1Check.Reasons);
                    }
                    else
                    {
                        // Neither ITR-1 nor ITR-2 is suitable
                        result.RecommendedITR = ITRType.NotSupported;
                        result.PrimaryReason = ITRSelectionReason.RequiresHigherITR;
                        result.AllReasons.AddRange(itr1Check.Reasons);
                        result.AllReasons.AddRange(itr2Check.Reasons);
                        result.Explanation = BuildNotSupportedExplanation(criteria);
                        result.Warnings.Add("Your income profile requires ITR-3 or higher, which is not currently supported by this application.");
                    }
                }

                // Add any warnings
                AddWarnings(result, criteria);

                return result;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error determining ITR type: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Check eligibility for ITR-1 (Sahaj)
        /// </summary>
        private (bool IsEligible, ITRSelectionReason PrimaryReason, List<ITRSelectionReason> Reasons) CheckITR1Eligibility(ITRSelectionCriteria criteria)
        {
            var reasons = new List<ITRSelectionReason>();

            // Basic eligibility checks for ITR-1
            
            // 1. Must be Individual (not HUF, Company, etc.)
            if (criteria.TaxpayerCategory != TaxpayerCategory.Individual)
            {
                reasons.Add(ITRSelectionReason.RejectedITR2_NotIndividualOrHUF);
                return (false, ITRSelectionReason.RejectedITR2_NotIndividualOrHUF, reasons);
            }

            // 2. Total income must be ≤ ₹50 lakhs
            if (criteria.TotalIncome > ITR1_INCOME_LIMIT)
            {
                reasons.Add(ITRSelectionReason.RejectedITR1_IncomeAbove50Lakh);
                return (false, ITRSelectionReason.RejectedITR1_IncomeAbove50Lakh, reasons);
            }

            // 3. No business income allowed
            if (criteria.HasAnyBusinessIncome)
            {
                reasons.Add(ITRSelectionReason.RejectedITR1_HasBusinessIncome);
                return (false, ITRSelectionReason.RejectedITR1_HasBusinessIncome, reasons);
            }

            // 4. No capital gains allowed
            if (criteria.HasAnyCapitalGains)
            {
                reasons.Add(ITRSelectionReason.RejectedITR1_HasCapitalGains);
                return (false, ITRSelectionReason.RejectedITR1_HasCapitalGains, reasons);
            }

            // 5. Cannot be director of a company
            if (criteria.IsDirectorOfCompany)
            {
                reasons.Add(ITRSelectionReason.RejectedITR1_DirectorOfCompany);
                return (false, ITRSelectionReason.RejectedITR1_DirectorOfCompany, reasons);
            }

            // 6. Cannot have unlisted shares
            if (criteria.HasUnlistedShares)
            {
                reasons.Add(ITRSelectionReason.RejectedITR1_HasUnlistedShares);
                return (false, ITRSelectionReason.RejectedITR1_HasUnlistedShares, reasons);
            }

            // 7. Cannot have foreign income
            if (criteria.HasForeignIncome)
            {
                reasons.Add(ITRSelectionReason.RejectedITR1_HasForeignIncome);
                return (false, ITRSelectionReason.RejectedITR1_HasForeignIncome, reasons);
            }

            // 8. Cannot have more than one house property
            if (criteria.HasMultipleHouseProperties)
            {
                reasons.Add(ITRSelectionReason.RejectedITR1_HasMultipleHouseProperties);
                return (false, ITRSelectionReason.RejectedITR1_HasMultipleHouseProperties, reasons);
            }

            // If we reach here, ITR-1 is eligible
            reasons.Add(ITRSelectionReason.EligibleForITR1_WithinIncomeLimit);
            reasons.Add(ITRSelectionReason.EligibleForITR1_SimpleIncomeStructure);
            
            if (criteria.SalaryIncome > 0)
            {
                reasons.Add(ITRSelectionReason.EligibleForITR1_BasicSalaryIncome);
            }

            return (true, ITRSelectionReason.EligibleForITR1_WithinIncomeLimit, reasons);
        }

        /// <summary>
        /// Check eligibility for ITR-2
        /// </summary>
        private (bool IsEligible, ITRSelectionReason PrimaryReason, List<ITRSelectionReason> Reasons) CheckITR2Eligibility(ITRSelectionCriteria criteria)
        {
            var reasons = new List<ITRSelectionReason>();

            // Basic eligibility checks for ITR-2

            // 1. Must be Individual or HUF
            if (criteria.TaxpayerCategory != TaxpayerCategory.Individual && criteria.TaxpayerCategory != TaxpayerCategory.HUF)
            {
                reasons.Add(ITRSelectionReason.RejectedITR2_NotIndividualOrHUF);
                return (false, ITRSelectionReason.RejectedITR2_NotIndividualOrHUF, reasons);
            }

            // 2. Must not have business income
            if (criteria.HasAnyBusinessIncome)
            {
                reasons.Add(ITRSelectionReason.RejectedITR2_HasBusinessIncome);
                return (false, ITRSelectionReason.RejectedITR2_HasBusinessIncome, reasons);
            }

            // If we reach here, ITR-2 is eligible
            reasons.Add(ITRSelectionReason.EligibleForITR2_IndividualOrHUF);
            reasons.Add(ITRSelectionReason.EligibleForITR2_NoBusinessIncome);

            if (criteria.HasAnyCapitalGains)
            {
                reasons.Add(ITRSelectionReason.EligibleForITR2_HasCapitalGains);
            }

            if (criteria.InterestIncome > 0 || criteria.DividendIncome > 0 || criteria.HousePropertyIncome > 0)
            {
                reasons.Add(ITRSelectionReason.EligibleForITR2_HasMultipleIncomeources);
            }

            return (true, ITRSelectionReason.EligibleForITR2_IndividualOrHUF, reasons);
        }

        private Dictionary<string, decimal> BuildIncomeBreakdown(ITRSelectionCriteria criteria)
        {
            var breakdown = new Dictionary<string, decimal>();

            if (criteria.SalaryIncome > 0)
                breakdown["Salary Income"] = criteria.SalaryIncome;
            
            if (criteria.InterestIncome > 0)
                breakdown["Interest Income"] = criteria.InterestIncome;
            
            if (criteria.DividendIncome > 0)
                breakdown["Dividend Income"] = criteria.DividendIncome;
            
            if (criteria.CapitalGains > 0)
                breakdown["Capital Gains"] = criteria.CapitalGains;
            
            if (criteria.BusinessIncome > 0)
                breakdown["Business Income"] = criteria.BusinessIncome;
            
            if (criteria.HousePropertyIncome > 0)
                breakdown["House Property Income"] = criteria.HousePropertyIncome;
            
            if (criteria.OtherIncome > 0)
                breakdown["Other Income"] = criteria.OtherIncome;

            return breakdown;
        }

        private string BuildITR1Explanation(ITRSelectionCriteria criteria)
        {
            return $"ITR-1 (Sahaj) is recommended for your income profile. " +
                   $"Your total income of ₹{criteria.TotalIncome:N0} is within the ₹50 lakh limit, " +
                   $"and you have a simple income structure consisting primarily of salary and other sources " +
                   $"without any business income or capital gains.";
        }

        private string BuildITR2Explanation(ITRSelectionCriteria criteria, List<ITRSelectionReason> itr1Rejections)
        {
            var explanation = $"ITR-2 is recommended for your income profile. ";
            
            if (itr1Rejections.Contains(ITRSelectionReason.RejectedITR1_IncomeAbove50Lakh))
            {
                explanation += $"Your total income of ₹{criteria.TotalIncome:N0} exceeds the ITR-1 limit of ₹50 lakhs. ";
            }
            
            if (itr1Rejections.Contains(ITRSelectionReason.RejectedITR1_HasCapitalGains))
            {
                explanation += "You have capital gains income which requires ITR-2. ";
            }
            
            if (itr1Rejections.Contains(ITRSelectionReason.RejectedITR1_HasMultipleHouseProperties))
            {
                explanation += "You have multiple house properties which requires ITR-2. ";
            }

            explanation += "ITR-2 supports your income structure without business income.";
            
            return explanation;
        }

        private string BuildNotSupportedExplanation(ITRSelectionCriteria criteria)
        {
            var explanation = "Your income profile requires ITR-3 or higher forms. ";
            
            if (criteria.HasAnyBusinessIncome)
            {
                explanation += "Business income requires ITR-3. ";
            }
            
            if (criteria.TaxpayerCategory != TaxpayerCategory.Individual && criteria.TaxpayerCategory != TaxpayerCategory.HUF)
            {
                explanation += $"{criteria.TaxpayerCategory} category requires specialized ITR forms. ";
            }

            explanation += "These forms are not currently supported by this application.";
            
            return explanation;
        }

        private void AddWarnings(ITRSelectionResult result, ITRSelectionCriteria criteria)
        {
            // Add specific warnings based on circumstances
            
            if (criteria.HasForeignAssets && result.RecommendedITR == ITRType.ITR2)
            {
                result.Warnings.Add("You have foreign assets. Ensure you disclose them properly in Schedule FA of ITR-2.");
            }

            if (criteria.HasLossesFromPreviousYear)
            {
                result.Warnings.Add("You have losses from previous years. Ensure proper carry forward in your ITR.");
            }

            if (criteria.TotalIncome > 4500000) // Close to ITR-1 limit
            {
                result.Warnings.Add("Your income is close to the ITR-1 limit. Double-check all income sources for accuracy.");
            }
        }
    }
}
