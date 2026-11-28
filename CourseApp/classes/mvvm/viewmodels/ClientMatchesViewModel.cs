using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using CourseApp.classes.information;
using CourseApp.classes.services;

namespace CourseApp.classes.mvvm.viewmodels
{
    public class ClientMatchesViewModel : ObservableObject
    {
        private readonly Client _client;
        private readonly IMatchService _matches;
        private readonly IMeetingService _meetings;
        private readonly IDialogService _dialogs;
        private readonly Action? _onChanged;

        private bool _mutualOnly;
        private string _subtitleText = string.Empty;
        private Visibility _emptyMatchesVisibility = Visibility.Collapsed;

        private Client? _picked;
        private Visibility _inviteOverlayVisibility = Visibility.Collapsed;
        private string _inviteTargetText = string.Empty;
        private string _inviteDate = string.Empty;
        private string _inviteTime = "18:00";

        public ObservableCollection<MatchDisplayItem> Matches { get; } = new ObservableCollection<MatchDisplayItem>();

        public bool MutualOnly
        {
            get => _mutualOnly;
            set { _mutualOnly = value; OnPropertyChanged(); Refresh(); }
        }

        public string SubtitleText { get => _subtitleText; private set { _subtitleText = value; OnPropertyChanged(); } }
        public Visibility EmptyMatchesVisibility { get => _emptyMatchesVisibility; private set { _emptyMatchesVisibility = value; OnPropertyChanged(); } }
        public Visibility InviteOverlayVisibility { get => _inviteOverlayVisibility; private set { _inviteOverlayVisibility = value; OnPropertyChanged(); } }
        public string InviteTargetText { get => _inviteTargetText; private set { _inviteTargetText = value; OnPropertyChanged(); } }
        public string InviteDate { get => _inviteDate; set { _inviteDate = value; OnPropertyChanged(); } }
        public string InviteTime { get => _inviteTime; set { _inviteTime = value; OnPropertyChanged(); } }

        public ICommand RefreshCommand { get; }
        public ICommand OpenInviteCommand { get; }
        public ICommand CancelInviteCommand { get; }
        public ICommand ConfirmInviteCommand { get; }

        public ClientMatchesViewModel(
            Client currentClient,
            IMatchService matches,
            IMeetingService meetings,
            IDialogService dialogs,
            Action? onChanged = null)
        {
            _client = currentClient;
            _matches = matches;
            _meetings = meetings;
            _dialogs = dialogs;
            _onChanged = onChanged;

            RefreshCommand = new RelayCommand(_ => Refresh());
            OpenInviteCommand = new RelayCommand(p => { if (p is MatchDisplayItem item) OpenInvite(item.Candidate); });
            CancelInviteCommand = new RelayCommand(_ => CloseInvite());
            ConfirmInviteCommand = new RelayCommand(_ => SendInvite());

            Refresh();
        }

        public void Refresh()
        {
            List<Client> found = _matches.Find(_client, MutualOnly, string.Empty);

            Matches.Clear();
            foreach (Client candidate in found)
            {
                Matches.Add(new MatchDisplayItem
                {
                    Candidate = candidate,
                    FullName = candidate.FullName ?? string.Empty,
                    AgeText = $"{candidate.Age} років",
                    GenderText = candidate.Gender.ToString(),
                    DetailsText = string.IsNullOrWhiteSpace(candidate.PersonalDetails)
                        ? "Опис профілю ще не заповнено."
                        : candidate.PersonalDetails,
                    AvatarPath = candidate.AvatarPath,
                    HeightText = candidate.HeightCm.HasValue ? $"📏 {candidate.HeightCm.Value} см" : string.Empty,
                    WeightText = candidate.WeightKg.HasValue ? $"⚖️ {candidate.WeightKg.Value:0.#} кг" : string.Empty,
                    HeightVisibility = candidate.HeightCm.HasValue ? Visibility.Visible : Visibility.Collapsed,
                    WeightVisibility = candidate.WeightKg.HasValue ? Visibility.Visible : Visibility.Collapsed,
                    Hobbies = candidate.Hobbies != null ? new List<string>(candidate.Hobbies) : new List<string>(),
                    MutualBadgeVisibility = _matches.IsMatch(candidate, _client)
                        ? Visibility.Visible
                        : Visibility.Collapsed
                });
            }

            SubtitleText = $"Підібрано анкет, з якими ви б хотіли познайомитись: {Matches.Count}";
            EmptyMatchesVisibility = Matches.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void OpenInvite(Client candidate)
        {
            _picked = candidate;
            InviteTargetText = $"Партнер: {candidate.FullName} ({candidate.Age} років)";
            InviteDate = DateTime.Now.AddDays(3).ToString("dd.MM.yyyy");
            InviteTime = "18:00";
            InviteOverlayVisibility = Visibility.Visible;
        }

        private void CloseInvite()
        {
            _picked = null;
            InviteOverlayVisibility = Visibility.Collapsed;
        }

        private void SendInvite()
        {
            if (_picked == null) return;

            string raw = $"{InviteDate.Trim()} {InviteTime.Trim()}";
            if (!DateTime.TryParseExact(raw, "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
            {
                _dialogs.Warn("Введіть коректну дату (дд.мм.рррр) та час (гг:хх).", "Помилка");
                return;
            }

            if (date <= DateTime.Now)
            {
                _dialogs.Warn("Дата та час зустрічі мають бути у майбутньому.", "Помилка");
                return;
            }

            _meetings.Create(_client, _picked, date);
            _onChanged?.Invoke();

            _dialogs.Info($"Запрошення для {_picked.FullName} надіслано!", "Успіх");
            CloseInvite();
        }
    }
}