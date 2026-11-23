using CourseApp.classes.enums;
using CourseApp.classes.information;
using CourseApp.classes.services.CourseApp.classes;
using CourseApp.windows.authentication;
using CourseApp.windows.views;
using System.Windows;

namespace CourseApp.windows.mainmenu
{
    public partial class ClientWindow : Window
    {
        private readonly Client _currentClient;
        private readonly DatingService _datingService;

        public ClientWindow(Client currentClient, DatingService datingService)
        {
            InitializeComponent();
            _currentClient = currentClient;
            _datingService = datingService;

            UpdateClientHeader();
            UpdateSidebarBadges();
            NavMatches.IsChecked = true;
        }

        public void UpdateClientHeader()
        {
            ClientNameText.Text = _currentClient.FullName;
            ClientMetaText.Text = $"{_currentClient.RegNumber} • {_currentClient.Age} років";
        }

        public void UpdateSidebarBadges()
        {
            int matchesCount = _datingService.FindMatchesForClient(_currentClient, false, "").Count;
            MatchesBadgeText.Text = matchesCount.ToString();
            MatchesBadge.Visibility = matchesCount > 0 ? Visibility.Visible : Visibility.Collapsed;

            int pendingCount = _datingService.GetPendingIncomingCount(_currentClient.RegNumber ?? "");
            MeetingsBadgeText.Text = pendingCount.ToString();
            MeetingsBadge.Visibility = pendingCount > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void OnProfileUpdated()
        {
            if (_currentClient.Status != ClientStatus.Active)
            {
                LoginWindow loginWindow = new LoginWindow(_datingService);
                loginWindow.Show();
                Close();
                return;
            }

            UpdateClientHeader();
            UpdateSidebarBadges();
        }

        private void NavMatches_Checked(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new ClientMatchesView(_currentClient, _datingService, UpdateSidebarBadges);
        }

        private void NavMeetings_Checked(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new ClientMeetingsView(_currentClient, _datingService, UpdateSidebarBadges);
        }

        private void NavProfile_Checked(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new ProfileMenuView(_datingService, OnProfileUpdated);
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow(_datingService);
            loginWindow.Show();
            Close();
        }
    }
}