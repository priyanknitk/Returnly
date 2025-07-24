using System;
using System.Windows;
using System.Windows.Controls;
using Returnly.ViewModels;

namespace Returnly
{
    public partial class LandingPage : Page
    {
        public LandingPage()
        {
            try
            {
                InitializeComponent();
                
                // Set up the ViewModel's navigation service
                if (DataContext is LandingPageViewModel viewModel)
                {
                    Loaded += (s, e) => viewModel.SetNavigationService(NavigationService);
                }
            }
            catch (Exception ex)
            {
                var messageBox = new Wpf.Ui.Controls.MessageBox
                {
                    Title = "Initialization Error",
                    Content = $"Error initializing LandingPage: {ex.Message}"
                };
                messageBox.ShowDialogAsync();
            }
        }
    }
}