using System;
using System.Collections.ObjectModel;
using System.Windows;
using CourseApp.classes.enums;
using CourseApp.classes.information;
using CourseApp.classes.services;

namespace CourseApp.classes.mvvm.viewmodels
{
    public class ClientProfilesViewModel : ObservableObject
    {
        private readonly IClientService _clients;

        private string _query = string.Empty;
        private int _genderIndex;
        private int _statusIndex;
        private string _countText = "Знайдено: 0";
        private Visibility _emptyVisibility = Visibility.Collapsed;
        private Visibility _listVisibility = Visibility.Visible;

        public ObservableCollection<Client> Clients { get; } = new ObservableCollection<Client>();

        public string Query { get => _query; set { _query = value; OnPropertyChanged(); Filter(); } }
        public int GenderIndex { get => _genderIndex; set { _genderIndex = value; OnPropertyChanged(); Filter(); } }
        public int StatusIndex { get => _statusIndex; set { _statusIndex = value; OnPropertyChanged(); Filter(); } }

        public string CountText { get => _countText; private set { _countText = value; OnPropertyChanged(); } }
        public Visibility EmptyVisibility { get => _emptyVisibility; private set { _emptyVisibility = value; OnPropertyChanged(); } }
        public Visibility ListVisibility { get => _listVisibility; private set { _listVisibility = value; OnPropertyChanged(); } }

        public ClientProfilesViewModel(IClientService clients)
        {
            _clients = clients;
            Filter();
        }

        public void Filter()
        {
            string q = Query?.Trim() ?? string.Empty;
            Clients.Clear();

            foreach (Client c in _clients.All) AddIfMatch(c, q);
            foreach (Client c in _clients.Deleted) AddIfMatch(c, q);

            CountText = $"Знайдено: {Clients.Count}";
            EmptyVisibility = Clients.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            ListVisibility = Clients.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void AddIfMatch(Client c, string q)
        {
            bool textOk = string.IsNullOrEmpty(q) ||
                (c.FullName != null && c.FullName.Contains(q, StringComparison.OrdinalIgnoreCase)) ||
                (c.RegNumber != null && c.RegNumber.Contains(q, StringComparison.OrdinalIgnoreCase));

            if (!textOk) return;
            if (GenderIndex == 1 && c.Gender != Gender.Чоловіча) return;
            if (GenderIndex == 2 && c.Gender != Gender.Жіноча) return;

            if (StatusIndex == 1 && c.Status != ClientStatus.Active) return;
            if (StatusIndex == 2 && c.Status != ClientStatus.Archived) return;
            if (StatusIndex == 3 && c.Status != ClientStatus.Deleted) return;

            Clients.Add(c);
        }
    }
}