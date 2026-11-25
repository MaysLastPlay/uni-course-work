using System.Windows;
using CourseApp.classes.information;
using CourseApp.classes.mvvm.viewmodels;
using CourseApp.classes.services;

namespace CourseApp.windows.mainmenu
{
    public partial class ClientWindow : Window
    {
        public ClientWindow(Client currentClient, AppServices app)
        {
            InitializeComponent();
            DataContext = new ClientWindowViewModel(currentClient, app, this);
        }
    }
}