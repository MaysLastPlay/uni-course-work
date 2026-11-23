using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CourseApp.classes;
using CourseApp.classes.information;
using CourseApp.classes.services.CourseApp.classes;
using CourseApp.classes.enums;

namespace CourseApp.windows.authentication
{
    public partial class RegisterWindow : Window
    {
        private readonly DatingService _datingService;

        public RegisterWindow(DatingService datingService)
        {
            InitializeComponent();
            _datingService = datingService ?? throw new ArgumentNullException(nameof(datingService));
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn != null) btn.IsEnabled = false;

            try
            {
                var requiredFields = new[]
                {
            (Field: RegNumberTextBox, Error: "введіть реєстраційний номер"),
            (Field: FullNameTextBox, Error: "введіть ПІБ клієнта")
        };

                var failedField = requiredFields.FirstOrDefault(f => string.IsNullOrWhiteSpace(f.Field.Text));
                if (failedField.Field != null)
                {
                    ShowWarning($"Будь ласка, {failedField.Error}.");
                    failedField.Field.Focus();
                    return;
                }

                string email = EmailTextBox.Text.Trim();
                if (string.IsNullOrWhiteSpace(email) || !email.Contains("@") || !email.Contains("."))
                {
                    ShowWarning("Будь ласка, введіть коректний Email.");
                    EmailTextBox.Focus();
                    return;
                }

                string password = PasswordBoxField.Password;
                if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
                {
                    ShowWarning("Будь ласка, введіть пароль (мінімум 8 символів).");
                    PasswordBoxField.Focus();
                    return;
                }

                if (!int.TryParse(AgeTextBox.Text, out int age) || age < 18)
                {
                    ShowWarning("Будь ласка, введіть коректний числовий вік (від 18 років).");
                    AgeTextBox.Focus();
                    return;
                }

                int minAge = int.TryParse(MinAgeTextBox.Text, out int min) ? min : 18;
                int maxAge = int.TryParse(MaxAgeTextBox.Text, out int max) ? max : 100;

                if (minAge > maxAge)
                {
                    ShowWarning("Мінімальний вік партнера не може перевищувати максимальний.");
                    MinAgeTextBox.Focus();
                    return;
                }

                var newClient = new Client
                {
                    RegNumber = RegNumberTextBox.Text.Trim(),
                    FullName = FullNameTextBox.Text.Trim(),
                    Email = email,
                    Password = password,
                    Gender = (Gender)GenderComboBox.SelectedIndex,
                    Age = age,
                    PersonalDetails = PersonalDetailsTextBox.Text.Trim(),
                    Status = ClientStatus.Active,
                    Requirements = new PRequirements
                    {
                        PreferredGender = (Gender)PrefGenderComboBox.SelectedIndex,
                        MinAge = minAge,
                        MaxAge = maxAge,
                        CriteriaDescription = RequirementsTextBox.Text.Trim()
                    }
                };

                string registrationResult = _datingService.RegisterNewClient(newClient);

                if (registrationResult != "Success")
                {
                    ShowWarning(registrationResult);
                    return;
                }

                MessageBox.Show($"Клієнт {newClient.FullName} успішно зареєстрований!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка:\n{ex.Message}", "Системна помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (btn != null) btn.IsEnabled = true;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void ShowWarning(string message) =>
            MessageBox.Show(message, "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
    }
}