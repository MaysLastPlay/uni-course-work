using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using CourseApp.classes.enums;
using CourseApp.classes.information;
using CourseApp.classes.services;

namespace CourseApp.classes.mvvm.viewmodels
{
    public class ClientMeetingsViewModel : ObservableObject
    {
        private readonly Client _client;
        private readonly IMeetingService _meetings;
        private readonly Action? _onChanged;

        private Visibility _emptyPendingVisibility = Visibility.Collapsed;
        private Visibility _emptyScheduledVisibility = Visibility.Collapsed;
        private Visibility _emptyHistoryVisibility = Visibility.Collapsed;

        public ObservableCollection<MeetingDisplayItem> PendingMeetings { get; } = new ObservableCollection<MeetingDisplayItem>();
        public ObservableCollection<MeetingDisplayItem> ScheduledMeetings { get; } = new ObservableCollection<MeetingDisplayItem>();
        public ObservableCollection<MeetingDisplayItem> HistoryMeetings { get; } = new ObservableCollection<MeetingDisplayItem>();

        public Visibility EmptyPendingVisibility { get => _emptyPendingVisibility; private set { _emptyPendingVisibility = value; OnPropertyChanged(); } }
        public Visibility EmptyScheduledVisibility { get => _emptyScheduledVisibility; private set { _emptyScheduledVisibility = value; OnPropertyChanged(); } }
        public Visibility EmptyHistoryVisibility { get => _emptyHistoryVisibility; private set { _emptyHistoryVisibility = value; OnPropertyChanged(); } }

        public ICommand AcceptCommand { get; }
        public ICommand DeclineCommand { get; }
        public ICommand CancelCommand { get; }

        public ClientMeetingsViewModel(Client currentClient, IMeetingService meetings, Action? onChanged = null)
        {
            _client = currentClient;
            _meetings = meetings;
            _onChanged = onChanged;

            AcceptCommand = new RelayCommand(p => { if (p is MeetingDisplayItem item) SetStatus(item.MeetingId, MeetingStatus.Accepted); });
            DeclineCommand = new RelayCommand(p => { if (p is MeetingDisplayItem item) SetStatus(item.MeetingId, MeetingStatus.Declined); });
            CancelCommand = new RelayCommand(p => { if (p is MeetingDisplayItem item) Cancel(item.MeetingId); });

            Reload();
        }

        public void Reload()
        {
            List<MeetingInvitation> list = _meetings.GetFor(_client.RegNumber ?? string.Empty);

            PendingMeetings.Clear();
            ScheduledMeetings.Clear();
            HistoryMeetings.Clear();

            foreach (MeetingInvitation meeting in list)
            {
                bool incoming = meeting.Target != null &&
                                string.Equals(meeting.Target.RegNumber, _client.RegNumber, StringComparison.OrdinalIgnoreCase);

                MeetingDisplayItem item = CreateItem(meeting, incoming);

                if (meeting.Status == MeetingStatus.Pending)
                    PendingMeetings.Add(item);
                else if (meeting.Status == MeetingStatus.Accepted && meeting.MeetingDate >= DateTime.Now)
                    ScheduledMeetings.Add(item);
                else
                    HistoryMeetings.Add(item);
            }

            EmptyPendingVisibility = PendingMeetings.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            EmptyScheduledVisibility = ScheduledMeetings.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            EmptyHistoryVisibility = HistoryMeetings.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void SetStatus(string id, MeetingStatus status)
        {
            _meetings.SetStatus(id, status);
            Reload();
            _onChanged?.Invoke();
        }

        private void Cancel(string id)
        {
            _meetings.Cancel(id);
            Reload();
            _onChanged?.Invoke();
        }

        private static MeetingDisplayItem CreateItem(MeetingInvitation meeting, bool incoming)
        {
            Client? partner = incoming ? meeting.Initiator : meeting.Target;
            string partnerName = partner?.FullName ?? "Невідомий клієнт";
            string partnerReg = partner != null ? $"ID: {partner.RegNumber}" : string.Empty;

            string statusText;
            string bgHex;
            string fgHex;

            if (meeting.Status == MeetingStatus.Pending)
            {
                statusText = "Очікує";
                bgHex = "#FEF9E7";
                fgHex = "#B7950B";
            }
            else if (meeting.Status == MeetingStatus.Accepted && meeting.MeetingDate >= DateTime.Now)
            {
                statusText = "Підтверджено";
                bgHex = "#E8F8F0";
                fgHex = "#27AE60";
            }
            else if (meeting.Status == MeetingStatus.Accepted)
            {
                statusText = "Завершено";
                bgHex = "#EBF5FB";
                fgHex = "#2980B9";
            }
            else
            {
                statusText = "Відхилено";
                bgHex = "#FDEDEC";
                fgHex = "#C0392B";
            }

            bool canRespond = meeting.Status == MeetingStatus.Pending && incoming;
            bool canCancel = (meeting.Status == MeetingStatus.Pending && !incoming) ||
                             (meeting.Status == MeetingStatus.Accepted && meeting.MeetingDate >= DateTime.Now);

            return new MeetingDisplayItem
            {
                MeetingId = meeting.Id,
                PartnerName = partnerName,
                DirectionText = incoming ? "Вхідне" : "Вихідне",
                DateText = $"🕒 {meeting.MeetingDate:dd.MM.yyyy HH:mm}",
                LocationText = partnerReg,
                StatusText = statusText,
                StatusBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(bgHex)),
                StatusForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(fgHex)),
                AcceptVisibility = canRespond ? Visibility.Visible : Visibility.Collapsed,
                DeclineVisibility = canRespond ? Visibility.Visible : Visibility.Collapsed,
                CancelVisibility = canCancel ? Visibility.Visible : Visibility.Collapsed
            };
        }
    }
}