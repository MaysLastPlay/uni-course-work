using CourseApp.classes;
using CourseApp.classes.information;
using CourseApp.classes.services.CourseApp.classes;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace CourseApp.windows.mainmenu
{
    public partial class AdminWindow : Window
    {
        private DatingService _datingService;
        private Client _currentActiveClient;

        public AdminWindow()
        {
            InitializeComponent();
            _datingService = new DatingService();

            InitializeAppLogic();
        }

        private void InitializeAppLogic()
        {
            CurrentClientComboBox.SelectionChanged += CurrentClientComboBox_SelectionChanged;
            UpdateClientDropdown();
        }

        private void UpdateClientDropdown()
        {
            var activeClients = _datingService.AllClients.Where(c => c.Status == ClientStatus.Active).ToList();
            CurrentClientComboBox.ItemsSource = activeClients;

            if (activeClients.Any())
            {
                CurrentClientComboBox.SelectedIndex = 0;
            }
            else
            {
                _currentActiveClient = null;
                CandidatesDataGrid.ItemsSource = null;
            }
        }

        private void CurrentClientComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CurrentClientComboBox.SelectedItem is Client selectedClient)
            {
                _currentActiveClient = selectedClient;
                CandidatesDataGrid.ItemsSource = _datingService.RunMatchingAlgorithm(_currentActiveClient);
            }
        }

        public void RegisterNewClientUI(Client newClient)
        {
            string result = _datingService.RegisterNewClient(newClient);
            if (result == "Success")
            {
                UpdateClientDropdown();
                MessageBox.Show("Клієнт успішно зареєстрований!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(result, "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void CreateMeetingInvitationUI(Client target)
        {
            if (_currentActiveClient == null || target == null)
            {
                MessageBox.Show("Оберіть обох учасників для зустрічі.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _datingService.CreateMeetingInvitation(_currentActiveClient, target);
            MessageBox.Show($"Запрошення між {_currentActiveClient.FullName} та {target.FullName} надіслано!", "Зустріч створена", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void MoveCoupleToArchiveUI(Client client1, Client client2)
        {
            _datingService.MoveCoupleToArchive(client1, client2);
            UpdateClientDropdown();
        }

        public void DeleteClientUI(string regNumber)
        {
            _datingService.DeleteClient(regNumber);
            UpdateClientDropdown();
        }
    }
}