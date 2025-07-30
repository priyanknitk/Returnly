import { NavigationItem, RouteConfig } from '../types/navigation';
import { Assessment, Home } from '@mui/icons-material';

// Generic navigation configuration
export const navigationConfig: NavigationItem[] = [
  { 
    path: '/', 
    label: 'Home', 
    icon: Home 
  },
  { 
    path: '/file-returns', 
    label: 'File Returns', 
    icon: Assessment 
  }
];

// Generic route redirects configuration
export const routeRedirects: Record<string, string> = {
  '/upload': '/file-returns',
  '/calculate': '/file-returns'
};

// Generic route configuration - elements will be created at runtime
export const routePaths = {
  HOME: '/',
  FILE_RETURNS: '/file-returns',
  RESULTS: '/results',
  ITR_GENERATION: '/itr-generation'
} as const;

export const routeRequirements: Record<string, string[]> = {
  [routePaths.RESULTS]: ['taxResults'],
  [routePaths.ITR_GENERATION]: ['form16Data']
};
