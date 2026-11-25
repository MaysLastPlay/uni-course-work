using System.Windows;
using CourseApp.classes.information;
using CourseApp.classes.mvvm.viewmodels;
using CourseApp.classes.services;

namespace CourseApp.windows.mainmenu
{
    public partial class MeetingsWindow : Window
    {
        public MeetingsWindow(IMeetingService meetings, IDialogService dialogs, Client? currentClient = null)
        {
            InitializeComponent();
            DataContext = new MeetingsWindowViewModel(meetings, dialogs, this, currentClient);
        }
    }
}