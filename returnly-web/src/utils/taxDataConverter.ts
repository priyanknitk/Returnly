import { TaxData } from '../types/taxData';
import { Form16DataDto, AdditionalTaxpayerInfoDto } from '../types/api';

export const convertTaxDataToForm16Data = (
  data: TaxData, 
  personalInfo?: AdditionalTaxpayerInfoDto
): Form16DataDto => {
  return {
    employeeName: personalInfo?.employeeName || 'Manual Entry User',
    pan: personalInfo?.pan || '',
    assessmentYear: data.assessmentYear,
    financialYear: data.financialYear,
    employerName: data.employerName,
    tan: data.tan,
    grossSalary: data.salarySection17 + data.perquisites + data.profitsInLieu,
    totalTaxDeducted: data.totalTaxDeducted,
    standardDeduction: data.standardDeduction,
    professionalTax: data.professionalTax,
    form16B: {
      salarySection17: data.salarySection17,
      perquisites: data.perquisites,
      profitsInLieu: data.profitsInLieu,
      basicSalary: 0, // These would come from actual Form16 processing
      hra: 0,
      specialAllowance: 0,
      otherAllowances: 0,
      interestOnSavings: data.interestOnSavings,
      interestOnFixedDeposits: data.interestOnFixedDeposits,
      interestOnBonds: 0,
      otherInterestIncome: 0,
      dividendIncomeAI: data.dividendIncome,
      dividendIncomeAII: 0,
      otherDividendIncome: 0,
      standardDeduction: data.standardDeduction,
      professionalTax: data.professionalTax,
      taxableIncome: data.salarySection17 + data.perquisites + data.profitsInLieu - data.standardDeduction - data.professionalTax
    },
    form16A: {
      employeeName: personalInfo?.employeeName || 'Manual Entry User',
      pan: personalInfo?.pan || '',
      assessmentYear: data.assessmentYear,
      financialYear: data.financialYear,
      employerName: data.employerName,
      tan: data.tan,
      certificateNumber: '',
      totalTaxDeducted: data.totalTaxDeducted,
      q1TDS: data.totalTaxDeducted / 4, // Default quarterly distribution
      q2TDS: data.totalTaxDeducted / 4,
      q3TDS: data.totalTaxDeducted / 4,
      q4TDS: data.totalTaxDeducted / 4
    },
    // Business income fields
    intradayTradingIncome: data.intradayTradingIncome,
    tradingBusinessExpenses: data.tradingBusinessExpenses,
    otherBusinessIncome: data.otherBusinessIncome,
    businessExpenses: data.businessExpenses,
    // Capital Gains fields
    stocksSTCG: data.stocksSTCG,
    stocksLTCG: data.stocksLTCG,
    mutualFundsSTCG: data.mutualFundsSTCG,
    mutualFundsLTCG: data.mutualFundsLTCG,
    fnoGains: data.fnoGains,
    realEstateSTCG: data.realEstateSTCG,
    realEstateLTCG: data.realEstateLTCG,
    bondsSTCG: data.bondsSTCG,
    bondsLTCG: data.bondsLTCG,
    goldSTCG: data.goldSTCG,
    goldLTCG: data.goldLTCG,
    cryptoGains: data.cryptoGains,
    usStocksSTCG: data.usStocksSTCG,
    usStocksLTCG: data.usStocksLTCG,
    otherForeignAssetsGains: data.otherForeignAssetsGains,
    rsuGains: data.rsuGains,
    esopGains: data.esopGains,
    esspGains: data.esspGains
  };
};
