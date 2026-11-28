using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CourseApp.classes.enums;
using CourseApp.classes.information;
using CourseApp.classes.services;

namespace CourseApp.classes.mvvm.viewmodels
{
    public class ProfileMenuViewModel : ObservableObject
    {
        private readonly IClientService _clients;
        private readonly IDialogService _dialogs;
        private readonly Action? _onChanged;
        private readonly Func<Window?>? _getOwner;

        private Client? _client;
        private string? _avatarPath;
        private readonly List<string> _photos = new List<string>();
        private readonly List<string> _hobbies = new List<string>();
        private bool _syncing;

        private string _fullName = string.Empty;
        private int _genderIndex;
        private string _age = string.Empty;
        private string _height = string.Empty;
        private string _weight = string.Empty;
        private string _details = string.Empty;

        private ImageSource? _avatar;
        private Visibility _avatarVisibility = Visibility.Collapsed;
        private Visibility _placeholderVisibility = Visibility.Visible;
        private string _avatarName = "Аватарку не обрано";
        private Visibility _emptyGalleryVisibility = Visibility.Visible;

        private string _hobbiesSummary = "Оберіть хобі...";
        private string _hobbiesCount = "0";

        public ObservableCollection<HobbyOption> PresetHobbies { get; } = new ObservableCollection<HobbyOption>();
        public ObservableCollection<string> SelectedHobbies { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> Photos { get; } = new ObservableCollection<string>();

        public string FullName { get => _fullName; set { _fullName = value; OnPropertyChanged(); } }
        public int GenderIndex { get => _genderIndex; set { _genderIndex = value; OnPropertyChanged(); } }
        public string Age { get => _age; set { _age = value; OnPropertyChanged(); } }
        public string Height { get => _height; set { _height = value; OnPropertyChanged(); } }
        public string Weight { get => _weight; set { _weight = value; OnPropertyChanged(); } }
        public string Details { get => _details; set { _details = value; OnPropertyChanged(); } }

        public ImageSource? Avatar { get => _avatar; private set { _avatar = value; OnPropertyChanged(); } }
        public Visibility AvatarVisibility { get => _avatarVisibility; private set { _avatarVisibility = value; OnPropertyChanged(); } }
        public Visibility PlaceholderVisibility { get => _placeholderVisibility; private set { _placeholderVisibility = value; OnPropertyChanged(); } }
        public string AvatarName { get => _avatarName; private set { _avatarName = value; OnPropertyChanged(); } }
        public Visibility EmptyGalleryVisibility { get => _emptyGalleryVisibility; private set { _emptyGalleryVisibility = value; OnPropertyChanged(); } }

        public string HobbiesSummary { get => _hobbiesSummary; private set { _hobbiesSummary = value; OnPropertyChanged(); } }
        public string HobbiesCount { get => _hobbiesCount; private set { _hobbiesCount = value; OnPropertyChanged(); } }

        public ICommand PickAvatarCommand { get; }
        public ICommand ClearAvatarCommand { get; }
        public ICommand AddPhotosCommand { get; }
        public ICommand RemovePhotoCommand { get; }
        public ICommand RemoveHobbyCommand { get; }
        public ICommand ChangePasswordCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand DeactivateCommand { get; }

        public ProfileMenuViewModel(
            IClientService clients,
            IDialogService dialogs,
            Action? onChanged = null,
            Func<Window?>? getOwner = null,
            Client? initialClient = null)
        {
            _clients = clients;
            _dialogs = dialogs;
            _onChanged = onChanged;
            _getOwner = getOwner;

            foreach (string h in ClientSearchViewModel.DefaultHobbies)
            {
                string title = h;
                HobbyOption? opt = null;
                opt = new HobbyOption(title, () => ToggleHobby(title, opt!.IsSelected));
                PresetHobbies.Add(opt);
            }

            PickAvatarCommand = new RelayCommand(_ => PickAvatar());
            ClearAvatarCommand = new RelayCommand(_ => { _avatarPath = null; UpdateMedia(); });
            AddPhotosCommand = new RelayCommand(_ => AddPhotos());
            RemovePhotoCommand = new RelayCommand(p => { if (p is string path) { _photos.Remove(path); UpdateMedia(); } });
            RemoveHobbyCommand = new RelayCommand(p => { if (p is string hobby) RemoveHobby(hobby); });
            ChangePasswordCommand = new RelayCommand(_ => OpenPasswordDialog());
            SaveCommand = new RelayCommand(_ => Save());
            DeactivateCommand = new RelayCommand(_ => Deactivate());

            Client? target = initialClient;
            if (target == null)
            {
                foreach (Client c in _clients.All)
                {
                    if (c.Status == ClientStatus.Active)
                    {
                        target = c;
                        break;
                    }
                }
            }

            if (target != null)
                LoadProfile(target);
        }

        public void LoadProfile(Client client)
        {
            _client = client;
            if (_client == null) return;

            FullName = _client.FullName ?? string.Empty;
            int idx = (int)_client.Gender;
            GenderIndex = idx >= 0 && idx <= 1 ? idx : 0;
            Age = _client.Age.ToString();
            Height = _client.HeightCm.HasValue ? _client.HeightCm.Value.ToString() : string.Empty;
            Weight = _client.WeightKg.HasValue ? _client.WeightKg.Value.ToString("0.#", CultureInfo.InvariantCulture) : string.Empty;
            Details = _client.PersonalDetails ?? string.Empty;

            _avatarPath = _client.AvatarPath;
            _photos.Clear();
            if (_client.PhotoPaths != null)
            {
                foreach (string p in _client.PhotoPaths) _photos.Add(p);
            }

            _hobbies.Clear();
            if (_client.Hobbies != null)
            {
                foreach (string raw in _client.Hobbies)
                {
                    if (string.IsNullOrWhiteSpace(raw)) continue;
                    string trimmed = raw.Trim();
                    if (!ContainsHobby(trimmed))
                        _hobbies.Add(trimmed);
                }
            }

            UpdateHobbies();
            UpdateMedia();
        }

        private bool ContainsHobby(string title)
        {
            foreach (string h in _hobbies)
            {
                if (string.Equals(h, title, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        private void ToggleHobby(string name, bool enabled)
        {
            if (_syncing) return;

            int idx = -1;
            for (int i = 0; i < _hobbies.Count; i++)
            {
                if (string.Equals(_hobbies[i], name, StringComparison.OrdinalIgnoreCase))
                {
                    idx = i;
                    break;
                }
            }

            if (enabled && idx == -1) _hobbies.Add(name);
            else if (!enabled && idx != -1) _hobbies.RemoveAt(idx);

            UpdateHobbies();
        }

        private void RemoveHobby(string name)
        {
            for (int i = _hobbies.Count - 1; i >= 0; i--)
            {
                if (string.Equals(_hobbies[i], name, StringComparison.OrdinalIgnoreCase))
                    _hobbies.RemoveAt(i);
            }
            UpdateHobbies();
        }

        private void UpdateHobbies()
        {
            _syncing = true;
            foreach (HobbyOption opt in PresetHobbies)
                opt.IsSelected = ContainsHobby(opt.Title);
            _syncing = false;

            SelectedHobbies.Clear();
            foreach (string h in _hobbies)
                SelectedHobbies.Add(h);

            HobbiesCount = SelectedHobbies.Count.ToString();
            HobbiesSummary = SelectedHobbies.Count > 0 ? string.Join(", ", SelectedHobbies) : "Оберіть хобі...";
        }

        private void UpdateMedia()
        {
            if (!string.IsNullOrWhiteSpace(_avatarPath) && File.Exists(_avatarPath))
            {
                Avatar = LoadBitmap(_avatarPath);
                AvatarVisibility = Visibility.Visible;
                PlaceholderVisibility = Visibility.Collapsed;
                AvatarName = Path.GetFileName(_avatarPath);
            }
            else
            {
                Avatar = null;
                AvatarVisibility = Visibility.Collapsed;
                PlaceholderVisibility = Visibility.Visible;
                AvatarName = "Аватарку не обрано";
            }

            Photos.Clear();
            foreach (string path in _photos)
            {
                if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
                    Photos.Add(path);
            }

            EmptyGalleryVisibility = Photos.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private static BitmapImage? LoadBitmap(string path)
        {
            try
            {
                BitmapImage bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.UriSource = new Uri(path, UriKind.Absolute);
                bmp.EndInit();
                bmp.Freeze();
                return bmp;
            }
            catch
            {
                return null;
            }
        }

        private static string SaveFile(string sourcePath)
        {
            string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UserMedia");
            Directory.CreateDirectory(dir);
            string dest = Path.Combine(dir, $"{Guid.NewGuid()}{Path.GetExtension(sourcePath)}");
            File.Copy(sourcePath, dest, overwrite: true);
            return dest;
        }

        private void PickAvatar()
        {
            string? file = _dialogs.PickImage("Оберіть аватарку");
            if (file != null)
            {
                _avatarPath = SaveFile(file);
                UpdateMedia();
            }
        }

        private void AddPhotos()
        {
            string[] files = _dialogs.PickImages("Оберіть фотографії для профілю");
            if (files.Length == 0) return;

            foreach (string file in files)
            {
                string saved = SaveFile(file);
                if (!_photos.Contains(saved))
                    _photos.Add(saved);
            }

            UpdateMedia();
        }

        private void OpenPasswordDialog()
        {
            if (_client == null) return;
            windows.views.ProfileMenuView.ChangePasswordDialog dialog =
                new windows.views.ProfileMenuView.ChangePasswordDialog(_client, _clients, _dialogs, _getOwner?.Invoke());
            dialog.ShowDialog();
        }

        private void Save()
        {
            if (_client == null) return;

            string name = FullName.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                _dialogs.Warn("Введіть ПІБ клієнта.", "Помилка валідації");
                return;
            }

            if (!int.TryParse(Age.Trim(), out int age) || age < 18)
            {
                _dialogs.Warn("Вік клієнта має бути числом від 18 років.", "Помилка валідації");
                return;
            }

            int? heightCm = null;
            if (!string.IsNullOrWhiteSpace(Height))
            {
                if (!int.TryParse(Height.Trim(), out int h) || h < 50 || h > 260)
                {
                    _dialogs.Warn("Зріст має бути коректним числом у сантиметрах (наприклад, 175).", "Помилка валідації");
                    return;
                }
                heightCm = h;
            }

            double? weightKg = null;
            string wRaw = Weight.Trim().Replace(',', '.');
            if (!string.IsNullOrWhiteSpace(wRaw))
            {
                if (!double.TryParse(wRaw, NumberStyles.Float, CultureInfo.InvariantCulture, out double w) || w < 20 || w > 350)
                {
                    _dialogs.Warn("Вага має бути коректним числом у кілограмах (наприклад, 68).", "Помилка валідації");
                    return;
                }
                weightKg = w;
            }

            _client.FullName = name;
            if (GenderIndex >= 0) _client.Gender = (Gender)GenderIndex;
            _client.Age = age;
            _client.HeightCm = heightCm;
            _client.WeightKg = weightKg;
            _client.Hobbies = new List<string>(_hobbies);
            _client.PersonalDetails = Details.Trim();
            _client.AvatarPath = _avatarPath;
            _client.PhotoPaths = new List<string>(_photos);

            _clients.Save();
            _onChanged?.Invoke();

            _dialogs.Info("Зміни профілю успішно збережено!", "Збережено");
        }

        private void Deactivate()
        {
            if (_client == null || string.IsNullOrWhiteSpace(_client.RegNumber)) return;

            bool ok = _dialogs.Confirm(
                "Ви дійсно бажаєте деактивувати свій акаунт? Ви вийдете з системи, а профіль переміститься до списку вилучених.",
                "Деактивація акаунту",
                isWarning: true);

            if (!ok) return;

            _clients.Delete(_client.RegNumber, "Деактивація профілю клієнтом");
            _onChanged?.Invoke();
        }
    }
}