import React, { useState } from 'react';
import { BrowserRouter as Router, Routes, Route, useNavigate } from 'react-router-dom';
import { ThemeProvider, createTheme } from '@mui/material/styles';
import CssBaseline from '@mui/material/CssBaseline';
import { Container, AppBar, Toolbar, Typography, Box, Button } from '@mui/material';
import LandingPageSimple from './components/LandingPageSimple';
import Form16Upload from './components/Form16Upload';
import TaxDataInput from './components/TaxDataInput';
import TaxResults from './components/TaxResults';
import ITRGeneration from './components/ITRGeneration';
import { Form16DataDto } from './types/api';

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
}

const theme = createTheme({
  palette: {
    mode: 'light',
    primary: {
      main: '#1976d2',
    },
    secondary: {
      main: '#dc004e',
    },
  },
  typography: {
    h4: {
      fontWeight: 600,
    },
    h6: {
      fontWeight: 500,
    },
  },
});

function App() {
  const [form16Data, setForm16Data] = useState<Form16DataDto | null>(null);
  const [taxResults, setTaxResults] = useState<any>(null);
  const [currentForm16Data, setCurrentForm16Data] = useState<Form16DataDto | null>(null);

  // Update current form16Data when tax results include form16Data
  const handleTaxCalculation = (results: any) => {
    setTaxResults(results);
    if (results.form16Data) {
      setCurrentForm16Data(results.form16Data);
    }
  };

  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <Router>
        <AppBar position="sticky" elevation={2}>
          <Toolbar>
            <Typography variant="h6" component="div" sx={{ flexGrow: 1 }}>
              Returnly - Indian Tax Calculator & ITR Generator
            </Typography>
          </Toolbar>
        </AppBar>
        
        <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
          <Routes>
            <Route path="/" element={<LandingPageSimple />} />
            <Route path="/upload" element={<Form16Upload onUploadSuccess={setForm16Data} />} />
            <Route path="/calculate" element={<TaxCalculationPageWrapper form16Data={form16Data} onCalculate={handleTaxCalculation} />} />
            <Route path="/results" element={<TaxResultsPageWrapper results={taxResults} form16Data={currentForm16Data || form16Data} />} />
            <Route path="/itr-generation" element={<ITRGenerationPageWrapper form16Data={currentForm16Data || form16Data} />} />
          </Routes>
        </Container>
        
        <Box 
          component="footer" 
          sx={{ 
            py: 3, 
            px: 2, 
            mt: 'auto', 
            backgroundColor: (theme) => theme.palette.grey[200] 
          }}
        >
          <Container maxWidth="sm">
            <Typography variant="body2" color="text.secondary" align="center">
              Returnly © {new Date().getFullYear()} - Indian Tax Calculation & ITR Generation Made Easy
            </Typography>
          </Container>
        </Box>
      </Router>
    </ThemeProvider>
  );
}

const TaxCalculationPageWrapper: React.FC<{ 
  form16Data: Form16DataDto | null; 
  onCalculate: (results: any) => void 
}> = ({ form16Data, onCalculate }) => {
  const navigate = useNavigate();
  const [generatedForm16Data, setGeneratedForm16Data] = useState<Form16DataDto | null>(form16Data);

  const handleCalculate = (data: TaxData) => {
    // Convert manual TaxData to Form16DataDto format for ITR generation
    const convertedForm16Data: Form16DataDto = {
      employeeName: data.employeeName || 'Manual Entry User',
      pan: data.pan || '',
      assessmentYear: data.assessmentYear || '2024-25',
      financialYear: data.financialYear || '2023-24',
      employerName: data.employerName || 'Self Employed',
      tan: data.tan || 'ABCD12345E',
      grossSalary: data.salarySection17 + data.perquisites + data.profitsInLieu,
      totalTaxDeducted: data.totalTaxDeducted,
      standardDeduction: data.standardDeduction,
      professionalTax: data.professionalTax,
      form16B: {
        salarySection17: data.salarySection17,
        perquisites: data.perquisites,
        profitsInLieu: data.profitsInLieu,
        basicSalary: data.salarySection17 * 0.5, // Estimate 50% as basic
        hra: data.salarySection17 * 0.25, // Estimate 25% as HRA
        specialAllowance: data.salarySection17 * 0.2, // Estimate 20% as special allowance
        otherAllowances: data.salarySection17 * 0.05, // Estimate 5% as other allowances
        interestOnSavings: data.interestOnSavings,
        interestOnFixedDeposits: data.interestOnFixedDeposits,
        interestOnBonds: 0,
        otherInterestIncome: 0,
        dividendIncomeAI: data.dividendIncome,
        dividendIncomeAII: 0,
        otherDividendIncome: 0,
        standardDeduction: data.standardDeduction,
        professionalTax: data.professionalTax,
        taxableIncome: (data.salarySection17 + data.perquisites + data.profitsInLieu + 
                       data.interestOnSavings + data.interestOnFixedDeposits + data.dividendIncome) -
                      data.standardDeduction - data.professionalTax
      },
      annexure: {
        q1TDS: data.totalTaxDeducted * 0.25, // Distribute TDS equally across quarters
        q2TDS: data.totalTaxDeducted * 0.25,
        q3TDS: data.totalTaxDeducted * 0.25,
        q4TDS: data.totalTaxDeducted * 0.25
      }
    };

    // Store the converted data for ITR generation
    setGeneratedForm16Data(convertedForm16Data);
    
    // Calculate taxes using the same logic
    const totalIncome = data.salarySection17 + data.perquisites + data.profitsInLieu + 
                       data.interestOnSavings + data.interestOnFixedDeposits + data.dividendIncome;
    
    const taxableIncome = totalIncome - data.standardDeduction - data.professionalTax;
    const baseTax = Math.max(0, taxableIncome * 0.1); // Simple 10% tax for demo
    const cess = baseTax * 0.04; // 4% cess
    const totalTax = baseTax + cess;
    
    const mockResults = {
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
            slabDescription: 'Up to ₹3,00,000',
            incomeInSlab: Math.min(taxableIncome, 300000),
            taxRate: 0,
            taxAmount: 0
          },
          {
            slabDescription: '₹3,00,001 - ₹6,00,000',
            incomeInSlab: Math.min(Math.max(0, taxableIncome - 300000), 300000),
            taxRate: 5,
            taxAmount: Math.min(Math.max(0, taxableIncome - 300000), 300000) * 0.05
          }
        ]
      },
      // Store the form16Data for the results page
      form16Data: convertedForm16Data
    };
    
    onCalculate(mockResults);
    navigate('/results');
  };

  return <TaxDataInput onCalculate={handleCalculate} initialData={form16Data || undefined} />;
};

