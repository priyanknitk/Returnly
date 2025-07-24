using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Returnly.Services;
using Returnly.Models;
using Returnly.ViewModels.Commands;

namespace Returnly.ViewModels
{
    public class TaxDataInputViewModel : INotifyPropertyChanged
    {
        private readonly TaxConfigurationService _taxConfigurationService;
        private readonly TaxCalculationService _taxCalculationService;
        private readonly NotificationService? _notificationService;
        private readonly TaxInputPageService? _taxInputPageService;
        private readonly IDialogService? _dialogService;

        // Personal Information fields
        private string _employeeName = string.Empty;
        private string _pan = string.Empty;
        private string _assessmentYear = string.Empty;
        private string _financialYear = string.Empty;
        private string _employerName = string.Empty;
        private string _tan = string.Empty;

        // Salary Information fields (inputs)
        private decimal _salarySection17 = 0;
        private decimal _perquisites = 0;
        private decimal _profitsInLieu = 0;
        
        // Salary breakdown fields (inputs)
        private decimal _basicSalary = 0;
        private decimal _hra = 0;
        private decimal _specialAllowance = 0;
        private decimal _otherAllowances = 0;

        // Deduction fields (inputs)
        private decimal _standardDeduction = 75000;
        private decimal _professionalTax = 0;

        // Tax information fields (inputs)
        private decimal _totalTaxDeducted = 0;
        private decimal _q1TDS = 0;
        private decimal _q2TDS = 0;
        private decimal _q3TDS = 0;
        private decimal _q4TDS = 0;

        // Calculated fields (no property change handlers)
        private decimal _grossSalary = 0;
        private decimal _calculatedSection17 = 0;
        private decimal _taxableIncome = 0;
        private string _standardDeductionLabel = "Standard Deduction (₹75,000)";

        // Collections for ComboBoxes
        public ObservableCollection<ComboBoxItemViewModel> AssessmentYears { get; }
        public ObservableCollection<ComboBoxItemViewModel> FinancialYears { get; }

        // Commands
        public ICommand? CalculateTaxCommand { get; }
        public ICommand? CompareTaxRegimesCommand { get; }
        public ICommand? SaveDataCommand { get; }

        // Current tax calculation result for navigation
        public TaxCalculationResult? CurrentTaxCalculation { get; private set; }

        public TaxDataInputViewModel(NotificationService? notificationService = null, 
                                   TaxInputPageService? taxInputPageService = null,
                                   IDialogService? dialogService = null,
                                   Form16Data? form16Data = null)
        {
            _taxConfigurationService = new TaxConfigurationService();
            _taxCalculationService = new TaxCalculationService();
            _notificationService = notificationService;
            _taxInputPageService = taxInputPageService;
            _dialogService = dialogService;

            AssessmentYears = new ObservableCollection<ComboBoxItemViewModel>();
            FinancialYears = new ObservableCollection<ComboBoxItemViewModel>();

            // Initialize commands only if we have the required services
            if (_taxInputPageService != null && _notificationService != null && form16Data != null)
            {
                CalculateTaxCommand = new CalculateTaxCommand(this, _taxInputPageService, _notificationService, OnCalculationComplete);
                CompareTaxRegimesCommand = new CompareTaxRegimesCommand(this, _taxInputPageService, _notificationService, OnComparisonComplete);
                SaveDataCommand = new SaveDataCommand(this, _taxInputPageService, _notificationService, form16Data);
            }

            InitializeYearCollections();
        }

        // Factory method for creating a fully configured ViewModel for the UI
        public static TaxDataInputViewModel CreateForUI(NotificationService notificationService, Form16Data form16Data)
        {
            var taxCalculationService = new TaxCalculationService();
            var taxInputPageService = new TaxInputPageService(taxCalculationService, notificationService);
            var dialogService = new DialogService(taxInputPageService);
            
            return new TaxDataInputViewModel(notificationService, taxInputPageService, dialogService, form16Data);
        }

        #region Personal Information Properties

        public string EmployeeName
        {
            get => _employeeName;
            set => SetProperty(ref _employeeName, value);
        }

        public string PAN
        {
            get => _pan;
            set => SetProperty(ref _pan, value?.ToUpper() ?? string.Empty);
        }

        public string AssessmentYear
        {
            get => _assessmentYear;
            set
            {
                if (SetProperty(ref _assessmentYear, value))
                {
                    UpdateFinancialYearFromAssessment(value);
                }
            }
        }

