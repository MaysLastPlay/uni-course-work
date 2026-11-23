using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CourseApp.classes;
using CourseApp.classes.information;
using CourseApp.classes.services.CourseApp.classes;
using CourseApp.classes.enums;

namespace CourseApp.windows.views
{
    public partial class ArchiveView : UserControl
    {
        private readonly DatingService _datingService;
        private readonly Action? _onDataChanged;

        public event Action<int>? TabChanged;

        public ArchiveView(DatingService datingService, Action? onDataChanged = null)
        {
            InitializeComponent();
            _datingService = datingService;
            _onDataChanged = onDataChanged;
            RefreshUI();
        }

        public void SelectTab(int index)
        {
            ArchiveTabControl.SelectedIndex = index;
            RefreshUI();
        }

        private void ArchiveTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.OriginalSource != ArchiveTabControl) return;
            TabChanged?.Invoke(ArchiveTabControl.SelectedIndex);
        }

        public void RefreshUI()
        {
            var activeClients = _datingService.AllClients.Where(c => c.Status == ClientStatus.Active).ToList();
            Client1ComboBox.ItemsSource = activeClients;
            Client2ComboBox.ItemsSource = activeClients;

            var couples = _datingService.ArchivedCouples.ToList();
            ActiveCouplesListBox.ItemsSource = couples;
            EmptyCouplesPanel.Visibility = couples.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            ActiveCouplesListBox.Visibility = couples.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

            var deleted = _datingService.DeletedClients.ToList();
            DeletedClientsListBox.ItemsSource = deleted;
            EmptyDeletedPanel.Visibility = deleted.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            DeletedClientsListBox.Visibility = deleted.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void MoveToArchive_Click(object sender, RoutedEventArgs e)
        {
            if (Client1ComboBox.SelectedItem is not Client client1 || Client2ComboBox.SelectedItem is not Client client2)
            {
                MessageBox.Show("Оберіть обох клієнтів для формування пари.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (client1.RegNumber == client2.RegNumber)
            {
                MessageBox.Show("Неможливо створити пару з одного й того ж клієнта!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _datingService.MoveCoupleToArchive(client1, client2);

            MessageBox.Show($"Пару {client1.FullName} та {client2.FullName} перенесено в архів! 💍",
                            "Архівація успішна", MessageBoxButton.OK, MessageBoxImage.Information);

            RefreshUI();
            _onDataChanged?.Invoke();
        }

        private void RestoreClient_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button { DataContext: Client client } || client.RegNumber == null) return;

            var confirm = MessageBox.Show(
                $"Відновити клієнта {client.FullName} ({client.RegNumber}) та повернути його до активної бази?",
                "Відновлення клієнта",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes) return;

            if (_datingService.RestoreClient(client.RegNumber))
            {
                RefreshUI();
                _onDataChanged?.Invoke();

                MessageBox.Show($"Клієнта {client.FullName} успішно відновлено!",
                                "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Не вдалося знайти клієнта для відновлення.",
                                "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        private void PermanentDeleteClient_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button { DataContext: Client client } || client.RegNumber == null) return;

            MessageBoxResult confirm = MessageBox.Show(
                $"Остаточно видалити клієнта {client.FullName} ({client.RegNumber}) з бази даних?\nЦю дію неможливо скасувати.",
                "Остаточне видалення",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes) return;

            if (_datingService.PermanentlyDeleteClient(client.RegNumber))
            {
                RefreshUI();
                _onDataChanged?.Invoke();

                MessageBox.Show($"Акаунт {client.FullName} повністю видалено.",
                                "Видалено", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Не вдалося знайти клієнта для видалення.",
                                "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}