const TaxResultsPageWrapper: React.FC<{ results: any; form16Data: Form16DataDto | null }> = ({ results, form16Data }) => {
  const navigate = useNavigate();
  
  // Use passed results or fallback to mock data
  const displayResults = results || {
    newRegime: {
      totalIncome: 1200000,
      taxableIncome: 1150000,
      incomeTax: 45000,
      surcharge: 0,
      cess: 1800,
      totalTax: 46800,
      taxPaid: 55000,
      refundOrDemand: 8200,
      slabCalculations: []
    }
  };

  // Convert the results to match TaxResults component interface
  const taxCalculation = {
    taxableIncome: displayResults.newRegime?.taxableIncome || 0,
    financialYear: '2023-24',
    taxRegime: 'New Tax Regime',
    totalTax: displayResults.newRegime?.incomeTax || 0,
    surcharge: displayResults.newRegime?.surcharge || 0,
    surchargeRate: 0,
    healthAndEducationCess: displayResults.newRegime?.cess || 0,
    totalTaxWithCess: displayResults.newRegime?.totalTax || 0,
    effectiveTaxRate: displayResults.newRegime?.taxableIncome > 0 
      ? ((displayResults.newRegime?.totalTax || 0) / displayResults.newRegime?.taxableIncome) * 100 
      : 0,
    taxBreakdown: displayResults.newRegime?.slabCalculations || [
      {
        slabDescription: 'Up to ₹3,00,000',
        incomeInSlab: 300000,
        taxRate: 0,
        taxAmount: 0
      },
      {
        slabDescription: '₹3,00,001 - ₹6,00,000', 
        incomeInSlab: 300000,
        taxRate: 5,
        taxAmount: 15000
      },
      {
        slabDescription: '₹6,00,001 - ₹9,00,000',
        incomeInSlab: 300000,
        taxRate: 10,
        taxAmount: 30000
      }
    ]
  };

  const refundCalculation = {
    totalTaxLiability: displayResults.newRegime?.totalTax || 0,
    tdsDeducted: displayResults.newRegime?.taxPaid || 0,
    refundAmount: Math.max(0, (displayResults.newRegime?.taxPaid || 0) - (displayResults.newRegime?.totalTax || 0)),
    additionalTaxDue: Math.max(0, (displayResults.newRegime?.totalTax || 0) - (displayResults.newRegime?.taxPaid || 0)),
    isRefundDue: (displayResults.newRegime?.taxPaid || 0) > (displayResults.newRegime?.totalTax || 0)
  };

  const handleGenerateITR = () => {
    // We always have form16Data now (either uploaded or generated from manual entry)
    navigate('/itr-generation');
  };

  return <TaxResults 
    taxCalculation={taxCalculation} 
    refundCalculation={refundCalculation}
    onGenerateITR={handleGenerateITR}
    showITRButton={true}
  />;
};

const ITRGenerationPageWrapper: React.FC<{ form16Data: Form16DataDto | null }> = ({ form16Data }) => {
  const navigate = useNavigate();

  if (!form16Data) {
    return (
      <Box sx={{ textAlign: 'center', mt: 4 }}>
        <Typography variant="h5" color="error" gutterBottom>
          Tax Data Required
        </Typography>
        <Typography variant="body1" sx={{ mb: 3 }}>
          Please calculate your taxes first to generate ITR.
        </Typography>
        <Button 
          variant="outlined" 
          onClick={() => navigate('/calculate')}
          sx={{ mt: 2 }}
        >
          Calculate Taxes
        </Button>
      </Box>
    );
  }

  const handleBack = () => {
    navigate('/results');
  };

  return <ITRGeneration form16Data={form16Data} onBack={handleBack} />;
};

export default App;
