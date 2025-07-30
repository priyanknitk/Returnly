import { TaxCalculationResult, TaxRefundCalculation } from '../components/TaxResults';

interface ApiTaxResults {
  newRegime?: {
    totalIncome?: number;
    taxableIncome?: number;
    incomeTax?: number;
    surcharge?: number;
    cess?: number;
    totalTax?: number;
    taxPaid?: number;
    refundOrDemand?: number;
    slabCalculations?: TaxSlabCalculation[];
    apiResponse?: ApiResponse;
  };
}

interface ApiResponse {
  taxCalculation?: {
    totalTax?: number;
    surcharge?: number;
    surchargeRate?: number;
    healthAndEducationCess?: number;
    totalTaxWithCess?: number;
    totalTaxLiabilityWithPenalties?: number;
    effectiveTaxRate?: number;
    taxBreakdown?: TaxSlabCalculation[];
  };
  refundCalculation?: {
    refundAmount?: number;
    additionalTaxDue?: number;
    isRefundDue?: boolean;
  };
  section234AInterest?: number;
  section234BInterest?: number;
  section234CInterest?: number;
  totalAdvanceTaxPenalties?: number;
  hasAdvanceTaxPenalties?: boolean;
}

interface TaxSlabCalculation {
  slabDescription: string;
  incomeInSlab: number;
  taxRate: number;
  taxAmount: number;
}

interface TaxData {
  financialYear?: string;
}

export interface MappedTaxResults {
  taxCalculation: TaxCalculationResult;
  refundCalculation: TaxRefundCalculation;
  error?: never;
}

export interface TaxResultsError {
  error: string;
  errorType: 'NO_DATA' | 'API_ERROR' | 'CALCULATION_ERROR';
  details?: string;
  taxCalculation?: never;
  refundCalculation?: never;
}

export type TaxResultsResponse = MappedTaxResults | TaxResultsError;

// Constants
const DEFAULT_FINANCIAL_YEAR = '2023-24';
const DEFAULT_TAX_REGIME = 'New Tax Regime';
const DEFAULT_SURCHARGE_RATE = 10;
const DEFAULT_SLAB_CALCULATION = {
  slabDescription: 'Calculation not available',
  incomeInSlab: 0,
  taxRate: 0,
  taxAmount: 0
};

/**
 * Type guard to check if the response is an error
 */
export const isTaxResultsError = (response: TaxResultsResponse): response is TaxResultsError => {
  return 'error' in response;
};

/**
 * Validates the API tax results structure
 */
const validateApiResults = (results: ApiTaxResults | null): TaxResultsError | null => {
  if (!results) {
    return {
      error: 'Tax calculation results are not available',
      errorType: 'NO_DATA',
      details: 'Please ensure the tax calculation was completed successfully'
    };
  }

  if (!results.newRegime || !results.newRegime.apiResponse) {
    return {
      error: 'Tax calculation data is incomplete',
      errorType: 'API_ERROR',
      details: 'The tax calculation response is missing required data'
    };
  }

  return null;
};

/**
 * Safely gets a numeric value with fallback
 */
const getNumericValue = (value: number | undefined, fallback: number = 0): number => {
  return typeof value === 'number' && !isNaN(value) ? value : fallback;
};

/**
 * Creates default tax breakdown when API data is unavailable
 */
const createDefaultTaxBreakdown = (taxableIncome: number): TaxSlabCalculation[] => {
  return [{
    ...DEFAULT_SLAB_CALCULATION,
    incomeInSlab: taxableIncome
  }];
};

/**
 * Maps API tax breakdown to component format
 */
const mapTaxBreakdown = (
  apiBreakdown?: TaxSlabCalculation[],
  fallbackIncome: number = 0
): TaxSlabCalculation[] => {
  if (!apiBreakdown || !Array.isArray(apiBreakdown) || apiBreakdown.length === 0) {
    return createDefaultTaxBreakdown(fallbackIncome);
  }

  return apiBreakdown.map(slab => ({
    slabDescription: slab.slabDescription || 'Unknown slab',
    incomeInSlab: getNumericValue(slab.incomeInSlab),
    taxRate: getNumericValue(slab.taxRate),
    taxAmount: getNumericValue(slab.taxAmount)
  }));
};

/**
 * Calculates effective tax rate safely
 */
const calculateEffectiveTaxRate = (totalTax: number, taxableIncome: number): number => {
  return taxableIncome > 0 ? (totalTax / taxableIncome) * 100 : 0;
};

/**
 * Maps penalty information from API response
 */
