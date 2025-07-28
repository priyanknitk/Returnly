// API response types matching the backend DTOs

export interface Form16DataDto {
  employeeName: string;
  pan: string;
  assessmentYear: string;
  financialYear: string;
  employerName: string;
  tan: string;
  grossSalary: number;
  totalTaxDeducted: number;
  standardDeduction: number;
  professionalTax: number;
  form16B: Form16BDataDto;
  annexure: AnnexureDataDto;
  // Business income fields (for manual tax data input)
  intradayTradingIncome?: number;
  tradingBusinessExpenses?: number;
  professionalIncome?: number;
  professionalExpenses?: number;
  businessIncomeSmall?: number;
  businessExpensesSmall?: number;
  largeBusinessIncome?: number;
  largeBusinessExpenses?: number;
  otherBusinessIncome?: number;
  businessExpenses?: number;
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
  // Capital Gains fields
  stocksSTCG?: number;
  stocksLTCG?: number;
  mutualFundsSTCG?: number;
  mutualFundsLTCG?: number;
  fnoGains?: number;
  realEstateSTCG?: number;
  realEstateLTCG?: number;
  bondsSTCG?: number;
  bondsLTCG?: number;
  goldSTCG?: number;
  goldLTCG?: number;
  cryptoGains?: number;
  usStocksSTCG?: number;
  usStocksLTCG?: number;
  otherForeignAssetsGains?: number;
  rsuGains?: number;
  esopGains?: number;
  esspGains?: number;
}

export interface Form16BDataDto {
  salarySection17: number;
  perquisites: number;
  profitsInLieu: number;
  basicSalary: number;
  hra: number;
  specialAllowance: number;
  otherAllowances: number;
  interestOnSavings: number;
  interestOnFixedDeposits: number;
  interestOnBonds: number;
  otherInterestIncome: number;
  dividendIncomeAI: number;
  dividendIncomeAII: number;
  otherDividendIncome: number;
  standardDeduction: number;
  professionalTax: number;
  taxableIncome: number;
}

export interface AnnexureDataDto {
  q1TDS: number;
  q2TDS: number;
  q3TDS: number;
  q4TDS: number;
}

export interface TaxCalculationRequestDto {
  taxableIncome: number;
  financialYear: string;
  age: number;
  tdsDeducted: number;
}

export interface TaxCalculationResponseDto {
  taxCalculation: TaxCalculationResultDto;
  refundCalculation: TaxRefundCalculationDto;
}

export interface TaxCalculationResultDto {
  taxableIncome: number;
  financialYear: string;
  taxRegime: string;
  age: number;
  taxBreakdown: TaxSlabCalculationDto[];
  totalTax: number;
  surcharge: number;
  surchargeRate: number;
  totalTaxWithSurcharge: number;
  healthAndEducationCess: number;
  totalTaxWithCess: number;
  effectiveTaxRate: number;
}

export interface TaxSlabCalculationDto {
  slabDescription: string;
  incomeInSlab: number;
  taxRate: number;
  taxAmount: number;
  minIncome: number;
  maxIncome: number | null;
}

export interface TaxRefundCalculationDto {
  totalTaxLiability: number;
  tdsDeducted: number;
  refundAmount: number;
  additionalTaxDue: number;
  isRefundDue: boolean;
}

export interface RegimeComparisonRequestDto {
  taxableIncome: number;
  financialYear: string;
  age: number;
  oldRegimeDeductions: number;
}

export interface RegimeComparisonResponseDto {
  oldRegimeCalculation: TaxCalculationResultDto;
  newRegimeCalculation: TaxCalculationResultDto;
  recommendedRegime: string;
  taxSavings: number;
  comparisonSummary: string;
}

export interface ApiError {
  error: string;
  details?: string;
}

// ITR Generation Types
export interface ITRGenerationRequestDto {
  form16Data: Form16DataDto;
  additionalInfo: AdditionalTaxpayerInfoDto;
  preferredITRType?: string;
}

