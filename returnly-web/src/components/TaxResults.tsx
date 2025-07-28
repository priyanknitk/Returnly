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

interface TaxCalculationResult {
  taxableIncome: number;
  financialYear: string;
  taxRegime: string;
  totalTax: number;
  surcharge: number;
  surchargeRate: number;
  healthAndEducationCess: number;
  totalTaxWithCess: number;
  effectiveTaxRate: number;
  taxBreakdown: TaxSlabCalculation[];
}

interface TaxRefundCalculation {
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
            <TrendingUp sx={{ fontSize: 32, mb: 1.5, opacity: 0.9 }} />
            <Typography variant="h5" gutterBottom sx={{ fontWeight: 700, mb: 1 }}>
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
              <MonetizationOn sx={{ fontSize: 40, color: 'info.main', mb: 2 }} />
              <Typography variant="h6" sx={{ fontWeight: 600, color: 'info.main', mb: 1 }}>
                Taxable Income
              </Typography>
              <Typography variant="h4" sx={{ fontWeight: 700, color: 'info.dark' }}>
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
              <Receipt sx={{ fontSize: 40, color: 'warning.main', mb: 2 }} />
              <Typography variant="h6" sx={{ fontWeight: 600, color: 'warning.main', mb: 1 }}>
                Total Tax Liability
              </Typography>
              <Typography variant="h4" sx={{ fontWeight: 700, color: 'warning.dark' }}>
                {formatCurrency(taxCalculation.totalTaxWithCess)}
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

      {/* Detailed Calculation */}
      <Grow in timeout={1200}>
        <Card sx={{ 
          mb: 4,
          borderRadius: 3,
          boxShadow: '0 4px 20px rgba(0,0,0,0.08)',
          border: '1px solid rgba(0,0,0,0.05)'
        }}>
          <CardContent sx={{ p: 4 }}>
            <Typography variant="h6" gutterBottom sx={{ 
              fontWeight: 600,
              display: 'flex',
              alignItems: 'center',
              gap: 1,
              mb: 3
            }}>
              <AccountBalance color="primary" /> Detailed Tax Calculation
            </Typography>
            
            <Stack spacing={2}>
              <Box display="flex" justifyContent="space-between" alignItems="center" sx={{ 
                p: 2, 
                backgroundColor: 'grey.50', 
                borderRadius: 2 
              }}>
                <Typography sx={{ fontWeight: 500 }}>Base Income Tax:</Typography>
                <Typography sx={{ fontFamily: 'monospace', fontWeight: 600 }}>
                  {formatCurrency(taxCalculation.totalTax)}
                </Typography>
              </Box>
              
              {taxCalculation.surcharge > 0 && (
                <Box display="flex" justifyContent="space-between" alignItems="center" sx={{ 
                  p: 2, 
                  backgroundColor: 'warning.50', 
                  borderRadius: 2,
                  borderLeft: '4px solid',
                  borderLeftColor: 'warning.main'
                }}>
                  <Typography sx={{ fontWeight: 500 }}>
                    Surcharge ({taxCalculation.surchargeRate}%):
                  </Typography>
                  <Typography sx={{ fontFamily: 'monospace', fontWeight: 600, color: 'warning.main' }}>
                    {formatCurrency(taxCalculation.surcharge)}
                  </Typography>
                </Box>
              )}
              
              <Box display="flex" justifyContent="space-between" alignItems="center" sx={{ 
                p: 2, 
                backgroundColor: 'info.50', 
                borderRadius: 2,
                borderLeft: '4px solid',
                borderLeftColor: 'info.main'
              }}>
                <Typography sx={{ fontWeight: 500 }}>Health & Education Cess (4%):</Typography>
                <Typography sx={{ fontFamily: 'monospace', fontWeight: 600, color: 'info.main' }}>
                  {formatCurrency(taxCalculation.healthAndEducationCess)}
                </Typography>
              </Box>
              
              <Divider sx={{ my: 2 }} />
              
              <Box display="flex" justifyContent="space-between" alignItems="center" sx={{ 
                p: 3, 
                backgroundColor: 'primary.50', 
                borderRadius: 2,
                border: '2px solid',
                borderColor: 'primary.main'
              }}>
                <Typography variant="h6" sx={{ fontWeight: 700, color: 'primary.main' }}>
                  Total Tax Liability:
                </Typography>
                <Typography variant="h6" sx={{ fontFamily: 'monospace', fontWeight: 700, color: 'primary.main' }}>
                  {formatCurrency(taxCalculation.totalTaxWithCess)}
                </Typography>
              </Box>
              
              <Box display="flex" justifyContent="space-between" alignItems="center" sx={{ 
                p: 2, 
                backgroundColor: 'grey.100', 
                borderRadius: 2 
              }}>
                <Typography sx={{ fontWeight: 500 }}>TDS Already Deducted:</Typography>
                <Typography sx={{ fontFamily: 'monospace', fontWeight: 600 }}>
                  {formatCurrency(refundCalculation.tdsDeducted)}
                </Typography>
              </Box>
              
              <Divider sx={{ my: 2 }} />
              
              <Box display="flex" justifyContent="space-between" alignItems="center" sx={{ 
                p: 3, 
                backgroundColor: refundCalculation.isRefundDue ? 'success.50' : 'error.50',
                borderRadius: 2,
                border: '2px solid',
                borderColor: refundCalculation.isRefundDue ? 'success.main' : 'error.main'
              }}>
                <Typography 
                  variant="h6" 
                  sx={{ 
                    fontWeight: 700,
                    color: refundCalculation.isRefundDue ? 'success.main' : 'error.main'
                  }}
                >
                  {refundCalculation.isRefundDue ? 'Refund Amount:' : 'Balance Tax Due:'}
                </Typography>
                <Typography 
                  variant="h6" 
                  sx={{ 
                    fontFamily: 'monospace',
                    fontWeight: 700,
                    color: refundCalculation.isRefundDue ? 'success.main' : 'error.main'
                  }}
                >
                  {formatCurrency(
                    refundCalculation.isRefundDue 
                      ? refundCalculation.refundAmount 
                      : refundCalculation.additionalTaxDue
                  )}
                </Typography>
              </Box>
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
