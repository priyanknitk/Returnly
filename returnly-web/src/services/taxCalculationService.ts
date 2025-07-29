import { TaxData, TaxCalculationResult } from '../types/taxData';
import { API_ENDPOINTS } from '../config/api';

export class TaxCalculationService {
  
  /**
   * Calculate total capital gains from all sources
   */
  static calculateTotalCapitalGains(data: TaxData): number {
    return data.stocksSTCG + data.stocksLTCG + data.mutualFundsSTCG + 
           data.mutualFundsLTCG + data.fnoGains + data.realEstateSTCG + 
           data.realEstateLTCG + data.bondsSTCG + data.bondsLTCG + 
           data.goldSTCG + data.goldLTCG + data.cryptoGains +
           data.usStocksSTCG + data.usStocksLTCG + data.otherForeignAssetsGains +
           data.rsuGains + data.esopGains + data.esspGains;
  }

  /**
   * Calculate net business income
   */
  static calculateNetBusinessIncome(data: TaxData): number {
    const basicBusinessIncome = Math.max(0, (data.intradayTradingIncome + data.otherBusinessIncome) - 
                                           (data.tradingBusinessExpenses + data.businessExpenses));
    
    // Add other business income fields if they exist
    const professionalIncome = Math.max(0, (data.professionalIncome || 0) - (data.professionalExpenses || 0));
    const smallBusinessIncome = Math.max(0, (data.businessIncomeSmall || 0) - (data.businessExpensesSmall || 0));
    const largeBusinessIncome = Math.max(0, (data.largeBusinessIncome || 0) - (data.largeBusinessExpenses || 0));
    
    return basicBusinessIncome + professionalIncome + smallBusinessIncome + largeBusinessIncome;
  }

  /**
   * Calculate total income from all sources
   */
  static calculateTotalIncome(data: TaxData): number {
    const totalCapitalGains = this.calculateTotalCapitalGains(data);
    const netBusinessIncome = this.calculateNetBusinessIncome(data);
    
    return data.salarySection17 + data.perquisites + data.profitsInLieu + 
           data.interestOnSavings + data.interestOnFixedDeposits + data.dividendIncome +
           totalCapitalGains + netBusinessIncome;
  }

  /**
   * Calculate taxable income after deductions
   */
  static calculateTaxableIncome(data: TaxData): number {
    const totalIncome = this.calculateTotalIncome(data);
    return totalIncome - data.standardDeduction - data.professionalTax;
  }

  /**
   * Create fallback tax calculation when API fails
   */
  static createFallbackCalculation(data: TaxData): TaxCalculationResult {
    const totalIncome = this.calculateTotalIncome(data);
    const taxableIncome = this.calculateTaxableIncome(data);
    const baseTax = Math.max(0, taxableIncome * 0.1);
    const cess = baseTax * 0.04;
    const totalTax = baseTax + cess;
    
    return {
      newRegime: {
        totalIncome: totalIncome,
        taxableIncome: taxableIncome,
        incomeTax: baseTax,
        surcharge: 0,
        cess: cess,
        totalTax: totalTax,
        taxPaid: data.totalTaxDeducted,
        refundOrDemand: data.totalTaxDeducted - totalTax,
        slabCalculations: [
          {
            slabDescription: 'API Error - Using fallback calculation',
            incomeInSlab: taxableIncome,
            taxRate: 10,
            taxAmount: baseTax
          }
        ]
      }
    };
  }

  /**
   * Calculate taxes using the backend API
   */
  static async calculateTaxes(data: TaxData, age: number = 30): Promise<TaxCalculationResult> {
    const totalIncome = this.calculateTotalIncome(data);
    const taxableIncome = this.calculateTaxableIncome(data);
    
    try {
      const response = await fetch(`${API_ENDPOINTS.TAX_CALCULATE}`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          taxableIncome: taxableIncome,
          financialYear: data.financialYear,
          age: age,
          tdsDeducted: data.totalTaxDeducted
        })
      });

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      const apiResult = await response.json();
      
      return {
        newRegime: {
          totalIncome: totalIncome,
          taxableIncome: taxableIncome,
          incomeTax: apiResult.taxCalculation.totalTax,
          surcharge: apiResult.taxCalculation.surcharge,
          cess: apiResult.taxCalculation.healthAndEducationCess,
          totalTax: apiResult.taxCalculation.totalTaxWithCess,
          taxPaid: data.totalTaxDeducted,
          refundOrDemand: apiResult.refundCalculation.isRefundDue ? 
            apiResult.refundCalculation.refundAmount : 
            -apiResult.refundCalculation.additionalTaxDue,
          slabCalculations: apiResult.taxCalculation.taxBreakdown.map((slab: any) => ({
            slabDescription: slab.slabDescription,
            incomeInSlab: slab.incomeInSlab,
            taxRate: slab.taxRate,
            taxAmount: slab.taxAmount
          })),
          apiResponse: apiResult
        }
      };
      
    } catch (error) {
      console.error('Error calling tax calculation API:', error);
      return this.createFallbackCalculation(data);
    }
  }
}
