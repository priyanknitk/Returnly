using System;
using System.Collections.Generic;

namespace Returnly.Models
{
    /// <summary>
    /// Result of ITR selection process with recommended ITR type and reasoning
    /// </summary>
    public class ITRSelectionResult
    {
        /// <summary>
        /// Recommended ITR form type
        /// </summary>
        public ITRType RecommendedITR { get; set; }

        /// <summary>
        /// Alternative ITR options if applicable
        /// </summary>
        public List<ITRType> AlternativeITRs { get; set; } = new();

        /// <summary>
        /// Primary reason for the selection
        /// </summary>
        public ITRSelectionReason PrimaryReason { get; set; }

        /// <summary>
        /// All reasons that influenced the selection
        /// </summary>
        public List<ITRSelectionReason> AllReasons { get; set; } = new();

        /// <summary>
        /// Human-readable explanation of the selection
        /// </summary>
        public string Explanation { get; set; } = string.Empty;

        /// <summary>
        /// Warnings or important notes about the selection
        /// </summary>
        public List<string> Warnings { get; set; } = new();

        /// <summary>
        /// Whether the selection is definitive or requires user confirmation
        /// </summary>
        public bool RequiresUserConfirmation { get; set; }

        /// <summary>
        /// Total income considered for ITR selection
        /// </summary>
        public decimal TotalIncome { get; set; }

        /// <summary>
        /// Breakdown of income sources considered
        /// </summary>
        public Dictionary<string, decimal> IncomeBreakdown { get; set; } = new();
    }

    /// <summary>
    /// Input parameters for ITR selection
    /// </summary>
    public class ITRSelectionCriteria
    {
        // Basic taxpayer information
        public TaxpayerCategory TaxpayerCategory { get; set; } = TaxpayerCategory.Individual;
        public ResidencyStatus ResidencyStatus { get; set; } = ResidencyStatus.Resident;
        public int Age { get; set; } = 30;

        // Income information
        public decimal SalaryIncome { get; set; }
        public decimal InterestIncome { get; set; }
        public decimal DividendIncome { get; set; }
        public decimal CapitalGains { get; set; }
        public decimal BusinessIncome { get; set; }
        public decimal HousePropertyIncome { get; set; }
        public decimal OtherIncome { get; set; }

        // Special circumstances
        public bool IsDirectorOfCompany { get; set; }
        public bool HasUnlistedShares { get; set; }
        public bool HasForeignAssets { get; set; }
        public bool HasForeignIncome { get; set; }
        public bool HasMultipleHouseProperties { get; set; }
        public bool HasLossesFromPreviousYear { get; set; }

        // Calculated fields
        public decimal TotalIncome => SalaryIncome + InterestIncome + DividendIncome + 
                                     CapitalGains + BusinessIncome + HousePropertyIncome + OtherIncome;

        public bool HasAnyCapitalGains => CapitalGains > 0;
        public bool HasAnyBusinessIncome => BusinessIncome > 0;
    }
}
