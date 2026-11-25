using System.Windows;
using CourseApp.classes.mvvm.viewmodels;
using CourseApp.classes.services;

namespace CourseApp.windows.authentication
{
    public partial class LoginWindow : Window
    {
        private readonly LoginViewModel _vm;

        public LoginWindow() : this(new AppServices())
        {
        }

        public LoginWindow(AppServices app)
        {
            InitializeComponent();
            _vm = new LoginViewModel(app, this);
            DataContext = _vm;
            Loaded += (_, _) => _vm.OnLoaded();
        }
    }
}