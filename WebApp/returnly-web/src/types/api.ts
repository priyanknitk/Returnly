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
