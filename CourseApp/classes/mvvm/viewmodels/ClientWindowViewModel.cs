using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CourseApp.classes.enums;
using CourseApp.classes.information;
using CourseApp.classes.services;
using CourseApp.windows.authentication;
using CourseApp.windows.views;

namespace CourseApp.classes.mvvm.viewmodels
{
    public class ClientWindowViewModel : ObservableObject
    {
        private readonly Client _client;
        private readonly AppServices _app;
        private readonly IDialogService _dialogs;
        private readonly Window _window;

        private string _name = string.Empty;
        private string _meta = string.Empty;
        private UserControl? _activeView;

        public string Name { get => _name; private set { _name = value; OnPropertyChanged(); } }
        public string Meta { get => _meta; private set { _meta = value; OnPropertyChanged(); } }
        public UserControl? ActiveView { get => _activeView; private set { _activeView = value; OnPropertyChanged(); } }

        public ICommand MatchesCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand MeetingsCommand { get; }
        public ICommand ProfileCommand { get; }
        public ICommand LogoutCommand { get; }

        public ClientWindowViewModel(Client currentClient, AppServices app, Window window)
        {
            _client = currentClient;
            _app = app;
            _dialogs = app.Dialogs;
            _window = window;

            MatchesCommand = new RelayCommand(_ => ShowMatches());
            SearchCommand = new RelayCommand(_ => ShowSearch());
            MeetingsCommand = new RelayCommand(_ => ShowMeetings());
            ProfileCommand = new RelayCommand(_ => ShowProfile());
            LogoutCommand = new RelayCommand(_ => ConfirmLogout());

            UpdateHeader();
            ShowMatches();
        }

        private void ShowMatches() => ActiveView = new ClientMatchesView(_client, _app.Matches, _app.Meetings, _app.Dialogs, UpdateHeader);
        private void ShowSearch() => ActiveView = new ClientSearchView(_client, _app.Clients);
        private void ShowMeetings() => ActiveView = new ClientMeetingsView(_client, _app.Meetings, UpdateHeader);
        private void ShowProfile() => ActiveView = new ProfileMenuView(_client, _app.Clients, _app.Dialogs, OnProfileChanged);

        private void UpdateHeader()
        {
            Name = _client.FullName ?? string.Empty;
            Meta = $"{_client.RegNumber} • {_client.Age} років";
        }

        private void OnProfileChanged()
        {
            if (_client.Status != ClientStatus.Active)
            {
                Logout();
                return;
            }
            UpdateHeader();
        }

        private void ConfirmLogout()
        {
            if (_dialogs.Confirm("Ви дійсно бажаєте вийти з акаунту?", "Вихід"))
                Logout();
        }

        private void Logout()
        {
            new LoginWindow(_app).Show();
            _window.Close();
        }
    }
}