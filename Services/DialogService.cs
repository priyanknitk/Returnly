using System;
using System.Threading.Tasks;
using Returnly.Models;

namespace Returnly.Services
{
    /// <summary>
    /// Service interface for handling UI dialog operations
    /// </summary>
    public interface IDialogService
    {
        Task ShowTaxCalculationResultsAsync(TaxCalculationResult taxCalculation, TaxRefundCalculation refundCalculation);
        Task ShowRegimeComparisonAsync(RegimeComparisonResult comparison);
        Task<bool> ShowConfirmationAsync(string title, string message);
        Task ShowErrorAsync(string title, string message);
        Task ShowInfoAsync(string title, string message);
    }

    /// <summary>
    /// Implementation of dialog service using WPF UI controls
    /// </summary>
    public class DialogService : IDialogService
    {
        private readonly TaxInputPageService _taxInputPageService;

        public DialogService(TaxInputPageService taxInputPageService)
        {
            _taxInputPageService = taxInputPageService ?? throw new ArgumentNullException(nameof(taxInputPageService));
        }

        public async Task ShowTaxCalculationResultsAsync(TaxCalculationResult taxCalculation, TaxRefundCalculation refundCalculation)
        {
            try
            {
                var breakdown = _taxInputPageService.GenerateCalculationBreakdown(taxCalculation, refundCalculation);

                var messageBox = new Wpf.Ui.Controls.MessageBox
                {
                    Title = "Tax Calculation Results",
                    Content = breakdown,
                    Width = 600,
                    Height = 500
                };

                await messageBox.ShowDialogAsync();
            }
            catch (Exception ex)
            {
                await ShowErrorAsync("Error", $"Failed to show calculation results: {ex.Message}");
            }
        }

        public async Task ShowRegimeComparisonAsync(RegimeComparisonResult comparison)
        {
            try
            {
                var comparisonText = _taxInputPageService.GenerateRegimeComparisonText(comparison);

                var messageBox = new Wpf.Ui.Controls.MessageBox
                {
                    Title = "Tax Regime Comparison",
                    Content = comparisonText,
                    Width = 500,
                    Height = 400
                };

                await messageBox.ShowDialogAsync();
            }
            catch (Exception ex)
            {
                await ShowErrorAsync("Error", $"Failed to show regime comparison: {ex.Message}");
            }
        }

        public async Task<bool> ShowConfirmationAsync(string title, string message)
        {
            try
            {
                var messageBox = new Wpf.Ui.Controls.MessageBox
                {
                    Title = title,
                    Content = message,
                    IsPrimaryButtonEnabled = true,
                    IsSecondaryButtonEnabled = true,
                    PrimaryButtonText = "Yes",
                    SecondaryButtonText = "No"
                };

                var result = await messageBox.ShowDialogAsync();
                return result == Wpf.Ui.Controls.MessageBoxResult.Primary;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task ShowErrorAsync(string title, string message)
        {
            try
            {
                var messageBox = new Wpf.Ui.Controls.MessageBox
                {
                    Title = title,
                    Content = message,
                    IsPrimaryButtonEnabled = true,
                    PrimaryButtonText = "OK"
                };

                await messageBox.ShowDialogAsync();
            }
            catch (Exception)
            {
                // Fallback to system message box if WPF UI fails
                System.Windows.MessageBox.Show(message, title, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        public async Task ShowInfoAsync(string title, string message)
        {
            try
            {
                var messageBox = new Wpf.Ui.Controls.MessageBox
                {
                    Title = title,
                    Content = message,
                    IsPrimaryButtonEnabled = true,
                    PrimaryButtonText = "OK"
                };

                await messageBox.ShowDialogAsync();
            }
            catch (Exception)
            {
                // Fallback to system message box if WPF UI fails
                System.Windows.MessageBox.Show(message, title, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
        }
    }
}
