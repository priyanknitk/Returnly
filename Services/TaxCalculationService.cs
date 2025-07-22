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
            var taxSlabs = _taxSlabService.GetTaxSlabs(financialYear, regime, age);
            
            if (taxSlabs == null || !taxSlabs.Any())
            {
                throw new InvalidOperationException($"No tax slabs found for financial year {financialYear} and regime {regime}");
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
            result.HealthAndEducationCess = totalTax * 0.04m; // 4% cess on total tax
            result.TotalTaxWithCess = result.TotalTax + result.HealthAndEducationCess;
            result.EffectiveTaxRate = taxableIncome > 0 ? (result.TotalTaxWithCess / taxableIncome) * 100 : 0;

            return result;
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
                    ((oldRegimeTax.TotalTaxWithCess - newRegimeTax.TotalTaxWithCess) / oldRegimeTax.TotalTaxWithCess) * 100 : 0
            };
        }
    }
}