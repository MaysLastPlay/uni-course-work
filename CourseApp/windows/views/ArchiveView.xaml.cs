using System;
using System.Windows.Controls;
using CourseApp.classes.mvvm.viewmodels;
using CourseApp.classes.services;

namespace CourseApp.windows.views
{
    public partial class ArchiveView : UserControl
    {
        private readonly ArchiveViewModel _vm;

        public event Action<int>? TabChanged
        {
            add => _vm.TabChanged += value;
            remove => _vm.TabChanged -= value;
        }

        public ArchiveView(IClientService clients, IMeetingService meetings, IDialogService dialogs, Action? onChanged = null)
        {
            InitializeComponent();
            _vm = new ArchiveViewModel(clients, meetings, dialogs, onChanged);
            DataContext = _vm;
        }

        public void SelectTab(int index) => _vm.SelectTab(index);
        public void RefreshUI() => _vm.Reload();
    }
}