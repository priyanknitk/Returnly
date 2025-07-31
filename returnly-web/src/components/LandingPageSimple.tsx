import React, { useState } from 'react';
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
  IconButton,
  Divider,
  Alert
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
  AccountBalance,
  SmartToy,
  Person
} from '@mui/icons-material';
import Form16Upload from './Form16Upload';
import { Form16DataDto } from '../types/api';
import { useTaxDataPersistence } from '../hooks/useTaxDataPersistence';
import { DEFAULT_PERSONAL_INFO } from '../constants/defaultValues';

const LandingPage: React.FC = () => {
  const navigate = useNavigate();
  const { savePersonalInfo, saveForm16Data, saveCurrentStep } = useTaxDataPersistence();
  const [uploadMode, setUploadMode] = useState<'manual' | 'form16'>('form16');
  const [form16Uploaded, setForm16Uploaded] = useState(false);

  const handleForm16UploadSuccess = (form16Data: Form16DataDto) => {
    // Save Form16 data
    saveForm16Data(form16Data);
    
    // Extract and save personal info from Form16
    const personalInfo = {
      ...DEFAULT_PERSONAL_INFO,
      employeeName: form16Data.employeeName || '',
      pan: form16Data.pan || '',
      // We can potentially extract more fields if they become available in Form16
    };
    
    savePersonalInfo(personalInfo);
    saveCurrentStep(1); // Skip to tax data input step since Form16 provides tax data
    
    setForm16Uploaded(true);
    
    // Navigate to tax filing wizard
    setTimeout(() => {
      navigate('/file-returns');
    }, 1500); // Give user time to see success message
  };

  const handleManualStart = () => {
    navigate('/file-returns');
  };

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
              <AccountBalance sx={{ fontSize: 32, mb: 2, opacity: 0.9 }} />
              <Typography variant="h4" component="h1" gutterBottom sx={{ 
                fontWeight: 700, 
                fontSize: { xs: '1.5rem', md: '1.8rem' },
                mb: 1,
                textShadow: '0 2px 4px rgba(0,0,0,0.1)'
              }}>
                Welcome to Returnly
              </Typography>
              <Typography variant="body1" sx={{ 
                mb: 3, 
                opacity: 0.95,
                fontWeight: 500,
                fontSize: '1.1rem',
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
                  startIcon={<Person />}
                  endIcon={<ArrowForward />}
                  onClick={() => setUploadMode('manual')}
                  sx={{ 
                    minWidth: 220,
                    py: 2,
                    px: 4,
                    fontSize: '1.1rem',
                    fontWeight: 700,
                    backgroundColor: uploadMode === 'manual' ? 'white' : 'rgba(255,255,255,0.8)',
                    color: 'primary.main',
                    borderRadius: 3,
                    boxShadow: uploadMode === 'manual' ? '0 8px 25px rgba(255,255,255,0.3)' : '0 4px 15px rgba(255,255,255,0.2)',
                    '&:hover': {
                      backgroundColor: 'white',
                      transform: 'translateY(-2px)',
                      boxShadow: '0 12px 35px rgba(255,255,255,0.4)'
                    }
                  }}
                >
                  Start Manually
                </Button>
                <Button
                  variant="outlined"
                  size="large"
                  startIcon={<FileUpload />}
                  endIcon={<ArrowForward />}
                  onClick={() => setUploadMode('form16')}
                  sx={{ 
                    minWidth: 220,
                    py: 2,
                    px: 4,
                    fontSize: '1.1rem',
                    fontWeight: 700,
                    borderColor: uploadMode === 'form16' ? 'white' : 'rgba(255,255,255,0.6)',
                    color: 'white',
                    borderWidth: 2,
                    borderRadius: 3,
                    backgroundColor: uploadMode === 'form16' ? 'rgba(255,255,255,0.1)' : 'transparent',
                    '&:hover': {
                      borderColor: 'white',
                      backgroundColor: 'rgba(255,255,255,0.2)',
                      transform: 'translateY(-2px)',
                      boxShadow: '0 8px 25px rgba(255,255,255,0.2)'
                    }
                  }}
                >
                  Upload Form16
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

      {/* Quick Start Section */}
      <Fade in timeout={1000}>
        <Box sx={{ mb: 8 }}>
          {uploadMode === 'form16' ? (
            <Box>
              {form16Uploaded ? (
                <Alert 
                  severity="success" 
                  sx={{ 
                    borderRadius: 3,
                    fontSize: '1.1rem',
                    py: 2,
                    mb: 4,
                    textAlign: 'center'
                  }}
                >
                  <Typography variant="h6" sx={{ fontWeight: 600, mb: 1 }}>
                    Form16 Uploaded Successfully! 🎉
                  </Typography>
                  <Typography>
                    Your tax data has been extracted and saved. Redirecting to complete your tax filing...
                  </Typography>
                </Alert>
              ) : (
                <>
                  <Typography variant="h5" sx={{ 
                    textAlign: 'center', 
                    mb: 3, 
                    fontWeight: 700,
                    background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
                    backgroundClip: 'text',
                    WebkitBackgroundClip: 'text',
                    WebkitTextFillColor: 'transparent',
                  }}>
                    Upload Your Form16 to Get Started
                  </Typography>
                  <Typography variant="body1" sx={{ 
                    textAlign: 'center', 
                    mb: 4,
                    color: 'text.secondary',
                    maxWidth: 600,
                    mx: 'auto'
                  }}>
                    Upload your Form16 PDF and we'll automatically extract all your tax information, 
                    pre-fill personal details, and calculate your taxes instantly.
                  </Typography>
                  <Form16Upload onUploadSuccess={handleForm16UploadSuccess} />
                </>
              )}
            </Box>
          ) : (
            <Box sx={{ textAlign: 'center' }}>
              <Typography variant="h5" sx={{ 
                mb: 3, 
                fontWeight: 700,
                background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
                backgroundClip: 'text',
                WebkitBackgroundClip: 'text',
                WebkitTextFillColor: 'transparent',
              }}>
                Start Your Tax Filing Journey
              </Typography>
              <Typography variant="body1" sx={{ 
                mb: 4,
                color: 'text.secondary',
                maxWidth: 600,
                mx: 'auto'
              }}>
                Begin with entering your personal details, then input your tax data manually or upload Form16 later in the process.
              </Typography>
              <Button
                variant="contained"
                size="large"
                startIcon={<Person />}
                endIcon={<ArrowForward />}
                onClick={handleManualStart}
                sx={{ 
                  py: 2,
                  px: 6,
                  fontSize: '1.2rem',
                  fontWeight: 700,
                  borderRadius: 3,
                  background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
                  boxShadow: '0 8px 25px rgba(102, 126, 234, 0.4)',
                  '&:hover': {
                    background: 'linear-gradient(135deg, #5a6fd8 0%, #6a4190 100%)',
                    transform: 'translateY(-2px)',
                    boxShadow: '0 12px 35px rgba(102, 126, 234, 0.5)'
                  }
                }}
              >
                Start Tax Filing
              </Button>
            </Box>
          )}
          
          <Divider sx={{ my: 6 }}>
            <Chip label="OR" sx={{ px: 2, fontWeight: 600 }} />
          </Divider>
          
          <Box sx={{ textAlign: 'center' }}>
            <Button
              variant="outlined"
              size="large"
              startIcon={uploadMode === 'form16' ? <Person /> : <FileUpload />}
              onClick={() => setUploadMode(uploadMode === 'form16' ? 'manual' : 'form16')}
              sx={{ 
                py: 1.5,
                px: 4,
                fontSize: '1rem',
                fontWeight: 600,
                borderRadius: 3,
                borderWidth: 2,
                '&:hover': {
                  borderWidth: 2,
                  transform: 'translateY(-2px)',
                  boxShadow: '0 8px 25px rgba(102, 126, 234, 0.2)'
                }
              }}
            >
              {uploadMode === 'form16' ? 'Start Manually Instead' : 'Upload Form16 Instead'}
            </Button>
          </Box>
        </Box>
      </Fade>

      {/* Feature Cards Grid */}
      <Box sx={{ mb: 8 }}>
        <Fade in timeout={1200}>
          <Typography variant="h5" sx={{ 
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
              onClick={() => navigate('/file-returns')}
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
                    fontSize: 32, 
                    color: 'white',
                    transition: 'all 0.3s ease-in-out'
                  }} />
                </Box>
                <Typography variant="h6" component="h3" gutterBottom sx={{ fontWeight: 600 }}>
                  Form16 Upload
                </Typography>
                <Typography variant="body2" color="text.secondary" sx={{ lineHeight: 1.6 }}>
                  Complete ITR filing process starting with personal details, then upload Form16 or enter data manually
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
              onClick={() => navigate('/file-returns')}
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
                  Complete ITR Filing
                </Typography>
                <Typography variant="body2" color="text.secondary" sx={{ lineHeight: 1.6 }}>
                  Start with personal details, then calculate taxes using latest Indian tax slabs with 100% accuracy
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
              onClick={() => navigate('/file-returns')}
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
                  Tax Planning
                </Typography>
                <Typography variant="body2" color="text.secondary" sx={{ lineHeight: 1.6 }}>
                  Complete guided ITR filing with regime comparison to find the best option for maximum savings
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
              onClick={() => navigate('/file-returns')}
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
                  End-to-end ITR filing: personal details → tax calculation → ITR-1/ITR-2 form generation
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
