using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CourseApp.classes.enums;
using CourseApp.classes.information;
using CourseApp.classes.services;

namespace CourseApp.classes.mvvm.viewmodels
{
    public class RegisterViewModel : ObservableObject
    {
        private readonly IClientService _clients;
        private readonly IDialogService _dialogs;
        private readonly Window _window;

        private int _step = 1;
        private readonly string _regNumber;
        private string _fullName = string.Empty;
        private int _genderIndex;
        private string _age = string.Empty;
        private string _email = string.Empty;

        private string _regLabel = string.Empty;
        private string _stepTitle = "Крок 1 з 2: Персональні дані";
        private string _stepDots = "● ○";
        private string _backText = "Скасувати";
        private string _nextText = "Далі →";
        private Visibility _step1Visibility = Visibility.Visible;
        private Visibility _step2Visibility = Visibility.Collapsed;

        public string FullName { get => _fullName; set => SetProperty(ref _fullName, value); }
        public int GenderIndex { get => _genderIndex; set => SetProperty(ref _genderIndex, value); }
        public string Age { get => _age; set => SetProperty(ref _age, value); }
        public string Email { get => _email; set => SetProperty(ref _email, value); }

        public string RegLabel { get => _regLabel; private set => SetProperty(ref _regLabel, value); }
        public string StepTitle { get => _stepTitle; private set => SetProperty(ref _stepTitle, value); }
        public string StepDots { get => _stepDots; private set => SetProperty(ref _stepDots, value); }
        public string BackText { get => _backText; private set => SetProperty(ref _backText, value); }
        public string NextText { get => _nextText; private set => SetProperty(ref _nextText, value); }
        public Visibility Step1Visibility { get => _step1Visibility; private set => SetProperty(ref _step1Visibility, value); }
        public Visibility Step2Visibility { get => _step2Visibility; private set => SetProperty(ref _step2Visibility, value); }

        public ICommand NextCommand { get; }
        public ICommand BackCommand { get; }

        public RegisterViewModel(IClientService clients, IDialogService dialogs, Window window)
        {
            _clients = clients;
            _dialogs = dialogs;
            _window = window;

            _regNumber = $"REG-{DateTime.Now:yyyy}-{Random.Shared.Next(1000, 9999)}";
            RegLabel = $"Реєстраційний номер: {_regNumber}";

            NextCommand = new RelayCommand(p => Next(p as PasswordBox));
            BackCommand = new RelayCommand(_ => Back());

            SyncStep();
        }

        private void Next(PasswordBox? passBox)
        {
            if (!Validate(passBox)) return;

            if (_step < 2)
            {
                _step++;
                SyncStep();
            }
            else
            {
                Submit(passBox?.Password ?? string.Empty);
            }
        }

        private void Back()
        {
            if (_step > 1)
            {
                _step--;
                SyncStep();
            }
            else
            {
                _window.DialogResult = false;
                _window.Close();
            }
        }

        private void SyncStep()
        {
            Step1Visibility = _step == 1 ? Visibility.Visible : Visibility.Collapsed;
            Step2Visibility = _step == 2 ? Visibility.Visible : Visibility.Collapsed;
            BackText = _step == 1 ? "Скасувати" : "← Назад";
            NextText = _step == 2 ? "Зареєструвати ✨" : "Далі →";
            StepTitle = _step == 1 ? "Крок 1 з 2: Персональні дані" : "Крок 2 з 2: Обліковий запис";
            StepDots = _step == 1 ? "● ○" : "● ●";
        }

        private bool Validate(PasswordBox? passBox)
        {
            if (_step == 1)
            {
                if (string.IsNullOrWhiteSpace(FullName))
                {
                    _dialogs.Warn("Будь ласка, введіть ПІБ клієнта.", "Увага");
                    return false;
                }
                if (!int.TryParse(Age, out int age) || age < 18)
                {
                    _dialogs.Warn("Будь ласка, введіть коректний вік (від 18 років).", "Увага");
                    return false;
                }
            }
            else
            {
                string email = Email.Trim();
                if (string.IsNullOrWhiteSpace(email) || !email.Contains('@') || !email.Contains('.'))
                {
                    _dialogs.Warn("Будь ласка, введіть коректний Email.", "Увага");
                    return false;
                }
                string pass = passBox?.Password ?? string.Empty;
                if (string.IsNullOrWhiteSpace(pass) || pass.Length < 8)
                {
                    _dialogs.Warn("Пароль повинен містити щонайменше 8 символів.", "Увага");
                    return false;
                }
            }

            return true;
        }

        private void Submit(string password)
        {
            try
            {
                Gender gender = (Gender)GenderIndex;
                Client client = new Client
                {
                    RegNumber = _regNumber,
                    FullName = FullName.Trim(),
                    Email = Email.Trim(),
                    Password = password,
                    Gender = gender,
                    Age = int.Parse(Age),
                    PersonalDetails = string.Empty,
                    Status = ClientStatus.Active,
                    Requirements = new PRequirements
                    {
                        PreferredGender = gender == Gender.Чоловіча ? Gender.Жіноча : Gender.Чоловіча,
                        MinAge = 18,
                        MaxAge = 99
                    }
                };

                string result = _clients.Register(client);
                if (result != "Success")
                {
                    _dialogs.Warn(result, "Увага");
                    return;
                }

                _dialogs.Info($"Клієнт {client.FullName} успішно зареєстрований!\nРеєстраційний номер: {_regNumber}", "Успіх");
                _window.DialogResult = true;
                _window.Close();
            }
            catch (Exception ex)
            {
                _dialogs.Error($"Помилка реєстрації:\n{ex.Message}", "Помилка");
            }
        }
    }
}