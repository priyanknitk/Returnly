import React, { useState } from 'react';
import {
  Card,
  CardContent,
  Typography,
  Button,
  Box,
  LinearProgress,
  Alert,
  Input
} from '@mui/material';
import { CloudUpload } from '@mui/icons-material';
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
    <Card sx={{ maxWidth: 600, mx: 'auto', mt: 4 }}>
      <CardContent>
        <Typography variant="h5" gutterBottom>
          Upload Form16 PDF
        </Typography>
        <Typography variant="body2" color="text.secondary" gutterBottom>
          Upload your Form16 PDF to extract tax information automatically
        </Typography>

        <Box sx={{ mt: 3 }}>
          <input
            accept=".pdf"
            style={{ display: 'none' }}
            id="file-upload"
            type="file"
            onChange={handleFileChange}
          />
          <label htmlFor="file-upload">
            <Button
              variant="outlined"
              component="span"
              startIcon={<CloudUpload />}
              fullWidth
              sx={{ mb: 2 }}
            >
              {file ? file.name : 'Choose PDF File'}
            </Button>
          </label>

          <Input
            type="password"
            placeholder="PDF Password (if required)"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            fullWidth
            sx={{ mb: 2 }}
          />

          <Button
            variant="contained"
            onClick={handleUpload}
            disabled={!file || uploading}
            fullWidth
            sx={{ mb: 2 }}
          >
            {uploading ? 'Uploading...' : 'Upload and Process'}
          </Button>

          <Button
            variant="outlined"
            onClick={handleTestAPI}
            disabled={testing}
            fullWidth
            sx={{ mb: 2 }}
          >
            {testing ? 'Testing...' : 'Test with Sample Data'}
          </Button>

          {(uploading || testing) && <LinearProgress sx={{ mb: 2 }} />}

          {error && (
            <Alert severity="error" sx={{ mb: 2 }}>
              {error}
            </Alert>
          )}
        </Box>
      </CardContent>
    </Card>
  );
};

export default Form16Upload;
