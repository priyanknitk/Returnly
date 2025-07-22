using System;

namespace Returnly.Models
{
    public class TaxSlabCalculation
    {
        public string SlabDescription { get; set; } = string.Empty;
        public decimal MinIncome { get; set; }
        public decimal? MaxIncome { get; set; }
        public decimal IncomeInSlab { get; set; }
        public decimal TaxRate { get; set; }
        public decimal TaxAmount { get; set; }
    }
}
