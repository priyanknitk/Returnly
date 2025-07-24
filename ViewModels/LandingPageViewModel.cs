using System;
using System.Windows.Input;
using System.Windows.Navigation;
using Returnly.Services.Interfaces;

namespace Returnly.ViewModels
{
    public class LandingPageViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;

        public LandingPageViewModel() : this(new Services.PageNavigationService())
        {
        }

        public LandingPageViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            GetStartedCommand = new RelayCommand(ExecuteGetStarted, CanExecuteGetStarted);
            LearnMoreCommand = new RelayCommand(ExecuteLearnMore);
        }

        public ICommand GetStartedCommand { get; }
        public ICommand LearnMoreCommand { get; }

        public void SetNavigationService(NavigationService wpfNavigationService)
        {
            if (_navigationService is Services.PageNavigationService pageNavService)
            {
                pageNavService.SetNavigationService(wpfNavigationService);
            }
        }

        private void ExecuteGetStarted()
        {
            try
            {
                _navigationService.NavigateTo<Form16UploadPage>();
            }
            catch (Exception ex)
            {
                var errorMessageBox = new Wpf.Ui.Controls.MessageBox
                {
                    Title = "Error",
                    Content = $"Error navigating to Form16 Upload: {ex.Message}"
                };
                errorMessageBox.ShowDialogAsync();
            }
        }

        private bool CanExecuteGetStarted()
        {
            return true; // Always allow navigation for now
        }

        private void ExecuteLearnMore()
        {
            try
            {
                var messageBox = new Wpf.Ui.Controls.MessageBox
                {
                    Title = "About Returnly",
                    Content = "Returnly is your intelligent tax filing assistant designed to make tax season stress-free. " +
                             "Our application helps you:\n\n" +
                             "• Upload and process your Form16 documents\n" +
                             "• Calculate taxes under both old and new tax regimes\n" +
                             "• Compare which regime is more beneficial for you\n" +
                             "• Get detailed breakdowns of your tax calculations\n\n" +
                             "Start your journey to hassle-free tax filing today!"
                };
                messageBox.ShowDialogAsync();
            }
            catch (Exception ex)
            {
                var errorMessageBox = new Wpf.Ui.Controls.MessageBox
                {
                    Title = "Error",
                    Content = $"Error displaying information: {ex.Message}"
                };
                errorMessageBox.ShowDialogAsync();
            }
        }
    }
}
