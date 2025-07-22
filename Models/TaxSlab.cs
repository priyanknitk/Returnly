using System;

namespace Returnly.Models
{
    public class TaxSlab
    {
        public decimal MinIncome { get; set; }
        public decimal? MaxIncome { get; set; }
        public decimal TaxRate { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
