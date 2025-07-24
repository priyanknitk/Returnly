using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Returnly.Services;
using Returnly.Models;
using Returnly.ViewModels;

namespace Returnly.ViewModels.Commands
{
    /// <summary>
    /// Command for calculating taxes
    /// </summary>
    public class CalculateTaxCommand : ICommand
    {
        private readonly TaxDataInputViewModel _viewModel;
        private readonly TaxInputPageService _taxInputPageService;
        private readonly NotificationService _notificationService;
        private readonly Action<TaxCalculationResult, TaxRefundCalculation> _onCalculationComplete;
        private bool _isExecuting;

        public CalculateTaxCommand(
            TaxDataInputViewModel viewModel, 
            TaxInputPageService taxInputPageService,
            NotificationService notificationService,
            Action<TaxCalculationResult, TaxRefundCalculation> onCalculationComplete)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            _taxInputPageService = taxInputPageService ?? throw new ArgumentNullException(nameof(taxInputPageService));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _onCalculationComplete = onCalculationComplete ?? throw new ArgumentNullException(nameof(onCalculationComplete));
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return !_isExecuting && _viewModel.ValidateData(out _);
        }

        public async void Execute(object? parameter)
        {
            if (!CanExecute(parameter))
                return;

            _isExecuting = true;
            OnCanExecuteChanged();

            try
            {
                var (taxCalculation, refundCalculation) = await _taxInputPageService.CalculateTaxAsync(
                    _viewModel.TaxableIncome,
                    _viewModel.FinancialYear,
                    _viewModel.TotalTaxDeducted,
                    30 // Default age, can be made configurable
                );

                _onCalculationComplete(taxCalculation, refundCalculation);

                _notificationService.ShowNotification(
                    $"Tax calculation completed! Total tax liability: ₹{taxCalculation.TotalTaxWithCess:N2}", 
                    NotificationType.Success
                );
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification($"Error calculating taxes: {ex.Message}", NotificationType.Error);
            }
            finally
            {
                _isExecuting = false;
                OnCanExecuteChanged();
            }
        }

        protected virtual void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Command for comparing tax regimes
    /// </summary>
    public class CompareTaxRegimesCommand : ICommand
    {
        private readonly TaxDataInputViewModel _viewModel;
        private readonly TaxInputPageService _taxInputPageService;
        private readonly NotificationService _notificationService;
        private readonly Action<RegimeComparisonResult> _onComparisonComplete;
        private bool _isExecuting;

        public CompareTaxRegimesCommand(
            TaxDataInputViewModel viewModel,
            TaxInputPageService taxInputPageService,
            NotificationService notificationService,
            Action<RegimeComparisonResult> onComparisonComplete)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            _taxInputPageService = taxInputPageService ?? throw new ArgumentNullException(nameof(taxInputPageService));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _onComparisonComplete = onComparisonComplete ?? throw new ArgumentNullException(nameof(onComparisonComplete));
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return !_isExecuting && _viewModel.ValidateData(out _);
        }

        public async void Execute(object? parameter)
        {
            if (!CanExecute(parameter))
                return;

            _isExecuting = true;
            OnCanExecuteChanged();

            try
            {
                var comparison = await _taxInputPageService.CompareTaxRegimesAsync(
                    _viewModel.TaxableIncome,
                    _viewModel.FinancialYear,
                    30 // Default age, can be made configurable
                );

                _onComparisonComplete(comparison);
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification($"Error comparing regimes: {ex.Message}", NotificationType.Error);
            }
            finally
            {
                _isExecuting = false;
                OnCanExecuteChanged();
            }
        }

        protected virtual void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Command for saving data
    /// </summary>
    public class SaveDataCommand : ICommand
    {
        private readonly TaxDataInputViewModel _viewModel;
        private readonly TaxInputPageService _taxInputPageService;
        private readonly NotificationService _notificationService;
        private readonly Form16Data _form16Data;
        private bool _isExecuting;

        public SaveDataCommand(
            TaxDataInputViewModel viewModel,
            TaxInputPageService taxInputPageService,
            NotificationService notificationService,
            Form16Data form16Data)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            _taxInputPageService = taxInputPageService ?? throw new ArgumentNullException(nameof(taxInputPageService));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _form16Data = form16Data ?? throw new ArgumentNullException(nameof(form16Data));
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return !_isExecuting && _viewModel.ValidateData(out _);
        }

        public void Execute(object? parameter)
        {
            if (!CanExecute(parameter))
                return;

            _isExecuting = true;
            OnCanExecuteChanged();

            try
            {
                _taxInputPageService.SaveDataToModel(_viewModel, _form16Data);
                _notificationService.ShowNotification("Tax data saved successfully!", NotificationType.Success);
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification($"Error saving data: {ex.Message}", NotificationType.Error);
            }
            finally
            {
                _isExecuting = false;
                OnCanExecuteChanged();
            }
        }

        protected virtual void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
