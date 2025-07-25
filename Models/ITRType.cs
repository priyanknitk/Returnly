using System;

namespace Returnly.Models
{
    /// <summary>
    /// Enum representing different ITR form types
    /// </summary>
    public enum ITRType
    {
        /// <summary>
        /// ITR-1 (Sahaj) - For individuals having income up to Rs 50 lakh 
        /// and having income from salary, one house property, other sources (interest etc)
        /// </summary>
        ITR1_Sahaj = 1,

        /// <summary>
        /// ITR-2 - For individuals and HUFs not having income from profits and gains of business or profession
        /// Includes capital gains, income from multiple house properties, foreign assets/income
        /// </summary>
        ITR2 = 2,

        /// <summary>
        /// Not eligible for ITR-1 or ITR-2, requires ITR-3 or higher
        /// </summary>
        NotSupported = 99
    }

    /// <summary>
    /// Reasons why a particular ITR form is selected or rejected
    /// </summary>
    public enum ITRSelectionReason
    {
        // ITR-1 Eligibility
        EligibleForITR1_BasicSalaryIncome,
        EligibleForITR1_WithinIncomeLimit,
        EligibleForITR1_SimpleIncomeStructure,

        // ITR-1 Rejections
        RejectedITR1_IncomeAbove50Lakh,
        RejectedITR1_HasCapitalGains,
        RejectedITR1_HasBusinessIncome,
        RejectedITR1_HasMultipleHouseProperties,
        RejectedITR1_HasForeignIncome,
        RejectedITR1_DirectorOfCompany,
        RejectedITR1_HasUnlistedShares,
        RejectedITR1_NotIndividualOrHUF,

        // ITR-2 Eligibility
        EligibleForITR2_IndividualOrHUF,
        EligibleForITR2_NoBusinessIncome,
        EligibleForITR2_HasCapitalGains,
        EligibleForITR2_HasMultipleIncomeources,

        // ITR-2 Rejections
        RejectedITR2_HasBusinessIncome,
        RejectedITR2_NotIndividualOrHUF,

        // General
        RequiresHigherITR,
        NotSupported
    }

    /// <summary>
    /// Taxpayer category for ITR selection
    /// </summary>
    public enum TaxpayerCategory
    {
        Individual,
        HUF, // Hindu Undivided Family
        Company,
        Partnership,
        LLP, // Limited Liability Partnership
        AOP, // Association of Persons
        BOI  // Body of Individuals
    }

    /// <summary>
    /// Residency status for tax purposes
    /// </summary>
    public enum ResidencyStatus
    {
        Resident,
        NonResident,
        ResidentNotOrdinaryResident
    }
}
