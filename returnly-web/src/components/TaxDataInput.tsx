import React, { useState, useEffect } from 'react';
import {
  Card,
  CardContent,
  Typography,
  TextField,
  Button,
  Box,
  Alert,
  Divider,
  Stack,
  Accordion,
  AccordionSummary,
  AccordionDetails,
  Chip,
  IconButton,
  Fade,
  Grow,
  FormControl,
  InputLabel,
  Select,
  MenuItem
} from '@mui/material';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import PersonIcon from '@mui/icons-material/Person';
import WorkIcon from '@mui/icons-material/Work';
import SavingsIcon from '@mui/icons-material/Savings';
import TrendingUpIcon from '@mui/icons-material/TrendingUp';
import ReceiptIcon from '@mui/icons-material/Receipt';
import CalculateIcon from '@mui/icons-material/Calculate';
import AccountBalanceWalletIcon from '@mui/icons-material/AccountBalanceWallet';
import MonetizationOnIcon from '@mui/icons-material/MonetizationOn';
import AssessmentIcon from '@mui/icons-material/Assessment';
import SpeedIcon from '@mui/icons-material/Speed';
import SecurityIcon from '@mui/icons-material/Security';
import ShowChartIcon from '@mui/icons-material/ShowChart';
import { API_ENDPOINTS } from '../config/api';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';

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
  // Stocks, Mutual Funds, F&O
  stocksSTCG: number;
  stocksLTCG: number;
  mutualFundsSTCG: number;
  mutualFundsLTCG: number;
  fnoGains: number;
  // Real Estate
  realEstateSTCG: number;
  realEstateLTCG: number;
  // Bonds and Debentures
  bondsSTCG: number;
  bondsLTCG: number;
  // Gold, Jewellery and Others
  goldSTCG: number;
  goldLTCG: number;
  // Cryptocurrency (separate due to special 30% rate)
  cryptoGains: number;
}

interface TaxDataInputProps {
  initialData?: Partial<TaxData>;
  onCalculate: (data: TaxData) => void;
}

