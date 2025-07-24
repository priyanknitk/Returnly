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
        private TaxDataInputViewModel _viewModel;
        private readonly Form16Data _form16Data;

        public TaxDataInputPage() : this(new Form16Data())
        {
        }

        public TaxDataInputPage(Form16Data form16Data)
        {
            InitializeComponent();
            
            _form16Data = form16Data ?? new Form16Data();
            _notificationService = new NotificationService(NotificationPanel, NotificationTextBlock);
            
            // Create ViewModel using factory method - NavigationService may be null initially
            _viewModel = TaxDataInputViewModel.CreateForUI(_notificationService, _form16Data, null);
            
            // Set the DataContext for binding
            this.DataContext = _viewModel;
            
            // Load data from Form16Data into ViewModel
            _viewModel.LoadFromForm16Data(_form16Data);
            
            // Subscribe to Loaded event to get NavigationService
            this.Loaded += TaxDataInputPage_Loaded;
        }

        private void TaxDataInputPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Update ViewModel with navigation service once it's available
            if (NavigationService != null)
            {
                // Recreate ViewModel with navigation service
                var currentTaxCalculation = _viewModel.CurrentTaxCalculation;
                _viewModel = TaxDataInputViewModel.CreateForUI(_notificationService, _form16Data, NavigationService);
                _viewModel.LoadFromForm16Data(_form16Data);
                
                // Restore current state if available
                if (currentTaxCalculation != null)
                {
                    // We'll need to add a setter for CurrentTaxCalculation or handle this differently
                    // For now, let's leave this as a TODO
                }
                
                this.DataContext = _viewModel;
            }
        }
    }
}