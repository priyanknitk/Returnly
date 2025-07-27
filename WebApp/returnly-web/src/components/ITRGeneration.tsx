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
  Divider
} from '@mui/material';
import {
  Assessment,
  Download,
  CheckCircle,
  Warning
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

      console.log('Recommendation response status:', response.status);

      if (response.ok) {
        const data: ITRRecommendationResponseDto = await response.json();
        console.log('Recommendation response data:', data);
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
      // Map to numeric enum values that match the WebApi ITRType enum
      let mappedITRType = null;
      if (recommendation.recommendedITRType === 'ITR1_Sahaj' || recommendation.recommendedITRType === 'ITR1') {
        mappedITRType = 1; // ITR1 = 1
      } else if (recommendation.recommendedITRType === 'ITR2') {
        mappedITRType = 2; // ITR2 = 2
      } else if (recommendation.recommendedITRType === 'ITR3') {
        mappedITRType = 3; // ITR3 = 3
      } else if (recommendation.recommendedITRType === 'ITR4') {
        mappedITRType = 4; // ITR4 = 4
      }

      console.log('Original recommendation type:', recommendation.recommendedITRType);
      console.log('Mapped to numeric enum:', mappedITRType);

      // Use the exact camelCase field names from your working Swagger request
      const generationRequest = {
        form16Data,
        additionalInfo,
        preferredITRType: mappedITRType
      };

      console.log('Sending generation request with Pascal case:', generationRequest);
      console.log('Original recommendation type:', recommendation.recommendedITRType);
      console.log('Mapped to numeric enum:', mappedITRType);
      console.log('Full request JSON:', JSON.stringify(generationRequest, null, 2));

      const response = await fetch(API_ENDPOINTS.ITR_GENERATE, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(generationRequest),
      });

      console.log('Generation response status:', response.status);

      if (response.ok) {
        const data: ITRGenerationResponseDto = await response.json();
        console.log('Generation response data:', data);
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
      // Map to numeric enum values same as in generation
      let mappedITRType = null;
      if (recommendation.recommendedITRType === 'ITR1_Sahaj' || recommendation.recommendedITRType === 'ITR1') {
        mappedITRType = 1; // ITR1 = 1
      } else if (recommendation.recommendedITRType === 'ITR2') {
        mappedITRType = 2; // ITR2 = 2
      } else if (recommendation.recommendedITRType === 'ITR3') {
        mappedITRType = 3; // ITR3 = 3
      } else if (recommendation.recommendedITRType === 'ITR4') {
        mappedITRType = 4; // ITR4 = 4
      }

      const endpoint = format === 'xml' ? API_ENDPOINTS.ITR_DOWNLOAD_XML : API_ENDPOINTS.ITR_DOWNLOAD_JSON;
      const generationRequest = {
        form16Data,
        additionalInfo,
        preferredITRType: mappedITRType
      };

      console.log(`Downloading ${format} file...`);

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
    <Card sx={{ maxWidth: 900, mx: 'auto', mt: 4 }}>
      <CardContent>
        <Box sx={{ display: 'flex', alignItems: 'center', mb: 3 }}>
          <Assessment sx={{ mr: 2, color: 'primary.main' }} />
          <Typography variant="h5">
            ITR Form Generation
          </Typography>
        </Box>

        <Stepper activeStep={activeStep} sx={{ mb: 4 }}>
          {steps.map((label) => (
            <Step key={label}>
              <StepLabel>{label}</StepLabel>
            </Step>
          ))}
        </Stepper>

        {error && (
          <Alert severity="error" sx={{ mb: 3 }}>
            {error}
          </Alert>
        )}

        {loading && <LinearProgress sx={{ mb: 3 }} />}

        {/* Step 0: Additional Information */}
        {activeStep === 0 && (
          <AdditionalInfoForm 
            onSubmit={handleAdditionalInfoSubmit}
            loading={loading}
          />
        )}

        {/* Step 1: ITR Recommendation */}
        {activeStep === 1 && recommendation && (
          <Box>
            <Typography variant="h6" gutterBottom>
              ITR Form Recommendation
            </Typography>
            
            <Card variant="outlined" sx={{ mb: 3, p: 2, bgcolor: 'primary.50' }}>
              <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
                <CheckCircle sx={{ color: 'success.main', mr: 1 }} />
                <Typography variant="h6" color="primary">
                  Recommended: {recommendation.recommendedITRType}
                </Typography>
              </Box>
              <Typography variant="body1" sx={{ mb: 2 }}>
                {recommendation.reason}
              </Typography>
              <Typography variant="body2" color="text.secondary">
                {recommendation.recommendationSummary}
              </Typography>
            </Card>

            <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', md: '1fr 1fr' }, gap: 3, mb: 3 }}>
              <Card variant="outlined">
                <CardContent>
                  <Typography variant="subtitle1" color="success.main" gutterBottom>
                    ✅ Requirements Met
                  </Typography>
                  <Box>
                    {recommendation.requirements.map((req, index) => (
                      <Typography key={index} variant="body2" sx={{ mb: 0.5 }}>
                        • {req}
                      </Typography>
                    ))}
                  </Box>
                </CardContent>
              </Card>

              <Card variant="outlined">
                <CardContent>
                  <Typography variant="subtitle1" color="warning.main" gutterBottom>
                    ⚠️ Limitations
                  </Typography>
                  <Box>
                    {recommendation.limitations.map((limitation, index) => (
                      <Typography key={index} variant="body2" sx={{ mb: 0.5 }}>
                        • {limitation}
                      </Typography>
                    ))}
                  </Box>
                </CardContent>
              </Card>
            </Box>

            <Box sx={{ display: 'flex', gap: 2 }}>
              <Button variant="outlined" onClick={onBack}>
                Back to Form16
              </Button>
              <Button 
                variant="contained" 
                onClick={proceedToGeneration}
                disabled={loading}
              >
                Generate ITR Form
              </Button>
            </Box>
          </Box>
        )}

        {/* Step 2: Form Generation (Loading) */}
        {activeStep === 2 && (
          <Box sx={{ textAlign: 'center', py: 4 }}>
            <Typography variant="h6" gutterBottom>
              Generating ITR Form...
            </Typography>
            <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
              Please wait while we generate your {recommendation?.recommendedITRType} form
            </Typography>
            <LinearProgress />
          </Box>
        )}

        {/* Step 3: Download */}
        {activeStep === 3 && generationResult && (
          <Box>
            <Box sx={{ display: 'flex', alignItems: 'center', mb: 3 }}>
              <CheckCircle sx={{ color: 'success.main', mr: 2 }} />
              <Typography variant="h6" color="success.main">
                ITR Form Generated Successfully!
              </Typography>
            </Box>

            <Card variant="outlined" sx={{ mb: 3, p: 2, bgcolor: 'success.50' }}>
              <Typography variant="subtitle1" gutterBottom>
                Generation Summary
              </Typography>
              <Typography variant="body2" sx={{ whiteSpace: 'pre-line' }}>
                {generationResult.generationSummary}
              </Typography>
            </Card>

            {generationResult.warnings.length > 0 && (
              <Alert severity="warning" sx={{ mb: 3 }}>
                <Typography variant="subtitle2" gutterBottom>
                  Warnings:
                </Typography>
                {generationResult.warnings.map((warning, index) => (
                  <Typography key={index} variant="body2">
                    • {warning}
                  </Typography>
                ))}
              </Alert>
            )}

            <Typography variant="h6" gutterBottom>
              Download Your ITR Form
            </Typography>

            <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', sm: '1fr 1fr' }, gap: 2, mb: 3 }}>
              <Button
                variant="contained"
                startIcon={<Download />}
                onClick={() => handleDownload('xml')}
                fullWidth
              >
                Download XML
                <Chip label="for e-filing" size="small" sx={{ ml: 1 }} />
              </Button>
              <Button
                variant="outlined"
                startIcon={<Download />}
                onClick={() => handleDownload('json')}
                fullWidth
              >
                Download JSON
                <Chip label="backup" size="small" sx={{ ml: 1 }} />
              </Button>
            </Box>

            <Divider sx={{ my: 3 }} />

            <Typography variant="h6" gutterBottom>
              Next Steps
            </Typography>
            <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
              1. Upload the XML file to the Income Tax e-filing portal<br/>
              2. Verify all details before final submission<br/>
              3. Submit your ITR before the due date<br/>
              4. Keep the JSON backup for your records
            </Typography>

            <Box sx={{ display: 'flex', gap: 2 }}>
              <Button variant="outlined" onClick={onBack}>
                Process Another Form16
              </Button>
              <Button 
                variant="contained" 
                href="https://www.incometax.gov.in/iec/foportal" 
                target="_blank"
                rel="noopener noreferrer"
              >
                Go to e-Filing Portal
              </Button>
            </Box>
          </Box>
        )}
      </CardContent>
    </Card>
  );
};

export default ITRGeneration;
