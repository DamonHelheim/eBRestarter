using eBRestarter.Desktop.WinUI3.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace eBRestarter.Desktop.WinUI3.VisualUIComponents.Views.UserControls;

public sealed partial class UC_AutomatedTasks : UserControl
{
    /// <summary>
    /// Typed view model reference required by <c>{x:Bind}</c> (Guide Kap. 23.1).
    /// </summary>
    /// <remarks>
    /// <c>{x:Bind}</c> resolves against the code-behind, not against the inherited
    /// <c>DataContext</c>. <see cref="ViewModelRestarterProperties"/> is registered as a singleton, so resolving it here
    /// yields exactly the same instance the hosting view supplies via <c>DataContext</c> —
    /// the binding source is unchanged, only the binding mechanism.
    /// </remarks>
    public ViewModelRestarterProperties ViewModelRestarterProperties { get; }

    public UC_AutomatedTasks()
    {
        ViewModelRestarterProperties = App.AppHost!.Services.GetRequiredService<ViewModelRestarterProperties>();

        InitializeComponent();
    }
}
