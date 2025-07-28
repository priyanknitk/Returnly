import React, { useState } from 'react';
import {
  Box,
  TextField,
  Typography,
  Button,
  FormControlLabel,
  Switch,
  Card,
  CardContent,
  Accordion,
  AccordionSummary,
  AccordionDetails,
  IconButton,
  Alert,
  Stack,
  Fade,
  Grow
} from '@mui/material';
import {
  ExpandMore,
  Add,
  Delete,
  Person,
  Home,
  TrendingUp,
  Public,
  Assignment,
  AccountBalance,
  ContactPhone,
  Email
} from '@mui/icons-material';
import { AdditionalTaxpayerInfoDto, HousePropertyDetailsDto, CapitalGainDetailsDto, ForeignAssetDetailsDto, BusinessIncomeDetailsDto, BusinessExpenseDetailsDto, Form16DataDto, Gender, MaritalStatus } from '../types/api';

interface AdditionalInfoFormProps {
  onSubmit: (info: AdditionalTaxpayerInfoDto) => void;
  loading: boolean;
  form16Data?: Form16DataDto | null;
}

const AdditionalInfoForm: React.FC<AdditionalInfoFormProps> = ({ onSubmit, loading, form16Data }) => {
  
  // Helper functions to check if data is already available in form16Data
  const hasCapitalGainsInForm16 = () => {
    if (!form16Data) return false;
    const totalCapitalGains = (form16Data.stocksSTCG || 0) + (form16Data.stocksLTCG || 0) + 
                             (form16Data.mutualFundsSTCG || 0) + (form16Data.mutualFundsLTCG || 0) + 
                             (form16Data.fnoGains || 0) + (form16Data.realEstateSTCG || 0) + 
                             (form16Data.realEstateLTCG || 0) + (form16Data.bondsSTCG || 0) + 
                             (form16Data.bondsLTCG || 0) + (form16Data.goldSTCG || 0) + 
                             (form16Data.goldLTCG || 0) + (form16Data.cryptoGains || 0) + 
                             (form16Data.usStocksSTCG || 0) + (form16Data.usStocksLTCG || 0) + 
                             (form16Data.otherForeignAssetsGains || 0) + (form16Data.rsuGains || 0) + 
                             (form16Data.esopGains || 0) + (form16Data.esspGains || 0);
    return totalCapitalGains > 0;
  };

  const hasForeignAssetsInForm16 = () => {
    if (!form16Data) return false;
    const totalForeignAssets = (form16Data.usStocksSTCG || 0) + (form16Data.usStocksLTCG || 0) + 
                               (form16Data.otherForeignAssetsGains || 0);
    return totalForeignAssets > 0;
  };

  const hasBusinessIncomeInForm16 = () => {
    if (!form16Data) return false;
    return ((form16Data.intradayTradingIncome || 0) > 0) || ((form16Data.otherBusinessIncome || 0) > 0);
  };

  const [formData, setFormData] = useState<AdditionalTaxpayerInfoDto>({
    dateOfBirth: '1990-01-01', // Default date instead of empty string
    fatherName: '',
    gender: Gender.Male,
    maritalStatus: MaritalStatus.Single,
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

  const [errors, setErrors] = useState<Record<string, string>>({});

  const validateForm = (): boolean => {
    const newErrors: Record<string, string> = {};

    // Only validate property-specific information since personal details 
    // and bank details are now handled in PersonalDetailsForm

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (validateForm()) {
      onSubmit(formData);
    }
  };

  const updateFormData = (field: keyof AdditionalTaxpayerInfoDto, value: any) => {
    setFormData(prev => ({ ...prev, [field]: value }));
    if (errors[field]) {
      setErrors(prev => ({ ...prev, [field]: '' }));
    }
  };

  const addHouseProperty = () => {
    const newProperty: HousePropertyDetailsDto = {
      propertyAddress: '',
      annualValue: 0,
      propertyTax: 0,
      interestOnLoan: 0
    };
    updateFormData('houseProperties', [...formData.houseProperties, newProperty]);
  };

  const updateHouseProperty = (index: number, field: keyof HousePropertyDetailsDto, value: any) => {
    const updated = [...formData.houseProperties];
    updated[index] = { ...updated[index], [field]: value };
    updateFormData('houseProperties', updated);
  };

  const removeHouseProperty = (index: number) => {
    const updated = formData.houseProperties.filter((_, i) => i !== index);
    updateFormData('houseProperties', updated);
  };

  // Modern Accordion Styling Function
  const getAccordionStyles = (color: string, colorSecondary: string) => ({
    mb: 3,
    borderRadius: 3,
    background: 'rgba(255, 255, 255, 0.9)',
    backdropFilter: 'blur(20px)',
    border: '1px solid rgba(255, 255, 255, 0.2)',
    boxShadow: '0 8px 32px rgba(0, 0, 0, 0.1)',
    overflow: 'hidden',
    '&:before': { display: 'none' },
    '&.Mui-expanded': {
      boxShadow: '0 12px 40px rgba(0, 0, 0, 0.15)',
      transform: 'translateY(-2px)',
      transition: 'all 0.3s cubic-bezier(0.4, 0, 0.2, 1)'
    }
  });

  const getAccordionSummaryStyles = (color: string, colorSecondary: string) => ({
    background: `linear-gradient(135deg, ${color}19 0%, ${colorSecondary}19 100%)`,
    borderBottom: `1px solid ${color}19`,
    py: 2,
    minHeight: 72,
    '&:hover': {
      background: `linear-gradient(135deg, ${color}26 0%, ${colorSecondary}26 100%)`,
    },
    '& .MuiAccordionSummary-expandIconWrapper.Mui-expanded': {
      transform: 'rotate(180deg)',
    }
  });

  const getIconBoxStyles = (color: string, colorSecondary: string) => ({
    p: 1.5,
    borderRadius: 2,
    background: `linear-gradient(135deg, ${color} 0%, ${colorSecondary} 100%)`,
    color: 'white',
    mr: 2,
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'center',
    boxShadow: `0 4px 12px ${color}4d`
  });

  const getTitleStyles = (color: string, colorSecondary: string) => ({
    fontWeight: 600,
    background: `linear-gradient(135deg, ${color} 0%, ${colorSecondary} 100%)`,
    backgroundClip: 'text',
    WebkitBackgroundClip: 'text',
    WebkitTextFillColor: 'transparent',
    mb: 0.5
  });

  return (
    <Grow in timeout={300}>
      <Box>
        {/* Header */}
        <Card sx={{ 
          mb: 3,
          background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
          color: 'white',
          borderRadius: 3
        }}>
          <CardContent sx={{ textAlign: 'center', py: 2.5 }}>
            <Assignment sx={{ fontSize: 32, mb: 1.5, opacity: 0.9 }} />
            <Typography variant="h5" gutterBottom sx={{ fontWeight: 700 }}>
              Property & Investment Details
            </Typography>
            <Typography variant="body1" sx={{ opacity: 0.9, maxWidth: 600, mx: 'auto' }}>
              Complete your tax profile with property and investment information for accurate ITR generation
            </Typography>
          </CardContent>
        </Card>

        <form onSubmit={handleSubmit}>
          
          {/* Auto-detected Information Summary */}
          {(hasCapitalGainsInForm16() || hasForeignAssetsInForm16() || hasBusinessIncomeInForm16()) && (
            <Alert 
              severity="info" 
              sx={{ 
                mb: 3,
                borderRadius: 2,
                border: '1px solid rgba(2, 136, 209, 0.2)',
                backgroundColor: 'info.50'
              }}
            >
              <Typography variant="subtitle2" gutterBottom>
                📋 Auto-detected from your tax input:
              </Typography>
              <Box component="ul" sx={{ m: 0, pl: 2 }}>
                {hasCapitalGainsInForm16() && (
                  <Typography component="li" variant="body2">
                    ✅ Capital Gains from your investment transactions
                  </Typography>
                )}
                {hasForeignAssetsInForm16() && (
                  <Typography component="li" variant="body2">
                    ✅ Foreign Assets (US Stocks and other foreign investments)
                  </Typography>
                )}
                {hasBusinessIncomeInForm16() && (
                  <Typography component="li" variant="body2">
                    ✅ Business Income from trading and other business activities
                  </Typography>
                )}
              </Box>
              <Typography variant="body2" sx={{ mt: 1, fontStyle: 'italic' }}>
                These sections are hidden since the information is already available.
              </Typography>
            </Alert>
          )}

          {/* House Property */}
          <Grow in timeout={800}>
            <Accordion sx={getAccordionStyles('#f59e0b', '#d97706')}>
              <AccordionSummary 
                expandIcon={
                  <ExpandMore sx={{ 
                    color: '#f59e0b',
                    fontSize: 28,
                    transition: 'transform 0.3s ease'
                  }} />
                }
                sx={getAccordionSummaryStyles('#f59e0b', '#d97706')}
              >
                <Box sx={{ display: 'flex', alignItems: 'center' }}>
                  <Box sx={getIconBoxStyles('#f59e0b', '#d97706')}>
                    <Home sx={{ fontSize: 24 }} />
                  </Box>
                  <Box>
                    <Typography variant="h6" sx={getTitleStyles('#f59e0b', '#d97706')}>
                      House Property Income
                    </Typography>
                    <Typography variant="body2" sx={{ 
                      color: 'text.secondary',
                      fontSize: '0.85rem'
                    }}>
                      Rental income and property details
                    </Typography>
                  </Box>
                </Box>
              </AccordionSummary>
              <AccordionDetails sx={{ 
                p: 3,
                background: 'rgba(255, 255, 255, 0.7)'
              }}>
                <FormControlLabel
                  control={
                    <Switch
                      checked={formData.hasHouseProperty}
                      onChange={(e) => updateFormData('hasHouseProperty', e.target.checked)}
                    />
                  }
                  label="I have house property income"
                  sx={{ mb: 2 }}
                />
                
                {formData.hasHouseProperty && (
                  <Box>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
                      <Typography variant="subtitle2">Property Details</Typography>
                      <Button startIcon={<Add />} onClick={addHouseProperty}>
                        Add Property
                      </Button>
                    </Box>
                    
                    {formData.houseProperties.map((property, index) => (
                      <Card key={index} variant="outlined" sx={{ mb: 2 }}>
                        <CardContent>
                          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
                            <Typography variant="subtitle2">Property {index + 1}</Typography>
                            <IconButton onClick={() => removeHouseProperty(index)} color="error">
                              <Delete />
                            </IconButton>
                          </Box>
                            <Stack spacing={2}>
                              <TextField
                                label="Property Address"
                                value={property.propertyAddress}
                                onChange={(e) => updateHouseProperty(index, 'propertyAddress', e.target.value)}
                                fullWidth
                              />
                              <Box sx={{ display: 'flex', gap: 2 }}>
                                <TextField
                                  label="Annual Value"
                                  type="number"
                                  value={property.annualValue}
                                  onChange={(e) => updateHouseProperty(index, 'annualValue', Number(e.target.value))}
                                  sx={{ flex: 1 }}
                                />
                                <TextField
                                  label="Property Tax"
                                  type="number"
                                  value={property.propertyTax}
                                  onChange={(e) => updateHouseProperty(index, 'propertyTax', Number(e.target.value))}
                                  sx={{ flex: 1 }}
                                />
                                <TextField
                                  label="Interest on Home Loan"
                                  type="number"
                                  value={property.interestOnLoan}
                                  onChange={(e) => updateHouseProperty(index, 'interestOnLoan', Number(e.target.value))}
                                  sx={{ flex: 1 }}
                                />
                              </Box>
                            </Stack>
                        </CardContent>
                      </Card>
                    ))}
                  </Box>
                )}
              </AccordionDetails>
            </Accordion>
          </Grow>

          {Object.keys(errors).length > 0 && (
            <Alert 
              severity="error" 
              sx={{ 
                mt: 2,
                borderRadius: 2,
                border: '1px solid rgba(211, 47, 47, 0.2)',
                backgroundColor: 'error.50'
              }}
            >
              Please fix the errors above before proceeding.
            </Alert>
          )}

          <Box sx={{ mt: 4, display: 'flex', gap: 2 }}>
            <Button
              type="submit"
              variant="contained"
              disabled={loading}
              fullWidth
              sx={{ 
                py: 1.5,
                fontWeight: 600,
                background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
                '&:hover': {
                  background: 'linear-gradient(135deg, #5a6fd8 0%, #6a4190 100%)',
                }
              }}
            >
              {loading ? 'Processing...' : 'Get ITR Recommendation'}
            </Button>
          </Box>
        </form>
      </Box>
    </Grow>
  );
};

export default AdditionalInfoForm;
