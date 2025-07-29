import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { Container, Box, Stepper, Step, StepLabel, Typography, Button, Stack } from '@mui/material';
import { ArrowBack } from '@mui/icons-material';
import PersonalDetailsForm from './PersonalDetailsForm';
import TaxDataInput from './TaxDataInput';
import TaxResults from './TaxResults';
import ITRGeneration from './ITRGeneration';
import { AdditionalTaxpayerInfoDto, Form16DataDto } from '../types/api';
import { TaxData } from '../types/taxData';
import { DEFAULT_PERSONAL_INFO } from '../constants/defaultValues';
import { useTaxDataPersistence } from '../hooks/useTaxDataPersistence';
import { TaxCalculationService } from '../services/taxCalculationService';
import { convertTaxDataToForm16Data } from '../utils/taxDataConverter';

interface TaxFilingWizardProps {
  onComplete: (results: any) => void;
}

const TaxFilingWizard: React.FC<TaxFilingWizardProps> = ({ onComplete }) => {
  const navigate = useNavigate();
  const { 
    currentStep: savedCurrentStep, 
    saveCurrentStep,
    hasSavedData,
    personalInfo: savedPersonalInfo,
    form16Data: savedTaxData, // This now stores TaxData directly
    saveForm16Data: saveTaxData
  } = useTaxDataPersistence();
  
  const [currentStep, setCurrentStep] = useState(0);
  const [taxData, setTaxData] = useState<TaxData | null>(null);
  const [taxResults, setTaxResults] = useState<any>(null);
  const [personalInfo, setPersonalInfo] = useState<AdditionalTaxpayerInfoDto>(DEFAULT_PERSONAL_INFO);

  // Load saved step on mount
  useEffect(() => {
    console.log('TaxFilingWizard useEffect: Loading saved data');
    console.log('savedTaxData:', savedTaxData);
    console.log('savedCurrentStep:', savedCurrentStep);
    console.log('hasSavedData():', hasSavedData());
    
    // Debug: Check what's in localStorage
    console.log('localStorage returnly_form16_data:', localStorage.getItem('returnly_form16_data'));
    console.log('localStorage returnly_current_step:', localStorage.getItem('returnly_current_step'));
    
    if (hasSavedData() && savedCurrentStep > 0) {
      console.log('Restoring saved step:', savedCurrentStep);
      setCurrentStep(savedCurrentStep);
    }
    
    if (savedPersonalInfo) {
      console.log('Restoring saved personal info');
      setPersonalInfo(savedPersonalInfo);
    }
    
    if (savedTaxData) {
      console.log('Restoring saved tax data:', savedTaxData);
      // Directly use the saved TaxData - no conversion needed!
      setTaxData(savedTaxData);
    } else {
      console.log('No saved tax data found');
    }
  }, []);

  // Auto-save current step when it changes
  useEffect(() => {
    if (currentStep > 0) {
      saveCurrentStep(currentStep);
    }
  }, [currentStep]); // Remove saveCurrentStep from dependency array

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
    
    // Save TaxData directly to browser storage - no conversion needed!
    saveTaxData(data);
    console.log('Tax data saved to browser storage:', data);
    
    // Perform actual tax calculation using the shared service
    try {
      const calculatedResults = await TaxCalculationService.calculateTaxes(data, 30);
      setTaxResults(calculatedResults);
    } catch (error) {
      console.error('Error in tax calculation:', error);
      // The service already handles fallbacks, so this shouldn't happen
      const fallbackResults = TaxCalculationService.createFallbackCalculation(data);
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
              initialData={taxData || undefined}
              onCalculate={handleTaxDataCalculate}
            />
          </Box>
        );
      case 2:
        // If we're on step 2 but don't have tax data, redirect to step 1
        if (!taxData) {
          console.log('On step 2 but no tax data, redirecting to step 1');
          setCurrentStep(1);
          return null;
        }
        
        // If we have tax data but no results, redirect to step 1 to recalculate
        if (!taxResults) {
          console.log('On step 2 but no tax results, redirecting to step 1 to recalculate');
          setCurrentStep(1);
          return null;
        }
        
        return (
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
        );
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
              form16Data={convertTaxDataToForm16Data(taxData, personalInfo)}
              personalInfo={personalInfo}
              taxCalculationResult={taxResults?.newRegime?.apiResponse?.taxCalculation}
              refundCalculationResult={taxResults?.newRegime?.apiResponse?.refundCalculation}
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
