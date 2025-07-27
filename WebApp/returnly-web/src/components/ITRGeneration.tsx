import React, { useState } from 'react';
import {
  Card,
  CardContent,
  Typography,
  Button,
  Box,
  Stepper,
  Step,
  StepLabel,
  Alert,
  LinearProgress,
  Chip,
  Divider,
  Stack,
  Fade,
  Grow
} from '@mui/material';
import {
  Assessment,
  Download,
  CheckCircle,
  Warning,
  FileDownload,
  CloudDownload,
  OpenInNew,
  Backup,
  Security
} from '@mui/icons-material';
import { 
  Form16DataDto, 
  AdditionalTaxpayerInfoDto, 
  ITRGenerationResponseDto,
  ITRRecommendationResponseDto,
  ApiError 
} from '../types/api';
import { API_ENDPOINTS } from '../config/api';
import AdditionalInfoForm from './AdditionalInfoForm';

interface ITRGenerationProps {
  form16Data: Form16DataDto;
  onBack: () => void;
}

const steps = [
  'Additional Information',
  'ITR Recommendation',
  'Form Generation',
  'Download'
];

const ITRGeneration: React.FC<ITRGenerationProps> = ({ form16Data, onBack }) => {
  const [activeStep, setActiveStep] = useState(0);
  const [additionalInfo, setAdditionalInfo] = useState<AdditionalTaxpayerInfoDto | null>(null);
  const [recommendation, setRecommendation] = useState<ITRRecommendationResponseDto | null>(null);
  const [generationResult, setGenerationResult] = useState<ITRGenerationResponseDto | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Helper function to map ITR type strings to numeric enum values
  const mapITRTypeToEnum = (itrType: string): number | null => {
    const typeMap: Record<string, number> = {
      'ITR1_Sahaj': 1,
      'ITR1': 1,
      'ITR2': 2,
      'ITR3': 3,
      'ITR4': 4
    };
    return typeMap[itrType] || null;
  };

  // Helper function to create ITR generation request
  const createITRRequest = () => {
    if (!additionalInfo || !recommendation) return null;
    
    return {
      form16Data,
      additionalInfo,
      preferredITRType: mapITRTypeToEnum(recommendation.recommendedITRType)
    };
  };

  const handleAdditionalInfoSubmit = async (info: AdditionalTaxpayerInfoDto) => {
    setAdditionalInfo(info);
    setLoading(true);
    setError(null);

    try {
      // Get ITR recommendation
      const recommendationRequest = {
        form16Data,
        hasHouseProperty: info.hasHouseProperty,
        hasCapitalGains: info.hasCapitalGains,
        hasBusinessIncome: false,
        hasForeignIncome: info.hasForeignIncome,
        hasForeignAssets: info.hasForeignAssets,
        isHUF: false,
        totalIncome: form16Data.grossSalary + info.otherInterestIncome + info.otherDividendIncome + info.otherSourcesIncome
      };

      console.log('Sending recommendation request:', recommendationRequest);

      const response = await fetch(API_ENDPOINTS.ITR_RECOMMEND, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(recommendationRequest),
      });

      if (response.ok) {
        const data: ITRRecommendationResponseDto = await response.json();
        console.log('ITR recommendation received:', data.recommendedITRType);
        setRecommendation(data);
        setActiveStep(1);
      } else {
        const errorText = await response.text();
        console.error('Recommendation error response:', errorText);
        try {
          const errorData: ApiError = JSON.parse(errorText);
          setError(errorData.error || 'Failed to get ITR recommendation');
        } catch {
          setError(`HTTP ${response.status}: ${errorText || 'Failed to get ITR recommendation'}`);
        }
      }
    } catch (err) {
      console.error('Network error:', err);
      setError('Network error. Please check if the API server is running on http://localhost:5201');
    } finally {
      setLoading(false);
    }
  };

  const handleGenerateITR = async () => {
    if (!additionalInfo || !recommendation) return;

    setLoading(true);
    setError(null);

    try {
      const generationRequest = createITRRequest();
      if (!generationRequest) {
        setError('Missing required data for ITR generation');
        return;
      }

      console.log('Generating ITR form:', recommendation.recommendedITRType);

      const response = await fetch(API_ENDPOINTS.ITR_GENERATE, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(generationRequest),
      });

      if (response.ok) {
        const data: ITRGenerationResponseDto = await response.json();
        setGenerationResult(data);
        if (data.isSuccess) {
          setActiveStep(3);
        } else {
          setError(`ITR generation failed: ${data.validationErrors.join(', ')}`);
        }
      } else {
        const errorText = await response.text();
        console.error('Generation error response:', errorText);
        try {
          const errorData: ApiError = JSON.parse(errorText);
          setError(errorData.error || 'Failed to generate ITR form');
        } catch {
          setError(`HTTP ${response.status}: ${errorText || 'Failed to generate ITR form'}`);
        }
      }
    } catch (err) {
      console.error('Network error:', err);
      setError('Network error. Please check if the API server is running on http://localhost:5201');
    } finally {
      setLoading(false);
    }
  };

  const handleDownload = async (format: 'xml' | 'json') => {
    if (!additionalInfo || !recommendation) return;

    try {
      const generationRequest = createITRRequest();
      if (!generationRequest) {
        setError('Missing required data for download');
        return;
      }

      const endpoint = format === 'xml' ? API_ENDPOINTS.ITR_DOWNLOAD_XML : API_ENDPOINTS.ITR_DOWNLOAD_JSON;
      
      console.log(`Downloading ${format.toUpperCase()} file...`);

      const response = await fetch(endpoint, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(generationRequest),
      });

      if (response.ok) {
        const blob = await response.blob();
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.style.display = 'none';
        a.href = url;
        a.download = generationResult?.fileName?.replace('.xml', `.${format}`) || `ITR_${form16Data.pan}.${format}`;
        document.body.appendChild(a);
        a.click();
        window.URL.revokeObjectURL(url);
        document.body.removeChild(a);
      } else {
        const errorText = await response.text();
        console.error(`Download ${format} error:`, errorText);
        setError(`Failed to download ${format.toUpperCase()} file: ${errorText}`);
      }
    } catch (err) {
      console.error('Download error:', err);
      setError('Download failed. Please try again.');
    }
  };

  const proceedToGeneration = () => {
    setActiveStep(2);
    handleGenerateITR();
  };

  return (
    <Box sx={{ maxWidth: 1400, mx: 'auto', mt: 4, px: { xs: 1, sm: 2, md: 3 } }}>
      {/* Header Card */}
      <Fade in timeout={600}>
        <Card sx={{ 
          mb: 4,
          background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
          color: 'white',
          borderRadius: 3,
          boxShadow: '0 8px 32px rgba(102, 126, 234, 0.3)'
        }}>
          <CardContent sx={{ textAlign: 'center', py: 4 }}>
            <Assessment sx={{ fontSize: 48, mb: 2, opacity: 0.9 }} />
            <Typography variant="h4" gutterBottom sx={{ fontWeight: 700, mb: 1 }}>
              ITR Form Generation
            </Typography>
            <Typography variant="subtitle1" sx={{ opacity: 0.9, maxWidth: 600, mx: 'auto' }}>
              Generate your Income Tax Return form automatically based on your tax data
            </Typography>
          </CardContent>
        </Card>
      </Fade>

      {/* Main Form Card */}
      <Grow in timeout={800}>
        <Card sx={{ 
          borderRadius: 3,
          boxShadow: '0 4px 20px rgba(0,0,0,0.08)',
          overflow: 'hidden',
          border: '1px solid rgba(0,0,0,0.05)'
        }}>
          <CardContent sx={{ p: 4 }}>
            {/* Progress Stepper */}
            <Stepper 
              activeStep={activeStep} 
              sx={{ 
                mb: 4,
                '& .MuiStepLabel-root .Mui-completed': {
                  color: 'success.main',
                },
                '& .MuiStepLabel-root .Mui-active': {
                  color: 'primary.main',
                }
              }}
            >
              {steps.map((label) => (
                <Step key={label}>
                  <StepLabel>{label}</StepLabel>
                </Step>
              ))}
            </Stepper>

            {/* Error Display */}
            {error && (
              <Fade in>
                <Alert 
                  severity="error" 
                  icon={<Warning />}
                  sx={{ 
                    mb: 3,
                    borderRadius: 2,
                    border: '1px solid rgba(244, 67, 54, 0.2)',
                    backgroundColor: 'error.50'
                  }}
                >
                  <Typography variant="body2" sx={{ fontWeight: 500 }}>
                    {error}
                  </Typography>
                </Alert>
              </Fade>
            )}

            {/* Loading Progress */}
            {loading && (
              <Fade in>
                <Box sx={{ mb: 3 }}>
                  <LinearProgress 
                    sx={{ 
                      borderRadius: 2, 
                      height: 8,
                      backgroundColor: 'grey.200',
                      '& .MuiLinearProgress-bar': {
                        background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)'
                      }
                    }} 
                  />
                </Box>
              </Fade>
            )}

            {/* Step 0: Additional Information */}
            {activeStep === 0 && (
              <AdditionalInfoForm 
                onSubmit={handleAdditionalInfoSubmit}
                loading={loading}
              />
            )}

            {/* Step 1: ITR Recommendation */}
            {activeStep === 1 && recommendation && (
              <Fade in timeout={300}>
                <Box>
                  <Typography variant="h5" gutterBottom sx={{ 
                    fontWeight: 600,
                    display: 'flex',
                    alignItems: 'center',
                    gap: 1,
                    mb: 3
                  }}>
                    <CheckCircle color="success" />
                    ITR Form Recommendation
                  </Typography>
                  
                  {/* Recommendation Card */}
                  <Card sx={{ 
                    mb: 4, 
                    background: 'linear-gradient(135deg, #e8f5e8 0%, #c8e6c9 100%)',
                    border: '2px solid rgba(76, 175, 80, 0.3)',
                    borderRadius: 3
                  }}>
                    <CardContent sx={{ p: 4 }}>
                      <Box sx={{ display: 'flex', alignItems: 'center', mb: 3 }}>
                        <CheckCircle sx={{ fontSize: 40, color: 'success.main', mr: 2 }} />
                        <Box>
                          <Typography variant="h6" sx={{ fontWeight: 700, color: 'success.main' }}>
                            Recommended: {recommendation.recommendedITRType}
                          </Typography>
                          <Chip 
                            label="Best Match for Your Profile" 
                            color="success" 
                            size="small"
                            sx={{ mt: 1, fontWeight: 600 }}
                          />
                        </Box>
                      </Box>
                      <Typography variant="body1" sx={{ mb: 2, fontWeight: 500 }}>
                        {recommendation.reason}
                      </Typography>
                      <Typography variant="body2" color="text.secondary" sx={{ lineHeight: 1.6 }}>
                        {recommendation.recommendationSummary}
                      </Typography>
                    </CardContent>
                  </Card>

                  {/* Requirements and Limitations */}
                  <Stack spacing={3} direction={{ xs: 'column', md: 'row' }} sx={{ mb: 4 }}>
                    <Card sx={{ 
                      flex: 1,
                      borderRadius: 3,
                      border: '1px solid rgba(76, 175, 80, 0.2)',
                      backgroundColor: 'success.50'
                    }}>
                      <CardContent sx={{ p: 3 }}>
                        <Typography variant="h6" sx={{ 
                          color: 'success.main',
                          fontWeight: 600,
                          mb: 2,
                          display: 'flex',
                          alignItems: 'center',
                          gap: 1
                        }}>
                          <CheckCircle />
                          Requirements Met
                        </Typography>
                        <Stack spacing={1}>
                          {recommendation.requirements.map((req, index) => (
                            <Box key={index} sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                              <CheckCircle sx={{ fontSize: 16, color: 'success.main' }} />
                              <Typography variant="body2" sx={{ fontWeight: 500 }}>
                                {req}
                              </Typography>
                            </Box>
                          ))}
                        </Stack>
                      </CardContent>
                    </Card>

                    <Card sx={{ 
                      flex: 1,
                      borderRadius: 3,
                      border: '1px solid rgba(255, 152, 0, 0.2)',
                      backgroundColor: 'warning.50'
                    }}>
                      <CardContent sx={{ p: 3 }}>
                        <Typography variant="h6" sx={{ 
                          color: 'warning.main',
                          fontWeight: 600,
                          mb: 2,
                          display: 'flex',
                          alignItems: 'center',
                          gap: 1
                        }}>
                          <Warning />
                          Limitations
                        </Typography>
                        <Stack spacing={1}>
                          {recommendation.limitations.map((limitation, index) => (
                            <Box key={index} sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                              <Warning sx={{ fontSize: 16, color: 'warning.main' }} />
                              <Typography variant="body2" sx={{ fontWeight: 500 }}>
                                {limitation}
                              </Typography>
                            </Box>
                          ))}
                        </Stack>
                      </CardContent>
                    </Card>
                  </Stack>

                  {/* Action Buttons */}
                  <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
                    <Button 
                      variant="outlined" 
                      onClick={onBack}
                      sx={{ py: 1.5, fontWeight: 600 }}
                    >
                      Back to Form16
                    </Button>
                    <Button 
                      variant="contained" 
                      onClick={proceedToGeneration}
                      disabled={loading}
                      sx={{ 
                        py: 1.5, 
                        px: 4,
                        fontWeight: 600,
                        background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
                        '&:hover': {
                          background: 'linear-gradient(135deg, #5a6fd8 0%, #6a4190 100%)',
                        }
                      }}
                    >
                      Generate ITR Form
                    </Button>
                  </Stack>
                </Box>
              </Fade>
            )}

            {/* Step 2: Form Generation (Loading) */}
            {activeStep === 2 && (
              <Fade in timeout={300}>
                <Box sx={{ textAlign: 'center', py: 6 }}>
                  <Box sx={{
                    background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
                    borderRadius: '50%',
                    width: 100,
                    height: 100,
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    mx: 'auto',
                    mb: 3,
                    boxShadow: '0 8px 25px rgba(102, 126, 234, 0.3)'
                  }}>
                    <FileDownload sx={{ fontSize: 48, color: 'white' }} />
                  </Box>
                  <Typography variant="h5" gutterBottom sx={{ fontWeight: 600 }}>
                    Generating ITR Form...
                  </Typography>
                  <Typography variant="body1" color="text.secondary" sx={{ mb: 3, maxWidth: 400, mx: 'auto' }}>
                    Please wait while we generate your {recommendation?.recommendedITRType} form. 
                    This process usually takes a few seconds.
                  </Typography>
                  <LinearProgress 
                    sx={{ 
                      maxWidth: 300, 
                      mx: 'auto',
                      borderRadius: 2, 
                      height: 8,
                      backgroundColor: 'grey.200',
                      '& .MuiLinearProgress-bar': {
                        background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)'
                      }
                    }} 
                  />
                </Box>
              </Fade>
            )}

            {/* Step 3: Download */}
            {activeStep === 3 && generationResult && (
              <Fade in timeout={300}>
                <Box>
                  {/* Success Header */}
                  <Card sx={{ 
                    mb: 4,
                    background: 'linear-gradient(135deg, #4caf50 0%, #2e7d32 100%)',
                    color: 'white',
                    borderRadius: 3
                  }}>
                    <CardContent sx={{ textAlign: 'center', py: 4 }}>
                      <CheckCircle sx={{ fontSize: 60, mb: 2, opacity: 0.9 }} />
                      <Typography variant="h4" gutterBottom sx={{ fontWeight: 700 }}>
                        ITR Form Generated Successfully!
                      </Typography>
                      <Typography variant="subtitle1" sx={{ opacity: 0.9 }}>
                        Your income tax return is ready for download and e-filing
                      </Typography>
                    </CardContent>
                  </Card>

                  {/* Generation Summary */}
                  <Card sx={{ 
                    mb: 4, 
                    borderRadius: 3,
                    border: '1px solid rgba(76, 175, 80, 0.2)',
                    backgroundColor: 'success.50'
                  }}>
                    <CardContent sx={{ p: 3 }}>
                      <Typography variant="h6" gutterBottom sx={{ 
                        fontWeight: 600,
                        color: 'success.main',
                        display: 'flex',
                        alignItems: 'center',
                        gap: 1
                      }}>
                        <Assessment />
                        Generation Summary
                      </Typography>
                      <Typography variant="body1" sx={{ 
                        whiteSpace: 'pre-line',
                        lineHeight: 1.6,
                        fontWeight: 500
                      }}>
                        {generationResult.generationSummary}
                      </Typography>
                    </CardContent>
                  </Card>

                  {/* Warnings */}
                  {generationResult.warnings.length > 0 && (
                    <Alert 
                      severity="warning" 
                      icon={<Warning />}
                      sx={{ 
                        mb: 4,
                        borderRadius: 2,
                        border: '1px solid rgba(255, 152, 0, 0.2)',
                        backgroundColor: 'warning.50'
                      }}
                    >
                      <Typography variant="subtitle2" gutterBottom sx={{ fontWeight: 600 }}>
                        Important Warnings:
                      </Typography>
                      <Stack spacing={1}>
                        {generationResult.warnings.map((warning, index) => (
                          <Typography key={index} variant="body2" sx={{ fontWeight: 500 }}>
                            • {warning}
                          </Typography>
                        ))}
                      </Stack>
                    </Alert>
                  )}

                  {/* Download Section */}
                  <Typography variant="h5" gutterBottom sx={{ 
                    fontWeight: 600,
                    mb: 3,
                    display: 'flex',
                    alignItems: 'center',
                    gap: 1
                  }}>
                    <CloudDownload color="primary" />
                    Download Your ITR Form
                  </Typography>

                  <Stack spacing={3} direction={{ xs: 'column', sm: 'row' }} sx={{ mb: 4 }}>
                    <Card sx={{ 
                      flex: 1,
                      borderRadius: 3,
                      border: '2px solid rgba(102, 126, 234, 0.3)',
                      transition: 'all 0.3s ease-in-out',
                      cursor: 'pointer',
                      '&:hover': {
                        transform: 'translateY(-4px)',
                        boxShadow: '0 8px 25px rgba(102, 126, 234, 0.3)'
                      }
                    }}>
                      <CardContent sx={{ textAlign: 'center', p: 3 }}>
                        <FileDownload sx={{ fontSize: 40, color: 'primary.main', mb: 2 }} />
                        <Typography variant="h6" gutterBottom sx={{ fontWeight: 600 }}>
                          XML Format
                        </Typography>
                        <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
                          Official format required for e-filing with Income Tax Department
                        </Typography>
                        <Button
                          variant="contained"
                          startIcon={<Download />}
                          onClick={() => handleDownload('xml')}
                          fullWidth
                          sx={{ 
                            background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
                            fontWeight: 600
                          }}
                        >
                          Download XML
                        </Button>
                        <Chip 
                          label="Required for e-filing" 
                          color="primary" 
                          size="small" 
                          sx={{ mt: 2, fontWeight: 600 }} 
                        />
                      </CardContent>
                    </Card>

                    <Card sx={{ 
                      flex: 1,
                      borderRadius: 3,
                      border: '2px solid rgba(158, 158, 158, 0.3)',
                      transition: 'all 0.3s ease-in-out',
                      cursor: 'pointer',
                      '&:hover': {
                        transform: 'translateY(-4px)',
                        boxShadow: '0 8px 25px rgba(158, 158, 158, 0.3)'
                      }
                    }}>
                      <CardContent sx={{ textAlign: 'center', p: 3 }}>
                        <Backup sx={{ fontSize: 40, color: 'grey.600', mb: 2 }} />
                        <Typography variant="h6" gutterBottom sx={{ fontWeight: 600 }}>
                          JSON Format
                        </Typography>
                        <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
                          Human-readable backup for your records and reference
                        </Typography>
                        <Button
                          variant="outlined"
                          startIcon={<Download />}
                          onClick={() => handleDownload('json')}
                          fullWidth
                          sx={{ fontWeight: 600 }}
                        >
                          Download JSON
                        </Button>
                        <Chip 
                          label="Backup & Records" 
                          color="default" 
                          size="small" 
                          sx={{ mt: 2, fontWeight: 600 }} 
                        />
                      </CardContent>
                    </Card>
                  </Stack>

                  {/* Next Steps */}
                  <Card sx={{ 
                    mb: 4,
                    borderRadius: 3,
                    border: '1px solid rgba(33, 150, 243, 0.2)',
                    backgroundColor: 'info.50'
                  }}>
                    <CardContent sx={{ p: 3 }}>
                      <Typography variant="h6" gutterBottom sx={{ 
                        fontWeight: 600,
                        color: 'info.main',
                        display: 'flex',
                        alignItems: 'center',
                        gap: 1
                      }}>
                        <Security />
                        Next Steps for E-Filing
                      </Typography>
                      <Stack spacing={1.5}>
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                          <Chip label="1" color="info" size="small" />
                          <Typography variant="body2" sx={{ fontWeight: 500 }}>
                            Upload the XML file to the Income Tax e-filing portal
                          </Typography>
                        </Box>
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                          <Chip label="2" color="info" size="small" />
                          <Typography variant="body2" sx={{ fontWeight: 500 }}>
                            Verify all details before final submission
                          </Typography>
                        </Box>
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                          <Chip label="3" color="info" size="small" />
                          <Typography variant="body2" sx={{ fontWeight: 500 }}>
                            Submit your ITR before the due date
                          </Typography>
                        </Box>
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                          <Chip label="4" color="info" size="small" />
                          <Typography variant="body2" sx={{ fontWeight: 500 }}>
                            Keep the JSON backup for your records
                          </Typography>
                        </Box>
                      </Stack>
                    </CardContent>
                  </Card>

                  {/* Action Buttons */}
                  <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
                    <Button 
                      variant="outlined" 
                      onClick={onBack}
                      sx={{ py: 1.5, fontWeight: 600 }}
                    >
                      Process Another Form16
                    </Button>
                    <Button 
                      variant="contained" 
                      href="https://www.incometax.gov.in/iec/foportal" 
                      target="_blank"
                      rel="noopener noreferrer"
                      startIcon={<OpenInNew />}
                      sx={{ 
                        py: 1.5, 
                        px: 4,
                        fontWeight: 600,
                        background: 'linear-gradient(135deg, #4caf50 0%, #2e7d32 100%)',
                        '&:hover': {
                          background: 'linear-gradient(135deg, #43a047 0%, #1b5e20 100%)',
                        }
                      }}
                    >
                      Go to e-Filing Portal
                    </Button>
                  </Stack>
                </Box>
              </Fade>
            )}
          </CardContent>
        </Card>
      </Grow>
    </Box>
  );
};

export default ITRGeneration;
