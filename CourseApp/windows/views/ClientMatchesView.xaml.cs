using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using CourseApp.classes.information;
using CourseApp.classes.services.CourseApp.classes;

namespace CourseApp.windows.views
{
    public partial class ClientMatchesView : UserControl
    {
        private readonly Client _currentClient;
        private readonly DatingService _datingService;
        private readonly Action? _onDataChanged;
        private Client? _selectedCandidate;

        public ClientMatchesView(Client currentClient, DatingService datingService, Action? onDataChanged = null)
        {
            InitializeComponent();
            _currentClient = currentClient;
            _datingService = datingService;
            _onDataChanged = onDataChanged;

            RefreshUI();
        }

        public void RefreshUI()
        {
            bool mutualOnly = MutualOnlyCheck.IsChecked == true;
            string keyword = SearchBox.Text;

            List<Client> matches = _datingService.FindMatchesForClient(_currentClient, mutualOnly, keyword);
            List<MatchDisplayItem> displayItems = new List<MatchDisplayItem>();

            foreach (Client candidate in matches)
            {
                bool isMutual = _datingService.MatchesPreferences(candidate, _currentClient);
                string details = string.IsNullOrWhiteSpace(candidate.PersonalDetails)
                    ? "Опис профілю ще не заповнено."
                    : candidate.PersonalDetails;

                displayItems.Add(new MatchDisplayItem
                {
                    Candidate = candidate,
                    FullName = candidate.FullName ?? "",
                    AgeText = $"{candidate.Age} років",
                    GenderText = candidate.Gender.ToString(),
                    DetailsText = details,
                    MutualBadgeVisibility = isMutual ? Visibility.Visible : Visibility.Collapsed
                });
            }

            MatchesListBox.ItemsSource = displayItems;
            SubtitleText.Text = $"Знайдено кандидатів за вашими критеріями: {displayItems.Count}";
            EmptyMatchesPanel.Visibility = displayItems.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Filter_Changed(object sender, RoutedEventArgs e)
        {
            if (IsLoaded) RefreshUI();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshUI();
        }

        private void InviteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button { DataContext: MatchDisplayItem item }) return;

            _selectedCandidate = item.Candidate;
            InviteTargetText.Text = $"Партнер: {_selectedCandidate.FullName} ({_selectedCandidate.Age} років)";
            InviteDateBox.Text = DateTime.Now.AddDays(3).ToString("dd.MM.yyyy");
            InviteTimeBox.Text = "18:00";
            InviteOverlay.Visibility = Visibility.Visible;
        }

        private void CancelInvite_Click(object sender, RoutedEventArgs e)
        {
            _selectedCandidate = null;
            InviteOverlay.Visibility = Visibility.Collapsed;
        }

        private void ConfirmInvite_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedCandidate == null) return;

            string rawDateTime = $"{InviteDateBox.Text.Trim()} {InviteTimeBox.Text.Trim()}";
            DateTime scheduledDate;
            if (!DateTime.TryParseExact(rawDateTime, "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out scheduledDate))
            {
                MessageBox.Show("Введіть коректну дату (дд.мм.рррр) та час (гг:хх).", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (scheduledDate <= DateTime.Now)
            {
                MessageBox.Show("Дата та час зустрічі мають бути у майбутньому.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _datingService.CreateMeetingInvitation(_currentClient, _selectedCandidate, scheduledDate);
            InviteOverlay.Visibility = Visibility.Collapsed;
            _onDataChanged?.Invoke();

            MessageBox.Show($"Запрошення для {_selectedCandidate.FullName} надіслано!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
            _selectedCandidate = null;
        }

        private class MatchDisplayItem
        {
            public Client Candidate { get; set; } = null!;
            public string FullName { get; set; } = "";
            public string AgeText { get; set; } = "";
            public string GenderText { get; set; } = "";
            public string DetailsText { get; set; } = "";
            public Visibility MutualBadgeVisibility { get; set; }
        }
    }
}