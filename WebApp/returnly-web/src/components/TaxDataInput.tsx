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
  Stack
} from '@mui/material';

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
    <Card sx={{ maxWidth: 800, mx: 'auto', mt: 4 }}>
      <CardContent>
        <Typography variant="h5" gutterBottom>
          Tax Data Input
        </Typography>

        <Box component="form" sx={{ mt: 3 }}>
          {/* Personal Information */}
          <Typography variant="h6" gutterBottom>
            Personal Information
          </Typography>
          <Stack spacing={2} direction={{ xs: 'column', sm: 'row' }}>
            <TextField
              fullWidth
              label="Employee Name"
              value={formData.employeeName}
              onChange={handleChange('employeeName')}
              error={!!errors.employeeName}
              helperText={errors.employeeName}
            />
            <TextField
              fullWidth
              label="PAN Number"
              value={formData.pan}
              onChange={handleChange('pan')}
              error={!!errors.pan}
              helperText={errors.pan}
              inputProps={{ maxLength: 10, style: { textTransform: 'uppercase' } }}
            />
          </Stack>

          <Stack spacing={2} direction={{ xs: 'column', sm: 'row' }} sx={{ mt: 2 }}>
            <TextField
              fullWidth
              label="Assessment Year"
              value={formData.assessmentYear}
              onChange={handleChange('assessmentYear')}
            />
            <TextField
              fullWidth
              label="Financial Year"
              value={formData.financialYear}
              onChange={handleChange('financialYear')}
            />
          </Stack>

          <Stack spacing={2} direction={{ xs: 'column', sm: 'row' }} sx={{ mt: 2 }}>
            <TextField
              fullWidth
              label="Employer Name"
              value={formData.employerName}
              onChange={handleChange('employerName')}
            />
            <TextField
              fullWidth
              label="TAN Number"
              value={formData.tan}
              onChange={handleChange('tan')}
              inputProps={{ maxLength: 10, style: { textTransform: 'uppercase' } }}
            />
          </Stack>

          <Divider sx={{ my: 3 }} />

          {/* Income Information */}
          <Typography variant="h6" gutterBottom>
            Income Information
          </Typography>
          <Stack spacing={2} direction={{ xs: 'column', sm: 'row' }}>
            <TextField
              fullWidth
              label="Salary (Section 17)"
              type="number"
              value={formData.salarySection17}
              onChange={handleChange('salarySection17')}
              error={!!errors.salarySection17}
              helperText={errors.salarySection17}
            />
            <TextField
              fullWidth
              label="Perquisites"
              type="number"
              value={formData.perquisites}
              onChange={handleChange('perquisites')}
            />
          </Stack>

          <Stack spacing={2} direction={{ xs: 'column', sm: 'row' }} sx={{ mt: 2 }}>
            <TextField
              fullWidth
              label="Profits in Lieu"
              type="number"
              value={formData.profitsInLieu}
              onChange={handleChange('profitsInLieu')}
            />
            <TextField
              fullWidth
              label="Interest on Savings"
              type="number"
              value={formData.interestOnSavings}
              onChange={handleChange('interestOnSavings')}
            />
          </Stack>

          <Stack spacing={2} direction={{ xs: 'column', sm: 'row' }} sx={{ mt: 2 }}>
            <TextField
              fullWidth
              label="Interest on Fixed Deposits"
              type="number"
              value={formData.interestOnFixedDeposits}
              onChange={handleChange('interestOnFixedDeposits')}
            />
            <TextField
              fullWidth
              label="Dividend Income"
              type="number"
              value={formData.dividendIncome}
              onChange={handleChange('dividendIncome')}
            />
          </Stack>

          <Divider sx={{ my: 3 }} />

          {/* Deductions */}
          <Typography variant="h6" gutterBottom>
            Deductions
          </Typography>
          <Stack spacing={2} direction={{ xs: 'column', sm: 'row' }}>
            <TextField
              fullWidth
              label="Standard Deduction"
              type="number"
              value={formData.standardDeduction}
              onChange={handleChange('standardDeduction')}
            />
            <TextField
              fullWidth
              label="Professional Tax"
              type="number"
              value={formData.professionalTax}
              onChange={handleChange('professionalTax')}
            />
          </Stack>

          <TextField
            fullWidth
            label="Total Tax Deducted (TDS)"
            type="number"
            value={formData.totalTaxDeducted}
            onChange={handleChange('totalTaxDeducted')}
            sx={{ mt: 2 }}
          />

          <Divider sx={{ my: 3 }} />

          {/* Calculated Values */}
          <Typography variant="h6" gutterBottom>
            Calculated Values
          </Typography>
          <Stack spacing={2} direction={{ xs: 'column', sm: 'row' }}>
            <Alert severity="info" sx={{ flex: 1 }}>
              <Typography variant="body2">
                <strong>Gross Salary:</strong> ₹{grossSalary.toLocaleString()}
              </Typography>
            </Alert>
            <Alert severity="info" sx={{ flex: 1 }}>
              <Typography variant="body2">
                <strong>Total Income:</strong> ₹{totalIncome.toLocaleString()}
              </Typography>
            </Alert>
            <Alert severity="info" sx={{ flex: 1 }}>
              <Typography variant="body2">
                <strong>Taxable Income:</strong> ₹{taxableIncome.toLocaleString()}
              </Typography>
            </Alert>
          </Stack>

          <Box sx={{ mt: 4, display: 'flex', justifyContent: 'center' }}>
            <Button
              variant="contained"
              size="large"
              onClick={handleCalculate}
              disabled={taxableIncome <= 0}
            >
              Calculate Tax
            </Button>
          </Box>
        </Box>
      </CardContent>
    </Card>
  );
};

export default TaxDataInput;
