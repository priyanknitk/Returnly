using System.Windows;
using System.Windows.Input;
using Wpf.Ui.Controls;

namespace Returnly.Dialogs
{
    public partial class PasswordInputDialog : FluentWindow
    {
        public string Password { get; private set; } = string.Empty;
        public bool IsPasswordProvided { get; private set; } = false;

        public PasswordInputDialog()
        {
            InitializeComponent();
            PasswordBox.Focus();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(PasswordBox.Password))
            {
                ShowError("Please enter a password.");
                return;
            }

            Password = PasswordBox.Password;
            IsPasswordProvided = true;
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            IsPasswordProvided = false;
            DialogResult = false;
            Close();
        }

        private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                OK_Click(sender, e);
            }
            else if (e.Key == Key.Escape)
            {
                Cancel_Click(sender, e);
            }
        }

        private void ShowError(string message)
        {
            ErrorMessage.Text = message;
            ErrorMessage.Visibility = Visibility.Visible;
        }

        public void ShowIncorrectPasswordError()
        {
            ShowError("Incorrect password. Please try again.");
            PasswordBox.Clear();
            PasswordBox.Focus();
        }
    }
}