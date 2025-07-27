import React, { useState } from 'react';
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
  Grow
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
  });

  const [errors, setErrors] = useState<Partial<Record<keyof TaxData, string>>>({});

  const handleChange = (field: keyof TaxData) => (event: React.ChangeEvent<HTMLInputElement>) => {
    const value = event.target.type === 'number' ? parseFloat(event.target.value) || 0 : event.target.value;
    setFormData(prev => ({ ...prev, [field]: value }));
    
    // Clear error when user starts typing
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
  const totalIncome = grossSalary + formData.interestOnSavings + formData.interestOnFixedDeposits + formData.dividendIncome;
  const taxableIncome = Math.max(0, totalIncome - formData.standardDeduction - formData.professionalTax);

  return (
    <Box sx={{ maxWidth: 900, mx: 'auto', mt: 4 }}>
      {/* Header Card with Gradient */}
      <Card sx={{ 
        mb: 3,
        background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
        color: 'white',
        borderRadius: 3,
        boxShadow: '0 8px 32px rgba(102, 126, 234, 0.3)'
      }}>
        <CardContent sx={{ textAlign: 'center', py: 4 }}>
          <AssessmentIcon sx={{ fontSize: 48, mb: 2, opacity: 0.9 }} />
          <Typography variant="h4" gutterBottom sx={{ fontWeight: 700, mb: 1 }}>
            Tax Data Input
          </Typography>
          <Typography variant="subtitle1" sx={{ opacity: 0.9, maxWidth: 600, mx: 'auto' }}>
            Enter your financial information to calculate accurate tax liability and generate ITR
          </Typography>
        </CardContent>
      </Card>

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
                    <TextField
                      fullWidth
                      label="Assessment Year"
                      value={formData.assessmentYear}
                      onChange={handleChange('assessmentYear')}
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
                      label="Financial Year"
                      value={formData.financialYear}
                      onChange={handleChange('financialYear')}
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
