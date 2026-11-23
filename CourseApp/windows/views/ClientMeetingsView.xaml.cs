using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CourseApp.classes.enums;
using CourseApp.classes.information;
using CourseApp.classes.services.CourseApp.classes;

namespace CourseApp.windows.views
{
    public partial class ClientMeetingsView : UserControl
    {
        private readonly Client _currentClient;
        private readonly DatingService _datingService;
        private readonly Action? _onDataChanged;

        public ClientMeetingsView(Client currentClient, DatingService datingService, Action? onDataChanged = null)
        {
            InitializeComponent();
            _currentClient = currentClient;
            _datingService = datingService;
            _onDataChanged = onDataChanged;

            RefreshUI();
        }

        public void RefreshUI()
        {
            List<MeetingInvitation> all = _datingService.GetClientMeetings(_currentClient.RegNumber ?? "");
            List<MeetingDisplayItem> pending = new List<MeetingDisplayItem>();
            List<MeetingDisplayItem> scheduled = new List<MeetingDisplayItem>();
            List<MeetingDisplayItem> history = new List<MeetingDisplayItem>();

            foreach (MeetingInvitation meeting in all)
            {
                bool isIncoming = meeting.Target != null &&
                                  string.Equals(meeting.Target.RegNumber, _currentClient.RegNumber, StringComparison.OrdinalIgnoreCase);

                MeetingDisplayItem item = BuildItem(meeting, isIncoming);

                if (meeting.Status == MeetingStatus.Pending)
                {
                    pending.Add(item);
                }
                else if (meeting.Status == MeetingStatus.Accepted && meeting.MeetingDate >= DateTime.Now)
                {
                    scheduled.Add(item);
                }
                else
                {
                    history.Add(item);
                }
            }

            PendingListBox.ItemsSource = pending;
            EmptyPendingPanel.Visibility = pending.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

            ScheduledListBox.ItemsSource = scheduled;
            EmptyScheduledPanel.Visibility = scheduled.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

            HistoryListBox.ItemsSource = history;
            EmptyHistoryPanel.Visibility = history.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private MeetingDisplayItem BuildItem(MeetingInvitation meeting, bool isIncoming)
        {
            Client? partner = isIncoming ? meeting.Initiator : meeting.Target;
            string partnerName = partner != null ? partner.FullName : "Невідомий клієнт";
            string partnerReg = partner != null ? $"ID: {partner.RegNumber}" : "";
            string direction = isIncoming ? "Вхідне" : "Вихідне";

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

            bool canAcceptDecline = meeting.Status == MeetingStatus.Pending && isIncoming;
            bool canCancel = (meeting.Status == MeetingStatus.Pending && !isIncoming) ||
                             (meeting.Status == MeetingStatus.Accepted && meeting.MeetingDate >= DateTime.Now);

            return new MeetingDisplayItem
            {
                MeetingId = meeting.Id,
                PartnerName = partnerName,
                DirectionText = direction,
                DateText = $"🕒 {meeting.MeetingDate:dd.MM.yyyy HH:mm}",
                LocationText = partnerReg,
                StatusText = statusText,
                StatusBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(bgHex)),
                StatusForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(fgHex)),
                AcceptVisibility = canAcceptDecline ? Visibility.Visible : Visibility.Collapsed,
                DeclineVisibility = canAcceptDecline ? Visibility.Visible : Visibility.Collapsed,
                CancelVisibility = canCancel ? Visibility.Visible : Visibility.Collapsed
            };
        }

        private void AcceptMeeting_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button { DataContext: MeetingDisplayItem item }) return;
            _datingService.UpdateMeetingStatus(item.MeetingId, MeetingStatus.Accepted);
            RefreshUI();
            _onDataChanged?.Invoke();
        }

        private void DeclineMeeting_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button { DataContext: MeetingDisplayItem item }) return;
            _datingService.UpdateMeetingStatus(item.MeetingId, MeetingStatus.Declined);
            RefreshUI();
            _onDataChanged?.Invoke();
        }

        private void CancelMeeting_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button { DataContext: MeetingDisplayItem item }) return;
            _datingService.CancelMeeting(item.MeetingId);
            RefreshUI();
            _onDataChanged?.Invoke();
        }

        private class MeetingDisplayItem
        {
            public string MeetingId { get; set; } = "";
            public string PartnerName { get; set; } = "";
            public string DirectionText { get; set; } = "";
            public string DateText { get; set; } = "";
            public string LocationText { get; set; } = "";
            public string StatusText { get; set; } = "";
            public Brush StatusBackground { get; set; } = Brushes.Transparent;
            public Brush StatusForeground { get; set; } = Brushes.Black;
            public Visibility AcceptVisibility { get; set; }
            public Visibility DeclineVisibility { get; set; }
            public Visibility CancelVisibility { get; set; }
        }
    }
}