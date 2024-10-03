using MahApps.Metro.Controls;
using eBRestarter.ViewModel.ViewModels;

namespace eBRestarter.Views.Windows
{
    public partial class InstalledeVisitorAddOnWindows : MetroWindow
    {
        private InstalledeVisitorAddOnViewModel InstalledeVisitorAddOnViewModel { get; } = new();

        public InstalledeVisitorAddOnWindows()
        {
            InitializeComponent();

            this.DataContext = InstalledeVisitorAddOnViewModel;
        }
    }
}
