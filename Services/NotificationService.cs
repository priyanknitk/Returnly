using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Returnly.Services
{
    public class NotificationService
    {
        private readonly Border _notificationPanel;
        private readonly TextBlock _notificationTextBlock;

        public NotificationService(Border notificationPanel, TextBlock notificationTextBlock)
        {
            _notificationPanel = notificationPanel;
            _notificationTextBlock = notificationTextBlock;
        }

        public void ShowNotification(string message, NotificationType type)
        {
            _notificationTextBlock.Text = message;
            _notificationPanel.Visibility = Visibility.Visible;

            // Set color based on type
            _notificationPanel.Background = type switch
            {
                NotificationType.Success => System.Windows.Media.Brushes.LightGreen,
                NotificationType.Error => System.Windows.Media.Brushes.LightCoral,
                NotificationType.Warning => System.Windows.Media.Brushes.LightYellow,
                NotificationType.Info => System.Windows.Media.Brushes.LightBlue,
                _ => System.Windows.Media.Brushes.LightGray
            };

            // Auto-hide notification after 3 seconds
            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3)
            };
            timer.Tick += (s, e) =>
            {
                _notificationPanel.Visibility = Visibility.Collapsed;
                timer.Stop();
            };
            timer.Start();
        }
    }

    public enum NotificationType
    {
        Success,
        Error,
        Warning,
        Info
    }
}