using System.Linq;
using System.Windows;
using CourseApp.classes;
using CourseApp.classes.information;
using CourseApp.classes.services.CourseApp.classes;
using CourseApp.classes.enums;

namespace CourseApp.windows.mainmenu
{
    public partial class MeetingsWindow : Window
    {
        private readonly DatingService _datingService;
        private readonly Client? _currentClient;

        public MeetingsWindow(DatingService datingService, Client? currentClient = null)
        {
            InitializeComponent();
            _datingService = datingService;
            _currentClient = currentClient;

            if (_currentClient != null)
                TxtTitle.Text = "📅 Мої зустрічі";

            LoadMeetings();
        }

        private void Filter_Checked(object sender, RoutedEventArgs e)
        {
            if (_datingService == null || MeetingsListBox == null) return;
            LoadMeetings();
        }

        private void LoadMeetings()
        {
            var query = _currentClient == null
                ? _datingService.Meetings.AsEnumerable()
                : _datingService.Meetings.Where(m =>
                    m.Initiator?.RegNumber == _currentClient.RegNumber ||
                    m.Target?.RegNumber == _currentClient.RegNumber);

            if (RbPending.IsChecked == true)
                query = query.Where(m => m.Status == MeetingStatus.Pending);
            else if (RbAccepted.IsChecked == true)
                query = query.Where(m => m.Status == MeetingStatus.Accepted);
            else if (RbRejected.IsChecked == true)
                query = query.Where(m => m.Status == MeetingStatus.Rejected);

            var list = query.ToList();
            MeetingsListBox.ItemsSource = list;
            TxtMeetingsCount.Text = $"Зустрічей: {list.Count}";

            EmptyMeetingsPanel.Visibility = list.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            MeetingsListBox.Visibility = list.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ChangeSelectedStatus(MeetingStatus status)
        {
            if (MeetingsListBox.SelectedItem is not MeetingInvitation selected)
            {
                MessageBox.Show("Оберіть зустріч зі списку.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _datingService.UpdateMeetingStatus(selected.Id, status);
            LoadMeetings();
        }

        private void BtnAccept_Click(object sender, RoutedEventArgs e) => ChangeSelectedStatus(MeetingStatus.Accepted);
        private void BtnReject_Click(object sender, RoutedEventArgs e) => ChangeSelectedStatus(MeetingStatus.Rejected);
        private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();
    }
}