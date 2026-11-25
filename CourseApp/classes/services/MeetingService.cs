using System;
using System.Collections.Generic;
using CourseApp.classes.enums;
using CourseApp.classes.information;

namespace CourseApp.classes.services
{
    public class MeetingService : IMeetingService
    {
        private const string MeetingsFile = "meetings.json";
        private readonly IStorage _storage;

        public List<MeetingInvitation> All { get; } = new List<MeetingInvitation>();

        public MeetingService(IStorage storage)
        {
            _storage = storage;
            foreach (MeetingInvitation m in _storage.Load<MeetingInvitation>(MeetingsFile))
                All.Add(m);
        }

        public void Create(Client initiator, Client target, DateTime? date = null)
        {
            if (initiator == null || target == null) return;

            All.Add(new MeetingInvitation
            {
                Initiator = initiator,
                Target = target,
                MeetingDate = date ?? DateTime.Now.AddDays(3)
            });

            _storage.Save(MeetingsFile, All);
        }

        public void SetStatus(string id, MeetingStatus status)
        {
            foreach (MeetingInvitation m in All)
            {
                if (m.Id == id)
                {
                    m.Status = status;
                    _storage.Save(MeetingsFile, All);
                    return;
                }
            }
        }

        public bool Cancel(string id)
        {
            for (int i = 0; i < All.Count; i++)
            {
                if (All[i].Id == id)
                {
                    All.RemoveAt(i);
                    _storage.Save(MeetingsFile, All);
                    return true;
                }
            }

            return false;
        }

        public List<MeetingInvitation> GetFor(string regNumber)
        {
            List<MeetingInvitation> result = new List<MeetingInvitation>();
            if (string.IsNullOrWhiteSpace(regNumber)) return result;

            foreach (MeetingInvitation m in All)
            {
                if (SameReg(m.Initiator?.RegNumber, regNumber) ||
                    SameReg(m.Target?.RegNumber, regNumber))
                {
                    result.Add(m);
                }
            }

            result.Sort((a, b) => b.MeetingDate.CompareTo(a.MeetingDate));
            return result;
        }

        public int CountPending(string regNumber)
        {
            if (string.IsNullOrWhiteSpace(regNumber)) return 0;

            int count = 0;
            foreach (MeetingInvitation m in All)
            {
                if (m.Status == MeetingStatus.Pending && SameReg(m.Target?.RegNumber, regNumber))
                    count++;
            }

            return count;
        }

        public void RemoveFor(string regNumber)
        {
            bool changed = false;
            for (int i = All.Count - 1; i >= 0; i--)
            {
                MeetingInvitation m = All[i];
                if (SameReg(m.Initiator?.RegNumber, regNumber) ||
                    SameReg(m.Target?.RegNumber, regNumber))
                {
                    All.RemoveAt(i);
                    changed = true;
                }
            }

            if (changed) _storage.Save(MeetingsFile, All);
        }

        private static bool SameReg(string? a, string? b)
        {
            return !string.IsNullOrWhiteSpace(a) &&
                   !string.IsNullOrWhiteSpace(b) &&
                   string.Equals(a.Trim(), b.Trim(), StringComparison.OrdinalIgnoreCase);
        }
    }
}