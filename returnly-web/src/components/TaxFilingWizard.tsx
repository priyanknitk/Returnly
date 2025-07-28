import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Container, Box, Stepper, Step, StepLabel, Typography } from '@mui/material';
import PersonalDetailsForm from './PersonalDetailsForm';
import TaxDataInput from './TaxDataInput';
import ITRGeneration from './ITRGeneration';
import { AdditionalTaxpayerInfoDto, Form16DataDto } from '../types/api';

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
  const [personalInfo, setPersonalInfo] = useState<AdditionalTaxpayerInfoDto>({
    dateOfBirth: '',
    address: '',
    city: '',
    state: '',
    pincode: '',
    emailAddress: '',
    mobileNumber: '',
    aadhaarNumber: '',
    bankAccountNumber: '',
    bankIFSCCode: '',
    bankName: '',
    hasHouseProperty: false,
    houseProperties: [],
    hasCapitalGains: false,
    capitalGains: [],
    hasForeignIncome: false,
    foreignIncome: 0,
    hasForeignAssets: false,
    foreignAssets: [],
    hasBusinessIncome: false,
    businessIncomes: [],
    businessExpenses: []
  });

  const steps = ['Personal Details', 'Tax Data Input', 'ITR Generation'];

  const handlePersonalInfoChange = (info: Partial<AdditionalTaxpayerInfoDto>) => {
    setPersonalInfo(prev => ({ ...prev, ...info }));
  };

  const handlePersonalDetailsNext = () => {
    setCurrentStep(1);
  };

  const handleTaxDataCalculate = (data: TaxData) => {
    // Store the tax data and move to ITR generation step
    setTaxData(data);
    setCurrentStep(2);
  };

  const handleBackToTaxData = () => {
    setCurrentStep(1);
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
            
            <TaxDataInput
              onCalculate={handleTaxDataCalculate}
            />
          </Box>
        );
      case 2:
        return taxData ? (
          <Box sx={{ maxWidth: 1200, mx: 'auto' }}>
            {/* Step indicator for ITR generation page */}
            <Box sx={{ mb: 4 }}>
              <Stepper activeStep={2} alternativeLabel>
                {steps.map((label) => (
                  <Step key={label}>
                    <StepLabel>{label}</StepLabel>
                  </Step>
                ))}
              </Stepper>
            </Box>
            
            <ITRGeneration
              form16Data={taxData as unknown as Form16DataDto}
              personalInfo={personalInfo}
              onBack={handleBackToTaxData}
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
