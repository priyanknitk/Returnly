using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows.Input;
using Microsoft.Win32;
using Returnly.Models;
using Returnly.Services;

namespace Returnly.ViewModels
{
    public class TaxCalculationResultsViewModel : BaseViewModel
    {
        private readonly NotificationService _notificationService;
        private readonly TaxCalculationResult _taxCalculationResult;
        private readonly TaxRefundCalculation _refundCalculation;
        private readonly Form16Data _form16Data;
        private RegimeComparisonResult? _regimeComparison;

        #region Properties

        private string _subtitleText = string.Empty;
        public string SubtitleText
        {
            get => _subtitleText;
            set => SetProperty(ref _subtitleText, value);
        }

        private string _taxableIncomeText = "₹0";
        public string TaxableIncomeText
        {
            get => _taxableIncomeText;
            set => SetProperty(ref _taxableIncomeText, value);
        }

        private string _taxLiabilityText = "₹0";
        public string TaxLiabilityText
        {
            get => _taxLiabilityText;
            set => SetProperty(ref _taxLiabilityText, value);
        }

        private string _tdsDeductedText = "₹0";
        public string TDSDeductedText
        {
            get => _tdsDeductedText;
            set => SetProperty(ref _tdsDeductedText, value);
        }

        private string _refundDueLabel = "Refund Due";
        public string RefundDueLabel
        {
            get => _refundDueLabel;
            set => SetProperty(ref _refundDueLabel, value);
        }

        private string _refundDueAmountText = "₹0";
        public string RefundDueAmountText
        {
            get => _refundDueAmountText;
            set => SetProperty(ref _refundDueAmountText, value);
        }

        private Wpf.Ui.Controls.SymbolRegular _refundDueIconSymbol = Wpf.Ui.Controls.SymbolRegular.Money24;
        public Wpf.Ui.Controls.SymbolRegular RefundDueIconSymbol
        {
            get => _refundDueIconSymbol;
            set => SetProperty(ref _refundDueIconSymbol, value);
        }

        private System.Windows.Media.Brush? _refundDueIconBrush;
        public System.Windows.Media.Brush? RefundDueIconBrush
        {
            get => _refundDueIconBrush;
            set => SetProperty(ref _refundDueIconBrush, value);
        }

        private ObservableCollection<TaxSlabCalculation> _taxBreakdown = new();
        public ObservableCollection<TaxSlabCalculation> TaxBreakdown
        {
            get => _taxBreakdown;
            set => SetProperty(ref _taxBreakdown, value);
        }

        private string _subtotalText = "₹0";
        public string SubtotalText
        {
            get => _subtotalText;
            set => SetProperty(ref _subtotalText, value);
        }

        private string _surchargeLabel = "Surcharge:";
        public string SurchargeLabel
        {
            get => _surchargeLabel;
            set => SetProperty(ref _surchargeLabel, value);
        }

        private string _surchargeText = "₹0";
        public string SurchargeText
        {
            get => _surchargeText;
            set => SetProperty(ref _surchargeText, value);
        }

        private bool _isSurchargeVisible = true;
        public bool IsSurchargeVisible
        {
            get => _isSurchargeVisible;
            set => SetProperty(ref _isSurchargeVisible, value);
        }

        private string _cessText = "₹0";
        public string CessText
        {
            get => _cessText;
            set => SetProperty(ref _cessText, value);
        }

        private string _totalTaxText = "₹0";
        public string TotalTaxText
        {
            get => _totalTaxText;
            set => SetProperty(ref _totalTaxText, value);
        }

        private string _effectiveRateText = "0%";
        public string EffectiveRateText
        {
            get => _effectiveRateText;
            set => SetProperty(ref _effectiveRateText, value);
        }

        private bool _isRegimeComparisonVisible = false;
        public bool IsRegimeComparisonVisible
        {
            get => _isRegimeComparisonVisible;
            set => SetProperty(ref _isRegimeComparisonVisible, value);
        }

        private string _oldRegimeTaxText = "Total Tax: ₹0";
        public string OldRegimeTaxText
        {
            get => _oldRegimeTaxText;
            set => SetProperty(ref _oldRegimeTaxText, value);
        }

        private string _oldRegimeRateText = "Effective Rate: 0%";
        public string OldRegimeRateText
        {
            get => _oldRegimeRateText;
            set => SetProperty(ref _oldRegimeRateText, value);
        }

        private string _newRegimeTaxText = "Total Tax: ₹0";
        public string NewRegimeTaxText
        {
            get => _newRegimeTaxText;
            set => SetProperty(ref _newRegimeTaxText, value);
        }

