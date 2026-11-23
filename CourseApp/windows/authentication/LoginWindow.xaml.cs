using System;
using System.Linq;
using System.Windows;
using CourseApp.classes;
using CourseApp.classes.information;
using CourseApp.classes.services.CourseApp.classes;
using CourseApp.windows.mainmenu;
using CourseApp.classes.enums;

namespace CourseApp.windows.authentication
{
    public partial class LoginWindow : Window
    {
        private const string AdminLogin = "admin";
        private const string AdminPassword = "admin123";

        private readonly DatingService _datingService;

        public LoginWindow()
        {
            InitializeComponent();
            _datingService = new DatingService();
        }

        public LoginWindow(DatingService datingService)
        {
            InitializeComponent();
            _datingService = datingService;
        }

        private void LoginWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (_datingService.AllClients.Count == 0 && _datingService.DeletedClients.Count == 0)
            {
                Hide();
                var registerWindow = new RegisterWindow(_datingService) { Owner = this };
                if (registerWindow.ShowDialog() == true)
                {
                    Show();
                }
                else
                {
                    Application.Current.Shutdown();
                }
            }
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string identifier = TxtLogin.Text.Trim();
            string password = TxtPassword.Password;

            if (string.IsNullOrWhiteSpace(identifier) || string.IsNullOrWhiteSpace(password))
            {
                ShowError("Введіть логін/Email/реєстраційний номер та пароль.");
                return;
            }

            if (identifier.Equals(AdminLogin, StringComparison.OrdinalIgnoreCase) && password == AdminPassword)
            {
                TxtError.Visibility = Visibility.Collapsed;
                new AdminWindow(_datingService).Show();
                Close();
                return;
            }

            Client? client = _datingService.AllClients.FirstOrDefault(c =>
                c.Status == ClientStatus.Active &&
                ((!string.IsNullOrWhiteSpace(c.Email) && c.Email.Equals(identifier, StringComparison.OrdinalIgnoreCase)) ||
                 (!string.IsNullOrWhiteSpace(c.RegNumber) && c.RegNumber.Equals(identifier, StringComparison.OrdinalIgnoreCase))));

            if (client == null || client.Password != password)
            {
                ShowError("Невірні дані для входу або акаунт неактивний.");
                return;
            }

            TxtError.Visibility = Visibility.Collapsed;
            new ClientWindow(client, _datingService).Show();
            Close();
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            TxtError.Visibility = Visibility.Collapsed;
            new RegisterWindow(_datingService) { Owner = this }.ShowDialog();
        }

        private void ShowError(string message)
        {
            TxtError.Text = message;
            TxtError.Visibility = Visibility.Visible;
        }
    }
}