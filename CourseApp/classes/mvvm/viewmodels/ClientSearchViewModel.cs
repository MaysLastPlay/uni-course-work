using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using CourseApp.classes.enums;
using CourseApp.classes.information;
using CourseApp.classes.services;

namespace CourseApp.classes.mvvm.viewmodels
{
    public sealed class HobbyOption : ObservableObject
    {
        private bool _isSelected;
        private readonly Action _onToggle;

        public string Title { get; }

        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); _onToggle(); }
        }

        public HobbyOption(string title, Action onToggle)
        {
            Title = title;
            _onToggle = onToggle;
        }
    }

    public class ClientSearchViewModel : ObservableObject
    {
        public static readonly string[] DefaultHobbies =
        {
            "Малювання", "Програмування", "Захоплення музикою", "Фотографія",
            "Подорожі", "Читання книг", "Кулінарія", "Спорт та фітнес",
            "Танці", "Кіно та серіали", "Настільні ігри", "Відеоігри",
            "Садівництво", "Йога та медитація", "Рукоділля", "Гра на інструментах",
            "Волонтерство", "Велоспорт", "Вивчення мов", "Кемпінг та природа"
        };

        private readonly Client _client;
        private readonly IClientService _clients;
        private readonly List<HobbyOption> _allHobbies = new List<HobbyOption>();

        private string _query = string.Empty;
        private string _minHeight = string.Empty;
        private string _maxHeight = string.Empty;
        private string _minAge = string.Empty;
        private string _maxAge = string.Empty;
        private string _maxWeight = string.Empty;
        private string _hobbyQuery = string.Empty;
        private string _hobbiesSummary = "🎯 Фільтр хобі (20)...";
        private string _hobbiesCount = "0";
        private string _resultsCount = "Знайдено: 0";
        private Visibility _emptyVisibility = Visibility.Collapsed;

        public ObservableCollection<Client> Results { get; } = new ObservableCollection<Client>();
        public ObservableCollection<HobbyOption> Hobbies { get; } = new ObservableCollection<HobbyOption>();

        public string Query { get => _query; set { _query = value; OnPropertyChanged(); Filter(); } }
        public string MinHeight { get => _minHeight; set { _minHeight = value; OnPropertyChanged(); Filter(); } }
        public string MaxHeight { get => _maxHeight; set { _maxHeight = value; OnPropertyChanged(); Filter(); } }
        public string MinAge { get => _minAge; set { _minAge = value; OnPropertyChanged(); Filter(); } }
        public string MaxAge { get => _maxAge; set { _maxAge = value; OnPropertyChanged(); Filter(); } }
        public string MaxWeight { get => _maxWeight; set { _maxWeight = value; OnPropertyChanged(); Filter(); } }
        public string HobbyQuery { get => _hobbyQuery; set { _hobbyQuery = value; OnPropertyChanged(); UpdateHobbies(); } }

        public string HobbiesSummary { get => _hobbiesSummary; private set { _hobbiesSummary = value; OnPropertyChanged(); } }
        public string HobbiesCount { get => _hobbiesCount; private set { _hobbiesCount = value; OnPropertyChanged(); } }
        public string ResultsCount { get => _resultsCount; private set { _resultsCount = value; OnPropertyChanged(); } }
        public Visibility EmptyVisibility { get => _emptyVisibility; private set { _emptyVisibility = value; OnPropertyChanged(); } }

        public ICommand ResetCommand { get; }

        public ClientSearchViewModel(Client currentClient, IClientService clients)
        {
            _client = currentClient;
            _clients = clients;

            foreach (string h in DefaultHobbies)
                _allHobbies.Add(new HobbyOption(h, OnHobbyChanged));

            ResetCommand = new RelayCommand(_ => Reset());

            UpdateHobbies();
            UpdateSummary();
            Filter();
        }

        private void OnHobbyChanged()
        {
            UpdateSummary();
            Filter();
        }

        private void UpdateHobbies()
        {
            string q = HobbyQuery?.Trim() ?? string.Empty;
            Hobbies.Clear();

            foreach (HobbyOption opt in _allHobbies)
            {
                if (string.IsNullOrEmpty(q) || opt.Title.Contains(q, StringComparison.OrdinalIgnoreCase))
                    Hobbies.Add(opt);
            }
        }

        private void UpdateSummary()
        {
            List<string> picked = GetPickedHobbies();
            HobbiesCount = picked.Count.ToString();
            HobbiesSummary = picked.Count > 0 ? string.Join(", ", picked) : "🎯 Фільтр хобі (20)...";
        }

        private List<string> GetPickedHobbies()
        {
            List<string> picked = new List<string>();
            foreach (HobbyOption opt in _allHobbies)
            {
                if (opt.IsSelected) picked.Add(opt.Title);
            }
            return picked;
        }

        public void Filter()
        {
            string q = Query?.Trim() ?? string.Empty;
            int.TryParse(MinHeight?.Trim(), out int minH);
            int.TryParse(MaxHeight?.Trim(), out int maxH);
            int.TryParse(MinAge?.Trim(), out int minA);
            int.TryParse(MaxAge?.Trim(), out int maxA);

            string wRaw = (MaxWeight ?? string.Empty).Trim().Replace(',', '.');
            double.TryParse(wRaw, NumberStyles.Float, CultureInfo.InvariantCulture, out double maxW);

            List<string> pickedHobbies = GetPickedHobbies();
            Results.Clear();

            foreach (Client c in _clients.All)
            {
                if (c.RegNumber == _client.RegNumber || c.Status != ClientStatus.Active) continue;
                if (!MatchesText(c, q)) continue;
                if (minH > 0 && (!c.HeightCm.HasValue || c.HeightCm.Value < minH)) continue;
                if (maxH > 0 && (!c.HeightCm.HasValue || c.HeightCm.Value > maxH)) continue;
                if (minA > 0 && c.Age < minA) continue;
                if (maxA > 0 && c.Age > maxA) continue;
                if (maxW > 0 && (!c.WeightKg.HasValue || c.WeightKg.Value > maxW)) continue;
                if (!HasAllHobbies(c, pickedHobbies)) continue;

                Results.Add(c);
            }

            ResultsCount = $"Знайдено: {Results.Count}";
            EmptyVisibility = Results.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private static bool MatchesText(Client c, string q)
        {
            if (string.IsNullOrEmpty(q)) return true;
            if (c.FullName != null && c.FullName.Contains(q, StringComparison.OrdinalIgnoreCase)) return true;
            if (c.PersonalDetails != null && c.PersonalDetails.Contains(q, StringComparison.OrdinalIgnoreCase)) return true;

            if (c.Hobbies != null)
            {
                foreach (string h in c.Hobbies)
                {
                    if (h.Contains(q, StringComparison.OrdinalIgnoreCase)) return true;
                }
            }

            return false;
        }

        private static bool HasAllHobbies(Client c, List<string> required)
        {
            if (required.Count == 0) return true;
            if (c.Hobbies == null) return false;

            foreach (string need in required)
            {
                bool found = false;
                foreach (string actual in c.Hobbies)
                {
                    if (string.Equals(actual, need, StringComparison.OrdinalIgnoreCase))
                    {
                        found = true;
                        break;
                    }
                }
                if (!found) return false;
            }

            return true;
        }

        private void Reset()
        {
            _query = _minHeight = _maxHeight = _minAge = _maxAge = _maxWeight = _hobbyQuery = string.Empty;

            OnPropertyChanged(nameof(Query));
            OnPropertyChanged(nameof(MinHeight));
            OnPropertyChanged(nameof(MaxHeight));
            OnPropertyChanged(nameof(MinAge));
            OnPropertyChanged(nameof(MaxAge));
            OnPropertyChanged(nameof(MaxWeight));
            OnPropertyChanged(nameof(HobbyQuery));

            foreach (HobbyOption opt in _allHobbies)
                opt.IsSelected = false;

            UpdateHobbies();
            UpdateSummary();
            Filter();
        }
    }
}