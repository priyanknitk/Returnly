using System.Windows;
using System.Windows.Controls;
using Returnly.Services;
using Returnly.Models;
using Returnly.ViewModels;
using System.Collections.ObjectModel;
using System;
using System.IO;
using System.Text;
using Microsoft.Win32;

namespace Returnly
{
    public partial class TaxCalculationResultsPage : Page
    {
        private readonly NotificationService _notificationService;
        private readonly TaxCalculationResultsViewModel _viewModel;

        public TaxCalculationResultsPage(TaxCalculationResult taxCalculationResult, 
                                        TaxRefundCalculation refundCalculation, 
                                        Form16Data form16Data,
                                        RegimeComparisonResult? regimeComparison = null)
        {
            InitializeComponent();
            
            _notificationService = new NotificationService(NotificationPanel, NotificationTextBlock);
            
            // Create ViewModel
            _viewModel = new TaxCalculationResultsViewModel(
                taxCalculationResult,
                refundCalculation,
                form16Data,
                _notificationService,
                regimeComparison);
            
            // Set DataContext for binding
            this.DataContext = _viewModel;
            
            // Subscribe to ViewModel events
            _viewModel.BackToTaxInputRequested += OnBackToTaxInputRequested;
            _viewModel.RecalculateRequested += OnRecalculateRequested;
        }

        #region Event Handlers

        private void OnBackToTaxInputRequested(object? sender, EventArgs e)
        {
            NavigationService?.GoBack();
        }

        private void OnRecalculateRequested(object? sender, EventArgs e)
        {
            NavigationService?.Navigate(new TaxDataInputPage(_viewModel.Form16Data));
        }

        #endregion

        #region Button Click Events (for XAML compatibility)

        private void BackToTaxInput_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.BackToTaxInputCommand.Execute(null);
        }

        private void Recalculate_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.RecalculateCommand.Execute(null);
        }

        private void ExportResults_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ExportResultsCommand.Execute(null);
        }

        private void ContinueToReturns_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ContinueToReturnsCommand.Execute(null);
        }

        #endregion
    }
}