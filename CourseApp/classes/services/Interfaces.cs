using System.Collections.ObjectModel;
using CourseApp.classes.enums;
using CourseApp.classes.information;

namespace CourseApp.classes.services
{
    public interface IStorage
    {
        List<T> Load<T>(string path);
        void Save<T>(string path, IEnumerable<T> items);
    }

    public interface IMatchRule
    {
        bool IsMatch(Client seeker, Client candidate);
    }

    public interface IClientService
    {
        ObservableCollection<Client> All { get; }
        ObservableCollection<Client> Deleted { get; }
        ObservableCollection<ArchivedCouple> Archived { get; }

        void Save();
        string Register(Client client);
        Client? FindByLogin(string id, string password);
        void Archive(Client first, Client second);
        void Delete(string regNumber, string reason = "Відмова від послуг");
        bool Restore(string regNumber);
        bool DeleteForever(string regNumber);
    }

    public interface IMeetingService
    {
        List<MeetingInvitation> All { get; }

        void Create(Client initiator, Client target, DateTime? date = null);
        void SetStatus(string id, MeetingStatus status);
        bool Cancel(string id);
        List<MeetingInvitation> GetFor(string regNumber);
        int CountPending(string regNumber);
        void RemoveFor(string regNumber);
    }

    public interface IMatchService
    {
        bool IsMatch(Client seeker, Client candidate);
        List<Client> Find(Client client, bool mutualOnly = false, string keyword = "");
    }

    public interface IDialogService
    {
        void Info(string message, string title);
        void Warn(string message, string title);
        void Error(string message, string title);
        bool Confirm(string message, string title, bool isWarning = false);
        string? PickImage(string title);
        string[] PickImages(string title);
    }
}