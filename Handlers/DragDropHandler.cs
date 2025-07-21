using System.Windows;
using System.Windows.Controls;

namespace Returnly.Handlers
{
    public class DragDropHandler
    {
        private readonly Border _uploadBorder;
        private readonly Action<string> _onFileDropped;

        public DragDropHandler(Border uploadBorder, Action<string> onFileDropped)
        {
            _uploadBorder = uploadBorder;
            _onFileDropped = onFileDropped;
            SetupDragAndDrop();
        }

        private void SetupDragAndDrop()
        {
            _uploadBorder.AllowDrop = true;
            _uploadBorder.DragEnter += UploadBorder_DragEnter;
            _uploadBorder.DragOver += UploadBorder_DragOver;
            _uploadBorder.DragLeave += UploadBorder_DragLeave;
            _uploadBorder.Drop += UploadBorder_Drop;
        }

        private void UploadBorder_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
                _uploadBorder.Opacity = 0.8;
                _uploadBorder.BorderThickness = new Thickness(3);
                _uploadBorder.BorderBrush = System.Windows.Media.Brushes.DodgerBlue;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void UploadBorder_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effects = DragDropEffects.Copy;
            else
                e.Effects = DragDropEffects.None;
            e.Handled = true;
        }

        private void UploadBorder_DragLeave(object sender, DragEventArgs e)
        {
            ResetVisualFeedback();
        }

        private void UploadBorder_Drop(object sender, DragEventArgs e)
        {
            ResetVisualFeedback();

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                try
                {
                    string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                    if (files?.Length > 0)
                    {
                        _onFileDropped(files[0]);
                    }
                }
                catch (Exception ex)
                {
                    // Handle error through callback if needed
                    throw new InvalidOperationException($"Error handling dropped file: {ex.Message}", ex);
                }
            }
            e.Handled = true;
        }

        private void ResetVisualFeedback()
        {
            _uploadBorder.Opacity = 1.0;
            _uploadBorder.BorderThickness = new Thickness(2);
            _uploadBorder.ClearValue(Border.BorderBrushProperty);
        }
    }
}