using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using CourseApp.classes.information;
using CourseApp.classes.enums;

namespace CourseApp.classes.services.CourseApp.classes
{
    public class DatingService
    {
        public ObservableCollection<Client> AllClients { get; private set; }
        public List<MeetingInvitation> Meetings { get; private set; }
        public ObservableCollection<ArchivedCouple> ArchivedCouples { get; private set; } = new();
        public ObservableCollection<Client> DeletedClients { get; private set; } = new();

        private const string DeletedClientsFilePath = "deleted_clients.json";
        private const string ArchiveFilePath = "archived_couples.json";
        private const string filePath = "clients.json";
        private const string MeetingsFilePath = "meetings.json";
        private readonly JsonSerializerOptions _jsonOptions;

        public DatingService()
        {
            AllClients = new ObservableCollection<Client>();
            Meetings = new List<MeetingInvitation>();

            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
            };

            LoadInitialData();
            LoadMeetings();
            LoadArchivedCouples();
            LoadDeletedClients();
            SyncDeletedClients();
        }

        private void LoadInitialData()
        {
            if (File.Exists(filePath))
            {
                try
                {
                    string json = File.ReadAllText(filePath);
                    List<Client>? loadedClients = JsonSerializer.Deserialize<List<Client>>(json, _jsonOptions);

                    if (loadedClients != null && loadedClients.Count > 0)
                    {
                        foreach (Client client in loadedClients)
                        {
                            AllClients.Add(client);
                        }
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        private void LoadMeetings()
        {
            if (!File.Exists(MeetingsFilePath)) return;

            try
            {
                string json = File.ReadAllText(MeetingsFilePath);
                List<MeetingInvitation>? loaded = JsonSerializer.Deserialize<List<MeetingInvitation>>(json, _jsonOptions);
                if (loaded != null) Meetings.AddRange(loaded);
            }
            catch (Exception) { }
        }

        public void SaveMeetings()
        {
            string json = JsonSerializer.Serialize(Meetings, _jsonOptions);
            File.WriteAllText(MeetingsFilePath, json);
        }

        public void CreateMeetingInvitation(Client initiator, Client target, DateTime? meetingDate = null)
        {
            if (initiator == null || target == null) return;

            Meetings.Add(new MeetingInvitation
            {
                Initiator = initiator,
                Target = target,
                MeetingDate = meetingDate ?? DateTime.Now.AddDays(3)
            });
            SaveMeetings();
        }

        public void UpdateMeetingStatus(string meetingId, MeetingStatus newStatus)
        {
            MeetingInvitation? meeting = Meetings.FirstOrDefault(m => m.Id == meetingId);
            if (meeting != null)
            {
                meeting.Status = newStatus;
                SaveMeetings();
            }
        }

        public bool CancelMeeting(string meetingId)
        {
            MeetingInvitation? meeting = Meetings.FirstOrDefault(m => m.Id == meetingId);
            if (meeting == null) return false;

            Meetings.Remove(meeting);
            SaveMeetings();
            return true;
        }

        public List<MeetingInvitation> GetClientMeetings(string regNumber)
        {
            List<MeetingInvitation> result = new List<MeetingInvitation>();
            if (string.IsNullOrWhiteSpace(regNumber)) return result;

            foreach (MeetingInvitation meeting in Meetings)
            {
                bool isInitiator = meeting.Initiator != null &&
                                   string.Equals(meeting.Initiator.RegNumber, regNumber, StringComparison.OrdinalIgnoreCase);
                bool isTarget = meeting.Target != null &&
                                string.Equals(meeting.Target.RegNumber, regNumber, StringComparison.OrdinalIgnoreCase);

                if (isInitiator || isTarget)
                {
                    result.Add(meeting);
                }
            }

            result.Sort((a, b) => b.MeetingDate.CompareTo(a.MeetingDate));
            return result;
        }

        public int GetPendingIncomingCount(string regNumber)
        {
            int count = 0;
            if (string.IsNullOrWhiteSpace(regNumber)) return 0;

            foreach (MeetingInvitation meeting in Meetings)
            {
                if (meeting.Status == MeetingStatus.Pending &&
                    meeting.Target != null &&
                    string.Equals(meeting.Target.RegNumber, regNumber, StringComparison.OrdinalIgnoreCase))
                {
                    count++;
                }
            }
            return count;
        }

        public void SaveClients()
        {
            string json = JsonSerializer.Serialize(AllClients, _jsonOptions);
            File.WriteAllText(filePath, json);
        }

        public string RegisterNewClient(Client newClient)
        {
            if (AllClients.Concat(DeletedClients).Any(c =>
                c.RegNumber != null && c.RegNumber.Equals(newClient.RegNumber, StringComparison.OrdinalIgnoreCase)))
                return "Помилка: Клієнт із таким реєстраційним номером вже існує!";

            if (string.IsNullOrWhiteSpace(newClient.FullName) || newClient.Age < 18)
                return "Помилка валідації: ПІБ не може бути порожнім, а вік має бути від 18 років.";

            newClient.RegDate = DateTime.Now;
            AllClients.Add(newClient);

            SaveClients();

            return "Success";
        }

        public bool MatchesPreferences(Client seeker, Client candidate)
        {
            if (seeker?.Requirements == null || candidate == null) return false;
            if (candidate.Status != ClientStatus.Active) return false;
            if (string.Equals(seeker.RegNumber, candidate.RegNumber, StringComparison.OrdinalIgnoreCase)) return false;

            if (candidate.Gender != seeker.Requirements.PreferredGender) return false;
            if (candidate.Age < seeker.Requirements.MinAge || candidate.Age > seeker.Requirements.MaxAge) return false;

            return true;
        }

        public List<Client> RunMatchingAlgorithm(Client client)
        {
            return FindMatchesForClient(client, false, "");
        }

        public List<Client> FindMatchesForClient(Client client, bool mutualOnly, string keyword)
        {
            List<Client> result = new List<Client>();
            if (client?.Requirements == null) return result;

            string search = keyword == null ? "" : keyword.Trim();

            foreach (Client candidate in AllClients)
            {
                if (!MatchesPreferences(client, candidate)) continue;
                if (mutualOnly && !MatchesPreferences(candidate, client)) continue;

                if (!string.IsNullOrEmpty(search))
                {
                    bool nameHit = !string.IsNullOrEmpty(candidate.FullName) &&
                                   candidate.FullName.Contains(search, StringComparison.OrdinalIgnoreCase);
                    bool detailsHit = !string.IsNullOrEmpty(candidate.PersonalDetails) &&
                                      candidate.PersonalDetails.Contains(search, StringComparison.OrdinalIgnoreCase);

                    if (!nameHit && !detailsHit) continue;
                }

                result.Add(candidate);
            }

            return result;
        }

        private void LoadArchivedCouples()
        {
            if (!File.Exists(ArchiveFilePath)) return;

            try
            {
                string json = File.ReadAllText(ArchiveFilePath);
                List<ArchivedCouple>? loaded = JsonSerializer.Deserialize<List<ArchivedCouple>>(json, _jsonOptions);
                if (loaded != null)
                {
                    foreach (ArchivedCouple couple in loaded)
                        ArchivedCouples.Add(couple);
                }
            }
            catch (Exception) { }
        }

        public void SaveArchivedCouples()
        {
            string json = JsonSerializer.Serialize(ArchivedCouples, _jsonOptions);
            File.WriteAllText(ArchiveFilePath, json);
        }

        public void MoveCoupleToArchive(Client client1, Client client2)
        {
            if (client1 == null || client2 == null) return;

            client1.Status = ClientStatus.Archived;
            client2.Status = ClientStatus.Archived;

            ArchivedCouples.Add(new ArchivedCouple
            {
                Client1Name = $"{client1.FullName} ({client1.RegNumber})",
                Client2Name = $"{client2.FullName} ({client2.RegNumber})",
                ArchiveDate = DateTime.Now,
                StatusInfo = "Активна пара (Архів)"
            });

            SaveClients();
            SaveArchivedCouples();
        }

        private void LoadDeletedClients()
        {
            if (!File.Exists(DeletedClientsFilePath)) return;

            try
            {
                string json = File.ReadAllText(DeletedClientsFilePath);
                List<Client>? loaded = JsonSerializer.Deserialize<List<Client>>(json, _jsonOptions);
                if (loaded != null)
                {
                    foreach (Client client in loaded)
                        DeletedClients.Add(client);
                }
            }
            catch (Exception) { }
        }

        public void SaveDeletedClients()
        {
            string json = JsonSerializer.Serialize(DeletedClients, _jsonOptions);
            File.WriteAllText(DeletedClientsFilePath, json);
        }

        public bool RestoreClient(string regNumber)
        {
            if (string.IsNullOrWhiteSpace(regNumber)) return false;
            string target = regNumber.Trim();

            List<Client> deletedMatches = DeletedClients
                .Where(c => string.Equals(c.RegNumber?.Trim(), target, StringComparison.OrdinalIgnoreCase))
                .ToList();

            Client? activeMatch = AllClients
                .FirstOrDefault(c => string.Equals(c.RegNumber?.Trim(), target, StringComparison.OrdinalIgnoreCase));

            Client? client = activeMatch ?? deletedMatches.FirstOrDefault();
            if (client == null) return false;

            client.Status = ClientStatus.Active;
            client.Reason = null;

            foreach (Client item in deletedMatches)
                DeletedClients.Remove(item);

            if (activeMatch == null)
                AllClients.Add(client);

            SaveClients();
            SaveDeletedClients();
            return true;
        }

        private void SyncDeletedClients()
        {
            bool clientsChanged = false;
            bool deletedChanged = false;

            List<Client> legacyDeleted = AllClients.Where(c => c.Status == ClientStatus.Deleted).ToList();
            foreach (Client client in legacyDeleted)
            {
                AllClients.Remove(client);
                clientsChanged = true;

                if (!DeletedClients.Any(d => string.Equals(d.RegNumber, client.RegNumber, StringComparison.OrdinalIgnoreCase)))
                {
                    DeletedClients.Add(client);
                    deletedChanged = true;
                }
            }

            List<Client> stale = DeletedClients.Where(d => AllClients.Any(a =>
                string.Equals(a.RegNumber, d.RegNumber, StringComparison.OrdinalIgnoreCase) ||
                (!string.IsNullOrWhiteSpace(d.Email) &&
                 string.Equals(a.Email, d.Email, StringComparison.OrdinalIgnoreCase))))
                .ToList();

            foreach (Client item in stale)
            {
                DeletedClients.Remove(item);
                deletedChanged = true;
            }

            if (clientsChanged) SaveClients();
            if (deletedChanged) SaveDeletedClients();
        }

        public bool PermanentlyDeleteClient(string regNumber)
        {
            if (string.IsNullOrWhiteSpace(regNumber)) return false;
            string target = regNumber.Trim();

            List<Client> deletedMatches = DeletedClients
                .Where(c => string.Equals(c.RegNumber?.Trim(), target, StringComparison.OrdinalIgnoreCase))
                .ToList();

            List<Client> legacyMatches = AllClients
                .Where(c => c.Status == ClientStatus.Deleted &&
                            string.Equals(c.RegNumber?.Trim(), target, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (deletedMatches.Count == 0 && legacyMatches.Count == 0) return false;

            foreach (Client item in deletedMatches)
                DeletedClients.Remove(item);

            foreach (Client item in legacyMatches)
                AllClients.Remove(item);

            int removedMeetings = Meetings.RemoveAll(m =>
                (m.Initiator != null && string.Equals(m.Initiator.RegNumber?.Trim(), target, StringComparison.OrdinalIgnoreCase)) ||
                (m.Target != null && string.Equals(m.Target.RegNumber?.Trim(), target, StringComparison.OrdinalIgnoreCase)));

            if (deletedMatches.Count > 0) SaveDeletedClients();
            if (legacyMatches.Count > 0) SaveClients();
            if (removedMeetings > 0) SaveMeetings();

            return true;
        }

        public void DeleteClient(string regNumber, string reason = "Відмова від послуг")
        {
            Client? client = AllClients.FirstOrDefault(c => c.RegNumber == regNumber);
            if (client == null) return;

            client.Status = ClientStatus.Deleted;
            client.Reason = reason;

            AllClients.Remove(client);
            DeletedClients.Add(client);

            SaveClients();
            SaveDeletedClients();
        }
    }
}