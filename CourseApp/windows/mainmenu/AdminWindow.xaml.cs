using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CourseApp.classes.information;
using CourseApp.classes.services.CourseApp.classes;
using CourseApp.windows.authentication;
using CourseApp.windows.views;
using CourseApp.classes.enums;

namespace CourseApp.windows.mainmenu
{
    public partial class AdminWindow : Window
    {
        private readonly DatingService _datingService;
        private Client? _currentActiveClient;

        private ArchiveView _archiveView = null!;
        private ClientProfilesView _clientProfilesView = null!;
        private ProfileMenuView _profileMenuView = null!;
        private Button[] _navButtons = null!;

        public AdminWindow(DatingService datingService)
        {
            InitializeComponent();
            _datingService = datingService;

            InitData();
        }

        private void InitData()
        {
            _archiveView = new ArchiveView(_datingService, RefreshAll);
            _clientProfilesView = new ClientProfilesView(_datingService);
            _profileMenuView = new ProfileMenuView(_datingService, RefreshAll);

            _archiveView.TabChanged += index =>
            {
                var targetBtn = index switch
                {
                    0 => BtnActiveCouples,
                    1 => BtnMoveArchive,
                    _ => BtnDeletedClients
                };
                HighlightNavButton(targetBtn);
            };

            _navButtons = new[]
            {
                BtnSearchMatching, BtnClientProfiles,
                BtnEditProfile, BtnPartnerReqs, BtnDeactivate,
                BtnActiveCouples, BtnMoveArchive, BtnDeletedClients
            };

            CurrentClientComboBox.SelectionChanged += CurrentClientComboBox_SelectionChanged;
            CandidatesListBox.SelectionChanged += (s, e) =>
            {
                if (CandidatesListBox.SelectedItem is Client candidate)
                    UpdateRightSidebar(candidate);
            };

            BtnLogin.Click += (s, e) => { new LoginWindow().Show(); Close(); };
            BtnRegister.Click += BtnRegister_Click;
            BtnSearchMatching.Click += (s, e) => ShowSearchView();
            BtnMeetingMgmt.Click += (s, e) => new MeetingsWindow(_datingService) { Owner = this }.ShowDialog();
            BtnClientProfiles.Click += (s, e) => { _clientProfilesView.RefreshUI(); ShowView(_clientProfilesView, BtnClientProfiles); };

            BtnEditProfile.Click += (s, e) => OpenProfileView(BtnEditProfile);
            BtnPartnerReqs.Click += (s, e) => OpenProfileView(BtnPartnerReqs);
            BtnDeactivate.Click += (s, e) => OpenProfileView(BtnDeactivate);

            BtnActiveCouples.Click += (s, e) => { _archiveView.SelectTab(0); ShowView(_archiveView, BtnActiveCouples); };
            BtnMoveArchive.Click += (s, e) => { _archiveView.SelectTab(1); ShowView(_archiveView, BtnMoveArchive); };
            BtnDeletedClients.Click += (s, e) => { _archiveView.SelectTab(2); ShowView(_archiveView, BtnDeletedClients); };

            BtnCreateMeeting.Click += (s, e) => CreateMeetingInvitationUI(CandidatesListBox.SelectedItem as Client);
            BtnArchiveCouple.Click += (s, e) =>
            {
                if (_currentActiveClient != null && CandidatesListBox.SelectedItem is Client target)
                    MoveCoupleToArchiveUI(_currentActiveClient, target);
                else
                    MessageBox.Show("Оберіть кандидата зі списку для перенесення пари в архів.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
            };
            BtnDeleteClient.Click += (s, e) =>
            {
                if (_currentActiveClient?.RegNumber != null)
                    DeleteClientUI(_currentActiveClient.RegNumber);
            };

            UpdateClientDropdown();
        }

        private void UpdateRightSidebar(Client? client)
        {
            if (client == null)
            {
                PreviewInitials.Text = "♥";
                PreviewFullName.Text = "Клієнта не обрано";
                PreviewMeta.Text = "—";
                PreviewDetails.Text = "Інформація відсутня.";
                PreviewRequirements.Text = "—";
                return;
            }

            PreviewInitials.Text = string.IsNullOrWhiteSpace(client.FullName)
                ? "♥"
                : client.FullName.Trim()[0].ToString().ToUpper();
            PreviewFullName.Text = $"{client.FullName}, {client.Age} р.";
            PreviewMeta.Text = $"{client.RegNumber} • Реєстрація: {client.RegDate:dd.MM.yyyy}";
            PreviewDetails.Text = string.IsNullOrWhiteSpace(client.PersonalDetails)
                ? "Інформація відсутня."
                : client.PersonalDetails;

            if (client.Requirements != null)
            {
                string criteria = string.IsNullOrWhiteSpace(client.Requirements.CriteriaDescription)
                    ? "Не вказано"
                    : client.Requirements.CriteriaDescription;

                PreviewRequirements.Text =
                    $"• Бажана стать: {client.Requirements.PreferredGender}\n" +
                    $"• Вік: від {client.Requirements.MinAge} до {client.Requirements.MaxAge} років\n" +
                    $"• Критерії: {criteria}";
            }
            else
            {
                PreviewRequirements.Text = "Вимоги не задані.";
            }
        }

        private void OpenProfileView(Button sourceBtn)
        {
            if (_currentActiveClient == null)
            {
                MessageBox.Show("Немає активного клієнта для редагування.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _profileMenuView.LoadClientProfile(_currentActiveClient);
            ShowView(_profileMenuView, sourceBtn);
        }

        private void ShowSearchView()
        {
            SearchMatchingView.Visibility = Visibility.Visible;
            ActiveViewHost.Visibility = Visibility.Collapsed;
            HighlightNavButton(BtnSearchMatching);
        }

        private void ShowView(UserControl view, Button activeBtn)
        {
            SearchMatchingView.Visibility = Visibility.Collapsed;
            ActiveViewHost.Content = view;
            ActiveViewHost.Visibility = Visibility.Visible;
            HighlightNavButton(activeBtn);
        }

        private void HighlightNavButton(Button activeBtn)
        {
            var defaultFg = (Brush)new BrushConverter().ConvertFrom("#555")!;
            var deactivateFg = (Brush)new BrushConverter().ConvertFrom("#E74C3C")!;

            foreach (var btn in _navButtons)
            {
                btn.Background = Brushes.Transparent;
                btn.Foreground = btn == BtnDeactivate ? deactivateFg : defaultFg;
            }

            activeBtn.Background = (Brush)FindResource("BrandColorLight");
            activeBtn.Foreground = (Brush)FindResource("BrandColor");
        }

        private void RefreshAll()
        {
            UpdateClientDropdown();
            _clientProfilesView.RefreshUI();
            _archiveView.RefreshUI();
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            if (new RegisterWindow(_datingService).ShowDialog() == true)
                RefreshAll();
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
                CandidatesListBox.ItemsSource = null;
                TxtCandidatesCount.Text = "Знайдено: 0";
                EmptyStatePanel.Visibility = Visibility.Visible;
                CandidatesListBox.Visibility = Visibility.Collapsed;
                UpdateRightSidebar(null);
            }
        }

        private void CurrentClientComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CurrentClientComboBox.SelectedItem is Client selectedClient)
            {
                _currentActiveClient = selectedClient;
                UpdateRightSidebar(_currentActiveClient);

                var candidates = _datingService.RunMatchingAlgorithm(_currentActiveClient);
                CandidatesListBox.ItemsSource = candidates;
                TxtCandidatesCount.Text = $"Знайдено: {candidates.Count}";

                EmptyStatePanel.Visibility = candidates.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
                CandidatesListBox.Visibility = candidates.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public void CreateMeetingInvitationUI(Client? target)
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
            RefreshAll();
        }

        public void DeleteClientUI(string regNumber)
        {
            _datingService.DeleteClient(regNumber);
            RefreshAll();
        }
    }
}