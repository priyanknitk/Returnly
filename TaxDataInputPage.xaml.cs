using System.Windows;
using System.Windows.Controls;
using Returnly.Services;
using System.Collections.ObjectModel;
using System;

namespace Returnly
{
    public partial class TaxDataInputPage : Page
    {
        private readonly NotificationService _notificationService;
        private Form16Data _form16Data;

        public TaxDataInputPage() : this(new Form16Data())
        {
        }

        public TaxDataInputPage(Form16Data form16Data)
        {
            InitializeComponent();
            
            _form16Data = form16Data ?? new Form16Data();
            _notificationService = new NotificationService(NotificationPanel, NotificationTextBlock);
            
            // Populate ComboBoxes dynamically
            InitializeYearCollections();
            
            // Subscribe to value change events for automatic calculations
            SubscribeToValueChanges();
            
            // Load data into controls
            LoadForm16Data();
        }

        private void InitializeYearCollections()
        {
            var currentYear = DateTime.Now.Year;
            var currentMonth = DateTime.Now.Month;
            var currentAssessmentYear = currentMonth >= 4 ? currentYear + 1 : currentYear;
            
            // Clear existing items
            AssessmentYearComboBox.Items.Clear();
            FinancialYearComboBox.Items.Clear();
            
            // Populate Assessment Year ComboBox
            for (int i = 1; i >= -7; i--)
            {
                var year = currentAssessmentYear + i;
                var assessmentYear = $"{year}-{(year + 1).ToString().Substring(2)}";
                var item = new ComboBoxItem
                {
                    Content = assessmentYear,
                    Tag = assessmentYear
                };
                AssessmentYearComboBox.Items.Add(item);
            }
            
            // Populate Financial Year ComboBox
            for (int i = 0; i >= -8; i--)
            {
                var year = currentAssessmentYear + i - 1;
                var financialYear = $"{year}-{(year + 1).ToString().Substring(2)}";
                var item = new ComboBoxItem
                {
                    Content = financialYear,
                    Tag = financialYear
                };
                FinancialYearComboBox.Items.Add(item);
            }
        }

        private void SubscribeToValueChanges()
        {
            // Subscribe to salary component changes to auto-calculate section 17(1)
            BasicSalaryNumberBox.ValueChanged += SalaryBreakdownComponent_ValueChanged;
            HRANumberBox.ValueChanged += SalaryBreakdownComponent_ValueChanged;
            SpecialAllowanceNumberBox.ValueChanged += SalaryBreakdownComponent_ValueChanged;
            OtherAllowancesNumberBox.ValueChanged += SalaryBreakdownComponent_ValueChanged;
            
            // Subscribe to main salary sections to auto-calculate gross salary
            SalarySection17NumberBox.ValueChanged += SalaryMainComponent_ValueChanged;
            PerquisitesNumberBox.ValueChanged += SalaryMainComponent_ValueChanged;
            ProfitsInLieuNumberBox.ValueChanged += SalaryMainComponent_ValueChanged;
            
            // Subscribe to deduction changes for taxable income calculation
            StandardDeductionNumberBox.ValueChanged += DeductionComponent_ValueChanged;
            ProfessionalTaxNumberBox.ValueChanged += DeductionComponent_ValueChanged;
            
            // Subscribe to TDS quarter changes to auto-calculate total
            Q1TDSNumberBox.ValueChanged += TDSComponent_ValueChanged;
            Q2TDSNumberBox.ValueChanged += TDSComponent_ValueChanged;
            Q3TDSNumberBox.ValueChanged += TDSComponent_ValueChanged;
            Q4TDSNumberBox.ValueChanged += TDSComponent_ValueChanged;
        }