        public string FinancialYear
        {
            get => _financialYear;
            set
            {
                if (SetProperty(ref _financialYear, value))
                {
                    UpdateTaxConfiguration();
                }
            }
        }

        public string EmployerName
        {
            get => _employerName;
            set => SetProperty(ref _employerName, value);
        }

        public string TAN
        {
            get => _tan;
            set => SetProperty(ref _tan, value?.ToUpper() ?? string.Empty);
        }

        #endregion

        #region Salary Information Properties (Input Fields)

        public decimal SalarySection17
        {
            get => _salarySection17;
            set
            {
                if (SetProperty(ref _salarySection17, value))
                {
                    CalculateGrossSalary();
                    CalculateTaxableIncome();
                }
            }
        }

        public decimal Perquisites
        {
            get => _perquisites;
            set
            {
                if (SetProperty(ref _perquisites, value))
                {
                    CalculateGrossSalary();
                    CalculateTaxableIncome();
                }
            }
        }

        public decimal ProfitsInLieu
        {
            get => _profitsInLieu;
            set
            {
                if (SetProperty(ref _profitsInLieu, value))
                {
                    CalculateGrossSalary();
                    CalculateTaxableIncome();
                }
            }
        }

        public decimal BasicSalary
        {
            get => _basicSalary;
            set
            {
                if (SetProperty(ref _basicSalary, value))
                {
                    CalculateSection17Total();
                    CalculateGrossSalary();
                    CalculateTaxableIncome();
                }
            }
        }

        public decimal HRA
        {
            get => _hra;
            set
            {
                if (SetProperty(ref _hra, value))
                {
                    CalculateSection17Total();
                    CalculateGrossSalary();
                    CalculateTaxableIncome();
                }
            }
        }

        public decimal SpecialAllowance
        {
            get => _specialAllowance;
            set
            {
                if (SetProperty(ref _specialAllowance, value))
                {
                    CalculateSection17Total();
                    CalculateGrossSalary();
                    CalculateTaxableIncome();
                }
            }
        }

        public decimal OtherAllowances
        {
            get => _otherAllowances;
            set
            {
                if (SetProperty(ref _otherAllowances, value))
                {
                    CalculateSection17Total();
                    CalculateGrossSalary();
                    CalculateTaxableIncome();
                }
            }
        }

        #endregion

        #region Deduction Properties (Input Fields)

        public decimal StandardDeduction
        {
            get => _standardDeduction;
            set
            {
                if (SetProperty(ref _standardDeduction, value))
                {
                    CalculateTaxableIncome();
                }
            }
        }

        public decimal ProfessionalTax
        {
            get => _professionalTax;
            set
            {
                if (SetProperty(ref _professionalTax, value))
                {
                    CalculateTaxableIncome();
                }
            }
        }

        public string StandardDeductionLabel
        {
            get => _standardDeductionLabel;
            private set => SetProperty(ref _standardDeductionLabel, value);
        }

        #endregion

        #region Tax Information Properties (Input Fields)

        public decimal TotalTaxDeducted
        {
            get => _totalTaxDeducted;
            set => SetProperty(ref _totalTaxDeducted, value);
        }

        public decimal Q1TDS
        {
            get => _q1TDS;
            set
            {
                if (SetProperty(ref _q1TDS, value))
                {
                    CalculateTotalTDS();
                }
            }
        }

        public decimal Q2TDS
        {
            get => _q2TDS;
            set
            {
                if (SetProperty(ref _q2TDS, value))
                {
                    CalculateTotalTDS();
                }
            }
        }

        public decimal Q3TDS
        {
            get => _q3TDS;
            set
            {
                if (SetProperty(ref _q3TDS, value))
                {
                    CalculateTotalTDS();
                }
            }
        }

        public decimal Q4TDS
        {
            get => _q4TDS;
            set
            {
                if (SetProperty(ref _q4TDS, value))
                {
                    CalculateTotalTDS();
                }
            }
        }

        #endregion

        #region Calculated Properties (Read-Only, No PropertyChanged Handlers)

        public decimal GrossSalary
        {
            get => _grossSalary;
            private set => SetProperty(ref _grossSalary, value);
        }

        public decimal CalculatedSection17
        {
            get => _calculatedSection17;
            private set => SetProperty(ref _calculatedSection17, value);
        }