        private string _newRegimeRateText = "Effective Rate: 0%";
        public string NewRegimeRateText
        {
            get => _newRegimeRateText;
            set => SetProperty(ref _newRegimeRateText, value);
        }

        private string _recommendationText = "Recommendation: New Tax Regime";
        public string RecommendationText
        {
            get => _recommendationText;
            set => SetProperty(ref _recommendationText, value);
        }

        private string _savingsText = "Savings: ₹0 (0%)";
        public string SavingsText
        {
            get => _savingsText;
            set => SetProperty(ref _savingsText, value);
        }

        #endregion

        #region Commands

        public ICommand BackToTaxInputCommand { get; }
        public ICommand RecalculateCommand { get; }
        public ICommand ExportResultsCommand { get; }
        public ICommand ContinueToReturnsCommand { get; }

        #endregion

        public TaxCalculationResultsViewModel(
            TaxCalculationResult taxCalculationResult,
            TaxRefundCalculation refundCalculation,
            Form16Data form16Data,
            NotificationService notificationService,
            RegimeComparisonResult? regimeComparison = null)
        {
            _taxCalculationResult = taxCalculationResult ?? throw new ArgumentNullException(nameof(taxCalculationResult));
            _refundCalculation = refundCalculation ?? throw new ArgumentNullException(nameof(refundCalculation));
            _form16Data = form16Data ?? throw new ArgumentNullException(nameof(form16Data));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _regimeComparison = regimeComparison;

            // Initialize commands
            BackToTaxInputCommand = new RelayCommand(ExecuteBackToTaxInput);
            RecalculateCommand = new RelayCommand(ExecuteRecalculate);
            ExportResultsCommand = new RelayCommand(ExecuteExportResults);
            ContinueToReturnsCommand = new RelayCommand(ExecuteContinueToReturns);

            // Load the tax results
            LoadTaxResults();
        }

