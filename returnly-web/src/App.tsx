import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { ThemeProvider, createTheme } from '@mui/material/styles';
import CssBaseline from '@mui/material/CssBaseline';
import { Container, Box, Typography, Button } from '@mui/material';
import LandingPageSimple from './components/LandingPageSimple';
import TaxResultsWrapper from './components/TaxResultsWrapper';
import ITRGeneration from './components/ITRGeneration';
import TaxFilingWizard from './components/TaxFilingWizard';
import { ModernNavigation } from './components/ModernNavigation';
import { createRouteWrapper } from './components/PageWrapper';
import { useAppState } from './hooks/useAppState';
import { useAppNavigation } from './hooks/useAppNavigation';
import { routeRedirects, routePaths } from './config/navigation';
import { Form16DataDto } from './types/api';
import { DEFAULT_PERSONAL_INFO } from './constants/defaultValues';
import { mapTaxResultsToComponents } from './utils/taxResultsMapper';

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
    h1: {
      fontWeight: 700,
      fontSize: '2rem',
      letterSpacing: '-0.5px',
    },
    h2: {
      fontWeight: 700,
      fontSize: '1.75rem',
      letterSpacing: '-0.5px',
    },
    h3: {
      fontWeight: 700,
      fontSize: '1.5rem',
      letterSpacing: '-0.5px',
    },
    h4: {
      fontWeight: 700,
      fontSize: '1.25rem',
      letterSpacing: '-0.5px',
    },
    h5: {
      fontWeight: 600,
      fontSize: '1.1rem',
      letterSpacing: '-0.25px',
    },
    h6: {
      fontWeight: 600,
      fontSize: '1rem',
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

// Redirect component for old routes - now generic
const RedirectComponent: React.FC<{ to: string }> = ({ to }) => {
  return <Navigate to={to} replace />;
};

// Generic route redirect factory
const createRedirectRoutes = () => {
  return Object.entries(routeRedirects).map(([from, to]) => (
    <Route key={from} path={from} element={<RedirectComponent to={to} />} />
  ));
};

// Generic Tax Results Page Component
const TaxResultsPage: React.FC<{ appState: any }> = ({ appState }) => {
  const { navigateToITRGeneration, navigateToFileReturns } = useAppNavigation();
  
  // Use the centralized mapping utility
  const mappedResults = mapTaxResultsToComponents(appState.taxResults);

  const handleGenerateITR = () => {
    navigateToITRGeneration();
  };

  const handleRetry = () => {
    navigateToFileReturns();
  };

  const handleBack = () => {
    navigateToFileReturns();
  };

  return (
    <TaxResultsWrapper
      results={mappedResults}
      onGenerateITR={handleGenerateITR}
      onRetry={handleRetry}
      onBack={handleBack}
      showITRButton={true}
    />
  );
};

// Generic ITR Generation Page Component
const ITRGenerationPage: React.FC<{ appState: any }> = ({ appState }) => {
  const { navigateToResults } = useAppNavigation();

  const handleBack = () => {
    navigateToResults();
  };

  return (
    <ITRGeneration 
      form16Data={appState.currentForm16Data || appState.form16Data} 
      personalInfo={DEFAULT_PERSONAL_INFO} 
      onBack={handleBack} 
    />
  );
};

// Generic File Returns Page Component
const FileReturnsPage: React.FC<{ onComplete: (results: any) => void }> = ({ onComplete }) => {
  return <TaxFilingWizard onComplete={onComplete} />;
};

// Create wrapped components with route requirements
const TaxResultsPageWrapper = createRouteWrapper(TaxResultsPage, routePaths.RESULTS, {
  fallbackRoute: routePaths.FILE_RETURNS,
  fallbackMessage: 'Please calculate your taxes first to view results.',
  dataMapper: (appState) => ({ appState })
});

const ITRGenerationPageWrapper = createRouteWrapper(ITRGenerationPage, routePaths.ITR_GENERATION, {
  fallbackRoute: routePaths.FILE_RETURNS,
  fallbackMessage: 'Please calculate your taxes first to generate ITR.',
  dataMapper: (appState) => ({ appState })
});

function App() {
  const { appState, handleTaxCalculation } = useAppState();

  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <Router>
        <ModernNavigation appState={appState} />
        
        <Container maxWidth="xl" sx={{ mt: 4, mb: 4, px: { xs: 2, sm: 3, md: 4 } }}>
          <Routes>
            <Route path={routePaths.HOME} element={<LandingPageSimple />} />
            <Route 
              path={routePaths.FILE_RETURNS} 
              element={<FileReturnsPage onComplete={handleTaxCalculation} />} 
            />
            <Route 
              path={routePaths.RESULTS} 
              element={<TaxResultsPageWrapper appState={appState} />} 
            />
            <Route 
              path={routePaths.ITR_GENERATION} 
              element={<ITRGenerationPageWrapper appState={appState} />} 
            />
            {/* Generic redirect routes */}
            {createRedirectRoutes()}
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

export default App;
