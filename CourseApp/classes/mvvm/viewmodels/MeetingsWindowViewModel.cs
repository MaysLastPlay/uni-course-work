using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using CourseApp.classes.enums;
using CourseApp.classes.information;
using CourseApp.classes.services;

namespace CourseApp.classes.mvvm.viewmodels
{

    public class MeetingsWindowViewModel : ObservableObject
    {
        private readonly IMeetingService _meetings;
        private readonly IDialogService _dialogs;
        private readonly Client? _client;
        private readonly Window _window;

        private MeetingFilterMode _filter = MeetingFilterMode.All;
        private MeetingInvitation? _selected;
        private string _titleText = "📅 Управління зустрічами";
        private string _countText = "Зустрічей: 0";
        private Visibility _emptyVisibility = Visibility.Collapsed;
        private Visibility _listVisibility = Visibility.Visible;

        public ObservableCollection<MeetingInvitation> FilteredMeetings { get; } = new ObservableCollection<MeetingInvitation>();

        public string TitleText { get => _titleText; private set { _titleText = value; OnPropertyChanged(); } }
        public MeetingFilterMode SelectedFilter { get => _filter; set { _filter = value; OnPropertyChanged(); Load(); } }
        public MeetingInvitation? SelectedMeeting { get => _selected; set { _selected = value; OnPropertyChanged(); } }
        public string MeetingsCountText { get => _countText; private set { _countText = value; OnPropertyChanged(); } }
        public Visibility EmptyVisibility { get => _emptyVisibility; private set { _emptyVisibility = value; OnPropertyChanged(); } }
        public Visibility ListVisibility { get => _listVisibility; private set { _listVisibility = value; OnPropertyChanged(); } }

        public ICommand SetFilterCommand { get; }
        public ICommand AcceptCommand { get; }
        public ICommand RejectCommand { get; }
        public ICommand CloseCommand { get; }

        public MeetingsWindowViewModel(IMeetingService meetings, IDialogService dialogs, Window window, Client? currentClient = null)
        {
            _meetings = meetings;
            _dialogs = dialogs;
            _window = window;
            _client = currentClient;

            if (_client != null)
                TitleText = "📅 Мої зустрічі";

            SetFilterCommand = new RelayCommand(p => { if (p is MeetingFilterMode mode) SelectedFilter = mode; });
            AcceptCommand = new RelayCommand(_ => SetStatus(MeetingStatus.Accepted));
            RejectCommand = new RelayCommand(_ => SetStatus(MeetingStatus.Rejected));
            CloseCommand = new RelayCommand(_ => _window.Close());

            Load();
        }

        public void Load()
        {
            List<MeetingInvitation> source = _client == null
                ? _meetings.All
                : _meetings.GetFor(_client.RegNumber ?? string.Empty);

            FilteredMeetings.Clear();
            foreach (MeetingInvitation m in source)
            {
                bool include;
                switch (SelectedFilter)
                {
                    case MeetingFilterMode.Pending:
                        include = m.Status == MeetingStatus.Pending;
                        break;
                    case MeetingFilterMode.Accepted:
                        include = m.Status == MeetingStatus.Accepted;
                        break;
                    case MeetingFilterMode.Rejected:
                        include = m.Status == MeetingStatus.Rejected;
                        break;
                    default:
                        include = true;
                        break;
                }

                if (include) FilteredMeetings.Add(m);
            }

            MeetingsCountText = $"Зустрічей: {FilteredMeetings.Count}";
            EmptyVisibility = FilteredMeetings.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            ListVisibility = FilteredMeetings.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void SetStatus(MeetingStatus status)
        {
            if (SelectedMeeting == null)
            {
                _dialogs.Warn("Оберіть зустріч зі списку.", "Увага");
                return;
            }

            _meetings.SetStatus(SelectedMeeting.Id, status);
            Load();
        }
    }
}