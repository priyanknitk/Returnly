using System;
using System.Collections.Generic;
using System.Linq;
using Returnly.Models;

namespace Returnly.Services
{
    public class TaxCalculationService
    {
        private readonly TaxSlabConfigurationService _taxSlabService;

        public TaxCalculationService()
        {
            _taxSlabService = new TaxSlabConfigurationService();
        }

        /// <summary>
        /// Calculate income tax based on taxable income and financial year
        /// </summary>
        /// <param name="taxableIncome">The taxable income amount</param>
        /// <param name="financialYear">Financial year in format "2023-24"</param>
        /// <param name="regime">Tax regime - Old or New</param>
        /// <param name="age">Age of taxpayer for senior citizen benefits</param>
        /// <returns>Tax calculation result</returns>
        public TaxCalculationResult CalculateTax(decimal taxableIncome, string financialYear, TaxRegime regime = TaxRegime.New, int age = 30)
        {
            try
            {
                var taxSlabs = _taxSlabService.GetTaxSlabs(financialYear, regime, age);
                
                if (taxSlabs == null || !taxSlabs.Any())
                {
                    throw new InvalidOperationException($"No tax slabs found for financial year {financialYear}, regime {regime}, age {age}");
                }

                var result = new TaxCalculationResult
                {
                    TaxableIncome = taxableIncome,
                    FinancialYear = financialYear,
                    TaxRegime = regime,
                    Age = age,
                    TaxBreakdown = []
                };

                decimal remainingIncome = taxableIncome;
                decimal totalTax = 0;

            foreach (var slab in taxSlabs.OrderBy(s => s.MinIncome))
            {
                if (remainingIncome <= 0) break;

                decimal slabUpperLimit = slab.MaxIncome ?? decimal.MaxValue;
                decimal incomeInThisSlab = Math.Min(remainingIncome, slabUpperLimit - slab.MinIncome);
                
                if (incomeInThisSlab > 0)
                {
                    decimal taxInThisSlab = incomeInThisSlab * (slab.TaxRate / 100);
                    totalTax += taxInThisSlab;

                    result.TaxBreakdown.Add(new TaxSlabCalculation
                    {
                        SlabDescription = slab.Description,
                        IncomeInSlab = incomeInThisSlab,
                        TaxRate = slab.TaxRate,
                        TaxAmount = taxInThisSlab,
                        MinIncome = slab.MinIncome,
                        MaxIncome = slab.MaxIncome
                    });

                    remainingIncome -= incomeInThisSlab;
                }
            }

            result.TotalTax = totalTax;
            
            // Calculate surcharge based on total income
            var surchargeInfo = CalculateSurcharge(taxableIncome, totalTax);
            result.Surcharge = surchargeInfo.Amount;
            result.SurchargeRate = surchargeInfo.Rate;
            result.TotalTaxWithSurcharge = result.TotalTax + result.Surcharge;
            
            // Calculate cess on tax + surcharge
            result.HealthAndEducationCess = result.TotalTaxWithSurcharge * 0.04m; // 4% cess on tax + surcharge
            result.TotalTaxWithCess = result.TotalTaxWithSurcharge + result.HealthAndEducationCess;
            result.EffectiveTaxRate = taxableIncome > 0 ? (result.TotalTaxWithCess / taxableIncome) * 100 : 0;

            return result;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error calculating tax for income ₹{taxableIncome:N0}, FY {financialYear}, {regime} regime: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Calculate tax refund or additional tax liability
        /// </summary>
        public TaxRefundCalculation CalculateRefund(TaxCalculationResult taxCalculation, decimal tdsDeducted)
        {
            return new TaxRefundCalculation
            {
                TotalTaxLiability = taxCalculation.TotalTaxWithCess,
                TDSDeducted = tdsDeducted,
                RefundAmount = Math.Max(0, tdsDeducted - taxCalculation.TotalTaxWithCess),
                AdditionalTaxDue = Math.Max(0, taxCalculation.TotalTaxWithCess - tdsDeducted),
                IsRefundDue = tdsDeducted > taxCalculation.TotalTaxWithCess
            };
        }

        /// <summary>
        /// Compare tax liability between old and new regime
        /// </summary>
        public RegimeComparisonResult CompareTaxRegimes(decimal taxableIncomeOld, decimal taxableIncomeNew, 
            string financialYear, int age = 30, decimal oldRegimeDeductions = 0)
        {
            var oldRegimeTax = CalculateTax(taxableIncomeOld, financialYear, TaxRegime.Old, age);
            var newRegimeTax = CalculateTax(taxableIncomeNew, financialYear, TaxRegime.New, age);

            return new RegimeComparisonResult
            {
                OldRegimeCalculation = oldRegimeTax,
                NewRegimeCalculation = newRegimeTax,
                OldRegimeDeductions = oldRegimeDeductions,
                TaxSavings = oldRegimeTax.TotalTaxWithCess - newRegimeTax.TotalTaxWithCess,
                RecommendedRegime = newRegimeTax.TotalTaxWithCess <= oldRegimeTax.TotalTaxWithCess ? TaxRegime.New : TaxRegime.Old,
                SavingsPercentage = oldRegimeTax.TotalTaxWithCess > 0 ? 
                    (oldRegimeTax.TotalTaxWithCess - newRegimeTax.TotalTaxWithCess) / oldRegimeTax.TotalTaxWithCess * 100 : 0
            };
        }

        /// <summary>
        /// Calculate surcharge based on total income
        /// </summary>
        /// <param name="totalIncome">Total income for surcharge calculation</param>
        /// <param name="incomeTax">Income tax amount on which surcharge is calculated</param>
        /// <returns>Surcharge information with amount and rate</returns>
        private (decimal Amount, decimal Rate) CalculateSurcharge(decimal totalIncome, decimal incomeTax)
        {
            decimal surchargeRate = 0;

            // Surcharge rates for individuals as per current tax laws
            if (totalIncome > 50000000) // Above ₹5 crores
            {
                surchargeRate = 37;
            }
            else if (totalIncome > 20000000) // Above ₹2 crores
            {
                surchargeRate = 25;
            }
            else if (totalIncome > 10000000) // Above ₹1 crore
            {
                surchargeRate = 15;
            }
            else if (totalIncome > 5000000) // Above ₹50 lakhs
            {
                surchargeRate = 10;
            }

            decimal surchargeAmount = incomeTax * (surchargeRate / 100);
            
            return (surchargeAmount, surchargeRate);
        }
    }
}