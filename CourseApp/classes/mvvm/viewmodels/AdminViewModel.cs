using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CourseApp.classes.enums;
using CourseApp.classes.information;
using CourseApp.classes.services;
using CourseApp.windows.authentication;
using CourseApp.windows.mainmenu;
using CourseApp.windows.views;

namespace CourseApp.classes.mvvm.viewmodels
{
    public class AdminViewModel : ObservableObject
    {
        private readonly AppServices _app;
        private readonly IClientService _clients;
        private readonly IMeetingService _meetings;
        private readonly IMatchService _matches;
        private readonly IDialogService _dialogs;
        private readonly Window _window;

        private readonly ArchiveView _archiveView;
        private readonly ClientProfilesView _profilesView;
        private readonly ProfileMenuView _profileView;

        private AdminNavSection _section = AdminNavSection.SearchMatching;
        private Client? _activeClient;
        private Client? _selectedCandidate;
        private Client? _previewClient;

        private string _statsText = "Активних: 0 • Пар: 0 • Зустрічей: 0";
        private string _candidatesCount = "Знайдено: 0";
        private Visibility _searchVisibility = Visibility.Visible;
        private Visibility _hostVisibility = Visibility.Collapsed;
        private UserControl? _currentView;
        private Visibility _emptyVisibility = Visibility.Collapsed;
        private Visibility _candidatesVisibility = Visibility.Visible;

        private ImageSource? _previewAvatar;
        private Visibility _previewAvatarVisibility = Visibility.Collapsed;
        private Visibility _previewInitialsVisibility = Visibility.Visible;
        private string _previewInitials = "♥";
        private string _previewName = "Клієнта не обрано";
        private string _previewMeta = "—";
        private string _previewHeight = string.Empty;
        private Visibility _previewHeightVisibility = Visibility.Collapsed;
        private string _previewWeight = string.Empty;
        private Visibility _previewWeightVisibility = Visibility.Collapsed;
        private Visibility _previewHobbiesVisibility = Visibility.Collapsed;
        private string _previewDetails = "Інформація відсутня.";
        private string _previewReqs = "—";

        public ObservableCollection<Client> ActiveClients { get; } = new ObservableCollection<Client>();
        public ObservableCollection<Client> Candidates { get; } = new ObservableCollection<Client>();
        public ObservableCollection<string> PreviewHobbies { get; } = new ObservableCollection<string>();

        public AdminNavSection Section { get => _section; private set { _section = value; OnPropertyChanged(); } }
        public Client? ActiveClient { get => _activeClient; set { _activeClient = value; OnPropertyChanged(); OnClientChanged(); } }
        public Client? SelectedCandidate { get => _selectedCandidate; set { _selectedCandidate = value; OnPropertyChanged(); SetPreview(value ?? ActiveClient); } }

        public string StatsText { get => _statsText; private set { _statsText = value; OnPropertyChanged(); } }
        public string CandidatesCount { get => _candidatesCount; private set { _candidatesCount = value; OnPropertyChanged(); } }
        public Visibility SearchVisibility { get => _searchVisibility; private set { _searchVisibility = value; OnPropertyChanged(); } }
        public Visibility HostVisibility { get => _hostVisibility; private set { _hostVisibility = value; OnPropertyChanged(); } }
        public UserControl? CurrentView { get => _currentView; private set { _currentView = value; OnPropertyChanged(); } }
        public Visibility EmptyVisibility { get => _emptyVisibility; private set { _emptyVisibility = value; OnPropertyChanged(); } }
        public Visibility CandidatesVisibility { get => _candidatesVisibility; private set { _candidatesVisibility = value; OnPropertyChanged(); } }

        public ImageSource? PreviewAvatar { get => _previewAvatar; private set { _previewAvatar = value; OnPropertyChanged(); } }
        public Visibility PreviewAvatarVisibility { get => _previewAvatarVisibility; private set { _previewAvatarVisibility = value; OnPropertyChanged(); } }
        public Visibility PreviewInitialsVisibility { get => _previewInitialsVisibility; private set { _previewInitialsVisibility = value; OnPropertyChanged(); } }
        public string PreviewInitials { get => _previewInitials; private set { _previewInitials = value; OnPropertyChanged(); } }
        public string PreviewName { get => _previewName; private set { _previewName = value; OnPropertyChanged(); } }
        public string PreviewMeta { get => _previewMeta; private set { _previewMeta = value; OnPropertyChanged(); } }
        public string PreviewHeight { get => _previewHeight; private set { _previewHeight = value; OnPropertyChanged(); } }
        public Visibility PreviewHeightVisibility { get => _previewHeightVisibility; private set { _previewHeightVisibility = value; OnPropertyChanged(); } }
        public string PreviewWeight { get => _previewWeight; private set { _previewWeight = value; OnPropertyChanged(); } }
        public Visibility PreviewWeightVisibility { get => _previewWeightVisibility; private set { _previewWeightVisibility = value; OnPropertyChanged(); } }
        public Visibility PreviewHobbiesVisibility { get => _previewHobbiesVisibility; private set { _previewHobbiesVisibility = value; OnPropertyChanged(); } }
        public string PreviewDetails { get => _previewDetails; private set { _previewDetails = value; OnPropertyChanged(); } }
        public string PreviewReqs { get => _previewReqs; private set { _previewReqs = value; OnPropertyChanged(); } }