        private void LoadTaxResults()
        {
            try
            {
                // Update subtitle
                SubtitleText = $"Your tax calculation for Financial Year {_taxCalculationResult.FinancialYear} (New Tax Regime)";

                // Update summary cards with improved formatting
                TaxableIncomeText = $"₹{_taxCalculationResult.TaxableIncome:N0}";
                TaxLiabilityText = $"₹{_taxCalculationResult.TotalTaxWithCess:N0}";
                TDSDeductedText = $"₹{_refundCalculation.TDSDeducted:N0}";

                // Update refund/due information
                UpdateRefundDueInformation();

                // Load tax breakdown
                TaxBreakdown.Clear();
                foreach (var slab in _taxCalculationResult.TaxBreakdown)
                {
                    TaxBreakdown.Add(slab);
                }

                // Update tax summary with improved formatting
                SubtotalText = $"₹{_taxCalculationResult.TotalTax:N2}";

                // Show/hide surcharge based on whether it's applicable
                if (_taxCalculationResult.Surcharge > 0)
                {
                    SurchargeLabel = $"Surcharge ({_taxCalculationResult.SurchargeRate:F0}%):";
                    SurchargeText = $"₹{_taxCalculationResult.Surcharge:N2}";
                    IsSurchargeVisible = true;
                }
                else
                {
                    IsSurchargeVisible = false;
                }

                CessText = $"₹{_taxCalculationResult.HealthAndEducationCess:N2}";
                TotalTaxText = $"₹{_taxCalculationResult.TotalTaxWithCess:N2}";
                EffectiveRateText = $"{_taxCalculationResult.EffectiveTaxRate:F2}%";

                // Load regime comparison if available
                if (_regimeComparison != null)
                {
                    LoadRegimeComparison();
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification($"Error loading tax results: {ex.Message}", NotificationType.Error);
            }
        }

        private void UpdateRefundDueInformation()
        {
            if (_refundCalculation.IsRefundDue)
            {
                RefundDueLabel = "Refund Due";
                RefundDueAmountText = $"₹{_refundCalculation.RefundAmount:N0}";
                RefundDueIconSymbol = Wpf.Ui.Controls.SymbolRegular.Money24;
                RefundDueIconBrush = System.Windows.Application.Current.Resources["SystemFillColorSuccessBrush"] as System.Windows.Media.Brush ?? System.Windows.Media.Brushes.Green;
            }
            else if (_refundCalculation.AdditionalTaxDue > 0)
            {
                RefundDueLabel = "Additional Tax Due";
                RefundDueAmountText = $"₹{_refundCalculation.AdditionalTaxDue:N0}";
                RefundDueIconSymbol = Wpf.Ui.Controls.SymbolRegular.Warning24;
                RefundDueIconBrush = System.Windows.Application.Current.Resources["SystemFillColorCautionBrush"] as System.Windows.Media.Brush ?? System.Windows.Media.Brushes.Orange;
            }
            else
            {
                RefundDueLabel = "Tax Balanced";
                RefundDueAmountText = "₹0";
                RefundDueIconSymbol = Wpf.Ui.Controls.SymbolRegular.Checkmark24;
                RefundDueIconBrush = System.Windows.Application.Current.Resources["SystemFillColorSuccessBrush"] as System.Windows.Media.Brush ?? System.Windows.Media.Brushes.Green;
            }
        }

        private void LoadRegimeComparison()
        {
            if (_regimeComparison == null) return;

            IsRegimeComparisonVisible = true;

            OldRegimeTaxText = $"Total Tax: ₹{_regimeComparison.OldRegimeCalculation.TotalTaxWithCess:N0}";
            OldRegimeRateText = $"Effective Rate: {_regimeComparison.OldRegimeCalculation.EffectiveTaxRate:F2}%";

            NewRegimeTaxText = $"Total Tax: ₹{_regimeComparison.NewRegimeCalculation.TotalTaxWithCess:N0}";
            NewRegimeRateText = $"Effective Rate: {_regimeComparison.NewRegimeCalculation.EffectiveTaxRate:F2}%";

            RecommendationText = $"Recommendation: {_regimeComparison.RecommendedRegime} Tax Regime";

            if (_regimeComparison.TaxSavings > 0)
            {
                SavingsText = $"Savings: ₹{Math.Abs(_regimeComparison.TaxSavings):N0} ({Math.Abs(_regimeComparison.SavingsPercentage):F1}%)";
            }
            else if (_regimeComparison.TaxSavings < 0)
            {
                SavingsText = $"Additional Cost: ₹{Math.Abs(_regimeComparison.TaxSavings):N0} ({Math.Abs(_regimeComparison.SavingsPercentage):F1}%)";
            }
            else
            {
                SavingsText = "No difference between regimes";
            }
        }

        #region Command Implementations

        private void ExecuteBackToTaxInput()
        {
            // This will be handled by the View through navigation events
            OnBackToTaxInputRequested();
        }

        private void ExecuteRecalculate()
        {
            // This will be handled by the View through navigation events
            OnRecalculateRequested();
        }

        private void ExecuteExportResults()
        {
            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Title = "Export Tax Calculation Results",
                    Filter = "Text Files (*.txt)|*.txt|CSV Files (*.csv)|*.csv",
                    DefaultExt = "txt",
                    FileName = $"TaxCalculation_{_taxCalculationResult.FinancialYear}_{DateTime.Now:yyyyMMdd}"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var content = GenerateExportContent(saveFileDialog.FilterIndex == 2);
                    File.WriteAllText(saveFileDialog.FileName, content, Encoding.UTF8);

                    _notificationService.ShowNotification($"Tax calculation results exported to {Path.GetFileName(saveFileDialog.FileName)}", NotificationType.Success);
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification($"Error exporting results: {ex.Message}", NotificationType.Error);
            }
        }

        private void ExecuteContinueToReturns()
        {
            // TODO: Navigate to tax return generation page
            _notificationService.ShowNotification("Tax return generation feature coming soon!", NotificationType.Info);
        }

        #endregion

        #region Events

        public event EventHandler? BackToTaxInputRequested;
        public event EventHandler? RecalculateRequested;

