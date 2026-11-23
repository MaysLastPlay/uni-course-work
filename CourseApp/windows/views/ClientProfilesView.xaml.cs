using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CourseApp.classes;
using CourseApp.classes.services.CourseApp.classes;
using CourseApp.classes.enums;

namespace CourseApp.windows.views
{
    public partial class ClientProfilesView : UserControl
    {
        private readonly DatingService _datingService;

        public ClientProfilesView(DatingService datingService)
        {
            InitializeComponent();
            _datingService = datingService;
            RefreshUI();
        }

        public void RefreshUI() => ApplyFilter();

        private void Filter_Changed(object sender, SelectionChangedEventArgs e) => ApplyFilter();
        private void Filter_Changed(object sender, TextChangedEventArgs e) => ApplyFilter();

        private void ApplyFilter()
        {
            if (_datingService == null || AllClientsListBox == null) return;

            string searchText = SearchTextBox.Text?.Trim() ?? string.Empty;
            int genderFilter = GenderFilterComboBox.SelectedIndex;
            int statusFilter = StatusFilterComboBox.SelectedIndex;

            var filtered = _datingService.AllClients.Where(c =>
            {
                bool matchesSearch = string.IsNullOrEmpty(searchText) ||
                    (c.FullName?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (c.RegNumber?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false);

                bool matchesGender = genderFilter == 0 ||
                    (genderFilter == 1 && c.Gender == Gender.Чоловіча) ||
                    (genderFilter == 2 && c.Gender == Gender.Жіноча);

                bool matchesStatus = statusFilter == 0 ||
                    (statusFilter == 1 && c.Status == ClientStatus.Active) ||
                    (statusFilter == 2 && c.Status == ClientStatus.Archived) ||
                    (statusFilter == 3 && c.Status == ClientStatus.Deleted);

                return matchesSearch && matchesGender && matchesStatus;
            }).ToList();

            AllClientsListBox.ItemsSource = filtered;
            TxtClientsCount.Text = $"Знайдено: {filtered.Count}";

            EmptyStatePanel.Visibility = filtered.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            AllClientsListBox.Visibility = filtered.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}