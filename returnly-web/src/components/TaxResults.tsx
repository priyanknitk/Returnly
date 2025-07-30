import React from 'react';
import {
  Card,
  CardContent,
  Typography,
  Box,
  Alert,
  Divider,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Stack,
  Button,
  Chip,
  Fade,
  Grow
} from '@mui/material';
import { 
  Assessment, 
  Receipt, 
  TrendingUp, 
  AccountBalance, 
  MonetizationOn,
  CheckCircle,
  Warning,
  Info,
  FileDownload
} from '@mui/icons-material';

interface TaxSlabCalculation {
  slabDescription: string;
  incomeInSlab: number;
  taxRate: number;
  taxAmount: number;
}

export interface TaxCalculationResult {
  taxableIncome: number;
  financialYear: string;
  taxRegime: string;
  totalTax: number;
  surcharge: number;
  surchargeRate: number;
  healthAndEducationCess: number;
  totalTaxWithCess: number;
  totalTaxWithAdvanceTaxPenalties: number;
  effectiveTaxRate: number;
  taxBreakdown: TaxSlabCalculation[];
  // Advance Tax Penalty fields
  section234AInterest: number;
  section234BInterest: number;
  section234CInterest: number;
  totalAdvanceTaxPenalties: number;
  hasAdvanceTaxPenalties: boolean;
}

export interface TaxRefundCalculation {
  totalTaxLiability: number;
  tdsDeducted: number;
  refundAmount: number;
  additionalTaxDue: number;
  isRefundDue: boolean;
}

interface TaxResultsProps {
  taxCalculation: TaxCalculationResult;
  refundCalculation: TaxRefundCalculation;
  onGenerateITR?: () => void;
  showITRButton?: boolean;
}

