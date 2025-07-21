using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace Returnly
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : FluentWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            ApplicationThemeManager.Apply(this);
        }

        private void GetStarted_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Navigate to the main tax filing workflow
            // For now, show a message
            var messageBox = new Wpf.Ui.Controls.MessageBox
            {
                Title = "Coming Soon",
                Content = "Tax filing workflow will be available soon!"
            };
            messageBox.ShowDialogAsync();
        }

        private void LearnMore_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Show information about the app
            var messageBox = new Wpf.Ui.Controls.MessageBox
            {
                Title = "About Returnly",
                Content = "Returnly is your intelligent tax filing assistant designed to make tax season stress-free."
            };
            messageBox.ShowDialogAsync();
        }
    }
}