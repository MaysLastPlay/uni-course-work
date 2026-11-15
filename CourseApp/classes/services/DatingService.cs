using System;
using System.Collections.Generic;
using System.Text;
using CourseApp.classes.information;

namespace CourseApp.classes.services
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    namespace CourseApp.classes
    {
        public class DatingService
        {
            public ObservableCollection<Client> AllClients { get; private set; }
            public List<MeetingInvitation> Meetings { get; private set; }

            public DatingService()
            {
                AllClients = new ObservableCollection<Client>();
                Meetings = new List<MeetingInvitation>();
                LoadInitialData();
            }

            private void LoadInitialData()
            {
                AllClients.Add(new Client { RegNumber = "REG-001", FullName = "Олександр Петренко", Gender = Gender.Чоловіча, Age = 28, RegDate = new DateTime(2025, 1, 10), Requirements = new PRequirements { PreferredGender = Gender.Жіноча, MinAge = 22, MaxAge = 30 } });
                AllClients.Add(new Client { RegNumber = "REG-002", FullName = "Марія Коваленко", Gender = Gender.Жіноча, Age = 25, RegDate = new DateTime(2025, 2, 5), Requirements = new PRequirements { PreferredGender = Gender.Чоловіча, MinAge = 25, MaxAge = 35 } });
                AllClients.Add(new Client { RegNumber = "REG-003", FullName = "Анна Шевченко", Gender = Gender.Жіноча, Age = 27, RegDate = new DateTime(2025, 2, 12), Requirements = new PRequirements { PreferredGender = Gender.Чоловіча, MinAge = 26, MaxAge = 36 } });
            }

            public string RegisterNewClient(Client newClient)
            {
                if (AllClients.Any(c => c.RegNumber.Equals(newClient.RegNumber, StringComparison.OrdinalIgnoreCase)))
                    return "Помилка: Клієнт із таким реєстраційним номером вже існує!";

                if (string.IsNullOrWhiteSpace(newClient.FullName) || newClient.Age < 18)
                    return "Помилка валідації: ПІБ не може бути порожнім, а вік має бути від 18 років.";

                newClient.RegDate = DateTime.Now;
                AllClients.Add(newClient);
                return "Success";
            }

            public List<Client> RunMatchingAlgorithm(Client client)
            {
                if (client?.Requirements == null) return new List<Client>();

                var req = client.Requirements;
                return AllClients.Where(c =>
                    c.Status == ClientStatus.Active &&
                    c.RegNumber != client.RegNumber &&
                    c.Gender == req.PreferredGender &&
                    c.Age >= req.MinAge &&
                    c.Age <= req.MaxAge).ToList();
            }

            public void CreateMeetingInvitation(Client initiator, Client target)
            {
                if (initiator == null || target == null) return;

                Meetings.Add(new MeetingInvitation
                {
                    Initiator = initiator,
                    Target = target,
                    MeetingDate = DateTime.Now.AddDays(3)
                });
            }

            public void MoveCoupleToArchive(Client client1, Client client2)
            {
                if (client1 != null) client1.Status = ClientStatus.Archived;
                if (client2 != null) client2.Status = ClientStatus.Archived;
            }

            public void DeleteClient(string regNumber)
            {
                var client = AllClients.FirstOrDefault(c => c.RegNumber == regNumber);
                if (client != null)
                {
                    client.Status = ClientStatus.Deleted;
                }
            }
        }
    }
}
