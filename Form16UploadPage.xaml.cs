using System;
using System.Windows;
using System.Windows.Controls;
using Returnly.ViewModels;
using Returnly.Handlers;

namespace Returnly
{
    public partial class Form16UploadPage : Page
    {
        private Form16UploadPageViewModel ViewModel => (Form16UploadPageViewModel)DataContext;

        public Form16UploadPage()
        {
            try
            {
                InitializeComponent();
                
                // Set up the ViewModel's navigation service
                Loaded += (s, e) => ViewModel.SetNavigationService(NavigationService);
                
                // Initialize drag drop handlers for each section
                InitializeDragDropHandlers();
            }
            catch (Exception ex)
            {
                var messageBox = new Wpf.Ui.Controls.MessageBox
                {
                    Title = "Initialization Error",
                    Content = $"Error initializing Form16UploadPage: {ex.Message}"
                };
                messageBox.ShowDialogAsync();
            }
        }

        private void InitializeDragDropHandlers()
        {
            // Initialize drag drop handlers for each upload area
            new DragDropHandler(PartAUploadBorder, filePath => ViewModel.ProcessSelectedFile(filePath, ViewModels.Form16PartType.PartA));
            new DragDropHandler(PartBUploadBorder, filePath => ViewModel.ProcessSelectedFile(filePath, ViewModels.Form16PartType.PartB));
            new DragDropHandler(AnnexureUploadBorder, filePath => ViewModel.ProcessSelectedFile(filePath, ViewModels.Form16PartType.Annexure));
        }
    }
}