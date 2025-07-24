using System.Windows;
using System.Windows.Controls;
using Returnly.Services;
using Returnly.Models;
using Returnly.ViewModels;
using System;

namespace Returnly
{
    public partial class TaxDataInputPage : Page
    {
        private readonly NotificationService _notificationService;
        private readonly TaxInputPageService _taxInputPageService;
        private readonly IDialogService _dialogService;
        private readonly TaxDataInputViewModel _viewModel;
        private readonly Form16Data _form16Data;
        private TaxCalculationResult? _currentTaxCalculation;

        public TaxDataInputPage() : this(new Form16Data())
        {
        }

        public TaxDataInputPage(Form16Data form16Data)
        {
            InitializeComponent();
            
            _form16Data = form16Data ?? new Form16Data();
            _notificationService = new NotificationService(NotificationPanel, NotificationTextBlock);
            
            // Create services
            var taxCalculationService = new TaxCalculationService();
            _taxInputPageService = new TaxInputPageService(taxCalculationService, _notificationService);
            _dialogService = new DialogService(_taxInputPageService);
            
            // Create ViewModel using factory method
            _viewModel = TaxDataInputViewModel.CreateForUI(_notificationService, _form16Data);
            
            // Set the DataContext for binding
            this.DataContext = _viewModel;
            
            // Load data from Form16Data into ViewModel
            _viewModel.LoadFromForm16Data(_form16Data);
        }

        private void SaveDataToModel()
        {
            try
            {
                _taxInputPageService.SaveDataToModel(_viewModel, _form16Data);
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification($"Error saving data: {ex.Message}", NotificationType.Error);
            }
        }

        private bool ValidateData()
        {
            return _viewModel.ValidateData(out string errorMessage);
        }

        private async void CalculateTaxes_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateData())
                    return;

                SaveDataToModel();

                var (taxCalculation, refundCalculation) = await _taxInputPageService.CalculateTaxAsync(
                    _viewModel.TaxableIncome,
                    _viewModel.FinancialYear,
                    _viewModel.TotalTaxDeducted,
                    30 // Default age, can be made configurable
                );

                _currentTaxCalculation = taxCalculation;

                // Show results
                await _dialogService.ShowTaxCalculationResultsAsync(taxCalculation, refundCalculation);

                _notificationService.ShowNotification(
                    $"Tax calculation completed! Total tax liability: ₹{taxCalculation.TotalTaxWithCess:N2}", 
                    NotificationType.Success
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}");
                _notificationService.ShowNotification($"Error calculating taxes: {ex.Message}", NotificationType.Error);
            }
        }

        private async void CompareRegimes_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateData())
                    return;

                var comparison = await _taxInputPageService.CompareTaxRegimesAsync(
                    _viewModel.TaxableIncome,
                    _viewModel.FinancialYear,
                    30 // Default age, can be made configurable
                );

                await _dialogService.ShowRegimeComparisonAsync(comparison);
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification($"Error comparing regimes: {ex.Message}", NotificationType.Error);
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

        private void SaveData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateData())
                {
                    return;
                }

                // Save data to model
                SaveDataToModel();
                
                _notificationService.ShowNotification("Tax data saved successfully!", NotificationType.Success);
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification($"Error saving data: {ex.Message}", NotificationType.Error);
            }
        }

        private async void ContinueToReturns_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateData())
                {
                    return;
                }

                // Save current data
                SaveDataToModel();

                // Check if tax calculation has been done
                if (_currentTaxCalculation == null)
                {
                    _notificationService.ShowNotification("Please calculate taxes first before proceeding.", NotificationType.Warning);
                    return;
                }

                // Calculate refund/additional tax using the service
                var (_, refundCalculation) = await _taxInputPageService.CalculateTaxAsync(
                    _viewModel.TaxableIncome,
                    _viewModel.FinancialYear,
                    _viewModel.TotalTaxDeducted,
                    30 // Default age, can be made configurable
                );

                // Navigate to results page
                NavigationService?.Navigate(new TaxCalculationResultsPage(_currentTaxCalculation, refundCalculation, _form16Data));
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification($"Error proceeding to returns: {ex.Message}", NotificationType.Error);
            }
        }
    }
}