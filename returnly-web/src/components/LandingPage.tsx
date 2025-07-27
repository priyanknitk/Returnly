import React from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Box,
  Typography,
  Button,
  Card,
  CardContent,
  Container,
  Stack,
  Chip
} from '@mui/material';
import {
  CloudUpload,
  Calculate,
  Assessment,
  TrendingUp
} from '@mui/icons-material';

const LandingPage: React.FC = () => {
  const navigate = useNavigate();

  const features = [
    {
      icon: <CloudUpload color="primary" sx={{ fontSize: 40 }} />,
      title: 'Form16 Upload',
      description: 'Upload your Form16 PDF and automatically extract tax data',
      action: () => navigate('/upload')
    },
    {
      icon: <Calculate color="primary" sx={{ fontSize: 40 }} />,
      title: 'Tax Calculation',
      description: 'Calculate taxes using latest Indian tax slabs for FY 2023-24',
      action: () => navigate('/calculate')
    },
    {
      icon: <Assessment color="primary" sx={{ fontSize: 40 }} />,
      title: 'Regime Comparison',
      description: 'Compare old vs new tax regime to find the best option',
      action: () => navigate('/calculate')
    },
    {
      icon: <TrendingUp color="primary" sx={{ fontSize: 40 }} />,
      title: 'ITR Generation',
      description: 'Generate ITR-1 and ITR-2 forms based on your data',
      action: () => navigate('/calculate')
    }
  ];

  return (
    <Container maxWidth="lg">
      <Box sx={{ textAlign: 'center', mb: 6 }}>
        <Typography variant="h3" component="h1" gutterBottom color="primary">
          Welcome to Returnly
        </Typography>
        <Typography variant="h6" color="text.secondary" sx={{ mb: 3 }}>
          Your Complete Indian Tax Calculation Solution
        </Typography>
        <Stack direction="row" spacing={1} justifyContent="center" sx={{ mb: 4 }}>
          <Chip label="AY 2024-25" color="primary" variant="outlined" />
          <Chip label="New Tax Regime" color="secondary" variant="outlined" />
          <Chip label="ITR-1 & ITR-2" color="info" variant="outlined" />
        </Stack>
        <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2} justifyContent="center">
          <Button
            variant="contained"
            size="large"
            startIcon={<CloudUpload />}
            onClick={() => navigate('/upload')}
            sx={{ minWidth: 200 }}
          >
            Upload Form16
          </Button>
          <Button
            variant="outlined"
            size="large"
            startIcon={<Calculate />}
            onClick={() => navigate('/calculate')}
            sx={{ minWidth: 200 }}
          >
            Manual Entry
          </Button>
        </Stack>
      </Box>

      <Box sx={{ 
        display: 'grid',
        gridTemplateColumns: { xs: '1fr', sm: 'repeat(2, 1fr)', md: 'repeat(4, 1fr)' },
        gap: 4 
      }}>
        {features.map((feature, index) => (
          <Card
            key={index}
            sx={{
              height: '100%',
              cursor: 'pointer',
              transition: 'transform 0.2s, box-shadow 0.2s',
              '&:hover': {
                transform: 'translateY(-4px)',
                boxShadow: 4,
              },
            }}
            onClick={feature.action}
          >
            <CardContent sx={{ textAlign: 'center', p: 3 }}>
              <Box sx={{ mb: 2 }}>
                {feature.icon}
              </Box>
              <Typography variant="h6" component="h3" gutterBottom>
                {feature.title}
              </Typography>
              <Typography variant="body2" color="text.secondary">
                {feature.description}
              </Typography>
            </CardContent>
          </Card>
        ))}
      </Box>

      <Box sx={{ mt: 6, p: 3, bgcolor: 'primary.main', color: 'white', borderRadius: 2 }}>
        <Typography variant="h5" gutterBottom>
          Why Choose Returnly?
        </Typography>
        <Box sx={{ 
          display: 'grid',
          gridTemplateColumns: { xs: '1fr', md: 'repeat(3, 1fr)' },
          gap: 3 
        }}>
          <Box>
            <Typography variant="subtitle1" gutterBottom>
              🚀 Fast & Accurate
            </Typography>
            <Typography variant="body2">
              Get instant tax calculations with 100% accuracy using latest tax laws
            </Typography>
          </Box>
          <Box>
            <Typography variant="subtitle1" gutterBottom>
              🔒 Secure & Private
            </Typography>
            <Typography variant="body2">
              Your data is processed locally and never stored on our servers
            </Typography>
          </Box>
          <Box>
            <Typography variant="subtitle1" gutterBottom>
              📱 Mobile Friendly
            </Typography>
            <Typography variant="body2">
              Works seamlessly on all devices - desktop, tablet, and mobile
            </Typography>
          </Box>
        </Box>
      </Box>
    </Container>
  );
};

export default LandingPage;
