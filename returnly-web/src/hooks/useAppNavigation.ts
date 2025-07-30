import { useNavigate } from 'react-router-dom';
import { useCallback } from 'react';
import { routeRedirects, routePaths } from '../config/navigation';

/**
 * Generic navigation hook with predefined routes and redirects
 */
export const useAppNavigation = () => {
  const navigate = useNavigate();

  const navigateTo = useCallback((path: string) => {
    // Check if this path should be redirected
    if (routeRedirects[path]) {
      navigate(routeRedirects[path], { replace: true });
      return;
    }
    navigate(path);
  }, [navigate]);

  const navigateToHome = useCallback(() => navigateTo(routePaths.HOME), [navigateTo]);
  const navigateToFileReturns = useCallback(() => navigateTo(routePaths.FILE_RETURNS), [navigateTo]);
  const navigateToResults = useCallback(() => navigateTo(routePaths.RESULTS), [navigateTo]);
  const navigateToITRGeneration = useCallback(() => navigateTo(routePaths.ITR_GENERATION), [navigateTo]);

  return {
    navigateTo,
    navigateToHome,
    navigateToFileReturns,
    navigateToResults,
    navigateToITRGeneration,
    goBack: () => navigate(-1)
  };
};
