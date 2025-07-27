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
  Button
} from '@mui/material';
import { Assessment, Receipt } from '@mui/icons-material';

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
    <Card sx={{ maxWidth: 1000, mx: 'auto', mt: 4 }}>
      <CardContent>
        <Typography variant="h5" gutterBottom>
          Tax Calculation Results
        </Typography>
        <Typography variant="subtitle1" color="text.secondary" gutterBottom>
          Financial Year: {taxCalculation.financialYear} | Tax Regime: {taxCalculation.taxRegime}
        </Typography>

        {/* Summary Cards */}
        <Stack spacing={2} direction={{ xs: 'column', md: 'row' }} sx={{ mt: 3 }}>
          <Alert severity="info" sx={{ flex: 1 }}>
            <Typography variant="body2">
              <strong>Taxable Income:</strong><br />
              {formatCurrency(taxCalculation.taxableIncome)}
            </Typography>
          </Alert>
          <Alert severity="warning" sx={{ flex: 1 }}>
            <Typography variant="body2">
              <strong>Total Tax Liability:</strong><br />
              {formatCurrency(taxCalculation.totalTaxWithCess)}
            </Typography>
          </Alert>
          <Alert 
            severity={refundCalculation.isRefundDue ? 'success' : 'error'} 
            sx={{ flex: 1 }}
          >
            <Typography variant="body2">
              <strong>
                {refundCalculation.isRefundDue ? 'Refund Due:' : 'Additional Tax Due:'}
              </strong><br />
              {formatCurrency(
                refundCalculation.isRefundDue 
                  ? refundCalculation.refundAmount 
                  : refundCalculation.additionalTaxDue
              )}
            </Typography>
          </Alert>
        </Stack>

        <Divider sx={{ my: 3 }} />

        {/* Tax Breakdown Table */}
        <Typography variant="h6" gutterBottom>
          Tax Slab Breakdown
        </Typography>
        <TableContainer component={Paper} sx={{ mb: 3 }}>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell>Tax Slab</TableCell>
                <TableCell align="right">Income in Slab</TableCell>
                <TableCell align="right">Tax Rate</TableCell>
                <TableCell align="right">Tax Amount</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {taxCalculation.taxBreakdown.map((slab, index) => (
                <TableRow key={index}>
                  <TableCell>{slab.slabDescription}</TableCell>
                  <TableCell align="right">{formatCurrency(slab.incomeInSlab)}</TableCell>
                  <TableCell align="right">{slab.taxRate}%</TableCell>
                  <TableCell align="right">{formatCurrency(slab.taxAmount)}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>

        {/* Detailed Calculation */}
        <Typography variant="h6" gutterBottom>
          Detailed Calculation
        </Typography>
        <Stack spacing={1}>
          <Box display="flex" justifyContent="space-between">
            <Typography>Base Tax:</Typography>
            <Typography>{formatCurrency(taxCalculation.totalTax)}</Typography>
          </Box>
          
          {taxCalculation.surcharge > 0 && (
            <Box display="flex" justifyContent="space-between">
              <Typography>Surcharge ({taxCalculation.surchargeRate}%):</Typography>
              <Typography>{formatCurrency(taxCalculation.surcharge)}</Typography>
            </Box>
          )}
          
          <Box display="flex" justifyContent="space-between">
            <Typography>Health & Education Cess (4%):</Typography>
            <Typography>{formatCurrency(taxCalculation.healthAndEducationCess)}</Typography>
          </Box>
          
          <Divider />
          
          <Box display="flex" justifyContent="space-between">
            <Typography variant="h6">Total Tax Liability:</Typography>
            <Typography variant="h6">{formatCurrency(taxCalculation.totalTaxWithCess)}</Typography>
          </Box>
          
          <Box display="flex" justifyContent="space-between">
            <Typography>TDS Deducted:</Typography>
            <Typography>{formatCurrency(refundCalculation.tdsDeducted)}</Typography>
          </Box>
          
          <Divider />
          
          <Box display="flex" justifyContent="space-between">
            <Typography 
              variant="h6" 
              color={refundCalculation.isRefundDue ? 'success.main' : 'error.main'}
            >
              {refundCalculation.isRefundDue ? 'Refund Amount:' : 'Balance Tax Due:'}
            </Typography>
            <Typography 
              variant="h6" 
              color={refundCalculation.isRefundDue ? 'success.main' : 'error.main'}
            >
              {formatCurrency(
                refundCalculation.isRefundDue 
                  ? refundCalculation.refundAmount 
                  : refundCalculation.additionalTaxDue
              )}
            </Typography>
          </Box>
          
          <Box display="flex" justifyContent="space-between" sx={{ mt: 2 }}>
            <Typography variant="body2" color="text.secondary">
              Effective Tax Rate:
            </Typography>
            <Typography variant="body2" color="text.secondary">
              {taxCalculation.effectiveTaxRate.toFixed(2)}%
            </Typography>
          </Box>
        </Stack>

        {/* ITR Generation Button */}
        {showITRButton && (
          <Box sx={{ mt: 4, textAlign: 'center' }}>
            <Divider sx={{ mb: 3 }}>
              <Typography variant="body2" color="text.secondary">
                Next Step
              </Typography>
            </Divider>
            
            <Alert severity="success" sx={{ mb: 3 }}>
              <Typography variant="body2">
                Ready to file your ITR? Generate your Income Tax Return form based on this calculation.
              </Typography>
            </Alert>
            <Button
              variant="contained"
              size="large"
              startIcon={<Assessment />}
              onClick={onGenerateITR}
              sx={{ px: 4, py: 1.5 }}
            >
              Generate ITR Form
            </Button>
            <Typography variant="body2" color="text.secondary" sx={{ mt: 2 }}>
              Generate ITR-1 or ITR-2 form for e-filing with the Income Tax Department
            </Typography>
          </Box>
        )}

        {/* Additional Information */}
        <Alert severity="info" sx={{ mt: 3 }}>
          <Typography variant="body2">
            This calculation is based on the New Tax Regime for AY 2024-25. 
            The results are for reference only and should be verified with official tax documents.
          </Typography>
        </Alert>
      </CardContent>
    </Card>
  );
};

export default TaxResults;
