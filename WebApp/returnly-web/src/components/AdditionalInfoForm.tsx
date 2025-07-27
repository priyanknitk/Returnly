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
import { AdditionalTaxpayerInfoDto, HousePropertyDetailsDto, CapitalGainDetailsDto, ForeignAssetDetailsDto } from '../types/api';

interface AdditionalInfoFormProps {
  onSubmit: (info: AdditionalTaxpayerInfoDto) => void;
  loading: boolean;
}

const AdditionalInfoForm: React.FC<AdditionalInfoFormProps> = ({ onSubmit, loading }) => {
  const [formData, setFormData] = useState<AdditionalTaxpayerInfoDto>({
    dateOfBirth: '1990-01-01', // Default date instead of empty string
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
    hasOtherIncome: false,
    otherInterestIncome: 0,
    otherDividendIncome: 0,
    otherSourcesIncome: 0,
    hasForeignIncome: false,
    foreignIncome: 0,
    hasForeignAssets: false,
    foreignAssets: []
  });

  const [errors, setErrors] = useState<Record<string, string>>({});

  const validateForm = (): boolean => {
    const newErrors: Record<string, string> = {};

    if (!formData.dateOfBirth) {
      newErrors.dateOfBirth = 'Date of birth is required';
    } else {
      const dob = new Date(formData.dateOfBirth);
      const today = new Date();
      const age = today.getFullYear() - dob.getFullYear();
      if (age < 18 || age > 100) {
        newErrors.dateOfBirth = 'Please enter a valid date of birth (age 18-100)';
      }
    }
    
    if (!formData.address) newErrors.address = 'Address is required';
    if (!formData.city) newErrors.city = 'City is required';
    if (!formData.state) newErrors.state = 'State is required';
    if (!formData.pincode || !/^\d{6}$/.test(formData.pincode)) {
      newErrors.pincode = 'Valid 6-digit pincode is required';
    }
    if (!formData.emailAddress || !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(formData.emailAddress)) {
      newErrors.emailAddress = 'Valid email address is required';
    }
    if (!formData.mobileNumber || !/^\d{10}$/.test(formData.mobileNumber)) {
      newErrors.mobileNumber = 'Valid 10-digit mobile number is required';
    }
    if (!formData.bankAccountNumber) newErrors.bankAccountNumber = 'Bank account number is required';
    if (!formData.bankIFSCCode || !/^[A-Z]{4}0[A-Z0-9]{6}$/.test(formData.bankIFSCCode)) {
      newErrors.bankIFSCCode = 'Valid IFSC code is required';
    }
    if (!formData.bankName) newErrors.bankName = 'Bank name is required';

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (validateForm()) {
      // Convert date string to ISO format for backend
      const formattedData = {
        ...formData,
        dateOfBirth: new Date(formData.dateOfBirth).toISOString()
      };
      onSubmit(formattedData);
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

  const addCapitalGain = () => {
    const newGain: CapitalGainDetailsDto = {
      assetType: '',
      dateOfSale: '2023-01-01',
      dateOfPurchase: '2022-01-01',
      salePrice: 0,
      costOfAcquisition: 0,
      costOfImprovement: 0,
      expensesOnTransfer: 0
    };
    updateFormData('capitalGains', [...formData.capitalGains, newGain]);
  };

  const updateCapitalGain = (index: number, field: keyof CapitalGainDetailsDto, value: any) => {
    const updated = [...formData.capitalGains];
    updated[index] = { ...updated[index], [field]: value };
    updateFormData('capitalGains', updated);
  };

  const removeCapitalGain = (index: number) => {
    const updated = formData.capitalGains.filter((_, i) => i !== index);
    updateFormData('capitalGains', updated);
  };

  const addForeignAsset = () => {
    const newAsset: ForeignAssetDetailsDto = {
      assetType: '',
      country: '',
      value: 0,
      currency: 'USD'
    };
    updateFormData('foreignAssets', [...formData.foreignAssets, newAsset]);
  };

  const updateForeignAsset = (index: number, field: keyof ForeignAssetDetailsDto, value: any) => {
    const updated = [...formData.foreignAssets];
    updated[index] = { ...updated[index], [field]: value };
    updateFormData('foreignAssets', updated);
  };

  const removeForeignAsset = (index: number) => {
    const updated = formData.foreignAssets.filter((_, i) => i !== index);
    updateFormData('foreignAssets', updated);
  };

  return (
    <Grow in timeout={300}>
      <Box>
        {/* Header */}
        <Card sx={{ 
          mb: 4,
          background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
          color: 'white',
          borderRadius: 3
        }}>
          <CardContent sx={{ textAlign: 'center', py: 4 }}>
            <Assignment sx={{ fontSize: 48, mb: 2, opacity: 0.9 }} />
            <Typography variant="h4" gutterBottom sx={{ fontWeight: 700 }}>
              Additional Information Required
            </Typography>
            <Typography variant="subtitle1" sx={{ opacity: 0.9, maxWidth: 600, mx: 'auto' }}>
              Please provide the following information that is not available in your Form16 
              to ensure accurate ITR generation
            </Typography>
          </CardContent>
        </Card>

        <form onSubmit={handleSubmit}>

      {/* Personal Information */}
      <Accordion defaultExpanded>
        <AccordionSummary expandIcon={<ExpandMore />}>
          <Box sx={{ display: 'flex', alignItems: 'center' }}>
            <Person sx={{ mr: 1 }} />
            <Typography variant="subtitle1">Personal Information</Typography>
          </Box>
        </AccordionSummary>
        <AccordionDetails>
          <Stack spacing={2}>
            <Box sx={{ display: 'flex', gap: 2 }}>
              <TextField
                label="Date of Birth"
                type="date"
                value={formData.dateOfBirth}
                onChange={(e) => updateFormData('dateOfBirth', e.target.value)}
                error={!!errors.dateOfBirth}
                helperText={errors.dateOfBirth}
                sx={{ flex: 1 }}
                InputLabelProps={{ shrink: true }}
                required
              />
              <TextField
                label="Aadhaar Number"
                value={formData.aadhaarNumber}
                onChange={(e) => updateFormData('aadhaarNumber', e.target.value)}
                placeholder="12-digit Aadhaar number"
                sx={{ flex: 1 }}
                inputProps={{ maxLength: 12 }}
              />
            </Box>
            <TextField
              label="Address"
              value={formData.address}
              onChange={(e) => updateFormData('address', e.target.value)}
              error={!!errors.address}
              helperText={errors.address}
              fullWidth
              multiline
              rows={2}
              required
            />
            <Box sx={{ display: 'flex', gap: 2 }}>
              <TextField
                label="City"
                value={formData.city}
                onChange={(e) => updateFormData('city', e.target.value)}
                error={!!errors.city}
                helperText={errors.city}
                sx={{ flex: 1 }}
                required
              />
              <TextField
                label="State"
                value={formData.state}
                onChange={(e) => updateFormData('state', e.target.value)}
                error={!!errors.state}
                helperText={errors.state}
                sx={{ flex: 1 }}
                required
              />
              <TextField
                label="Pincode"
                value={formData.pincode}
                onChange={(e) => updateFormData('pincode', e.target.value)}
                error={!!errors.pincode}
                helperText={errors.pincode}
                sx={{ flex: 1 }}
                required
              />
            </Box>
            <Box sx={{ display: 'flex', gap: 2 }}>
              <TextField
                label="Email Address"
                type="email"
                value={formData.emailAddress}
                onChange={(e) => updateFormData('emailAddress', e.target.value)}
                error={!!errors.emailAddress}
                helperText={errors.emailAddress}
                sx={{ flex: 1 }}
                required
              />
              <TextField
                label="Mobile Number"
                value={formData.mobileNumber}
                onChange={(e) => updateFormData('mobileNumber', e.target.value)}
                error={!!errors.mobileNumber}
                helperText={errors.mobileNumber}
                sx={{ flex: 1 }}
                required
              />
            </Box>
          </Stack>
        </AccordionDetails>
      </Accordion>

      {/* Bank Details */}
      <Accordion>
        <AccordionSummary expandIcon={<ExpandMore />}>
          <Typography variant="subtitle1">Bank Details (for Refund)</Typography>
        </AccordionSummary>
        <AccordionDetails>
          <Stack spacing={2}>
            <Box sx={{ display: 'flex', gap: 2 }}>
              <TextField
                label="Bank Account Number"
                value={formData.bankAccountNumber}
                onChange={(e) => updateFormData('bankAccountNumber', e.target.value)}
                error={!!errors.bankAccountNumber}
                helperText={errors.bankAccountNumber}
                sx={{ flex: 1 }}
                required
              />
              <TextField
                label="IFSC Code"
                value={formData.bankIFSCCode}
                onChange={(e) => updateFormData('bankIFSCCode', e.target.value.toUpperCase())}
                error={!!errors.bankIFSCCode}
                helperText={errors.bankIFSCCode}
                sx={{ flex: 1 }}
                required
              />
            </Box>
            <TextField
              label="Bank Name"
              value={formData.bankName}
              onChange={(e) => updateFormData('bankName', e.target.value)}
              error={!!errors.bankName}
              helperText={errors.bankName}
              fullWidth
              required
            />
          </Stack>
        </AccordionDetails>
      </Accordion>

      {/* House Property */}
      <Accordion>
        <AccordionSummary expandIcon={<ExpandMore />}>
          <Box sx={{ display: 'flex', alignItems: 'center' }}>
            <Home sx={{ mr: 1 }} />
            <Typography variant="subtitle1">House Property Income</Typography>
          </Box>
        </AccordionSummary>
        <AccordionDetails>
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

      {/* Capital Gains */}
      <Accordion>
        <AccordionSummary expandIcon={<ExpandMore />}>
          <Box sx={{ display: 'flex', alignItems: 'center' }}>
            <TrendingUp sx={{ mr: 1 }} />
            <Typography variant="subtitle1">Capital Gains</Typography>
          </Box>
        </AccordionSummary>
        <AccordionDetails>
          <FormControlLabel
            control={
              <Switch
                checked={formData.hasCapitalGains}
                onChange={(e) => updateFormData('hasCapitalGains', e.target.checked)}
              />
            }
            label="I have capital gains from sale of assets"
            sx={{ mb: 2 }}
          />
          
          {formData.hasCapitalGains && (
            <Box>
              <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
                <Typography variant="subtitle2">Capital Gain Details</Typography>
                <Button startIcon={<Add />} onClick={addCapitalGain}>
                  Add Capital Gain
                </Button>
              </Box>
              
              {formData.capitalGains.map((gain, index) => (
                <Card key={index} variant="outlined" sx={{ mb: 2 }}>
                  <CardContent>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
                      <Typography variant="subtitle2">Capital Gain {index + 1}</Typography>
                      <IconButton onClick={() => removeCapitalGain(index)} color="error">
                        <Delete />
                      </IconButton>
                    </Box>
                      <Stack spacing={2}>
                        <Box sx={{ display: 'flex', gap: 2 }}>
                          <TextField
                            label="Asset Type"
                            value={gain.assetType}
                            onChange={(e) => updateCapitalGain(index, 'assetType', e.target.value)}
                            placeholder="e.g., Equity Shares, Property"
                            sx={{ flex: 2 }}
                          />
                          <TextField
                            label="Date of Purchase"
                            type="date"
                            value={gain.dateOfPurchase}
                            onChange={(e) => updateCapitalGain(index, 'dateOfPurchase', e.target.value)}
                            sx={{ flex: 1 }}
                            InputLabelProps={{ shrink: true }}
                          />
                          <TextField
                            label="Date of Sale"
                            type="date"
                            value={gain.dateOfSale}
                            onChange={(e) => updateCapitalGain(index, 'dateOfSale', e.target.value)}
                            sx={{ flex: 1 }}
                            InputLabelProps={{ shrink: true }}
                          />
                        </Box>
                        <Box sx={{ display: 'flex', gap: 2 }}>
                          <TextField
                            label="Sale Price"
                            type="number"
                            value={gain.salePrice}
                            onChange={(e) => updateCapitalGain(index, 'salePrice', Number(e.target.value))}
                            sx={{ flex: 1 }}
                          />
                          <TextField
                            label="Cost of Acquisition"
                            type="number"
                            value={gain.costOfAcquisition}
                            onChange={(e) => updateCapitalGain(index, 'costOfAcquisition', Number(e.target.value))}
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

      {/* Other Income */}
      <Accordion>
        <AccordionSummary expandIcon={<ExpandMore />}>
          <Typography variant="subtitle1">Other Income Sources</Typography>
        </AccordionSummary>
        <AccordionDetails>
          <FormControlLabel
            control={
              <Switch
                checked={formData.hasOtherIncome}
                onChange={(e) => updateFormData('hasOtherIncome', e.target.checked)}
              />
            }
            label="I have additional income sources"
            sx={{ mb: 2 }}
          />
          
          {formData.hasOtherIncome && (
            <Box sx={{ display: 'flex', gap: 2 }}>
              <TextField
                label="Other Interest Income"
                type="number"
                value={formData.otherInterestIncome}
                onChange={(e) => updateFormData('otherInterestIncome', Number(e.target.value))}
                sx={{ flex: 1 }}
              />
              <TextField
                label="Other Dividend Income"
                type="number"
                value={formData.otherDividendIncome}
                onChange={(e) => updateFormData('otherDividendIncome', Number(e.target.value))}
                sx={{ flex: 1 }}
              />
              <TextField
                label="Other Sources Income"
                type="number"
                value={formData.otherSourcesIncome}
                onChange={(e) => updateFormData('otherSourcesIncome', Number(e.target.value))}
                sx={{ flex: 1 }}
              />
            </Box>
          )}
        </AccordionDetails>
      </Accordion>

      {/* Foreign Income & Assets */}
      <Accordion>
        <AccordionSummary expandIcon={<ExpandMore />}>
          <Box sx={{ display: 'flex', alignItems: 'center' }}>
            <Public sx={{ mr: 1 }} />
            <Typography variant="subtitle1">Foreign Income & Assets</Typography>
          </Box>
        </AccordionSummary>
        <AccordionDetails>
          <FormControlLabel
            control={
              <Switch
                checked={formData.hasForeignIncome}
                onChange={(e) => updateFormData('hasForeignIncome', e.target.checked)}
              />
            }
            label="I have foreign income"
            sx={{ mb: 2 }}
          />
          
          {formData.hasForeignIncome && (
            <TextField
              label="Foreign Income Amount (₹)"
              type="number"
              value={formData.foreignIncome}
              onChange={(e) => updateFormData('foreignIncome', Number(e.target.value))}
              fullWidth
              sx={{ mb: 2 }}
            />
          )}
          
          <FormControlLabel
            control={
              <Switch
                checked={formData.hasForeignAssets}
                onChange={(e) => updateFormData('hasForeignAssets', e.target.checked)}
              />
            }
            label="I have foreign assets"
            sx={{ mb: 2 }}
          />
          
          {formData.hasForeignAssets && (
            <Box>
              <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
                <Typography variant="subtitle2">Foreign Asset Details</Typography>
                <Button startIcon={<Add />} onClick={addForeignAsset}>
                  Add Asset
                </Button>
              </Box>
              
              {formData.foreignAssets.map((asset, index) => (
                <Card key={index} variant="outlined" sx={{ mb: 2 }}>
                  <CardContent>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
                      <Typography variant="subtitle2">Foreign Asset {index + 1}</Typography>
                      <IconButton onClick={() => removeForeignAsset(index)} color="error">
                        <Delete />
                      </IconButton>
                    </Box>
                      <Stack spacing={2}>
                        <Box sx={{ display: 'flex', gap: 2 }}>
                          <TextField
                            label="Asset Type"
                            value={asset.assetType}
                            onChange={(e) => updateForeignAsset(index, 'assetType', e.target.value)}
                            sx={{ flex: 1 }}
                          />
                          <TextField
                            label="Country"
                            value={asset.country}
                            onChange={(e) => updateForeignAsset(index, 'country', e.target.value)}
                            sx={{ flex: 1 }}
                          />
                        </Box>
                        <Box sx={{ display: 'flex', gap: 2 }}>
                          <TextField
                            label="Value"
                            type="number"
                            value={asset.value}
                            onChange={(e) => updateForeignAsset(index, 'value', Number(e.target.value))}
                            sx={{ flex: 1 }}
                          />
                          <TextField
                            label="Currency"
                            value={asset.currency}
                            onChange={(e) => updateForeignAsset(index, 'currency', e.target.value)}
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
