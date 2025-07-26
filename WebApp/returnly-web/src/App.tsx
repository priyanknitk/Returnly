import React, { useState } from 'react';
import { BrowserRouter as Router, Routes, Route, useNavigate } from 'react-router-dom';
import { ThemeProvider, createTheme } from '@mui/material/styles';
import CssBaseline from '@mui/material/CssBaseline';
import { Container, AppBar, Toolbar, Typography, Box } from '@mui/material';
import LandingPageSimple from './components/LandingPageSimple';
import Form16Upload from './components/Form16Upload';
import TaxDataInput from './components/TaxDataInput';
import TaxResults from './components/TaxResults';

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
  const [form16Data, setForm16Data] = useState<any>(null);
  const [taxResults, setTaxResults] = useState<any>(null);

  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <Router>
        <AppBar position="sticky" elevation={2}>
          <Toolbar>
            <Typography variant="h6" component="div" sx={{ flexGrow: 1 }}>
              Returnly - Indian Tax Calculator
            </Typography>
          </Toolbar>
        </AppBar>
        
        <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
          <Routes>
            <Route path="/" element={<LandingPageSimple />} />
            <Route path="/upload" element={<Form16Upload onUploadSuccess={setForm16Data} />} />
            <Route path="/calculate" element={<TaxCalculationPageWrapper form16Data={form16Data} onCalculate={setTaxResults} />} />
            <Route path="/results" element={<TaxResultsPageWrapper results={taxResults} />} />
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
              Returnly © {new Date().getFullYear()} - Indian Tax Calculation Made Easy
            </Typography>
          </Container>
        </Box>
      </Router>
    </ThemeProvider>
  );
}

const TaxCalculationPageWrapper: React.FC<{ form16Data: any; onCalculate: (results: any) => void }> = ({ form16Data, onCalculate }) => {
  const navigate = useNavigate();

  const handleCalculate = (data: TaxData) => {
    // For now, simulate calculation results using the correct field names
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
      }
    };
    
    onCalculate(mockResults);
    navigate('/results');
  };

  return <TaxDataInput onCalculate={handleCalculate} initialData={form16Data} />;
};

const TaxResultsPageWrapper: React.FC<{ results: any }> = ({ results }) => {
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

  return <TaxResults taxCalculation={taxCalculation} refundCalculation={refundCalculation} />;
};

export default App;