const TaxDataInput: React.FC<TaxDataInputProps> = ({ initialData, onCalculate }) => {
  const [formData, setFormData] = useState<TaxData>({
    employeeName: initialData?.employeeName || '',
    pan: initialData?.pan || '',
    assessmentYear: initialData?.assessmentYear || '2024-25',
    financialYear: initialData?.financialYear || '2023-24',
    employerName: initialData?.employerName || '',
    tan: initialData?.tan || '',
    salarySection17: initialData?.salarySection17 || 0,
    perquisites: initialData?.perquisites || 0,
    profitsInLieu: initialData?.profitsInLieu || 0,
    interestOnSavings: initialData?.interestOnSavings || 0,
    interestOnFixedDeposits: initialData?.interestOnFixedDeposits || 0,
    dividendIncome: initialData?.dividendIncome || 0,
    standardDeduction: initialData?.standardDeduction || 75000,
    professionalTax: initialData?.professionalTax || 0,
    totalTaxDeducted: initialData?.totalTaxDeducted || 0,
    // Capital Gains fields
    stocksSTCG: initialData?.stocksSTCG || 0,
    stocksLTCG: initialData?.stocksLTCG || 0,
    mutualFundsSTCG: initialData?.mutualFundsSTCG || 0,
    mutualFundsLTCG: initialData?.mutualFundsLTCG || 0,
    fnoGains: initialData?.fnoGains || 0,
    realEstateSTCG: initialData?.realEstateSTCG || 0,
    realEstateLTCG: initialData?.realEstateLTCG || 0,
    bondsSTCG: initialData?.bondsSTCG || 0,
    bondsLTCG: initialData?.bondsLTCG || 0,
    goldSTCG: initialData?.goldSTCG || 0,
    goldLTCG: initialData?.goldLTCG || 0,
    cryptoGains: initialData?.cryptoGains || 0,
  });

  const [errors, setErrors] = useState<Partial<Record<keyof TaxData, string>>>({});
  const [financialYears, setFinancialYears] = useState<string[]>([]);
  const [assessmentYears, setAssessmentYears] = useState<string[]>([]);
  const [loading, setLoading] = useState(true);

  // Fetch dropdown data on component mount
  useEffect(() => {
    const fetchDropdownData = async () => {
      try {
        setLoading(true);
        
        // Fetch financial years and assessment years
        const [fyResponse, ayResponse] = await Promise.all([
          fetch(API_ENDPOINTS.TAX_FINANCIAL_YEARS),
          fetch(API_ENDPOINTS.TAX_ASSESSMENT_YEARS)
        ]);

        if (fyResponse.ok && ayResponse.ok) {
          const fyData = await fyResponse.json();
          const ayData = await ayResponse.json();
          
          setFinancialYears(fyData || []);
          setAssessmentYears(ayData || []);
          
          // Set default values if not already set
          if (!formData.financialYear && fyData && fyData.length > 0) {
            setFormData(prev => ({ ...prev, financialYear: fyData[0] }));
          }
          if (!formData.assessmentYear && ayData && ayData.length > 0) {
            setFormData(prev => ({ ...prev, assessmentYear: ayData[0] }));
          }
        }
      } catch (error) {
        console.error('Error fetching dropdown data:', error);
        // Set fallback values
        setFinancialYears(['2023-24', '2024-25', '2025-26']);
        setAssessmentYears(['2024-25', '2025-26', '2026-27']);
      } finally {
        setLoading(false);
      }
    };

    fetchDropdownData();
  }, []);

  const handleChange = (field: keyof TaxData) => (event: React.ChangeEvent<HTMLInputElement>) => {
    const value = event.target.type === 'number' ? parseFloat(event.target.value) || 0 : event.target.value;
    setFormData(prev => ({ ...prev, [field]: value }));
    
    // Clear error when user starts typing
    if (errors[field]) {
      setErrors(prev => ({ ...prev, [field]: undefined }));
    }
  };

  const handleSelectChange = (field: keyof TaxData) => (event: any) => {
    const value = event.target.value;
    
    // If assessment year is selected, automatically set financial year to one year before
    if (field === 'assessmentYear') {
      const ayYear = parseInt(value.split('-')[0]);
      const fyYear = ayYear - 1;
      const correspondingFY = `${fyYear}-${(fyYear + 1).toString().substring(2)}`;
      
      // Check if the corresponding FY exists in the available options
      if (financialYears.includes(correspondingFY)) {
        setFormData(prev => ({ 
          ...prev, 
          [field]: value,
          financialYear: correspondingFY
        }));
      } else {
        setFormData(prev => ({ ...prev, [field]: value }));
      }
    } else {
      setFormData(prev => ({ ...prev, [field]: value }));
    }
    
    // Clear error when user makes selection
    if (errors[field]) {
      setErrors(prev => ({ ...prev, [field]: undefined }));
    }
  };

  const validateForm = (): boolean => {
    const newErrors: Partial<Record<keyof TaxData, string>> = {};

    if (!formData.employeeName.trim()) {
      newErrors.employeeName = 'Employee name is required';
    }

    if (!formData.pan.trim() || formData.pan.length !== 10) {
      newErrors.pan = 'Valid PAN number is required (10 characters)';
    }

    if (formData.salarySection17 < 0) {
      newErrors.salarySection17 = 'Salary cannot be negative';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleCalculate = () => {
    if (validateForm()) {
      onCalculate(formData);
    }
  };

  const grossSalary = formData.salarySection17 + formData.perquisites + formData.profitsInLieu;
  const totalCapitalGains = formData.stocksSTCG + formData.stocksLTCG + formData.mutualFundsSTCG + formData.mutualFundsLTCG + formData.fnoGains + formData.realEstateSTCG + formData.realEstateLTCG + formData.bondsSTCG + formData.bondsLTCG + formData.goldSTCG + formData.goldLTCG + formData.cryptoGains;
  const totalIncome = grossSalary + formData.interestOnSavings + formData.interestOnFixedDeposits + formData.dividendIncome + totalCapitalGains;
  const taxableIncome = Math.max(0, totalIncome - formData.standardDeduction - formData.professionalTax);

  return (
    <Box sx={{ maxWidth: 1400, mx: 'auto', mt: 4, px: { xs: 1, sm: 2, md: 3 } }}>
      {/* Header Card with Gradient */}
      {/* Ultra Modern Header */}
      <Box sx={{ 
        mb: 5,
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
          p: 6,
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
          <Box sx={{
            position: 'absolute',
            bottom: -30,
            left: 60,
            width: 60,
            height: 60,
            background: 'linear-gradient(135deg, #f093fb 0%, #f5576c 100%)',
            borderRadius: '50%',
            opacity: 0.08,
            animation: 'float 8s ease-in-out infinite reverse'
          }} />
          
          <Stack spacing={4} alignItems="center" textAlign="center">
            {/* Modern Icon */}
            <Box sx={{
              position: 'relative',
              display: 'inline-flex'
            }}>
              <Box sx={{
                p: 2.5,
                borderRadius: 3,
                background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
                boxShadow: '0 20px 40px rgba(102, 126, 234, 0.3)',
                transform: 'rotate(-5deg)',
                transition: 'all 0.3s ease'
              }}>
                <AssessmentIcon sx={{ fontSize: 42, color: 'white' }} />
              </Box>
              <Box sx={{
                position: 'absolute',
                top: 8,
                left: 8,
                p: 2.5,
                borderRadius: 3,
                background: 'linear-gradient(135deg, rgba(102, 126, 234, 0.1) 0%, rgba(118, 75, 162, 0.1) 100%)',
                border: '2px solid rgba(102, 126, 234, 0.2)',
                zIndex: -1
              }}>
                <AssessmentIcon sx={{ fontSize: 42, color: 'transparent' }} />
              </Box>
            </Box>

            {/* Ultra Modern Typography */}
            <Box>
              <Typography sx={{ 
                fontSize: { xs: '2.5rem', md: '3.5rem' },
                fontWeight: 900,
                background: 'linear-gradient(135deg, #1a1a1a 0%, #4a4a4a 100%)',
                backgroundClip: 'text',
                WebkitBackgroundClip: 'text',
                color: 'transparent',
                letterSpacing: '-0.02em',
                lineHeight: 0.9,
                mb: 2
              }}>
                Smart Tax
              </Typography>
              <Typography sx={{ 
                fontSize: { xs: '2rem', md: '2.8rem' },
                fontWeight: 300,
                background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
                backgroundClip: 'text',
                WebkitBackgroundClip: 'text',
                color: 'transparent',
                letterSpacing: '-0.01em',
                lineHeight: 0.9
              }}>
                Calculator
              </Typography>
            </Box>

            {/* Sleek Description */}
            <Typography variant="h6" sx={{ 
              color: 'text.secondary',
              fontWeight: 400,
              maxWidth: 600,
              lineHeight: 1.5,
              fontSize: '1.1rem'
            }}>
              Calculate your tax liability with precision and generate ITR-ready reports
            </Typography>

            {/* Modern Feature Pills */}
            <Stack 
              direction={{ xs: 'column', sm: 'row' }} 
              spacing={2}
              sx={{ mt: 3 }}
            >
              {[
                { icon: SpeedIcon, text: 'Instant Results', color: '#667eea' },
                { icon: SecurityIcon, text: 'Bank-Grade Security', color: '#764ba2' },
                { icon: CheckCircleIcon, text: 'ITR Ready', color: '#4caf50' }
              ].map((feature, index) => (
                <Box key={index} sx={{
                  display: 'flex',
                  alignItems: 'center',
                  gap: 1.5,
                  px: 3,
                  py: 1.5,
                  borderRadius: 3,
                  background: `linear-gradient(135deg, ${feature.color}15 0%, ${feature.color}08 100%)`,
                  border: `1px solid ${feature.color}20`,
                  transition: 'all 0.3s ease',
                  '&:hover': {
                    transform: 'translateY(-2px)',
                    boxShadow: `0 10px 25px ${feature.color}25`
                  }
                }}>
                  <feature.icon sx={{ fontSize: 20, color: feature.color }} />
                  <Typography variant="body2" sx={{ 
                    fontWeight: 600, 
                    color: 'text.primary',
                    fontSize: '0.9rem'
                  }}>
                    {feature.text}
                  </Typography>
                </Box>
              ))}
            </Stack>
          </Stack>
        </Box>

        {/* Animated Background */}
        <Box sx={{
          position: 'absolute',
          top: 0,
          left: 0,
          right: 0,
          bottom: 0,
          background: 'linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%)',
          borderRadius: 4,
          transform: 'rotate(1deg) scale(1.02)',
          zIndex: 1
        }} />
      </Box>

      <style>
        {`
          @keyframes float {
            0%, 100% { transform: translateY(0px) rotate(0deg); }
            50% { transform: translateY(-20px) rotate(180deg); }
          }
        `}
      </style>

      {/* Main Form Card */}
      <Card sx={{ 
        borderRadius: 3,
        boxShadow: '0 4px 20px rgba(0,0,0,0.08)',
        overflow: 'hidden',
        border: '1px solid rgba(0,0,0,0.05)'
      }}>
        <CardContent sx={{ p: 4 }}>
          <Box component="form">{/* Personal Information */}
          <Grow in timeout={300}>
            <Accordion 
              defaultExpanded 
              sx={{ 
                mb: 3,
                borderRadius: 2,
                boxShadow: '0 2px 12px rgba(0,0,0,0.06)',
                border: '1px solid rgba(0,0,0,0.08)',
                '&:before': { display: 'none' },
                overflow: 'hidden'
              }}
            >
              <AccordionSummary 
                expandIcon={<ExpandMoreIcon sx={{ color: 'primary.main' }} />}
                sx={{ 
                  background: 'linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%)',
                  borderBottom: '1px solid rgba(0,0,0,0.08)',
                  py: 2,
                  '&.Mui-expanded': {
                    minHeight: 64
                  }
                }}
              >
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                  <PersonIcon sx={{ color: 'primary.main', fontSize: 28 }} />
                  <Box>
                    <Typography variant="h6" sx={{ fontWeight: 600, color: 'primary.main' }}>
                      Personal Information
                    </Typography>
                    <Typography variant="caption" sx={{ color: 'text.secondary' }}>
                      Basic details and identification
                    </Typography>
                  </Box>
                </Box>
              </AccordionSummary>
              <AccordionDetails sx={{ p: 3, backgroundColor: 'grey.50' }}>
                <Stack spacing={3}>
                  <Stack spacing={3} direction={{ xs: 'column', sm: 'row' }}>
                    <TextField
                      fullWidth
                      label="Employee Name"
                      value={formData.employeeName}
                      onChange={handleChange('employeeName')}
                      error={!!errors.employeeName}
                      helperText={errors.employeeName}
                      variant="outlined"
                      sx={{
                        '& .MuiOutlinedInput-root': {
                          borderRadius: 2,
                          backgroundColor: 'white',
                          transition: 'all 0.2s ease-in-out',
                          '&:hover': {
                            boxShadow: '0 2px 8px rgba(0,0,0,0.1)'
                          },
                          '&.Mui-focused': {
                            boxShadow: '0 4px 16px rgba(25, 118, 210, 0.2)'
                          }
                        }
                      }}
                    />
                    <TextField
                      fullWidth
                      label="PAN Number"
                      value={formData.pan}
                      onChange={handleChange('pan')}
                      error={!!errors.pan}
                      helperText={errors.pan}
                      inputProps={{ maxLength: 10, style: { textTransform: 'uppercase' } }}
                      variant="outlined"
                      sx={{
                        '& .MuiOutlinedInput-root': {
                          borderRadius: 2,
                          backgroundColor: 'white',
                          transition: 'all 0.2s ease-in-out',
                          '&:hover': {
                            boxShadow: '0 2px 8px rgba(0,0,0,0.1)'
                          },
                          '&.Mui-focused': {
                            boxShadow: '0 4px 16px rgba(25, 118, 210, 0.2)'
                          }
                        }
                      }}
                    />
                  </Stack>

                  <Stack spacing={3} direction={{ xs: 'column', sm: 'row' }}>
                    <FormControl fullWidth>
                      <InputLabel id="assessment-year-label">Assessment Year</InputLabel>
                      <Select
                        labelId="assessment-year-label"
                        value={formData.assessmentYear}
                        label="Assessment Year"
                        onChange={handleSelectChange('assessmentYear')}
                        disabled={loading}
                        sx={{
                          borderRadius: 2,
                          backgroundColor: 'white',
                          transition: 'all 0.2s ease-in-out',
                          '&:hover': {
                            boxShadow: '0 2px 8px rgba(0,0,0,0.1)'
                          },
                          '&.Mui-focused': {
                            boxShadow: '0 4px 16px rgba(25, 118, 210, 0.2)'
                          }
                        }}
                      >
                        {assessmentYears.map((year) => (
                          <MenuItem key={year} value={year}>
                            {year}
                          </MenuItem>
                        ))}
                      </Select>
                    </FormControl>
                    <FormControl fullWidth>
                      <InputLabel id="financial-year-label">Financial Year</InputLabel>
                      <Select
                        labelId="financial-year-label"
                        value={formData.financialYear}
                        label="Financial Year"
                        onChange={handleSelectChange('financialYear')}
                        disabled={loading}
                        sx={{
                          borderRadius: 2,
                          backgroundColor: 'white',
                          transition: 'all 0.2s ease-in-out',
                          '&:hover': {
                            boxShadow: '0 2px 8px rgba(0,0,0,0.1)'
                          },
                          '&.Mui-focused': {
                            boxShadow: '0 4px 16px rgba(25, 118, 210, 0.2)'
                          }
                        }}
                      >
                        {financialYears.map((year) => (
                          <MenuItem key={year} value={year}>
                            {year}
                          </MenuItem>
                        ))}
                      </Select>
                    </FormControl>
                  </Stack>

                  <Stack spacing={3} direction={{ xs: 'column', sm: 'row' }}>
                    <TextField
                      fullWidth
                      label="Employer Name"
                      value={formData.employerName}
                      onChange={handleChange('employerName')}
                      variant="outlined"
                      sx={{
                        '& .MuiOutlinedInput-root': {
                          borderRadius: 2,
                          backgroundColor: 'white',
                          transition: 'all 0.2s ease-in-out',
                          '&:hover': {
                            boxShadow: '0 2px 8px rgba(0,0,0,0.1)'
                          },
                          '&.Mui-focused': {
                            boxShadow: '0 4px 16px rgba(25, 118, 210, 0.2)'
                          }
                        }
                      }}
                    />
                    <TextField
                      fullWidth
                      label="TAN Number"
                      value={formData.tan}
                      onChange={handleChange('tan')}
                      inputProps={{ maxLength: 10, style: { textTransform: 'uppercase' } }}
                      variant="outlined"
                      sx={{
                        '& .MuiOutlinedInput-root': {
                          borderRadius: 2,
                          backgroundColor: 'white',
                          transition: 'all 0.2s ease-in-out',
                          '&:hover': {
                            boxShadow: '0 2px 8px rgba(0,0,0,0.1)'
                          },
                          '&.Mui-focused': {
                            boxShadow: '0 4px 16px rgba(25, 118, 210, 0.2)'
                          }
                        }
                      }}
                    />
                  </Stack>
                </Stack>
              </AccordionDetails>
            </Accordion>
          </Grow>

          {/* Salary Income Section */}
          <Grow in timeout={500}>
            <Accordion 
              defaultExpanded 
              sx={{ 
                mb: 3,
                borderRadius: 2,
                boxShadow: '0 2px 12px rgba(0,0,0,0.06)',
                border: '1px solid rgba(0,0,0,0.08)',
                '&:before': { display: 'none' },
                overflow: 'hidden'
              }}
            >
              <AccordionSummary 
                expandIcon={<ExpandMoreIcon sx={{ color: 'success.main' }} />}
                sx={{ 
                  background: 'linear-gradient(135deg, #e8f5e8 0%, #c8e6c9 100%)',
                  borderBottom: '1px solid rgba(0,0,0,0.08)',
                  py: 2,
                  '&.Mui-expanded': {
                    minHeight: 64
                  }
                }}
              >
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                  <WorkIcon sx={{ color: 'success.main', fontSize: 28 }} />
                  <Box>
                    <Typography variant="h6" sx={{ fontWeight: 600, color: 'success.main' }}>
                      Salary Income
                    </Typography>
                    <Typography variant="caption" sx={{ color: 'text.secondary' }}>
                      Employment income and benefits
                    </Typography>
                  </Box>
                </Box>
              </AccordionSummary>
              <AccordionDetails sx={{ p: 3, backgroundColor: 'success.50' }}>
                <Stack spacing={3}>
                  <Stack spacing={3} direction={{ xs: 'column', sm: 'row' }}>
                    <TextField
                      fullWidth
                      label="Salary (Section 17)"
                      type="number"
                      value={formData.salarySection17}
                      onChange={handleChange('salarySection17')}
                      error={!!errors.salarySection17}
                      helperText={errors.salarySection17}
                      variant="outlined"
                      sx={{
                        '& .MuiOutlinedInput-root': {
                          borderRadius: 2,
                          backgroundColor: 'white',
                          transition: 'all 0.2s ease-in-out',
                          '&:hover': {
                            boxShadow: '0 2px 8px rgba(0,0,0,0.1)'
                          },
                          '&.Mui-focused': {
                            boxShadow: '0 4px 16px rgba(46, 125, 50, 0.2)'
                          }
                        }
                      }}
                    />
                    <TextField
                      fullWidth
                      label="Perquisites"
                      type="number"
                      value={formData.perquisites}
                      onChange={handleChange('perquisites')}
                      variant="outlined"
                      sx={{
                        '& .MuiOutlinedInput-root': {
                          borderRadius: 2,
                          backgroundColor: 'white',
                          transition: 'all 0.2s ease-in-out',
                          '&:hover': {
                            boxShadow: '0 2px 8px rgba(0,0,0,0.1)'
                          },
                          '&.Mui-focused': {
                            boxShadow: '0 4px 16px rgba(46, 125, 50, 0.2)'
                          }
                        }
                      }}
                    />
                  </Stack>

                  <Stack spacing={3} direction={{ xs: 'column', sm: 'row' }}>
                    <TextField
                      fullWidth
                      label="Profits in Lieu"
                      type="number"
                      value={formData.profitsInLieu}
                      onChange={handleChange('profitsInLieu')}
                      variant="outlined"
                      sx={{
                        '& .MuiOutlinedInput-root': {
                          borderRadius: 2,
                          backgroundColor: 'white',
                          transition: 'all 0.2s ease-in-out',
                          '&:hover': {
                            boxShadow: '0 2px 8px rgba(0,0,0,0.1)'
                          },
                          '&.Mui-focused': {
                            boxShadow: '0 4px 16px rgba(46, 125, 50, 0.2)'
                          }
                        }
                      }}
                    />
                    <Box sx={{ flex: 1 }} /> {/* Empty space to balance the layout */}
                  </Stack>
                </Stack>
              </AccordionDetails>
            </Accordion>
          </Grow>

          {/* Interest Income Section */}
          <Grow in timeout={700}>
            <Accordion 
              sx={{ 
                mb: 3,
                borderRadius: 2,
                boxShadow: '0 2px 12px rgba(0,0,0,0.06)',
                border: '1px solid rgba(0,0,0,0.08)',
                '&:before': { display: 'none' },
                overflow: 'hidden'
              }}
            >
              <AccordionSummary 
                expandIcon={<ExpandMoreIcon sx={{ color: 'info.main' }} />}
                sx={{ 
                  background: 'linear-gradient(135deg, #e3f2fd 0%, #bbdefb 100%)',
                  borderBottom: '1px solid rgba(0,0,0,0.08)',
                  py: 2,
                  '&.Mui-expanded': {
                    minHeight: 64
                  }
                }}
              >
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                  <SavingsIcon sx={{ color: 'info.main', fontSize: 28 }} />
                  <Box>
                    <Typography variant="h6" sx={{ fontWeight: 600, color: 'info.main' }}>
                      Interest Income
                    </Typography>
                    <Typography variant="caption" sx={{ color: 'text.secondary' }}>
                      Income from savings and deposits
                    </Typography>
                  </Box>
                </Box>
              </AccordionSummary>
              <AccordionDetails sx={{ p: 3, backgroundColor: 'info.50' }}>
                <Stack spacing={3}>
                  <Stack spacing={3} direction={{ xs: 'column', sm: 'row' }}>
                    <TextField
                      fullWidth
                      label="Interest on Savings Account"
                      type="number"
                      value={formData.interestOnSavings}
                      onChange={handleChange('interestOnSavings')}
                      variant="outlined"
                      sx={{
                        '& .MuiOutlinedInput-root': {
                          borderRadius: 2,
                          backgroundColor: 'white',
                          transition: 'all 0.2s ease-in-out',
                          '&:hover': {
                            boxShadow: '0 2px 8px rgba(0,0,0,0.1)'
                          },
                          '&.Mui-focused': {
                            boxShadow: '0 4px 16px rgba(2, 136, 209, 0.2)'
                          }
                        }
                      }}
                    />
                    <TextField
                      fullWidth
                      label="Interest on Fixed Deposits"
                      type="number"
                      value={formData.interestOnFixedDeposits}
                      onChange={handleChange('interestOnFixedDeposits')}
                      variant="outlined"
                      sx={{
                        '& .MuiOutlinedInput-root': {
                          borderRadius: 2,
                          backgroundColor: 'white',
                          transition: 'all 0.2s ease-in-out',
                          '&:hover': {
                            boxShadow: '0 2px 8px rgba(0,0,0,0.1)'
                          },
                          '&.Mui-focused': {
                            boxShadow: '0 4px 16px rgba(2, 136, 209, 0.2)'
                          }
                        }
                      }}
                    />
                  </Stack>
                </Stack>
              </AccordionDetails>
            </Accordion>
          </Grow>

          {/* Dividend Income Section */}
          <Grow in timeout={900}>
            <Accordion 
              sx={{ 
                mb: 3,
                borderRadius: 2,
                boxShadow: '0 2px 12px rgba(0,0,0,0.06)',
                border: '1px solid rgba(0,0,0,0.08)',
                '&:before': { display: 'none' },
                overflow: 'hidden'
              }}
            >
              <AccordionSummary 
                expandIcon={<ExpandMoreIcon sx={{ color: 'warning.main' }} />}
                sx={{ 
                  background: 'linear-gradient(135deg, #fff8e1 0%, #ffecb3 100%)',
                  borderBottom: '1px solid rgba(0,0,0,0.08)',
                  py: 2,
                  '&.Mui-expanded': {
                    minHeight: 64
                  }
                }}
              >
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                  <TrendingUpIcon sx={{ color: 'warning.main', fontSize: 28 }} />
                  <Box>
                    <Typography variant="h6" sx={{ fontWeight: 600, color: 'warning.main' }}>
                      Dividend Income
                    </Typography>
                    <Typography variant="caption" sx={{ color: 'text.secondary' }}>
                      Income from investments
                    </Typography>
                  </Box>
                </Box>
              </AccordionSummary>
              <AccordionDetails sx={{ p: 3, backgroundColor: 'warning.50' }}>
                <Stack spacing={3}>
                  <Stack spacing={3} direction={{ xs: 'column', sm: 'row' }}>
                    <TextField
                      fullWidth
                      label="Dividend Income"
                      type="number"
                      value={formData.dividendIncome}
                      onChange={handleChange('dividendIncome')}
                      variant="outlined"
                      sx={{
                        '& .MuiOutlinedInput-root': {
                          borderRadius: 2,
                          backgroundColor: 'white',
                          transition: 'all 0.2s ease-in-out',
                          '&:hover': {
                            boxShadow: '0 2px 8px rgba(0,0,0,0.1)'
                          },
                          '&.Mui-focused': {
                            boxShadow: '0 4px 16px rgba(237, 108, 2, 0.2)'
                          }
                        }
                      }}
                    />
                    <Box sx={{ flex: 1 }} /> {/* Empty space to balance the layout */}
                  </Stack>
                </Stack>
              </AccordionDetails>
            </Accordion>
          </Grow>

          {/* Capital Gains Section */}
          <Grow in timeout={1000}>
            <Accordion 
              sx={{ 
                mb: 3,
                borderRadius: 2,
                boxShadow: '0 2px 12px rgba(0,0,0,0.06)',
                border: '1px solid rgba(0,0,0,0.08)',
                '&:before': { display: 'none' },
                overflow: 'hidden'
              }}
            >
              <AccordionSummary 
                expandIcon={<ExpandMoreIcon sx={{ color: 'success.main' }} />}
                sx={{ 
                  background: 'linear-gradient(135deg, #e8f5e8 0%, #c8e6c9 100%)',
                  borderBottom: '1px solid rgba(0,0,0,0.08)',
                  py: 2,
                  '&.Mui-expanded': {
                    minHeight: 64
                  }
                }}
              >
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                  <ShowChartIcon sx={{ color: 'success.main', fontSize: 28 }} />
                  <Box>
                    <Typography variant="h6" sx={{ fontWeight: 600, color: 'success.main' }}>
                      Capital Gains
                    </Typography>
                    <Typography variant="caption" sx={{ color: 'text.secondary' }}>
                      Gains from Stocks, Mutual Funds, FnO & Others
                    </Typography>
                  </Box>
                </Box>
              </AccordionSummary>
              <AccordionDetails sx={{ p: 3, backgroundColor: 'success.50' }}>
                <Stack spacing={4}>
                  
                  {/* Stocks, Mutual Funds, F&O Section */}
                  <Box>
                    <Typography variant="h6" sx={{ color: 'success.main', fontWeight: 600, mb: 2, display: 'flex', alignItems: 'center', gap: 1 }}>
                      <TrendingUpIcon sx={{ fontSize: 20 }} />
                      Stocks, Mutual Funds, Futures & Options (F&O) and Others
                    </Typography>
                    <Typography variant="body2" sx={{ color: 'text.secondary', mb: 2 }}>
                      Easy auto-processing of your Gains from selling of Stocks, Mutual Funds, US Stocks, Land, Bonds, RSUs, Jewellery and more.
                    </Typography>
                    <Stack spacing={3}>
                      <Stack spacing={3} direction={{ xs: 'column', sm: 'row' }}>
                        <TextField
                          fullWidth
                          label="Stocks STCG"
                          type="number"
                          value={formData.stocksSTCG}
                          onChange={handleChange('stocksSTCG')}
                          variant="outlined"
                          helperText="15% tax rate (holding < 1 year)"
                          sx={{
                            '& .MuiOutlinedInput-root': {
                              borderRadius: 2,
                              backgroundColor: 'white',
                              transition: 'all 0.2s ease-in-out',
                              '&:hover': {
                                boxShadow: '0 2px 8px rgba(0,0,0,0.1)'
                              },
                              '&.Mui-focused': {
                                boxShadow: '0 4px 16px rgba(46, 125, 50, 0.2)'
                              }
                            }
                          }}
                        />
                        <TextField
                          fullWidth
                          label="Stocks LTCG"
                          type="number"
                          value={formData.stocksLTCG}
                          onChange={handleChange('stocksLTCG')}
                          variant="outlined"
                          helperText="10% tax rate above ₹1L (holding ≥ 1 year)"
                          sx={{
                            '& .MuiOutlinedInput-root': {
                              borderRadius: 2,
                              backgroundColor: 'white',
                              transition: 'all 0.2s ease-in-out',
                              '&:hover': {
                                boxShadow: '0 2px 8px rgba(0,0,0,0.1)'
                              },
                              '&.Mui-focused': {
                                boxShadow: '0 4px 16px rgba(46, 125, 50, 0.2)'
                              }
                            }
                          }}
                        />
                      </Stack>
                      <Stack spacing={3} direction={{ xs: 'column', sm: 'row' }}>
                        <TextField
                          fullWidth
                          label="Mutual Funds STCG"
                          type="number"
                          value={formData.mutualFundsSTCG}
                          onChange={handleChange('mutualFundsSTCG')}
                          variant="outlined"
                          helperText="15% for equity funds, slab rate for debt funds"
                          sx={{
                            '& .MuiOutlinedInput-root': {
                              borderRadius: 2,
                              backgroundColor: 'white',
                              transition: 'all 0.2s ease-in-out',
                              '&:hover': {
                                boxShadow: '0 2px 8px rgba(0,0,0,0.1)'
                              },
                              '&.Mui-focused': {
                                boxShadow: '0 4px 16px rgba(46, 125, 50, 0.2)'
                              }
                            }
                          }}
                        />
                        <TextField
                          fullWidth
                          label="Mutual Funds LTCG"
                          type="number"
                          value={formData.mutualFundsLTCG}
                          onChange={handleChange('mutualFundsLTCG')}
                          variant="outlined"
                          helperText="10% for equity funds (above ₹1L), 20% for debt funds"
                          sx={{
                            '& .MuiOutlinedInput-root': {
                              borderRadius: 2,
                              backgroundColor: 'white',
                              transition: 'all 0.2s ease-in-out',
                              '&:hover': {
                                boxShadow: '0 2px 8px rgba(0,0,0,0.1)'
                              },
                              '&.Mui-focused': {
                                boxShadow: '0 4px 16px rgba(46, 125, 50, 0.2)'
                              }
                            }
                          }}
                        />
                      </Stack>
                      <TextField
                        fullWidth
                        label="F&O (Futures & Options) Gains"
                        type="number"
                        value={formData.fnoGains}
                        onChange={handleChange('fnoGains')}
                        variant="outlined"
                        helperText="Taxed as business income (slab rates)"
                        sx={{
                          '& .MuiOutlinedInput-root': {
                            borderRadius: 2,
                            backgroundColor: 'white',
                            transition: 'all 0.2s ease-in-out',
                            '&:hover': {
                              boxShadow: '0 2px 8px rgba(0,0,0,0.1)'
                            },
                            '&.Mui-focused': {
                              boxShadow: '0 4px 16px rgba(46, 125, 50, 0.2)'
                            }
                          }
                        }}
                      />
                    </Stack>
                  </Box>

                  <Divider />

                  {/* Sale of Land or Building Section */}
                  <Box>
                    <Typography variant="h6" sx={{ color: 'success.main', fontWeight: 600, mb: 2, display: 'flex', alignItems: 'center', gap: 1 }}>
                      🏠 Sale of Land or Building
                    </Typography>
                    <Typography variant="body2" sx={{ color: 'text.secondary', mb: 2 }}>
                      Gains from sale of land, residential or commercial buildings and other real estate properties
                    </Typography>
                    <Stack spacing={3} direction={{ xs: 'column', sm: 'row' }}>
                      <TextField
                        fullWidth
                        label="Real Estate STCG"
                        type="number"
                        value={formData.realEstateSTCG}
                        onChange={handleChange('realEstateSTCG')}
                        variant="outlined"
                        helperText="Taxed as per slab rates (holding < 24 months)"
                        sx={{
                          '& .MuiOutlinedInput-root': {
                            borderRadius: 2,
                            backgroundColor: 'white',
                            transition: 'all 0.2s ease-in-out',
                            '&:hover': {
                              boxShadow: '0 2px 8px rgba(0,0,0,0.1)'
                            },
                            '&.Mui-focused': {
                              boxShadow: '0 4px 16px rgba(46, 125, 50, 0.2)'
                            }
                          }
                        }}
                      />
                      <TextField
                        fullWidth
                        label="Real Estate LTCG"
                        type="number"
                        value={formData.realEstateLTCG}
                        onChange={handleChange('realEstateLTCG')}
                        variant="outlined"
                        helperText="20% with indexation (holding ≥ 24 months)"
                        sx={{
                          '& .MuiOutlinedInput-root': {
                            borderRadius: 2,
                            backgroundColor: 'white',
                            transition: 'all 0.2s ease-in-out',
                            '&:hover': {
                              boxShadow: '0 2px 8px rgba(0,0,0,0.1)'
                            },
                            '&.Mui-focused': {
                              boxShadow: '0 4px 16px rgba(46, 125, 50, 0.2)'
                            }
                          }
                        }}
                      />
                    </Stack>
                  </Box>

                  <Divider />

                  {/* Bonds and Debentures Section */}
                  <Box>
                    <Typography variant="h6" sx={{ color: 'success.main', fontWeight: 600, mb: 2, display: 'flex', alignItems: 'center', gap: 1 }}>
                      🏦 Bonds and Debentures
                    </Typography>
                    <Typography variant="body2" sx={{ color: 'text.secondary', mb: 2 }}>
                      Gains or losses from Bonds and Debentures including Government, Corporate and Tax-free Bonds
                    </Typography>
                    <Stack spacing={3} direction={{ xs: 'column', sm: 'row' }}>
                      <TextField
                        fullWidth
                        label="Bonds STCG"
                        type="number"
                        value={formData.bondsSTCG}
                        onChange={handleChange('bondsSTCG')}
                        variant="outlined"
                        helperText="Taxed as per slab rates (holding < 36 months)"
                        sx={{
                          '& .MuiOutlinedInput-root': {
                            borderRadius: 2,
                            backgroundColor: 'white',
                            transition: 'all 0.2s ease-in-out',
                            '&:hover': {
                              boxShadow: '0 2px 8px rgba(0,0,0,0.1)'
                            },
                            '&.Mui-focused': {
                              boxShadow: '0 4px 16px rgba(46, 125, 50, 0.2)'
                            }
                          }
                        }}
                      />
                      <TextField
                        fullWidth
                        label="Bonds LTCG"
                        type="number"
                        value={formData.bondsLTCG}
                        onChange={handleChange('bondsLTCG')}
                        variant="outlined"
                        helperText="20% with indexation (holding ≥ 36 months)"
                        sx={{
                          '& .MuiOutlinedInput-root': {
                            borderRadius: 2,
                            backgroundColor: 'white',
                            transition: 'all 0.2s ease-in-out',
                            '&:hover': {
                              boxShadow: '0 2px 8px rgba(0,0,0,0.1)'
                            },
                            '&.Mui-focused': {
                              boxShadow: '0 4px 16px rgba(46, 125, 50, 0.2)'
                            }
                          }
                        }}
                      />
                    </Stack>
                  </Box>

                  <Divider />

                  {/* Gold, Jewellery and Others Section */}
                  <Box>
                    <Typography variant="h6" sx={{ color: 'success.main', fontWeight: 600, mb: 2, display: 'flex', alignItems: 'center', gap: 1 }}>
                      💎 Gold, Jewellery and Others
                    </Typography>
                    <Typography variant="body2" sx={{ color: 'text.secondary', mb: 2 }}>
                      Gold, Jewellery, Paintings, Sculptures, Archaeological Collections, and any other relevant capital assets
                    </Typography>
                    <Stack spacing={3} direction={{ xs: 'column', sm: 'row' }}>
                      <TextField
                        fullWidth
                        label="Gold/Jewellery STCG"
                        type="number"
                        value={formData.goldSTCG}
                        onChange={handleChange('goldSTCG')}
                        variant="outlined"
                        helperText="Taxed as per slab rates (holding < 36 months)"
                        sx={{
                          '& .MuiOutlinedInput-root': {
                            borderRadius: 2,
                            backgroundColor: 'white',
                            transition: 'all 0.2s ease-in-out',
                            '&:hover': {
                              boxShadow: '0 2px 8px rgba(0,0,0,0.1)'
                            },
                            '&.Mui-focused': {
                              boxShadow: '0 4px 16px rgba(46, 125, 50, 0.2)'
                            }
                          }
                        }}
                      />
                      <TextField
                        fullWidth
                        label="Gold/Jewellery LTCG"
                        type="number"
                        value={formData.goldLTCG}
                        onChange={handleChange('goldLTCG')}
                        variant="outlined"
                        helperText="20% with indexation (holding ≥ 36 months)"
                        sx={{
                          '& .MuiOutlinedInput-root': {
                            borderRadius: 2,
                            backgroundColor: 'white',
                            transition: 'all 0.2s ease-in-out',
                            '&:hover': {
                              boxShadow: '0 2px 8px rgba(0,0,0,0.1)'
                            },
                            '&.Mui-focused': {
                              boxShadow: '0 4px 16px rgba(46, 125, 50, 0.2)'
                            }
                          }
                        }}
                      />
                    </Stack>
                  </Box>

                  <Divider />

                  {/* Cryptocurrency Section */}
                  <Box>
                    <Typography variant="h6" sx={{ color: 'warning.main', fontWeight: 600, mb: 2, display: 'flex', alignItems: 'center', gap: 1 }}>
                      ₿ Cryptocurrency
                    </Typography>
                    <Typography variant="body2" sx={{ color: 'text.secondary', mb: 2 }}>
                      Gains from cryptocurrency transactions (Bitcoin, Ethereum, etc.)
                    </Typography>
                    <TextField
                      fullWidth
                      label="Cryptocurrency Gains"
                      type="number"
                      value={formData.cryptoGains}
                      onChange={handleChange('cryptoGains')}
                      variant="outlined"
                      helperText="30% flat tax rate (no indexation benefit)"
                      sx={{
                        '& .MuiOutlinedInput-root': {
                          borderRadius: 2,
                          backgroundColor: 'white',
                          transition: 'all 0.2s ease-in-out',
                          '&:hover': {
                            boxShadow: '0 2px 8px rgba(0,0,0,0.1)'
                          },
                          '&.Mui-focused': {
                            boxShadow: '0 4px 16px rgba(255, 152, 0, 0.2)'
                          }
                        }
                      }}
                    />
                  </Box>

                </Stack>
              </AccordionDetails>
            </Accordion>
          </Grow>

          {/* Deductions */}
          <Grow in timeout={1100}>
            <Accordion 
              sx={{ 
                mb: 3,
                borderRadius: 2,
                boxShadow: '0 2px 12px rgba(0,0,0,0.06)',
                border: '1px solid rgba(0,0,0,0.08)',
                '&:before': { display: 'none' },
                overflow: 'hidden'
              }}
            >
              <AccordionSummary 
                expandIcon={<ExpandMoreIcon sx={{ color: 'error.main' }} />}
                sx={{ 
                  background: 'linear-gradient(135deg, #ffebee 0%, #ffcdd2 100%)',
                  borderBottom: '1px solid rgba(0,0,0,0.08)',
                  py: 2,
                  '&.Mui-expanded': {
                    minHeight: 64
                  }
                }}
              >
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                  <ReceiptIcon sx={{ color: 'error.main', fontSize: 28 }} />
                  <Box>
                    <Typography variant="h6" sx={{ fontWeight: 600, color: 'error.main' }}>
                      Deductions
                    </Typography>
                    <Typography variant="caption" sx={{ color: 'text.secondary' }}>
                      Tax deductions and TDS
                    </Typography>
                  </Box>
                </Box>
              </AccordionSummary>
              <AccordionDetails sx={{ p: 3, backgroundColor: 'error.50' }}>
                <Stack spacing={3}>
                  <Stack spacing={3} direction={{ xs: 'column', sm: 'row' }}>
                    <TextField
                      fullWidth
                      label="Standard Deduction"
                      type="number"
                      value={formData.standardDeduction}
                      onChange={handleChange('standardDeduction')}
                      variant="outlined"
                      sx={{
                        '& .MuiOutlinedInput-root': {
                          borderRadius: 2,
                          backgroundColor: 'white',
                          transition: 'all 0.2s ease-in-out',
                          '&:hover': {
                            boxShadow: '0 2px 8px rgba(0,0,0,0.1)'
                          },
                          '&.Mui-focused': {
                            boxShadow: '0 4px 16px rgba(211, 47, 47, 0.2)'
                          }
                        }
                      }}
                    />
                    <TextField
                      fullWidth
                      label="Professional Tax"
                      type="number"
                      value={formData.professionalTax}
                      onChange={handleChange('professionalTax')}
                      variant="outlined"
                      sx={{
                        '& .MuiOutlinedInput-root': {
                          borderRadius: 2,
                          backgroundColor: 'white',
                          transition: 'all 0.2s ease-in-out',
                          '&:hover': {
                            boxShadow: '0 2px 8px rgba(0,0,0,0.1)'
                          },
                          '&.Mui-focused': {
                            boxShadow: '0 4px 16px rgba(211, 47, 47, 0.2)'
                          }
                        }
                      }}
                    />
                  </Stack>

                  <Stack spacing={3} direction={{ xs: 'column', sm: 'row' }}>
                    <TextField
                      fullWidth
                      label="Total Tax Deducted (TDS)"
                      type="number"
                      value={formData.totalTaxDeducted}
                      onChange={handleChange('totalTaxDeducted')}
                      variant="outlined"
                      sx={{
                        '& .MuiOutlinedInput-root': {
                          borderRadius: 2,
                          backgroundColor: 'white',
                          transition: 'all 0.2s ease-in-out',
                          '&:hover': {
                            boxShadow: '0 2px 8px rgba(0,0,0,0.1)'
                          },
                          '&.Mui-focused': {
                            boxShadow: '0 4px 16px rgba(211, 47, 47, 0.2)'
                          }
                        }
                      }}
                    />
                    <Box sx={{ flex: 1 }} /> {/* Empty space to balance the layout */}
                  </Stack>
                </Stack>
              </AccordionDetails>
            </Accordion>
          </Grow>

          {/* Calculated Values */}
          <Grow in timeout={1300}>
            <Accordion 
              defaultExpanded 
              sx={{ 
                mb: 4,
                borderRadius: 2,
                boxShadow: '0 4px 20px rgba(102, 126, 234, 0.15)',
                border: '2px solid rgba(102, 126, 234, 0.2)',
                '&:before': { display: 'none' },
                overflow: 'hidden'
              }}
            >
              <AccordionSummary 
                expandIcon={<ExpandMoreIcon sx={{ color: 'primary.main' }} />}
                sx={{ 
                  background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
                  color: 'white',
                  py: 2,
                  '&.Mui-expanded': {
                    minHeight: 64
                  }
                }}
              >
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                  <CalculateIcon sx={{ color: 'white', fontSize: 28 }} />
                  <Box>
                    <Typography variant="h6" sx={{ fontWeight: 600, color: 'white' }}>
                      Calculated Values
                    </Typography>
                    <Typography variant="caption" sx={{ color: 'rgba(255,255,255,0.8)' }}>
                      Real-time calculations based on your input
                    </Typography>
                  </Box>
                </Box>
              </AccordionSummary>
              <AccordionDetails sx={{ p: 4, backgroundColor: 'rgba(102, 126, 234, 0.02)' }}>
                <Stack spacing={3} direction={{ xs: 'column', md: 'row' }}>
                  <Card sx={{ 
                    flex: 1, 
                    borderRadius: 3,
                    background: 'linear-gradient(135deg, #e3f2fd 0%, #bbdefb 100%)',
                    border: '1px solid rgba(33, 150, 243, 0.2)',
                    transition: 'all 0.3s ease-in-out',
                    '&:hover': {
                      transform: 'translateY(-2px)',
                      boxShadow: '0 8px 25px rgba(33, 150, 243, 0.2)'
                    }
                  }}>
                    <CardContent sx={{ textAlign: 'center', p: 3 }}>
                      <AccountBalanceWalletIcon sx={{ fontSize: 40, color: 'info.main', mb: 1 }} />
                      <Typography variant="h6" sx={{ fontWeight: 600, color: 'info.main', mb: 1 }}>
                        Gross Salary
                      </Typography>
                      <Typography variant="h5" sx={{ fontWeight: 700, color: 'info.dark' }}>
                        ₹{grossSalary.toLocaleString()}
                      </Typography>
                      <Chip 
                        label="Base Income" 
                        size="small" 
                        sx={{ 
                          mt: 1, 
                          backgroundColor: 'info.main', 
                          color: 'white',
                          fontWeight: 500
                        }} 
                      />
                    </CardContent>
                  </Card>

                  <Card sx={{ 
                    flex: 1, 
                    borderRadius: 3,
                    background: 'linear-gradient(135deg, #e8f5e8 0%, #c8e6c9 100%)',
                    border: '1px solid rgba(76, 175, 80, 0.2)',
                    transition: 'all 0.3s ease-in-out',
                    '&:hover': {
                      transform: 'translateY(-2px)',
                      boxShadow: '0 8px 25px rgba(76, 175, 80, 0.2)'
                    }
                  }}>
                    <CardContent sx={{ textAlign: 'center', p: 3 }}>
                      <ShowChartIcon sx={{ fontSize: 40, color: 'success.main', mb: 1 }} />
                      <Typography variant="h6" sx={{ fontWeight: 600, color: 'success.main', mb: 1 }}>
                        Capital Gains
                      </Typography>
                      <Typography variant="h5" sx={{ fontWeight: 700, color: 'success.dark' }}>
                        ₹{totalCapitalGains.toLocaleString()}
                      </Typography>
                      <Chip 
                        label="Investment Income" 
                        size="small" 
                        sx={{ 
                          mt: 1, 
                          backgroundColor: 'success.main', 
                          color: 'white',
                          fontWeight: 500
                        }} 
                      />
                    </CardContent>
                  </Card>

                  <Card sx={{ 
                    flex: 1, 
                    borderRadius: 3,
                    background: 'linear-gradient(135deg, #f3e5f5 0%, #e1bee7 100%)',
                    border: '1px solid rgba(156, 39, 176, 0.2)',
                    transition: 'all 0.3s ease-in-out',
                    '&:hover': {
                      transform: 'translateY(-2px)',
                      boxShadow: '0 8px 25px rgba(156, 39, 176, 0.2)'
                    }
                  }}>
                    <CardContent sx={{ textAlign: 'center', p: 3 }}>
                      <MonetizationOnIcon sx={{ fontSize: 40, color: 'secondary.main', mb: 1 }} />
                      <Typography variant="h6" sx={{ fontWeight: 600, color: 'secondary.main', mb: 1 }}>
                        Total Income
                      </Typography>
                      <Typography variant="h5" sx={{ fontWeight: 700, color: 'secondary.dark' }}>
                        ₹{totalIncome.toLocaleString()}
                      </Typography>
                      <Chip 
                        label="All Sources" 
                        size="small" 
                        sx={{ 
                          mt: 1, 
                          backgroundColor: 'secondary.main', 
                          color: 'white',
                          fontWeight: 500
                        }} 
                      />
                    </CardContent>
                  </Card>

                  <Card sx={{ 
                    flex: 1, 
                    borderRadius: 3,
                    background: 'linear-gradient(135deg, #e8f5e8 0%, #c8e6c9 100%)',
                    border: '2px solid rgba(76, 175, 80, 0.3)',
                    transition: 'all 0.3s ease-in-out',
                    '&:hover': {
                      transform: 'translateY(-2px)',
                      boxShadow: '0 8px 25px rgba(76, 175, 80, 0.3)'
                    }
                  }}>
                    <CardContent sx={{ textAlign: 'center', p: 3 }}>
                      <AssessmentIcon sx={{ fontSize: 40, color: 'success.main', mb: 1 }} />
                      <Typography variant="h6" sx={{ fontWeight: 600, color: 'success.main', mb: 1 }}>
                        Taxable Income
                      </Typography>
                      <Typography variant="h5" sx={{ fontWeight: 700, color: 'success.dark' }}>
                        ₹{taxableIncome.toLocaleString()}
                      </Typography>
                      <Chip 
                        label="After Deductions" 
                        size="small" 
                        sx={{ 
                          mt: 1, 
                          backgroundColor: 'success.main', 
                          color: 'white',
                          fontWeight: 500
                        }} 
                      />
                    </CardContent>
                  </Card>
                </Stack>
              </AccordionDetails>
            </Accordion>
          </Grow>

          {/* Calculate Button */}
          <Fade in timeout={1500}>
            <Box sx={{ mt: 4, display: 'flex', justifyContent: 'center' }}>
              <Button
                variant="contained"
                size="large"
                onClick={handleCalculate}
                disabled={taxableIncome <= 0}
                startIcon={<CalculateIcon />}
                sx={{ 
                  px: 6, 
                  py: 2, 
                  fontSize: '1.2rem',
                  fontWeight: 600,
                  borderRadius: 4,
                  background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
                  boxShadow: '0 8px 25px rgba(102, 126, 234, 0.4)',
                  transition: 'all 0.3s ease-in-out',
                  textTransform: 'none',
                  '&:hover': {
                    background: 'linear-gradient(135deg, #5a6fd8 0%, #6a4190 100%)',
                    transform: 'translateY(-2px)',
                    boxShadow: '0 12px 35px rgba(102, 126, 234, 0.5)'
                  },
                  '&:disabled': {
                    background: 'linear-gradient(135deg, #e0e0e0 0%, #bdbdbd 100%)',
                    color: 'rgba(0,0,0,0.4)',
                    boxShadow: 'none'
                  }
                }}
              >
                Calculate Tax
              </Button>
            </Box>
          </Fade>
        </Box>
      </CardContent>
    </Card>
    </Box>
  );
};

export default TaxDataInput;