export interface AdditionalTaxpayerInfoDto {
  dateOfBirth: string;
  address: string;
  city: string;
  state: string;
  pincode: string;
  emailAddress: string;
  mobileNumber: string;
  aadhaarNumber: string;
  bankAccountNumber: string;
  bankIFSCCode: string;
  bankName: string;
  hasHouseProperty: boolean;
  houseProperties: HousePropertyDetailsDto[];
  hasCapitalGains: boolean;
  capitalGains: CapitalGainDetailsDto[];
  hasOtherIncome: boolean;
  otherInterestIncome: number;
  otherDividendIncome: number;
  otherSourcesIncome: number;
  hasForeignIncome: boolean;
  foreignIncome: number;
  hasForeignAssets: boolean;
  foreignAssets: ForeignAssetDetailsDto[];
  hasBusinessIncome: boolean;
  businessIncomes: BusinessIncomeDetailsDto[];
  businessExpenses: BusinessExpenseDetailsDto[];
}

export interface HousePropertyDetailsDto {
  propertyAddress: string;
  annualValue: number;
  propertyTax: number;
  interestOnLoan: number;
}

export interface CapitalGainDetailsDto {
  assetType: string;
  dateOfSale: string;
  dateOfPurchase: string;
  salePrice: number;
  costOfAcquisition: number;
  costOfImprovement: number;
  expensesOnTransfer: number;
}

export interface ForeignAssetDetailsDto {
  assetType: string;
  country: string;
  value: number;
  currency: string;
}

export interface BusinessIncomeDetailsDto {
  incomeType: string;
  description: string;
  grossReceipts: number;
  otherIncome: number;
}

export interface BusinessExpenseDetailsDto {
  expenseCategory: string;
  description: string;
  amount: number;
  date: string;
  isCapitalExpense: boolean;
}

export interface ITRGenerationResponseDto {
  isSuccess: boolean;
  recommendedITRType: string;
  itrFormXml: string;
  itrFormJson: string;
  fileName: string;
  validationErrors: string[];
  warnings: string[];
  generationSummary: string;
  formData?: ITRFormDataDto;
}

export interface ITRFormDataDto {
  formType: string;
  assessmentYear: string;
  financialYear: string;
  pan: string;
  name: string;
  dateOfBirth: string;
  residencyStatus: string;
  address: string;
  city: string;
  state: string;
  pincode: string;
  emailAddress: string;
  mobileNumber: string;
  aadhaarNumber: string;
  bankAccountNumber: string;
  bankIFSCCode: string;
  bankName: string;
  totalIncome: number;
  taxLiability: number;
  refundOrDemand: number;
  isRefundDue: boolean;
  taxDeductedAtSource: number;
  advanceTax: number;
  selfAssessmentTax: number;
  itr1Data?: ITR1SpecificDataDto;
  itr2Data?: ITR2SpecificDataDto;
}

export interface ITR1SpecificDataDto {
  employerName: string;
  employerTAN: string;
  employerAddress: string;
  salaryIncome: number;
  housePropertyIncome: number;
  otherSourcesIncome: number;
  standardDeduction: number;
  professionalTax: number;
  q1TDS: number;
  q2TDS: number;
  q3TDS: number;
  q4TDS: number;
}

export interface ITR2SpecificDataDto {
  taxpayerCategory: string;
  hufName: string;
  salaryIncome: number;
  housePropertyIncome: number;
  capitalGainsIncome: number;
  otherSourcesIncome: number;
  foreignIncome: number;
  salaryDetails: SalaryDetailsDto[];
  houseProperties: HousePropertyDetailsDto[];
  capitalGains: CapitalGainDetailsDto[];
  foreignAssets: ForeignAssetDetailsDto[];
  tdsDetails: TDSDetailsDto[];
  hasForeignIncome: boolean;
  hasForeignAssets: boolean;
}

export interface SalaryDetailsDto {
  employerName: string;
  employerTAN: string;
  grossSalary: number;
  taxDeducted: number;
  certificateNumber: string;
}

export interface TDSDetailsDto {
  deductorName: string;
  deductorTAN: string;
  taxDeducted: number;
  certificateNumber: string;
  dateOfDeduction: string;
}

export interface ITRRecommendationRequestDto {
  form16Data: Form16DataDto;
  hasHouseProperty: boolean;
  hasCapitalGains: boolean;
  hasBusinessIncome: boolean;
  hasForeignIncome: boolean;
  hasForeignAssets: boolean;
  isHUF: boolean;
  totalIncome: number;
}

export interface ITRRecommendationResponseDto {
  recommendedITRType: string;
  reason: string;
  requirements: string[];
  limitations: string[];
  canUseITR1: boolean;
  canUseITR2: boolean;
  recommendationSummary: string;
}
