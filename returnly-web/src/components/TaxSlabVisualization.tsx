import React from 'react';
import {
  Card,
  CardContent,
  Typography,
  Box,
  Stack
} from '@mui/material';
import { TrendingUp } from '@mui/icons-material';

interface TaxSlabCalculation {
  slabDescription: string;
  incomeInSlab: number;
  taxRate: number;
  taxAmount: number;
}

interface TaxSlabVisualizationProps {
  taxableIncome: number;
  totalTax: number;
  slabCalculations: TaxSlabCalculation[];
}

const TaxSlabVisualization: React.FC<TaxSlabVisualizationProps> = ({
  taxableIncome,
  totalTax,
  slabCalculations
}) => {
  const formatCurrency = (amount: number) => `₹${amount.toLocaleString()}`;

  // Create standardized slabs based on new tax regime 2024-25
  const createStandardSlabs = () => {
    const standardSlabs = [
      { min: 0, max: 300000, rate: 0, label: '₹0' },
      { min: 300000, max: 700000, rate: 5, label: '₹3L' },
      { min: 700000, max: 1000000, rate: 10, label: '₹7L' },
      { min: 1000000, max: 1200000, rate: 15, label: '₹10L' },
      { min: 1200000, max: 1500000, rate: 20, label: '₹12L' },
      { min: 1500000, max: Infinity, rate: 30, label: '₹15L' }
    ];

    return standardSlabs.map((slab) => {
      const incomeInThisSlab = Math.max(0, Math.min(
        slab.max === Infinity ? taxableIncome : slab.max,
        taxableIncome
      ) - slab.min);

      const taxForThisSlab = incomeInThisSlab * (slab.rate / 100);

      return {
        ...slab,
        incomeInSlab: incomeInThisSlab,
        taxAmount: taxForThisSlab,
        isActive: incomeInThisSlab > 0
      };
    });
  };

  const visualSlabs = createStandardSlabs();

  // Colors matching the screenshot
  const getSlabColor = (rate: number) => {
    switch (rate) {
      case 0: return '#FFB3B3'; // Light red for 0%
      case 5: return '#FFCCB3'; // Light orange for 5%
      case 10: return '#FFE6B3'; // Light yellow for 10%
      case 15: return '#D4FFB3'; // Light green for 15%
      case 20: return '#B3E6FF'; // Light blue for 20%
      case 30: return '#D9B3FF'; // Light purple for 30%
      default: return '#F0F0F0';
    }
  };

  const getSlabTextColor = (rate: number) => {
    switch (rate) {
      case 0: return '#CC0000';
      case 5: return '#FF6600';
      case 10: return '#FF9900';
      case 15: return '#00AA00';
      case 20: return '#0066CC';
      case 30: return '#9900CC';
      default: return '#666666';
    }
  };

  return (
    <Card sx={{ 
      borderRadius: 2,
      boxShadow: '0 2px 8px rgba(0,0,0,0.1)',
      overflow: 'hidden',
      mb: 4,
      border: '1px solid #e0e0e0'
    }}>
      {/* Header matching screenshot */}
      <Box sx={{ 
        backgroundColor: 'white',
        borderBottom: '1px solid #e0e0e0',
        p: 2
      }}>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <Typography variant="h6" sx={{ 
            fontWeight: 600,
            color: '#333',
            fontSize: '1.1rem'
          }}>
            Slab Rate Tax
          </Typography>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <Typography variant="h5" sx={{ 
              fontWeight: 700,
              color: '#333'
            }}>
              {formatCurrency(totalTax)}
            </Typography>
            <TrendingUp sx={{ color: '#666', fontSize: 20 }} />
          </Box>
        </Box>
        <Typography variant="body2" sx={{ 
          color: '#666',
          mt: 0.5,
          fontSize: '0.875rem'
        }}>
          Slab Rate Tax is applied on the Taxable Income.
        </Typography>
      </Box>

      <CardContent sx={{ p: 3 }}>
        {/* Taxable Income scale */}
        <Box sx={{ mb: 3 }}>
          <Typography variant="body2" sx={{ 
            color: '#666',
            fontWeight: 600,
            mb: 2
          }}>
            Taxable Income
          </Typography>
          
          {/* Income scale bar */}
          <Box sx={{ position: 'relative', height: 40 }}>
            {/* Scale markers */}
            <Box sx={{ 
              display: 'flex', 
              justifyContent: 'space-between',
              position: 'absolute',
              top: 0,
              left: 0,
              right: 0,
              height: 20
            }}>
              {visualSlabs.map((slab, index) => (
                <Box key={index} sx={{ 
                  textAlign: 'center',
                  fontSize: '0.75rem',
                  color: '#666',
                  fontWeight: 500
                }}>
                  {slab.label}
                </Box>
              ))}
              <Box sx={{ 
                fontSize: '0.75rem',
                color: '#666',
                fontWeight: 500
              }}>
                ₹1.4Cr
              </Box>
            </Box>
          </Box>
        </Box>

        {/* Single horizontal tax slab bar */}
        <Box sx={{ position: 'relative' }}>
          {/* Single horizontal bar container */}
          <Box sx={{ 
            display: 'flex',
            height: 80,
            border: '1px solid #ddd',
            borderRadius: 1,
            overflow: 'hidden'
          }}>
            {visualSlabs.map((slab, index) => {
              // Calculate width percentage based on slab range
              let slabWidth = 0;
              if (slab.max === Infinity) {
                slabWidth = 20; // Fixed width for highest slab
              } else {
                const slabRange = slab.max - slab.min;
                const totalRange = 1500000; // Up to 15L for calculation
                slabWidth = (slabRange / totalRange) * 80; // 80% of total width
              }

              return (
                <Box
                  key={index}
                  sx={{
                    width: `${slabWidth}%`,
                    backgroundColor: getSlabColor(slab.rate),
                    border: '1px solid rgba(0,0,0,0.1)',
                    borderLeft: index === 0 ? 'none' : '1px solid rgba(0,0,0,0.1)',
                    borderRight: 'none',
                    borderTop: 'none',
                    borderBottom: 'none',
                    display: 'flex',
                    flexDirection: 'column',
                    justifyContent: 'center',
                    alignItems: 'center',
                    position: 'relative'
                  }}
                >
                  {/* Tax rate */}
                  <Typography variant="body2" sx={{ 
                    fontWeight: 700,
                    color: getSlabTextColor(slab.rate),
                    fontSize: '0.75rem'
                  }}>
                    Tax: {slab.rate}%
                  </Typography>
                  
                  {/* Income range */}
                  <Typography variant="caption" sx={{ 
                    color: '#666',
                    fontSize: '0.65rem',
                    textAlign: 'center',
                    lineHeight: 1.2
                  }}>
                    {slab.min === 0 && slab.max === 300000 && '(₹0-₹3L)'}
                    {slab.min === 300000 && slab.max === 700000 && '(₹3L-₹7L)'}
                    {slab.min === 700000 && slab.max === 1000000 && '(₹7L-₹10L)'}
                    {slab.min === 1000000 && slab.max === 1200000 && '(₹10L-₹12L)'}
                    {slab.min === 1200000 && slab.max === 1500000 && '(₹12L-₹15L)'}
                    {slab.min === 1500000 && slab.max === Infinity && '(₹15L+)'}
                  </Typography>

                  {/* Tax amount */}
                  <Typography variant="caption" sx={{ 
                    fontWeight: 700,
                    color: getSlabTextColor(slab.rate),
                    fontSize: '0.7rem',
                    fontFamily: 'monospace'
                  }}>
                    ₹{slab.taxAmount.toLocaleString()}
                  </Typography>
                </Box>
              );
            })}
          </Box>
        </Box>

        {/* Total at bottom */}
        <Box sx={{
          mt: 2,
          p: 2,
          backgroundColor: '#f5f5f5',
          borderRadius: 1,
          border: '2px solid #ddd'
        }}>
          <Box sx={{ 
            display: 'flex', 
            justifyContent: 'space-between', 
            alignItems: 'center'
          }}>
            <Typography variant="body1" sx={{ 
              fontWeight: 700,
              color: '#333'
            }}>
              Total Tax ₹{totalTax.toLocaleString()}
            </Typography>
            <Typography variant="body2" sx={{ 
              color: '#666',
              fontStyle: 'italic'
            }}>
              Effective Rate: {((totalTax / taxableIncome) * 100).toFixed(2)}%
            </Typography>
          </Box>
        </Box>
      </CardContent>
    </Card>
  );
};

export default TaxSlabVisualization;
