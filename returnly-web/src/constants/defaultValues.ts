import { AdditionalTaxpayerInfoDto } from '../types/api';

/**
 * Default sample personal information used throughout the application
 * for better user experience and testing purposes
 */
export const DEFAULT_PERSONAL_INFO: AdditionalTaxpayerInfoDto = {
    dateOfBirth: '1990-01-15',
    address: 'Flat 4B, Mock Residency, Sector 99, Test Nagar',
    city: 'Samplepur',
    state: 'Testland',
    pincode: '999999',
    emailAddress: 'sample.user@example.test',
    mobileNumber: '9123456789',
    aadhaarNumber: '999988887777',
    bankAccountNumber: '0000111122223333',
    bankIFSCCode: 'TEST0001234',
    bankName: 'Test Bank Ltd',
    hasHouseProperty: false,
    houseProperties: [],
    hasCapitalGains: false,
    capitalGains: [],
    hasForeignIncome: false,
    foreignIncome: 0,
    hasForeignAssets: false,
    foreignAssets: [],
    hasBusinessIncome: false,
    businessIncomes: [],
    businessExpenses: []
};

/**
 * Default sample salary breakdown data for demonstration
 */
export const DEFAULT_SALARY_BREAKDOWN = {
    basicPay: 0,
    ltaAllowance: 0,
    houseRentAllowance: 0,
    specialAllowance: 0,
    performanceBonus: 0,
    bonus: 0,
    otherAllowances: 0
};
