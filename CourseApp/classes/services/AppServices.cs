using System;
using System.Collections.Generic;
using CourseApp.classes.enums;
using CourseApp.classes.information;

namespace CourseApp.classes.services
{
    public class AppServices
    {
        public IClientService Clients { get; }
        public IMeetingService Meetings { get; }
        public IMatchService Matches { get; }
        public IDialogService Dialogs { get; }

        public AppServices()
        {
            IStorage storage = new JsonStorage();

            Clients = new ClientService(storage);
            Meetings = new MeetingService(storage);
            Dialogs = new WpfDialogs();

            List<IMatchRule> rules = new List<IMatchRule>
            {
                new ActiveRule(),
                new GenderRule(),
                new AgeRule()
            };

            Matches = new MatchService(Clients, rules);
            SeedDemoBots();
        }

        private void SeedDemoBots()
        {
            List<Client> bots = new List<Client>
            {
                new Client
                {
                    RegNumber = "BOT-001",
                    FullName = "Коваленко Аліна Сергіївна",
                    Email = "alina.kovalenko@cupidlove.demo",
                    Password = "demo1234",
                    Gender = Gender.Жіноча,
                    Age = 19,
                    HeightCm = 166,
                    WeightKg = 53,
                    RegDate = DateTime.Now.AddDays(-10),
                    Hobbies = new List<string> { "Малювання", "Відеоігри", "Кіно та серіали" },
                    PersonalDetails = "Студентка-дизайнерка. Обожнюю вечірні прогулянки, каву, інді-ігри та хороші фільми.",
                    Status = ClientStatus.Active,
                    Requirements = new PRequirements { PreferredGender = Gender.Чоловіча, MinAge = 18, MaxAge = 24, CriteriaDescription = "Почуття гумору, щирість і спільні інтереси." }
                },
                new Client
                {
                    RegNumber = "BOT-002",
                    FullName = "Мельник Софія Олександрівна",
                    Email = "sofiia.melnyk@cupidlove.demo",
                    Password = "demo1234",
                    Gender = Gender.Жіноча,
                    Age = 20,
                    HeightCm = 170,
                    WeightKg = 56,
                    RegDate = DateTime.Now.AddDays(-8),
                    Hobbies = new List<string> { "Фотографія", "Подорожі", "Захоплення музикою" },
                    PersonalDetails = "Знімаю на плівку, слухаю альтернативний рок і мрію об'їздити всю Європу.",
                    Status = ClientStatus.Active,
                    Requirements = new PRequirements { PreferredGender = Gender.Чоловіча, MinAge = 18, MaxAge = 25, CriteriaDescription = "Відкритість до нових вражень та любов до музики." }
                },
                new Client
                {
                    RegNumber = "BOT-003",
                    FullName = "Ткаченко Дарина Ігорівна",
                    Email = "daryna.tkachenko@cupidlove.demo",
                    Password = "demo1234",
                    Gender = Gender.Жіноча,
                    Age = 18,
                    HeightCm = 163,
                    WeightKg = 50,
                    RegDate = DateTime.Now.AddDays(-6),
                    Hobbies = new List<string> { "Програмування", "Відеоігри", "Настільні ігри" },
                    PersonalDetails = "Вивчаю IT, люблю кооперативні ігри, настілки з друзями та нічні розмови про все на світі.",
                    Status = ClientStatus.Active,
                    Requirements = new PRequirements { PreferredGender = Gender.Чоловіча, MinAge = 18, MaxAge = 23, CriteriaDescription = "Легкість у спілкуванні та спільні ігрові вечори." }
                },
                new Client
                {
                    RegNumber = "BOT-004",
                    FullName = "Бондаренко Анастасія Дмитрівна",
                    Email = "anastasiia.bondarenko@cupidlove.demo",
                    Password = "demo1234",
                    Gender = Gender.Жіноча,
                    Age = 21,
                    HeightCm = 172,
                    WeightKg = 58,
                    RegDate = DateTime.Now.AddDays(-5),
                    Hobbies = new List<string> { "Читання книг", "Кулінарія", "Йога та медитація" },
                    PersonalDetails = "Люблю затишні вечори, готую смачну пасту та шукаю людину для щирого спілкування.",
                    Status = ClientStatus.Active,
                    Requirements = new PRequirements { PreferredGender = Gender.Чоловіча, MinAge = 19, MaxAge = 26, CriteriaDescription = "Доброта, надійність та повага." }
                },
                new Client
                {
                    RegNumber = "BOT-005",
                    FullName = "Шевченко Вікторія Андріївна",
                    Email = "viktoriia.shevchenko@cupidlove.demo",
                    Password = "demo1234",
                    Gender = Gender.Жіноча,
                    Age = 19,
                    HeightCm = 168,
                    WeightKg = 54,
                    RegDate = DateTime.Now.AddDays(-3),
                    Hobbies = new List<string> { "Спорт та фітнес", "Танці", "Вивчення мов" },
                    PersonalDetails = "Активна, позитивна, займаюся танцями й вивчаю англійську. Завжди за спонтанні прогулянки!",
                    Status = ClientStatus.Active,
                    Requirements = new PRequirements { PreferredGender = Gender.Чоловіча, MinAge = 18, MaxAge = 24, CriteriaDescription = "Енергійність, почуття гумору та ініціативність." }
                }
            };

            bool addedAny = false;
            foreach (Client bot in bots)
            {
                if (BotExists(bot.RegNumber)) continue;
                Clients.All.Add(bot);
                addedAny = true;
            }

            if (addedAny) Clients.Save();
        }

        private bool BotExists(string? reg)
        {
            if (string.IsNullOrWhiteSpace(reg)) return false;

            foreach (Client c in Clients.All)
            {
                if (string.Equals(c.RegNumber, reg, StringComparison.OrdinalIgnoreCase)) return true;
            }

            foreach (Client c in Clients.Deleted)
            {
                if (string.Equals(c.RegNumber, reg, StringComparison.OrdinalIgnoreCase)) return true;
            }

            foreach (ArchivedCouple couple in Clients.Archived)
            {
                if ((!string.IsNullOrWhiteSpace(couple.Client1Name) && couple.Client1Name.Contains(reg, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(couple.Client2Name) && couple.Client2Name.Contains(reg, StringComparison.OrdinalIgnoreCase)))
                {
                    return true;
                }
            }

            return false;
        }
    }
}