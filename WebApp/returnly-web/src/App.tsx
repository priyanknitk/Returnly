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
import { API_ENDPOINTS } from './config/api';

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
      main: '#667eea',
      light: '#9198ee',
      dark: '#4c63d2',
    },
    secondary: {
      main: '#764ba2',
      light: '#9575cd',
      dark: '#512da8',
    },
    success: {
      main: '#4caf50',
      50: 'rgba(76, 175, 80, 0.05)',
    },
    info: {
      main: '#2196f3',
      50: 'rgba(33, 150, 243, 0.05)',
    },
    warning: {
      main: '#ff9800',
      50: 'rgba(255, 152, 0, 0.05)',
    },
    error: {
      main: '#f44336',
      50: 'rgba(244, 67, 54, 0.05)',
    },
    grey: {
      50: '#fafafa',
      100: '#f5f5f5',
    },
    background: {
      default: '#fafafa',
      paper: '#ffffff',
    },
  },
  typography: {
    fontFamily: '"Inter", "Roboto", "Helvetica", "Arial", sans-serif',
    h4: {
      fontWeight: 700,
      letterSpacing: '-0.5px',
    },
    h5: {
      fontWeight: 600,
      letterSpacing: '-0.25px',
    },
    h6: {
      fontWeight: 600,
      letterSpacing: '-0.125px',
    },
    body1: {
      lineHeight: 1.6,
    },
    button: {
      textTransform: 'none',
      fontWeight: 600,
    },
  },
  shape: {
    borderRadius: 12,
  },
  components: {
    MuiButton: {
      styleOverrides: {
        root: {
          borderRadius: 12,
          padding: '10px 24px',
          fontSize: '0.95rem',
          fontWeight: 600,
        },
      },
    },
    MuiCard: {
      styleOverrides: {
        root: {
          borderRadius: 16,
          boxShadow: '0 4px 20px rgba(0,0,0,0.08)',
        },
      },
    },
    MuiTextField: {
      styleOverrides: {
        root: {
          '& .MuiOutlinedInput-root': {
            borderRadius: 12,
          },
        },
      },
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

    const handleCalculate = async (data: TaxData) => {
    // Convert manual TaxData to Form16DataDto format for ITR generation
    const convertedForm16Data: Form16DataDto = {
      employeeName: data.employeeName || 'Manual Entry User',
      pan: data.pan || '',
      assessmentYear: data.assessmentYear || '2024-25',
      financialYear: data.financialYear || '2023-24',
      employerName: data.employerName || 'Self Employed',
      tan: data.tan || '',
      grossSalary: data.salarySection17 + data.perquisites + data.profitsInLieu,
      totalTaxDeducted: data.totalTaxDeducted,
      standardDeduction: data.standardDeduction,
      professionalTax: data.professionalTax,
      form16B: {
        salarySection17: data.salarySection17,
        perquisites: data.perquisites,
        profitsInLieu: data.profitsInLieu,
        basicSalary: data.salarySection17 * 0.5,
        hra: data.salarySection17 * 0.3,
        specialAllowance: data.salarySection17 * 0.2,
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
        taxableIncome: 0 // Will be calculated by API
      },
      annexure: {
        q1TDS: data.totalTaxDeducted * 0.25,
        q2TDS: data.totalTaxDeducted * 0.25,
        q3TDS: data.totalTaxDeducted * 0.25,
        q4TDS: data.totalTaxDeducted * 0.25
      }
    };

    // Store the converted data for ITR generation
    setGeneratedForm16Data(convertedForm16Data);
    
    // Calculate taxes using the backend API
    try {
      const totalIncome = data.salarySection17 + data.perquisites + data.profitsInLieu + 
                         data.interestOnSavings + data.interestOnFixedDeposits + data.dividendIncome;
      
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
      
      const results = {
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
          apiResponse: apiResult // Store the full API response for the results page
        },
        form16Data: convertedForm16Data
      };
      
      onCalculate(results);
    } catch (error) {
      console.error('Error calling tax calculation API:', error);
      
      // Fallback to basic calculation if API fails
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
        },
        form16Data: convertedForm16Data
      };
      
      onCalculate(fallbackResults);
    }
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
    surchargeRate: displayResults.newRegime?.surcharge > 0 ? 10 : 0, // Default to 10% if surcharge exists
    healthAndEducationCess: displayResults.newRegime?.cess || 0,
    totalTaxWithCess: displayResults.newRegime?.totalTax || 0,
    effectiveTaxRate: displayResults.newRegime?.taxableIncome > 0 
      ? ((displayResults.newRegime?.totalTax || 0) / displayResults.newRegime?.taxableIncome) * 100 
      : 0,
    taxBreakdown: displayResults.newRegime?.slabCalculations || [
      {
        slabDescription: 'Calculation not available',
        incomeInSlab: displayResults.newRegime?.taxableIncome || 0,
        taxRate: 0,
        taxAmount: 0
      }
    ]
  };

  // Use API response if available, otherwise use the converted values
  if (displayResults.newRegime?.apiResponse) {
    const apiData = displayResults.newRegime.apiResponse;
    taxCalculation.totalTax = apiData.taxCalculation.totalTax;
    taxCalculation.surcharge = apiData.taxCalculation.surcharge;
    taxCalculation.surchargeRate = apiData.taxCalculation.surchargeRate;
    taxCalculation.healthAndEducationCess = apiData.taxCalculation.healthAndEducationCess;
    taxCalculation.totalTaxWithCess = apiData.taxCalculation.totalTaxWithCess;
    taxCalculation.effectiveTaxRate = apiData.taxCalculation.effectiveTaxRate;
    taxCalculation.taxBreakdown = apiData.taxCalculation.taxBreakdown.map((slab: any) => ({
      slabDescription: slab.slabDescription,
      incomeInSlab: slab.incomeInSlab,
      taxRate: slab.taxRate,
      taxAmount: slab.taxAmount
    }));
  }

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
