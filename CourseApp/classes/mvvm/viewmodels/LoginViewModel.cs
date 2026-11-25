using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CourseApp.classes.information;
using CourseApp.classes.services;
using CourseApp.windows.authentication;
using CourseApp.windows.mainmenu;

namespace CourseApp.classes.mvvm.viewmodels
{
    public class LoginViewModel : ObservableObject
    {
        private const string AdminLogin = "admin";
        private const string AdminPass = "admin123";

        private readonly AppServices _app;
        private readonly IClientService _clients;
        private readonly Window _window;

        private string _login = string.Empty;
        private string _error = string.Empty;
        private Visibility _errorVisibility = Visibility.Collapsed;

        public string Login { get => _login; set => SetProperty(ref _login, value); }
        public string Error { get => _error; private set => SetProperty(ref _error, value); }
        public Visibility ErrorVisibility { get => _errorVisibility; private set => SetProperty(ref _errorVisibility, value); }

        public ICommand LoginCommand { get; }
        public ICommand RegisterCommand { get; }

        public LoginViewModel(AppServices app, Window window)
        {
            _app = app;
            _clients = app.Clients;
            _window = window;

            LoginCommand = new RelayCommand(p => SignIn(p as PasswordBox));
            RegisterCommand = new RelayCommand(_ => OpenRegister());
        }

        public void OnLoaded()
        {
            if (_clients.All.Count == 0 && _clients.Deleted.Count == 0)
            {
                _window.Hide();
                RegisterWindow reg = new RegisterWindow(_app) { Owner = _window };
                if (reg.ShowDialog() == true)
                    _window.Show();
                else
                    Application.Current.Shutdown();
            }
        }

        private void SignIn(PasswordBox? passBox)
        {
            string id = Login.Trim();
            string pass = passBox?.Password ?? string.Empty;

            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(pass))
            {
                SetError("Введіть логін/Email/реєстраційний номер та пароль.");
                return;
            }

            if (id.Equals(AdminLogin, StringComparison.OrdinalIgnoreCase) && pass == AdminPass)
            {
                ErrorVisibility = Visibility.Collapsed;
                new AdminWindow(_app).Show();
                _window.Close();
                return;
            }

            Client? client = _clients.FindByLogin(id, pass);
            if (client == null)
            {
                SetError("Невірні дані для входу або акаунт неактивний.");
                return;
            }

            ErrorVisibility = Visibility.Collapsed;
            new ClientWindow(client, _app).Show();
            _window.Close();
        }

        private void OpenRegister()
        {
            ErrorVisibility = Visibility.Collapsed;
            new RegisterWindow(_app) { Owner = _window }.ShowDialog();
        }

        private void SetError(string msg)
        {
            Error = msg;
            ErrorVisibility = Visibility.Visible;
        }
    }
}