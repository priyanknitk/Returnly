import { useState, useCallback } from 'react';
import { AppState } from '../types/navigation';
import { Form16DataDto } from '../types/api';

/**
 * Generic application state management hook
 */
export const useAppState = () => {
  const [appState, setAppState] = useState<AppState>({
    form16Data: null,
    taxResults: null,
    currentForm16Data: null
  });

  const updateState = useCallback(<T extends keyof AppState>(
    key: T, 
    value: AppState[T]
  ) => {
    setAppState(prev => ({ ...prev, [key]: value }));
  }, []);

  const updateMultipleState = useCallback((updates: Partial<AppState>) => {
    setAppState(prev => ({ ...prev, ...updates }));
  }, []);

  const resetState = useCallback(() => {
    setAppState({
      form16Data: null,
      taxResults: null,
      currentForm16Data: null
    });
  }, []);

  const hasRequiredData = useCallback((requirements: string[]): boolean => {
    return requirements.every(req => appState[req] !== null);
  }, [appState]);

  // Specific handlers for common operations
  const handleTaxCalculation = useCallback((results: any) => {
    const updates: Partial<AppState> = { taxResults: results };
    if (results.form16Data) {
      updates.currentForm16Data = results.form16Data;
    }
    updateMultipleState(updates);
  }, [updateMultipleState]);

  const setForm16Data = useCallback((data: Form16DataDto | null) => {
    updateState('form16Data', data);
  }, [updateState]);

  return {
    appState,
    updateState,
    updateMultipleState,
    resetState,
    hasRequiredData,
    handleTaxCalculation,
    setForm16Data
  };
};