        public decimal TaxableIncome
        {
            get => _taxableIncome;
            private set => SetProperty(ref _taxableIncome, value);
        }

        #endregion

        #region Calculation Methods

        private void CalculateSection17Total()
        {
            try
            {
                var section17Total = _basicSalary + _hra + _specialAllowance + _otherAllowances;
                CalculatedSection17 = section17Total;
                
                // Auto-update the main Section 17 field
                _salarySection17 = section17Total;
                OnPropertyChanged(nameof(SalarySection17));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error calculating Section 17 total: {ex.Message}");
            }
        }

        private void CalculateGrossSalary()
        {
            try
            {
                var grossSalary = _salarySection17 + _perquisites + _profitsInLieu;
                GrossSalary = grossSalary;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error calculating gross salary: {ex.Message}");
            }
        }

        private void CalculateTotalTDS()
        {
            try
            {
                var totalTDS = _q1TDS + _q2TDS + _q3TDS + _q4TDS;
                _totalTaxDeducted = totalTDS;
                OnPropertyChanged(nameof(TotalTaxDeducted));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error calculating total TDS: {ex.Message}");
            }
        }

        private void CalculateTaxableIncome()
        {
            try
            {
                var taxableIncome = _grossSalary - _standardDeduction - _professionalTax;
                TaxableIncome = Math.Max(0, taxableIncome);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error calculating taxable income: {ex.Message}");
            }
        }

        #endregion

        #region Configuration and Initialization

        private void InitializeYearCollections()
        {
            var currentYear = DateTime.Now.Year;
            var currentMonth = DateTime.Now.Month;
            var currentAssessmentYear = currentMonth >= 4 ? currentYear + 1 : currentYear;
            
            AssessmentYears.Clear();
            FinancialYears.Clear();
            
            var minSupportedAssessmentYear = 2024; // AY 2024-25 (for FY 2023-24)
            
            // Populate Assessment Year collection
            for (int i = 1; i >= -5; i--)
            {
                var year = currentAssessmentYear + i;
                if (year >= minSupportedAssessmentYear)
                {
                    var assessmentYear = $"{year}-{(year + 1).ToString().Substring(2)}";
                    AssessmentYears.Add(new ComboBoxItemViewModel 
                    { 
                        Display = assessmentYear, 
                        Value = assessmentYear 
                    });
                }
            }
            
            // Populate Financial Year collection
            var minSupportedFinancialYear = 2023; // FY 2023-24
            for (int i = 0; i >= -5; i--)
            {
                var year = currentAssessmentYear + i - 1;
                if (year >= minSupportedFinancialYear)
                {
                    var financialYear = $"{year}-{(year + 1).ToString().Substring(2)}";
                    FinancialYears.Add(new ComboBoxItemViewModel 
                    { 
                        Display = financialYear, 
                        Value = financialYear 
                    });
                }
            }
        }

        private void UpdateFinancialYearFromAssessment(string assessmentYear)
        {
            if (string.IsNullOrEmpty(assessmentYear) || !assessmentYear.Contains("-"))
                return;

            var years = assessmentYear.Split('-');
            if (years.Length != 2 || !int.TryParse(years[0], out int startYear))
                return;

            var financialStartYear = startYear - 1;
            var financialEndYear = startYear;
            
            var financialYear = $"{financialStartYear}-{financialEndYear.ToString().Substring(2)}";
            FinancialYear = financialYear;
        }

        private void UpdateTaxConfiguration()
        {
            if (string.IsNullOrEmpty(_financialYear))
                return;

            try
            {
                var taxConfig = _taxConfigurationService.GetTaxConfiguration(_financialYear);
                
                // Update the standard deduction limit but keep user input if it's within bounds
                var maxStandardDeduction = taxConfig.StandardDeduction;
                if (_standardDeduction > maxStandardDeduction)
                {
                    StandardDeduction = maxStandardDeduction;
                }
                
                StandardDeductionLabel = $"Standard Deduction (₹{taxConfig.StandardDeduction:N0})";
                
                _notificationService?.ShowNotification(
                    $"Tax configuration updated for FY {_financialYear}. Standard Deduction: ₹{taxConfig.StandardDeduction:N0}",
                    NotificationType.Info
                );
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating tax configuration: {ex.Message}");
                _notificationService?.ShowNotification($"Error updating tax configuration: {ex.Message}", NotificationType.Warning);
            }
        }

        #endregion

