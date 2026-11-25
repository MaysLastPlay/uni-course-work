using System.Windows.Controls;
using CourseApp.classes.information;
using CourseApp.classes.mvvm.viewmodels;
using CourseApp.classes.services;

namespace CourseApp.windows.views
{
    public partial class ClientSearchView : UserControl
    {
        public ClientSearchView(Client currentClient, IClientService clients)
        {
            InitializeComponent();
            DataContext = new ClientSearchViewModel(currentClient, clients);
        }
    }
}