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
    slabCalculations?: any[];
    apiResponse?: any;
  };
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

/**
 * Type guard to check if the response is an error
 */
export const isTaxResultsError = (response: TaxResultsResponse): response is TaxResultsError => {
  return 'error' in response;
};

/**
 * Maps API tax results to the format expected by TaxResults component
 * Centralizes the logic that was duplicated between App.tsx and TaxFilingWizard.tsx
 */
export const mapTaxResultsToComponents = (
  results: ApiTaxResults | null,
  taxData?: TaxData
): TaxResultsResponse => {
  // Return proper error state if no results provided
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

  const displayResults = results;

  const newRegime = displayResults.newRegime;
  const surcharge = newRegime?.surcharge || 0;
  const taxableIncome = newRegime?.taxableIncome || 0;
  const totalTax = newRegime?.totalTax || 0;
  const taxPaid = newRegime?.taxPaid || 0;

  // Convert the results to match TaxResults component interface
  const taxCalculation: TaxCalculationResult = {
    taxableIncome,
    financialYear: taxData?.financialYear || '2023-24',
    taxRegime: 'New Tax Regime',
    totalTax: newRegime?.incomeTax || 0,
    surcharge,
    surchargeRate: surcharge > 0 ? 10 : 0, // Default to 10% if surcharge exists
    healthAndEducationCess: newRegime?.cess || 0,
    totalTaxWithCess: totalTax,
    effectiveTaxRate: taxableIncome > 0 ? (totalTax / taxableIncome) * 100 : 0,
    taxBreakdown: newRegime?.slabCalculations || [
      {
        slabDescription: 'Calculation not available',
        incomeInSlab: taxableIncome,
        taxRate: 0,
        taxAmount: 0
      }
    ],
    // Advance Tax Penalty fields - get from API response or default to 0
    section234AInterest: newRegime?.apiResponse?.section234AInterest || 0,
    section234BInterest: newRegime?.apiResponse?.section234BInterest || 0,
    section234CInterest: newRegime?.apiResponse?.section234CInterest || 0,
    totalAdvanceTaxPenalties: newRegime?.apiResponse?.totalAdvanceTaxPenalties || 0,
    totalTaxWithAdvanceTaxPenalties: newRegime?.apiResponse?.totalTaxLiabilityWithPenalties || 0,
    hasAdvanceTaxPenalties: newRegime?.apiResponse?.hasAdvanceTaxPenalties || false
  };

  // Use API response if available, otherwise use the converted values
  if (newRegime?.apiResponse) {
    const apiData = newRegime.apiResponse;
    taxCalculation.totalTax = apiData.taxCalculation?.totalTax || taxCalculation.totalTax;
    taxCalculation.surcharge = apiData.taxCalculation?.surcharge || taxCalculation.surcharge;
    taxCalculation.surchargeRate = apiData.taxCalculation?.surchargeRate || taxCalculation.surchargeRate;
    taxCalculation.healthAndEducationCess = apiData.taxCalculation?.healthAndEducationCess || taxCalculation.healthAndEducationCess;
    taxCalculation.totalTaxWithCess = apiData.taxCalculation?.totalTaxWithCess || taxCalculation.totalTaxWithCess;
    taxCalculation.totalTaxWithAdvanceTaxPenalties = apiData.taxCalculation?.totalTaxLiabilityWithPenalties;
    taxCalculation.effectiveTaxRate = apiData.taxCalculation?.effectiveTaxRate || taxCalculation.effectiveTaxRate;
    
    if (apiData.taxCalculation?.taxBreakdown) {
      taxCalculation.taxBreakdown = apiData.taxCalculation.taxBreakdown.map((slab: any) => ({
        slabDescription: slab.slabDescription,
        incomeInSlab: slab.incomeInSlab,
        taxRate: slab.taxRate,
        taxAmount: slab.taxAmount
      }));
    }
    
    // Update penalty fields from API response (they're at the root level of the response)
    taxCalculation.section234AInterest = apiData.section234AInterest || 0;
    taxCalculation.section234BInterest = apiData.section234BInterest || 0;
    taxCalculation.section234CInterest = apiData.section234CInterest || 0;
    taxCalculation.totalAdvanceTaxPenalties = apiData.totalAdvanceTaxPenalties || 0;
    taxCalculation.hasAdvanceTaxPenalties = apiData.hasAdvanceTaxPenalties || false;
  }

  const refundCalculation: TaxRefundCalculation = {
    totalTaxLiability: totalTax,
    tdsDeducted: taxPaid,
    refundAmount: displayResults?.newRegime?.apiResponse?.refundCalculation?.refundAmount,
    additionalTaxDue: displayResults?.newRegime?.apiResponse?.refundCalculation?.additionalTaxDue,
    isRefundDue: displayResults?.newRegime?.apiResponse?.refundCalculation?.isRefundDue
  };

  return {
    taxCalculation,
    refundCalculation
  };
};
