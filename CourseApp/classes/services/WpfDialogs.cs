using System.Windows;
using Microsoft.Win32;

namespace CourseApp.classes.services
{
    public class WpfDialogs : IDialogService
    {
        public void Info(string message, string title) =>
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);

        public void Warn(string message, string title) =>
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);

        public void Error(string message, string title) =>
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);

        public bool Confirm(string message, string title, bool isWarning = false)
        {
            MessageBoxImage icon = isWarning ? MessageBoxImage.Warning : MessageBoxImage.Question;
            MessageBoxResult result = MessageBox.Show(message, title, MessageBoxButton.YesNo, icon);
            return result == MessageBoxResult.Yes;
        }

        public string? PickImage(string title)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Title = title,
                Filter = "Зображення (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp"
            };
            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }

        public string[] PickImages(string title)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Title = title,
                Filter = "Зображення (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp",
                Multiselect = true
            };
            return dialog.ShowDialog() == true ? dialog.FileNames : new string[0];
        }
    }
}