import React from 'react';
import { Box, Typography, Button } from '@mui/material';
import { useAppNavigation } from '../hooks/useAppNavigation';
import { routeRequirements } from '../config/navigation';
import { AppState } from '../types/navigation';

interface PageWrapperProps {
  appState: AppState;
  requiredData?: string[];
  fallbackRoute?: string;
  fallbackMessage?: string;
  children: React.ReactNode;
}

/**
 * Generic page wrapper that handles data requirements and navigation
 */
export const PageWrapper: React.FC<PageWrapperProps> = ({
  appState,
  requiredData = [],
  fallbackRoute = '/file-returns',
  fallbackMessage = 'Required data is missing. Please complete the previous steps.',
  children
}) => {
  const { navigateTo } = useAppNavigation();

  // Check if all required data is available
  const hasRequiredData = requiredData.every(key => appState[key] !== null);

  if (!hasRequiredData) {
    return (
      <Box sx={{ textAlign: 'center', mt: 4 }}>
        <Typography variant="h5" color="error" gutterBottom>
          Data Required
        </Typography>
        <Typography variant="body1" sx={{ mb: 3 }}>
          {fallbackMessage}
        </Typography>
        <Button 
          variant="outlined" 
          onClick={() => navigateTo(fallbackRoute)}
          sx={{ mt: 2 }}
        >
          Go to {fallbackRoute === '/file-returns' ? 'File Returns' : 'Previous Step'}
        </Button>
      </Box>
    );
  }

  return <>{children}</>;
};

/**
 * HOC factory for creating route-specific wrappers
 */
export const createRouteWrapper = <TProps extends object>(
  Component: React.ComponentType<TProps>,
  routePath: string,
  options: {
    fallbackRoute?: string;
    fallbackMessage?: string;
    dataMapper?: (appState: AppState) => Partial<TProps>;
  } = {}
) => {
  return React.forwardRef<any, TProps & { appState: AppState }>((props, ref) => {
    const { appState, ...componentProps } = props;
    const requiredData = routeRequirements[routePath] || [];
    const mappedProps = options.dataMapper ? options.dataMapper(appState) : {};

    return (
      <PageWrapper
        appState={appState}
        requiredData={requiredData}
        fallbackRoute={options.fallbackRoute}
        fallbackMessage={options.fallbackMessage}
      >
        <Component 
          ref={ref}
          {...(componentProps as TProps)}
          {...mappedProps}
        />
      </PageWrapper>
    );
  });
};
