using System;
using System.Windows;
using System.Windows.Controls;
using CourseApp.classes.enums;
using CourseApp.classes.information;
using CourseApp.classes.services.CourseApp.classes;

namespace CourseApp.windows.views
{
    public partial class ProfileMenuView : UserControl
    {
        private readonly DatingService _datingService;
        private readonly Action? _onProfileChanged;
        private Client? _currentClient;

        public ProfileMenuView(Client currentClient, DatingService datingService, Action? onProfileChanged = null)
        {
            InitializeComponent();
            _datingService = datingService;
            _onProfileChanged = onProfileChanged;

            LoadClientProfile(currentClient);
        }

        public ProfileMenuView(DatingService datingService, Action? onProfileChanged = null)
        {
            InitializeComponent();
            _datingService = datingService;
            _onProfileChanged = onProfileChanged;

            for (int i = 0; i < _datingService.AllClients.Count; i++)
            {
                if (_datingService.AllClients[i].Status == ClientStatus.Active)
                {
                    LoadClientProfile(_datingService.AllClients[i]);
                    break;
                }
            }
        }

        public void LoadClientProfile(Client client)
        {
            _currentClient = client;
            if (_currentClient == null) return;

            FullNameBox.Text = _currentClient.FullName ?? "";
            AgeBox.Text = _currentClient.Age.ToString();
            PersonalDetailsBox.Text = _currentClient.PersonalDetails ?? "";

            CurrentPasswordBox.Password = "";
            NewPasswordBox.Password = "";
            ConfirmPasswordBox.Password = "";

            if (_currentClient.Requirements != null)
            {
                int genderIndex = (int)_currentClient.Requirements.PreferredGender;
                PrefGenderComboBox.SelectedIndex = genderIndex >= 0 && genderIndex < PrefGenderComboBox.Items.Count
                    ? genderIndex
                    : 0;

                MinAgeBox.Text = _currentClient.Requirements.MinAge.ToString();
                MaxAgeBox.Text = _currentClient.Requirements.MaxAge.ToString();
                CriteriaBox.Text = _currentClient.Requirements.CriteriaDescription ?? "";
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentClient == null) return;

            string fullName = FullNameBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(fullName))
            {
                MessageBox.Show("Введіть ПІБ клієнта.", "Помилка валідації", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(AgeBox.Text.Trim(), out int age) || age < 18)
            {
                MessageBox.Show("Вік клієнта має бути числом від 18 років.", "Помилка валідації", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(MinAgeBox.Text.Trim(), out int minAge) || minAge < 18)
            {
                MessageBox.Show("Мінімальний вік партнера має бути від 18 років.", "Помилка валідації", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(MaxAgeBox.Text.Trim(), out int maxAge) || maxAge < minAge)
            {
                MessageBox.Show("Максимальний вік не може бути меншим за мінімальний.", "Помилка валідації", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _currentClient.FullName = fullName;
            _currentClient.Age = age;
            _currentClient.PersonalDetails = PersonalDetailsBox.Text.Trim();

            if (_currentClient.Requirements != null)
            {
                if (PrefGenderComboBox.SelectedIndex >= 0)
                {
                    _currentClient.Requirements.PreferredGender = (Gender)PrefGenderComboBox.SelectedIndex;
                }

                _currentClient.Requirements.MinAge = minAge;
                _currentClient.Requirements.MaxAge = maxAge;
                _currentClient.Requirements.CriteriaDescription = CriteriaBox.Text.Trim();
            }

            _datingService.SaveClients();
            _onProfileChanged?.Invoke();

            MessageBox.Show("Зміни профілю успішно збережено!", "Збережено", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentClient == null) return;

            string currentPassword = CurrentPasswordBox.Password;
            string newPassword = NewPasswordBox.Password;
            string confirmPassword = ConfirmPasswordBox.Password;

            if (currentPassword != (_currentClient.Password ?? ""))
            {
                MessageBox.Show("Поточний пароль введено неправильно.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 4)
            {
                MessageBox.Show("Новий пароль має містити щонайменше 4 символи.", "Помилка валідації", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (newPassword != confirmPassword)
            {
                MessageBox.Show("Новий пароль та підтвердження не збігаються.", "Помилка валідації", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (newPassword == currentPassword)
            {
                MessageBox.Show("Новий пароль має відрізнятися від поточного.", "Помилка валідації", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _currentClient.Password = newPassword;
            _datingService.SaveClients();

            CurrentPasswordBox.Password = "";
            NewPasswordBox.Password = "";
            ConfirmPasswordBox.Password = "";

            MessageBox.Show("Пароль успішно змінено!", "Безпека", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void DeactivateButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentClient == null || string.IsNullOrWhiteSpace(_currentClient.RegNumber)) return;

            MessageBoxResult confirm = MessageBox.Show(
                "Ви дійсно бажаєте деактивувати свій акаунт? Ви вийдете з системи, а профіль переміститься до списку вилучених.",
                "Деактивація акаунту",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes) return;

            _datingService.DeleteClient(_currentClient.RegNumber, "Деактивація профілю клієнтом");
            _onProfileChanged?.Invoke();
        }
    }
}