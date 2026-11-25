using System.Windows.Controls;
using CourseApp.classes.mvvm.viewmodels;
using CourseApp.classes.services;

namespace CourseApp.windows.views
{
    public partial class ClientProfilesView : UserControl
    {
        private readonly ClientProfilesViewModel _vm;

        public ClientProfilesView(IClientService clients)
        {
            InitializeComponent();
            _vm = new ClientProfilesViewModel(clients);
            DataContext = _vm;
        }

        public void RefreshUI() => _vm.Filter();
    }
}