        private void AssessmentYearComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AssessmentYearComboBox.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag != null)
            {
                var assessmentYear = selectedItem.Tag.ToString();
                var financialYear = GetFinancialYearFromAssessmentYear(assessmentYear);
                
                // Set the financial year automatically
                SetFinancialYearSelection(financialYear);
            }
        }

        private string GetFinancialYearFromAssessmentYear(string assessmentYear)
        {
            // Assessment Year format: "2024-25"
            // Financial Year should be: "2023-24" (one year before)
            
            if (string.IsNullOrEmpty(assessmentYear) || !assessmentYear.Contains("-"))
                return string.Empty;

            var years = assessmentYear.Split('-');
            if (years.Length != 2 || !int.TryParse(years[0], out int startYear))
                return string.Empty;

            var financialStartYear = startYear - 1;
            var financialEndYear = startYear;
            
            return $"{financialStartYear}-{financialEndYear.ToString().Substring(2)}";
        }

        private void SetFinancialYearSelection(string financialYear)
        {
            foreach (ComboBoxItem item in FinancialYearComboBox.Items)
            {
                if (item.Tag?.ToString() == financialYear)
                {
                    FinancialYearComboBox.SelectedItem = item;
                    break;
                }
            }
        }

        private void SetAssessmentYearSelection(string assessmentYear)
        {
            foreach (ComboBoxItem item in AssessmentYearComboBox.Items)
            {
                if (item.Tag?.ToString() == assessmentYear)
                {
                    AssessmentYearComboBox.SelectedItem = item;
                    break;
                }
            }
        }

        private void LoadForm16Data()
        {
            try
            {
                // Personal Information
                EmployeeNameTextBox.Text = _form16Data.EmployeeName;
                PANTextBox.Text = _form16Data.PAN;
                
                // Set Assessment Year dropdown
                SetAssessmentYearSelection(_form16Data.AssessmentYear);
                
                // Financial Year will be auto-set by the AssessmentYear selection
                if (!string.IsNullOrEmpty(_form16Data.FinancialYear))
                {
                    SetFinancialYearSelection(_form16Data.FinancialYear);
                }
                
                EmployerNameTextBox.Text = _form16Data.EmployerName;
                TANTextBox.Text = _form16Data.TAN;

                // Official Form 16 Salary Structure
                SalarySection17NumberBox.Value = (double)_form16Data.Form16B.SalarySection17;
                PerquisitesNumberBox.Value = (double)_form16Data.Form16B.Perquisites;
                ProfitsInLieuNumberBox.Value = (double)_form16Data.Form16B.ProfitsInLieu;
                
                // Breakdown of Section 17(1)
                BasicSalaryNumberBox.Value = (double)_form16Data.Form16B.BasicSalary;
                HRANumberBox.Value = (double)_form16Data.Form16B.HRA;
                SpecialAllowanceNumberBox.Value = (double)_form16Data.Form16B.SpecialAllowance;
                OtherAllowancesNumberBox.Value = (double)_form16Data.Form16B.OtherAllowances;
                
                // Auto-calculated fields
                GrossSalaryNumberBox.Value = (double)_form16Data.Form16B.GrossSalary;

                // New Tax Regime Deductions Only
                StandardDeductionNumberBox.Value = (double)_form16Data.Form16B.StandardDeduction;
                ProfessionalTaxNumberBox.Value = (double)_form16Data.Form16B.ProfessionalTax;

                // Tax Information
                TotalTaxDeductedNumberBox.Value = (double)_form16Data.TotalTaxDeducted;
                TaxableIncomeNumberBox.Value = (double)_form16Data.Form16B.TaxableIncome;

                // Quarterly TDS
                Q1TDSNumberBox.Value = (double)_form16Data.Annexure.Q1TDS;
                Q2TDSNumberBox.Value = (double)_form16Data.Annexure.Q2TDS;
                Q3TDSNumberBox.Value = (double)_form16Data.Annexure.Q3TDS;
                Q4TDSNumberBox.Value = (double)_form16Data.Annexure.Q4TDS;

                // Auto-calculate values
                CalculateSection17Total();
                CalculateGrossSalary();
                CalculateTaxableIncome();
                
                _notificationService.ShowNotification("Form 16 data loaded successfully!", NotificationType.Success);
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification($"Error loading data: {ex.Message}", NotificationType.Error);
            }
        }

        private void SalaryComponent_ValueChanged(object sender, RoutedEventArgs e)
        {
            CalculateGrossSalary();
            CalculateTaxableIncome();
        }

        private void TDSComponent_ValueChanged(object sender, RoutedEventArgs e)
        {
            CalculateTotalTDS();
        }

        private void SalaryBreakdownComponent_ValueChanged(object sender, RoutedEventArgs e)
        {
            CalculateSection17Total();
            CalculateGrossSalary();
            CalculateTaxableIncome();
        }

        private void SalaryMainComponent_ValueChanged(object sender, RoutedEventArgs e)
        {
            CalculateGrossSalary();
            CalculateTaxableIncome();
        }

        private void DeductionComponent_ValueChanged(object sender, RoutedEventArgs e)
        {
            CalculateTaxableIncome();
        }

        private void CalculateSection17Total()
        {
            try
            {
                var basicSalary = BasicSalaryNumberBox.Value ?? 0;
                var hra = HRANumberBox.Value ?? 0;
                var specialAllowance = SpecialAllowanceNumberBox.Value ?? 0;
                var otherAllowances = OtherAllowancesNumberBox.Value ?? 0;
                
                var section17Total = basicSalary + hra + specialAllowance + otherAllowances;
                
                // Update both the calculated field and the main 1(a) field
                CalculatedSection17NumberBox.Value = section17Total;
                SalarySection17NumberBox.Value = section17Total;
            }
            catch (Exception)
            {
                // Handle any calculation errors silently
            }
        }

        private void CalculateGrossSalary()
        {
            try
            {
                var salarySection17 = SalarySection17NumberBox.Value ?? 0;
                var perquisites = PerquisitesNumberBox.Value ?? 0;
                var profitsInLieu = ProfitsInLieuNumberBox.Value ?? 0;
                
                var grossSalary = salarySection17 + perquisites + profitsInLieu;
                GrossSalaryNumberBox.Value = grossSalary;
            }
            catch (Exception)
            {
                // Handle any calculation errors silently
            }
        }

        private void CalculateTotalTDS()
        {
            try
            {
                var q1 = Q1TDSNumberBox.Value ?? 0;
                var q2 = Q2TDSNumberBox.Value ?? 0;
                var q3 = Q3TDSNumberBox.Value ?? 0;
                var q4 = Q4TDSNumberBox.Value ?? 0;
                
                var totalTDS = q1 + q2 + q3 + q4;
                TotalTaxDeductedNumberBox.Value = totalTDS;
            }
            catch (Exception)
            {
                // Handle any calculation errors silently
            }
        }

        private void CalculateTaxableIncome()
        {
            try
            {
                var grossSalary = GrossSalaryNumberBox.Value ?? 0;
                var standardDeduction = StandardDeductionNumberBox.Value ?? 0;
                var professionalTax = ProfessionalTaxNumberBox.Value ?? 0;

                // New Tax Regime: Only Standard Deduction and Professional Tax are allowed
                var taxableIncome = grossSalary - standardDeduction - professionalTax;
                
                TaxableIncomeNumberBox.Value = Math.Max(0, taxableIncome); // Ensure non-negative
            }
            catch (Exception)
            {
                // Handle any calculation errors silently
            }
        }

        private void SaveDataToModel()
        {
            try
            {
                // Personal Information
                _form16Data.EmployeeName = EmployeeNameTextBox.Text ?? string.Empty;
                _form16Data.PAN = PANTextBox.Text ?? string.Empty;
                
                // Get selected values from ComboBoxes
                _form16Data.AssessmentYear = (AssessmentYearComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? string.Empty;
                _form16Data.FinancialYear = (FinancialYearComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? string.Empty;
                
                _form16Data.EmployerName = EmployerNameTextBox.Text ?? string.Empty;
                _form16Data.TAN = TANTextBox.Text ?? string.Empty;

                // Update Form16A data
                _form16Data.Form16A.EmployeeName = _form16Data.EmployeeName;
                _form16Data.Form16A.PAN = _form16Data.PAN;
                _form16Data.Form16A.AssessmentYear = _form16Data.AssessmentYear;
                _form16Data.Form16A.FinancialYear = _form16Data.FinancialYear;
                _form16Data.Form16A.EmployerName = _form16Data.EmployerName;
                _form16Data.Form16A.TAN = _form16Data.TAN;
                _form16Data.Form16A.TotalTaxDeducted = (decimal)(TotalTaxDeductedNumberBox.Value ?? 0);

                // Update Form16B data - Official Structure
                _form16Data.Form16B.SalarySection17 = (decimal)(SalarySection17NumberBox.Value ?? 0);
                _form16Data.Form16B.Perquisites = (decimal)(PerquisitesNumberBox.Value ?? 0);
                _form16Data.Form16B.ProfitsInLieu = (decimal)(ProfitsInLieuNumberBox.Value ?? 0);
                
                // Breakdown of Section 17(1)
                _form16Data.Form16B.BasicSalary = (decimal)(BasicSalaryNumberBox.Value ?? 0);
                _form16Data.Form16B.HRA = (decimal)(HRANumberBox.Value ?? 0);
                _form16Data.Form16B.SpecialAllowance = (decimal)(SpecialAllowanceNumberBox.Value ?? 0);
                _form16Data.Form16B.OtherAllowances = (decimal)(OtherAllowancesNumberBox.Value ?? 0);
                
                // New Tax Regime Deductions Only
                _form16Data.Form16B.StandardDeduction = (decimal)(StandardDeductionNumberBox.Value ?? 0);
                _form16Data.Form16B.ProfessionalTax = (decimal)(ProfessionalTaxNumberBox.Value ?? 0);
                _form16Data.Form16B.TaxableIncome = (decimal)(TaxableIncomeNumberBox.Value ?? 0);

                // Update Annexure data (only TDS, no Section 80 deductions)
                _form16Data.Annexure.Q1TDS = (decimal)(Q1TDSNumberBox.Value ?? 0);
                _form16Data.Annexure.Q2TDS = (decimal)(Q2TDSNumberBox.Value ?? 0);
                _form16Data.Annexure.Q3TDS = (decimal)(Q3TDSNumberBox.Value ?? 0);
                _form16Data.Annexure.Q4TDS = (decimal)(Q4TDSNumberBox.Value ?? 0);

                // Update backward compatibility fields
                _form16Data.GrossSalary = _form16Data.Form16B.GrossSalary;
                _form16Data.TotalTaxDeducted = _form16Data.Form16A.TotalTaxDeducted;
                _form16Data.StandardDeduction = _form16Data.Form16B.StandardDeduction;
                _form16Data.ProfessionalTax = _form16Data.Form16B.ProfessionalTax;
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification($"Error saving data: {ex.Message}", NotificationType.Error);
            }
        }

        private bool ValidateData()
        {
            var errors = new List<string>();

            // Validate required fields
            if (string.IsNullOrWhiteSpace(EmployeeNameTextBox.Text))
                errors.Add("Employee Name is required");

            if (string.IsNullOrWhiteSpace(PANTextBox.Text) || PANTextBox.Text.Length != 10)
                errors.Add("Valid PAN Number is required (10 characters)");

            if (AssessmentYearComboBox.SelectedItem == null)
                errors.Add("Assessment Year is required");

            if (FinancialYearComboBox.SelectedItem == null)
                errors.Add("Financial Year is required");

            // Validate PAN format
            if (!string.IsNullOrWhiteSpace(PANTextBox.Text))
            {
                var panPattern = @"^[A-Z]{5}[0-9]{4}[A-Z]{1}$";
                if (!System.Text.RegularExpressions.Regex.IsMatch(PANTextBox.Text.ToUpper(), panPattern))
                {
                    errors.Add("PAN format is invalid (e.g., ABCDE1234F)");
                }
            }

            // Validate TAN format if provided
            if (!string.IsNullOrWhiteSpace(TANTextBox.Text))
            {
                var tanPattern = @"^[A-Z]{4}[0-9]{5}[A-Z]{1}$";
                if (!System.Text.RegularExpressions.Regex.IsMatch(TANTextBox.Text.ToUpper(), tanPattern))
                {
                    errors.Add("TAN format is invalid (e.g., ABCD12345E)");
                }
            }

            if (errors.Any())
            {
                _notificationService.ShowNotification($"Validation errors:\n• {string.Join("\n• ", errors)}", NotificationType.Error);
                return false;
            }

            return true;
        }

        private void CalculateTaxes_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CalculateGrossSalary();
                CalculateTaxableIncome();
                CalculateTotalTDS();
                
                _notificationService.ShowNotification("Tax calculations updated successfully!", NotificationType.Success);
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification($"Error calculating taxes: {ex.Message}", NotificationType.Error);
            }
        }

        private void SaveData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateData())
                    return;

                SaveDataToModel();
                _notificationService.ShowNotification("Data saved successfully!", NotificationType.Success);
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification($"Error saving data: {ex.Message}", NotificationType.Error);
            }
        }

        private void ContinueToReturns_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateData())
                    return;

                SaveDataToModel();
                
                // TODO: Navigate to tax returns generation page
                _notificationService.ShowNotification("Proceeding to tax returns generation...", NotificationType.Info);
                
                // For now, show a message that this feature is coming soon
                MessageBox.Show("Tax returns generation feature is coming soon!", "Feature Coming Soon", 
                               MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification($"Error proceeding to returns: {ex.Message}", NotificationType.Error);
            }
        }

        private void BackToUpload_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (NavigationService != null)
                {
                    NavigationService.Navigate(new Form16UploadPage());
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification($"Error navigating back: {ex.Message}", NotificationType.Error);
            }
        }
    }
}