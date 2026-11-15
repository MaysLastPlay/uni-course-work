using CourseApp.classes.information;
using CourseApp.classes.services.CourseApp.classes;
using System.Windows;

namespace CourseApp.windows.mainmenu
{
    public partial class ClientWindow : Window
    {
        private Client _currentClient;
        private DatingService _datingService;

        public ClientWindow(Client currentClient, DatingService datingService)
        {
            InitializeComponent();

            _currentClient = currentClient;
            _datingService = datingService;

            LoadUserData();
            LoadCandidates();
        }

        private void LoadUserData()
        {
            TxtFullName.Text = _currentClient.FullName;
            TxtRegNumber.Text = _currentClient.RegNumber;
            TxtPersonalDetails.Text = string.IsNullOrWhiteSpace(_currentClient.PersonalDetails)
                ? "Інформація відсутня."
                : _currentClient.PersonalDetails;
        }

        private void LoadCandidates()
        {
            var candidates = _datingService.RunMatchingAlgorithm(_currentClient);
            CandidatesDataGrid.ItemsSource = candidates;
        }

        private void BtnProposeMeeting_Click(object sender, RoutedEventArgs e)
        {
            if (CandidatesDataGrid.SelectedItem is Client targetClient)
            {
                _datingService.CreateMeetingInvitation(_currentClient, targetClient);
                MessageBox.Show($"Запрошення для {targetClient.FullName} успішно надіслано!",
                                "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Будь ласка, оберіть кандидата зі списку.",
                                "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}