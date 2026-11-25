using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CourseApp.classes.information;
using CourseApp.classes.mvvm.viewmodels;
using CourseApp.classes.services;

namespace CourseApp.windows.views
{
    public partial class ProfileMenuView : UserControl
    {
        private readonly ProfileMenuViewModel _vm;

        public ProfileMenuView(Client currentClient, IClientService clients, IDialogService dialogs, Action? onChanged = null)
        {
            InitializeComponent();
            _vm = new ProfileMenuViewModel(clients, dialogs, onChanged, () => Window.GetWindow(this), currentClient);
            DataContext = _vm;
        }

        public ProfileMenuView(IClientService clients, IDialogService dialogs, Action? onChanged = null)
        {
            InitializeComponent();
            _vm = new ProfileMenuViewModel(clients, dialogs, onChanged, () => Window.GetWindow(this));
            DataContext = _vm;
        }

        public void LoadClientProfile(Client client) => _vm.LoadProfile(client);

        internal sealed class ChangePasswordDialog : Window
        {
            private readonly Client _client;
            private readonly IClientService _clients;
            private readonly IDialogService _dialogs;
            private readonly PasswordBox _currentBox;
            private readonly PasswordBox _newBox;
            private readonly PasswordBox _confirmBox;

            public ChangePasswordDialog(Client client, IClientService clients, IDialogService dialogs, Window? owner)
            {
                _client = client;
                _clients = clients;
                _dialogs = dialogs;

                Title = "Зміна пароля";
                Width = 410;
                Height = 320;
                MinWidth = 360;
                MinHeight = 300;
                WindowStartupLocation = owner != null ? WindowStartupLocation.CenterOwner : WindowStartupLocation.CenterScreen;
                ResizeMode = ResizeMode.NoResize;
                if (owner != null) Owner = owner;

                Background = new LinearGradientBrush(
                    Color.FromRgb(0xFF, 0xC3, 0xA0),
                    Color.FromRgb(0xD4, 0xA5, 0xFF),
                    new Point(0, 0),
                    new Point(1, 1));

                Border card = new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(0xFF, 0xE4, 0xE1)),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(0xFF, 0x85, 0xA1)),
                    BorderThickness = new Thickness(2),
                    CornerRadius = new CornerRadius(16),
                    Padding = new Thickness(18),
                    Margin = new Thickness(14)
                };

                StackPanel stack = new StackPanel();
                stack.Children.Add(new TextBlock
                {
                    Text = "🔑 Зміна пароля акаунту",
                    FontSize = 17,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Color.FromRgb(0xD6, 0x30, 0x31)),
                    Margin = new Thickness(0, 0, 0, 12)
                });

                stack.Children.Add(CreateLabel("Поточний пароль*"));
                _currentBox = CreatePasswordBox();
                stack.Children.Add(_currentBox);

                stack.Children.Add(CreateLabel("Новий пароль (мін. 4 символи)*"));
                _newBox = CreatePasswordBox();
                stack.Children.Add(_newBox);

                stack.Children.Add(CreateLabel("Підтвердьте новий пароль*"));
                _confirmBox = CreatePasswordBox();
                stack.Children.Add(_confirmBox);

                StackPanel buttonsPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Margin = new Thickness(0, 14, 0, 0)
                };

                Button cancelBtn = new Button
                {
                    Content = "Скасувати",
                    Height = 36,
                    Margin = new Thickness(0, 0, 10, 0),
                    FontWeight = FontWeights.Bold,
                    Background = new SolidColorBrush(Color.FromRgb(0xFF, 0xB3, 0xC6)),
                    Foreground = new SolidColorBrush(Color.FromRgb(0x1E, 0x27, 0x2E)),
                    Cursor = System.Windows.Input.Cursors.Hand
                };
                cancelBtn.Click += (_, _) => DialogResult = false;

                Button saveBtn = new Button
                {
                    Content = "Зберегти пароль",
                    Height = 36,
                    FontWeight = FontWeights.Bold,
                    Background = new SolidColorBrush(Color.FromRgb(0xFF, 0x2A, 0x55)),
                    Foreground = Brushes.White,
                    Cursor = System.Windows.Input.Cursors.Hand
                };
                saveBtn.Click += SavePassword_Click;

                buttonsPanel.Children.Add(cancelBtn);
                buttonsPanel.Children.Add(saveBtn);
                stack.Children.Add(buttonsPanel);

                card.Child = stack;
                Content = card;
            }

            private static TextBlock CreateLabel(string text) => new TextBlock
            {
                Text = text,
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(0x1E, 0x27, 0x2E)),
                Margin = new Thickness(0, 0, 0, 4)
            };

            private static PasswordBox CreatePasswordBox() => new PasswordBox
            {
                Height = 34,
                FontSize = 12.5,
                Background = new SolidColorBrush(Color.FromRgb(0xFF, 0xF0, 0xF5)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0xFF, 0x85, 0xA1)),
                BorderThickness = new Thickness(1.5),
                Padding = new Thickness(8, 5, 8, 5),
                Margin = new Thickness(0, 0, 0, 8)
            };

            private void SavePassword_Click(object sender, RoutedEventArgs e)
            {
                string currentPassword = _currentBox.Password;
                string newPassword = _newBox.Password;
                string confirmPassword = _confirmBox.Password;

                if (currentPassword != (_client.Password ?? string.Empty))
                {
                    _dialogs.Warn("Поточний пароль введено неправильно.", "Помилка");
                    return;
                }

                if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 4)
                {
                    _dialogs.Warn("Новий пароль має містити щонайменше 4 символи.", "Помилка валідації");
                    return;
                }

                if (newPassword != confirmPassword)
                {
                    _dialogs.Warn("Новий пароль та підтвердження не збігаються.", "Помилка валідації");
                    return;
                }

                if (newPassword == currentPassword)
                {
                    _dialogs.Warn("Новий пароль має відрізнятися від поточного.", "Помилка валідації");
                    return;
                }

                _client.Password = newPassword;
                _clients.Save();

                _dialogs.Info("Пароль успішно змінено!", "Безпека");
                DialogResult = true;
            }
        }
    }
}