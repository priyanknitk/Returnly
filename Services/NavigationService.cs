using System.Windows.Controls;
using System.Windows.Navigation;
using Returnly.Services.Interfaces;

namespace Returnly.Services
{
    public class PageNavigationService : INavigationService
    {
        private NavigationService? _wpfNavigationService;

        public PageNavigationService(NavigationService? wpfNavigationService = null)
        {
            _wpfNavigationService = wpfNavigationService;
        }

        public void SetNavigationService(NavigationService wpfNavigationService)
        {
            _wpfNavigationService = wpfNavigationService;
        }

        public void NavigateTo<T>() where T : Page, new()
        {
            var page = new T();
            NavigateTo(page);
        }

        public void NavigateTo(Page page)
        {
            _wpfNavigationService?.Navigate(page);
        }

        public bool CanGoBack => _wpfNavigationService?.CanGoBack ?? false;

        public void GoBack()
        {
            if (CanGoBack)
            {
                _wpfNavigationService?.GoBack();
            }
        }
    }
}
