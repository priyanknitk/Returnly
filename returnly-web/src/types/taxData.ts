export interface TaxData {
  assessmentYear: string;
  financialYear: string;
  employerName: string;
  tan: string;
  // Additional employer details
  employerCategory?: string;
  employerPinCode?: string;
  employerAddress?: string;
  employerCountry?: string;
  employerState?: string;
  employerCity?: string;
  salarySection17: number;
  perquisites: number;
  profitsInLieu: number;
  // Salary Breakdown fields
  basicPay?: number;
  ltaAllowance?: number;
  houseRentAllowance?: number;
  specialAllowance?: number;
  performanceBonus?: number;
  bonus?: number;
  otherAllowances?: number;
  interestOnSavings: number;
  interestOnFixedDeposits: number;
  dividendIncome: number;
  standardDeduction: number;
  professionalTax: number;
  totalTaxDeducted: number;
  // Capital Gains fields
  stocksSTCG: number;
  stocksLTCG: number;
  mutualFundsSTCG: number;
  mutualFundsLTCG: number;
  fnoGains: number;
  realEstateSTCG: number;
  realEstateLTCG: number;
  bondsSTCG: number;
  bondsLTCG: number;
  goldSTCG: number;
  goldLTCG: number;
  cryptoGains: number;
  // Foreign Assets - US Stocks
  usStocksSTCG: number;
  usStocksLTCG: number;
  otherForeignAssetsGains: number;
  // RSUs/ESOPs/ESSPs
  rsuGains: number;
  esopGains: number;
  esspGains: number;
  // Business Income fields
  intradayTradingIncome: number;
  tradingBusinessExpenses: number;
  professionalIncome?: number;
  professionalExpenses?: number;
  businessIncomeSmall?: number;
  businessExpensesSmall?: number;
  largeBusinessIncome?: number;
  largeBusinessExpenses?: number;
  otherBusinessIncome: number;
  businessExpenses: number;
  // Financial Particulars
  isPresumptiveTaxation?: boolean;
  presumptiveIncomeRate?: number;
  totalTurnover?: number;
  requiresAudit?: boolean;
  auditorName?: string;
  auditReportDate?: string;
  // Financial Statements & Disclosures
  totalAssets?: number;
  totalLiabilities?: number;
  grossProfit?: number;
  netProfit?: number;
  maintainsBooksOfAccounts?: boolean;
  hasQuantitativeDetails?: boolean;
  quantitativeDetails?: string;
}

export interface TaxCalculationResult {
  newRegime: {
    totalIncome: number;
    taxableIncome: number;
    incomeTax: number;
    surcharge: number;
    cess: number;
    totalTax: number;
    taxPaid: number;
    refundOrDemand: number;
    slabCalculations: Array<{
      slabDescription: string;
      incomeInSlab: number;
      taxRate: number;
      taxAmount: number;
    }>;
    apiResponse?: any;
  };
  form16Data?: any;
}
