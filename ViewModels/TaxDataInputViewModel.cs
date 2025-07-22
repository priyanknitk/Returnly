using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Returnly.Services;

namespace Returnly.ViewModels
{
    public class TaxDataInputViewModel : INotifyPropertyChanged
    {
        private readonly TaxConfigurationService _taxConfigurationService;
        private string _financialYear = string.Empty;
        private decimal _standardDeduction = 75000;
        private string _standardDeductionLabel = "Standard Deduction (₹75,000)";

        public TaxDataInputViewModel()
        {
            _taxConfigurationService = new TaxConfigurationService();
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

        public decimal StandardDeduction
        {
            get => _standardDeduction;
            set => SetProperty(ref _standardDeduction, value);
        }

        public string StandardDeductionLabel
        {
            get => _standardDeductionLabel;
            set => SetProperty(ref _standardDeductionLabel, value);
        }

        private void UpdateTaxConfiguration()
        {
            if (string.IsNullOrEmpty(_financialYear))
                return;

            try
            {
                var taxConfig = _taxConfigurationService.GetTaxConfiguration(_financialYear);
                StandardDeduction = taxConfig.StandardDeduction;
                StandardDeductionLabel = $"Standard Deduction (₹{taxConfig.StandardDeduction:N0})";
            }
            catch (Exception ex)
            {
                // Handle error silently or log it
                System.Diagnostics.Debug.WriteLine($"Error updating tax configuration: {ex.Message}");
            }
        }

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
    }
}