const mapPenaltyInfo = (apiResponse?: ApiResponse) => ({
  section234AInterest: getNumericValue(apiResponse?.section234AInterest),
  section234BInterest: getNumericValue(apiResponse?.section234BInterest),
  section234CInterest: getNumericValue(apiResponse?.section234CInterest),
  totalAdvanceTaxPenalties: getNumericValue(apiResponse?.totalAdvanceTaxPenalties),
  hasAdvanceTaxPenalties: apiResponse?.hasAdvanceTaxPenalties ?? false
});

/**
 * Creates tax calculation result from API data
 */
const createTaxCalculationResult = (
  newRegime: NonNullable<ApiTaxResults['newRegime']>,
  financialYear: string
): TaxCalculationResult => {
  const taxableIncome = getNumericValue(newRegime.taxableIncome);
  const surcharge = getNumericValue(newRegime.surcharge);
  const totalTax = getNumericValue(newRegime.totalTax);
  const apiResponse = newRegime.apiResponse;
  const apiTaxCalc = apiResponse?.taxCalculation;

  // Use API response values if available, otherwise use basic calculation
  const finalTotalTax = getNumericValue(apiTaxCalc?.totalTax, getNumericValue(newRegime.incomeTax));
  const finalSurcharge = getNumericValue(apiTaxCalc?.surcharge, surcharge);
  const finalSurchargeRate = getNumericValue(
    apiTaxCalc?.surchargeRate, 
    surcharge > 0 ? DEFAULT_SURCHARGE_RATE : 0
  );
  const finalHealthAndEducationCess = getNumericValue(
    apiTaxCalc?.healthAndEducationCess, 
    getNumericValue(newRegime.cess)
  );
  const finalTotalTaxWithCess = getNumericValue(apiTaxCalc?.totalTaxWithCess, totalTax);
  const finalEffectiveTaxRate = getNumericValue(
    apiTaxCalc?.effectiveTaxRate,
    calculateEffectiveTaxRate(finalTotalTaxWithCess, taxableIncome)
  );

  return {
    taxableIncome,
    financialYear,
    taxRegime: DEFAULT_TAX_REGIME,
    totalTax: finalTotalTax,
    surcharge: finalSurcharge,
    surchargeRate: finalSurchargeRate,
    healthAndEducationCess: finalHealthAndEducationCess,
    totalTaxWithCess: finalTotalTaxWithCess,
    effectiveTaxRate: finalEffectiveTaxRate,
    taxBreakdown: mapTaxBreakdown(
      apiTaxCalc?.taxBreakdown || newRegime.slabCalculations,
      taxableIncome
    ),
    totalTaxWithAdvanceTaxPenalties: getNumericValue(apiTaxCalc?.totalTaxLiabilityWithPenalties),
    ...mapPenaltyInfo(apiResponse)
  };
};

/**
 * Creates refund calculation result from API data
 */
const createRefundCalculationResult = (
  newRegime: NonNullable<ApiTaxResults['newRegime']>
): TaxRefundCalculation => {
  const apiRefund = newRegime.apiResponse?.refundCalculation;
  
  return {
    totalTaxLiability: getNumericValue(newRegime.totalTax),
    tdsDeducted: getNumericValue(newRegime.taxPaid),
    refundAmount: getNumericValue(apiRefund?.refundAmount),
    additionalTaxDue: getNumericValue(apiRefund?.additionalTaxDue),
    isRefundDue: apiRefund?.isRefundDue ?? false
  };
};

/**
 * Maps API tax results to the format expected by TaxResults component
 * Centralizes the logic that was duplicated between App.tsx and TaxFilingWizard.tsx
 */
export const mapTaxResultsToComponents = (
  results: ApiTaxResults | null,
  taxData?: TaxData
): TaxResultsResponse => {
  // Validate input data
  const validationError = validateApiResults(results);
  if (validationError) {
    return validationError;
  }

  // At this point, we know results and newRegime are valid due to validation
  const newRegime = results!.newRegime!;
  const financialYear = taxData?.financialYear || DEFAULT_FINANCIAL_YEAR;

  try {
    const taxCalculation = createTaxCalculationResult(newRegime, financialYear);
    const refundCalculation = createRefundCalculationResult(newRegime);

    return {
      taxCalculation,
      refundCalculation
    };
  } catch (error) {
    return {
      error: 'Error processing tax calculation data',
      errorType: 'CALCULATION_ERROR',
      details: error instanceof Error ? error.message : 'Unknown error occurred'
    };
  }
};
