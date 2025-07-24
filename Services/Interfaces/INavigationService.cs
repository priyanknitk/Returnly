using System.Windows.Controls;

namespace Returnly.Services.Interfaces
{
    public interface INavigationService
    {
        void NavigateTo<T>() where T : Page, new();
        void NavigateTo(Page page);
        bool CanGoBack { get; }
        void GoBack();
    }
}
