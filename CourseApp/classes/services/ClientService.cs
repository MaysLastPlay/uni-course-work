using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CourseApp.classes.enums;
using CourseApp.classes.information;

namespace CourseApp.classes.services
{
    public class ClientService : IClientService
    {
        private const string ClientsFile = "clients.json";
        private const string DeletedFile = "deleted_clients.json";
        private const string ArchiveFile = "archived_couples.json";

        private readonly IStorage _storage;

        public ObservableCollection<Client> All { get; } = new ObservableCollection<Client>();
        public ObservableCollection<Client> Deleted { get; } = new ObservableCollection<Client>();
        public ObservableCollection<ArchivedCouple> Archived { get; } = new ObservableCollection<ArchivedCouple>();
        private const int DeleteRetentionDays = 14;

        public ClientService(IStorage storage)
        {
            _storage = storage;

            foreach (Client c in _storage.Load<Client>(ClientsFile)) All.Add(c);
            foreach (Client d in _storage.Load<Client>(DeletedFile)) Deleted.Add(d);
            foreach (ArchivedCouple a in _storage.Load<ArchivedCouple>(ArchiveFile)) Archived.Add(a);

            SyncDeleted();
            PurgeExpiredDeleted();
        }

        public void Save() => _storage.Save(ClientsFile, All);

        public string Register(Client client)
        {
            if (Exists(client.RegNumber))
                return "Помилка: Клієнт із таким реєстраційним номером вже існує!";

            if (string.IsNullOrWhiteSpace(client.FullName) || client.Age < 18)
                return "Помилка валідації: ПІБ не може бути порожнім, а вік має бути від 18 років.";

            client.RegDate = DateTime.Now;
            All.Add(client);
            Save();
            return "Success";
        }

        public Client? FindByLogin(string id, string password)
        {
            string key = id.Trim();

            foreach (Client c in All)
            {
                if (c.Status != ClientStatus.Active) continue;

                bool emailOk = !string.IsNullOrWhiteSpace(c.Email) &&
                               c.Email.Equals(key, StringComparison.OrdinalIgnoreCase);
                bool regOk = SameReg(c.RegNumber, key);

                if ((emailOk || regOk) && c.Password == password)
                    return c;
            }

            return null;
        }

        public void Archive(Client first, Client second)
        {
            if (first == null || second == null) return;

            first.Status = ClientStatus.Archived;
            second.Status = ClientStatus.Archived;

            Archived.Add(new ArchivedCouple
            {
                Client1Name = $"{first.FullName} ({first.RegNumber})",
                Client2Name = $"{second.FullName} ({second.RegNumber})",
                ArchiveDate = DateTime.Now,
                StatusInfo = "Активна пара (Архів)"
            });

            Save();
            _storage.Save(ArchiveFile, Archived);
        }

        public void Delete(string regNumber, string reason = "Відмова від послуг")
        {
            Client? target = null;
            foreach (Client c in All)
            {
                if (SameReg(c.RegNumber, regNumber))
                {
                    target = c;
                    break;
                }
            }

            if (target == null) return;

            target.Status = ClientStatus.Deleted;
            target.Reason = reason;
            target.DeletedAt = DateTime.Now;

            All.Remove(target);
            Deleted.Add(target);

            Save();
            _storage.Save(DeletedFile, Deleted);
        }

        public bool Restore(string regNumber)
        {
            if (string.IsNullOrWhiteSpace(regNumber)) return false;

            List<Client> removed = new List<Client>();
            foreach (Client c in Deleted)
            {
                if (SameReg(c.RegNumber, regNumber))
                    removed.Add(c);
            }

            Client? active = null;
            foreach (Client c in All)
            {
                if (SameReg(c.RegNumber, regNumber))
                {
                    active = c;
                    break;
                }
            }

            Client? client = active ?? (removed.Count > 0 ? removed[0] : null);
            if (client == null) return false;

            client.Status = ClientStatus.Active;
            client.Reason = null;
            client.DeletedAt = null;

            foreach (Client item in removed) Deleted.Remove(item);
            if (active == null) All.Add(client);

            Save();
            _storage.Save(DeletedFile, Deleted);
            return true;
        }

        public bool DeleteForever(string regNumber)
        {
            if (string.IsNullOrWhiteSpace(regNumber)) return false;

            List<Client> fromDeleted = new List<Client>();
            foreach (Client c in Deleted)
            {
                if (SameReg(c.RegNumber, regNumber))
                    fromDeleted.Add(c);
            }

            List<Client> fromAll = new List<Client>();
            foreach (Client c in All)
            {
                if (c.Status == ClientStatus.Deleted && SameReg(c.RegNumber, regNumber))
                    fromAll.Add(c);
            }

            if (fromDeleted.Count == 0 && fromAll.Count == 0) return false;

            foreach (Client item in fromDeleted) Deleted.Remove(item);
            foreach (Client item in fromAll) All.Remove(item);

            if (fromDeleted.Count > 0) _storage.Save(DeletedFile, Deleted);
            if (fromAll.Count > 0) Save();

            return true;
        }

        private void PurgeExpiredDeleted()
        {
            DateTime now = DateTime.Now;
            List<Client> expired = new List<Client>();
            bool changed = false;

            foreach (Client c in Deleted)
            {
                if (!c.DeletedAt.HasValue)
                {
                    c.DeletedAt = now;
                    changed = true;
                    continue;
                }

                if ((now - c.DeletedAt.Value).TotalDays >= DeleteRetentionDays)
                    expired.Add(c);
            }

            foreach (Client item in expired)
            {
                Deleted.Remove(item);
                changed = true;
            }

            if (changed)
                _storage.Save(DeletedFile, Deleted);
        }

        private bool Exists(string? regNumber)
        {
            foreach (Client c in All)
            {
                if (SameReg(c.RegNumber, regNumber)) return true;
            }

            foreach (Client c in Deleted)
            {
                if (SameReg(c.RegNumber, regNumber)) return true;
            }

            return false;
        }

        private void SyncDeleted()
        {
            List<Client> moved = new List<Client>();
            foreach (Client c in All)
            {
                if (c.Status == ClientStatus.Deleted)
                    moved.Add(c);
            }

            bool allChanged = moved.Count > 0;
            bool deletedChanged = false;

            foreach (Client client in moved)
            {
                All.Remove(client);
                bool exists = false;
                foreach (Client d in Deleted)
                {
                    if (SameReg(d.RegNumber, client.RegNumber))
                    {
                        exists = true;
                        break;
                    }
                }

                if (!exists)
                {
                    Deleted.Add(client);
                    deletedChanged = true;
                }
            }

            List<Client> stale = new List<Client>();
            foreach (Client d in Deleted)
            {
                foreach (Client a in All)
                {
                    bool sameEmail = !string.IsNullOrWhiteSpace(d.Email) &&
                                     string.Equals(a.Email, d.Email, StringComparison.OrdinalIgnoreCase);
                    if (SameReg(a.RegNumber, d.RegNumber) || sameEmail)
                    {
                        stale.Add(d);
                        break;
                    }
                }
            }

            foreach (Client item in stale)
            {
                Deleted.Remove(item);
                deletedChanged = true;
            }

            if (allChanged) Save();
            if (deletedChanged) _storage.Save(DeletedFile, Deleted);
        }

        private static bool SameReg(string? a, string? b)
        {
            return !string.IsNullOrWhiteSpace(a) &&
                   !string.IsNullOrWhiteSpace(b) &&
                   string.Equals(a.Trim(), b.Trim(), StringComparison.OrdinalIgnoreCase);
        }
    }
}