const TaxResults: React.FC<TaxResultsProps> = ({ 
  taxCalculation, 
  refundCalculation, 
  onGenerateITR, 
  showITRButton = false
}) => {
  const formatCurrency = (amount: number) => `₹${amount.toLocaleString()}`;

  return (
    <Box sx={{ maxWidth: 1400, mx: 'auto', mt: 2, px: { xs: 1, sm: 2, md: 3 } }}>
      {/* Header Card */}
      <Fade in timeout={600}>
        <Card sx={{ 
          mb: 4,
          background: 'linear-gradient(135deg, #4caf50 0%, #2e7d32 100%)',
          color: 'white',
          borderRadius: 3,
          boxShadow: '0 8px 32px rgba(76, 175, 80, 0.3)'
        }}>
          <CardContent sx={{ textAlign: 'center', py: 2.5 }}>
            <TrendingUp sx={{ fontSize: 28, mb: 1.5, opacity: 0.9 }} />
            <Typography variant="h6" gutterBottom sx={{ fontWeight: 700, mb: 1 }}>
              Tax Calculation Results
            </Typography>
            <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2} justifyContent="center" sx={{ mt: 3 }}>
              <Chip 
                label={`Financial Year: ${taxCalculation.financialYear}`}
                sx={{ 
                  backgroundColor: 'rgba(255,255,255,0.2)', 
                  color: 'white',
                  fontWeight: 600
                }} 
              />
              <Chip 
                label={`Tax Regime: ${taxCalculation.taxRegime}`}
                sx={{ 
                  backgroundColor: 'rgba(255,255,255,0.2)', 
                  color: 'white',
                  fontWeight: 600
                }} 
              />
              <Chip 
                label={`Effective Rate: ${taxCalculation.effectiveTaxRate.toFixed(2)}%`}
                sx={{ 
                  backgroundColor: 'rgba(255,255,255,0.2)', 
                  color: 'white',
                  fontWeight: 600
                }} 
              />
            </Stack>
          </CardContent>
        </Card>
      </Fade>

      {/* Summary Cards */}
      <Grow in timeout={800}>
        <Stack spacing={3} direction={{ xs: 'column', lg: 'row' }} sx={{ mb: 4 }}>
          <Card sx={{ 
            flex: 1, 
            borderRadius: 3,
            background: 'linear-gradient(135deg, #e3f2fd 0%, #bbdefb 100%)',
            border: '1px solid rgba(33, 150, 243, 0.2)',
            transition: 'all 0.3s ease-in-out',
            '&:hover': {
              transform: 'translateY(-4px)',
              boxShadow: '0 8px 25px rgba(33, 150, 243, 0.2)'
            }
          }}>
            <CardContent sx={{ textAlign: 'center', p: 3 }}>
              <MonetizationOn sx={{ fontSize: 32, color: 'info.main', mb: 2 }} />
              <Typography variant="body1" sx={{ fontWeight: 600, color: 'info.main', mb: 1, fontSize: '1.1rem' }}>
                Taxable Income
              </Typography>
              <Typography variant="h5" sx={{ fontWeight: 700, color: 'info.dark' }}>
                {formatCurrency(taxCalculation.taxableIncome)}
              </Typography>
              <Chip 
                label="After Deductions" 
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
            background: 'linear-gradient(135deg, #fff3e0 0%, #ffcc80 100%)',
            border: '1px solid rgba(255, 152, 0, 0.2)',
            transition: 'all 0.3s ease-in-out',
            '&:hover': {
              transform: 'translateY(-4px)',
              boxShadow: '0 8px 25px rgba(255, 152, 0, 0.2)'
            }
          }}>
            <CardContent sx={{ textAlign: 'center', p: 3 }}>
              <Receipt sx={{ fontSize: 32, color: 'warning.main', mb: 2 }} />
              <Typography variant="body1" sx={{ fontWeight: 600, color: 'warning.main', mb: 1, fontSize: '1.1rem' }}>
                Total Tax Liability
              </Typography>
              <Typography variant="h5" sx={{ fontWeight: 700, color: 'warning.dark' }}>
                {formatCurrency(taxCalculation.totalTaxWithAdvanceTaxPenalties)}
              </Typography>
              <Chip 
                label="Including Cess" 
                size="small" 
                sx={{ 
                  mt: 1, 
                  backgroundColor: 'warning.main', 
                  color: 'white',
                  fontWeight: 500
                }} 
              />
            </CardContent>
          </Card>

          <Card sx={{ 
            flex: 1, 
            borderRadius: 3,
            background: refundCalculation.isRefundDue 
              ? 'linear-gradient(135deg, #e8f5e8 0%, #c8e6c9 100%)'
              : 'linear-gradient(135deg, #ffebee 0%, #ffcdd2 100%)',
            border: refundCalculation.isRefundDue 
              ? '2px solid rgba(76, 175, 80, 0.3)'
              : '2px solid rgba(244, 67, 54, 0.3)',
            transition: 'all 0.3s ease-in-out',
            '&:hover': {
              transform: 'translateY(-4px)',
              boxShadow: refundCalculation.isRefundDue 
                ? '0 8px 25px rgba(76, 175, 80, 0.3)'
                : '0 8px 25px rgba(244, 67, 54, 0.3)'
            }
          }}>
            <CardContent sx={{ textAlign: 'center', p: 3 }}>
              {refundCalculation.isRefundDue ? (
                <CheckCircle sx={{ fontSize: 40, color: 'success.main', mb: 2 }} />
              ) : (
                <Warning sx={{ fontSize: 40, color: 'error.main', mb: 2 }} />
              )}
              <Typography variant="h6" sx={{ 
                fontWeight: 600, 
                color: refundCalculation.isRefundDue ? 'success.main' : 'error.main', 
                mb: 1 
              }}>
                {refundCalculation.isRefundDue ? 'Refund Due' : 'Additional Tax Due'}
              </Typography>
              <Typography variant="h4" sx={{ 
                fontWeight: 700, 
                color: refundCalculation.isRefundDue ? 'success.dark' : 'error.dark'
              }}>
                {formatCurrency(
                  refundCalculation.isRefundDue 
                    ? refundCalculation.refundAmount 
                    : refundCalculation.additionalTaxDue
                )}
              </Typography>
              <Chip 
                label={refundCalculation.isRefundDue ? 'You Get Money Back!' : 'Payment Required'} 
                size="small" 
                sx={{ 
                  mt: 1, 
                  backgroundColor: refundCalculation.isRefundDue ? 'success.main' : 'error.main', 
                  color: 'white',
                  fontWeight: 500
                }} 
              />
            </CardContent>
          </Card>
        </Stack>
      </Grow>

      {/* Tax Due Payment Section - ClearTax Style */}
      {!refundCalculation.isRefundDue && refundCalculation.additionalTaxDue > 0 && (
        <Grow in timeout={900}>
          <Card sx={{ 
            mb: 4,
            borderRadius: 2,
            border: '2px solid #2196F3',
            backgroundColor: '#E3F2FD'
          }}>
            <CardContent sx={{ py: 2, px: 3 }}>
              <Box sx={{ 
                display: 'flex', 
                alignItems: 'center',
                justifyContent: 'space-between'
              }}>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                  <Box sx={{
                    width: 20,
                    height: 20,
                    backgroundColor: '#FF9800',
                    borderRadius: '4px',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center'
                  }}>
                    <Receipt sx={{ fontSize: 14, color: 'white' }} />
                  </Box>
                  <Typography variant="body1" sx={{ fontWeight: 600, color: '#424242' }}>
                    You have a tax due of {formatCurrency(refundCalculation.additionalTaxDue)}
                  </Typography>
                </Box>
                <Typography variant="body2" sx={{ 
                  color: '#666666',
                  fontStyle: 'italic'
                }}>
                  Pay your tax due on the ITD website and come back here
                </Typography>
              </Box>
            </CardContent>
          </Card>
        </Grow>
      )}

      {/* Tax Breakdown Table */}
      <Grow in timeout={1000}>
        <Card sx={{ 
          mb: 4,
          borderRadius: 3,
          boxShadow: '0 4px 20px rgba(0,0,0,0.08)',
          overflow: 'hidden'
        }}>
          <CardContent sx={{ p: 0 }}>
            <Box sx={{ 
              background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
              color: 'white',
              p: 3
            }}>
              <Typography variant="h6" sx={{ fontWeight: 600, display: 'flex', alignItems: 'center', gap: 1 }}>
                <Assessment /> Tax Slab Breakdown
              </Typography>
              <Typography variant="body2" sx={{ opacity: 0.9 }}>
                Detailed breakdown of tax calculation by income slabs
              </Typography>
            </Box>
            
            <TableContainer sx={{ backgroundColor: 'white' }}>
              <Table>
                <TableHead>
                  <TableRow sx={{ backgroundColor: 'grey.50' }}>
                    <TableCell sx={{ fontWeight: 600, fontSize: '0.9rem' }}>Tax Slab</TableCell>
                    <TableCell align="right" sx={{ fontWeight: 600, fontSize: '0.9rem' }}>Income in Slab</TableCell>
                    <TableCell align="right" sx={{ fontWeight: 600, fontSize: '0.9rem' }}>Tax Rate</TableCell>
                    <TableCell align="right" sx={{ fontWeight: 600, fontSize: '0.9rem' }}>Tax Amount</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {taxCalculation.taxBreakdown.map((slab, index) => (
                    <TableRow 
                      key={index}
                      sx={{ 
                        '&:hover': { backgroundColor: 'grey.50' },
                        borderLeft: slab.taxAmount > 0 ? '4px solid' : 'none',
                        borderLeftColor: slab.taxAmount > 0 ? 'primary.main' : 'transparent'
                      }}
                    >
                      <TableCell sx={{ fontWeight: 500 }}>{slab.slabDescription}</TableCell>
                      <TableCell align="right" sx={{ fontFamily: 'monospace' }}>
                        {formatCurrency(slab.incomeInSlab)}
                      </TableCell>
                      <TableCell align="right">
                        <Chip 
                          label={`${slab.taxRate}%`} 
                          size="small"
                          color={slab.taxRate > 0 ? "primary" : "default"}
                          variant={slab.taxRate > 0 ? "filled" : "outlined"}
                        />
                      </TableCell>
                      <TableCell align="right" sx={{ 
                        fontFamily: 'monospace',
                        fontWeight: slab.taxAmount > 0 ? 600 : 400,
                        color: slab.taxAmount > 0 ? 'primary.main' : 'text.secondary'
                      }}>
                        {formatCurrency(slab.taxAmount)}
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          </CardContent>
        </Card>
      </Grow>

      {/* Detailed Tax Calculation - ClearTax Style */}
      <Grow in timeout={1200}>
        <Card sx={{ 
          mb: 4,
          borderRadius: 3,
          boxShadow: '0 4px 20px rgba(0,0,0,0.08)',
          border: '1px solid rgba(0,0,0,0.05)'
        }}>
          <CardContent sx={{ p: 3 }}>
            <Typography variant="h6" gutterBottom sx={{ 
              fontWeight: 600,
              display: 'flex',
              alignItems: 'center',
              gap: 1,
              mb: 2
            }}>
              <AccountBalance color="primary" /> Details of your Tax Calculations
            </Typography>

            <Stack spacing={2}>
              {/* Total Tax Liability Section */}
              <Box sx={{ 
                p: 2, 
                backgroundColor: 'primary.50', 
                borderRadius: 2,
                border: '2px solid',
                borderColor: 'primary.main'
              }}>
                <Box display="flex" justifyContent="space-between" alignItems="center">
                  <Typography variant="body1" sx={{ fontWeight: 700, color: 'primary.main' }}>
                    Total Tax Liability
                  </Typography>
                  <Typography variant="h6" sx={{ fontFamily: 'monospace', fontWeight: 700, color: 'primary.main' }}>
                    {formatCurrency(taxCalculation.totalTaxWithCess + taxCalculation.totalAdvanceTaxPenalties)}
                  </Typography>
                </Box>
              </Box>

              {/* Compact Tax Breakdown */}
              <Stack spacing={1}>
                {/* Slab Rate Tax */}
                <Box display="flex" justifyContent="space-between" alignItems="center" sx={{ 
                  p: 1.5, 
                  backgroundColor: 'grey.50', 
                  borderRadius: 1
                }}>
                  <Typography variant="body2" sx={{ fontWeight: 600 }}>Slab Rate Tax</Typography>
                  <Typography variant="body2" sx={{ fontFamily: 'monospace', fontWeight: 600 }}>
                    {formatCurrency(taxCalculation.totalTax)}
                  </Typography>
                </Box>

                {/* Other Charges and Fees - Compact */}
                <Box sx={{ 
                  p: 1.5, 
                  backgroundColor: 'warning.50', 
                  borderRadius: 1,
                  borderLeft: '4px solid',
                  borderLeftColor: 'warning.main'
                }}>
                  <Box display="flex" justifyContent="space-between" alignItems="center" sx={{ mb: 1 }}>
                    <Typography variant="body2" sx={{ fontWeight: 600, color: 'warning.dark' }}>
                      Other Charges and Fees
                    </Typography>
                    <Typography variant="body2" sx={{ fontFamily: 'monospace', fontWeight: 600, color: 'warning.dark' }}>
                      {formatCurrency(
                        taxCalculation.surcharge + 
                        taxCalculation.healthAndEducationCess + 
                        taxCalculation.totalAdvanceTaxPenalties
                      )}
                    </Typography>
                  </Box>
                  
                  {/* Compact Sub-items */}
                  <Stack spacing={0.5} sx={{ ml: 1 }}>
                    {taxCalculation.surcharge > 0 && (
                      <Box display="flex" justifyContent="space-between" alignItems="center">
                        <Typography variant="caption" sx={{ color: 'text.secondary' }}>
                          A. Surcharge
                        </Typography>
                        <Typography variant="caption" sx={{ fontFamily: 'monospace', fontWeight: 600 }}>
                          {formatCurrency(taxCalculation.surcharge)}
                        </Typography>
                      </Box>
                    )}
                    
                    <Box display="flex" justifyContent="space-between" alignItems="center">
                      <Typography variant="caption" sx={{ color: 'text.secondary' }}>
                        B. Health and Education Cess
                      </Typography>
                      <Typography variant="caption" sx={{ fontFamily: 'monospace', fontWeight: 600 }}>
                        {formatCurrency(taxCalculation.healthAndEducationCess)}
                      </Typography>
                    </Box>

                    {taxCalculation.section234AInterest > 0 && (
                      <Box display="flex" justifyContent="space-between" alignItems="center">
                        <Typography variant="caption" sx={{ color: 'error.main' }}>
                          C. Default in Payment of Advance Tax(234A)
                        </Typography>
                        <Typography variant="caption" sx={{ fontFamily: 'monospace', fontWeight: 600, color: 'error.main' }}>
                          {formatCurrency(taxCalculation.section234AInterest)}
                        </Typography>
                      </Box>
                    )}

                    {taxCalculation.section234BInterest > 0 && (
                      <Box display="flex" justifyContent="space-between" alignItems="center">
                        <Typography variant="caption" sx={{ color: 'error.main' }}>
                          D. Failure to Pay Advance Tax(234B)
                        </Typography>
                        <Typography variant="caption" sx={{ fontFamily: 'monospace', fontWeight: 600, color: 'error.main' }}>
                          {formatCurrency(taxCalculation.section234BInterest)}
                        </Typography>
                      </Box>
                    )}

                    {taxCalculation.section234CInterest > 0 && (
                      <Box display="flex" justifyContent="space-between" alignItems="center">
                        <Typography variant="caption" sx={{ color: 'error.main' }}>
                          E. Deferment of Advance Tax(234C)
                        </Typography>
                        <Typography variant="caption" sx={{ fontFamily: 'monospace', fontWeight: 600, color: 'error.main' }}>
                          {formatCurrency(taxCalculation.section234CInterest)}
                        </Typography>
                      </Box>
                    )}
                  </Stack>
                </Box>

                {/* Taxes Paid - Compact */}
                <Box display="flex" justifyContent="space-between" alignItems="center" sx={{ 
                  p: 1.5, 
                  backgroundColor: 'success.50', 
                  borderRadius: 1,
                  borderLeft: '4px solid',
                  borderLeftColor: 'success.main'
                }}>
                  <Typography variant="body2" sx={{ fontWeight: 600, color: 'success.main' }}>
                    Taxes Paid
                  </Typography>
                  <Typography variant="body2" sx={{ fontFamily: 'monospace', fontWeight: 600, color: 'success.main' }}>
                    {formatCurrency(refundCalculation.tdsDeducted)}
                  </Typography>
                </Box>

                {/* Tax Due/Refund - Compact */}
                <Box display="flex" justifyContent="space-between" alignItems="center" sx={{ 
                  p: 1.5, 
                  backgroundColor: refundCalculation.isRefundDue ? 'success.50' : 'error.50',
                  borderRadius: 1,
                  border: '2px solid',
                  borderColor: refundCalculation.isRefundDue ? 'success.main' : 'error.main'
                }}>
                  <Typography variant="body2" sx={{ 
                    fontWeight: 700, 
                    color: refundCalculation.isRefundDue ? 'success.main' : 'error.main' 
                  }}>
                    {refundCalculation.isRefundDue ? 'Refund Due' : 'Tax Due'}
                  </Typography>
                  <Typography variant="body1" sx={{ 
                    fontFamily: 'monospace', 
                    fontWeight: 700, 
                    color: refundCalculation.isRefundDue ? 'success.main' : 'error.main' 
                  }}>
                    {formatCurrency(
                      refundCalculation.isRefundDue 
                        ? refundCalculation.refundAmount 
                        : refundCalculation.additionalTaxDue
                    )}
                  </Typography>
                </Box>
              </Stack>
            </Stack>
          </CardContent>
        </Card>
      </Grow>

      {/* ITR Generation Section */}
      {showITRButton && (
        <Fade in timeout={1400}>
          <Card sx={{ 
            mb: 4,
            background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
            color: 'white',
            borderRadius: 3,
            boxShadow: '0 8px 32px rgba(102, 126, 234, 0.3)'
          }}>
            <CardContent sx={{ textAlign: 'center', py: 2.5 }}>
              <FileDownload sx={{ fontSize: 32, mb: 1.5, opacity: 0.9 }} />
              <Typography variant="h6" gutterBottom sx={{ fontWeight: 700 }}>
                Ready to File Your ITR?
              </Typography>
              <Typography variant="body2" sx={{ mb: 3, opacity: 0.9, maxWidth: 600, mx: 'auto' }}>
                Generate your Income Tax Return form based on this calculation. 
                Choose between ITR-1 or ITR-2 forms for e-filing with the Income Tax Department.
              </Typography>
              
              <Button
                variant="contained"
                size="large"
                startIcon={<Assessment />}
                onClick={onGenerateITR}
                sx={{ 
                  px: 6, 
                  py: 2, 
                  fontSize: '1.1rem',
                  fontWeight: 600,
                  backgroundColor: 'white',
                  color: 'primary.main',
                  borderRadius: 3,
                  boxShadow: '0 8px 25px rgba(255,255,255,0.3)',
                  '&:hover': {
                    backgroundColor: 'grey.100',
                    transform: 'translateY(-2px)',
                    boxShadow: '0 12px 35px rgba(255,255,255,0.4)'
                  }
                }}
              >
                Generate ITR Form
              </Button>
            </CardContent>
          </Card>
        </Fade>
      )}

      {/* Additional Information */}
      <Fade in timeout={1600}>
        <Alert 
          severity="info" 
          icon={<Info />}
          sx={{ 
            borderRadius: 3,
            border: '1px solid rgba(33, 150, 243, 0.2)',
            backgroundColor: 'info.50'
          }}
        >
          <Typography variant="body2" sx={{ lineHeight: 1.6 }}>
            <strong>Important Note:</strong> This calculation is based on the New Tax Regime for Assessment Year 2024-25. 
            The results are for reference purposes only and should be verified with official tax documents. 
            Please consult a tax professional for complex tax situations.
          </Typography>
        </Alert>
      </Fade>
    </Box>
  );
};

export default TaxResults;
