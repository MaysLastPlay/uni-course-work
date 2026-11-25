using System;
using System.Windows.Controls;
using CourseApp.classes.information;
using CourseApp.classes.mvvm.viewmodels;
using CourseApp.classes.services;

namespace CourseApp.windows.views
{
    public partial class ClientMeetingsView : UserControl
    {
        public ClientMeetingsView(Client currentClient, IMeetingService meetings, Action? onChanged = null)
        {
            InitializeComponent();
            DataContext = new ClientMeetingsViewModel(currentClient, meetings, onChanged);
        }
    }
}