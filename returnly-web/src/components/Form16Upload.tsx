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
  IconButton
} from '@mui/material';
import { 
  CloudUpload, 
  FileUpload, 
  Security, 
  CheckCircle, 
  Error as ErrorIcon,
  Description,
  SmartToy
} from '@mui/icons-material';
import { Form16DataDto, ApiError } from '../types/api';

interface Form16UploadProps {
  onUploadSuccess: (data: Form16DataDto) => void;
}

const Form16Upload: React.FC<Form16UploadProps> = ({ onUploadSuccess }) => {
  const [file, setFile] = useState<File | null>(null);
  const [password, setPassword] = useState('');
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

  const handleFileChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const selectedFile = event.target.files?.[0];
    if (selectedFile) {
      if (selectedFile.type === 'application/pdf') {
        setFile(selectedFile);
        setError(null);
      } else {
        setError('Please select a PDF file');
        setFile(null);
      }
    }
  };

  const handleUpload = async () => {
    if (!file) {
      setError('Please select a file first');
      return;
    }

    setUploading(true);
    setError(null);

    try {
      const formData = new FormData();
      formData.append('pdfFile', file);
      if (password) {
        formData.append('password', password);
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
    } catch (err) {
      setError('Network error. Please try again.');
    } finally {
      setUploading(false);
    }
  };

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
              Upload your Form16 PDF and we'll automatically extract all tax information with AI precision
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
                label="Instant Results" 
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

      {/* Upload Form Card */}
      <Grow in timeout={800}>
        <Card sx={{ 
          borderRadius: 3,
          boxShadow: '0 4px 20px rgba(0,0,0,0.08)',
          overflow: 'hidden',
          border: '1px solid rgba(0,0,0,0.05)'
        }}>
          <CardContent sx={{ p: 4 }}>
            <Stack spacing={4}>
              {/* File Upload Section */}
              <Box>
                <Typography variant="h6" gutterBottom sx={{ 
                  fontWeight: 600,
                  display: 'flex',
                  alignItems: 'center',
                  gap: 1,
                  mb: 2
                }}>
                  <Description color="primary" />
                  Select Your Form16 PDF
                </Typography>
                
                <input
                  accept=".pdf"
                  style={{ display: 'none' }}
                  id="file-upload"
                  type="file"
                  onChange={handleFileChange}
                />
                <label htmlFor="file-upload">
                  <Card
                    component="div"
                    sx={{
                      p: 4,
                      textAlign: 'center',
                      cursor: 'pointer',
                      border: file ? '2px solid' : '2px dashed',
                      borderColor: file ? 'success.main' : 'grey.300',
                      backgroundColor: file ? 'success.50' : 'grey.50',
                      borderRadius: 2,
                      transition: 'all 0.3s ease-in-out',
                      '&:hover': {
                        borderColor: file ? 'success.dark' : 'primary.main',
                        backgroundColor: file ? 'success.100' : 'primary.50',
                        transform: 'translateY(-2px)',
                        boxShadow: '0 8px 25px rgba(0,0,0,0.1)'
                      }
                    }}
                  >
                    {file ? (
                      <CheckCircle sx={{ fontSize: 48, color: 'success.main', mb: 2 }} />
                    ) : (
                      <CloudUpload sx={{ fontSize: 48, color: 'grey.400', mb: 2 }} />
                    )}
                    <Typography variant="h6" gutterBottom sx={{ 
                      color: file ? 'success.main' : 'text.primary',
                      fontWeight: 600
                    }}>
                      {file ? file.name : 'Click to choose PDF file'}
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      {file ? 'File selected successfully!' : 'Drag and drop your PDF here or click to browse'}
                    </Typography>
                    {file && (
                      <Chip 
                        label="Ready to Upload" 
                        color="success" 
                        sx={{ mt: 2, fontWeight: 600 }} 
                      />
                    )}
                  </Card>
                </label>
              </Box>

              {/* Password Section */}
              <Box>
                <Typography variant="h6" gutterBottom sx={{ 
                  fontWeight: 600,
                  display: 'flex',
                  alignItems: 'center',
                  gap: 1,
                  mb: 2
                }}>
                  <Security color="warning" />
                  Password Protection (Optional)
                </Typography>
                
                <TextField
                  type="password"
                  label="PDF Password"
                  placeholder="Enter password if your PDF is protected"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  fullWidth
                  variant="outlined"
                  sx={{
                    '& .MuiOutlinedInput-root': {
                      borderRadius: 2,
                      backgroundColor: 'white',
                      transition: 'all 0.2s ease-in-out',
                      '&:hover': {
                        boxShadow: '0 2px 8px rgba(0,0,0,0.1)'
                      },
                      '&.Mui-focused': {
                        boxShadow: '0 4px 16px rgba(255, 152, 0, 0.2)'
                      }
                    }
                  }}
                />
              </Box>

              {/* Action Buttons */}
              <Stack spacing={3}>
                <Button
                  variant="contained"
                  onClick={handleUpload}
                  disabled={!file || uploading}
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
                  <Box>
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
                  borderRadius: 2,
                  border: '1px solid rgba(33, 150, 243, 0.2)',
                  backgroundColor: 'info.50'
                }}
              >
                <Typography variant="body2" sx={{ lineHeight: 1.6 }}>
                  <strong>How it works:</strong> Our AI system will scan your Form16 PDF and automatically extract 
                  employee details, salary information, tax deductions, and all other relevant data needed for 
                  accurate tax calculations. The process is secure and your data remains private.
                </Typography>
              </Alert>
            </Stack>
          </CardContent>
        </Card>
      </Grow>
    </Box>
  );
};

export default Form16Upload;
