import React from 'react';
import { useLocation } from 'react-router-dom';
import { 
  AppBar, 
  Toolbar, 
  Typography, 
  Box, 
  Button, 
  Stack, 
  Container 
} from '@mui/material';
import { Assessment } from '@mui/icons-material';
import { useAppNavigation } from '../hooks/useAppNavigation';
import { navigationConfig } from '../config/navigation';
import { AppState } from '../types/navigation';

interface ModernNavigationProps {
  appState: AppState;
}

/**
 * Generic navigation component that uses configuration
 */
export const ModernNavigation: React.FC<ModernNavigationProps> = ({ appState }) => {
  const { navigateTo } = useAppNavigation();
  const location = useLocation();

  return (
    <AppBar 
      position="sticky" 
      elevation={0}
      sx={{
        background: 'rgba(255, 255, 255, 0.98)',
        backdropFilter: 'blur(20px)',
        borderBottom: '1px solid rgba(0, 0, 0, 0.08)',
        boxShadow: 'none'
      }}
    >
      <Container maxWidth="xl">
        <Toolbar sx={{ 
          py: 1.5, 
          justifyContent: 'space-between',
          minHeight: 64
        }}>
          {/* Elegant Logo */}
          <Box 
            sx={{
              display: 'flex',
              alignItems: 'center',
              cursor: 'pointer',
              '&:hover': {
                '& .logo-icon': {
                  transform: 'rotate(5deg) scale(1.05)'
                }
              }
            }} 
            onClick={() => navigateTo('/')}
          >
            <Box 
              className="logo-icon"
              sx={{
                width: 36,
                height: 36,
                borderRadius: 2,
                background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                mr: 2,
                transition: 'all 0.3s cubic-bezier(0.4, 0, 0.2, 1)'
              }}
            >
              <Assessment sx={{ fontSize: 18, color: 'white' }} />
            </Box>
            <Box>
              <Typography sx={{ 
                fontSize: '1.25rem',
                fontWeight: 700,
                color: '#1a1a1a',
                lineHeight: 1,
                letterSpacing: '-0.02em'
              }}>
                Returnly
              </Typography>
              <Typography sx={{ 
                fontSize: '0.65rem',
                color: 'text.secondary',
                fontWeight: 500,
                lineHeight: 1,
                mt: 0.25,
                letterSpacing: '0.02em'
              }}>
                TAX SOLUTIONS
              </Typography>
            </Box>
          </Box>

          {/* Generic Navigation */}
          <Stack 
            direction="row" 
            spacing={0.5} 
            sx={{ 
              display: { xs: 'none', md: 'flex' },
              background: 'rgba(0, 0, 0, 0.04)',
              borderRadius: 2,
              p: 0.5
            }}
          >
            {navigationConfig.map((item) => {
              const isActive = location.pathname === item.path;
              const IconComponent = item.icon;
              
              // Check if navigation item should be enabled based on requirements
              const isEnabled = !item.requiresData || 
                item.requiresData.every(key => appState[key] !== null);
              
              return (
                <Button
                  key={item.path}
                  onClick={() => navigateTo(item.path)}
                  startIcon={<IconComponent sx={{ fontSize: 18 }} />}
                  disabled={!isEnabled}
                  sx={{
                    px: 3,
                    py: 1.25,
                    borderRadius: 1.5,
                    textTransform: 'none',
                    fontWeight: isActive ? 600 : 500,
                    fontSize: '0.875rem',
                    color: isActive ? 'white' : '#6b7280',
                    background: isActive 
                      ? 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)'
                      : 'transparent',
                    boxShadow: isActive ? '0 2px 8px rgba(102, 126, 234, 0.3)' : 'none',
                    minWidth: 'auto',
                    '&:hover': {
                      background: isActive 
                        ? 'linear-gradient(135deg, #5a6fd8 0%, #6a4190 100%)'
                        : 'rgba(255, 255, 255, 0.8)',
                      color: isActive ? 'white' : '#374151',
                      transform: 'translateY(-0.5px)'
                    },
                    '&:disabled': {
                      color: '#d1d5db',
                      cursor: 'not-allowed'
                    },
                    transition: 'all 0.2s cubic-bezier(0.4, 0, 0.2, 1)'
                  }}
                >
                  {item.label}
                </Button>
              );
            })}
          </Stack>

          {/* Elegant Status */}
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <Typography
              variant="caption"
              sx={{
                background: 'linear-gradient(135deg, #10b981 0%, #059669 100%)',
                backgroundClip: 'text',
                WebkitBackgroundClip: 'text',
                color: 'transparent',
                fontWeight: 700,
                fontSize: '0.75rem',
                letterSpacing: '0.1em',
                display: { xs: 'none', sm: 'block' }
              }}
            >
              BETA
            </Typography>
            <Box
              sx={{
                width: 8,
                height: 8,
                borderRadius: '50%',
                background: 'linear-gradient(135deg, #10b981 0%, #059669 100%)',
                animation: 'pulse 2s infinite',
                '@keyframes pulse': {
                  '0%, 100%': { opacity: 1 },
                  '50%': { opacity: 0.5 }
                }
              }}
            />
          </Box>
        </Toolbar>
      </Container>
    </AppBar>
  );
};
