using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using CourseApp.classes.enums;
using CourseApp.classes.information;
using CourseApp.classes.services;

namespace CourseApp.classes.mvvm.viewmodels
{
    public class ArchiveViewModel : ObservableObject
    {
        private readonly IClientService _clients;
        private readonly IMeetingService _meetings;
        private readonly IDialogService _dialogs;
        private readonly Action? _onChanged;

        private int _tabIndex;
        private Client? _partner1;
        private Client? _partner2;
        private Visibility _emptyCouplesVisibility = Visibility.Collapsed;
        private Visibility _couplesVisibility = Visibility.Visible;
        private Visibility _emptyDeletedVisibility = Visibility.Collapsed;
        private Visibility _deletedVisibility = Visibility.Visible;

        public event Action<int>? TabChanged;

        public ObservableCollection<Client> ActiveClients { get; } = new ObservableCollection<Client>();
        public ObservableCollection<ArchivedCouple> Couples { get; } = new ObservableCollection<ArchivedCouple>();
        public ObservableCollection<Client> DeletedClients { get; } = new ObservableCollection<Client>();

        public int TabIndex
        {
            get => _tabIndex;
            set { if (SetProperty(ref _tabIndex, value)) TabChanged?.Invoke(value); }
        }

        public Client? Partner1 { get => _partner1; set => SetProperty(ref _partner1, value); }
        public Client? Partner2 { get => _partner2; set => SetProperty(ref _partner2, value); }

        public Visibility EmptyCouplesVisibility { get => _emptyCouplesVisibility; private set => SetProperty(ref _emptyCouplesVisibility, value); }
        public Visibility CouplesVisibility { get => _couplesVisibility; private set => SetProperty(ref _couplesVisibility, value); }
        public Visibility EmptyDeletedVisibility { get => _emptyDeletedVisibility; private set => SetProperty(ref _emptyDeletedVisibility, value); }
        public Visibility DeletedVisibility { get => _deletedVisibility; private set => SetProperty(ref _deletedVisibility, value); }

        public ICommand ArchiveCommand { get; }
        public ICommand RestoreCommand { get; }
        public ICommand DeleteCommand { get; }

        public ArchiveViewModel(IClientService clients, IMeetingService meetings, IDialogService dialogs, Action? onChanged = null)
        {
            _clients = clients;
            _meetings = meetings;
            _dialogs = dialogs;
            _onChanged = onChanged;

            ArchiveCommand = new RelayCommand(_ => Archive());
            RestoreCommand = new RelayCommand(p => { if (p is Client c) Restore(c); });
            DeleteCommand = new RelayCommand(p => { if (p is Client c) Delete(c); });

            Reload();
        }

        public void SelectTab(int index)
        {
            TabIndex = index;
            Reload();
        }

        public void Reload()
        {
            ActiveClients.Clear();
            foreach (Client c in _clients.All)
            {
                if (c.Status == ClientStatus.Active)
                    ActiveClients.Add(c);
            }

            Couples.Clear();
            foreach (ArchivedCouple couple in _clients.Archived)
                Couples.Add(couple);

            EmptyCouplesVisibility = Couples.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            CouplesVisibility = Couples.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

            DeletedClients.Clear();
            foreach (Client d in _clients.Deleted)
                DeletedClients.Add(d);

            EmptyDeletedVisibility = DeletedClients.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            DeletedVisibility = DeletedClients.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Archive()
        {
            if (Partner1 == null || Partner2 == null)
            {
                _dialogs.Warn("Оберіть обох клієнтів для формування пари.", "Увага");
                return;
            }

            if (Partner1.RegNumber == Partner2.RegNumber)
            {
                _dialogs.Error("Неможливо створити пару з одного й того ж клієнта!", "Помилка");
                return;
            }

            string n1 = Partner1.FullName ?? string.Empty;
            string n2 = Partner2.FullName ?? string.Empty;

            _clients.Archive(Partner1, Partner2);
            Partner1 = null;
            Partner2 = null;

            _dialogs.Info($"Пару {n1} та {n2} перенесено в архів! 💍", "Архівація успішна");
            Reload();
            _onChanged?.Invoke();
        }

        private void Restore(Client client)
        {
            if (string.IsNullOrWhiteSpace(client.RegNumber)) return;

            bool ok = _dialogs.Confirm(
                $"Відновити клієнта {client.FullName} ({client.RegNumber}) та повернути його до активної бази?",
                "Відновлення клієнта");

            if (!ok) return;

            if (_clients.Restore(client.RegNumber))
            {
                Reload();
                _onChanged?.Invoke();
                _dialogs.Info($"Клієнта {client.FullName} успішно відновлено!", "Успіх");
            }
            else
            {
                _dialogs.Warn("Не вдалося знайти клієнта для відновлення.", "Помилка");
            }
        }

        private void Delete(Client client)
        {
            if (string.IsNullOrWhiteSpace(client.RegNumber)) return;

            bool ok = _dialogs.Confirm(
                $"Остаточно видалити клієнта {client.FullName} ({client.RegNumber}) з бази даних?\nЦю дію неможливо скасувати.",
                "Остаточне видалення",
                isWarning: true);

            if (!ok) return;

            if (_clients.DeleteForever(client.RegNumber))
            {
                _meetings.RemoveFor(client.RegNumber);
                Reload();
                _onChanged?.Invoke();
                _dialogs.Info($"Акаунт {client.FullName} повністю видалено.", "Видалено");
            }
            else
            {
                _dialogs.Warn("Не вдалося знайти клієнта для видалення.", "Помилка");
            }
        }
    }
}