        public ICommand NavigateCommand { get; }
        public ICommand OpenMeetingsCommand { get; }
        public ICommand RegisterCommand { get; }
        public ICommand InviteCommand { get; }
        public ICommand ArchiveCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand LogoutCommand { get; }

        public AdminViewModel(AppServices app, Window window)
        {
            _app = app;
            _clients = app.Clients;
            _meetings = app.Meetings;
            _matches = app.Matches;
            _dialogs = app.Dialogs;
            _window = window;

            _archiveView = new ArchiveView(_clients, _meetings, _dialogs, Refresh);
            _profilesView = new ClientProfilesView(_clients);
            _profileView = new ProfileMenuView(_clients, _dialogs, Refresh);

            _archiveView.TabChanged += idx =>
            {
                switch (idx)
                {
                    case 0: Section = AdminNavSection.ActiveCouples; break;
                    case 1: Section = AdminNavSection.MoveArchive; break;
                    default: Section = AdminNavSection.DeletedClients; break;
                }
            };

            NavigateCommand = new RelayCommand(p => { if (p is AdminNavSection sec) Navigate(sec); });
            OpenMeetingsCommand = new RelayCommand(_ =>
            {
                new MeetingsWindow(_meetings, _dialogs) { Owner = _window }.ShowDialog();
                UpdateStats();
            });
            RegisterCommand = new RelayCommand(_ =>
            {
                if (new RegisterWindow(_app) { Owner = _window }.ShowDialog() == true)
                    Refresh();
            });
            InviteCommand = new RelayCommand(_ => Invite());
            ArchiveCommand = new RelayCommand(_ => Archive());
            DeleteCommand = new RelayCommand(_ => Delete());
            LogoutCommand = new RelayCommand(_ =>
            {
                new LoginWindow(_app).Show();
                _window.Close();
            });

            Refresh();
        }

        public void Navigate(AdminNavSection section)
        {
            Section = section;

            if (section == AdminNavSection.SearchMatching)
            {
                SearchVisibility = Visibility.Visible;
                HostVisibility = Visibility.Collapsed;
                return;
            }

            UserControl? view = null;
            switch (section)
            {
                case AdminNavSection.ClientProfiles:
                    _profilesView.RefreshUI();
                    view = _profilesView;
                    break;
                case AdminNavSection.EditProfile:
                    Client? target = _previewClient ?? ActiveClient;
                    if (target == null)
                    {
                        _dialogs.Warn("Немає активного клієнта для редагування.", "Увага");
                        return;
                    }
                    _profileView.LoadClientProfile(target);
                    view = _profileView;
                    break;
                case AdminNavSection.ActiveCouples:
                    _archiveView.SelectTab(0);
                    view = _archiveView;
                    break;
                case AdminNavSection.MoveArchive:
                    _archiveView.SelectTab(1);
                    view = _archiveView;
                    break;
                case AdminNavSection.DeletedClients:
                    _archiveView.SelectTab(2);
                    view = _archiveView;
                    break;
            }

            if (view == null) return;

            SearchVisibility = Visibility.Collapsed;
            CurrentView = view;
            HostVisibility = Visibility.Visible;
        }

        public void Refresh()
        {
            UpdateStats();

            ActiveClients.Clear();
            foreach (Client c in _clients.All)
            {
                if (c.Status == ClientStatus.Active)
                    ActiveClients.Add(c);
            }

            if (ActiveClients.Count > 0)
            {
                ActiveClient = ActiveClients[0];
            }
            else
            {
                ActiveClient = null;
                Candidates.Clear();
                CandidatesCount = "Знайдено: 0";
                EmptyVisibility = Visibility.Visible;
                CandidatesVisibility = Visibility.Collapsed;
                SetPreview(null);
            }

            _profilesView.RefreshUI();
            _archiveView.RefreshUI();
        }

        private void UpdateStats()
        {
            int active = 0;
            foreach (Client c in _clients.All)
            {
                if (c.Status == ClientStatus.Active) active++;
            }

            StatsText = $"Активних: {active} • Пар: {_clients.Archived.Count} • Зустрічей: {_meetings.All.Count}";
        }

