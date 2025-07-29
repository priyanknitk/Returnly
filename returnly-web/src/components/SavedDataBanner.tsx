import React, { useState } from 'react';
import {
  Alert,
  AlertTitle,
  Box,
  Button,
  Chip,
  Dialog,
  DialogActions,
  DialogContent,
  DialogContentText,
  DialogTitle,
  IconButton,
  Snackbar,
  Typography,
  Stack,
  Collapse
} from '@mui/material';
import {
  Save as SaveIcon,
  Restore as RestoreIcon,
  Delete as DeleteIcon,
  CheckCircle as CheckCircleIcon,
  Schedule as ScheduleIcon,
  ExpandMore as ExpandMoreIcon,
  ExpandLess as ExpandLessIcon
} from '@mui/icons-material';
import { useTaxDataPersistence } from '../hooks/useTaxDataPersistence';

interface SavedDataBannerProps {
  onRestore?: () => void;
  showSaveButton?: boolean;
  onSave?: () => void;
}

const SavedDataBanner: React.FC<SavedDataBannerProps> = ({ 
  onRestore, 
  showSaveButton = false,
  onSave 
}) => {
  const { 
    hasSavedData, 
    getLastSavedMessage, 
    clearAllData,
    personalInfo,
    form16Data 
  } = useTaxDataPersistence();
  
  const [showClearDialog, setShowClearDialog] = useState(false);
  const [showDetails, setShowDetails] = useState(false);
  const [showSnackbar, setShowSnackbar] = useState(false);
  const [snackbarMessage, setSnackbarMessage] = useState('');

  const handleSave = () => {
    if (onSave) {
      onSave();
      setSnackbarMessage('Data saved successfully!');
      setShowSnackbar(true);
    }
  };

  const handleRestore = () => {
    if (onRestore) {
      onRestore();
      setSnackbarMessage('Data restored successfully!');
      setShowSnackbar(true);
    }
  };

  const handleClearData = () => {
    clearAllData();
    setShowClearDialog(false);
    setSnackbarMessage('All saved data cleared!');
    setShowSnackbar(true);
  };

  const getSavedDataSummary = () => {
    const items = [];
    // Only show personal details if they're different from sample data
    if (personalInfo.emailAddress && 
        personalInfo.emailAddress !== 'sample.user@example.test' &&
        personalInfo.emailAddress.includes('@')) {
      items.push('Personal Details');
    }
    if (form16Data) {
      items.push('Form16 Data');
    }
    return items;
  };

  if (!hasSavedData()) {
    // Only show save button if there's meaningful data to save
    const hasDataToSave = showSaveButton && 
                         personalInfo.emailAddress && 
                         personalInfo.emailAddress !== 'sample.user@example.test' &&
                         personalInfo.emailAddress.includes('@');
    
    return hasDataToSave ? (
      <Box sx={{ mb: 2 }}>
        <Button
          variant="outlined"
          startIcon={<SaveIcon />}
          onClick={handleSave}
          size="small"
          sx={{ 
            borderRadius: 2,
            textTransform: 'none'
          }}
        >
          Save Progress
        </Button>
      </Box>
    ) : null;
  }

  const savedItems = getSavedDataSummary();
  const lastSavedMessage = getLastSavedMessage();

  return (
    <>
      <Alert 
        severity="info" 
        variant="outlined"
        sx={{ 
          mb: 3,
          borderRadius: 2,
          border: '1px solid rgba(2, 136, 209, 0.3)',
          backgroundColor: 'rgba(2, 136, 209, 0.05)'
        }}
        action={
          <Stack direction="row" spacing={1} alignItems="center">
            {showSaveButton && (
              <Button
                size="small"
                startIcon={<SaveIcon />}
                onClick={handleSave}
                sx={{ textTransform: 'none' }}
              >
                Save
              </Button>
            )}
            {onRestore && (
              <Button
                size="small"
                startIcon={<RestoreIcon />}
                onClick={handleRestore}
                sx={{ textTransform: 'none' }}
              >
                Restore
              </Button>
            )}
            <IconButton 
              size="small" 
              onClick={() => setShowDetails(!showDetails)}
              sx={{ ml: 1 }}
            >
              {showDetails ? <ExpandLessIcon /> : <ExpandMoreIcon />}
            </IconButton>
          </Stack>
        }
      >
        <AlertTitle sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <CheckCircleIcon fontSize="small" />
          Progress Saved
        </AlertTitle>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
          <ScheduleIcon fontSize="small" sx={{ opacity: 0.7 }} />
          <Typography variant="body2">
            {lastSavedMessage}
          </Typography>
        </Box>
        
        <Collapse in={showDetails}>
          <Box sx={{ mt: 2 }}>
            <Typography variant="body2" sx={{ mb: 1, fontWeight: 500 }}>
              Saved Data:
            </Typography>
            <Stack direction="row" spacing={1} flexWrap="wrap" sx={{ gap: 1 }}>
              {savedItems.map((item, index) => (
                <Chip 
                  key={index}
                  label={item}
                  size="small"
                  color="primary"
                  variant="outlined"
                />
              ))}
            </Stack>
            <Box sx={{ mt: 2 }}>
              <Button
                size="small"
                startIcon={<DeleteIcon />}
                onClick={() => setShowClearDialog(true)}
                color="error"
                sx={{ textTransform: 'none' }}
              >
                Clear All Saved Data
              </Button>
            </Box>
          </Box>
        </Collapse>
      </Alert>

      {/* Clear Data Confirmation Dialog */}
      <Dialog
        open={showClearDialog}
        onClose={() => setShowClearDialog(false)}
        maxWidth="sm"
        fullWidth
      >
        <DialogTitle>Clear Saved Data?</DialogTitle>
        <DialogContent>
          <DialogContentText>
            This will permanently remove all your saved progress including personal details,
            Form16 data, and additional information. You'll need to start over from the beginning.
          </DialogContentText>
          <Box sx={{ mt: 2 }}>
            <Typography variant="subtitle2" gutterBottom>
              Data to be cleared:
            </Typography>
            <Stack direction="row" spacing={1} flexWrap="wrap" sx={{ gap: 1 }}>
              {savedItems.map((item, index) => (
                <Chip 
                  key={index}
                  label={item}
                  size="small"
                  color="error"
                  variant="outlined"
                />
              ))}
            </Stack>
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setShowClearDialog(false)}>
            Cancel
          </Button>
          <Button 
            onClick={handleClearData} 
            color="error" 
            variant="contained"
          >
            Clear All Data
          </Button>
        </DialogActions>
      </Dialog>

      {/* Success Snackbar */}
      <Snackbar
        open={showSnackbar}
        autoHideDuration={3000}
        onClose={() => setShowSnackbar(false)}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}
      >
        <Alert 
          onClose={() => setShowSnackbar(false)} 
          severity="success"
          variant="filled"
          sx={{ borderRadius: 2 }}
        >
          {snackbarMessage}
        </Alert>
      </Snackbar>
    </>
  );
};

export default SavedDataBanner;
