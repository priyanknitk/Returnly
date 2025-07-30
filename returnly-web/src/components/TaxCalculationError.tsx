import React from 'react';
import {
  Alert,
  AlertTitle,
  Box,
  Button,
  Card,
  CardContent,
  Typography,
  Stack
} from '@mui/material';
import {
  Error as ErrorIcon,
  Refresh,
  ArrowBack,
  Warning
} from '@mui/icons-material';
import { TaxResultsError } from '../utils/taxResultsMapper';

interface TaxCalculationErrorProps {
  error: TaxResultsError;
  onRetry?: () => void;
  onBack?: () => void;
}

const TaxCalculationError: React.FC<TaxCalculationErrorProps> = ({
  error,
  onRetry,
  onBack
}) => {
  const getErrorSeverity = () => {
    switch (error.errorType) {
      case 'NO_DATA':
        return 'warning';
      case 'API_ERROR':
        return 'error';
      case 'CALCULATION_ERROR':
        return 'error';
      default:
        return 'error';
    }
  };

  const getErrorIcon = () => {
    switch (error.errorType) {
      case 'NO_DATA':
        return <Warning sx={{ fontSize: 48, color: 'warning.main' }} />;
      case 'API_ERROR':
      case 'CALCULATION_ERROR':
        return <ErrorIcon sx={{ fontSize: 48, color: 'error.main' }} />;
      default:
        return <ErrorIcon sx={{ fontSize: 48, color: 'error.main' }} />;
    }
  };

  const getErrorTitle = () => {
    switch (error.errorType) {
      case 'NO_DATA':
        return 'Tax Calculation Required';
      case 'API_ERROR':
        return 'Service Error';
      case 'CALCULATION_ERROR':
        return 'Calculation Error';
      default:
        return 'Error';
    }
  };

  const getRecommendedAction = () => {
    switch (error.errorType) {
      case 'NO_DATA':
        return 'Please complete the tax calculation before viewing results.';
      case 'API_ERROR':
        return 'There was an issue with the tax calculation service. Please try again.';
      case 'CALCULATION_ERROR':
        return 'There was an error processing your tax data. Please verify your information and try again.';
      default:
        return 'Please try again or contact support if the problem persists.';
    }
  };

  return (
    <Box sx={{ 
      maxWidth: 800, 
      mx: 'auto', 
      mt: 4, 
      px: { xs: 2, sm: 3 } 
    }}>
      <Card sx={{ 
        borderRadius: 3,
        boxShadow: '0 4px 20px rgba(0,0,0,0.08)',
        border: `2px solid ${error.errorType === 'NO_DATA' ? 'rgba(255, 152, 0, 0.3)' : 'rgba(244, 67, 54, 0.3)'}`
      }}>
        <CardContent sx={{ p: 4, textAlign: 'center' }}>
          {/* Error Icon */}
          <Box sx={{ mb: 3 }}>
            {getErrorIcon()}
          </Box>

          {/* Error Title */}
          <Typography variant="h5" sx={{ 
            fontWeight: 700, 
            mb: 2,
            color: error.errorType === 'NO_DATA' ? 'warning.main' : 'error.main'
          }}>
            {getErrorTitle()}
          </Typography>

          {/* Error Message */}
          <Typography variant="body1" sx={{ 
            mb: 3,
            color: 'text.primary',
            fontSize: '1.1rem',
            lineHeight: 1.6
          }}>
            {error.error}
          </Typography>

          {/* Error Details Alert */}
          {error.details && (
            <Alert 
              severity={getErrorSeverity()} 
              sx={{ 
                mb: 3,
                textAlign: 'left',
                borderRadius: 2
              }}
            >
              <AlertTitle>Details</AlertTitle>
              {error.details}
            </Alert>
          )}

          {/* Recommended Action */}
          <Typography variant="body2" sx={{ 
            mb: 4,
            color: 'text.secondary',
            fontSize: '0.95rem',
            lineHeight: 1.5
          }}>
            {getRecommendedAction()}
          </Typography>

          {/* Action Buttons */}
          <Stack 
            direction={{ xs: 'column', sm: 'row' }} 
            spacing={2} 
            justifyContent="center"
          >
            {onBack && (
              <Button
                variant="outlined"
                startIcon={<ArrowBack />}
                onClick={onBack}
                sx={{
                  px: 3,
                  py: 1.5,
                  borderRadius: 2,
                  textTransform: 'none',
                  fontWeight: 600
                }}
              >
                Go Back
              </Button>
            )}
            
            {onRetry && (
              <Button
                variant="contained"
                startIcon={<Refresh />}
                onClick={onRetry}
                sx={{
                  px: 3,
                  py: 1.5,
                  borderRadius: 2,
                  textTransform: 'none',
                  fontWeight: 600,
                  background: error.errorType === 'NO_DATA' 
                    ? 'linear-gradient(135deg, #ff9800 0%, #f57c00 100%)'
                    : 'linear-gradient(135deg, #f44336 0%, #d32f2f 100%)'
                }}
              >
                Try Again
              </Button>
            )}
          </Stack>
        </CardContent>
      </Card>

      {/* Additional Help */}
      <Alert 
        severity="info" 
        sx={{ 
          mt: 3,
          borderRadius: 2,
          backgroundColor: 'rgba(33, 150, 243, 0.05)'
        }}
      >
        <AlertTitle>Need Help?</AlertTitle>
        If you continue to experience issues, please check your internet connection and ensure all required fields are filled correctly. 
        For persistent problems, contact our support team.
      </Alert>
    </Box>
  );
};

export default TaxCalculationError;