        private void OnClientChanged()
        {
            if (ActiveClient == null) return;

            SetPreview(ActiveClient);
            Candidates.Clear();

            foreach (Client c in _matches.Find(ActiveClient))
                Candidates.Add(c);

            CandidatesCount = $"Знайдено: {Candidates.Count}";
            EmptyVisibility = Candidates.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            CandidatesVisibility = Candidates.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void SetPreview(Client? client)
        {
            _previewClient = client;

            if (client == null)
            {
                PreviewAvatar = null;
                PreviewAvatarVisibility = Visibility.Collapsed;
                PreviewInitialsVisibility = Visibility.Visible;
                PreviewInitials = "♥";
                PreviewName = "Клієнта не обрано";
                PreviewMeta = "—";
                PreviewHeightVisibility = Visibility.Collapsed;
                PreviewWeightVisibility = Visibility.Collapsed;
                PreviewHobbiesVisibility = Visibility.Collapsed;
                PreviewHobbies.Clear();
                PreviewDetails = "Інформація відсутня.";
                PreviewReqs = "—";
                return;
            }

            if (!string.IsNullOrWhiteSpace(client.AvatarPath) && File.Exists(client.AvatarPath))
            {
                PreviewAvatar = LoadBitmap(client.AvatarPath);
                PreviewAvatarVisibility = Visibility.Visible;
                PreviewInitialsVisibility = Visibility.Collapsed;
            }
            else
            {
                PreviewAvatar = null;
                PreviewAvatarVisibility = Visibility.Collapsed;
                PreviewInitialsVisibility = Visibility.Visible;
                PreviewInitials = string.IsNullOrWhiteSpace(client.FullName) ? "♥" : client.FullName.Trim()[0].ToString().ToUpper();
            }

            PreviewName = $"{client.FullName}, {client.Age} р.";
            PreviewMeta = $"{client.RegNumber} • Реєстрація: {client.RegDate:dd.MM.yyyy}";

            if (client.HeightCm.HasValue)
            {
                PreviewHeight = $"📏 {client.HeightCm.Value} см";
                PreviewHeightVisibility = Visibility.Visible;
            }
            else PreviewHeightVisibility = Visibility.Collapsed;

            if (client.WeightKg.HasValue)
            {
                PreviewWeight = $"⚖️ {client.WeightKg.Value:0.#} кг";
                PreviewWeightVisibility = Visibility.Visible;
            }
            else PreviewWeightVisibility = Visibility.Collapsed;

            PreviewHobbies.Clear();
            if (client.Hobbies != null && client.Hobbies.Count > 0)
            {
                foreach (string h in client.Hobbies) PreviewHobbies.Add(h);
                PreviewHobbiesVisibility = Visibility.Visible;
            }
            else PreviewHobbiesVisibility = Visibility.Collapsed;

            PreviewDetails = string.IsNullOrWhiteSpace(client.PersonalDetails) ? "Інформація відсутня." : client.PersonalDetails;

            if (client.Requirements != null)
            {
                string criteria = string.IsNullOrWhiteSpace(client.Requirements.CriteriaDescription) ? "Не вказано" : client.Requirements.CriteriaDescription;
                int minA = client.Requirements.MinAge > 0 ? client.Requirements.MinAge : 18;
                int maxA = client.Requirements.MaxAge > 0 ? client.Requirements.MaxAge : 99;
                PreviewReqs = $"• Бажана стать: {client.Requirements.PreferredGender}\n• Вік: від {minA} до {maxA} років\n• Критерії: {criteria}";
            }
            else PreviewReqs = "Вимоги не задані.";
        }

        private static BitmapImage? LoadBitmap(string path)
        {
            try
            {
                BitmapImage bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.UriSource = new Uri(path, UriKind.Absolute);
                bmp.EndInit();
                bmp.Freeze();
                return bmp;
            }
            catch
            {
                return null;
            }
        }

        private void Invite()
        {
            if (ActiveClient == null || SelectedCandidate == null)
            {
                _dialogs.Warn("Оберіть обох учасників для зустрічі.", "Увага");
                return;
            }

            _meetings.Create(ActiveClient, SelectedCandidate);
            UpdateStats();
            _dialogs.Info($"Запрошення між {ActiveClient.FullName} та {SelectedCandidate.FullName} надіслано!", "Зустріч створена");
        }

        private void Archive()
        {
            if (ActiveClient == null || SelectedCandidate == null)
            {
                _dialogs.Warn("Оберіть кандидата зі списку для перенесення пари в архів.", "Увага");
                return;
            }

            bool ok = _dialogs.Confirm(
                $"Перенести пару {ActiveClient.FullName} та {SelectedCandidate.FullName} до архіву?",
                "Архівація пари");

            if (!ok) return;

            string n1 = ActiveClient.FullName ?? string.Empty;
            string n2 = SelectedCandidate.FullName ?? string.Empty;

            _clients.Archive(ActiveClient, SelectedCandidate);
            Refresh();
            _dialogs.Info($"Пару {n1} та {n2} перенесено в архів! 💍", "Успіх");
        }

        private void Delete()
        {
            Client? target = _previewClient ?? ActiveClient;
            if (target == null || string.IsNullOrWhiteSpace(target.RegNumber)) return;

            bool ok = _dialogs.Confirm(
                $"Видалити клієнта {target.FullName} ({target.RegNumber}) до списку вилучених?",
                "Видалення клієнта",
                isWarning: true);

            if (!ok) return;

            _clients.Delete(target.RegNumber);
            Refresh();
        }
    }
}