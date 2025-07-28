import React, { useState } from 'react';
import { BrowserRouter as Router, Routes, Route, useNavigate, useLocation, Navigate } from 'react-router-dom';
import { ThemeProvider, createTheme } from '@mui/material/styles';
import CssBaseline from '@mui/material/CssBaseline';
import { Container, AppBar, Toolbar, Typography, Box, Button, Stack, Chip } from '@mui/material';
import { Assessment, Calculate, Upload, Home, TrendingUp } from '@mui/icons-material';
import LandingPageSimple from './components/LandingPageSimple';
import Form16Upload from './components/Form16Upload';
import TaxDataInput from './components/TaxDataInput';
import TaxResults from './components/TaxResults';
import ITRGeneration from './components/ITRGeneration';
import TaxFilingWizard from './components/TaxFilingWizard';
import { Form16DataDto } from './types/api';
import { API_ENDPOINTS } from './config/api';
import { DEFAULT_PERSONAL_INFO } from './constants/defaultValues';

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

// Redirect component for old routes
const RedirectToFileReturns: React.FC = () => {
  return <Navigate to="/file-returns" replace />;
};

// Elegant Navigation Component
const ModernNavigation: React.FC<{ 
  taxResults: any; 
  form16Data: Form16DataDto | null; 
}> = ({ taxResults, form16Data }) => {
  const navigate = useNavigate();
  const location = useLocation();

  // Define navigation items based on current state and location
  const getAvailableNavItems = () => {
    const baseItems = [
      { path: '/', label: 'Home', icon: Home }
    ];

    // Always show File Returns as the main entry point
    if (location.pathname !== '/file-returns') {
      baseItems.push({ path: '/file-returns', label: 'File Returns', icon: Assessment });
    }

    // Only show Results if user has completed calculations
    if (taxResults && location.pathname !== '/results') {
      baseItems.push({ path: '/results', label: 'Results', icon: TrendingUp });
    }

    return baseItems;
  };

  const navItems = getAvailableNavItems();

  return (
    <AppBar 
      position="sticky" 
      elevation={0}
      sx={{
        background: 'rgba(255, 255, 255, 0.98)',
        backdropFilter: 'blur(20px)',
        borderBottom: '1px solid rgba(0, 0, 0, 0.08)',
        boxShadow: 'none'
      }}
    >
      <Container maxWidth="xl">
        <Toolbar sx={{ 
          py: 2, 
          justifyContent: 'space-between',
          minHeight: 72
        }}>
          {/* Elegant Logo */}
          <Box 
            sx={{
              display: 'flex',
              alignItems: 'center',
              cursor: 'pointer',
              '&:hover': {
                '& .logo-icon': {
                  transform: 'rotate(5deg) scale(1.05)'
                }
              }
            }} 
            onClick={() => navigate('/')}
          >
            <Box 
              className="logo-icon"
              sx={{
                width: 44,
                height: 44,
                borderRadius: 2,
                background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                mr: 2.5,
                transition: 'all 0.3s cubic-bezier(0.4, 0, 0.2, 1)'
              }}
            >
              <Assessment sx={{ fontSize: 22, color: 'white' }} />
            </Box>
            <Box>
              <Typography sx={{ 
                fontSize: '1.5rem',
                fontWeight: 700,
                color: '#1a1a1a',
                lineHeight: 1,
                letterSpacing: '-0.02em'
              }}>
                Returnly
              </Typography>
              <Typography sx={{ 
                fontSize: '0.75rem',
                color: 'text.secondary',
                fontWeight: 500,
                lineHeight: 1,
                mt: 0.25,
                letterSpacing: '0.02em'
              }}>
                TAX SOLUTIONS
              </Typography>
            </Box>
          </Box>

          {/* Elegant Navigation */}
          <Stack 
            direction="row" 
            spacing={0.5} 
            sx={{ 
              display: { xs: 'none', md: 'flex' },
              background: 'rgba(0, 0, 0, 0.04)',
              borderRadius: 2,
              p: 0.5
            }}
          >
            {navItems.map((item) => {
              const isActive = location.pathname === item.path;
              const IconComponent = item.icon;
              
              return (
                <Button
                  key={item.path}
                  onClick={() => navigate(item.path)}
                  startIcon={<IconComponent sx={{ fontSize: 18 }} />}
                  sx={{
                    px: 3,
                    py: 1.25,
                    borderRadius: 1.5,
                    textTransform: 'none',
                    fontWeight: isActive ? 600 : 500,
                    fontSize: '0.875rem',
                    color: isActive ? 'white' : '#6b7280',
                    background: isActive 
                      ? 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)'
                      : 'transparent',
                    boxShadow: isActive ? '0 2px 8px rgba(102, 126, 234, 0.3)' : 'none',
                    minWidth: 'auto',
                    '&:hover': {
                      background: isActive 
                        ? 'linear-gradient(135deg, #5a6fd8 0%, #6a4190 100%)'
                        : 'rgba(255, 255, 255, 0.8)',
                      color: isActive ? 'white' : '#374151',
                      transform: 'translateY(-0.5px)'
                    },
                    transition: 'all 0.2s cubic-bezier(0.4, 0, 0.2, 1)'
                  }}
                >
                  {item.label}
                </Button>
              );
            })}
          </Stack>

          {/* Workflow Progress Buttons */}
          {location.pathname !== '/' && (
            <Stack 
              direction="row" 
              spacing={0.5} 
              sx={{ 
                display: { xs: 'none', lg: 'flex' },
                background: 'rgba(0, 0, 0, 0.04)',
                borderRadius: 2,
                p: 0.5
              }}
            >
              {/* Filing Step */}
              <Button
                onClick={() => navigate('/file-returns')}
                disabled={false} // Always accessible
                sx={{
                  minWidth: 'auto',
                  px: 2,
                  py: 1,
                  borderRadius: 1.5,
                  textTransform: 'none',
                  fontSize: '0.75rem',
                  fontWeight: 600,
                  color: location.pathname === '/file-returns' ? 'white' : '#6b7280',
                  background: location.pathname === '/file-returns'
                    ? 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)'
                    : 'transparent',
                  boxShadow: location.pathname === '/file-returns' ? '0 2px 8px rgba(102, 126, 234, 0.3)' : 'none',
                  '&:hover': {
                    background: location.pathname === '/file-returns'
                      ? 'linear-gradient(135deg, #5a6fd8 0%, #6a4190 100%)'
                      : 'rgba(255, 255, 255, 0.6)',
                    transform: 'translateY(-1px)'
                  },
                  '&:disabled': {
                    color: '#9ca3af',
                    background: 'transparent'
                  },
                  transition: 'all 0.2s cubic-bezier(0.4, 0, 0.2, 1)'
                }}
              >
                Filing
              </Button>
              
              {/* Results Step */}
              <Button
                onClick={() => taxResults && navigate('/results')}
                disabled={!taxResults} // Only accessible after calculations
                sx={{
                  minWidth: 'auto',
                  px: 2,
                  py: 1,
                  borderRadius: 1.5,
                  textTransform: 'none',
                  fontSize: '0.75rem',
                  fontWeight: 600,
                  color: (taxResults && location.pathname === '/results') ? 'white' : 
                         taxResults ? '#6b7280' : '#9ca3af',
                  background: (taxResults && location.pathname === '/results')
                    ? 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)'
                    : 'transparent',
                  boxShadow: (taxResults && location.pathname === '/results') ? '0 2px 8px rgba(102, 126, 234, 0.3)' : 'none',
                  '&:hover': {
                    background: (taxResults && location.pathname === '/results')
                      ? 'linear-gradient(135deg, #5a6fd8 0%, #6a4190 100%)'
                      : taxResults ? 'rgba(255, 255, 255, 0.6)' : 'transparent',
                    transform: taxResults ? 'translateY(-1px)' : 'none'
                  },
                  '&:disabled': {
                    color: '#9ca3af',
                    background: 'transparent',
                    cursor: 'not-allowed'
                  },
                  transition: 'all 0.2s cubic-bezier(0.4, 0, 0.2, 1)'
                }}
              >
                Results
              </Button>
              
              {/* ITR Step */}
              <Button
                onClick={() => (taxResults && form16Data) && navigate('/itr-generation')}
                disabled={!taxResults || !form16Data} // Only accessible after results
                sx={{
                  minWidth: 'auto',
                  px: 2,
                  py: 1,
                  borderRadius: 1.5,
                  textTransform: 'none',
                  fontSize: '0.75rem',
                  fontWeight: 600,
                  color: location.pathname === '/itr-generation' ? 'white' : 
                         (taxResults && form16Data) ? '#6b7280' : '#9ca3af',
                  background: location.pathname === '/itr-generation'
                    ? 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)'
                    : 'transparent',
                  boxShadow: location.pathname === '/itr-generation' ? '0 2px 8px rgba(102, 126, 234, 0.3)' : 'none',
                  '&:hover': {
                    background: location.pathname === '/itr-generation'
                      ? 'linear-gradient(135deg, #5a6fd8 0%, #6a4190 100%)'
                      : (taxResults && form16Data) ? 'rgba(255, 255, 255, 0.6)' : 'transparent',
                    transform: (taxResults && form16Data) ? 'translateY(-1px)' : 'none'
                  },
                  '&:disabled': {
                    color: '#9ca3af',
                    background: 'transparent',
                    cursor: 'not-allowed'
                  },
                  transition: 'all 0.2s cubic-bezier(0.4, 0, 0.2, 1)'
                }}
              >
                ITR
              </Button>
            </Stack>
          )}

          {/* Elegant Status */}
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <Typography
              variant="caption"
              sx={{
                background: 'linear-gradient(135deg, #10b981 0%, #059669 100%)',
                backgroundClip: 'text',
                WebkitBackgroundClip: 'text',
                color: 'transparent',
                fontWeight: 700,
                fontSize: '0.75rem',
                letterSpacing: '0.1em',
                display: { xs: 'none', sm: 'block' }
              }}
            >
              BETA
            </Typography>
            <Box
              sx={{
                width: 8,
                height: 8,
                borderRadius: '50%',
                background: 'linear-gradient(135deg, #10b981 0%, #059669 100%)',
                animation: 'pulse 2s infinite',
                '@keyframes pulse': {
                  '0%, 100%': { opacity: 1 },
                  '50%': { opacity: 0.5 }
                }
              }}
            />
          </Box>
        </Toolbar>
      </Container>
    </AppBar>
  );
};

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
        <ModernNavigation taxResults={taxResults} form16Data={currentForm16Data || form16Data} />
        
        <Container maxWidth="xl" sx={{ mt: 4, mb: 4, px: { xs: 2, sm: 3, md: 4 } }}>
          <Routes>
            <Route path="/" element={<LandingPageSimple />} />
            <Route path="/file-returns" element={<TaxFilingWizard onComplete={handleTaxCalculation} />} />
            {/* Redirect direct access to upload/calculate to the proper flow */}
            <Route path="/upload" element={<RedirectToFileReturns />} />
            <Route path="/calculate" element={<RedirectToFileReturns />} />
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
      // Include business income fields
      intradayTradingIncome: data.intradayTradingIncome || 0,
      tradingBusinessExpenses: data.tradingBusinessExpenses || 0,
      otherBusinessIncome: data.otherBusinessIncome || 0,
      businessExpenses: data.businessExpenses || 0,
      // Include capital gains fields
      stocksSTCG: data.stocksSTCG || 0,
      stocksLTCG: data.stocksLTCG || 0,
      mutualFundsSTCG: data.mutualFundsSTCG || 0,
      mutualFundsLTCG: data.mutualFundsLTCG || 0,
      fnoGains: data.fnoGains || 0,
      realEstateSTCG: data.realEstateSTCG || 0,
      realEstateLTCG: data.realEstateLTCG || 0,
      bondsSTCG: data.bondsSTCG || 0,
      bondsLTCG: data.bondsLTCG || 0,
      goldSTCG: data.goldSTCG || 0,
      goldLTCG: data.goldLTCG || 0,
      cryptoGains: data.cryptoGains || 0,
      usStocksSTCG: data.usStocksSTCG || 0,
      usStocksLTCG: data.usStocksLTCG || 0,
      otherForeignAssetsGains: data.otherForeignAssetsGains || 0,
      rsuGains: data.rsuGains || 0,
      esopGains: data.esopGains || 0,
      esspGains: data.esspGains || 0,
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
      
      console.log('TaxCalculation - Results being passed to onCalculate:', {
        form16Data: results.form16Data,
        businessIncomeFields: {
          intradayTradingIncome: results.form16Data.intradayTradingIncome,
          otherBusinessIncome: results.form16Data.otherBusinessIncome
        }
      });
      
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

  return <ITRGeneration form16Data={form16Data} personalInfo={DEFAULT_PERSONAL_INFO} onBack={handleBack} />;
};

export default App;
