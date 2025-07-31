import React, { useState } from 'react';
import {
  Card,
  CardContent,
  Typography,
  Button,
  Box,
  LinearProgress,
  Alert,
  TextField,
  Stack,
  Fade,
  Grow,
  Chip,
  IconButton,
  FormControl,
  FormLabel,
  RadioGroup,
  FormControlLabel,
  Radio,
  Divider
} from '@mui/material';
import { 
  CloudUpload, 
  FileUpload, 
  Security, 
  CheckCircle, 
  Error as ErrorIcon,
  Description,
  SmartToy,
  AttachFile,
  DeleteOutline
} from '@mui/icons-material';
import { Form16DataDto, ApiError } from '../types/api';

interface Form16File {
  file: File;
  password?: string;
}

interface Form16UploadProps {
  onUploadSuccess: (data: Form16DataDto) => void;
}

const Form16Upload: React.FC<Form16UploadProps> = ({ onUploadSuccess }) => {
  const [uploadMode, setUploadMode] = useState<'combined' | 'separate'>('combined');
  const [combinedFile, setCombinedFile] = useState<Form16File | null>(null);
  const [form16AFile, setForm16AFile] = useState<Form16File | null>(null);
  const [form16BFile, setForm16BFile] = useState<Form16File | null>(null);
  const [uploading, setUploading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [testing, setTesting] = useState(false);

  const handleTestAPI = async () => {
    setTesting(true);
    setError(null);

    try {
      const response = await fetch('http://localhost:5201/api/form16/sample', {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
        },
      });

      if (response.ok) {
        const sampleData: Form16DataDto = await response.json();
        onUploadSuccess(sampleData);
      } else {
        setError('API connection failed');
      }
    } catch (err) {
      setError('Cannot connect to backend API. Make sure the API server is running.');
    } finally {
      setTesting(false);
    }
  };

  const handleFileChange = (fileType: 'combined' | 'form16A' | 'form16B') => 
    (event: React.ChangeEvent<HTMLInputElement>) => {
      const selectedFile = event.target.files?.[0];
      if (selectedFile) {
        if (selectedFile.type === 'application/pdf') {
          const newFile: Form16File = { file: selectedFile };
          
          switch (fileType) {
            case 'combined':
              setCombinedFile(newFile);
              break;
            case 'form16A':
              setForm16AFile(newFile);
              break;
            case 'form16B':
              setForm16BFile(newFile);
              break;
          }
          setError(null);
        } else {
          setError('Please select a PDF file');
        }
      }
    };

  const updateFilePassword = (fileType: 'combined' | 'form16A' | 'form16B', password: string) => {
    switch (fileType) {
      case 'combined':
        if (combinedFile) {
          setCombinedFile({ ...combinedFile, password });
        }
        break;
      case 'form16A':
        if (form16AFile) {
          setForm16AFile({ ...form16AFile, password });
        }
        break;
      case 'form16B':
        if (form16BFile) {
          setForm16BFile({ ...form16BFile, password });
        }
        break;
    }
  };

  const removeFile = (fileType: 'combined' | 'form16A' | 'form16B') => {
    switch (fileType) {
      case 'combined':
        setCombinedFile(null);
        break;
      case 'form16A':
        setForm16AFile(null);
        break;
      case 'form16B':
        setForm16BFile(null);
        break;
    }
  };

  const handleUpload = async () => {
    if (uploadMode === 'combined' && !combinedFile) {
      setError('Please select a Form16 PDF file first');
      return;
    }

    if (uploadMode === 'separate' && !form16AFile && !form16BFile) {
      setError('Please select at least one Form16 file (A or B)');
      return;
    }

    setUploading(true);
    setError(null);

    try {
      const formData = new FormData();

      if (uploadMode === 'combined') {
        formData.append('pdfFile', combinedFile!.file);
        if (combinedFile!.password) {
          formData.append('password', combinedFile!.password);
        }
        
        const response = await fetch('http://localhost:5201/api/form16/upload', {
          method: 'POST',
          body: formData,
        });

        if (response.ok) {
          const data: Form16DataDto = await response.json();
          onUploadSuccess(data);
        } else {
          const errorData: ApiError = await response.json();
          setError(errorData.error || 'Upload failed');
        }
      } else {
        // Separate files upload
        if (form16AFile) {
          formData.append('form16AFile', form16AFile.file);
          if (form16AFile.password) {
            formData.append('form16APassword', form16AFile.password);
          }
        }
        
        if (form16BFile) {
          formData.append('form16BFile', form16BFile.file);
          if (form16BFile.password) {
            formData.append('form16BPassword', form16BFile.password);
          }
        }

        const response = await fetch('http://localhost:5201/api/form16/upload-multiple', {
          method: 'POST',
          body: formData,
        });

        if (response.ok) {
          const data: Form16DataDto = await response.json();
          onUploadSuccess(data);
        } else {
          const errorData: ApiError = await response.json();
          setError(errorData.error || 'Upload failed');
        }
      }
    } catch (err) {
      setError('Network error. Please try again.');
    } finally {
      setUploading(false);
    }
  };

  const renderFileUploadCard = (
    fileType: 'combined' | 'form16A' | 'form16B',
    title: string,
    description: string,
    currentFile: Form16File | null
  ) => (
    <Card
      sx={{
        border: currentFile ? '2px solid' : '2px dashed',
        borderColor: currentFile ? 'success.main' : 'grey.300',
        backgroundColor: currentFile ? 'success.50' : 'grey.50',
        borderRadius: 2,
        transition: 'all 0.3s ease-in-out',
        '&:hover': {
          borderColor: currentFile ? 'success.dark' : 'primary.main',
          backgroundColor: currentFile ? 'success.100' : 'primary.50',
          transform: 'translateY(-2px)',
          boxShadow: '0 8px 25px rgba(0,0,0,0.1)'
        }
      }}
    >
      <CardContent sx={{ p: 3 }}>
        <Box sx={{ textAlign: 'center', mb: 2 }}>
          {currentFile ? (
            <CheckCircle sx={{ fontSize: 40, color: 'success.main', mb: 1 }} />
          ) : (
            <CloudUpload sx={{ fontSize: 40, color: 'grey.400', mb: 1 }} />
          )}
          <Typography variant="h6" gutterBottom sx={{ 
            color: currentFile ? 'success.main' : 'text.primary',
            fontWeight: 600
          }}>
            {title}
          </Typography>
          <Typography variant="body2" color="text.secondary">
            {description}
          </Typography>
        </Box>

        {currentFile ? (
          <Box>
            <Stack direction="row" spacing={2} alignItems="center" justifyContent="center" sx={{ mb: 2 }}>
              <AttachFile sx={{ color: 'success.main' }} />
              <Typography variant="body2" sx={{ fontWeight: 500 }}>
                {currentFile.file.name}
              </Typography>
              <IconButton 
                size="small" 
                onClick={() => removeFile(fileType)}
                sx={{ color: 'error.main' }}
              >
                <DeleteOutline />
              </IconButton>
            </Stack>
            <TextField
              type="password"
              label="Password (if protected)"
              size="small"
              fullWidth
              value={currentFile.password || ''}
              onChange={(e) => updateFilePassword(fileType, e.target.value)}
              sx={{ mt: 1 }}
            />
          </Box>
        ) : (
          <Box>
            <input
              accept=".pdf"
              style={{ display: 'none' }}
              id={`file-upload-${fileType}`}
              type="file"
              onChange={handleFileChange(fileType)}
            />
            <label htmlFor={`file-upload-${fileType}`}>
              <Button
                component="span"
                variant="outlined"
                startIcon={<CloudUpload />}
                fullWidth
                sx={{ 
                  py: 1.5,
                  borderStyle: 'dashed',
                  '&:hover': {
                    borderStyle: 'solid'
                  }
                }}
              >
                Choose PDF File
              </Button>
            </label>
          </Box>
        )}
      </CardContent>
    </Card>
  );

  const canUpload = uploadMode === 'combined' 
    ? combinedFile !== null 
    : (form16AFile !== null || form16BFile !== null);

  return (
    <Box sx={{ maxWidth: 900, mx: 'auto', mt: 2, px: { xs: 1, sm: 2, md: 3 } }}>
      {/* Header Card */}
      <Fade in timeout={600}>
        <Card sx={{ 
          mb: 3,
          background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
          color: 'white',
          borderRadius: 3,
          boxShadow: '0 8px 32px rgba(102, 126, 234, 0.3)'
        }}>
          <CardContent sx={{ textAlign: 'center', py: 2.5 }}>
            <FileUpload sx={{ fontSize: 32, mb: 1.5, opacity: 0.9 }} />
            <Typography variant="h5" gutterBottom sx={{ fontWeight: 700, mb: 1 }}>
              Upload Form16 PDF
            </Typography>
            <Typography variant="body1" sx={{ opacity: 0.9, maxWidth: 500, mx: 'auto' }}>
              Upload your Form16 PDF files and we'll automatically extract all tax information with AI precision
            </Typography>
            <Stack direction="row" spacing={2} justifyContent="center" sx={{ mt: 3 }}>
              <Chip 
                label="AI Powered" 
                sx={{ 
                  backgroundColor: 'rgba(255,255,255,0.2)', 
                  color: 'white',
                  fontWeight: 600
                }} 
              />
              <Chip 
                label="Secure Processing" 
                sx={{ 
                  backgroundColor: 'rgba(255,255,255,0.2)', 
                  color: 'white',
                  fontWeight: 600
                }} 
              />
              <Chip 
                label="Multiple Formats" 
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

      {/* Upload Mode Selection */}
      <Grow in timeout={800}>
        <Card sx={{ 
          mb: 3,
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
              <Description color="primary" />
              Form16 Upload Options
            </Typography>
            
            <FormControl component="fieldset">
              <RadioGroup
                value={uploadMode}
                onChange={(e) => setUploadMode(e.target.value as 'combined' | 'separate')}
                row
              >
                <FormControlLabel 
                  value="combined" 
                  control={<Radio />} 
                  label="Combined Form16 (single PDF with both A & B parts)" 
                />
                <FormControlLabel 
                  value="separate" 
                  control={<Radio />} 
                  label="Separate Files (Form16A and/or Form16B)" 
                />
              </RadioGroup>
            </FormControl>

            <Alert severity="info" sx={{ mt: 2, borderRadius: 2 }}>
              <Typography variant="body2">
                {uploadMode === 'combined' 
                  ? "Upload a single PDF containing both Form16A (TDS Certificate) and Form16B (Tax Computation) sections."
                  : "Upload separate Form16A (TDS Certificate) and Form16B (Tax Computation) PDF files. You can upload one or both files."
                }
              </Typography>
            </Alert>
          </CardContent>
        </Card>
      </Grow>

      {/* File Upload Section */}
      <Grow in timeout={1000}>
        <Card sx={{ 
          mb: 3,
          borderRadius: 3,
          boxShadow: '0 4px 20px rgba(0,0,0,0.08)',
          border: '1px solid rgba(0,0,0,0.05)'
        }}>
          <CardContent sx={{ p: 4 }}>
            {uploadMode === 'combined' ? (
              <Box>
                <Typography variant="h6" gutterBottom sx={{ 
                  fontWeight: 600,
                  mb: 3
                }}>
                  Upload Combined Form16 PDF
                </Typography>
                {renderFileUploadCard(
                  'combined',
                  'Form16 PDF',
                  'Upload your complete Form16 document',
                  combinedFile
                )}
              </Box>
            ) : (
              <Box>
                <Typography variant="h6" gutterBottom sx={{ 
                  fontWeight: 600,
                  mb: 3
                }}>
                  Upload Form16 Parts Separately
                </Typography>
                <Box sx={{ 
                  display: 'grid', 
                  gridTemplateColumns: { xs: '1fr', md: '1fr 1fr' }, 
                  gap: 3 
                }}>
                  {renderFileUploadCard(
                    'form16A',
                    'Form16A (TDS Certificate)',
                    'Tax Deducted at Source certificate with quarterly TDS details',
                    form16AFile
                  )}
                  {renderFileUploadCard(
                    'form16B',
                    'Form16B (Tax Computation)',
                    'Detailed salary breakdown and tax computation',
                    form16BFile
                  )}
                </Box>
              </Box>
            )}

            <Divider sx={{ my: 4 }} />

            {/* Action Buttons */}
            <Stack spacing={3}>
              <Button
                variant="contained"
                onClick={handleUpload}
                disabled={!canUpload || uploading}
                size="large"
                startIcon={uploading ? undefined : <FileUpload />}
                sx={{ 
                  py: 2,
                  fontSize: '1.1rem',
                  fontWeight: 600,
                  borderRadius: 3,
                  background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
                  boxShadow: '0 8px 25px rgba(102, 126, 234, 0.4)',
                  transition: 'all 0.3s ease-in-out',
                  '&:hover': {
                    background: 'linear-gradient(135deg, #5a6fd8 0%, #6a4190 100%)',
                    transform: 'translateY(-2px)',
                    boxShadow: '0 12px 35px rgba(102, 126, 234, 0.5)'
                  },
                  '&:disabled': {
                    background: 'linear-gradient(135deg, #e0e0e0 0%, #bdbdbd 100%)',
                    color: 'rgba(0,0,0,0.4)',
                    boxShadow: 'none'
                  }
                }}
              >
                {uploading ? 'Processing Your PDF...' : 'Upload and Extract Data'}
              </Button>

              <Box sx={{ textAlign: 'center', position: 'relative' }}>
                <Typography variant="body2" color="text.secondary" sx={{ 
                  position: 'relative',
                  zIndex: 2,
                  backgroundColor: 'white',
                  px: 2,
                  display: 'inline-block'
                }}>
                  OR
                </Typography>
                <Box sx={{
                  position: 'absolute',
                  top: '50%',
                  left: 0,
                  right: 0,
                  height: 1,
                  backgroundColor: 'grey.300',
                  zIndex: 1
                }} />
              </Box>

              <Button
                variant="outlined"
                onClick={handleTestAPI}
                disabled={testing}
                size="large"
                startIcon={testing ? undefined : <SmartToy />}
                sx={{ 
                  py: 2,
                  fontSize: '1.1rem',
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
                {testing ? 'Loading Sample Data...' : 'Try with Sample Data'}
              </Button>
            </Stack>

            {/* Progress Bar */}
            {(uploading || testing) && (
              <Fade in>
                <Box sx={{ mt: 3 }}>
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
                  <Typography variant="body2" color="text.secondary" sx={{ mt: 1, textAlign: 'center' }}>
                    {uploading ? 'Extracting data from your PDF...' : 'Loading sample data...'}
                  </Typography>
                </Box>
              </Fade>
            )}

            {/* Error Display */}
            {error && (
              <Fade in>
                <Alert 
                  severity="error" 
                  icon={<ErrorIcon />}
                  sx={{ 
                    mt: 3,
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

            {/* Information Card */}
            <Alert 
              severity="info" 
              sx={{ 
                mt: 3,
                borderRadius: 2,
                border: '1px solid rgba(33, 150, 243, 0.2)',
                backgroundColor: 'info.50'
              }}
            >
              <Typography variant="body2" sx={{ lineHeight: 1.6 }}>
                <strong>How it works:</strong> Our AI system will scan your Form16 PDF files and automatically extract 
                employee details, salary information, tax deductions, and all other relevant data needed for 
                accurate tax calculations. The process is secure and your data remains private.
              </Typography>
            </Alert>
          </CardContent>
        </Card>
      </Grow>
    </Box>
  );
};

export default Form16Upload;
