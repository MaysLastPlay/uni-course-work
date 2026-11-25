using System.Windows;
using CourseApp.classes.mvvm.viewmodels;
using CourseApp.classes.services;

namespace CourseApp.windows.authentication
{
    public partial class RegisterWindow : Window
    {
        public RegisterWindow(AppServices app)
        {
            InitializeComponent();
            DataContext = new RegisterViewModel(app.Clients, app.Dialogs, this);
        }
    }
}