        protected virtual void OnBackToTaxInputRequested()
        {
            BackToTaxInputRequested?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnRecalculateRequested()
        {
            RecalculateRequested?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Export Methods

        private string GenerateExportContent(bool isCsv)
        {
            var separator = isCsv ? "," : "\t";
            var sb = new StringBuilder();

            if (isCsv)
            {
                sb.AppendLine("Tax Calculation Results Export");
                sb.AppendLine($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine($"Financial Year: {_taxCalculationResult.FinancialYear}");
                sb.AppendLine($"Tax Regime: {_taxCalculationResult.TaxRegime}");
                sb.AppendLine();

                sb.AppendLine("Summary");
                sb.AppendLine($"Taxable Income{separator}₹{_taxCalculationResult.TaxableIncome:N2}");
                sb.AppendLine($"Income Tax{separator}₹{_taxCalculationResult.TotalTax:N2}");
                if (_taxCalculationResult.Surcharge > 0)
                {
                    sb.AppendLine($"Surcharge ({_taxCalculationResult.SurchargeRate}%){separator}₹{_taxCalculationResult.Surcharge:N2}");
                }
                sb.AppendLine($"Health & Education Cess (4%){separator}₹{_taxCalculationResult.HealthAndEducationCess:N2}");
                sb.AppendLine($"Total Tax Liability{separator}₹{_taxCalculationResult.TotalTaxWithCess:N2}");
                sb.AppendLine($"TDS Deducted{separator}₹{_refundCalculation.TDSDeducted:N2}");
                sb.AppendLine($"Refund/Due{separator}₹{(_refundCalculation.IsRefundDue ? _refundCalculation.RefundAmount : -_refundCalculation.AdditionalTaxDue):N2}");
                sb.AppendLine();

                sb.AppendLine("Tax Breakdown");
                sb.AppendLine($"Slab Description{separator}Income in Slab{separator}Tax Rate{separator}Tax Amount");

                foreach (var slab in _taxCalculationResult.TaxBreakdown)
                {
                    sb.AppendLine($"{slab.SlabDescription}{separator}₹{slab.IncomeInSlab:N2}{separator}{slab.TaxRate}%{separator}₹{slab.TaxAmount:N2}");
                }
            }
            else
            {
                sb.AppendLine("TAX CALCULATION RESULTS");
                sb.AppendLine("=".PadRight(50, '='));
                sb.AppendLine($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine($"Financial Year: {_taxCalculationResult.FinancialYear}");
                sb.AppendLine($"Tax Regime: {_taxCalculationResult.TaxRegime}");
                sb.AppendLine();

                sb.AppendLine("SUMMARY");
                sb.AppendLine("-".PadRight(30, '-'));
                sb.AppendLine($"Taxable Income:           ₹{_taxCalculationResult.TaxableIncome:N2}");
                sb.AppendLine($"Income Tax:               ₹{_taxCalculationResult.TotalTax:N2}");
                if (_taxCalculationResult.Surcharge > 0)
                {
                    sb.AppendLine($"Surcharge ({_taxCalculationResult.SurchargeRate}%):        ₹{_taxCalculationResult.Surcharge:N2}");
                }
                sb.AppendLine($"Health & Education Cess:  ₹{_taxCalculationResult.HealthAndEducationCess:N2}");
                sb.AppendLine($"Total Tax Liability:      ₹{_taxCalculationResult.TotalTaxWithCess:N2}");
                sb.AppendLine($"TDS Deducted:             ₹{_refundCalculation.TDSDeducted:N2}");
                sb.AppendLine($"Effective Tax Rate:       {_taxCalculationResult.EffectiveTaxRate:F2}%");
                sb.AppendLine();

                if (_refundCalculation.IsRefundDue)
                {
                    sb.AppendLine($"REFUND DUE:               ₹{_refundCalculation.RefundAmount:N2}");
                }
                else if (_refundCalculation.AdditionalTaxDue > 0)
                {
                    sb.AppendLine($"ADDITIONAL TAX DUE:       ₹{_refundCalculation.AdditionalTaxDue:N2}");
                }
                else
                {
                    sb.AppendLine("TAX LIABILITY BALANCED");
                }
                sb.AppendLine();

                sb.AppendLine("TAX BREAKDOWN");
                sb.AppendLine("-".PadRight(80, '-'));

                foreach (var slab in _taxCalculationResult.TaxBreakdown)
                {
                    sb.AppendLine($"{slab.SlabDescription.PadRight(30)} ₹{slab.IncomeInSlab:N0} @ {slab.TaxRate}% = ₹{slab.TaxAmount:N2}");
                }

                sb.AppendLine("-".PadRight(80, '-'));
                sb.AppendLine($"{"Subtotal (Income Tax):".PadRight(30)} ₹{_taxCalculationResult.TotalTax:N2}");
                if (_taxCalculationResult.Surcharge > 0)
                {
                    sb.AppendLine($"{"Surcharge (" + _taxCalculationResult.SurchargeRate + "%):".PadRight(30)} ₹{_taxCalculationResult.Surcharge:N2}");
                }
                sb.AppendLine($"{"Health & Education Cess (4%):".PadRight(30)} ₹{_taxCalculationResult.HealthAndEducationCess:N2}");
                sb.AppendLine($"{"TOTAL TAX LIABILITY:".PadRight(30)} ₹{_taxCalculationResult.TotalTaxWithCess:N2}");
            }

            return sb.ToString();
        }

        #endregion

        #region Public Properties for Navigation

        public TaxCalculationResult TaxCalculationResult => _taxCalculationResult;
        public Form16Data Form16Data => _form16Data;

        #endregion
    }
}
