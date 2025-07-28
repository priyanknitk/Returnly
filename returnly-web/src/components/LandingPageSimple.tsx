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
  Chip,
  Fade,
  Grow,
  IconButton
} from '@mui/material';
import {
  CloudUpload,
  Calculate,
  Assessment,
  TrendingUp,
  Speed,
  Security,
  PhoneAndroid,
  ArrowForward,
  FileUpload,
  AccountBalance
} from '@mui/icons-material';

const LandingPage: React.FC = () => {
  const navigate = useNavigate();

  return (
    <Container maxWidth="xl" sx={{ px: { xs: 2, sm: 3, md: 4 } }}>
      {/* Hero Section */}
      <Fade in timeout={800}>
        <Box sx={{ textAlign: 'center', mb: 8, pt: 4 }}>
          {/* Main Hero Card */}
          <Card sx={{ 
            background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
            color: 'white',
            borderRadius: 4,
            boxShadow: '0 20px 60px rgba(102, 126, 234, 0.3)',
            mb: 6,
            overflow: 'hidden',
            position: 'relative'
          }}>
            <CardContent sx={{ py: 4, px: 4, position: 'relative', zIndex: 2 }}>
              <AccountBalance sx={{ fontSize: 40, mb: 2, opacity: 0.9 }} />
              <Typography variant="h3" component="h1" gutterBottom sx={{ 
                fontWeight: 800, 
                fontSize: { xs: '1.8rem', md: '2.2rem' },
                mb: 1,
                textShadow: '0 2px 4px rgba(0,0,0,0.1)'
              }}>
                Welcome to Returnly
              </Typography>
              <Typography variant="h6" sx={{ 
                mb: 3, 
                opacity: 0.95,
                fontWeight: 400,
                maxWidth: 600,
                mx: 'auto',
                lineHeight: 1.4
              }}>
                Your Complete Indian Tax Calculation & ITR Generation Solution
              </Typography>
              
              {/* Feature Chips */}
              <Stack 
                direction={{ xs: 'column', sm: 'row' }} 
                spacing={2} 
                justifyContent="center" 
                sx={{ mb: 5 }}
                flexWrap="wrap"
              >
                <Chip 
                  label="Assessment Year 2024-25" 
                  sx={{ 
                    backgroundColor: 'rgba(255,255,255,0.2)', 
                    color: 'white',
                    fontWeight: 600,
                    fontSize: '0.9rem',
                    '&:hover': { backgroundColor: 'rgba(255,255,255,0.3)' }
                  }} 
                />
                <Chip 
                  label="New Tax Regime Support" 
                  sx={{ 
                    backgroundColor: 'rgba(255,255,255,0.2)', 
                    color: 'white',
                    fontWeight: 600,
                    fontSize: '0.9rem',
                    '&:hover': { backgroundColor: 'rgba(255,255,255,0.3)' }
                  }} 
                />
                <Chip 
                  label="ITR-1 & ITR-2 Generation" 
                  sx={{ 
                    backgroundColor: 'rgba(255,255,255,0.2)', 
                    color: 'white',
                    fontWeight: 600,
                    fontSize: '0.9rem',
                    '&:hover': { backgroundColor: 'rgba(255,255,255,0.3)' }
                  }} 
                />
              </Stack>
              
              {/* CTA Buttons */}
              <Stack 
                direction={{ xs: 'column', sm: 'row' }} 
                spacing={3} 
                justifyContent="center"
                alignItems="center"
              >
                <Button
                  variant="contained"
                  size="large"
                  startIcon={<FileUpload />}
                  endIcon={<ArrowForward />}
                  onClick={() => navigate('/upload')}
                  sx={{ 
                    minWidth: 220,
                    py: 2,
                    px: 4,
                    fontSize: '1.1rem',
                    fontWeight: 700,
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
                  Upload Form16
                </Button>
                <Button
                  variant="outlined"
                  size="large"
                  startIcon={<Calculate />}
                  endIcon={<ArrowForward />}
                  onClick={() => navigate('/calculate')}
                  sx={{ 
                    minWidth: 220,
                    py: 2,
                    px: 4,
                    fontSize: '1.1rem',
                    fontWeight: 700,
                    borderColor: 'white',
                    color: 'white',
                    borderWidth: 2,
                    borderRadius: 3,
                    '&:hover': {
                      borderColor: 'white',
                      backgroundColor: 'rgba(255,255,255,0.1)',
                      transform: 'translateY(-2px)',
                      boxShadow: '0 8px 25px rgba(255,255,255,0.2)'
                    }
                  }}
                >
                  Manual Entry
                </Button>
              </Stack>
            </CardContent>
            
            {/* Background Pattern */}
            <Box sx={{
              position: 'absolute',
              top: 0,
              right: 0,
              width: '100%',
              height: '100%',
              opacity: 0.1,
              backgroundImage: 'url("data:image/svg+xml,%3Csvg width="60" height="60" viewBox="0 0 60 60" xmlns="http://www.w3.org/2000/svg"%3E%3Cg fill="none" fill-rule="evenodd"%3E%3Cg fill="%23ffffff" fill-opacity="0.4"%3E%3Ccircle cx="7" cy="7" r="1"/%3E%3Ccircle cx="27" cy="7" r="1"/%3E%3Ccircle cx="47" cy="7" r="1"/%3E%3Ccircle cx="7" cy="27" r="1"/%3E%3Ccircle cx="27" cy="27" r="1"/%3E%3Ccircle cx="47" cy="27" r="1"/%3E%3Ccircle cx="7" cy="47" r="1"/%3E%3Ccircle cx="27" cy="47" r="1"/%3E%3Ccircle cx="47" cy="47" r="1"/%3E%3C/g%3E%3C/g%3E%3C/svg%3E")',
              pointerEvents: 'none'
            }} />
          </Card>
        </Box>
      </Fade>

      {/* Feature Cards Grid */}
      <Box sx={{ mb: 8 }}>
        <Fade in timeout={1200}>
          <Typography variant="h4" sx={{ 
            textAlign: 'center', 
            mb: 6, 
            fontWeight: 700,
            background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
            backgroundClip: 'text',
            WebkitBackgroundClip: 'text',
            WebkitTextFillColor: 'transparent',
          }}>
            Everything You Need for Tax Filing
          </Typography>
        </Fade>

        <Box sx={{ 
          display: 'grid', 
          gridTemplateColumns: { xs: '1fr', sm: '1fr 1fr', lg: '1fr 1fr 1fr 1fr' },
          gap: 4
        }}>
          <Grow in timeout={600}>
            <Card
              sx={{
                cursor: 'pointer',
                borderRadius: 3,
                border: '1px solid rgba(102, 126, 234, 0.1)',
                transition: 'all 0.3s ease-in-out',
                position: 'relative',
                overflow: 'hidden',
                '&:hover': {
                  transform: 'translateY(-8px)',
                  boxShadow: '0 20px 40px rgba(102, 126, 234, 0.2)',
                  '& .feature-icon': {
                    transform: 'scale(1.1) rotate(5deg)',
                  }
                },
              }}
              onClick={() => navigate('/upload')}
            >
              <CardContent sx={{ textAlign: 'center', p: 4, position: 'relative', zIndex: 2 }}>
                <Box sx={{
                  background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
                  borderRadius: '50%',
                  width: 80,
                  height: 80,
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  mx: 'auto',
                  mb: 3,
                  boxShadow: '0 8px 25px rgba(102, 126, 234, 0.3)'
                }}>
                  <CloudUpload className="feature-icon" sx={{ 
                    fontSize: 40, 
                    color: 'white',
                    transition: 'all 0.3s ease-in-out'
                  }} />
                </Box>
                <Typography variant="h6" component="h3" gutterBottom sx={{ fontWeight: 600 }}>
                  Form16 Upload
                </Typography>
                <Typography variant="body2" color="text.secondary" sx={{ lineHeight: 1.6 }}>
                  Upload your Form16 PDF and automatically extract all tax data with AI precision
                </Typography>
              </CardContent>
              <Box sx={{
                position: 'absolute',
                bottom: 0,
                left: 0,
                right: 0,
                height: 4,
                background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
              }} />
            </Card>
          </Grow>

          <Grow in timeout={800}>
            <Card
              sx={{
                cursor: 'pointer',
                borderRadius: 3,
                border: '1px solid rgba(76, 175, 80, 0.1)',
                transition: 'all 0.3s ease-in-out',
                position: 'relative',
                overflow: 'hidden',
                '&:hover': {
                  transform: 'translateY(-8px)',
                  boxShadow: '0 20px 40px rgba(76, 175, 80, 0.2)',
                  '& .feature-icon': {
                    transform: 'scale(1.1) rotate(5deg)',
                  }
                },
              }}
              onClick={() => navigate('/calculate')}
            >
              <CardContent sx={{ textAlign: 'center', p: 4, position: 'relative', zIndex: 2 }}>
                <Box sx={{
                  background: 'linear-gradient(135deg, #4caf50 0%, #2e7d32 100%)',
                  borderRadius: '50%',
                  width: 80,
                  height: 80,
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  mx: 'auto',
                  mb: 3,
                  boxShadow: '0 8px 25px rgba(76, 175, 80, 0.3)'
                }}>
                  <Calculate className="feature-icon" sx={{ 
                    fontSize: 40, 
                    color: 'white',
                    transition: 'all 0.3s ease-in-out'
                  }} />
                </Box>
                <Typography variant="h6" component="h3" gutterBottom sx={{ fontWeight: 600 }}>
                  Tax Calculation
                </Typography>
                <Typography variant="body2" color="text.secondary" sx={{ lineHeight: 1.6 }}>
                  Calculate taxes using latest Indian tax slabs for FY 2023-24 with 100% accuracy
                </Typography>
              </CardContent>
              <Box sx={{
                position: 'absolute',
                bottom: 0,
                left: 0,
                right: 0,
                height: 4,
                background: 'linear-gradient(135deg, #4caf50 0%, #2e7d32 100%)',
              }} />
            </Card>
          </Grow>

          <Grow in timeout={1000}>
            <Card
              sx={{
                cursor: 'pointer',
                borderRadius: 3,
                border: '1px solid rgba(33, 150, 243, 0.1)',
                transition: 'all 0.3s ease-in-out',
                position: 'relative',
                overflow: 'hidden',
                '&:hover': {
                  transform: 'translateY(-8px)',
                  boxShadow: '0 20px 40px rgba(33, 150, 243, 0.2)',
                  '& .feature-icon': {
                    transform: 'scale(1.1) rotate(5deg)',
                  }
                },
              }}
              onClick={() => navigate('/calculate')}
            >
              <CardContent sx={{ textAlign: 'center', p: 4, position: 'relative', zIndex: 2 }}>
                <Box sx={{
                  background: 'linear-gradient(135deg, #2196f3 0%, #1565c0 100%)',
                  borderRadius: '50%',
                  width: 80,
                  height: 80,
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  mx: 'auto',
                  mb: 3,
                  boxShadow: '0 8px 25px rgba(33, 150, 243, 0.3)'
                }}>
                  <Assessment className="feature-icon" sx={{ 
                    fontSize: 40, 
                    color: 'white',
                    transition: 'all 0.3s ease-in-out'
                  }} />
                </Box>
                <Typography variant="h6" component="h3" gutterBottom sx={{ fontWeight: 600 }}>
                  Regime Comparison
                </Typography>
                <Typography variant="body2" color="text.secondary" sx={{ lineHeight: 1.6 }}>
                  Compare old vs new tax regime to find the best option for maximum savings
                </Typography>
              </CardContent>
              <Box sx={{
                position: 'absolute',
                bottom: 0,
                left: 0,
                right: 0,
                height: 4,
                background: 'linear-gradient(135deg, #2196f3 0%, #1565c0 100%)',
              }} />
            </Card>
          </Grow>

          <Grow in timeout={1200}>
            <Card
              sx={{
                cursor: 'pointer',
                borderRadius: 3,
                border: '1px solid rgba(255, 152, 0, 0.1)',
                transition: 'all 0.3s ease-in-out',
                position: 'relative',
                overflow: 'hidden',
                '&:hover': {
                  transform: 'translateY(-8px)',
                  boxShadow: '0 20px 40px rgba(255, 152, 0, 0.2)',
                  '& .feature-icon': {
                    transform: 'scale(1.1) rotate(5deg)',
                  }
                },
              }}
              onClick={() => navigate('/calculate')}
            >
              <CardContent sx={{ textAlign: 'center', p: 4, position: 'relative', zIndex: 2 }}>
                <Box sx={{
                  background: 'linear-gradient(135deg, #ff9800 0%, #e65100 100%)',
                  borderRadius: '50%',
                  width: 80,
                  height: 80,
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  mx: 'auto',
                  mb: 3,
                  boxShadow: '0 8px 25px rgba(255, 152, 0, 0.3)'
                }}>
                  <TrendingUp className="feature-icon" sx={{ 
                    fontSize: 40, 
                    color: 'white',
                    transition: 'all 0.3s ease-in-out'
                  }} />
                </Box>
                <Typography variant="h6" component="h3" gutterBottom sx={{ fontWeight: 600 }}>
                  ITR Generation
                </Typography>
                <Typography variant="body2" color="text.secondary" sx={{ lineHeight: 1.6 }}>
                  Generate ITR-1 and ITR-2 forms automatically based on your tax data
                </Typography>
              </CardContent>
              <Box sx={{
                position: 'absolute',
                bottom: 0,
                left: 0,
                right: 0,
                height: 4,
                background: 'linear-gradient(135deg, #ff9800 0%, #e65100 100%)',
              }} />
            </Card>
          </Grow>
        </Box>
      </Box>

      {/* Why Choose Returnly Section */}
      <Fade in timeout={1600}>
        <Card sx={{ 
          background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
          color: 'white',
          borderRadius: 4,
          boxShadow: '0 20px 60px rgba(102, 126, 234, 0.3)',
          overflow: 'hidden',
          position: 'relative'
        }}>
          <CardContent sx={{ p: 6, position: 'relative', zIndex: 2 }}>
            <Typography variant="h4" gutterBottom sx={{ 
              fontWeight: 700,
              textAlign: 'center',
              mb: 5,
              textShadow: '0 2px 4px rgba(0,0,0,0.1)'
            }}>
              Why Choose Returnly?
            </Typography>
            
            <Box sx={{ 
              display: 'grid', 
              gridTemplateColumns: { xs: '1fr', md: '1fr 1fr 1fr' },
              gap: 4
            }}>
              <Box sx={{ textAlign: 'center' }}>
                <Box sx={{
                  backgroundColor: 'rgba(255,255,255,0.15)',
                  borderRadius: '50%',
                  width: 80,
                  height: 80,
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  mx: 'auto',
                  mb: 3,
                  backdropFilter: 'blur(10px)'
                }}>
                  <Speed sx={{ fontSize: 40, color: 'white' }} />
                </Box>
                <Typography variant="h6" gutterBottom sx={{ fontWeight: 600 }}>
                  Fast & Accurate
                </Typography>
                <Typography variant="body1" sx={{ opacity: 0.9, lineHeight: 1.6 }}>
                  Get instant tax calculations with 100% accuracy using the latest Indian tax laws and regulations
                </Typography>
              </Box>
              
              <Box sx={{ textAlign: 'center' }}>
                <Box sx={{
                  backgroundColor: 'rgba(255,255,255,0.15)',
                  borderRadius: '50%',
                  width: 80,
                  height: 80,
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  mx: 'auto',
                  mb: 3,
                  backdropFilter: 'blur(10px)'
                }}>
                  <Security sx={{ fontSize: 40, color: 'white' }} />
                </Box>
                <Typography variant="h6" gutterBottom sx={{ fontWeight: 600 }}>
                  Secure & Private
                </Typography>
                <Typography variant="body1" sx={{ opacity: 0.9, lineHeight: 1.6 }}>
                  Your sensitive financial data is processed securely with bank-grade encryption and privacy protection
                </Typography>
              </Box>
              
              <Box sx={{ textAlign: 'center' }}>
                <Box sx={{
                  backgroundColor: 'rgba(255,255,255,0.15)',
                  borderRadius: '50%',
                  width: 80,
                  height: 80,
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  mx: 'auto',
                  mb: 3,
                  backdropFilter: 'blur(10px)'
                }}>
                  <PhoneAndroid sx={{ fontSize: 40, color: 'white' }} />
                </Box>
                <Typography variant="h6" gutterBottom sx={{ fontWeight: 600 }}>
                  Mobile Friendly
                </Typography>
                <Typography variant="body1" sx={{ opacity: 0.9, lineHeight: 1.6 }}>
                  Works seamlessly on all devices - desktop, tablet, and mobile with responsive design
                </Typography>
              </Box>
            </Box>
          </CardContent>
          
          {/* Background Pattern */}
          <Box sx={{
            position: 'absolute',
            top: 0,
            right: 0,
            width: '100%',
            height: '100%',
            opacity: 0.05,
            backgroundImage: 'url("data:image/svg+xml,%3Csvg width="60" height="60" viewBox="0 0 60 60" xmlns="http://www.w3.org/2000/svg"%3E%3Cg fill="none" fill-rule="evenodd"%3E%3Cg fill="%23ffffff" fill-opacity="0.4"%3E%3Ccircle cx="7" cy="7" r="1"/%3E%3Ccircle cx="27" cy="7" r="1"/%3E%3Ccircle cx="47" cy="7" r="1"/%3E%3Ccircle cx="7" cy="27" r="1"/%3E%3Ccircle cx="27" cy="27" r="1"/%3E%3Ccircle cx="47" cy="27" r="1"/%3E%3Ccircle cx="7" cy="47" r="1"/%3E%3Ccircle cx="27" cy="47" r="1"/%3E%3Ccircle cx="47" cy="47" r="1"/%3E%3C/g%3E%3C/g%3E%3C/svg%3E")',
            pointerEvents: 'none'
          }} />
        </Card>
      </Fade>
    </Container>
  );
};

export default LandingPage;
