// API configuration
const API_BASE_URL = process.env.REACT_APP_API_BASE_URL || 'http://localhost:5201';
const API_URL = process.env.REACT_APP_API_URL || `${API_BASE_URL}/api`;

export const API_ENDPOINTS = {
  // Form16 endpoints
  FORM16_UPLOAD: `${API_URL}/form16/upload`,
  FORM16_SAMPLE: `${API_URL}/form16/sample`,
  
  // Tax calculation endpoints
  TAX_CALCULATE: `${API_URL}/taxcalculation/calculate`,
  TAX_COMPARE_REGIMES: `${API_URL}/taxcalculation/compare-regimes`,
  TAX_FINANCIAL_YEARS: `${API_URL}/taxcalculation/financial-years`,
  TAX_ASSESSMENT_YEARS: `${API_URL}/taxcalculation/assessment-years`,
  
  // ITR endpoints
  ITR_SAMPLE: `${API_URL}/itr/sample`,
  ITR_RECOMMEND: `${API_URL}/itr/recommend`,
  ITR_GENERATE: `${API_URL}/itr/generate`,
  ITR_DOWNLOAD_XML: `${API_URL}/itr/download/xml`,
  ITR_DOWNLOAD_JSON: `${API_URL}/itr/download/json`,
};

export const API_CONFIG = {
  baseURL: API_BASE_URL,
  apiURL: API_URL,
  timeout: 30000, // 30 seconds
  headers: {
    'Content-Type': 'application/json',
  },
};

export default API_CONFIG;
