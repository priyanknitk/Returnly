import React, { useState } from 'react';
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
  Stack
} from '@mui/material';
import {
  ExpandMore as ExpandMoreIcon,
  Person as PersonIcon,
  AccountBalance as BankIcon,
  NavigateNext as NextIcon
} from '@mui/icons-material';
import { AdditionalTaxpayerInfoDto } from '../types/api';

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

  const handleAccordionChange = (panel: string) => (
    event: React.SyntheticEvent,
    isExpanded: boolean
  ) => {
    setExpandedSection(isExpanded ? panel : '');
  };

  const handleNext = () => {
    // Basic validation
    if (!personalInfo.emailAddress || !personalInfo.mobileNumber) {
      alert('Please fill in required fields (Email and Mobile Number)');
      return;
    }
    onNext();
  };

  return (
    <Box sx={{ maxWidth: 800, mx: 'auto', p: 3 }}>
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
            <StepLabel>ITR Generation</StepLabel>
          </Step>
        </Stepper>
      </Box>

      <Typography variant="h4" gutterBottom sx={{ mb: 3, textAlign: 'center' }}>
        Personal Details
      </Typography>

      {/* Personal Information Section */}
      <Accordion 
        expanded={expandedSection === 'personal'} 
        onChange={handleAccordionChange('personal')}
        sx={{ mb: 2 }}
      >
        <AccordionSummary expandIcon={<ExpandMoreIcon />}>
          <Avatar sx={{ bgcolor: '#6366f1', mr: 2 }}>
            <PersonIcon />
          </Avatar>
          <Box>
            <Typography variant="h6">Personal Information</Typography>
            <Typography variant="body2" color="text.secondary">
              Basic details and contact information
            </Typography>
          </Box>
        </AccordionSummary>
        <AccordionDetails>
          <Stack spacing={3}>
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
                label="Email Address *"
                type="email"
                value={personalInfo.emailAddress || ''}
                onChange={(e) => onPersonalInfoChange({ emailAddress: e.target.value })}
                required
              />
            </Stack>
            <Stack direction={{ xs: 'column', md: 'row' }} spacing={2}>
              <TextField
                fullWidth
                label="Mobile Number *"
                value={personalInfo.mobileNumber || ''}
                onChange={(e) => onPersonalInfoChange({ mobileNumber: e.target.value })}
                required
              />
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

      {/* Bank Details Section */}
      <Accordion 
        expanded={expandedSection === 'bank'} 
        onChange={handleAccordionChange('bank')}
        sx={{ mb: 4 }}
      >
        <AccordionSummary expandIcon={<ExpandMoreIcon />}>
          <Avatar sx={{ bgcolor: '#059669', mr: 2 }}>
            <BankIcon />
          </Avatar>
          <Box>
            <Typography variant="h6">Bank Details (for Refund)</Typography>
            <Typography variant="body2" color="text.secondary">
              Required for tax refund processing
            </Typography>
          </Box>
        </AccordionSummary>
        <AccordionDetails>
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

      {/* Navigation Button */}
      <Box sx={{ display: 'flex', justifyContent: 'center', mt: 4 }}>
        <Button
          variant="contained"
          size="large"
          endIcon={<NextIcon />}
          onClick={handleNext}
          sx={{
            px: 4,
            py: 1.5,
            fontSize: '1.1rem',
            borderRadius: 2,
            background: 'linear-gradient(45deg, #6366f1 30%, #8b5cf6 90%)',
            '&:hover': {
              background: 'linear-gradient(45deg, #5856eb 30%, #7c3aed 90%)',
            }
          }}
        >
          Continue to Tax Data
        </Button>
      </Box>
    </Box>
  );
};

export default PersonalDetailsForm;
