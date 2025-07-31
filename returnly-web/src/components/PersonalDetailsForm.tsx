import React, { useState, useEffect } from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  TextField,
  Button,
  Accordion,
  AccordionSummary,
  AccordionDetails,
  Avatar,
  Stepper,
  Step,
  StepLabel,
  Stack,
  FormControl,
  InputLabel,
  Select,
  MenuItem
} from '@mui/material';
import {
  ExpandMore as ExpandMoreIcon,
  Person as PersonIcon,
  AccountBalance as BankIcon,
  NavigateNext as NextIcon,
  CloudUpload
} from '@mui/icons-material';
import { AdditionalTaxpayerInfoDto, Gender, MaritalStatus } from '../types/api';
import { useTaxDataPersistence } from '../hooks/useTaxDataPersistence';

interface PersonalDetailsFormProps {
  personalInfo: AdditionalTaxpayerInfoDto;
  onPersonalInfoChange: (info: Partial<AdditionalTaxpayerInfoDto>) => void;
  onNext: () => void;
}

const PersonalDetailsForm: React.FC<PersonalDetailsFormProps> = ({
  personalInfo,
  onPersonalInfoChange,
  onNext
}) => {
  const [expandedSection, setExpandedSection] = useState<string>('personal');
  const { 
    savePersonalInfo, 
    saveCurrentStep,
    personalInfo: savedPersonalInfo,
    hasSavedData,
    form16Data: savedForm16Data
  } = useTaxDataPersistence();

  // Check if we have Form16 data that pre-populated fields
  const hasForm16Data = savedForm16Data && (
    savedForm16Data.employeeName || 
    savedForm16Data.pan
  );

  // Auto-restore saved data on mount if current data is still default
  useEffect(() => {
    if (hasSavedData() && personalInfo.emailAddress === 'sample.user@example.test') {
      onPersonalInfoChange(savedPersonalInfo);
    }
  }, []); // Only run on mount

  const handleAccordionChange = (panel: string) => (
    event: React.SyntheticEvent,
    isExpanded: boolean
  ) => {
    setExpandedSection(isExpanded ? panel : '');
  };

  const handleNext = () => {
    // Basic validation - properly check for undefined/null instead of falsy values
    if (!personalInfo.employeeName || 
        !personalInfo.pan || 
        !personalInfo.emailAddress || 
        !personalInfo.mobileNumber || 
        !personalInfo.fatherName || 
        personalInfo.gender === undefined || 
        personalInfo.gender === null ||
        personalInfo.maritalStatus === undefined || 
        personalInfo.maritalStatus === null) {
      alert('Please fill in all required fields (Full Name, PAN Number, Father\'s Name, Gender, Marital Status, Email and Mobile Number)');
      return;
    }
    
    // Save data when moving to next step
    savePersonalInfo(personalInfo);
    saveCurrentStep(1);
    
    onNext();
  };

  return (
    <Box sx={{ 
      maxWidth: 1200, 
      mx: 'auto', 
      mt: 2, 
      px: { xs: 1, sm: 2, md: 3 },
      '@keyframes float': {
        '0%, 100%': { transform: 'translateY(0px)' },
        '50%': { transform: 'translateY(-10px)' }
      }
    }}>
      {/* Modern Header */}
      <Box sx={{ 
        mb: 4,
        position: 'relative',
        overflow: 'hidden'
      }}>
        {/* Floating Header Card */}
        <Box sx={{
          background: 'linear-gradient(145deg, rgba(255,255,255,0.95) 0%, rgba(255,255,255,0.85) 100%)',
          backdropFilter: 'blur(20px)',
          borderRadius: 4,
          border: '1px solid rgba(255,255,255,0.3)',
          boxShadow: '0 25px 50px -12px rgba(0, 0, 0, 0.25), 0 0 0 1px rgba(255,255,255,0.05)',
          p: 4,
          position: 'relative',
          zIndex: 2
        }}>
          {/* Floating Elements */}
          <Box sx={{
            position: 'absolute',
            top: -20,
            right: 40,
            width: 80,
            height: 80,
            background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
            borderRadius: '50%',
            opacity: 0.1,
            animation: 'float 6s ease-in-out infinite'
          }} />
          
          <Box sx={{ position: 'relative', zIndex: 3, textAlign: 'center' }}>
            <PersonIcon sx={{ 
              fontSize: 32, 
              mb: 2, 
              color: 'primary.main',
              opacity: 0.8
            }} />
            <Typography variant="h5" gutterBottom sx={{ 
              fontWeight: 700,
              background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
              backgroundClip: 'text',
              WebkitBackgroundClip: 'text',
              WebkitTextFillColor: 'transparent',
              mb: 1
            }}>
              Personal Details
            </Typography>
            <Typography variant="body1" color="text.secondary" sx={{ 
              mb: 3, 
              fontStyle: 'italic',
              fontSize: '0.95rem'
            }}>
              {hasForm16Data 
                ? "Some details have been pre-filled from your Form16. Please review and complete the remaining information."
                : "Sample data has been pre-filled for your convenience. Please update with your actual information."
              }
            </Typography>
          </Box>
        </Box>
      </Box>

      {/* Progress Stepper */}
      <Box sx={{ mb: 4 }}>
        <Stepper activeStep={0} alternativeLabel>
          <Step>
            <StepLabel>Personal Details</StepLabel>
          </Step>
          <Step>
            <StepLabel>Tax Data Input</StepLabel>
          </Step>
          <Step>
            <StepLabel>Tax Results</StepLabel>
          </Step>
          <Step>
            <StepLabel>ITR Generation</StepLabel>
          </Step>
        </Stepper>
      </Box>

      {/* Personal Information Section */}
      <Card sx={{ 
        mb: 3,
        borderRadius: 3,
        boxShadow: '0 4px 20px rgba(0,0,0,0.08)',
        border: '1px solid rgba(0,0,0,0.05)'
      }}>
        <Accordion 
          expanded={expandedSection === 'personal'} 
          onChange={handleAccordionChange('personal')}
          sx={{ 
            boxShadow: 'none',
            '&:before': { display: 'none' }
          }}
        >
          <AccordionSummary 
            expandIcon={<ExpandMoreIcon />}
            sx={{
              background: 'linear-gradient(135deg, rgba(102, 126, 234, 0.05) 0%, rgba(118, 75, 162, 0.05) 100%)',
              borderRadius: expandedSection === 'personal' ? '12px 12px 0 0' : '12px',
              transition: 'all 0.3s ease'
            }}
          >
            <Avatar sx={{ 
              bgcolor: 'primary.main', 
              mr: 2.5,
              width: 48,
              height: 48,
              background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)'
            }}>
              <PersonIcon sx={{ fontSize: 24 }} />
            </Avatar>
            <Box>
              <Typography variant="h6" sx={{ fontWeight: 600, color: 'text.primary' }}>
                Personal Information
              </Typography>
              <Typography variant="body2" color="text.secondary" sx={{ fontSize: '0.875rem' }}>
                Basic details and contact information
              </Typography>
            </Box>
          </AccordionSummary>
          <AccordionDetails sx={{ p: 4, pt: 3 }}>
            <Stack spacing={3}>
            <Stack direction={{ xs: 'column', md: 'row' }} spacing={2}>
              <TextField
                fullWidth
                label="Full Name *"
                value={personalInfo.employeeName || ''}
                onChange={(e) => onPersonalInfoChange({ employeeName: e.target.value })}
                required
                helperText={
                  hasForm16Data && savedForm16Data?.employeeName ? (
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, color: 'success.main' }}>
                      <CloudUpload sx={{ fontSize: 14 }} />
                      Filled from Form16
                    </Box>
                  ) : undefined
                }
                FormHelperTextProps={{
                  sx: { color: 'success.main', fontWeight: 500 }
                }}
              />
              <TextField
                fullWidth
                label="PAN Number *"
                value={personalInfo.pan || ''}
                onChange={(e) => onPersonalInfoChange({ pan: e.target.value.toUpperCase() })}
                inputProps={{ maxLength: 10, style: { textTransform: 'uppercase' } }}
                required
                helperText={
                  hasForm16Data && savedForm16Data?.pan ? (
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, color: 'success.main' }}>
                      <CloudUpload sx={{ fontSize: 14 }} />
                      Filled from Form16
                    </Box>
                  ) : undefined
                }
                FormHelperTextProps={{
                  sx: { color: 'success.main', fontWeight: 500 }
                }}
              />
            </Stack>
            <Stack direction={{ xs: 'column', md: 'row' }} spacing={2}>
              <TextField
                fullWidth
                label="Date of Birth"
                type="date"
                value={personalInfo.dateOfBirth || ''}
                onChange={(e) => onPersonalInfoChange({ 
                  dateOfBirth: e.target.value 
                })}
                InputLabelProps={{ shrink: true }}
              />
              <TextField
                fullWidth
                label="Father's Name *"
                value={personalInfo.fatherName || ''}
                onChange={(e) => onPersonalInfoChange({ fatherName: e.target.value })}
                required
              />
            </Stack>
            <Stack direction={{ xs: 'column', md: 'row' }} spacing={2}>
              <FormControl fullWidth required>
                <InputLabel>Gender *</InputLabel>
                <Select
                  value={personalInfo.gender !== undefined ? personalInfo.gender : ''}
                  label="Gender *"
                  onChange={(e) => onPersonalInfoChange({ gender: e.target.value as Gender })}
                >
                  <MenuItem value={Gender.Male}>Male</MenuItem>
                  <MenuItem value={Gender.Female}>Female</MenuItem>
                  <MenuItem value={Gender.Other}>Other</MenuItem>
                </Select>
              </FormControl>
              <FormControl fullWidth required>
                <InputLabel>Marital Status *</InputLabel>
                <Select
                  value={personalInfo.maritalStatus !== undefined ? personalInfo.maritalStatus : ''}
                  label="Marital Status *"
                  onChange={(e) => onPersonalInfoChange({ maritalStatus: e.target.value as MaritalStatus })}
                >
                  <MenuItem value={MaritalStatus.Single}>Single</MenuItem>
                  <MenuItem value={MaritalStatus.Married}>Married</MenuItem>
                  <MenuItem value={MaritalStatus.Divorced}>Divorced</MenuItem>
                  <MenuItem value={MaritalStatus.Widowed}>Widowed</MenuItem>
                </Select>
              </FormControl>
            </Stack>
            <Stack direction={{ xs: 'column', md: 'row' }} spacing={2}>
              <TextField
                fullWidth
                label="Email Address *"
                type="email"
                value={personalInfo.emailAddress || ''}
                onChange={(e) => onPersonalInfoChange({ emailAddress: e.target.value })}
                required
              />
              <TextField
                fullWidth
                label="Mobile Number *"
                value={personalInfo.mobileNumber || ''}
                onChange={(e) => onPersonalInfoChange({ mobileNumber: e.target.value })}
                required
              />
            </Stack>
            <Stack direction={{ xs: 'column', md: 'row' }} spacing={2}>
              <TextField
                fullWidth
                label="Aadhaar Number"
                value={personalInfo.aadhaarNumber || ''}
                onChange={(e) => onPersonalInfoChange({ aadhaarNumber: e.target.value })}
                inputProps={{ maxLength: 12 }}
              />
            </Stack>
            <TextField
              fullWidth
              label="Address"
              multiline
              rows={3}
              value={personalInfo.address || ''}
              onChange={(e) => onPersonalInfoChange({ address: e.target.value })}
            />
            <Stack direction={{ xs: 'column', md: 'row' }} spacing={2}>
              <TextField
                fullWidth
                label="City"
                value={personalInfo.city || ''}
                onChange={(e) => onPersonalInfoChange({ city: e.target.value })}
              />
              <TextField
                fullWidth
                label="State"
                value={personalInfo.state || ''}
                onChange={(e) => onPersonalInfoChange({ state: e.target.value })}
              />
              <TextField
                fullWidth
                label="Pincode"
                value={personalInfo.pincode || ''}
                onChange={(e) => onPersonalInfoChange({ pincode: e.target.value })}
                inputProps={{ maxLength: 6 }}
              />
            </Stack>
          </Stack>
        </AccordionDetails>
        </Accordion>
      </Card>

      {/* Bank Details Section */}
      <Card sx={{ 
        mb: 3,
        borderRadius: 3,
        boxShadow: '0 4px 20px rgba(0,0,0,0.08)',
        border: '1px solid rgba(0,0,0,0.05)'
      }}>
        <Accordion 
          expanded={expandedSection === 'bank'} 
          onChange={handleAccordionChange('bank')}
          sx={{ 
            boxShadow: 'none',
            '&:before': { display: 'none' }
          }}
        >
          <AccordionSummary 
            expandIcon={<ExpandMoreIcon />}
            sx={{
              background: 'linear-gradient(135deg, rgba(5, 150, 105, 0.05) 0%, rgba(16, 185, 129, 0.05) 100%)',
              borderRadius: expandedSection === 'bank' ? '12px 12px 0 0' : '12px',
              transition: 'all 0.3s ease'
            }}
          >
            <Avatar sx={{ 
              bgcolor: 'success.main', 
              mr: 2.5,
              width: 48,
              height: 48,
              background: 'linear-gradient(135deg, #059669 0%, #10b981 100%)'
            }}>
              <BankIcon sx={{ fontSize: 24 }} />
            </Avatar>
            <Box>
              <Typography variant="h6" sx={{ fontWeight: 600, color: 'text.primary' }}>
                Bank Details (for Refund)
              </Typography>
              <Typography variant="body2" color="text.secondary" sx={{ fontSize: '0.875rem' }}>
                Required for tax refund processing
              </Typography>
            </Box>
          </AccordionSummary>
          <AccordionDetails sx={{ p: 4, pt: 3 }}>
          <Stack spacing={3}>
            <Stack direction={{ xs: 'column', md: 'row' }} spacing={2}>
              <TextField
                fullWidth
                label="Bank Account Number"
                value={personalInfo.bankAccountNumber || ''}
                onChange={(e) => onPersonalInfoChange({ bankAccountNumber: e.target.value })}
              />
              <TextField
                fullWidth
                label="Bank IFSC Code"
                value={personalInfo.bankIFSCCode || ''}
                onChange={(e) => onPersonalInfoChange({ bankIFSCCode: e.target.value.toUpperCase() })}
                inputProps={{ maxLength: 11 }}
              />
            </Stack>
            <TextField
              fullWidth
              label="Bank Name"
              value={personalInfo.bankName || ''}
              onChange={(e) => onPersonalInfoChange({ bankName: e.target.value })}
            />
          </Stack>
        </AccordionDetails>
        </Accordion>
      </Card>

      {/* Navigation Button */}
      <Box sx={{ display: 'flex', justifyContent: 'center', mt: 4 }}>
        <Button
          variant="contained"
          size="large"
          endIcon={<NextIcon />}
          onClick={handleNext}
          sx={{
            px: 6,
            py: 1.5,
            fontSize: '1rem',
            borderRadius: 3,
            background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
            boxShadow: '0 8px 25px rgba(102, 126, 234, 0.3)',
            '&:hover': {
              background: 'linear-gradient(135deg, #5a6fd8 0%, #6a4190 100%)',
              transform: 'translateY(-2px)',
              boxShadow: '0 12px 35px rgba(102, 126, 234, 0.4)',
            },
            transition: 'all 0.3s cubic-bezier(0.4, 0, 0.2, 1)'
          }}
        >
          Continue to Tax Data
        </Button>
      </Box>
    </Box>
  );
};

export default PersonalDetailsForm;
