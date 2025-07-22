using System;

namespace Returnly.Models
{
    public class TaxRefundCalculation
    {
        public decimal TotalTaxLiability { get; set; }
        public decimal TDSDeducted { get; set; }
        public decimal RefundAmount { get; set; }
        public decimal AdditionalTaxDue { get; set; }
        public bool IsRefundDue { get; set; }
    }
}
