using System;
using System.Collections.Generic;
using CourseApp.classes.enums;
using CourseApp.classes.information;

namespace CourseApp.classes.services
{
    public class ActiveRule : IMatchRule
    {
        public bool IsMatch(Client seeker, Client candidate)
        {
            if (candidate.Status != ClientStatus.Active) return false;
            return !string.Equals(seeker.RegNumber?.Trim(), candidate.RegNumber?.Trim(), StringComparison.OrdinalIgnoreCase);
        }
    }

    public class GenderRule : IMatchRule
    {
        public bool IsMatch(Client seeker, Client candidate)
        {
            if (seeker.Requirements == null ||
                (seeker.Requirements.MinAge == 0 && seeker.Requirements.MaxAge == 0))
            {
                Gender preferred = seeker.Gender == Gender.Чоловіча ? Gender.Жіноча : Gender.Чоловіча;
                return candidate.Gender == preferred;
            }

            return candidate.Gender == seeker.Requirements.PreferredGender;
        }
    }

    public class AgeRule : IMatchRule
    {
        public bool IsMatch(Client seeker, Client candidate)
        {
            if (seeker.Requirements == null ||
                (seeker.Requirements.MinAge == 0 && seeker.Requirements.MaxAge == 0))
            {
                return true;
            }

            int minAge = seeker.Requirements.MinAge > 0 ? seeker.Requirements.MinAge : 18;
            int maxAge = seeker.Requirements.MaxAge > 0 ? seeker.Requirements.MaxAge : 120;

            return candidate.Age >= minAge && candidate.Age <= maxAge;
        }
    }

    public class MatchService : IMatchService
    {
        private readonly IClientService _clients;
        private readonly List<IMatchRule> _rules;

        public MatchService(IClientService clients, List<IMatchRule> rules)
        {
            _clients = clients;
            _rules = rules;
        }

        public bool IsMatch(Client seeker, Client candidate)
        {
            if (seeker == null || candidate == null) return false;

            foreach (IMatchRule rule in _rules)
            {
                if (!rule.IsMatch(seeker, candidate))
                    return false;
            }

            return true;
        }

        public List<Client> Find(Client client, bool mutualOnly = false, string keyword = "")
        {
            List<Client> result = new List<Client>();
            if (client == null) return result;

            string query = keyword?.Trim() ?? string.Empty;

            foreach (Client candidate in _clients.All)
            {
                if (!IsMatch(client, candidate)) continue;
                if (mutualOnly && !IsMatch(candidate, client)) continue;
                if (!HasKeyword(candidate, query)) continue;

                result.Add(candidate);
            }

            return result;
        }

        private static bool HasKeyword(Client candidate, string query)
        {
            if (string.IsNullOrEmpty(query)) return true;

            if (candidate.FullName != null && candidate.FullName.Contains(query, StringComparison.OrdinalIgnoreCase))
                return true;

            if (candidate.PersonalDetails != null && candidate.PersonalDetails.Contains(query, StringComparison.OrdinalIgnoreCase))
                return true;

            if (candidate.Hobbies != null)
            {
                foreach (string hobby in candidate.Hobbies)
                {
                    if (hobby.Contains(query, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
            }

            return false;
        }
    }
}