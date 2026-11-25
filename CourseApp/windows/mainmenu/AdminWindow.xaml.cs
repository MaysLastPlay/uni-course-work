using System.Windows;
using CourseApp.classes.mvvm.viewmodels;
using CourseApp.classes.services;

namespace CourseApp.windows.mainmenu
{
    public partial class AdminWindow : Window
    {
        public AdminWindow(AppServices app)
        {
            InitializeComponent();
            DataContext = new AdminViewModel(app, this);
        }
    }
}