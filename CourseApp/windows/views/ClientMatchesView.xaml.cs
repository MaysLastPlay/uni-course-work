using System;
using System.Windows.Controls;
using CourseApp.classes.information;
using CourseApp.classes.mvvm.viewmodels;
using CourseApp.classes.services;

namespace CourseApp.windows.views
{
    public partial class ClientMatchesView : UserControl
    {
        public ClientMatchesView(Client currentClient, IMatchService matches, IMeetingService meetings, IDialogService dialogs, Action? onChanged = null)
        {
            InitializeComponent();
            DataContext = new ClientMatchesViewModel(currentClient, matches, meetings, dialogs, onChanged);
        }
    }
}