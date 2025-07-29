import { useLocalStorage, clearLocalStorageItems } from './useLocalStorage';
import { AdditionalTaxpayerInfoDto, Form16DataDto } from '../types/api';
import { DEFAULT_PERSONAL_INFO } from '../constants/defaultValues';

// Local storage keys
const STORAGE_KEYS = {
  PERSONAL_INFO: 'returnly_personal_info',
  FORM16_DATA: 'returnly_form16_data',
  ADDITIONAL_INFO: 'returnly_additional_info',
  CURRENT_STEP: 'returnly_current_step',
  LAST_SAVED: 'returnly_last_saved'
} as const;

export interface SavedTaxData {
  personalInfo: AdditionalTaxpayerInfoDto;
  form16Data: any | null; // Can store either Form16DataDto or TaxData
  additionalInfo: Partial<AdditionalTaxpayerInfoDto>;
  currentStep: number;
  lastSaved: string;
}

/**
 * Custom hook for managing tax-related data persistence in localStorage
 */
export function useTaxDataPersistence() {
  const [personalInfo, setPersonalInfo] = useLocalStorage<AdditionalTaxpayerInfoDto>(
    STORAGE_KEYS.PERSONAL_INFO, 
    DEFAULT_PERSONAL_INFO
  );
  
  const [form16Data, setForm16Data] = useLocalStorage<any | null>(
    STORAGE_KEYS.FORM16_DATA, 
    null
  );
  
  const [additionalInfo, setAdditionalInfo] = useLocalStorage<Partial<AdditionalTaxpayerInfoDto>>(
    STORAGE_KEYS.ADDITIONAL_INFO, 
    {}
  );
  
  const [currentStep, setCurrentStep] = useLocalStorage<number>(
    STORAGE_KEYS.CURRENT_STEP, 
    0
  );
  
  const [lastSaved, setLastSaved] = useLocalStorage<string>(
    STORAGE_KEYS.LAST_SAVED, 
    ''
  );

  /**
   * Save personal information and update last saved timestamp
   */
  const savePersonalInfo = (info: AdditionalTaxpayerInfoDto) => {
    setPersonalInfo(info);
    setLastSaved(new Date().toISOString());
  };

  /**
   * Save Form16 data and update last saved timestamp
   * Note: This can store either Form16DataDto or TaxData format
   */
  const saveForm16Data = (data: any | null) => {
    setForm16Data(data);
    setLastSaved(new Date().toISOString());
  };

  /**
   * Save additional information and update last saved timestamp
   */
  const saveAdditionalInfo = (info: Partial<AdditionalTaxpayerInfoDto>) => {
    setAdditionalInfo(info);
    setLastSaved(new Date().toISOString());
  };

  /**
   * Save current step and update last saved timestamp
   */
  const saveCurrentStep = (step: number) => {
    setCurrentStep(step);
    setLastSaved(new Date().toISOString());
  };

  /**
   * Get all saved data as a single object
   */
  const getSavedData = (): SavedTaxData => ({
    personalInfo,
    form16Data,
    additionalInfo,
    currentStep,
    lastSaved
  });

  /**
   * Check if there is any meaningful saved data
   */
  const hasSavedData = (): boolean => {
    // Check if personal info is different from default sample data
    const hasRealPersonalInfo = personalInfo.emailAddress !== 'sample.user@example.test' ||
                               personalInfo.mobileNumber !== '9123456789' ||
                               personalInfo.fatherName !== 'Sample Father Name';
    
    // Check if we have form16 data
    const hasForm16Data = form16Data !== null;
    
    // Check if we have additional info
    const hasAdditionalInfo = Object.keys(additionalInfo).length > 0;
    
    return hasRealPersonalInfo || hasForm16Data || hasAdditionalInfo;
  };

  /**
   * Clear all saved data
   */
  const clearAllData = () => {
    clearLocalStorageItems(Object.values(STORAGE_KEYS));
    setPersonalInfo(DEFAULT_PERSONAL_INFO);
    setForm16Data(null);
    setAdditionalInfo({});
    setCurrentStep(0);
    setLastSaved('');
  };

  /**
   * Get a user-friendly last saved message
   */
  const getLastSavedMessage = (): string => {
    if (!lastSaved) return '';
    
    try {
      const savedDate = new Date(lastSaved);
      const now = new Date();
      const diffMs = now.getTime() - savedDate.getTime();
      const diffMins = Math.floor(diffMs / (1000 * 60));
      const diffHours = Math.floor(diffMs / (1000 * 60 * 60));
      const diffDays = Math.floor(diffMs / (1000 * 60 * 60 * 24));
      
      if (diffMins < 1) return 'Saved just now';
      if (diffMins < 60) return `Saved ${diffMins} minute${diffMins > 1 ? 's' : ''} ago`;
      if (diffHours < 24) return `Saved ${diffHours} hour${diffHours > 1 ? 's' : ''} ago`;
      if (diffDays < 7) return `Saved ${diffDays} day${diffDays > 1 ? 's' : ''} ago`;
      
      return `Saved on ${savedDate.toLocaleDateString()}`;
    } catch (error) {
      return 'Previously saved';
    }
  };

  return {
    // Data
    personalInfo,
    form16Data,
    additionalInfo,
    currentStep,
    lastSaved,
    
    // Setters
    savePersonalInfo,
    saveForm16Data,
    saveAdditionalInfo,
    saveCurrentStep,
    
    // Utilities
    getSavedData,
    hasSavedData,
    clearAllData,
    getLastSavedMessage
  };
}
