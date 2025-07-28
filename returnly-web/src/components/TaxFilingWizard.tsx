import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Container, Box, Stepper, Step, StepLabel, Typography, Button, Stack } from '@mui/material';
import { ArrowBack } from '@mui/icons-material';
import PersonalDetailsForm from './PersonalDetailsForm';
import TaxDataInput from './TaxDataInput';
import TaxResults from './TaxResults';
import ITRGeneration from './ITRGeneration';
import { AdditionalTaxpayerInfoDto, Form16DataDto } from '../types/api';
import { DEFAULT_PERSONAL_INFO } from '../constants/defaultValues';
import { API_ENDPOINTS } from '../config/api';

interface TaxFilingWizardProps {
  onComplete: (results: any) => void;
}

interface TaxData {
  employeeName: string;
  pan: string;
  assessmentYear: string;
  financialYear: string;
  employerName: string;
  tan: string;
  salarySection17: number;
  perquisites: number;
  profitsInLieu: number;
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
  otherBusinessIncome: number;
  businessExpenses: number;
}

const TaxFilingWizard: React.FC<TaxFilingWizardProps> = ({ onComplete }) => {
  const navigate = useNavigate();
  const [currentStep, setCurrentStep] = useState(0);
  const [taxData, setTaxData] = useState<TaxData | null>(null);
  const [taxResults, setTaxResults] = useState<any>(null);
  const [personalInfo, setPersonalInfo] = useState<AdditionalTaxpayerInfoDto>(DEFAULT_PERSONAL_INFO);

  const steps = ['Personal Details', 'Tax Data Input', 'Tax Results', 'ITR Generation'];

  const handlePersonalInfoChange = (info: Partial<AdditionalTaxpayerInfoDto>) => {
    setPersonalInfo(prev => ({ ...prev, ...info }));
  };

  const handlePersonalDetailsNext = () => {
    setCurrentStep(1);
  };

  const handleTaxDataCalculate = async (data: TaxData) => {
    // Store the tax data
    setTaxData(data);
    
    // Perform actual tax calculation
    try {
      const totalCapitalGains = data.stocksSTCG + data.stocksLTCG + data.mutualFundsSTCG + 
                               data.mutualFundsLTCG + data.fnoGains + data.realEstateSTCG + 
                               data.realEstateLTCG + data.bondsSTCG + data.bondsLTCG + 
                               data.goldSTCG + data.goldLTCG + data.cryptoGains +
                               data.usStocksSTCG + data.usStocksLTCG + data.otherForeignAssetsGains +
                               data.rsuGains + data.esopGains + data.esspGains;
      
      const netBusinessIncome = Math.max(0, (data.intradayTradingIncome + data.otherBusinessIncome) - 
                                            (data.tradingBusinessExpenses + data.businessExpenses));
      
      const totalIncome = data.salarySection17 + data.perquisites + data.profitsInLieu + 
                         data.interestOnSavings + data.interestOnFixedDeposits + data.dividendIncome +
                         totalCapitalGains + netBusinessIncome;
      
      const taxableIncome = totalIncome - data.standardDeduction - data.professionalTax;
      
      const response = await fetch(`${API_ENDPOINTS.TAX_CALCULATE}`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          taxableIncome: taxableIncome,
          financialYear: data.financialYear,
          age: 30, // Default age, could be made configurable
          tdsDeducted: data.totalTaxDeducted
        })
      });

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      const apiResult = await response.json();
      
      const calculatedResults = {
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
      
      setTaxResults(calculatedResults);
    } catch (error) {
      console.error('Error calling tax calculation API:', error);
      
      // Fallback calculation if API fails
      const totalIncome = data.salarySection17 + data.perquisites + data.profitsInLieu + 
                         data.interestOnSavings + data.interestOnFixedDeposits + data.dividendIncome;
      
      const taxableIncome = totalIncome - data.standardDeduction - data.professionalTax;
      const baseTax = Math.max(0, taxableIncome * 0.1);
      const cess = baseTax * 0.04;
      const totalTax = baseTax + cess;
      
      const fallbackResults = {
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
      
      setTaxResults(fallbackResults);
    }
    
    // Move to Tax Results step
    setCurrentStep(2);
  };

  const handleTaxResultsNext = (results: any) => {
    // Store tax results and move to ITR generation
    setTaxResults(results);
    setCurrentStep(3); // Move to ITR Generation step
    onComplete(results); // Notify parent component
  };

  const handleBackToTaxData = () => {
    setCurrentStep(1);
  };

  const handleBackToResults = () => {
    setCurrentStep(2);
  };

  const renderCurrentStep = () => {
    switch (currentStep) {
      case 0:
        return (
          <PersonalDetailsForm
            personalInfo={personalInfo}
            onPersonalInfoChange={handlePersonalInfoChange}
            onNext={handlePersonalDetailsNext}
          />
        );
      case 1:
        return (
          <Box sx={{ maxWidth: 1200, mx: 'auto' }}>
            {/* Step indicator for tax data page */}
            <Box sx={{ mb: 4 }}>
              <Stepper activeStep={1} alternativeLabel>
                {steps.map((label) => (
                  <Step key={label}>
                    <StepLabel>{label}</StepLabel>
                  </Step>
                ))}
              </Stepper>
            </Box>
            
            {/* Back button */}
            <Box sx={{ mb: 3 }}>
              <Button
                onClick={() => setCurrentStep(0)}
                startIcon={<ArrowBack />}
                variant="outlined"
                size="small"
                sx={{
                  textTransform: 'none',
                  borderRadius: 2,
                  fontSize: '0.875rem'
                }}
              >
                Back to Personal Details
              </Button>
            </Box>
            
            <TaxDataInput
              onCalculate={handleTaxDataCalculate}
            />
          </Box>
        );
      case 2:
        return taxData && taxResults ? (
          <Box sx={{ maxWidth: 1200, mx: 'auto' }}>
            {/* Step indicator for tax results page */}
            <Box sx={{ mb: 4 }}>
              <Stepper activeStep={2} alternativeLabel>
                {steps.map((label) => (
                  <Step key={label}>
                    <StepLabel>{label}</StepLabel>
                  </Step>
                ))}
              </Stepper>
            </Box>
            
            {/* Back button */}
            <Box sx={{ mb: 3 }}>
              <Button
                onClick={() => setCurrentStep(1)}
                startIcon={<ArrowBack />}
                variant="outlined"
                size="small"
                sx={{
                  textTransform: 'none',
                  borderRadius: 2,
                  fontSize: '0.875rem'
                }}
              >
                Back to Tax Data
              </Button>
            </Box>
            
            <TaxResults
              taxCalculation={{
                taxableIncome: taxResults.newRegime?.taxableIncome || 0,
                financialYear: taxData.financialYear || '2023-24',
                taxRegime: 'New Tax Regime',
                totalTax: taxResults.newRegime?.incomeTax || 0,
                surcharge: taxResults.newRegime?.surcharge || 0,
                surchargeRate: taxResults.newRegime?.surcharge > 0 ? 10 : 0,
                healthAndEducationCess: taxResults.newRegime?.cess || 0,
                totalTaxWithCess: taxResults.newRegime?.totalTax || 0,
                effectiveTaxRate: taxResults.newRegime?.taxableIncome > 0 
                  ? ((taxResults.newRegime?.totalTax || 0) / taxResults.newRegime?.taxableIncome) * 100 
                  : 0,
                taxBreakdown: taxResults.newRegime?.slabCalculations || [
                  {
                    slabDescription: 'Calculation not available',
                    incomeInSlab: taxResults.newRegime?.taxableIncome || 0,
                    taxRate: 0,
                    taxAmount: 0
                  }
                ]
              }}
              refundCalculation={{
                totalTaxLiability: taxResults.newRegime?.totalTax || 0,
                tdsDeducted: taxResults.newRegime?.taxPaid || 0,
                refundAmount: Math.max(0, (taxResults.newRegime?.taxPaid || 0) - (taxResults.newRegime?.totalTax || 0)),
                additionalTaxDue: Math.max(0, (taxResults.newRegime?.totalTax || 0) - (taxResults.newRegime?.taxPaid || 0)),
                isRefundDue: (taxResults.newRegime?.taxPaid || 0) > (taxResults.newRegime?.totalTax || 0)
              }}
              onGenerateITR={() => handleTaxResultsNext(taxResults)}
              showITRButton={true}
            />
          </Box>
        ) : null;
      case 3:
        return taxData ? (
          <Box sx={{ maxWidth: 1200, mx: 'auto' }}>
            {/* Step indicator for ITR generation page */}
            <Box sx={{ mb: 4 }}>
              <Stepper activeStep={3} alternativeLabel>
                {steps.map((label) => (
                  <Step key={label}>
                    <StepLabel>{label}</StepLabel>
                  </Step>
                ))}
              </Stepper>
            </Box>
            
            {/* Back button */}
            <Box sx={{ mb: 3 }}>
              <Button
                onClick={handleBackToResults}
                startIcon={<ArrowBack />}
                variant="outlined"
                size="small"
                sx={{
                  textTransform: 'none',
                  borderRadius: 2,
                  fontSize: '0.875rem'
                }}
              >
                Back to Tax Results
              </Button>
            </Box>
            
            <ITRGeneration
              form16Data={taxData as unknown as Form16DataDto}
              personalInfo={personalInfo}
              onBack={handleBackToResults}
            />
          </Box>
        ) : null;
      default:
        return null;
    }
  };

  return (
    <Container maxWidth="xl" sx={{ mt: 2, mb: 4 }}>
      {renderCurrentStep()}
    </Container>
  );
};

export default TaxFilingWizard;