        #region Data Loading and Validation

        public void LoadFromForm16Data(Form16Data form16Data)
        {
            if (form16Data == null) return;

            try
            {
                // Personal Information
                EmployeeName = form16Data.EmployeeName ?? string.Empty;
                PAN = form16Data.PAN ?? string.Empty;
                AssessmentYear = form16Data.AssessmentYear ?? string.Empty;
                EmployerName = form16Data.EmployerName ?? string.Empty;
                TAN = form16Data.TAN ?? string.Empty;

                // Salary Information
                SalarySection17 = form16Data.Form16B?.SalarySection17 ?? 0;
                Perquisites = form16Data.Form16B?.Perquisites ?? 0;
                ProfitsInLieu = form16Data.Form16B?.ProfitsInLieu ?? 0;
                
                // Salary Breakdown
                BasicSalary = form16Data.Form16B?.BasicSalary ?? 0;
                HRA = form16Data.Form16B?.HRA ?? 0;
                SpecialAllowance = form16Data.Form16B?.SpecialAllowance ?? 0;
                OtherAllowances = form16Data.Form16B?.OtherAllowances ?? 0;
                
                // Deductions
                StandardDeduction = form16Data.Form16B?.StandardDeduction ?? 75000;
                ProfessionalTax = form16Data.Form16B?.ProfessionalTax ?? 0;

                // Tax Information
                TotalTaxDeducted = form16Data.TotalTaxDeducted;
                Q1TDS = form16Data.Annexure?.Q1TDS ?? 0;
                Q2TDS = form16Data.Annexure?.Q2TDS ?? 0;
                Q3TDS = form16Data.Annexure?.Q3TDS ?? 0;
                Q4TDS = form16Data.Annexure?.Q4TDS ?? 0;

                // Trigger all calculations
                CalculateSection17Total();
                CalculateGrossSalary();
                CalculateTaxableIncome();
                CalculateTotalTDS();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading Form16 data: {ex.Message}");
                _notificationService?.ShowNotification($"Error loading data: {ex.Message}", NotificationType.Error);
            }
        }

        public bool ValidateData(out string errorMessage)
        {
            var errors = new List<string>();
            errorMessage = string.Empty;

            // Validate required fields
            if (string.IsNullOrWhiteSpace(_employeeName))
                errors.Add("Employee Name is required");

            if (string.IsNullOrWhiteSpace(_pan) || _pan.Length != 10)
                errors.Add("Valid PAN Number is required (10 characters)");

            if (string.IsNullOrWhiteSpace(_assessmentYear))
                errors.Add("Assessment Year is required");

            if (string.IsNullOrWhiteSpace(_financialYear))
                errors.Add("Financial Year is required");

            // Validate PAN format
            if (!string.IsNullOrWhiteSpace(_pan))
            {
                var panPattern = @"^[A-Z]{5}[0-9]{4}[A-Z]{1}$";
                if (!System.Text.RegularExpressions.Regex.IsMatch(_pan, panPattern))
                {
                    errors.Add("PAN format is invalid (e.g., ABCDE1234F)");
                }
            }

            // Validate TAN format if provided
            if (!string.IsNullOrWhiteSpace(_tan))
            {
                var tanPattern = @"^[A-Z]{4}[0-9]{5}[A-Z]{1}$";
                if (!System.Text.RegularExpressions.Regex.IsMatch(_tan, tanPattern))
                {
                    errors.Add("TAN format is invalid (e.g., ABCD12345E)");
                }
            }

            if (errors.Count > 0)
            {
                errorMessage = string.Join("\n• ", errors.Prepend("Validation errors:"));
                return false;
            }

            return true;
        }

        #endregion

        #region Command Callbacks

        private async void OnCalculationComplete(TaxCalculationResult taxCalculation, TaxRefundCalculation refundCalculation)
        {
            CurrentTaxCalculation = taxCalculation;
            
            // Show results dialog
            if (_dialogService != null)
            {
                await _dialogService.ShowTaxCalculationResultsAsync(taxCalculation, refundCalculation);
            }
        }

        private async void OnComparisonComplete(RegimeComparisonResult comparison)
        {
            // Show comparison dialog
            if (_dialogService != null)
            {
                await _dialogService.ShowRegimeComparisonAsync(comparison);
            }
        }

        #endregion

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    // Helper class for ComboBox items
    public class ComboBoxItemViewModel
    {
        public string Display { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}
