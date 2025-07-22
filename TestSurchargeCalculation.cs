using System;
using Returnly.Services;
using Returnly.Models;

namespace Returnly
{
    /// <summary>
    /// Simple test class to verify surcharge calculation logic
    /// </summary>
    public class TestSurchargeCalculation
    {
        public static void TestSurchargeRates()
        {
            Console.WriteLine("Testing Surcharge Calculation for New Tax Regime\n");
            Console.WriteLine("Expected rates for New Tax Regime (FY 2023-24 onwards):");
            Console.WriteLine("- Up to ₹50 Lakh: Nil");
            Console.WriteLine("- Above ₹50 Lakh to ₹1 Crore: 10%");
            Console.WriteLine("- Above ₹1 Crore to ₹2 Crore: 15%");
            Console.WriteLine("- Above ₹2 Crore: 25% (capped)\n");

            var taxService = new TaxCalculationService();

            // Test cases with different income levels
            var testCases = new[]
            {
                new { Income = 3000000m, Description = "₹30 Lakh", ExpectedSurcharge = 0m },
                new { Income = 4500000m, Description = "₹45 Lakh", ExpectedSurcharge = 0m },
                new { Income = 6000000m, Description = "₹60 Lakh", ExpectedSurcharge = 10m },
                new { Income = 8000000m, Description = "₹80 Lakh", ExpectedSurcharge = 10m },
                new { Income = 12000000m, Description = "₹1.2 Crore", ExpectedSurcharge = 15m },
                new { Income = 18000000m, Description = "₹1.8 Crore", ExpectedSurcharge = 15m },
                new { Income = 25000000m, Description = "₹2.5 Crore", ExpectedSurcharge = 25m },
                new { Income = 60000000m, Description = "₹6 Crore", ExpectedSurcharge = 25m } // Should be capped at 25%
            };

            foreach (var testCase in testCases)
            {
                try
                {
                    var result = taxService.CalculateTax(testCase.Income, "2024-25", TaxRegime.New, 30);
                    
                    Console.WriteLine($"Income: {testCase.Description}");
                    Console.WriteLine($"  Tax before surcharge: ₹{result.TotalTax:N2}");
                    Console.WriteLine($"  Surcharge rate: {result.SurchargeRate}% (Expected: {testCase.ExpectedSurcharge}%)");
                    Console.WriteLine($"  Surcharge amount: ₹{result.Surcharge:N2}");
                    Console.WriteLine($"  Total tax with cess: ₹{result.TotalTaxWithCess:N2}");
                    
                    var isCorrect = Math.Abs(result.SurchargeRate - testCase.ExpectedSurcharge) < 0.01m;
                    Console.WriteLine($"  Status: {(isCorrect ? "✅ CORRECT" : "❌ INCORRECT")}");
                    Console.WriteLine();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error testing {testCase.Description}: {ex.Message}");
                    Console.WriteLine();
                }
            }

            Console.WriteLine("\nTesting Old Tax Regime (should have higher surcharge rates):\n");
            
            // Test one high income case for old regime
            try
            {
                var oldRegimeResult = taxService.CalculateTax(60000000m, "2024-25", TaxRegime.Old, 30);
                Console.WriteLine($"Income: ₹6 Crore (Old Regime)");
                Console.WriteLine($"  Surcharge rate: {oldRegimeResult.SurchargeRate}% (Expected: 37%)");
                Console.WriteLine($"  Status: {(oldRegimeResult.SurchargeRate == 37 ? "✅ CORRECT" : "❌ INCORRECT")}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error testing old regime: {ex.Message}");
            }
        }
    }
}
