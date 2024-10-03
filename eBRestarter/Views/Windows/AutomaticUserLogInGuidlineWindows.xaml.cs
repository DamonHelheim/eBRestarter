using MahApps.Metro.Controls;
using eBRestarter.ViewModel.ViewModels;

namespace eBRestarter.Views.Windows
{

    public partial class AutomaticUserLogInGuidlineWindows : MetroWindow
    {
        public AutomaticUserLogInGuidlineViewModel AutomaticUserLogInGuidlineViewModel { get; } = new();

        public AutomaticUserLogInGuidlineWindows()
        {
            InitializeComponent();

            DataContext = AutomaticUserLogInGuidlineViewModel;
        }
    }
}
